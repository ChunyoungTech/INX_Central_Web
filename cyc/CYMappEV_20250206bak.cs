using CYCloud.MappEV.Data;
using NPOI.SS.Formula.Functions;
using NPOI.SS.Formula.PTG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using System.Web.UI.WebControls;
using static Dapper.SqlMapper;

namespace CYCloud.MappEV
{
    public class MappEVCreate : cyc.Auto.AutoJob //地震壓降MAPP定時掃描來源
    {
        public static readonly string JobKey = "DoMappEV";
        public static readonly string JobName = "地震壓降MAPP";

        cyc.DB.SqlDapperConn _DB = null;
        cyc.DB.SqlDapperConn bDB { get { if (_DB == null) { _DB = new cyc.DB.SqlDapperConn(oResult); }; return _DB; } }
        static DateTime TimeNow { get; set; } = DateTime.MinValue;

        List<Data.LogData> LogList = new List<Data.LogData>();

        protected override void Run()
        {
            if (cyc.Auto.Manager.GetExclusive(JobKey))
            {
                try
                {
                    DoExec();
                }
                catch (Exception ex) 
                { 
                    cyc.Log.WriteSysErrorLog($"{JobName}:{ex.Message}"); 
                    oResult.Error(ex.Message);
                    LogList.Add(new LogData { Log = $"Error: {ex.Message}" });
                }
                finally 
                {
                    _DB?.Dispose();
                    cyc.Auto.Manager.CloseExclusive(JobKey, oResult);

                    if (LogList.Count > 0)
                        cyc.Log.WriteFileLog(string.Join(System.Environment.NewLine, LogList.Select(p => $"{p.Time:HH:mm:ss} {p.Log}")), JobName);
                }
            }
        }

        string MappX1 { get; set; } = cyc.Shared.SysQuery.GetSysSettingValue("MappX1"); //地震 加發條件(級數)
        string MappX2 { get; set; } = cyc.Shared.SysQuery.GetSysSettingValue("MappX2"); //壓降 加發條件(落點區域)
        string MappMsgX1 { get; set; } = cyc.Shared.SysQuery.GetSysSettingValue("MappMsgX1"); //地震 加發訊息
        string MappMsgX2 { get; set; } = cyc.Shared.SysQuery.GetSysSettingValue("MappMsgX2"); //壓降 加發訊息
        int iMappX1;
        bool SendX1; //地震是否加發
        bool SendX2; //壓降是否加發

        private void DoExec()
        {
            try
            {
                //系統重啟後第一次執行，先跳過(避免IIS回收機制，重複執行)
                if (TimeNow == DateTime.MinValue) { TimeNow = DateTime.Now; return; }

                //目前執行時間
                TimeNow = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                //LogList.Add(new LogData { Log = $"{JobName} 啟動" });

                //高階彙整 重設秒數 預設 300
                if (int.TryParse(cyc.Shared.SysQuery.GetSysSettingValue("MappEVReset"), out int iStop)) { Shared.HighStopSeconds = iStop; }
                //高階彙整 發送秒數 預設 30
                if (int.TryParse(cyc.Shared.SysQuery.GetSysSettingValue("MappEVSecond"), out int iSend)) { Shared.HighSendSeconds = iSend; }

                //20220928 新增 地震>5 壓降=C、D 單廠訊息 加發
                //SetMappExtend();
                if (!int.TryParse(MappX1, out iMappX1)) { iMappX1 = 0; }
                //地震是否加發
                SendX1 = iMappX1 > 0 && !string.IsNullOrWhiteSpace(MappMsgX1);
                //壓降是否加發
                SendX2 = !string.IsNullOrWhiteSpace(MappX2) && !string.IsNullOrWhiteSpace(MappMsgX2);

                //暫存新增的MAPP
                List<Data.MappEVMessage> msgList = new List<Data.MappEVMessage>();
                //查詢尚未處理的地震壓降事件MappEVInput
                var qList = bDB.QueryList<Data.MappEVInput>("select * from MappEVInput where InputTime>@Time and InputStatus=0 order by InputTime", new { Time = TimeNow.AddMinutes(-5) });
                if (qList != null && qList.Any())
                {
                    LogList.Add(new LogData { Log = $"地震壓降未處理資料{qList.Count()}筆" });

                    //處理單廠資料
                    foreach (var group in qList.GroupBy(p => new { p.Type, p.FacName }))
                        msgList.AddRange(DoMappEVHandleForFAC(group, group.Key.Type, group.Key.FacName));
                }

                //處理高階彙整資料
                msgList.AddRange(DoMappEVHandleForHigh());

                //有新增 MappMessage 或 MappInput
                if (msgList.Count > 0 || (qList != null && qList.Any()))
                {
                    using (cyc.DB.SqlDapperConn oDB = new cyc.DB.SqlDapperConn(oResult, null, true))
                    {
                        if (qList != null && qList.Any()) //已處理清單更新註記
                            oDB.Execute("update MappEVInput set InputStatus=1,ReadTime=getdate() where ID in @ID", new { ID = qList.Select(p => p.ID) });

                        if (oResult.Success && msgList.Count > 0) //新增至MappMessage
                            oDB.Execute("insert into MappMessage (MS_SYS_NAME,MM_CONTENT_TYPE,MM_TEXT_CONTENT,MM_SUBJECT,MM_TYPE,MM_Priority) values (@MS_SYS_NAME,@MM_CONTENT_TYPE,@MM_TEXT_CONTENT,@MM_SUBJECT,@MM_TYPE,@MM_Priority)", msgList);
                        
                        oDB.ResultTransaction();
                    }
                }
            }
            catch (Exception ex) { cyc.Log.WriteSysErrorLog("Mapp地震壓降:" + ex.Message); oResult.Error(ex.Message); }

            //執行CimWebApi
            if (Shared.DoCimWebApi) 
                Task.Run(() => CimWebApi.Execute());
        }

