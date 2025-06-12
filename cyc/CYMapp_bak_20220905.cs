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
using Quartz;
using CYCloud.Mapp.Data;
using System.Web;

namespace CYCloud
{

    //執行 發送Mapp作業
    //[DisallowConcurrentExecutionAttribute()]
    public class AutoMapp : IJob
    {
        static bool IsRunning { get; set; } = false;
        public void Execute(IJobExecutionContext context)
        {
            pin.SysAutoTask.UpdateTask("MappSend");
            if (!IsRunning)
            {
                IsRunning = true;

                try
                {
                    DoExecute();
                }
                catch (Exception ex) { pin.Global.WriteSysError(ex.Message + ":" + ex.StackTrace); }

                if (CYMapp.Tasks.Count > 0 && !CYMapp.IsRunning) { System.Threading.Tasks.Task.Run(() => CYMapp.Execute()); }

                IsRunning = false;
            }
        }
        void DoExecute()
        {
            pin.ExeResult oResult = new pin.ExeResult();
            using (pin.DB.SqlDapperConn oDB = new pin.DB.SqlDapperConn(null, oResult))
            {
                var oList = oDB.QueryList<MappMessage>(@"
select A.* from MappMessage A inner join MappSetting B on A.MS_SYS_NAME=B.MS_SYS_NAME where MM_SENDED_FLAG='N'");

                //排除[MS_SYS_NAME]為空項目
                //oList = oList.Where(x => x.MS_SYS_NAME != null);
                //Global.AutoSignal.DoSendMappPublish("Mapp新增" + oList.Count().ToString() + "筆資料");

                //目前被隔離MAPP設定
                var dList = oDB.QueryList<string>(@"
select A.MS_SYS_NAME from MappSetting A inner join MappDisable B on A.MS_SEQ_ID=B.MS_SEQ_ID where GETDATE() between B.MD_DATE_START and B.MD_DATE_END and B.MD_STOP_TIME is null");

                var disList = from o in oList
                              join d in dList on o.MS_SYS_NAME equals d
                              select o;

                var sendList = from o in oList
                               join d in dList on o.MS_SYS_NAME equals d into DD
                               from d in DD.DefaultIfEmpty()
                               where d == null
                               select o;
                //欲發送MAPP
                if (sendList.Count() > 0)
                {
                    oDB.Execute("update MappMessage set MM_SENDED_FLAG='P' where MM_SEQ_ID in @ID and MM_SENDED_FLAG='N'", new { ID = sendList.Select(p => p.MM_SEQ_ID) });
                    if (oResult.Success) { CYMapp.Tasks.AddList(sendList); }
                }
                //被隔離MAPP
                if (disList.Count() > 0)
                {
                    oDB.Execute("update MappMessage set MM_SENDED_FLAG='D' where MM_SEQ_ID in @ID and MM_SENDED_FLAG='N'", new { ID = disList.Select(p => p.MM_SEQ_ID) });
                }
            }
            pin.SysAutoTask.UpdateTask("MappSend", oResult);
        }

    }

    //警報報表自動發送
    //[DisallowConcurrentExecutionAttribute()]
    public class WorkingReport : IJob
    {
        static bool IsRunning = false;
        static pin.ExeResult oResult = new pin.ExeResult();

        public void Execute(IJobExecutionContext context)
        {
            pin.SysAutoTask.UpdateTask("WorkCheckReport");
            if (!IsRunning)
            {
                IsRunning = true;
                try
                {
                    CYCloud.WorkCheck.Shared.WorkCheckReport();
                }
                catch (Exception ex)
                {
                    pin.Global.WriteSysError(ex.Message + ":" + ex.StackTrace); oResult.Error(ex.Message);
                }
                finally
                {

                }
                pin.SysAutoTask.UpdateTask("WorkCheckReport", oResult);
                IsRunning = false;
            }
        }
    }

    //同步內外網 MAPP DATA
    //[DisallowConcurrentExecutionAttribute()]
    public class SyncMapp : IJob
    {
        static bool IsRunning = false;
        static pin.ExeResult oResult = new pin.ExeResult();

        public void Execute(IJobExecutionContext context)
        {
            pin.SysAutoTask.UpdateTask("SyncMapp");
            if (!IsRunning)
            {
                IsRunning = true;
                try
                {
                    using (StreamWriter outputFile = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\SyncMappJob.txt", true))
                    {
                        outputFile.WriteLine(DateTime.Now + " syncJOB開始");
                    }
                    oResult.Reset();
                    string sp = pin.Comm.SysQuery.GetAppSettingValue("SyncMappSP");
                    if (sp.Length > 0)
                    {
                        using (pin.DB.SqlDapperConn oDB = new pin.DB.SqlDapperConn())
                        {
                            oDB.Connection.Execute(sp, commandType: System.Data.CommandType.StoredProcedure);
                        }
                    }
                }
                catch (Exception ex) { pin.Global.WriteSysError(ex.Message + ":" + ex.StackTrace); oResult.Error(ex.Message); }
                finally
                {
                    Global.AutoSignal.DoSyncMappPublish("SyncMappData，結果：" + (oResult.Success ? "成功" : "失敗，" + oResult.Message));
                    using (StreamWriter outputFile = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\SyncMappJob.txt", true))
                    {
                        outputFile.WriteLine(DateTime.Now + " syncJOB結束");
                    }
                }
                pin.SysAutoTask.UpdateTask("SyncMapp", oResult);
                IsRunning = false;
            }
        }
    }

    /// <summary>
    /// 執行 Mapp 佇列
    /// </summary>
    public static class CYMapp
    {
        //執行中
        public static bool IsRunning { get; private set; } = false;

        public static void Execute()
        {
            if (!IsRunning)
            {
                IsRunning = true;
                try
                {
                    MappMessage oTask = Tasks.GetOne();
                    if (oTask != null)
                    {
                        using (var oRequest = new MappRequest())
                        {
                            while (oTask != null)
                            {
                                oRequest.SendMessage(oTask);
                                System.Threading.Thread.Sleep(10);
                                oTask = Tasks.GetOne();
                            }
                        }
                    }
                }
                catch (Exception ex) { pin.Global.WriteSysError(ex.Message + ":" + ex.StackTrace); }
                IsRunning = false;
            }
        }

        public static class Tasks
        {
            static object _Lock = new object();
            static Queue<MappMessage> Queue { get; set; } = new Queue<MappMessage>();//佇列
            public static int Count { get { return Queue.Count; } }//佇列中task數量
            //加入
            public static void AddList(IEnumerable<MappMessage> oList)
            {
                lock (_Lock) { foreach (var oTask in oList) Queue.Enqueue(oTask); }
            }
            public static void Add(MappMessage oTask)
            {
                lock (_Lock) { Queue.Enqueue(oTask); }
            }
            //取出
            public static MappMessage GetOne()
            {
                lock (_Lock) { if (Queue.Count > 0) { return Queue.Dequeue(); } }
                return null;
            }
        }
    }

    public class MappRequest : IDisposable
    {
        static readonly string sURL = "https://mapp.innolux.com/teamplus_innolux/API/TeamService.ashx";
        static readonly string sURL2 = "https://mapp.innolux.com/teamplus_innolux/API/IMService.ashx";
        static readonly string sMessageTemplate = "account={0}&api_key={1}&team_sn={2}&content_type={3}&text_content={4}&media_content={5}&file_show_name={6}&subject={7}";
        static readonly string sFileTemplate = "account={0}&api_key={1}&team_sn={2}&file_type={3}&data_binary={4}";
        static readonly string sFileTemplate2 = "account={0}&api_key={1}&team_sn={2}&content_type={3}&text_content={4}&media_content={5}&file_show_name={6}";
        static readonly string sMessageTemplate2 = "account={0}&api_key={1}&chat_sn={2}&content_type={3}&msg_content={4}&file_show_name={5}";

        pin.DB.SqlDapperConn oDB = null;

        public pin.ExeResult oResult = new pin.ExeResult();
        MappMessage oMessage;
        MappSetting oSetting;

        public MappRequest()
        {
            oDB = new pin.DB.SqlDapperConn("", oResult);
        }
        //傳送訊息
        public void SendMessage(MappMessage tTask)
        {
            oMessage = tTask;
            MappSendLog oLog = new MappSendLog() { MM_SEQ_ID = oMessage.MM_SEQ_ID, ML_SEND_TIME = DateTime.Now, ML_DESCRIPTION = "", ML_BATCH_ID = "", ML_ERROR_CODE = 0, ML_IS_SUCCESS = false };

            oSetting = Global.MappSetting.List.FirstOrDefault(p => p.MS_SYS_NAME == oMessage.MS_SYS_NAME);
            if (oSetting == null) { oResult.Error("查無相關MAPP設定"); }

            string ApiFileName = "";
            if (oResult.Success && oMessage.MM_CONTENT_TYPE == "2")//檔案
            {
                //ApiFileName = SendFile();
            }

            if (oResult.Success && oMessage.MM_CONTENT_TYPE == "3")//檔案byte[]
            {
                //if (oMessage.MM_MEDIA_CONTENT.Length > 0)
                //    SendFileByte();
                //else
                //    oResult.Error("上傳檔案不存在");
                try
                {
                    HttpWebRequest oRequest = (HttpWebRequest)WebRequest.Create(new Uri(sURL + "?ask=uploadFile"));
                    oRequest.Method = "POST";
                    oRequest.ContentType = "application/x-www-form-urlencoded";

                    byte[] bs = Encoding.UTF8.GetBytes(string.Format(sFileTemplate,
                        Uri.EscapeDataString(oSetting.MS_MAPP_ACCOUNT),
                        Uri.EscapeDataString(oSetting.MS_MAPP_API_KEY),
                        oSetting.MS_MAPP_TEAM_SN,
                        Uri.EscapeDataString(oMessage.MM_ExtFileName),
                        //Uri.EscapeDataString(Path.GetExtension(oMessage.MM_MEDIA_CONTENT).Replace(".", "")),
                        HttpUtility.UrlEncode(Convert.ToBase64String(oMessage.MM_MEDIA_CONTENT))));

                    oRequest.ContentLength = bs.Length;
                    oRequest.GetRequestStream().Write(bs, 0, bs.Length);

                    HttpWebResponse oResponse = (HttpWebResponse)oRequest.GetResponse();
                    using (StreamReader sr = new StreamReader(oResponse.GetResponseStream()))
                    {
                        MappFileResponse oReturn = Newtonsoft.Json.JsonConvert.DeserializeObject<MappFileResponse>(sr.ReadToEnd());
                        oResult.Success = oReturn.IsSuccess;
                        oResult.Message = oReturn.Description;
                        if (oReturn.IsSuccess)
                        {
                            HttpWebRequest oRequest2 = (HttpWebRequest)WebRequest.Create(new Uri(sURL + "?ask=postMessage"));
                            oRequest2.Method = "POST";
                            oRequest2.ContentType = "application/x-www-form-urlencoded";
                            byte[] bs2 = Encoding.UTF8.GetBytes(string.Format(sFileTemplate2,
                            Uri.EscapeDataString(oSetting.MS_MAPP_ACCOUNT),
                            Uri.EscapeDataString(oSetting.MS_MAPP_API_KEY),
                            oSetting.MS_MAPP_TEAM_SN,
                            2,
                            Uri.EscapeDataString(oMessage.MM_TEXT_CONTENT),
                            Uri.EscapeDataString(oReturn.FileName),
                            Uri.EscapeDataString(oMessage.MM_FILE_SHOW_NAME)));
                            oRequest2.ContentLength = bs2.Length;
                            oRequest2.GetRequestStream().Write(bs2, 0, bs2.Length);
                            HttpWebResponse oResponse2 = (HttpWebResponse)oRequest2.GetResponse();
                            using (StreamReader sr2 = new StreamReader(oResponse2.GetResponseStream()))
                            {
                                MappFileResponse2 oReturn2 = Newtonsoft.Json.JsonConvert.DeserializeObject<MappFileResponse2>(sr2.ReadToEnd());
                                oLog.ML_BATCH_ID = oReturn2.BatchID;
                                oLog.ML_DESCRIPTION = oReturn2.Description;
                                oLog.ML_ERROR_CODE = oReturn2.ErrorCode;
                                oLog.ML_IS_SUCCESS = oReturn2.IsSuccess;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    pin.Global.WriteSysError(ex.Message + ":" + ex.StackTrace);
                    oResult.Error(ex.Message);
                }
            }

            if (oResult.Success && oMessage.MM_CONTENT_TYPE == "1")
            {
                try
                {
                    HttpWebRequest oRequest = (HttpWebRequest)WebRequest.Create(new Uri(sURL + "?ask=postMessage"));
                    oRequest.Method = "POST";
                    oRequest.ContentType = "application/x-www-form-urlencoded";

                    byte[] bs = Encoding.UTF8.GetBytes(string.Format(sMessageTemplate,
                        Uri.EscapeDataString(oSetting.MS_MAPP_ACCOUNT),
                        Uri.EscapeDataString(oSetting.MS_MAPP_API_KEY),
                        oSetting.MS_MAPP_TEAM_SN,
                        oMessage.MM_CONTENT_TYPE,
                        Uri.EscapeDataString(oMessage.MM_TEXT_CONTENT ?? ""),
                        Uri.EscapeDataString(oMessage.ReFileName ?? ""),
                        Uri.EscapeDataString(oMessage.MM_FILE_SHOW_NAME ?? ""),
                        Uri.EscapeDataString(oMessage.MM_SUBJECT ?? "")));

                    oRequest.ContentLength = bs.Length;
                    oRequest.GetRequestStream().Write(bs, 0, bs.Length);

                    HttpWebResponse oResponse = (HttpWebResponse)oRequest.GetResponse();
                    using (StreamReader sr = new StreamReader(oResponse.GetResponseStream()))
                    {
                        MappMessageResponse oReturn = Newtonsoft.Json.JsonConvert.DeserializeObject<MappMessageResponse>(sr.ReadToEnd());
                        oLog.ML_BATCH_ID = oReturn.BatchID;
                        oLog.ML_DESCRIPTION = oReturn.Description;
                        oLog.ML_ERROR_CODE = oReturn.ErrorCode;
                        oLog.ML_IS_SUCCESS = oReturn.IsSuccess;
                    }
                }
                catch (Exception ex)
                {
                    pin.Global.WriteSysError(ex.Message + ":" + ex.StackTrace);
                    oResult.Error(ex.Message);
                }
            }

            if (oResult.Success && oMessage.MM_CONTENT_TYPE == "4")
            {
                try
                {
                    HttpWebRequest oRequest = (HttpWebRequest)WebRequest.Create(new Uri(sURL2 + "?ask=sendChatMessage"));
                    oRequest.Method = "POST";
                    oRequest.ContentType = "application/x-www-form-urlencoded";

                    byte[] bs = Encoding.UTF8.GetBytes(string.Format(sMessageTemplate2,
                        Uri.EscapeDataString(oSetting.MS_MAPP_ACCOUNT),
                        Uri.EscapeDataString(oSetting.MS_MAPP_API_KEY),
                        oSetting.MS_MAPP_TEAM_SN,
                        oMessage.MM_CONTENT_TYPE,
                        Uri.EscapeDataString(oMessage.MM_TEXT_CONTENT ?? ""),
                        Uri.EscapeDataString(oMessage.MM_FILE_SHOW_NAME ?? "")));

                    oRequest.ContentLength = bs.Length;
                    oRequest.GetRequestStream().Write(bs, 0, bs.Length);

                    HttpWebResponse oResponse = (HttpWebResponse)oRequest.GetResponse();
                    using (StreamReader sr = new StreamReader(oResponse.GetResponseStream()))
                    {
                        MappMessageResponse oReturn = Newtonsoft.Json.JsonConvert.DeserializeObject<MappMessageResponse>(sr.ReadToEnd());
                        oLog.ML_BATCH_ID = oReturn.BatchID;
                        oLog.ML_DESCRIPTION = oReturn.Description;
                        oLog.ML_ERROR_CODE = oReturn.ErrorCode;
                        oLog.ML_IS_SUCCESS = oReturn.IsSuccess;
                    }
                }
                catch (Exception ex)
                {
                    pin.Global.WriteSysError(ex.Message);
                    oResult.Error(ex.Message);
                }
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

            Global.AutoSignal.DoSendMappPublish(string.Format("ID:{0} {1}-{2}", oLog.MM_SEQ_ID, oLog.ML_IS_SUCCESS, oLog.ML_DESCRIPTION));
        }

        private string SendFile()
        {
            try
            {
                HttpWebRequest oRequest = (HttpWebRequest)WebRequest.Create(new Uri(sURL + "?ask=uploadFile"));
                oRequest.Method = "POST";
                oRequest.ContentType = "application/x-www-form-urlencoded";

                byte[] bs = Encoding.UTF8.GetBytes(string.Format(sFileTemplate,
                    Uri.EscapeDataString(oSetting.MS_MAPP_ACCOUNT),
                    Uri.EscapeDataString(oSetting.MS_MAPP_API_KEY),
                    oSetting.MS_MAPP_TEAM_SN,
                    Uri.EscapeDataString(oMessage.MM_ExtFileName.Replace(".", "")),
                    System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(oMessage.MM_MEDIA_CONTENT))));

                oRequest.ContentLength = bs.Length;
                oRequest.GetRequestStream().Write(bs, 0, bs.Length);

                HttpWebResponse oResponse = (HttpWebResponse)oRequest.GetResponse();
                using (StreamReader sr = new StreamReader(oResponse.GetResponseStream()))
                {
                    MappFileResponse oReturn = Newtonsoft.Json.JsonConvert.DeserializeObject<MappFileResponse>(sr.ReadToEnd());
                    oResult.Success = oReturn.IsSuccess;
                    oResult.Message = oReturn.Description;
                    if (oReturn.IsSuccess) { return oReturn.FileName; }
                }
            }
            catch (Exception ex)
            {
                pin.Global.WriteSysError(ex.Message + ":" + ex.StackTrace);
                oResult.Error(ex.Message);
            }
            return "";
        }

        public void Dispose()
        {
            if (oDB != null) { oDB.Dispose(); }
        }
    }

//    /// <summary>
//    /// 執行送出Mapp的類別
//    /// </summary>
//    public class MappRequest
//    {
//        static readonly string sURL = "https://mapp.innolux.com/teamplus_innolux/API/TeamService.ashx";
//        static readonly string sMessageTemplate = "account={0}&api_key={1}&team_sn={2}&content_type={3}&text_content={4}&media_content={5}&file_show_name={6}&subject={7}";
//        static readonly string sFileTemplate = "account={0}&api_key={1}&team_sn={2}&file_type={3}&data_binary={4}";

//        public pin.ExeResult oResult = new pin.ExeResult();
//        MappMessage oMessage;
//        MappSetting oSetting;

//        public MappRequest(MappMessage tTask)
//        {
//            oMessage = tTask;
//        }
//        //傳送訊息
//        public void SendMessage()
//        {
//            MappSendLog oLog = new MappSendLog() { MM_SEQ_ID = oMessage.MM_SEQ_ID, ML_SEND_TIME = DateTime.Now, ML_DESCRIPTION = "", ML_BATCH_ID = "", ML_ERROR_CODE = 0, ML_IS_SUCCESS = false };

//            //if (gObj.MappSettings.List != null)
//            //    oSetting = gObj.MappSettings.List.FirstOrDefault(p => p.MS_SYS_NAME == oMessage.MS_SYS_NAME);
//            oSetting = Global.MappSetting.List.FirstOrDefault(p => p.MS_SYS_NAME == oMessage.MS_SYS_NAME);
//            if (oSetting == null) { oResult.Error("查無相關MAPP設定"); }

//            string ApiFileName = "";
//            if (oResult.Success && oMessage.MM_CONTENT_TYPE == "2")//檔案
//            {
//                //if (oMessage.MM_MEDIA_CONTENT.Length > 0 && File.Exists(oMessage.MM_MEDIA_CONTENT))
//                //    SendFile();
//                //else
//                //    oResult.Error("上傳檔案不存在");

//                ApiFileName = SendFile();
//            }

//            if (oResult.Success)
//            {
//                try
//                {
//                    HttpWebRequest oRequest = (HttpWebRequest)WebRequest.Create(new Uri(sURL + "?ask=postMessage"));
//                    oRequest.Method = "POST";
//                    oRequest.ContentType = "application/x-www-form-urlencoded";

//                    byte[] bs = Encoding.UTF8.GetBytes(string.Format(sMessageTemplate,
//                        Uri.EscapeDataString(oSetting.MS_MAPP_ACCOUNT),
//                        Uri.EscapeDataString(oSetting.MS_MAPP_API_KEY),
//                        oSetting.MS_MAPP_TEAM_SN,
//                        oMessage.MM_CONTENT_TYPE,
//                        Uri.EscapeDataString(oMessage.MM_TEXT_CONTENT ?? ""),
//                        //Uri.EscapeDataString(oMessage.MM_MEDIA_CONTENT ?? ""),
//                        Uri.EscapeDataString(ApiFileName ?? ""),
//                        Uri.EscapeDataString(oMessage.MM_FILE_SHOW_NAME ?? ""),
//                        Uri.EscapeDataString(oMessage.MM_SUBJECT ?? "")));

//                    oRequest.ContentLength = bs.Length;
//                    oRequest.GetRequestStream().Write(bs, 0, bs.Length);

//                    HttpWebResponse oResponse = (HttpWebResponse)oRequest.GetResponse();
//                    using (StreamReader sr = new StreamReader(oResponse.GetResponseStream()))
//                    {
//                        MappMessageResponse oReturn = Newtonsoft.Json.JsonConvert.DeserializeObject<MappMessageResponse>(sr.ReadToEnd());
//                        oLog.ML_BATCH_ID = oReturn.BatchID;
//                        oLog.ML_DESCRIPTION = oReturn.Description;
//                        oLog.ML_ERROR_CODE = oReturn.ErrorCode;
//                        oLog.ML_IS_SUCCESS = oReturn.IsSuccess;
//                    }
//                }
//                catch (Exception ex)
//                {
//                    pin.Global.WriteSysError(ex.Message + ":" + ex.StackTrace);
//                    oResult.Error(ex.Message);
//                }
//            }

//            if (!oResult.Success)
//            {
//                oLog.ML_IS_SUCCESS = oResult.Success;
//                oLog.ML_DESCRIPTION = oResult.Message;
//            }

//            try
//            {
//                pin.Global.gDB.Execute(@"
//update MappMessage set MM_SENDED_FLAG='Y' where MM_SEQ_ID=@MM_SEQ_ID;
//delete from MappSendLog where MM_SEQ_ID=@MM_SEQ_ID;
//insert into MappSendLog (MM_SEQ_ID,ML_IS_SUCCESS,ML_DESCRIPTION,ML_ERROR_CODE,ML_BATCH_ID,ML_SEND_TIME)
//values (@MM_SEQ_ID,@ML_IS_SUCCESS,@ML_DESCRIPTION,@ML_ERROR_CODE,@ML_BATCH_ID,@ML_SEND_TIME)", oLog);
//            }
//            catch (Exception ex) { pin.Global.WriteSysError(ex.Message + ":" + ex.StackTrace, oResult); }

//            gObj.AutoSignal.DoSendMappPublish(string.Format("ID:{0} {1}-{2}", oLog.MM_SEQ_ID, oLog.ML_IS_SUCCESS, oLog.ML_DESCRIPTION));
//        }

//        //上傳檔案
//        //private void SendFile()
//        //{
//        //    try
//        //    {
//        //        HttpWebRequest oRequest = (HttpWebRequest)WebRequest.Create(new Uri(sURL + "?ask=uploadFile"));
//        //        oRequest.Method = "POST";
//        //        oRequest.ContentType = "application/x-www-form-urlencoded";

//        //        byte[] bs = Encoding.UTF8.GetBytes(string.Format(sFileTemplate,
//        //            Uri.EscapeDataString(oSetting.MS_MAPP_ACCOUNT),
//        //            Uri.EscapeDataString(oSetting.MS_MAPP_API_KEY),
//        //            oSetting.MS_MAPP_TEAM_SN,
//        //            Uri.EscapeDataString(Path.GetExtension(oMessage.MM_MEDIA_CONTENT).Replace(".", "")),
//        //            System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(File.ReadAllBytes(oMessage.MM_MEDIA_CONTENT)))));

//        //        oRequest.ContentLength = bs.Length;
//        //        oRequest.GetRequestStream().Write(bs, 0, bs.Length);

//        //        HttpWebResponse oResponse = (HttpWebResponse)oRequest.GetResponse();
//        //        using (StreamReader sr = new StreamReader(oResponse.GetResponseStream()))
//        //        {
//        //            MappFileResponse oReturn = Newtonsoft.Json.JsonConvert.DeserializeObject<MappFileResponse>(sr.ReadToEnd());
//        //            oResult.Success = oReturn.IsSuccess;
//        //            oResult.Message = oReturn.Description;
//        //            if (oReturn.IsSuccess) { oMessage.MM_MEDIA_CONTENT = oReturn.FileName; }
//        //        }
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        pin.Global.WriteSysError(ex.Message + ":" + ex.StackTrace);
//        //        oResult.Error(ex.Message);
//        //    }
//        //}

//        private string SendFile()
//        {
//            try
//            {
//                HttpWebRequest oRequest = (HttpWebRequest)WebRequest.Create(new Uri(sURL + "?ask=uploadFile"));
//                oRequest.Method = "POST";
//                oRequest.ContentType = "application/x-www-form-urlencoded";

//                byte[] bs = Encoding.UTF8.GetBytes(string.Format(sFileTemplate,
//                    Uri.EscapeDataString(oSetting.MS_MAPP_ACCOUNT),
//                    Uri.EscapeDataString(oSetting.MS_MAPP_API_KEY),
//                    oSetting.MS_MAPP_TEAM_SN,
//                    Uri.EscapeDataString(oMessage.MM_ExtFileName.Replace(".", "")),
//                    System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(oMessage.MM_MEDIA_CONTENT))));

//                oRequest.ContentLength = bs.Length;
//                oRequest.GetRequestStream().Write(bs, 0, bs.Length);

//                HttpWebResponse oResponse = (HttpWebResponse)oRequest.GetResponse();
//                using (StreamReader sr = new StreamReader(oResponse.GetResponseStream()))
//                {
//                    MappFileResponse oReturn = Newtonsoft.Json.JsonConvert.DeserializeObject<MappFileResponse>(sr.ReadToEnd());
//                    oResult.Success = oReturn.IsSuccess;
//                    oResult.Message = oReturn.Description;
//                    if (oReturn.IsSuccess) { return oReturn.FileName; }
//                }
//            }
//            catch (Exception ex)
//            {
//                pin.Global.WriteSysError(ex.Message + ":" + ex.StackTrace);
//                oResult.Error(ex.Message);
//            }
//            return "";
//        }
//    }

    #region 類別定義
    //Mapp訊息
    public class MappMessage
    {
        public int MM_SEQ_ID { get; set; }
        public string MS_SYS_NAME { get; set; }
        public string MM_CONTENT_TYPE { get; set; }
        public string MM_TEXT_CONTENT { get; set; }
        //public string MM_MEDIA_CONTENT { get; set; }
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

    //Mapp手動發送
    public class MappManual
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
    #endregion
}

namespace CYCloud.Mapp.Data
{
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
        public int UPDATE_USER { get; set; }
        public DateTime UPDATE_TIME { get; set; }
        public string MS_SYS_NAME { get; set; }
        //public int MD_TRANS_ID { get; set; }
        public int MD_STOP_USER { get; set; }//解隔離人員
        public DateTime? MD_STOP_TIME { get; set; }//解隔離時間
        public string MD_STOP_USER_NAME { get; set; }
    }
    
    public class MappSendLog //Mapp傳送紀錄
    {
        public int ML_SEQ_ID { get; set; }
        public int MM_SEQ_ID { get; set; }
        public bool ML_IS_SUCCESS { get; set; }
        public string ML_DESCRIPTION { get; set; }
        public int ML_ERROR_CODE { get; set; }
        public string ML_BATCH_ID { get; set; }
        public DateTime ML_SEND_TIME { get; set; }
        public char MM_SENDED_FLAG { get; set; }
    }

    public class MappMessageResponse //傳送訊息回覆資料
    {
        public bool IsSuccess { get; set; }
        public string Description { get; set; }
        public int ErrorCode { get; set; }
        public string BatchID { get; set; }
    }
    
    public class MappFileResponse //上傳檔案回覆資料
    {
        public bool IsSuccess { get; set; }
        public string Description { get; set; }
        public int ErrorCode { get; set; }
        public string FileName { get; set; }
    }

    //發送檔案回覆資料
    public class MappFileResponse2
    {
        public bool IsSuccess { get; set; }
        public string Description { get; set; }
        public int ErrorCode { get; set; }
        public string BatchID { get; set; }
    }
}
