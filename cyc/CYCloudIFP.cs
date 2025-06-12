using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using NPOI.SS.Formula.Functions;

namespace CYCloud.IFP
{
    //模擬產生 隨機辨識結果
    public static class RecognitionSimulation
    {
        public static List<RecognitionAuth> List { get; private set; } = new List<RecognitionAuth>();

        //static bool IsRunning = false;
        //static bool ToRun = false;
        //static readonly string[] Device = { "A0001", "A0002", "A0003", "A0004", "A0005", "A0006" };
        //static readonly string[] UserID = { "User01", "User02", "User03", "User04", "User05", "User06", "User07", "User08", "User09", "User10" };

        //public static void Start(bool run = true)
        //{
        //    if (!IsRunning)
        //    {
        //        IsRunning = true;
        //        ToRun = run;

        //        Random random = new Random((int)DateTime.Now.TimeOfDay.Ticks);

        //        while (ToRun)
        //        {
        //            DateTime dDate = DateTime.Now;
        //            List.Add(new CYCloud.IFP.RecognitionAuth()
        //            {
        //                DeviceName = Device[random.Next(0, 6)],
        //                FRUserID = UserID[random.Next(0, 10)],
        //                Authorization = "Success",
        //                Code = "000",
        //                //LogDateTimeS = dDate.ToString("yyyy-MM-dd HH:mm:ss"),
        //                LogDateTime = dDate
        //            });

        //            System.Threading.Thread.Sleep(random.Next(5000, 15000));

        //            List.RemoveAll(p => p.LogDateTime < dDate.AddMinutes(-3));
        //        }

        //        IsRunning = false;
        //    }
        //}

        //public static void Stop()
        //{
        //    ToRun = false;
        //    List.Clear();
        //}
    }

    public class SyncRecognitionAuth : cyc.Auto.AutoJob //IJob
    {
        public static readonly bool DoAutoCheckIn = cyc.Shared.SysQuery.GetAppSettingValue("WorkAutoCheckIn") == "1";
        public static readonly bool DoAutoCheckOut = cyc.Shared.SysQuery.GetAppSettingValue("WorkAutoCheckOut") == "1";

        public const string JobKey = "SyncRecognitionAuthInterval";
        public const string JobName = "同步辨識結果";

        private static DateTime InsertWorkCheckTime = DateTime.MinValue; //記錄 新增 WORK_CHECKIN 最新時間
        private static DateTime UpdateWorkCheckTime = DateTime.MinValue; //記錄 更新 WORK_CHECKIN 最新時間

        protected override void Run()
        {
            DateTime TimeNow = DateTime.Now;
            if (cyc.Auto.Manager.GetExclusive(JobKey))
            {
                try
                {
                    //每10分鐘 新增今日[WORK_CHECKIN]資料
                    if (InsertWorkCheckTime.AddMinutes(10) < TimeNow) 
                    {
                        InsertWorkCheckTime = TimeNow;
                        CYCloud.WorkCheck.AutoCheckInOut.ImportWorkCheck(TimeNow);
                    }
                    //每60分鐘 更新三日內[WORK_CHECKIN]資料
                    if (UpdateWorkCheckTime.AddHours(1) < TimeNow) 
                    {
                        UpdateWorkCheckTime = TimeNow;
                        CYCloud.WorkCheck.AutoCheckInOut.UpdateWorkCheck(TimeNow);
                    }
                    //執行自動簽到
                    if (DoAutoCheckIn) 
                    {
                        try
                        {
                            oResult.Reset();
                            CYCloud.WorkCheck.AutoCheckInOut.AutoCheckIn(TimeNow);
                        }
                        catch (Exception ex) { cyc.Log.WriteSysErrorLog($"自動簽到:{ex.Message}"); oResult.Error(ex.Message); }
                    }
                    //執行自動簽退
                    if (DoAutoCheckOut) 
                    {
                        try
                        {
                            oResult.Reset();
                            CYCloud.WorkCheck.AutoCheckInOut.AutoCheckOut(TimeNow);
                        }
                        catch (Exception ex) { cyc.Log.WriteSysErrorLog($"自動簽退:{ex.Message}"); oResult.Error(ex.Message); }
                    }
                }
                catch (Exception ex) { cyc.Log.WriteSysErrorLog($"{JobName}:{ex.Message}"); oResult.Error(ex.Message); }
                finally { cyc.Auto.Manager.CloseExclusive(JobKey, oResult); }
            }
        }