        //處理單廠資料
        private List<Data.MappEVMessage> DoMappEVHandleForFAC(IEnumerable<Data.MappEVInput> oList, string Type, string Fac)
        {
            StringBuilder oStr = new StringBuilder($"處理[{Fac}{(Type == "E" ? "地震" : "壓降")}]");
            //新增的MAPP
            List<Data.MappEVMessage> msgList = new List<Data.MappEVMessage>();
            try
            {
                //查詢是否有符合的 MappEV設定
                var oSetting = Global.MappEV.List.FirstOrDefault(p => p.Code == Fac && p.Type == Type);
                if (oSetting != null)
                {
                    //轉換Template
                    Shared.ConvertMappTemplate(oSetting);

                    //查詢正式群組是否有隔離，20230320 地震壓降不設定隔離群組，一律改在隔離設定做，隔離僅處理高階彙整
                    bool IsDisable = bDB.QueryOne<int>("select count(1) from MappDisable where MS_SEQ_ID=@ID and MD_STOP_TIME is null and @Time between MD_DATE_START and MD_DATE_END", new { ID = oSetting.NormalID, Time = TimeNow }) > 0;

                    //新增至MAPP清單，20230320 地震壓降不設定隔離群組，一律改在隔離設定做，隔離處置僅高階彙整
                    msgList.AddRange(oList.Select(p => new Data.MappEVMessage
                    {
                        //MS_SYS_NAME = IsDisable ? oSetting.DisableCode : oSetting.NormalCode,
                        MS_SYS_NAME = oSetting.NormalCode,
                        MM_SUBJECT = Shared.ConvertMappContent(oSetting.MappSubjectX, p),
                        MM_Priority = 1, //次優先
                        MM_TEXT_CONTENT = Shared.ConvertMappContent(oSetting.MappContentX, p) + CheckMappX(p)
                    }));
                    oStr.Append("，產出MAPP");

                    //20220928 新增 地震>5 壓降=C、D 單廠訊息 加發
                    string CheckMappX(Data.MappEVInput oData)
                    {
                        if (SendX1 && oData.Type == "E" && int.TryParse(oData.Value2, out int iValue) && iValue >= iMappX1)
                            return $"{Shared.NewLine}{MappMsgX1}";
                        if (SendX2 && oData.Type == "P" && !string.IsNullOrEmpty(oData.Value4) && MappX2.Contains(oData.Value4))
                            return $"{Shared.NewLine}{MappMsgX2}";
                        return string.Empty;
                    }

                    //系統CIM啟用，各廠CIM啟用
                    if (Shared.DoCimWebApi && oSetting.CimEnable && !string.IsNullOrEmpty(oSetting.CimWebApi) && !string.IsNullOrEmpty(oSetting.CimParaData))
                    {
                        foreach (var f in oList)
                        {
                            CimWebApi.Enqueue(new Data.CimTask
                            {
                                MappEVID = oSetting.ID,
                                WebApi = oSetting.CimWebApi,
                                Method = oSetting.CimMethod,
                                ParaData = Shared.ConvertMappContent(oSetting.CimParaDataX, f),
                                IsHigh = false
                            });
                            oStr.Append($"，加入CIM單廠-ID:{oSetting.ID}");
                        }
                    }

                    //高階彙整 排除 地震震度小於2 or 壓降大於85%
                    var hList = oList.ToList();
                    if (Type == "E")
                        hList.RemoveAll(p => !decimal.TryParse(p.Value2, out decimal value) || value < 2);
                    else
                        hList.RemoveAll(p => !decimal.TryParse(p.Value1, out decimal value) || value >= 85);

                    if (hList.Any())
                    {
                        var oLast = hList.Last(); //只處理最新的一筆
                        oLast.IsDisable = IsDisable; //單廠是否隔離
                        oLast.CimLevel = oSetting.CimLevel; //單廠CIM比重
                        oLast.CimGroup = oSetting.CimGroup; //CIM高階分組

                        //高階彙整 (地震、壓降 + 南廠、北廠)
                        MappHighSummary.AddHighDetail(Type, oSetting.FacArea, oLast, TimeNow);
                        oStr.Append($"，加入[{(oSetting.FacArea == "1" ? "南廠" : "北廠")}]彙整");
                    }
                }
            }
            catch (Exception ex) 
            { 
                cyc.Log.WriteSysErrorLog($"MappEV處理[{Fac}][{Type}]：" + ex.StackTrace);
                oStr.Append($"，Error: {ex.Message}");
            }
            finally
            {
                LogList.Add(new LogData { Log = oStr.ToString() });
            }
            return msgList;
        }

