using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Dapper;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using CYCloud.Mapp.Data;
using System.Web;

namespace CYCloud.Mapp
{
    //執行 發送Mapp作業
    //[DisallowConcurrentExecutionAttribute()]
    public class AutoMapp : cyc.Auto.AutoJob //IJob
    {
        public const string JobKey = "DoMappSend";
        public const string JobName = "MAPP發送";
        protected override void Run()
        {
            if (cyc.Auto.Manager.GetExclusive(JobKey))
            {
                try
                {
                    Shared.MappTeamServic = cyc.Shared.SysQuery.GetSysSettingValue("MappTeamService"); //MAPP團隊訊息API
                    Shared.MappIMService = cyc.Shared.SysQuery.GetSysSettingValue("MappIMService"); //MAPP交談室API

                    if (!string.IsNullOrEmpty(Shared.MappTeamServic) && !string.IsNullOrEmpty(Shared.MappIMService))
                        DoExecute50();
                }
                catch (Exception ex) { cyc.Log.WriteSysErrorLog($"{JobName}:{ex.Message}"); oResult.Error(ex.Message); }
                finally { cyc.Auto.Manager.CloseExclusive(JobKey, oResult); }
            }
        }

        void DoExecute50()
        {
            int iDays = 1; //限制只發送 N天內MAPP訊息

            using (cyc.DB.SqlDapperConn oDB = new cyc.DB.SqlDapperConn(oResult))
            {
                var msgList = oDB.QueryList<MappMessage>("select top 50 A.* from MappMessage A where A.MM_SENDED_FLAG='N' and A.UPDATE_TIME>@Time order by ISNULL(A.MM_Priority,99),A.UPDATE_TIME", new { Time = DateTime.Now.AddDays(-iDays) });
                
                if (oResult.Success && msgList.Any())
                {
                    //如果有待發送MAPP，重新整理MappSetting
                    Global.MappSetting.Init(oDB, true);
                    //查詢目前隔離(未解隔)資料
                    var tmpList = oDB.QueryList<MappDisable>("select MS_SEQ_ID,ISNULL(MD_TRANS_ID,0)as MD_TRANS_ID from MappDisable where MD_STOP_TIME is null and MD_DATE_START<getdate() order by MD_DATE_START desc");
                    var disList = from t in tmpList
                                  join s in Global.MappSetting.List on t.MS_SEQ_ID equals s.MS_SEQ_ID
                                  join x in Global.MappSetting.List on t.MD_TRANS_ID equals x.MS_SEQ_ID into XX
                                  from x in XX.DefaultIfEmpty()
                                  select new cyc.Data.BaseObj
                                  { 
                                      ID = t.MS_SEQ_ID, Code = s.MS_SYS_NAME, Name = x.MS_SYS_NAME,
                                  };

                    //即時更新待發送註記
                    oDB.Execute("update MappMessage set MM_SENDED_FLAG='P' where MM_SEQ_ID in @ID and MM_SENDED_FLAG='N'", new { ID = msgList.Select(p => p.MM_SEQ_ID) });

                    foreach (var oMsg in msgList)
                    {
                        bool ToSend = true;
                        var oDisable = disList.FirstOrDefault(p => p.Code == oMsg.MS_SYS_NAME);

                        if (oDisable != null)//有隔離
                        {
                            if (string.IsNullOrEmpty(oDisable.Name))//未轉發
                            {
                                ToSend = false;
                                oDB.Execute("update MappMessage set MM_SENDED_FLAG='D' where MM_SEQ_ID=@MM_SEQ_ID and MM_SENDED_FLAG='P'", new { oMsg.MM_SEQ_ID });
                            }
                            else//有轉發
                            {
                                oMsg.MS_SYS_NAME = oDisable.Name;
                                oDB.Execute("update MappMessage set MM_TRANS_NAME=@MS_SYS_NAME where MM_SEQ_ID=@MM_SEQ_ID and MM_SENDED_FLAG='P'", new { oMsg.MM_SEQ_ID, oMsg.MS_SYS_NAME });
                            }
                        }

                        if (ToSend)
                        {
                            var oSetting = Global.MappSetting.List.FirstOrDefault(p => p.MS_SYS_NAME == oMsg.MS_SYS_NAME);
                            if (oSetting != null && oSetting.MS_SYS_STOP == "Y") //判斷是否停用
                                oDB.Execute("update MappMessage set MM_SENDED_FLAG='S' where MM_SEQ_ID=@MM_SEQ_ID and MM_SENDED_FLAG='P'", new { oMsg.MM_SEQ_ID });
                            else
                                Shared.SendMessage(oMsg, oDB);
                        }
                    }
                }
            }
        }