        //static bool IsRunning = false;
        //static bool IsRunning2 = false;

        //protected override void Run()
        //{
        //    DateTime TimeNow = DateTime.Now;
        //    cyc.Auto.Manager.Update("SyncAuth");
        //    if (!IsRunning)
        //    {
        //        IsRunning = true;
        //        cyc.Data.ExeResult oResult = new cyc.Data.ExeResult();
        //        try
        //        {
        //            //CompareData();
        //            CYCloud.WorkCheck.Shared.AutoCheckIn(TimeNow);
        //        }
        //        catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex.Message + ":" + ex.StackTrace); oResult.Error(ex.Message); }
        //        cyc.Auto.Manager.Update("SyncAuth", oResult);
        //        IsRunning = false;
        //    }
        //    if (!IsRunning2)
        //    {
        //        IsRunning2 = true;
        //        cyc.Data.ExeResult oResult2 = new cyc.Data.ExeResult();
        //        try
        //        {
        //            //CompareData2();
        //            CYCloud.WorkCheck.Shared.AutoCheckOut(TimeNow);
        //        }
        //        catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex.Message + ":" + ex.StackTrace); oResult2.Error(ex.Message); }
        //        IsRunning2 = false;
        //    }
        //}

        ////取回 辨識結果 資料
        //void GetDataFromSource()
        //{
        //    string deviceName = cyc.Shared.SysQuery.GetAppSettingValue("DeviceName");
        //    string[] devices = deviceName.Split(',');
        //    int i;
        //    for (i = 0; i < devices.Length; i++)
        //    {
        //        HttpWebRequest oRequest = (HttpWebRequest)WebRequest.Create(new Uri(cyc.Shared.SysQuery.GetAppSettingValue("RecognitionAuth")));
        //        oRequest.Method = "POST";
        //        oRequest.ContentType = "application/json";

        //        dDate = DateTime.Now;
        //        if (cyc.Shared.Check.IsInteger(sDiffMins, true)) { iMins = Convert.ToInt32(sDiffMins); }

        //        using (var sw = new System.IO.StreamWriter(oRequest.GetRequestStream()))
        //        {
        //            //送出查詢條件 DeviceName:空白
        //            //因為主機間時間可能不同步，時間區間 為 目前時間前後5分鐘

        //            sw.Write(Newtonsoft.Json.JsonConvert.SerializeObject(new
        //            {
        //                AuthCode = SyncRecognitionUser.AuthCode,
        //                FRUserId = "",
        //                ResultLog = cyc.Shared.SysQuery.GetAppSettingValue("ResultLog"),
        //                DeviceName = devices[i],
        //                StartDateTime = dDate.AddMinutes(-iMins).ToString("yyyy-MM-dd HH:mm:ss"),
        //                EndDateTime = dDate.AddMinutes(iMins).ToString("yyyy-MM-dd HH:mm:ss")
        //            }));
        //            sw.Flush();
        //            sw.Close();
        //        }

        //        //取回查詢結果
        //        var httpResponse = (HttpWebResponse)oRequest.GetResponse();
        //        using (var sr = new System.IO.StreamReader(httpResponse.GetResponseStream()))
        //        {
        //            var oData = Newtonsoft.Json.JsonConvert.DeserializeObject<RecognitionAuthResult>(sr.ReadToEnd());

        //            if (oData.Result == "Success")//結果成功 做紀錄
        //            {
        //                if (oData.Logs != null && oData.Logs.Count > 0)
        //                {
        //                    //cyc.Log.WriteSysErrorLog($"取得辨識紀錄: {oData.Logs.Count} 筆");
        //                    //CompareData(oData.Logs);
        //                }
        //            }
        //            else
        //            {
        //                if (oData.Code != "ED-00007")
        //                    cyc.Log.WriteSysErrorLog($"取得辨識紀錄失敗 {SyncRecognitionUser.AuthCode} {cyc.Shared.SysQuery.GetAppSettingValue("ResultLog")} {devices[i]} {dDate.AddMinutes(-iMins).ToString("yyyy-MM-dd HH:mm:ss")} ~ {dDate.AddMinutes(iMins).ToString("yyyy-MM-dd HH:mm:ss")} {oData.Result}: {oData.Logs}");
        //            }
        //            sr.Close();
        //        }
        //    }
        //}
        ////比對 本次取回資料 與 系統暫存區
        //void CompareData()
        //{
        //    bDB = new cyc.DB.SqlDapperConn();
        //    // 比對出 新資料
        //    //var nList = from n in lstNew
        //    //            join c in lstCurrent on new { n.DeviceName, n.Authorization, n.Code, n.FRUserID, n.FRUserName, n.LogContent, n.LogDateTime } equals new { c.DeviceName, c.Authorization, c.Code, c.FRUserID, c.FRUserName, c.LogContent, c.LogDateTime } into lstExcept
        //    //            from e in lstExcept.DefaultIfEmpty()
        //    //            where e == null
        //    //            select n;