        //執行高階彙整資料處理 => 回傳產出的MAPP
        private List<Data.MappEVMessage> DoMappEVHandleForHigh()
        {
            //新增的MAPP
            List<Data.MappEVMessage> msgList = new List<Data.MappEVMessage>();
            for (int idx = MappHighSummary.HighList.Count - 1; idx >= 0; idx--)
            {
                StringBuilder oStr = new StringBuilder(string.Empty);
                MappEVHigh hData = MappHighSummary.HighList[idx];
                try
                {
                    if (hData.SendTime <= TimeNow) //已達發送時間
                    {
                        oStr.Append("，已達發送時間");

                        if (hData.SendTime < TimeNow.AddHours(-1)) //排除發送時間已超過1小時
                            oStr.Append("，發送時間已超過1小時");
                        else
                        {
                            //高階設定
                            var hSetting = Global.MappEV.List.FirstOrDefault(p => p.Type == hData.Type && p.FacArea == hData.FacArea && p.IsTop);
                            if (hSetting != null)
                            {
                                //第1筆 非Null的資料且無單廠隔離
                                var first = hData.List.FirstOrDefault(p => !p.IsDisable && p.Value1 != "NA");
                                if (first != null)
                                {
                                    //轉換Template
                                    Shared.ConvertMappTemplate(hSetting);

                                    var oMessage = new Data.MappEVMessage
                                    {
                                        MS_SYS_NAME = hSetting.NormalCode,
                                        MM_SUBJECT = Shared.ConvertMappContent(hSetting.MappSubjectX, first), //MAPP主旨
                                        MM_Priority = 0 //最優先
                                    };

                                    //MAPP內容 - 標頭+內容+結尾
                                    var sCont = (hSetting.MappContentX ?? string.Empty).Split(new string[] { "~@~" }, StringSplitOptions.None);
                                    if (sCont.Length == 1)
                                        oMessage.MM_TEXT_CONTENT = string.Join(Shared.NewLine, hData.List.Select(p => Shared.ConvertMappContent(sCont[0], p, p.IsDisable)));
                                    else if (sCont.Length == 3)
                                    {
                                        oMessage.MM_TEXT_CONTENT = string.Format("{0}{1}{2}",
                                            sCont[0].Length > 0 ? $"{Shared.ConvertMappContent(sCont[0], first)}{Shared.NewLine}" : string.Empty,
                                            string.Join(Shared.NewLine, hData.List.Select(p => Shared.ConvertMappContent(sCont[1], p, p.IsDisable))),
                                            sCont[2].Length > 0 ? $"{Shared.NewLine}{Shared.ConvertMappContent(sCont[2], first)}" : string.Empty);
                                    }
                                    msgList.Add(oMessage);

                                    oStr.Append("，產出MAPP");

                                    //系統CIM啟用，高階CIM啟用，各廠比重加總>0
                                    if (Shared.DoCimWebApi && hSetting.CimEnable && !string.IsNullOrEmpty(hSetting.CimWebApi) && !string.IsNullOrEmpty(hSetting.CimParaData))
                                    {
                                        //20240716 CIM 高階 分組
                                        try
                                        {
                                            var cList = hData.List.Where(p => !p.IsDisable && p.Value1 != "NA").GroupBy(p => p.CimGroup).Where(p => p.Sum(q => q.CimLevel) > 0);
                                            foreach (var cData in cList)
                                            {
                                                CimWebApi.Enqueue(new Data.CimTask
                                                {
                                                    MappEVID = hSetting.ID,
                                                    WebApi = hSetting.CimWebApi,
                                                    Method = hSetting.CimMethod,
                                                    ParaData = Shared.ConvertMappContent(hSetting.CimParaDataX, cData.First()),
                                                    IsHigh = true
                                                });
                                                oStr.Append($"，加入CIM高階-ID:{hSetting.ID}(分組{cData.Key})");
                                            }
                                        }
                                        catch (Exception ex) 
                                        { 
                                            cyc.Log.WriteSysErrorLog($"MappEV處理高階彙整CIM：{ex.Message}");
                                            oStr.Append($"，設定ID:{hSetting.ID}-CIM錯誤: {ex.Message}");
                                        }
                                    }
                                }
                            }
                        }

                        hData.SendTime = DateTime.MaxValue;
                        hData.IsChanged = true; //異動
                    }

                    //已達重設時間，清除資料
                    if (hData.ResetTime <= TimeNow)
                    {
                        oStr.Append("，已達重設時間");

                        hData.List?.Clear();
                        hData.SendTime = DateTime.MaxValue;
                        hData.ResetTime = DateTime.MaxValue;
                        hData.IsChanged = true; //異動
                    }

                    //更新最新資料->補發查詢(高階彙整)
                    if (hData.IsChanged)
                    {
                        oStr.Append("，記錄異動LOG");

                        MappHighSummary.DoMappEVHighLog(hData); //更新彙整資料、記錄LOG、更新最後記錄
                        hData.IsChanged = false;
                    }
                }
                catch (Exception ex)
                {
                    cyc.Log.WriteSysErrorLog($"MappEV處理高階彙整：{ex.Message}");
                    oStr.Append($"，Error:{ex.Message}");
                }
                finally
                {
                    if (oStr.Length > 0)
                        LogList.Add(new LogData { Log = $"處理[{(hData.FacArea == "1" ? "南廠" : "北廠")}{(hData.Type == "E" ? "地震" : "壓降")}]彙整{oStr}" });
                }
            }
            return msgList;
        }
    }

