using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
namespace CYCloud
{
    public static class ExecLog
    {
        public static void WriteLog(LogItem log = null, cyc.DB.SqlDapperConn oDB = null)
        {
            if (log != null)
            {
                bool isNewDB = false;
                if (oDB == null) { oDB = new cyc.DB.SqlDapperConn(); isNewDB = true; }
                oDB.Execute("insert into SysOperationLog (SYS_PROG_ID,OPERATION_TYPE,OPERATION_DESC,OPERATION_USER) values (@ExecID,@ExecType,@ExecDesc,@UserID)", log);
                if (isNewDB) { oDB.Dispose(); }
            }
        }

        public static void WriteFFULog(LogItem log = null, cyc.DB.SqlDapperConn oDB = null)
        {
            if (log != null)
            {
                bool isNewDB = false;
                if (oDB == null) { oDB = new cyc.DB.SqlDapperConn(); isNewDB = true; }
                oDB.Execute("insert into FFUOperationLog (SYS_PROG_ID,OPERATION_TYPE,OPERATION_DESC,OPERATION_USER) values (@ExecID,@ExecType,@ExecDesc,@UserID)", log);
                if (isNewDB) { oDB.Dispose(); }
            }
        }

        public class LogItem
        {
            public int ExecID { get; set; }
            public string ExecType { get; set; }
            public string ExecDesc { get; set; }
            public int UserID { get; set; }
        }
    }
}
