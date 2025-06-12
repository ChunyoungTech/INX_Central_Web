using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using System.Reflection;
using System.IO;
using cyc.Data;
using cyc.DB;
using cyc.UC;

namespace cyc
{
    public static partial class Global
    {
        //public delegate void ApplicationEndHandler(); 
        //public static event ApplicationEndHandler ApplicationEnd;  //Application_End 委派事件

        //public static void DoApplicationEnd()
        //{
        //    try { ApplicationEnd?.Invoke(); }
        //    catch { }
        //}

        static Dictionary<string, ISysCache> SysCacheList = new Dictionary<string, ISysCache>();

        #region SysCache
        public static SysCacheObj<SysDept> SysDept { get; private set; } = new SysCacheObj<SysDept>("select * from View_SysDeptLevel");
        public static SysCacheObj<SysDir> SysDir { get; private set; } = new SysCacheObj<SysDir>("select * from SysDir order by Seq");
        public static SysCacheObj<SysProg> SysProg { get; private set; } = new SysCacheObj<SysProg>("select A.*,B.Name as DirName from SysProg A inner join SysDir B on A.DirID=B.ID order by B.Seq,A.Seq");
        public static SysCacheObj<SysProgSub> SysProgSub { get; private set; } = new SysCacheObj<SysProgSub>("select * from SysProgSub");
        public static SysCacheObj<SysRole> SysRole { get; private set; } = new SysCacheObj<SysRole>("select * from SysRole");
        public static SysCacheObj<SysRoleProg> SysRoleProg { get; private set; } = new SysCacheObj<SysRoleProg>("select * from SysRoleProg");
        public static SysCacheObj<SysRoleProgSub> SysRoleProgSub { get; private set; } = new SysCacheObj<SysRoleProgSub>("select * from SysRoleProgSub");
        public static SysCacheObj<SysRoleUser> SysRoleUser { get; private set; } = new SysCacheObj<SysRoleUser>("select distinct * from SysRoleUser");
        public static SysCacheObj<SysSetting> SysSetting { get; private set; } = new SysCacheObj<SysSetting>("select * from SysSetting");
        #endregion

        public static void Close()
        {
            foreach (var oCache in SysCacheList.Select(p => p.Value)) { oCache.Clear(); }//清除系統快取
        }

        //static bool IsRegister = false;
        public static void AddSysCache(ISysCache nCache, string sKey)
        {
            ////註冊 Application_End 委派事件
            //if (!IsRegister) { cyc.Global.ApplicationEnd += Close; IsRegister = true; }

            SysCacheList.Remove(sKey);
            SysCacheList.Add(sKey, nCache);
        }

        public static string AppBasePath { get; } = AppDomain.CurrentDomain.BaseDirectory;
        public static bool IsDevelop { get; set; } = cyc.Shared.SysQuery.GetAppSettingValue("IsDevelop") == "1";
    }