    //地震壓降MAPP設定 共用
    public static class Shared
    {
        //日期、時間 格式
        public const string DateFormat = "yyyy-MM-dd", TimeFormat = "HH:mm:ss", NewLine = "\n", NullString = "NA", NullDate = "0000-00-00", NullTime = "00:00:00";
        //高階彙整 重設秒數 預設 300
        internal static int HighStopSeconds { get; set; } = 300;
        //高階彙整 發送秒數 預設 30
        internal static int HighSendSeconds { get; set; } = 30;
        //是否執行CIM
        public static bool DoCimWebApi { get; } = cyc.Shared.SysQuery.GetAppSettingValue("DoCimWebApi") == "1";
        //[地震]可用標籤
        public static string[] TemplateTagE { get; } = new string[] { "發報日期", "發報時間", "廠別代號", "資料來源", "日期", "時間", "震度", "級數", "持續時間" };
        //[壓降]可用標籤
        public static string[] TemplateTagP { get; } = new string[] { "發報日期", "發報時間", "廠別代號", "資料來源", "日期", "時間", "剩餘電壓", "壓降前用電量", "壓降後用電量", "壓降落點區域", "持續時間" };

        //轉換MAPP範本
        public static void ConvertMappTemplate(Data.MappEVSetting oSetting)
        {
            if (string.IsNullOrEmpty(oSetting.MappSubjectX)) oSetting.MappSubjectX = ConvertMappTemplate(oSetting.MappSubject, oSetting.Type);
            if (string.IsNullOrEmpty(oSetting.MappContentX)) oSetting.MappContentX = ConvertMappTemplate(oSetting.MappContent, oSetting.Type);
            if (string.IsNullOrEmpty(oSetting.CimParaDataX)) oSetting.CimParaDataX = ConvertMappTemplate(oSetting.CimParaData, oSetting.Type);
        }
        static string ConvertMappTemplate(string sTemplate, string sType)
        {
            if (string.IsNullOrEmpty(sTemplate)) return "";
            ReplaceTag(sType == "E" ? TemplateTagE : TemplateTagP);
            //地震Value 1.震度 2.級數 3.持續時間
            //壓降Value 1.壓降剩餘電壓% 2.壓降前用電量 3.壓降後用電量 4.壓降落點區域 5.持續時間
            void ReplaceTag(string[] sTags)
            {
                for (int idx = 0; idx < sTags.Length; idx++)
                    sTemplate = sTemplate.Replace(string.Format("{{{0}}}", sTags[idx]), string.Format("{{{0}}}", idx));
            }
            return sTemplate;
        }
        //轉換MAPP內容
        public static string ConvertMappContent(string sTemp, Data.MappEVInput oData, bool IsDisable = false)
        {
            if (string.IsNullOrWhiteSpace(sTemp)) { return ""; }
            DateTime nDate = DateTime.Now;
            return string.Format(sTemp, nDate.ToString("yyyy-MM-dd"), nDate.ToString("HH:mm:ss"), oData.FacName, oData.InputSource, GetValueDate(oData.InputTime), GetValueTime(oData.InputTime), GetValue(oData.Value1), GetValue(oData.Value2), GetValue(oData.Value3), GetValue(oData.Value4), GetValue(oData.Value5));

            string GetValue(string Value) { return IsDisable ? NullString : Value ?? NullString; }
            string GetValueDate(DateTime dDate) { return IsDisable ? NullDate : dDate.ToString(DateFormat); }
            string GetValueTime(DateTime dDate) { return IsDisable ? NullTime : dDate.ToString(TimeFormat); }
        }
    }