        #region "Bak"
//        //改每次執行限制發送50筆，避免IIS重啟，多個執行個體互相影響
//        //1次1筆，立即更新註記，判斷是否隔離後，立即發送
//        void DoExecute(cyc.Data.ExeResult oResult)
//        {
//            int iCnt = 0; //每一回合限制50筆
//            using (cyc.DB.SqlDapperConn oDB = new cyc.DB.SqlDapperConn(oResult))
//            {
//                MappMessage oMsg = GetOneMapp();
//                if (oMsg != null)
//                {
//                    //如果有待發送MAPP，重新載入MappSetting
//                    Global.MappSetting.Init(oDB, true);

//                    while (oMsg != null && iCnt < 50)
//                    {
//                        iCnt++;

//                        try
//                        {
//                            //即時更新待發送註記
//                            oDB.Execute("update MappMessage set MM_SENDED_FLAG='P' where MM_SEQ_ID=@MM_SEQ_ID and MM_SENDED_FLAG='N'", new { oMsg.MM_SEQ_ID });
//                            if (oResult.Success)
//                            {
//                                //查詢是否有隔離 ， 20231201 改為 未解隔離，一律不發送
//                                var oDisable = oDB.QueryOne<cyc.Data.BaseObj>(@"
//select A.MD_SEQ_ID as ID,B.MS_SYS_NAME as Code,C.MS_SYS_NAME as Name from MappDisable A
//inner join MappSetting B on A.MS_SEQ_ID=B.MS_SEQ_ID
//left join MappSetting C on A.MD_TRANS_ID=C.MS_SEQ_ID
//where B.MS_SYS_NAME=@MS_SYS_NAME and A.MD_STOP_TIME is null and GETDATE()>=A.MD_DATE_START", new { oMsg.MS_SYS_NAME });

//                                if (oResult.Success)
//                                {
//                                    bool ToSend = true;
//                                    if (oDisable != null) //隔離
//                                    {
//                                        if (string.IsNullOrEmpty(oDisable.Name)) //隔離，不轉送
//                                        {
//                                            ToSend = false;
//                                            oDB.Execute("update MappMessage set MM_SENDED_FLAG='D' where MM_SEQ_ID=@MM_SEQ_ID and MM_SENDED_FLAG='P'", new { oMsg.MM_SEQ_ID });
//                                        }
//                                        else //隔離，轉送
//                                        {
//                                            oMsg.MS_SYS_NAME = oDisable.Name;
//                                            oDB.Execute("update MappMessage set MM_TRANS_NAME=@MS_SYS_NAME where MM_SEQ_ID=@MM_SEQ_ID and MM_SENDED_FLAG='P'", new { oMsg.MM_SEQ_ID, oMsg.MS_SYS_NAME });
//                                        }
//                                    }

//                                    if (ToSend)
//                                    {
//                                        Shared.SendMessage(oMsg, oDB);
//                                    }
//                                }
//                                else
//                                {

//                                }
//                            }
//                        }
//                        catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex.Message + ":" + ex.StackTrace); }

//                        oMsg = GetOneMapp();
//                    }
//                }

//                MappMessage GetOneMapp()
//                {
//                    //取得一筆，最近一天內MAPP
//                    return oDB.QueryOne<MappMessage>("select top 1 A.* from MappMessage A inner join MappSetting B on A.MS_SYS_NAME=B.MS_SYS_NAME where MM_SENDED_FLAG='N' and UPDATE_TIME>@Time", new { Time = DateTime.Now.AddDays(-1) });
//                }

//                #region OLD
////                do
////                {
////                    try
////                    {
////                        oMsg = oDB.QueryOne<MappMessage>("select top 1 A.* from MappMessage A inner join MappSetting B on A.MS_SYS_NAME=B.MS_SYS_NAME where MM_SENDED_FLAG='N'");
////                        if (oMsg != null)
////                        {
////                            oDB.Execute("update MappMessage set MM_SENDED_FLAG='P' where MM_SEQ_ID=@MM_SEQ_ID and MM_SENDED_FLAG='N'", new { oMsg.MM_SEQ_ID });
////                            if (oResult.Success)
////                            {
////                                //20231201 改為 未解隔離，一律不發送
////                                var oDisable = oDB.QueryOne<cyc.Data.BaseObj>(@"
////select A.MD_SEQ_ID as ID,B.MS_SYS_NAME as Code,C.MS_SYS_NAME as Name from MappDisable A
////inner join MappSetting B on A.MS_SEQ_ID=B.MS_SEQ_ID
////left join MappSetting C on A.MD_TRANS_ID=C.MS_SEQ_ID
////where B.MS_SYS_NAME=@MS_SYS_NAME and A.MD_STOP_TIME is null and GETDATE()>=A.MD_DATE_START", new { oMsg.MS_SYS_NAME });

////                                if (oResult.Success)
////                                {
////                                    bool ToSend = true;
////                                    if (oDisable != null) //隔離
////                                    {
////                                        if (string.IsNullOrEmpty(oDisable.Name)) //隔離，不轉送
////                                        {
////                                            ToSend = false;
////                                            oDB.Execute("update MappMessage set MM_SENDED_FLAG='D' where MM_SEQ_ID=@MM_SEQ_ID and MM_SENDED_FLAG='P'", new { oMsg.MM_SEQ_ID });
////                                        }
////                                        else //隔離，轉送
////                                        {
////                                            oMsg.MS_SYS_NAME = oDisable.Name;
////                                            oDB.Execute("update MappMessage set MM_TRANS_NAME=@MS_SYS_NAME where MM_SEQ_ID=@MM_SEQ_ID and MM_SENDED_FLAG='P'", new { oMsg.MM_SEQ_ID, oMsg.MS_SYS_NAME });
////                                        }
////                                    }

////                                    if (ToSend)
////                                    {
////                                        Shared.SendMessage(oMsg, oDB);
////                                    }
////                                }
////                                else
////                                {

////                                }
////                            }
////                        }
////                    }
////                    catch (Exception ex)
////                    {
////                        cyc.Log.WriteSysErrorLog(ex.Message + ":" + ex.StackTrace);
////                        oMsg = null;
////                    }
////                    iCnt++;
////                }
////                while (oMsg != null && iCnt < 50);
//                #endregion
//            }
//        }
        #endregion

        //        void DoExecute(cyc.Data.ExeResult oResult)
        //        {
        //            using (cyc.DB.SqlDapperConn oDB = new cyc.DB.SqlDapperConn(oResult))
        //            {
        //                //尚未處理MAPP清單
        //                var mList = oDB.QueryList<MappMessage>(@"
        //select A.* from MappMessage A inner join MappSetting B on A.MS_SYS_NAME=B.MS_SYS_NAME where MM_SENDED_FLAG='N'");

