using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CYCloud.MappEV
{
    public class MappEVCreate : cyc.Auto.AutoJob //IJob //地震壓降MAPP定時掃描來源
    {
        static object oLock = new object();
        static bool IsRunning { get; set; } = false;
        static bool IsFirstRun { get; set; } = true;//系統啟動第一次執行

        protected override void Run()
        {
            cyc.Auto.Manager.Update("MappEV");
            if (GetExecution())
            {
                cyc.Data.ExeResult oResult = new cyc.Data.ExeResult();
                try
                {
                    if (!int.TryParse(cyc.Shared.SysQuery.GetSysSettingValue("MappEVReset"), out Shared.HighStopSeconds)) { Shared.HighStopSeconds = 300; }
                    if (!int.TryParse(cyc.Shared.SysQuery.GetSysSettingValue("MappEVSecond"), out Shared.HighSendSeconds)) { Shared.HighSendSeconds = 30; }

                    List<Data.MappEVMessage> msgList = new List<Data.MappEVMessage>();
                    //string sSec = cyc.Shared.SysQuery.GetSysSettingValue("MappEVSecond");

                    //20220928 新增 地震>5 壓降=C、D 單廠訊息 加發
                    string sMappX1 = cyc.Shared.SysQuery.GetSysSettingValue("MappX1");
                    string sMappX2 = cyc.Shared.SysQuery.GetSysSettingValue("MappX2");
                    string sMappMsgX1 = cyc.Shared.SysQuery.GetSysSettingValue("MappMsgX1");
                    string sMappMsgX2 = cyc.Shared.SysQuery.GetSysSettingValue("MappMsgX2");
                    if (!int.TryParse(sMappX1, out int iMappX1)) { iMappX1 = 0; }
                    bool SendX1 = iMappX1 > 0 && !string.IsNullOrWhiteSpace(sMappMsgX1);
                    bool SendX2 = !string.IsNullOrWhiteSpace(sMappX2) && !string.IsNullOrWhiteSpace(sMappMsgX2);

                    DateTime nTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));

                    IEnumerable<Data.MappEVInput> qList;
                    using (cyc.DB.SqlDapperConn oDB = new cyc.DB.SqlDapperConn(oResult))
                    {
                        //系統啟動(重啟)後第一次執行
                        if (IsFirstRun)
                        {
                            IsFirstRun = false;
                            //取暫存DB的高階彙整
                            var tmpList = oDB.QueryList<string>("select RawData from MappEVTemp");
                            if (oDB.Result.Success && tmpList.Any())
                            {
                                //避免重複，GroupBy後，取最新值
                                Global.MappEVHighList = tmpList.Select(p => Newtonsoft.Json.JsonConvert.DeserializeObject<Data.MappHighWait>(p)).GroupBy(p => new { p.Type, p.FacArea }).Select(p => p.Last()).ToList();
                            }
                            //Shared.GetMappEVTemp(oDB);
                            //高階彙整 發送註記重設
                            foreach (var hData in Global.MappEVHighList)
                            {
                                hData.IsToSend = false;
                                if (hData.StopTime <= nTime) hData.ClearItems();
                            }
                        }

                        //高階彙整 異動註記重設
                        foreach (var hData in Global.MappEVHighList)
                            hData.IsChanged = false;

                        //查詢尚未處理的事件
                        qList = oDB.QueryList<Data.MappEVInput>("select * from MappEVInput where InputTime>@Time and InputStatus=0 order by InputTime", new { Time = nTime.AddMinutes(-5) });
                        
                        if (qList != null && qList.Any())
                        {
                            //依據[分類]分組
                            var tGroup = qList.GroupBy(p => p.Type);
                            foreach (var type in tGroup)
                            {
                                //依據[廠別]分組
                                var fGroup = type.GroupBy(p => p.FacName);

                                foreach (var fac in fGroup)
                                {
                                    //查詢是否有符合的 MappEV設定
                                    var oSetting = Global.MappEV.List.FirstOrDefault(p => p.Code == fac.Key && p.Type == type.Key);
                                    if (oSetting != null)
                                    {
                                        //轉換Template
                                        Shared.ConvertMappTemplate(oSetting);

                                        //查詢正式群組是否有隔離
                                        bool IsDisable = oDB.QueryOne<int>("select count(1) from MappDisable where MS_SEQ_ID=@ID and MD_STOP_TIME is null and @Time between MD_DATE_START and MD_DATE_END", new { ID = oSetting.NormalID, Time = nTime }) > 0;

                                        //新增至 MAPP清單
                                        msgList.AddRange(fac.Select(p => new Data.MappEVMessage
                                        {
                                            MS_SYS_NAME = IsDisable ? oSetting.DisableCode : oSetting.NormalCode,
                                            MM_SUBJECT = Shared.ConvertMappContent(oSetting.MappSubjectX, p),
                                            MM_TEXT_CONTENT = Shared.ConvertMappContent(oSetting.MappContentX, p) + CheckMappX(p)
                                        }));

                                        //20220928 新增 地震>5 壓降=C、D 單廠訊息 加發
                                        string CheckMappX(Data.MappEVInput oData)
                                        {
                                            if (SendX1 && oData.Type == "E" && int.TryParse(oData.Value2, out int iValue) && iValue >= iMappX1)
                                                return "\n" + sMappMsgX1;
                                            if (SendX2 && oData.Type == "P" && !string.IsNullOrEmpty(oData.Value4) && sMappX2.Contains(oData.Value4))
                                                return "\n" + sMappMsgX2;
                                            return string.Empty;
                                        }

                                        //系統CIM啟用，各廠CIM啟用
                                        if (Shared.DoCimWebApi && oSetting.CimEnable && !string.IsNullOrEmpty(oSetting.CimWebApi) && !string.IsNullOrEmpty(oSetting.CimParaData))
                                        {
                                            foreach (var f in fac)
                                            {
                                                Global.CimWebApiTaskQueue.Enqueue(new Data.CimTask
                                                {
                                                    MappEVID = oSetting.ID,
                                                    WebApi = oSetting.CimWebApi,
                                                    Method = oSetting.CimMethod,
                                                    ParaData = Shared.ConvertMappContent(oSetting.CimParaDataX, f)
                                                });
                                            }
                                        }

                                        //排除 地震震度小於2 or 壓降大於85%
                                        var vList = fac.ToList();
                                        if (type.Key == "E")
                                            vList.RemoveAll(p => cyc.Shared.Check.IsNumeric(p.Value2) && Convert.ToDouble(p.Value2) < 2);
                                        else
                                            vList.RemoveAll(p => cyc.Shared.Check.IsNumeric(p.Value1) && Convert.ToDouble(p.Value1) >= 85);

                                        if (vList.Count > 0)
                                        {
                                            //高階彙整 (地震、壓降 + 南廠、北廠)
                                            var wData = Global.MappEVHighList.FirstOrDefault(p => p.Type == type.Key && p.FacArea == oSetting.FacArea);
                                            if (wData == null)
                                            {
                                                wData = new Data.MappHighWait(type.Key, oSetting.FacArea);
                                                Global.MappEVHighList.Add(wData);
                                            }
                                            //處理最新的一筆
                                            var oLast = vList.Last();
                                            oLast.IsDisable = IsDisable; //單廠是否隔離
                                            oLast.CimLevel = oSetting.CimLevel; //單廠CIM比重
                                            wData.AddItem(oLast, nTime);
                                        }
                                    }
                                }
                            }
                        }

                        //處理高階彙整資料
                        for (int idx = Global.MappEVHighList.Count - 1; idx >= 0; idx--)
                        {
                            var wait = Global.MappEVHighList[idx];
                            if (wait.IsToSend && wait.SendTime <= nTime)//註記發送，且已達發送時間
                            {
                                //高階設定
                                var hSetting = Global.MappEV.List.FirstOrDefault(p => p.Type == wait.Type && p.FacArea == wait.FacArea && p.IsTop);
                                if (hSetting != null)
                                {
                                    //轉換Template
                                    Shared.ConvertMappTemplate(hSetting);

                                    //第1筆 非Null的資料
                                    var first = wait.List.FirstOrDefault(p => p.Value1 != "NA");
                                    if (first != null)
                                    {
                                        //高階正式群組是否有隔離
                                        bool bDisable = oDB.QueryOne<int>("select COUNT(1) from MappDisable where MS_SEQ_ID=@ID and MD_STOP_TIME is null and @Time between MD_DATE_START and MD_DATE_END", new { ID = hSetting.NormalID, Time = nTime }) > 0;
                                        var oMessage = new Data.MappEVMessage { MS_SYS_NAME = bDisable ? hSetting.DisableCode : hSetting.NormalCode };

                                        if (wait.List.Any(p => !p.IsDisable))
                                        {
                                            string sNewLine = "\n";
                                            //MAPP主旨
                                            oMessage.MM_SUBJECT = Shared.ConvertMappContent(hSetting.MappSubjectX, first);
                                            //MAPP內容 - 標頭+內容+結尾
                                            var sCont = (hSetting.MappContentX ?? "").Split(new string[] { "~@~" }, StringSplitOptions.None);
                                            if (sCont.Length == 1)
                                                oMessage.MM_TEXT_CONTENT = string.Join(sNewLine, wait.List.Select(p => Shared.ConvertMappContent(sCont[0], p, p.IsDisable)));
                                            else if (sCont.Length == 3)
                                            {
                                                oMessage.MM_TEXT_CONTENT = string.Format("{0}{1}{2}",
                                                    sCont[0].Length > 0 ? Shared.ConvertMappContent(sCont[0], first) + sNewLine : "",
                                                    string.Join(sNewLine, wait.List.Select(p => Shared.ConvertMappContent(sCont[1], p, p.IsDisable))),
                                                    sCont[2].Length > 0 ? sNewLine + Shared.ConvertMappContent(sCont[2], first) : "");
                                            }
                                            msgList.Add(oMessage);

                                            //系統CIM啟用，高階CIM啟用，各廠比重加總>0
                                            if (Shared.DoCimWebApi && hSetting.CimEnable && !string.IsNullOrEmpty(hSetting.CimWebApi) && !string.IsNullOrEmpty(hSetting.CimParaData) && wait.List.Sum(p => p.CimLevel) > 0)
                                            {
                                                Global.CimWebApiTaskQueue.Enqueue(new Data.CimTask
                                                {
                                                    MappEVID = hSetting.ID,
                                                    WebApi = hSetting.CimWebApi,
                                                    Method = hSetting.CimMethod,
                                                    ParaData = Shared.ConvertMappContent(hSetting.CimParaDataX, first)
                                                });
                                            }
                                        }
                                    }
                                }
                                wait.IsToSend = false;
                                wait.IsChanged = true;
                                //更新最新資料->補發查詢
                                Shared.UpdateMappEVLatest(wait, oDB);
                            }

                            //已達重設時間，清除資料
                            if (wait.StopTime <= nTime) { wait.ClearItems(); Global.MappEVHighList.Remove(wait); }

                            //如果有異動註記
                            if (wait.IsChanged) 
                            {
                                //Shared.SetMappEVTemp(wait, oDB);
                                oDB.Execute(@"
delete from MappEVTemp where [Type]=@Type and FacArea=@FacArea;
insert into MappEVTemp ([Type],FacArea,RawData,UpdateTime) values (@Type,@FacArea,@RawData,getdate())"
, new { wait.Type, wait.FacArea, RawData = Newtonsoft.Json.JsonConvert.SerializeObject(wait) });
                            }
                        }
                    }

                    //有新增 MAPP 或 MappInput
                    if (msgList.Count > 0 || (qList != null && qList.Any()))
                    {
                        using (cyc.DB.SqlDapperConn oDB = new cyc.DB.SqlDapperConn(oResult, null, true))
                        {
                            //已處理清單更新註記
                            if (qList != null && qList.Any())
                                oDB.Execute("update MappEVInput set InputStatus=1,ReadTime=getdate() where ID in @ID", new { ID = qList.Select(p => p.ID) });
                            //新增至MappMessage
                            if (oResult.Success && msgList.Count > 0)
                                oDB.Execute("insert into MappMessage (MS_SYS_NAME,MM_CONTENT_TYPE,MM_TEXT_CONTENT,MM_SUBJECT,MM_TYPE) values (@MS_SYS_NAME,@MM_CONTENT_TYPE,@MM_TEXT_CONTENT,@MM_SUBJECT,@MM_TYPE)", msgList);
                            oDB.ResultTransaction();
                        }
                    }
                }
                catch (Exception ex) { cyc.Log.WriteSysErrorLog("Mapp地震壓降:" + ex.Message); oResult.Error(ex.Message); }
                cyc.Auto.Manager.Update("MappEV", oResult);
                IsRunning = false;
            }

            //CimWebApi
            Task.Run(() => CimWebApi.Execute());
        }

        private bool GetExecution()
        {
            lock (oLock)
            {
                if (!IsRunning)
                {
                    IsRunning = true;
                    return true;
                }
                return false;
            }
        }
    }

    public static class Shared //地震壓降MAPP設定 共用
    {
        internal static int HighStopSeconds = 300;
        internal static int HighSendSeconds = 30;

        //日期、時間 格式
        public static readonly string DateFormat = "yyyy-MM-dd", TimeFormat = "HH:mm:ss", NullString = "NA", NullDate = "0000-00-00", NullTime = "00:00:00";
        //是否執行CIM
        public static bool DoCimWebApi = cyc.Shared.SysQuery.GetAppSettingValue("DoCimWebApi") == "1";
        //[地震]可用標籤
        public static readonly string[] TemplateTagE = "發報日期,發報時間,廠別代號,資料來源,日期,時間,震度,級數,持續時間".Split(',');
        //[壓降]可用標籤
        public static readonly string[] TemplateTagP = "發報日期,發報時間,廠別代號,資料來源,日期,時間,剩餘電壓,壓降前用電量,壓降後用電量,壓降落點區域,持續時間".Split(',');

        private static string ConvertMappTemplate(string sTemplate, string sType)
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
        public static void ConvertMappTemplate(Data.MappEVSetting oSetting)
        {
            if (string.IsNullOrEmpty(oSetting.MappSubjectX)) oSetting.MappSubjectX = ConvertMappTemplate(oSetting.MappSubject, oSetting.Type);
            if (string.IsNullOrEmpty(oSetting.MappContentX)) oSetting.MappContentX = ConvertMappTemplate(oSetting.MappContent, oSetting.Type);
            if (string.IsNullOrEmpty(oSetting.CimParaDataX)) oSetting.CimParaDataX = ConvertMappTemplate(oSetting.CimParaData, oSetting.Type);
        }

        public static string ConvertMappContent(string sTemp, Data.MappEVInput oData, bool IsDisable = false)
        {
            if (string.IsNullOrWhiteSpace(sTemp)) { return ""; }
            DateTime nDate = DateTime.Now;
            return string.Format(sTemp, nDate.ToString("yyyy-MM-dd"), nDate.ToString("HH:mm:ss"), oData.FacName, oData.InputSource, GetValueDate(oData.InputTime), GetValueTime(oData.InputTime), GetValue(oData.Value1), GetValue(oData.Value2), GetValue(oData.Value3), GetValue(oData.Value4), GetValue(oData.Value5));

            string GetValue(string Value) { return IsDisable ? NullString : Value ?? NullString; }
            string GetValueDate(DateTime dDate) { return IsDisable ? NullDate : dDate.ToString(DateFormat); }
            string GetValueTime(DateTime dDate) { return IsDisable ? NullTime : dDate.ToString(TimeFormat); }
        }
        public static void UpdateMappEVLatest(Data.MappHighWait oData, cyc.DB.SqlDapperConn oDB)
        {
            var nData = new Data.MappEVLatest { FacArea = oData.FacArea, Type = oData.Type, List = Newtonsoft.Json.JsonConvert.SerializeObject(oData.List), UpdateTime = DateTime.Now };
            var qData = oDB.QueryOne<Data.MappEVLatest>("select FacArea,Type from MappEVLatest where FacArea=@FacArea and Type=@Type", new { oData.FacArea, oData.Type });
            if (oDB.Result.Success)
            { 
                if (qData == null)
                    oDB.Execute("insert into MappEVLatest (FacArea,Type,List,UpdateTime) values (@FacArea,@Type,@List,@UpdateTime)", nData);
                else
                    oDB.Execute("update MappEVLatest set List=@List,UpdateTime=@UpdateTime where FacArea=@FacArea and Type=@Type", nData);
            }
        }

        //        internal static void SetMappEVTemp(Data.MappHighWait oData, cyc.DB.SqlDapperConn oDB)
        //        {
        //            oDB.Execute(@"
        //delete from MappEVTemp where [Type]=@Type and FacArea=@FacArea;
        //insert into MappEVTemp ([Type],FacArea,RawData,UpdateTime) values (@Type,@FacArea,@RawData,getdate())
        //", new { oData.Type, oData.FacArea, RawData = Newtonsoft.Json.JsonConvert.SerializeObject(oData) });
        //        }
        //internal static void GetMappEVTemp(cyc.DB.SqlDapperConn oDB)
        //{
        //    var tmpList = oDB.QueryList<string>("select RawData from MappEVTemp");
        //    if (oDB.Result.Success && tmpList.Any())
        //    {
        //        //避免重複，GroupBy後，取最新值
        //        Global.MappEVHighList = tmpList.Select(p => Newtonsoft.Json.JsonConvert.DeserializeObject<Data.MappHighWait>(p)).GroupBy(p => new { p.Type, p.FacArea }).Select(p => p.Last()).ToList();
        //    }
        //}

        internal static List<Data.MappEVInput> GetMappHighDefault(string type, string area)
        {
            using (cyc.DB.SqlDapperConn oDB = new cyc.DB.SqlDapperConn())
            {
                var list = oDB.QueryList<Data.MappEVInput>("select Code as FacName,Type,1 as IsDisable from MappEV where IsTop=0 and Type=@Type and FacArea=@FacArea order by Code", new { Type = type, FacArea = area });
                if (list != null) return list.ToList();
            }
            return new List<Data.MappEVInput>();
        }
    }

    public static class CimWebApi 
    {
        static bool IsRunning { get; set; } = false;
        public static void Execute()
        {
            if (!IsRunning && Shared.DoCimWebApi)
            {
                IsRunning = true;
                while (Global.CimWebApiTaskQueue.Count > 0)
                {
                    var oData = CYCloud.Global.CimWebApiTaskQueue.Dequeue();
                    cyc.Data.ExeResult oResult = new cyc.Data.ExeResult();
                    try
                    {
                        HttpWebRequest oRequest = (HttpWebRequest)WebRequest.Create(new Uri(oData.WebApi + (oData.Method == "GET" ? "?" + oData.ParaData : "")));
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
                        cyc.Log.WriteSysErrorLog(ex.Message + ":" + ex.StackTrace);
                        oResult.Error(ex.Message);
                    }
                    using (cyc.DB.SqlDapperConn oDB = new cyc.DB.SqlDapperConn())
                    {
                        oDB.Execute("insert into CimWebApiLog (Success,Message,MappID) values (@Success,@Message,@MappID)", new { oResult.Success, oResult.Message, MappID = oData.MappEVID });
                    }
                }
                IsRunning = false;
            }
        }
    }
}