    //地震、壓降 高階彙整資料
    public static class MappHighSummary
    {
        //static bool IsRegister = false;
        //目前高階彙整資料清單
        public static List<MappEVHigh> HighList { get; private set; } = GetHighList();

        //查詢取得高階彙整 最新(預設)資料
        private static List<MappEVHigh> GetHighList()
        {
            ////註冊 Application_End 委派事件
            //if (!IsRegister) { cyc.Global.ApplicationEnd += Close; IsRegister = true; } 

            using (cyc.DB.SqlDapperConn oDB = new cyc.DB.SqlDapperConn())
            {
                var list = oDB.QueryList<Data.MappEVHigh>("select FacArea,Type,List as ListJson,SendTime,ResetTime from MappEVHigh");
                if (list != null && list.Any())
                {
                    Parallel.ForEach(list.Where(p => !string.IsNullOrEmpty(p.ListJson)), (item) =>
                    {
                        item.List = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Data.MappEVInput>>(item.ListJson);
                        item.ListJson = string.Empty;
                    });
                    return list.ToList();
                }
            }
            return new List<Data.MappEVHigh>();
        }

        //查詢取得高階彙整 明細資料
        public static List<Data.MappEVInput> GetDetailList(string type, string area)
        {
            using (cyc.DB.SqlDapperConn oDB = new cyc.DB.SqlDapperConn())
            {
                var list = oDB.QueryList<Data.MappEVInput>("select Code as FacName,Type,1 as IsDisable from MappEV where IsTop=0 and Type=@Type and FacArea=@FacArea order by Code", new { Type = type, FacArea = area });
                if (list != null) return list.ToList();
            }
            return new List<Data.MappEVInput>();
        }

        //高階彙整 新增(更新)明細資料
        public static void AddHighDetail(string type, string area, MappEVInput oLast, DateTime tTime) 
        {
            var hData = HighList.FirstOrDefault(p => p.Type == type && p.FacArea == area);
            if (hData == null)
            {
                hData = new Data.MappEVHigh { Type = type, FacArea = area };
                HighList.Add(hData);
            }

            hData.IsChanged = true; //一律異動

            if (hData.List == null || !hData.List.Any())
                hData.List = GetDetailList(type, area);

            var fData = hData.List.FirstOrDefault(p => p.FacName == oLast.FacName);
            if (fData != null) //取代
            {
                int idx = hData.List.IndexOf(fData);
                hData.List.RemoveAt(idx);
                hData.List.Insert(idx, oLast);

                //尚未註記發送，且單廠未隔離，設定發送時間  ( 因舊版本設為 2999/12/31，所以增加判斷式 => SendTime大於一年以後 )
                if ((hData.SendTime == DateTime.MaxValue || hData.SendTime > tTime.AddYears(1)) && !oLast.IsDisable) 
                    hData.SendTime = tTime.AddSeconds(Shared.HighSendSeconds);
            }

            //有異動，重設時間延長
            hData.ResetTime = tTime.AddSeconds(Shared.HighStopSeconds);
        }

        ////執行高階彙整資料處理 => 回傳產出的MAPP
        //public static List<Data.MappEVMessage> DoMappEVHandleForHigh(DateTime TimeNow)
        //{
        //    //新增的MAPP
        //    List<Data.MappEVMessage> msgList = new List<Data.MappEVMessage>();
        //    for (int idx = HighList.Count - 1; idx >= 0; idx--)
        //    {
        //        StringBuilder oStr = new StringBuilder($"{MappEVCreate.JobKey} ");
        //        try
        //        {
        //            MappEVHigh hData = HighList[idx];

        //            if (hData.List.Any())
        //                oStr.Append($"處理[{(hData.FacArea == "1" ? "南廠" : "北廠")}{(hData.Type == "E" ? "地震" : "壓降")}]高階彙整");

        //            if (hData.SendTime <= TimeNow) //已達發送時間
        //            {
        //                oStr.Append("，已達發送時間");

        //                if (hData.SendTime < TimeNow.AddHours(-1)) //排除發送時間已超過1小時
        //                    oStr.Append("，發送時間已超過1小時");
        //                else
        //                {
        //                    //高階設定
        //                    var hSetting = Global.MappEV.List.FirstOrDefault(p => p.Type == hData.Type && p.FacArea == hData.FacArea && p.IsTop);
        //                    if (hSetting != null)
        //                    {
        //                        //第1筆 非Null的資料且無單廠隔離
        //                        var first = hData.List.FirstOrDefault(p => !p.IsDisable && p.Value1 != "NA");
        //                        if (first != null)
        //                        {
        //                            //轉換Template
        //                            Shared.ConvertMappTemplate(hSetting);