        //                //20230320 新增隔離轉發 => 隔離設定時，可設定轉發，轉發設定=>不檢查是否隔離
        //                //隔離設定清單，避免重複JOIN，每一個MAPP設定只取最新隔離資料
        //                var dList = oDB.QueryList<TransDisabled>(@"
        //select B.MS_SYS_NAME as OldName,C.MS_SYS_NAME as NewName from MappDisable A
        //inner join MappSetting B on A.MS_SEQ_ID=B.MS_SEQ_ID
        //left join MappSetting C on A.MD_TRANS_ID=C.MS_SEQ_ID
        //where GETDATE() between A.MD_DATE_START and A.MD_DATE_END and A.MD_STOP_TIME is null order by A.UPDATE_TIME desc")
        //                    .GroupBy(p => p.OldName).Select(p => p.First());

        //                var allList = from m in mList
        //                              join d in dList on m.MS_SYS_NAME equals d.OldName into DD
        //                              from d in DD.DefaultIfEmpty()
        //                              select new { MAPP = m, TS = d };

        //                var sendList = allList.Where(p => p.TS == null).Select(p => p.MAPP);
        //                var disList = allList.Where(p => p.TS != null && string.IsNullOrEmpty(p.TS.NewName)).Select(p => p.MAPP.MM_SEQ_ID);
        //                var tranList = allList.Where(p => p.TS != null && !string.IsNullOrEmpty(p.TS.NewName));

        //                //隔離MAPP
        //                if (disList.Any())
        //                {
        //                    oDB.Execute("update MappMessage set MM_SENDED_FLAG='D' where MM_SEQ_ID in @ID and MM_SENDED_FLAG='N'", new { ID = disList });
        //                }
        //                //發送MAPP
        //                if (sendList.Any())
        //                {
        //                    oDB.Execute("update MappMessage set MM_SENDED_FLAG='P' where MM_SEQ_ID in @ID and MM_SENDED_FLAG='N'", new { ID = sendList.Select(p => p.MM_SEQ_ID) });
        //                    if (oResult.Success) { CYMapp.Tasks.AddList(sendList); }
        //                }
        //                //轉發MAPP
        //                if (tranList.Any())
        //                {
        //                    oDB.Execute("update MappMessage set MM_SENDED_FLAG='P',MM_TRANS_NAME=@Name where MM_SEQ_ID=@ID and MM_SENDED_FLAG='N'", tranList.Select(p => new { ID = p.MAPP.MM_SEQ_ID, Name = p.TS.NewName }));
        //                    if (oResult.Success)
        //                    {
        //                        foreach (var tData in tranList) 
        //                        { 
        //                            tData.MAPP.MS_SYS_NAME = tData.TS.NewName;
        //                            CYMapp.Tasks.Add(tData.MAPP);
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //class TransDisabled
        //{
        //    public string OldName { get; set; }
        //    public string NewName { get; set; }
        //    //public DateTime UpdateTime { get; set; }
        //}
    }

    //public class SyncMapp : cyc.Auto.AutoJob //IJob
    //{
    //    static bool IsRunning = false;
    //    static cyc.Data.ExeResult oResult = new cyc.Data.ExeResult();

    //    protected override void Run()
    //    {
    //        cyc.Auto.Manager.Update("SyncMapp");
    //        if (!IsRunning)
    //        {
    //            IsRunning = true;
    //            try
    //            {
    //                using (StreamWriter outputFile = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\SyncMappJob.txt", true))
    //                {
    //                    outputFile.WriteLine(DateTime.Now + " syncJOB開始");
    //                }
    //                oResult.Reset();
    //                string sp = cyc.Shared.SysQuery.GetAppSettingValue("SyncMappSP");
    //                if (sp.Length > 0)
    //                {
    //                    using (cyc.DB.SqlDapperConn oDB = new cyc.DB.SqlDapperConn())
    //                    {
    //                        oDB.Connection.Execute(sp, commandType: System.Data.CommandType.StoredProcedure);
    //                    }
    //                }
    //            }
    //            catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex.Message + ":" + ex.StackTrace); oResult.Error(ex.Message); }
    //            finally
    //            {
    //                Global.AutoSignal.DoSyncMappPublish("SyncMappData，結果：" + (oResult.Success ? "成功" : "失敗，" + oResult.Message));
    //                using (StreamWriter outputFile = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\SyncMappJob.txt", true))
    //                {
    //                    outputFile.WriteLine(DateTime.Now + " syncJOB結束");
    //                }
    //            }
    //            cyc.Auto.Manager.Update("SyncMapp", oResult);
    //            IsRunning = false;
    //        }
    //    }
    //}

    ///// <summary>
    ///// 執行 Mapp 佇列
    ///// </summary>
    //public static class CYMapp
    //{
    //    //執行中
    //    public static bool IsRunning { get; private set; } = false;

    //    public static void Execute()
    //    {
    //        if (!IsRunning)
    //        {
    //            IsRunning = true;
    //            try
    //            {
    //                MappMessage oTask = Tasks.GetOne();
    //                if (oTask != null)
    //                {
    //                    using (var oRequest = new MappRequest())
    //                    {
    //                        while (oTask != null)
    //                        {
    //                            oRequest.SendMessage(oTask);
    //                            System.Threading.Thread.Sleep(10);
    //                            oTask = Tasks.GetOne();
    //                        }
    //                    }
    //                }
    //            }
    //            catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex.Message + ":" + ex.StackTrace); }
    //            IsRunning = false;
    //        }
    //    }