        //    var xList = bDB.Connection.QueryMultiple(@"select ID, Fac, LogDateTime, FRUserID From RecognitionAuth Where UseFlag = 'N' and LogDateTime>= convert(varchar(100),getdate(),23) + ' 00:00:00'");

        //    //var nList = from n in lstNew
        //    //            join c in lstCurrent on new { n.DeviceName, n.Authorization, n.FRUserID, n.FRUserName, n.LogContent, n.LogDateTime } equals new { c.DeviceName, c.Authorization, c.FRUserID, c.FRUserName, c.LogContent, c.LogDateTime } into lstExcept
        //    //            from e in lstExcept.DefaultIfEmpty()
        //    //            where e == null
        //    //            select n;
        //    var nList = xList.Read<RecognitionAuth>().ToList();
        //    if (nList.Count() > 0)
        //    {
        //        //20200728 新增 廠商施工管理 自動報到
        //        CYCloud.WorkCheck.Shared.AutoCheckIn(nList.ToList());
        //        //20181204 新增 廠商施工管理 自動簽退
        //        //Task.Run(() =>
        //        //{

        //        //});

        //        //新增資料 存入暫存區
        //        //lstCurrent.AddRange(nList);

        //        ////觸發 新增資料 發送
        //        //AlertAuth.AlertAuthToClient(nList);
        //    }
        //    //清除 N分鐘前資料
        //    //lstCurrent.RemoveAll(p => p.LogDateTime < dDate.AddMinutes(-(iMins + 1)));
        //}
        ////自動報退
        //void CompareData2()
        //{
        //    bDB = new cyc.DB.SqlDapperConn();
        //    var xList2 = bDB.Connection.QueryMultiple(@"select top 1 ID, Fac, LogDateTime, FRUserID From RecognitionAuth Where UseFlag2 = 'N' and LogDateTime>= convert(varchar(100),getdate(),23) + ' 00:00:00' Order by ID");
        //    var nList2 = xList2.Read<RecognitionAuth>().ToList();
        //    if (nList2.Count() > 0)
        //    {
        //        CYCloud.WorkCheck.Shared.AutoCheckOut(nList2.ToList());
        //    }
        //}
    }

    #region 同步辨識系統-已停用
    ////發送 辨識結果 SignalR 通知 (暫不用)
    //public static class AlertAuth
    //{
    //    //public static event EventHandler<ClientAlert> RaiseIFPAlertEvent;

    //    //static List<ClientInfo> lstClient = new List<ClientInfo>();
    //    //static List<ClientAlert> lstContinued = new List<ClientAlert>();

    //    //static object oLock = new object();
    //    //static object oLock2 = new object();

    //    //public static void AddClientDevice(string ID, string Device)
    //    //{
    //    //    Task.Run(() =>
    //    //    {
    //    //        lock (oLock) { if (!lstClient.Any(p => p.ID == ID && p.Device == Device)) { lstClient.Add(new ClientInfo() { ID = ID, Device = Device }); } }
    //    //    });
    //    //}

    //    //public static void DelClientDevice(string ID, string Device)
    //    //{
    //    //    Task.Run(() =>
    //    //    {
    //    //        lock (oLock) { lstClient.RemoveAll(p => p.ID == ID && p.Device == Device); }
    //    //    });
    //    //}

    //    //public static void RemoveClient(string ID)
    //    //{
    //    //    Task.Run(() =>
    //    //    {
    //    //        lock (oLock) { lstClient.RemoveAll(p => p.ID == ID); }
    //    //    });
    //    //}

    //    //public static void AlertConfirm(string Key)
    //    //{
    //    //    lock (oLock2)
    //    //    {
    //    //        var x = lstContinued.FirstOrDefault(p => p.Key == Key);
    //    //        if (x != null) { x.OK = true; }
    //    //        lstContinued.RemoveAll(p => p.OK || p.Count >= 10);
    //    //    }
    //    //}