        //                            var oMessage = new Data.MappEVMessage
        //                            {
        //                                MS_SYS_NAME = hSetting.NormalCode,
        //                                MM_SUBJECT = Shared.ConvertMappContent(hSetting.MappSubjectX, first)//MAPP主旨
        //                            };
        //                            //MAPP內容 - 標頭+內容+結尾
        //                            var sCont = (hSetting.MappContentX ?? string.Empty).Split(new string[] { "~@~" }, StringSplitOptions.None);
        //                            if (sCont.Length == 1)
        //                                oMessage.MM_TEXT_CONTENT = string.Join(Shared.NewLine, hData.List.Select(p => Shared.ConvertMappContent(sCont[0], p, p.IsDisable)));
        //                            else if (sCont.Length == 3)
        //                            {
        //                                oMessage.MM_TEXT_CONTENT = string.Format("{0}{1}{2}",
        //                                    sCont[0].Length > 0 ? $"{Shared.ConvertMappContent(sCont[0], first)}{Shared.NewLine}" : string.Empty,
        //                                    string.Join(Shared.NewLine, hData.List.Select(p => Shared.ConvertMappContent(sCont[1], p, p.IsDisable))),
        //                                    sCont[2].Length > 0 ? $"{Shared.NewLine}{Shared.ConvertMappContent(sCont[2], first)}" : string.Empty);
        //                            }
        //                            msgList.Add(oMessage);

        //                            oStr.Append("，產出MAPP");

        //                            //系統CIM啟用，高階CIM啟用，各廠比重加總>0
        //                            if (Shared.DoCimWebApi && hSetting.CimEnable && !string.IsNullOrEmpty(hSetting.CimWebApi) && !string.IsNullOrEmpty(hSetting.CimParaData))
        //                            {
        //                                //20240716 CIM 高階 分組
        //                                try
        //                                {
        //                                    var cList = hData.List.Where(p => !p.IsDisable && p.Value1 != "NA").GroupBy(p => p.CimGroup).Where(p => p.Sum(q => q.CimLevel) > 1);
        //                                    foreach (var cData in cList)
        //                                    {
        //                                        Global.CimWebApiTaskQueue.Enqueue(new Data.CimTask
        //                                        {
        //                                            MappEVID = hSetting.ID,
        //                                            WebApi = hSetting.CimWebApi,
        //                                            Method = hSetting.CimMethod,
        //                                            ParaData = Shared.ConvertMappContent(hSetting.CimParaDataX, cData.First()),
        //                                            IsHigh = true
        //                                        });
        //                                    }
        //                                }
        //                                catch (Exception ex) { cyc.Log.WriteSysErrorLog($"MappEV處理高階彙整CIM：{ex.Message}"); }
        //                            }
        //                        }
        //                    }
        //                }

        //                hData.SendTime = DateTime.MaxValue;
        //                hData.IsChanged = true; //異動
        //            }

        //            //已達重設時間，清除資料
        //            if (hData.ResetTime <= TimeNow)
        //            {
        //                oStr.Append("，已達重設時間");

        //                hData.List?.Clear();
        //                hData.SendTime = DateTime.MaxValue;
        //                hData.ResetTime = DateTime.MaxValue;
        //                hData.IsChanged = true; //異動
        //            }

        //            //更新最新資料->補發查詢(高階彙整)
        //            if (hData.IsChanged)
        //            {
        //                oStr.Append("，記錄異動LOG");

        //                DoMappEVHighLog(hData); //更新彙整資料、記錄LOG、更新最後記錄

        //                hData.IsChanged = false;
        //            }
        //        }
        //        catch (Exception ex) 
        //        { 
        //            cyc.Log.WriteSysErrorLog($"MappEV處理高階彙整：{ex.Message}");
        //            oStr.Append($"，Error:{ex.Message}");
        //        }
        //        finally 
        //        {
        //            cyc.Log.WriteFileLog(oStr.ToString());
        //        }
        //    }
        //    return msgList;
        //}

        //記錄 目前高階彙整資料 至 LOG檔 及 最新檔
        public static void DoMappEVHighLog(MappEVHigh hData)
        {
            using (cyc.DB.SqlDapperConn oDB = new cyc.DB.SqlDapperConn())
            {
                oDB.Execute(@"
delete from MappEVHigh where FacArea=@FacArea and Type=@Type
;
insert into MappEVHigh (FacArea,Type,List,SendTime,ResetTime,UpdateTime) values (@FacArea,@Type,@List,@SendTime,@ResetTime,getdate())
;
insert into MappEVHighLog (FacArea,Type,List,SendTime,ResetTime,UpdateTime) values (@FacArea,@Type,@List,@SendTime,@ResetTime,getdate())"
, new { hData.FacArea, hData.Type, List = Newtonsoft.Json.JsonConvert.SerializeObject(hData.List), hData.SendTime, hData.ResetTime });
                //                oDB.Execute(@"
                //insert into MappEVHighLog (FacArea,Type,List,SendTime,ResetTime,UpdateTime) 
                //select FacArea,Type,List,SendTime,ResetTime,UpdateTime from MappEVHigh where FacArea=@FacArea and Type=@Type
                //;
                //delete from MappEVHigh where FacArea=@FacArea and Type=@Type
                //;
                //insert into MappEVHigh (FacArea,Type,List,SendTime,ResetTime,UpdateTime) values (@FacArea,@Type,@List,@SendTime,@ResetTime,getdate())"
                //, new { hData.FacArea, hData.Type, List = Newtonsoft.Json.JsonConvert.SerializeObject(hData.List), hData.SendTime, hData.ResetTime });

                if (hData.List.Any())
                {
                    oDB.Execute(@"
delete from MappEVLatest where FacArea=@FacArea and Type=@Type
;
insert into MappEVLatest (FacArea,Type,List,UpdateTime) values (@FacArea,@Type,@List,getdate())"
, new { hData.FacArea, hData.Type, List = Newtonsoft.Json.JsonConvert.SerializeObject(hData.List) });
                }
            }
        }