    //    public static class Tasks
    //    {
    //        static object _Lock = new object();
    //        static Queue<MappMessage> Queue { get; set; } = new Queue<MappMessage>();//佇列
    //        public static int Count { get { return Queue.Count; } }//佇列中task數量
    //        //加入
    //        public static void AddList(IEnumerable<MappMessage> oList)
    //        {
    //            lock (_Lock) { foreach (var oTask in oList) Queue.Enqueue(oTask); }
    //        }
    //        public static void Add(MappMessage oTask)
    //        {
    //            lock (_Lock) { Queue.Enqueue(oTask); }
    //        }
    //        //取出
    //        public static MappMessage GetOne()
    //        {
    //            lock (_Lock) { if (Queue.Count > 0) { return Queue.Dequeue(); } }
    //            return null;
    //        }
    //    }
    //}

    //    public class MappRequest : IDisposable
    //    {
    //        cyc.DB.SqlDapperConn oDB = null;

    //        public cyc.Data.ExeResult oResult = new cyc.Data.ExeResult();
    //        MappMessage oMessage;
    //        MappSetting oSetting;

    //        public MappRequest()
    //        {
    //            oDB = new cyc.DB.SqlDapperConn(oResult);
    //        }
    //        //傳送訊息
    //        public void SendMessage(MappMessage tTask)
    //        {
    //            oMessage = tTask;
    //            MappSendLog oLog = new MappSendLog() { MM_SEQ_ID = oMessage.MM_SEQ_ID};

    //            oSetting = Global.MappSetting.List.FirstOrDefault(p => p.MS_SYS_NAME == oMessage.MS_SYS_NAME);
    //            if (oSetting == null) { oResult.Error("查無相關MAPP設定"); }

    //            if (oResult.Success)
    //            {
    //                byte[] bArray = null;
    //                switch (oMessage.MM_CONTENT_TYPE)
    //                {
    //                    case 1:
    //                        bArray = Encoding.UTF8.GetBytes(string.Format(Shared.sMessageTemplate,
    //                            Uri.EscapeDataString(oSetting.MS_MAPP_ACCOUNT),
    //                            Uri.EscapeDataString(oSetting.MS_MAPP_API_KEY),
    //                            oSetting.MS_MAPP_TEAM_SN,
    //                            oMessage.MM_CONTENT_TYPE,
    //                            Uri.EscapeDataString(oMessage.MM_TEXT_CONTENT ?? ""),
    //                            Uri.EscapeDataString(oMessage.ReFileName ?? ""),
    //                            Uri.EscapeDataString(oMessage.MM_FILE_SHOW_NAME ?? ""),
    //                            Uri.EscapeDataString(oMessage.MM_SUBJECT ?? "")));
    //                        SendRequestSetLog(new Uri(Shared.sURL + "?ask=postMessage"), bArray);
    //                        break;

    //                    case 2:
    //                    case 3:
    //                        bArray = Encoding.UTF8.GetBytes(string.Format(Shared.sFileTemplate,
    //                            Uri.EscapeDataString(oSetting.MS_MAPP_ACCOUNT),
    //                            Uri.EscapeDataString(oSetting.MS_MAPP_API_KEY),
    //                            oSetting.MS_MAPP_TEAM_SN,
    //                            Uri.EscapeDataString(oMessage.MM_ExtFileName.Replace(".", "")),
    //                            HttpUtility.UrlEncode(Convert.ToBase64String(oMessage.MM_MEDIA_CONTENT))));
    //                        SendRequestSetLog(new Uri(Shared.sURL + "?ask=uploadFile"), bArray);

    //                        if (oResult.Success && oLog.ML_IS_SUCCESS)
    //                        {
    //                            bArray = Encoding.UTF8.GetBytes(string.Format(Shared.sMessageTemplate,
    //                                Uri.EscapeDataString(oSetting.MS_MAPP_ACCOUNT),
    //                                Uri.EscapeDataString(oSetting.MS_MAPP_API_KEY),
    //                                oSetting.MS_MAPP_TEAM_SN,
    //                                2,
    //                                "",
    //                                Uri.EscapeDataString(oLog.FileName),
    //                                Uri.EscapeDataString(oMessage.MM_FILE_SHOW_NAME),
    //                                Uri.EscapeDataString(oMessage.MM_SUBJECT ?? "")));
    //                            SendRequestSetLog(new Uri(Shared.sURL + "?ask=postMessage"), bArray);
    //                        }
    //                        break;

    //                    case 4:
    //                        bArray = Encoding.UTF8.GetBytes(string.Format(Shared.sChatTemplate,
    //                            Uri.EscapeDataString(oSetting.MS_MAPP_ACCOUNT),
    //                            Uri.EscapeDataString(oSetting.MS_MAPP_API_KEY),
    //                            oSetting.MS_MAPP_TEAM_SN,
    //                            oMessage.MM_CONTENT_TYPE,
    //                            Uri.EscapeDataString(oMessage.MM_TEXT_CONTENT ?? ""),
    //                            Uri.EscapeDataString(oMessage.MM_FILE_SHOW_NAME ?? "")));
    //                        SendRequestSetLog(new Uri(Shared.sURL2 + "?ask=sendChatMessage"), bArray);
    //                        break;

    //                    default:
    //                        oResult.Error(string.Format("MApp發送類別錯誤：MM_SEQ_ID={0}", oMessage.MM_SEQ_ID));
    //                        break;
    //                }
    //            }