    //    //public static void AlertAuthToClient(IEnumerable<RecognitionAuth> oList)
    //    //{
    //    //    if (RaiseIFPAlertEvent != null)
    //    //    {
    //    //        foreach (var auth in oList)
    //    //        {
    //    //            Task.Run(() =>
    //    //            {
    //    //                Parallel.ForEach(lstClient.Where(p => p.Device == auth.DeviceName), (client) =>
    //    //                {
    //    //                    var x = new ClientAlert()
    //    //                    {
    //    //                        ClientID = client.ID,
    //    //                        Device = client.Device,
    //    //                        UserID = auth.FRUserID,
    //    //                        LogDateTime = auth.LogDateTime,
    //    //                        Result = auth.Authorization
    //    //                    };
    //    //                    lock (oLock2) { lstContinued.Add(x); }
    //    //                    RaiseIFPAlertEvent?.Invoke(null, x);
    //    //                });
    //    //            });
    //    //        }
    //    //    }
    //    //}

    //    ////public static List<ClientInfo> GetAllClient()
    //    ////{
    //    ////    return lstClient;
    //    ////}

    //    //public class ClientInfo
    //    //{
    //    //    public string ID { get; set; }
    //    //    public string Device { get; set; }
    //    //}

    //    //public class ClientAlert
    //    //{
    //    //    public ClientAlert() { Count = 0; OK = false; Key = Guid.NewGuid().ToString(); }
    //    //    public string ClientID { get; set; }
    //    //    public string Device { get; set; }
    //    //    public string UserID { get; set; }
    //    //    public string Result { get; set; }
    //    //    public DateTime LogDateTime { get; set; }
    //    //    public string Key { get; set; }
    //    //    public int Count { get; set; }
    //    //    public bool OK { get; set; }
    //    //}
    //}
    ////同步 辨識系統USERS
    //public static class SyncRecognitionUser
    //{
    //    static string _AuthCode = "";
    //    public static string AuthCode
    //    {
    //        get { if (_AuthCode.Length == 0) { _AuthCode = GetSyncAuthCode(); }; return _AuthCode; }
    //    }

    //    static string sAuthPath = cyc.Shared.SysQuery.GetAppSettingValue("RecognitionPath");
    //    //public static string AuthMAC = cyc.Shared.SysQuery.GetAppSettingValue("RecognitionMAC");
    //    //static readonly string RecSQL = "update IFP_SupplierDriver set AuthDate=getdate(),AuthResult=@AuthResult where ID=@ID";

    //    public static void SyncDriver(SupplierDriver oDriver)
    //    {
    //        cyc.Data.ExeResult oResult = new cyc.Data.ExeResult();
    //        //string AuthCode = GetSyncAuthCode();
    //        if (AuthCode.Length > 0 && sAuthPath.Length > 0)
    //        {
    //            try
    //            {
    //                var oData = SearchUser(oDriver.Code);
    //                if (oData != null)
    //                {
    //                    bool isByPhone = false;
    //                    if ((oData.Users == null || oData.Users.Count == 0) && oDriver.Phone.Trim().Length > 0)
    //                    {
    //                        isByPhone = true;
    //                        oData = SearchUser(oDriver.Phone);
    //                    }

    //                    if (oData.Users != null && oData.Users.Count > 0)
    //                    {
    //                        RecognitionUser oUser = oData.Users.First();
    //                        if (oUser.Name.Trim() != oDriver.Name.Trim() || oUser.State != (oDriver.StopDate == null ? "Enable" : "Disable"))
    //                        {
    //                            oResult = DoSyncDriver(oDriver, 'U', isByPhone);
    //                            RecordSync(oDriver);
    //                        }
    //                    }
    //                    else
    //                    {
    //                        oResult = DoSyncDriver(oDriver, 'I');
    //                        RecordSync(oDriver);
    //                    }
    //                }
    //                else { oDriver.AuthResult = false; RecordSync(oDriver); }
    //            }
    //            catch (Exception ex) { oResult.Error(ex.Message); }
    //        }
    //        else { oDriver.AuthResult = false; RecordSync(oDriver); }

    //        if (!oResult.Success) { cyc.Log.WriteSysErrorLog("辨識系統同步：" + oResult.Message); }
    //    }