        //提供 Application_End事件 呼叫
        public static void Close()
        {
            foreach (var hData in HighList)
            {
                DoMappEVHighLog(hData); //更新彙整資料、記錄LOG、更新最後記錄
            }
        }
    }

    //CIM訊息處理
    public static class CimWebApi
    {
        static readonly string JobKey = "DoCimWebApi";
        static Queue<CYCloud.MappEV.Data.CimTask> CimWebApiTaskQueue { get; set; } = new Queue<MappEV.Data.CimTask>();
        public static void Execute()
        {
            if (Shared.DoCimWebApi && cyc.Auto.Manager.GetExclusive(JobKey))
            {
                try
                {
                    while (CimWebApiTaskQueue.Any())
                    {
                        Data.CimTask oData = Dequeue();
                        cyc.Data.ExeResult oResult = new cyc.Data.ExeResult();
                        try
                        {
                            HttpWebRequest oRequest = (HttpWebRequest)WebRequest.Create(new Uri(oData.WebApi + (oData.Method == "GET" ? "?" + oData.ParaData : "")));

                            if (oData.IsHigh) //只有高階 加上Authorization
                                oRequest.Headers.Add("Authorization", "Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbXBObyI6IiIsImVtcElEIjoiRmFjaWxpdHlUZWNoIiwiZW1wTmFtZSI6IkZhY2lsaXR5VGVjaCIsImF1dGhNZXRob2QiOiIiLCJleHAiOjI4NDAxNDA4MDB9.OlhCYnyLMR2pp6KVrpckOjKuwiyoEeNTLwBjLm6F3Bc1Wn67PWm90eClp9S78_n1noghfKwyOZsn_5UcOkqfugmeDogAwG-jXQHpXbjgU4ljbKJlanDUkt06Er9uhZaHw1pvY0cuwaGnO4xY2QD0ovS8Q2jyiv0AjWDOIqddI7zKX0P6iope8eqI5zxiQzQUWmC-PzWbr0RUBQ9TR8nKhn57lmWmQxLufO9jtQVY1riVnAkQT0mN6elRysjM3n_3SV7Hup-lFxU1MKTkspwGbIIpIGI4Rk43qyEBN2vKgU9UZ766YjC2bI-6Gkx_bSARjic4jP0ffgKWGNXq6ub4XA");

                            if (oData.Method == "POST")
                            {
                                oRequest.Method = oData.Method;
                                oRequest.ContentType = "application/json";

                                if (oData.Method == "POST" && !string.IsNullOrEmpty(oData.ParaData))
                                {
                                    List<string> sList = new List<string>();
                                    foreach (var pa in oData.ParaData.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
                                    {
                                        string[] ss = pa.Split('=');
                                        if (ss.Length == 2)
                                            sList.Add(string.Format("\"{0}\":{1}", ss[0], ss[1]));
                                    }
                                    if (sList.Count > 0)
                                    {
                                        using (var sWriter = new StreamWriter(oRequest.GetRequestStream()))
                                        {
                                            sWriter.Write(string.Format("{{{0}}}", string.Join(",", sList)));
                                            sWriter.Close();
                                        }
                                    }
                                }
                            }
                            using (HttpWebResponse oResponse = (HttpWebResponse)oRequest.GetResponse())
                            {
                                oResult.Message = oResponse.StatusCode.ToString();
                                oResponse.Close();
                            }
                        }
                        catch (Exception ex)
                        {
                            cyc.Log.WriteSysErrorLog($"{JobKey}:" + ex.StackTrace);
                            oResult.Error(ex.Message);
                        }
                        using (cyc.DB.SqlDapperConn oDB = new cyc.DB.SqlDapperConn())
                        {
                            oDB.Execute("insert into CimWebApiLog (Success,Message,MappID) values (@Success,@Message,@MappID)", new { oResult.Success, oResult.Message, MappID = oData.MappEVID });
                        }
                    }
                }
                catch { }
                cyc.Auto.Manager.CloseExclusive(JobKey);
            }
        }

        private static object oLock { get; set; } = new object();

        public static void Enqueue(Data.CimTask oData)
        {
            lock (oLock)
            {
                CimWebApiTaskQueue.Enqueue(oData);
            }
        }

        private static Data.CimTask Dequeue()
        {
            lock (oLock)
            {
                return CimWebApiTaskQueue.Dequeue();
            }
        }
    }
}

namespace CYCloud.MappEV.Data
{
    public class MappEVHigh //地震壓降MAPP高階彙整資料: MappEVHighData
    {
        public string FacArea { get; set; } // 1:南廠、2:北廠
        public string Type { get; set; } // E:地震 P:壓降
        public DateTime SendTime { get; set; } = DateTime.MaxValue; //預定發送時間
        public DateTime ResetTime { get; set; } = DateTime.MaxValue; //預定重設(清除)時間
        public DateTime UpdateTime { get; set; } //更新時間
        public bool IsChanged { get; set; } = false; //異動註記
        public List<MappEVInput> List { get; set; } //明細清單
        public string ListJson { get; set; }   
    }