    //            void SendRequestSetLog(Uri uri, byte[] bs)
    //            {
    //                try
    //                {
    //                    HttpWebRequest oRequest = (HttpWebRequest)WebRequest.Create(uri);
    //                    oRequest.Method = "POST";
    //                    oRequest.ContentType = "application/x-www-form-urlencoded";
    //                    oRequest.ContentLength = bs.Length;
    //                    oRequest.GetRequestStream().Write(bs, 0, bs.Length);

    //                    using (HttpWebResponse oResponse = (HttpWebResponse)oRequest.GetResponse())
    //                    {
    //                        using (StreamReader sr = new StreamReader(oResponse.GetResponseStream()))
    //                        {
    //                            var oReturn = Newtonsoft.Json.JsonConvert.DeserializeObject<MappMessageResponse>(sr.ReadToEnd());
    //                            oLog.ML_BATCH_ID = oReturn.BatchID ?? string.Empty;
    //                            oLog.ML_DESCRIPTION = oReturn.Description ?? string.Empty;
    //                            oLog.ML_ERROR_CODE = oReturn.ErrorCode;
    //                            oLog.ML_IS_SUCCESS = oReturn.IsSuccess;
    //                            oLog.FileName = oReturn.FileName ?? string.Empty;
    //                            sr.Close();
    //                        }
    //                        oResponse.Close();
    //                    }
    //                }
    //                catch (Exception ex) { oResult.Error(string.Format("MApp發送發生錯誤：MM_SEQ_ID={0}，{1}", oMessage.MM_SEQ_ID, ex.Message)); }
    //            }

    //            if (!oResult.Success)
    //            {
    //                oLog.ML_IS_SUCCESS = oResult.Success;
    //                oLog.ML_DESCRIPTION = oResult.Message;
    //            }

    //            oDB.Execute(@"
    //update MappMessage set MM_SENDED_FLAG='Y' where MM_SEQ_ID=@MM_SEQ_ID;
    //delete from MappSendLog where MM_SEQ_ID=@MM_SEQ_ID;
    //insert into MappSendLog (MM_SEQ_ID,ML_IS_SUCCESS,ML_DESCRIPTION,ML_ERROR_CODE,ML_BATCH_ID,ML_SEND_TIME)
    //values (@MM_SEQ_ID,@ML_IS_SUCCESS,@ML_DESCRIPTION,@ML_ERROR_CODE,@ML_BATCH_ID,@ML_SEND_TIME)", oLog);

    //            //Global.AutoSignal.DoSendMappPublish(string.Format("ID:{0} {1}-{2}", oLog.MM_SEQ_ID, oLog.ML_IS_SUCCESS, oLog.ML_DESCRIPTION));
    //        }

    //        public void Dispose()
    //        {
    //            if (oDB != null) { oDB.Dispose(); }
    //        }
    //    }

    //    public class MappRequestNew //: IDisposable
    //    {
    //        cyc.DB.SqlDapperConn oDB = null;
    //        public cyc.Data.ExeResult oResult = new cyc.Data.ExeResult();
    //        MappMessage oMessage;
    //        MappSetting oSetting;

    //        public MappRequestNew(cyc.DB.SqlDapperConn xDB)
    //        {
    //            oDB = xDB;
    //        }
    //        //傳送訊息
    //        public void SendMessage(MappMessage tTask)
    //        {
    //            oMessage = tTask;
    //            MappSendLog oLog = new MappSendLog() { MM_SEQ_ID = oMessage.MM_SEQ_ID };

    //            oSetting = Global.MappSetting.List.FirstOrDefault(p => p.MS_SYS_NAME == oMessage.MS_SYS_NAME);
    //            if (oSetting == null) { oResult.Error("查無相關MAPP設定"); }

    //            if (oResult.Success)
    //            {
    //                byte[] bArray = null;
    //                switch (oMessage.MM_CONTENT_TYPE)
    //                {
    //                    case 1:
    //                        bArray = Encoding.UTF8.GetBytes(string.Format(Shared.sMessageTemplate,
    //                            Uri.EscapeDataString(oSetting.MS_MAPP_ACCOUNT),
    //                            Uri.EscapeDataString(oSetting.MS_MAPP_API_KEY),
    //                            oSetting.MS_MAPP_TEAM_SN,
    //                            oMessage.MM_CONTENT_TYPE,
    //                            Uri.EscapeDataString(oMessage.MM_TEXT_CONTENT ?? ""),
    //                            Uri.EscapeDataString(oMessage.ReFileName ?? ""),
    //                            Uri.EscapeDataString(oMessage.MM_FILE_SHOW_NAME ?? ""),
    //                            Uri.EscapeDataString(oMessage.MM_SUBJECT ?? "")));
    //                        SendRequestSetLog(new Uri(Shared.sURL + "?ask=postMessage"), bArray);
    //                        break;

    //                    case 2:
    //                    case 3:
    //                        bArray = Encoding.UTF8.GetBytes(string.Format(Shared.sFileTemplate,
    //                            Uri.EscapeDataString(oSetting.MS_MAPP_ACCOUNT),
    //                            Uri.EscapeDataString(oSetting.MS_MAPP_API_KEY),
    //                            oSetting.MS_MAPP_TEAM_SN,
    //                            Uri.EscapeDataString(oMessage.MM_ExtFileName.Replace(".", "")),
    //                            HttpUtility.UrlEncode(Convert.ToBase64String(oMessage.MM_MEDIA_CONTENT))));
    //                        SendRequestSetLog(new Uri(Shared.sURL + "?ask=uploadFile"), bArray);