    public static class Log
    {
        //寫入 系統錯誤記錄1
        public static void WriteSysErrorLog(string error, ExeResult oResult = null, string msg = null)
        {
            oResult?.Error(msg ?? "發生不可預期錯誤");
            System.Threading.Tasks.Task.Run(() => DoWriteError(error));
        }
        //寫入 系統錯誤記錄2
        public static void WriteSysErrorLog(Exception ex, ExeResult oResult = null)
        {
            oResult?.Error(string.Format("發生不可預期錯誤:{0}", ex.Message));
            System.Threading.Tasks.Task.Run(() => DoWriteError(string.Format("{0}:{1}", ex.Message, ex.StackTrace)));
        }
        //寫入 SQL錯誤記錄
        public static void WriteSysErrorSQL(Exception ex, SqlDapperConn oDB)
        {
            oDB.Result?.Error(string.Format("發生不可預期錯誤:{0}", ex.Message));
            System.Threading.Tasks.Task.Run(() => DoWriteError($"Msg: {ex.Message}，Cmd: {oDB.Command}， Obj: {Newtonsoft.Json.JsonConvert.SerializeObject(oDB.Object)}"));
        }
        ////寫入 系統執行記錄 (資料匯入 + 報表產出 + .......)
        //public static void WriteSysTaskLog(string sType, DateTime TimeS, DateTime TimeE, ExeResult oResult)
        //{
        //    System.Threading.Tasks.Task.Run(() => DoWriteTaskLog(new LogItem
        //    {
        //        ExecType = sType,
        //        TimeS = TimeS,
        //        TimeE = TimeE,
        //        Success = oResult.Success,
        //        Message = oResult.Message.Length > 500 ? oResult.Message.Substring(0, 500) : oResult.Message
        //    }));
        //}
        static void DoWriteError(string error)
        {
            try
            {
                using (var oDB = new DB.SqlDapperConn())
                {
                    oDB.Connection.Execute("insert into SysErrorLog (Message) values (@Msg)", new { Msg = error });
                }
            }
            catch (Exception ex)
            {
                using (StreamWriter outputFile = new StreamWriter(Path.Combine(Global.AppBasePath, "errOutput.txt"), true))
                {
                    try
                    {
                        outputFile.WriteLine(DateTime.Now + " 寫入錯誤時發生錯誤，改寫入文字檔。");
                        outputFile.WriteLine(ex.Message);
                        outputFile.WriteLine(ex.StackTrace);
                        outputFile.WriteLine("原始錯誤");
                        outputFile.WriteLine(error);
                        outputFile.WriteLine("");
                    }
                    catch { }
                }
            }
        }
        //static void DoWriteTaskLog(LogItem oItem)
        //{
        //    cyc.DB.Shared.Execute(@"insert into SysTaskLog (ExecType,TimeS,TimeE,Success,Message) values (@ExecType,@TimeS,@TimeE,@Success,@Message)", oItem);
        //}
        //public static void WriteOperateLog(LogItem log, cyc.DB.SqlDapperConn oDB = null)
        //{
        //    if (log != null)
        //    {
        //        bool isNewDB = cyc.DB.Shared.CheckNewDB(ref oDB);
        //        oDB.Execute("insert into SysOperationLog (SYS_PROG_ID,OPERATION_TYPE,OPERATION_DESC,OPERATION_USER) values (@ExecID,@ExecType,@ExecDesc,@UserID)", log);
        //        if (isNewDB) { oDB.Dispose(); }
        //    }
        //}

        //public class LogItem
        //{
        //    public int ExecID { get; set; }
        //    public string ExecType { get; set; }
        //    public string ExecDesc { get; set; }
        //    public int UserID { get; set; }
        //    public bool Success { get; set; }
        //    public string Message { get; set; }
        //    public DateTime TimeS { get; set; }
        //    public DateTime TimeE { get; set; }
        //}

        public static void WriteFileLog(string sLog, string sType = "")
        {
            System.Threading.Tasks.Task.Run(() => DoWriteFileLog(sLog, sType));
        }

        private static object oLock = new object();
        private static void DoWriteFileLog(string sLog, string sType)
        {
            try
            {
                lock (oLock)
                {
                    using (StreamWriter outputFile = new StreamWriter(Path.Combine(Global.AppBasePath, "_logFile", $"{DateTime.Today:yyyyMMdd}_{sType}.txt"), true))
                    {
                        outputFile.WriteLine(sLog);
                    }
                }
            }
            catch { }
        }
    }

    //public static class Error
    //{
    //    private static object Lock = new object();
    //    private static List<ErrorData> List { get; set; } = new List<ErrorData>();
        
    //    public static string AddError(Exception ex)
    //    {
    //        lock (Lock)
    //        {
    //            string sID = Guid.NewGuid().ToString("N"); //取得新ID
    //            List.Add(new ErrorData { ID = sID, Ex = ex }); //新增至List
    //            Log.WriteSysErrorLog(ex); //寫入DB
    //            List.RemoveAll(p => p.Time < DateTime.Now.AddMinutes(-5)); //移除超過5分鐘資料
    //            return sID;
    //        }
    //    }

    //    public static ErrorData GetError(string sID)
    //    {
    //        lock (Lock)
    //        {
    //            var oError = List.FirstOrDefault(p => p.ID == sID);
    //            if (oError != null) { List.Remove(oError); }
    //            return oError;
    //        }
    //    }

    //    public class ErrorData
    //    {
    //        public string ID { get; set; }
    //        public Exception Ex { get; set; }
    //        public DateTime Time { get; set; } = DateTime.Now;
    //    }
    //}
}