    public class MappEVSetting : cyc.Data.BaseObj //地震壓降MAPP設定
    {
        public string Type { get; set; } // E:地震 P:壓降
        public string FacArea { get; set; } //廠區 => 1:南廠 2:北廠
        public bool IsTop { get; set; } //是否為高階
        public int NormalID { get; set; } //正式發送設定ID
        public int DisableID { get; set; } //隔離發送設定ID
        public string MappSubject { get; set; } //MAPP主旨
        public string MappContent { get; set; } //MAPP內容
        public int UpdateUser { get; set; }
        public DateTime UpdateTime { get; set; }

        public string NormalCode { get; set; }
        //public string DisableCode { get; set; }

        public string CimWebApi { get; set; } //CIM API 路徑
        public string CimMethod { get; set; } //CIM API 方法  POST or GET
        public string CimParaData { get; set; } //CIM API 發送參數範本
        public bool CimEnable { get; set; } //CIM啟用
        public int CimLevel { get; set; } //CIM 彙整比重，0 or 1，當高階傳送時，各廠比重加總>1時，才發送CIM
        public int CimGroup { get; set; } //CIM 彙整發送 分組 ， 高階傳送時，依照分組分批發送

        public string MappSubjectX { get; set; } //MAPP主旨(轉換後)
        public string MappContentX { get; set; } //MAPP內容(轉換後)
        public string CimParaDataX { get; set; } //CIM傳送參數(轉換後)
    }

    public class MappEVInput //地震壓降MAPP來源
    {
        public int ID { get; set; }
        public string FacName { get; set; } = ""; //廠別代號
        public string Type { get; set; } //分類 E=>地震，P=>壓降
        public string Value1 { get; set; } = Shared.NullString; //地震：震度          壓降：壓降剩餘電壓%
        public string Value2 { get; set; } = Shared.NullString; //地震：級數          壓降：壓降前用電量
        public string Value3 { get; set; } = Shared.NullString; //地震：持續時間      壓降：壓降後用電量
        public string Value4 { get; set; } = Shared.NullString; //地震：X             壓降：壓降落點區域
        public string Value5 { get; set; } = Shared.NullString; //地震：X             壓降：持續時間
        public DateTime InputTime { get; set; } //發生時間
        public string InputSource { get; set; } = ""; //來源
        //public bool InputStatus { get; set; } //狀態 0=>未處理，1=>已處理
        public bool IsDisable { get; set; } //各廠是否隔離
        public int CimLevel { get; set; } //CIM 是否加入彙整 1 or 0
        public int CimGroup { get; set; } //CIM 彙整分組 0,1,2,3
    }

    public class MappEVMessage //地震壓降MAPP資料
    {
        public string MS_SYS_NAME { get; set; }
        public int MM_CONTENT_TYPE { get; set; } = 1;
        public string MM_TEXT_CONTENT { get; set; }
        public string MM_SUBJECT { get; set; }
        public char MM_TYPE { get; set; } = 'A';
        public int MM_Priority { get; set; } = 0; //MAPP發送優先序，0:最優先 ，高階=0，單廠=1，其他(NULL=99)
        public int UPDATE_USER { get; set; }
    }

    public class MappEVLatest
    {
        public string FacArea { get; set; } // 1:南廠、2:北廠
        public string Type { get; set; } // E:地震 P:壓降
        public string List { get; set; } //最新發送清單，JSON格式 (List<MappEVHigh>)
        public DateTime UpdateTime { get; set; }
    }

    public class CimTask
    {
        public string WebApi { get; set; }
        public string Method { get; set; }
        public string ParaData { get; set; }
        public int MappEVID { get; set; }
        public bool IsHigh { get; set; }
    }

    public class LogData
    {
        public DateTime Time { get; set; } = DateTime.Now;
        public string Log { get; set; }
    }
}