    //    //public static void SyncOne(SupplierDriver oDriver)
    //    //{
    //    //    if (sAuthPath.Length > 0)
    //    //    {
    //    //        var oData = SearchUser(oDriver.Code);

    //    //        //20190211 
    //    //        bool isByPhone = false;
    //    //        if ((oData == null || oData.Result != "Success" || oData.Users == null || oData.Users.Count == 0) && oDriver.Phone.Trim().Length > 0)
    //    //        {
    //    //            isByPhone = true;
    //    //            oData = SearchUser(oDriver.Phone);
    //    //        }

    //    //        if (oData != null && oData.Result == "Success")
    //    //        {
    //    //            if (oData.Users != null && oData.Users.Count > 0)
    //    //            {
    //    //                RecognitionUser oUser = oData.Users.First();
    //    //                if (oUser.Name.Trim() != oDriver.Name.Trim() || oUser.State != (oDriver.StopDate == null ? "Enable" : "Disable"))
    //    //                {
    //    //                    DoSync(oDriver, 'U', isByPhone);
    //    //                    RecordSync(oDriver);
    //    //                }
    //    //            }
    //    //            else
    //    //            {
    //    //                DoSync(oDriver, 'I');
    //    //                RecordSync(oDriver);
    //    //            }
    //    //        }
    //    //    }
    //    //}

    //    //public static void SyncAll()
    //    //{
    //    //    if (sAuthPath.Length > 0)
    //    //    {
    //    //        var oData = SearchUser();
    //    //        if (oData != null && oData.Result == "Success" && oData.Users != null && oData.Users.Count > 0)
    //    //        {
    //    //            using (var oDB = new cyc.DB.SqlDBConn())
    //    //            {
    //    //                var gList = oDB.oConn.Query<SupplierDriver>("select ID,Code,Name,StopDate from IFP_SupplierDriver").GroupBy(p => p.Code);

    //    //                foreach (var xData in from lsO in oData.Users
    //    //                                      join lsX in gList on lsO.FRUserId equals lsX.Key
    //    //                                      select new { lsO.FRUserId, lsO.Name, List = lsX })
    //    //                {
    //    //                    var oUser = xData.List.FirstOrDefault(p => p.StopDate == null);
    //    //                    if (oUser == null)
    //    //                    {
    //    //                        DoSync(new SupplierDriver { Code = xData.FRUserId, Name = xData.Name, StopDate = DateTime.Now }, 'U');
    //    //                    }
    //    //                    else if (oUser.Name != xData.Name)
    //    //                    {
    //    //                        DoSync(oUser, 'U');
    //    //                        RecordSync(oUser);
    //    //                    }
    //    //                }

    //    //                foreach (var xData in from lsX in gList
    //    //                                      join lsO in oData.Users on lsX.Key equals lsO.FRUserId into lsOO
    //    //                                      from ls in lsOO.DefaultIfEmpty()
    //    //                                      where ls == null
    //    //                                      select lsX.FirstOrDefault(p => p.StopDate == null))
    //    //                {
    //    //                    if (xData != null) { SyncOne(xData); }
    //    //                }
    //    //            }
    //    //        }
    //    //    }
    //    //}

    //    static RecognitionUserResult SearchUser(string sCode)
    //    {
    //        if (sAuthPath.Length > 0)
    //        {
    //            try
    //            {
    //                HttpWebRequest oRequest = (HttpWebRequest)WebRequest.Create(new Uri(sAuthPath + "user/search"));
    //                oRequest.Method = "POST";
    //                oRequest.ContentType = "application/json";

    //                using (var sw = new System.IO.StreamWriter(oRequest.GetRequestStream()))
    //                {
    //                    sw.Write(Newtonsoft.Json.JsonConvert.SerializeObject(new { AuthCode, Field = "FRUserId", Keyword = sCode, State = "" }));
    //                    sw.Flush();
    //                    sw.Close();
    //                }
    //                //取回查詢結果
    //                var httpResponse = (HttpWebResponse)oRequest.GetResponse();
    //                using (var sr = new System.IO.StreamReader(httpResponse.GetResponseStream()))
    //                {
    //                    var oData = Newtonsoft.Json.JsonConvert.DeserializeObject<RecognitionUserResult>(sr.ReadToEnd());
    //                    sr.Close();
    //                    return oData;
    //                }
    //            }
    //            catch (Exception ex) { cyc.Log.WriteSysErrorLog("同步辨識人員SearchUser：" + ex.Message); }
    //        }
    //        return null;
    //    }