    //                        if (oResult.Success && oLog.ML_IS_SUCCESS)
    //                        {
    //                            bArray = Encoding.UTF8.GetBytes(string.Format(Shared.sMessageTemplate,
    //                                Uri.EscapeDataString(oSetting.MS_MAPP_ACCOUNT),
    //                                Uri.EscapeDataString(oSetting.MS_MAPP_API_KEY),
    //                                oSetting.MS_MAPP_TEAM_SN,
    //                                2,
    //                                "",
    //                                Uri.EscapeDataString(oLog.FileName),
    //                                Uri.EscapeDataString(oMessage.MM_FILE_SHOW_NAME),
    //                                Uri.EscapeDataString(oMessage.MM_SUBJECT ?? "")));
    //                            SendRequestSetLog(new Uri(Shared.sURL + "?ask=postMessage"), bArray);
    //                        }
    //                        break;

    //                    case 4:
    //                        bArray = Encoding.UTF8.GetBytes(string.Format(Shared.sChatTemplate,
    //                            Uri.EscapeDataString(oSetting.MS_MAPP_ACCOUNT),
    //                            Uri.EscapeDataString(oSetting.MS_MAPP_API_KEY),
    //                            oSetting.MS_MAPP_TEAM_SN,
    //                            oMessage.MM_CONTENT_TYPE,
    //                            Uri.EscapeDataString(oMessage.MM_TEXT_CONTENT ?? ""),
    //                            Uri.EscapeDataString(oMessage.MM_FILE_SHOW_NAME ?? "")));
    //                        SendRequestSetLog(new Uri(Shared.sURL2 + "?ask=sendChatMessage"), bArray);
    //                        break;

    //                    default:
    //                        oResult.Error(string.Format("MApp發送類別錯誤：MM_SEQ_ID={0}", oMessage.MM_SEQ_ID));
    //                        break;
    //                }
    //            }

    //            void SendRequestSetLog(Uri uri, byte[] bs)
    //            {
    //                try
    //                {
    //                    HttpWebRequest oRequest = (HttpWebRequest)WebRequest.Create(uri);
    //                    oRequest.Method = "POST";
    //                    oRequest.ContentType = "application/x-www-form-urlencoded";
    //                    oRequest.ContentLength = bs.Length;
    //                    oRequest.GetRequestStream().Write(bs, 0, bs.Length);

    //                    using (HttpWebResponse oResponse = (HttpWebResponse)oRequest.GetResponse())
    //                    {
    //                        using (StreamReader sr = new StreamReader(oResponse.GetResponseStream()))
    //                        {
    //                            var oReturn = Newtonsoft.Json.JsonConvert.DeserializeObject<MappMessageResponse>(sr.ReadToEnd());
    //                            oLog.ML_BATCH_ID = oReturn.BatchID ?? string.Empty;
    //                            oLog.ML_DESCRIPTION = oReturn.Description ?? string.Empty;
    //                            oLog.ML_ERROR_CODE = oReturn.ErrorCode;
    //                            oLog.ML_IS_SUCCESS = oReturn.IsSuccess;
    //                            oLog.FileName = oReturn.FileName ?? string.Empty;
    //                            sr.Close();
    //                        }
    //                        oResponse.Close();
    //                    }
    //                }
    //                catch (Exception ex) { oResult.Error(string.Format("MApp發送發生錯誤：MM_SEQ_ID={0}，{1}", oMessage.MM_SEQ_ID, ex.Message)); }
    //            }

    //            if (!oResult.Success)
    //            {
    //                oLog.ML_IS_SUCCESS = oResult.Success;
    //                oLog.ML_DESCRIPTION = oResult.Message;
    //            }

    //            oDB.Execute(@"
    //update MappMessage set MM_SENDED_FLAG='Y' where MM_SEQ_ID=@MM_SEQ_ID;
    //delete from MappSendLog where MM_SEQ_ID=@MM_SEQ_ID;
    //insert into MappSendLog (MM_SEQ_ID,ML_IS_SUCCESS,ML_DESCRIPTION,ML_ERROR_CODE,ML_BATCH_ID,ML_SEND_TIME)
    //values (@MM_SEQ_ID,@ML_IS_SUCCESS,@ML_DESCRIPTION,@ML_ERROR_CODE,@ML_BATCH_ID,@ML_SEND_TIME)", oLog);
    //        }
    //    }

    internal static class Shared
    {
        internal static string MappTeamServic { get; set; } = string.Empty; //MAPP團隊訊息API位址
        internal static string MappIMService { get; set; } = string.Empty; //MAPP交談室API位址