namespace CYCloud.MappEV.Data
{
    public class MappHighWait //地震壓降MAPP高階彙整資料
    {
        public string FacArea { get; set; } // 1:南廠、2:北廠
        public string Type { get; private set; } // E:地震 P:壓降
        public bool IsToSend { get; set; } = false; //發送註記
        public bool IsChanged { get; set; } = false; //異動註記
        public DateTime SendTime { get; set; }
        public DateTime StopTime { get; set; }
        public List<MappEVInput> List { get; set; }
        public MappHighWait(string type, string area)
        {
            Type = type;
            FacArea = area;
            SendTime = DateTime.MaxValue;
            StopTime = DateTime.MaxValue;
            List = Shared.GetMappHighDefault(type, area);
        }
        public void AddItem(MappEVInput mData, DateTime nTime)
        {
            IsChanged = true;

            if (List == null || !List.Any())
                List = Shared.GetMappHighDefault(Type, FacArea);

            var fData = List.FirstOrDefault(p => p.FacName == mData.FacName);
            if (fData != null)
            {
                int idx = List.IndexOf(fData);
                List.RemoveAt(idx);
                List.Insert(idx, mData);
            }

            if (!IsToSend && !mData.IsDisable)//尚未註記發送
            {
                IsToSend = true;
                SendTime = nTime.AddSeconds(Shared.HighSendSeconds);
            }

            StopTime = nTime.AddSeconds(Shared.HighStopSeconds);//有新進資料，停止時間再延長
        }
        public void ClearItems()
        {
            if (List.Any())
            {
                List.Clear();
                IsChanged = true;
            }
            //List = Shared.GetMappHighDefault(Type, FacArea);
        }
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
        public string DisableCode { get; set; }

        public string CimWebApi { get; set; }
        public string CimMethod { get; set; }
        public string CimParaData { get; set; }
        public bool CimEnable { get; set; } //CIM啟用
        public int CimLevel { get; set; } //CIM比重，0 or 1，當高階傳送時，各廠比重加總>1時，才發送CIM

        public string MappSubjectX { get; set; } //MAPP主旨(轉換)
        public string MappContentX { get; set; } //MAPP內容(轉換)
        public string CimParaDataX { get; set; } //CIM傳送參數(轉換)
    }

    public class MappEVInputFac
    {
        public MappEVInput Data { get; set; } = new MappEVInput();
        public bool IsDisable { get; set; } = true; //各廠是否隔離
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
        public bool InputStatus { get; set; } //狀態 0=>未處理，1=>已處理
        public bool IsDisable { get; set; } //各廠是否隔離
        public int CimLevel { get; set; }
    }

    public class MappEVMessage //地震壓降MAPP資料
    {
        public string MS_SYS_NAME { get; set; }
        public int MM_CONTENT_TYPE { get; set; } = 1;
        public string MM_TEXT_CONTENT { get; set; }
        public string MM_SUBJECT { get; set; }
        public char MM_TYPE { get; set; } = 'A';
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
    }
}