    //    static string GetSyncAuthCode()
    //    {
    //        string code = "";
    //        try
    //        {
    //            string sPath = sAuthPath + "authcode/get";
    //            HttpWebRequest oRequest = (HttpWebRequest)WebRequest.Create(new Uri(sPath));
    //            oRequest.Method = "POST";
    //            oRequest.ContentType = "application/json";

    //            using (var sw = new System.IO.StreamWriter(oRequest.GetRequestStream()))
    //            {
    //                sw.Write(Newtonsoft.Json.JsonConvert.SerializeObject(new
    //                {
    //                    Email = cyc.Shared.SysQuery.GetAppSettingValue("RecognitionAccount"),
    //                    Password = cyc.Shared.SysQuery.GetAppSettingValue("RecognitionPassword")
    //                }));
    //                sw.Flush();
    //                sw.Close();
    //            }
    //            //取回結果
    //            var httpResponse = (HttpWebResponse)oRequest.GetResponse();
    //            using (var sr = new System.IO.StreamReader(httpResponse.GetResponseStream()))
    //            {
    //                var oData = Newtonsoft.Json.JsonConvert.DeserializeObject<AuthResult>(sr.ReadToEnd());
    //                if (oData.Result == "Success") { code = oData.AuthCode; }
    //                sr.Close();
    //            }
    //        }
    //        catch (Exception ex) { cyc.Log.WriteSysErrorLog("同步辨識人員取AuthCode：" + ex.Message); }
    //        return code;
    //    }

    //    static void RecordSync(SupplierDriver oDriver)
    //    {
    //        using (cyc.DB.SqlDapperConn oDB = new cyc.DB.SqlDapperConn())
    //        {
    //            try
    //            {
    //                oDB.Connection.Execute("update IFP_SupplierDriver set AuthDate=getdate(),AuthResult=@AuthResult where ID=@ID", new { oDriver.AuthResult, oDriver.ID });
    //            }
    //            catch (Exception ex) { cyc.Log.WriteSysErrorLog("同步辨識人員後回寫紀錄：" + ex.Message); }
    //        }
    //    }

    //    static cyc.Data.ExeResult DoSyncDriver(SupplierDriver oDriver, char cType, bool isByPhone = false)
    //    {
    //        cyc.Data.ExeResult oResult = new cyc.Data.ExeResult();
    //        try
    //        {
    //            if (AuthCode.Length > 0)
    //            {
    //                string sPath = sAuthPath;
    //                switch (cType)
    //                {
    //                    case 'I':
    //                        sPath += "user/add"; break;
    //                    case 'U':
    //                        sPath += "user/update"; break;
    //                    case 'D':
    //                        sPath += "user/delete"; break;
    //                }
    //                HttpWebRequest oRequest = (HttpWebRequest)WebRequest.Create(new Uri(sPath));
    //                oRequest.Method = "POST";
    //                oRequest.ContentType = "application/json";

    //                using (var sw = new System.IO.StreamWriter(oRequest.GetRequestStream()))
    //                {
    //                    if (cType == 'D')
    //                        sw.Write(Newtonsoft.Json.JsonConvert.SerializeObject(new { AuthCode, FRUserId = oDriver.Code }));
    //                    else
    //                        sw.Write(Newtonsoft.Json.JsonConvert.SerializeObject(new RecognitionUser { AuthCode = AuthCode, FRUserId = (isByPhone ? oDriver.Phone : oDriver.Code), Name = oDriver.Name, State = oDriver.StopDate == null ? "Enable" : "Disable" }));
    //                    sw.Flush();
    //                    sw.Close();
    //                }
    //                //取回結果
    //                var httpResponse = (HttpWebResponse)oRequest.GetResponse();
    //                using (var sr = new System.IO.StreamReader(httpResponse.GetResponseStream()))
    //                {
    //                    var oData = Newtonsoft.Json.JsonConvert.DeserializeObject<RecognitionResult>(sr.ReadToEnd());
    //                    oDriver.AuthResult = oData.Result == "Success";
    //                    if (oData.Result != "Success")
    //                    {
    //                        oResult.Error(oData.Code);
    //                        cyc.Log.WriteSysErrorLog($"同步人員不成功 {cType} {AuthCode} {isByPhone} {oDriver.Phone} {oDriver.Code} {oDriver.Name} {oDriver.StopDate}");
    //                    }
    //                    sr.Close();
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            cyc.Log.WriteSysErrorLog(ex.Message + ":" + ex.StackTrace, oResult);
    //            cyc.Log.WriteSysErrorLog($"同步人員出錯 {cType} {AuthCode} {isByPhone} {oDriver.Phone} {oDriver.Code} {oDriver.Name} {oDriver.StopDate}");
    //        }
    //        return oResult;
    //    }
    //}
    #endregion