        static readonly int iTimeout = 10000; //10秒
        internal static void SendMessage(MappMessage oMessage, cyc.DB.SqlDapperConn oDB)
        {
            cyc.Data.ExeResult oResult = new cyc.Data.ExeResult();
            MappSendLog oLog = new MappSendLog() { MM_SEQ_ID = oMessage.MM_SEQ_ID };

            //檢查MAPP 設定名稱
            var oSetting = Global.MappSetting.List.FirstOrDefault(p => p.MS_SYS_NAME == oMessage.MS_SYS_NAME);
            if (oSetting == null) { oResult.Error("查無相關MAPP設定"); }

            if (oResult.Success)
            {
                switch (oMessage.MM_CONTENT_TYPE)
                {
                    case 1:
                        SendRequestSetLog(new Uri(SetPostUrl(oSetting.MS_SEND_TYPE, false)), SetTextCont(oMessage, oSetting));
                        break;
                    case 2:
                    case 3:
                        SendRequestSetLog(new Uri(SetPostUrl(oSetting.MS_SEND_TYPE, true)), SetFileCont(oMessage, oSetting));
                        if (oResult.Success && oLog.ML_IS_SUCCESS)
                            SendRequestSetLog(new Uri(SetPostUrl(oSetting.MS_SEND_TYPE, false)), SetTextCont(oMessage, oSetting, oLog.FileName));
                        break;
                    default:
                        oResult.Error(string.Format("MApp發送類別錯誤：MM_SEQ_ID={0}", oMessage.MM_SEQ_ID));
                        break;
                }
            }

            void SendRequestSetLog(Uri uri, byte[] bs)
            {
                try
                {
                    HttpWebRequest oRequest = (HttpWebRequest)WebRequest.Create(uri);
                    oRequest.Method = "POST";
                    oRequest.ContentType = "application/x-www-form-urlencoded";
                    oRequest.ContentLength = bs.Length;
                    oRequest.Timeout = iTimeout;
                    using (Stream oStream = oRequest.GetRequestStream())
                    {
                        oStream.Write(bs, 0, bs.Length);
                    }

                    using (HttpWebResponse oResponse = (HttpWebResponse)oRequest.GetResponse())
                    {
                        using (StreamReader sr = new StreamReader(oResponse.GetResponseStream()))
                        {
                            var oReturn = Newtonsoft.Json.JsonConvert.DeserializeObject<MappMessageResponse>(sr.ReadToEnd());
                            oLog.ML_BATCH_ID = oReturn.BatchID ?? string.Empty;
                            oLog.ML_DESCRIPTION = oReturn.Description ?? string.Empty;
                            oLog.ML_ERROR_CODE = oReturn.ErrorCode;
                            oLog.ML_IS_SUCCESS = oReturn.IsSuccess;
                            oLog.FileName = oReturn.FileName ?? string.Empty;
                            sr.Close();
                        }
                        oResponse.Close();
                    }
                }
                catch (Exception ex) { oResult.Error(string.Format("MApp發送發生錯誤：MM_SEQ_ID={0}，{1}", oMessage.MM_SEQ_ID, ex.Message)); }
            }

            if (!oResult.Success)
            {
                oLog.ML_IS_SUCCESS = oResult.Success;
                oLog.ML_DESCRIPTION = oResult.Message;
            }

            oDB.Execute(@"
update MappMessage set MM_SENDED_FLAG='Y' where MM_SEQ_ID=@MM_SEQ_ID;
delete from MappSendLog where MM_SEQ_ID=@MM_SEQ_ID;
insert into MappSendLog (MM_SEQ_ID,ML_IS_SUCCESS,ML_DESCRIPTION,ML_ERROR_CODE,ML_BATCH_ID,ML_SEND_TIME)
values (@MM_SEQ_ID,@ML_IS_SUCCESS,@ML_DESCRIPTION,@ML_ERROR_CODE,@ML_BATCH_ID,@ML_SEND_TIME)", oLog);
        }

        //產生文字傳送內容
        static byte[] SetTextCont(MappMessage oMessage, MappSetting oSetting, string sFileName = "")
        {
            byte[] bArray;
            if (oSetting.MS_SEND_TYPE == 1) //團隊
            {
                bArray = Encoding.UTF8.GetBytes(string.Format("account={0}&api_key={1}&team_sn={2}&content_type={3}&text_content={4}&media_content={5}&file_show_name={6}&subject={7}",
                            Uri.EscapeDataString(oSetting.MS_MAPP_ACCOUNT),
                            Uri.EscapeDataString(oSetting.MS_MAPP_API_KEY),
                            oSetting.MS_MAPP_TEAM_SN,
                            string.IsNullOrEmpty(sFileName) ? 1 : 2,
                            Uri.EscapeDataString(oMessage.MM_TEXT_CONTENT ?? string.Empty),
                            Uri.EscapeDataString(string.IsNullOrEmpty(sFileName) ? oMessage.ReFileName ?? string.Empty : sFileName),
                            Uri.EscapeDataString(oMessage.MM_FILE_SHOW_NAME ?? string.Empty),
                            Uri.EscapeDataString(oMessage.MM_SUBJECT ?? string.Empty)));
            }
            else //交談室
            {
                bArray = Encoding.UTF8.GetBytes(string.Format("account={0}&api_key={1}&chat_sn={2}&content_type={3}&msg_content={4}&file_show_name={5}",
                            Uri.EscapeDataString(oSetting.MS_MAPP_ACCOUNT),
                            Uri.EscapeDataString(oSetting.MS_MAPP_API_KEY),
                            oSetting.MS_MAPP_TEAM_SN,
                            string.IsNullOrEmpty(sFileName) ? 1 : 2,
                            Uri.EscapeDataString(string.IsNullOrEmpty(sFileName) ? oMessage.MM_TEXT_CONTENT ?? string.Empty : sFileName),
                            Uri.EscapeDataString(oMessage.MM_FILE_SHOW_NAME ?? string.Empty)));
            }
            return bArray;
        }
        //產生檔案傳送內容
        static byte[] SetFileCont(MappMessage oMessage, MappSetting oSetting)
        {
            byte[] bArray;
            if (oSetting.MS_SEND_TYPE == 1) //團隊
            {
                bArray = Encoding.UTF8.GetBytes(string.Format("account={0}&api_key={1}&team_sn={2}&file_type={3}&data_binary={4}",
                    Uri.EscapeDataString(oSetting.MS_MAPP_ACCOUNT),
                    Uri.EscapeDataString(oSetting.MS_MAPP_API_KEY),
                    oSetting.MS_MAPP_TEAM_SN,
                    Uri.EscapeDataString(oMessage.MM_ExtFileName.Replace(".", "")),
                    HttpUtility.UrlEncode(Convert.ToBase64String(oMessage.MM_MEDIA_CONTENT))));
            }
            else //交談室
            {
                bArray = Encoding.UTF8.GetBytes(string.Format("account={0}&api_key={1}&file_type={2}&data_binary={3}",
                    Uri.EscapeDataString(oSetting.MS_MAPP_ACCOUNT),
                    Uri.EscapeDataString(oSetting.MS_MAPP_API_KEY),
                    Uri.EscapeDataString(oMessage.MM_ExtFileName.Replace(".", "")),
                    HttpUtility.UrlEncode(Convert.ToBase64String(oMessage.MM_MEDIA_CONTENT))));
            }
            return bArray;
        }
        //取得 POST URL
        static string SetPostUrl(int SendType, bool IsFile)
        {
            if (SendType == 1) //團隊
                return $"{MappTeamServic}?ask={(IsFile ? "uploadFile" : "postMessage")}";
            else //交談室
                return $"{MappIMService}?ask={(IsFile ? "uploadFile" : "sendChatMessage")}";
        }
    }
}

namespace CYCloud.Mapp.Data
{
    public class MappMessage //Mapp訊息
    {
        public int MM_SEQ_ID { get; set; }
        public string MS_SYS_NAME { get; set; }
        public int MM_CONTENT_TYPE { get; set; }
        public string MM_TEXT_CONTENT { get; set; }
        public byte[] MM_MEDIA_CONTENT { get; set; }
        public string MM_FILE_SHOW_NAME { get; set; }
        public char MM_SENDED_FLAG { get; set; }
        public char? MM_TYPE { get; set; }
        public string MM_SUBJECT { get; set; }
        public string MM_ExtFileName { get; set; }
        public int UPDATE_USER { get; set; }
        public DateTime UPDATE_TIME { get; set; }
        public char? MM_SendToOA { get; set; }
        public char? MM_SyncFromOA { get; set; }
        public string ReFileName { get; set; }
    }
    
    public class MappManual //Mapp手動發送(應該已經沒用到)
    {
        public int SEQ_ID { get; set; }
        public string MApp_No { get; set; }
        public string MApp_Plant { get; set; }
        public string MApp_Date { get; set; }
        public string MApp_Time { get; set; }
        public string MApp_Value1 { get; set; }
        public string MApp_Value2 { get; set; }
        public string MApp_Value3 { get; set; }
        public int MApp_Sec { get; set; }
        public string MApp_Type { get; set; }
        public char MApp_Ack_Flag { get; set; }
        public string MApp_Provider { get; set; }
    }

    public class MappSetting //Mapp設定
    {
        public int MS_SEQ_ID { get; set; }
        public string MS_SYS_NAME { get; set; }
        public string MS_SYS_DESC { get; set; }
        public string MS_MAPP_ACCOUNT { get; set; }
        public string MS_MAPP_API_KEY { get; set; }
        public int MS_MAPP_TEAM_SN { get; set; }
        public string MS_SYS_STOP { get; set; }
        public int UPDATE_USER { get; set; }
        public DateTime UPDATE_TIME { get; set; }
        public char MS_SYNC_TO_OA { get; set; }
        public int MS_SYS_DEPT { get; set; }//使用部門
        //public int MS_TRANS_ID { get; set; }//隔離轉發設定
        public string MS_SYS_DEPT_NAME { get; set; }
        public string UPDATE_USER_NAME { get; set; }
        public int MT_SEQ_ID { get; set; }//分類ID
        public bool MS_DEFAULT_REMIND { get; set; }//預設逾時未解隔通知設定

        public int MS_SEND_TYPE { get; set; } //發送類別 1:團隊 2:交談室
    }

    public class MappSettingType //Mapp設定分類
    {
        public int MT_SEQ_ID { get; set; }
        public string MT_TYPE_NAME { get; set; }
        public int MT_SORT_NUM { get; set; }
        public int UPDATE_USER { get; set; }
        public DateTime UPDATE_TIME { get; set; }
    }

    public class MappDisable //Mapp隔離設定
    {
        public int MS_SEQ_ID { get; set; }
        public int MD_SEQ_ID { get; set; }
        public string MD_REASON { get; set; }
        public DateTime MD_DATE_START { get; set; }
        public DateTime MD_DATE_END { get; set; }
        public int MD_REMIND_MIN { get; set; }
        public int MD_REMIND_SETTING { get; set; }
        public int UPDATE_USER { get; set; }
        public DateTime UPDATE_TIME { get; set; }
        public string MS_SYS_NAME { get; set; }
        public int MD_TRANS_ID { get; set; }
        public int MD_STOP_USER { get; set; }//解隔離人員
        public DateTime? MD_STOP_TIME { get; set; }//解隔離時間
        public string MD_STOP_USER_NAME { get; set; }
    }
    
    public class MappSendLog //Mapp傳送紀錄
    {
        public int ML_SEQ_ID { get; set; }
        public int MM_SEQ_ID { get; set; }
        public bool ML_IS_SUCCESS { get; set; } = false;
        public string ML_DESCRIPTION { get; set; } = string.Empty;
        public int ML_ERROR_CODE { get; set; } = 0;
        public string ML_BATCH_ID { get; set; } = string.Empty;
        public DateTime ML_SEND_TIME { get; set; } = DateTime.Now;
        public char MM_SENDED_FLAG { get; set; }

        public string FileName { get; set; } //上傳檔案用
    }

    public class MappMessageResponse //傳送訊息回覆資料
    {
        public bool IsSuccess { get; set; }
        public string Description { get; set; }
        public int ErrorCode { get; set; }
        public string BatchID { get; set; }
        public string FileName { get; set; } //上傳檔案用
    }
    
    //public class MappFileResponse //上傳檔案回覆資料
    //{
    //    public bool IsSuccess { get; set; }
    //    public string Description { get; set; }
    //    public int ErrorCode { get; set; }
    //    public string FileName { get; set; }
    //}
}