    #region 類別定義

    public class Supplier : BaseData
    {
        public string IDNo { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string ContactPerson { get; set; }
        public string ContactPhone { get; set; }
    }

    public class Material : BaseData
    {
        public string Type { get; set; }
        public string Unit { get; set; }
        public int SupplierID { get; set; }
        public int TypeID { get; set; }
        public string TypeName { get; set; }
        public string PortNo { get; set; }
        public string FaceDevice { get; set; }
    }
    ////填充口分區
    //public class FillingArea : BaseData
    //{
    //}
    ////填充口
    //public class FillingPort : BaseData
    //{
    //    public int AreaID { get; set; }
    //    public int MaterialID { get; set; }
    //    public string Location { get; set; }
    //    public string CameraIP { get; set; }
    //    public string DeviceName { get; set; }
    //}

    public class SupplierDriver : BaseData
    {
        public string Phone { get; set; }
        public string Photo { get; set; }
        public string CardInfo { get; set; }
        public int SupplierID { get; set; }
        public string SupplierName { get; set; }
        public DateTime? StopDate { get; set; }
        public int StopUser { get; set; }
        public DateTime? AuthDate { get; set; }
        public bool AuthResult { get; set; }
    }

    public class SupplierCar : BaseData
    {
        public int SupplierID { get; set; }
    }

    public class MaterialOrder : BaseData
    {
        public int? FillingPortID { get; set; }
        public int MaterialID { get; set; }
        public DateTime OrderDate { get; set; }
        public int OrderUser { get; set; }
        public DateTime? EstimateDate { get; set; }
        public DateTime? FillingDate { get; set; }

        public int? SupplierCarID { get; set; }
        public int? SupplierDriverID { get; set; }
        public string FillingMemo { get; set; }

        public DateTime? CancelDate { get; set; }
        public int CancelUser { get; set; }

        public string UserName { get; set; }
        public string MaterialName { get; set; }
        public string FillingPortName { get; set; }
        public int SupplierID { get; set; }
        public int MaterialTypeID { get; set; }

        public int PortNo { get; set; }
        public string FaceDevice { get; set; }
    }

    public class BaseData
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class RecognitionAuth
    {
        public int ID { get; set; }
        public string DeviceName { get; set; }
        public DateTime LogDateTime { get; set; }
        public string FRUserID { get; set; }
        public string FRUserName { get; set; }
        public string Authorization { get; set; }
        public string Code { get; set; }
        public string LogContent { get; set; }
        public string Fac { get; set; }
        //public string LogDateTimeS { get; set; }
    }

    public class RecognitionUser
    {
        public string AuthCode { get; set; }
        public string FRUserId { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public string NFC { get; set; }
        public List<RecognitionGroup> GroupId { get; set; }
        public RecognitionUser()
        {
            NFC = "";
            GroupId = new List<RecognitionGroup>();
        }
        //public string MAC { get; set; }
        //public string Gender { get; set; }
        //public int Age { get; set; }
        //public string RFIDCard { get; set; }
        //public List<RecognitionGroup> Groups { get; set; }
        //public RecognitionUser() { MAC = SyncRecognitionUser.AuthMAC; Gender = ""; Age = 18; RFIDCard = ""; Groups = new List<RecognitionGroup>(); }
    }

    public class RecognitionGroup
    {
        public string OId { get; set; }
        public string Name { get; set; }
    }

    public class RecognitionResult
    {
        public string Result { get; set; }
        public string Code { get; set; }
        //public string Message { get; set; }
    }

    public class RecognitionAuthResult : RecognitionResult
    {
        public List<RecognitionAuth> Logs { get; set; }
    }

    public class RecognitionUserResult : RecognitionResult
    {
        public List<RecognitionUser> Users { get; set; }
    }

    public class AuthResult
    {
        public string Result { get; set; }
        public string Code { get; set; }
        public string AuthCode { get; set; }
    }

    #endregion
}
