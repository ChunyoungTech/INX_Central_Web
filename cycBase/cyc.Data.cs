using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace cyc.Data
{
    public class BaseObj
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class UserInfo
    {
        public SysUser User { get; set; }
        public SysDept Dept { get; set; }
        public int[] Role { get; set; }
        public string Guid { get; set; }
        public string IP { get; set; }
        public int From { get; set; } = 0;
    }

    public class SysUser : BaseObj
    {
        public int DeptID { get; set; }
        public bool isManager { get; set; }
        public bool Enabled { get; set; }
        public string Password { get; set; }
        public int DeptLevel { get; set; }
        public string LoginToken { get; set; }
        public object Clone() { return this.MemberwiseClone(); }
    }

    public class SysDept : BaseObj
    {
        public int UpperID { get; set; }
        public int LevelNo { get; set; }
        public string NameAll { get; set; }
        public int ID1 { get; set; }
        public int ID2 { get; set; }
        public int ID3 { get; set; }
        public int ID4 { get; set; }
        public int ID5 { get; set; }
        public int ID6 { get; set; }
        public object Clone() { return this.MemberwiseClone(); }
    }

    public class SysDir : BaseObj
    {
        //public int ID { get; set; }
        //public string Code { get; set; }
        //public string Name { get; set; }
        public int Seq { get; set; }
        public bool Enabled { get; set; }
    }

    public class SysProg : BaseObj
    {
        public int DirID { get; set; }
        public string DirName { get; set; }
        public string Folder { get; set; }
        public string Path { get; set; }
        public int Seq { get; set; }
        public bool Enabled { get; set; }
        public bool IsOpen { get; set; }//20250502 另開新視窗
        public object Clone() { return this.MemberwiseClone(); }
    }

    public class SysProgSub : BaseObj
    {
        public string Path { get; set; }
        public int UpperID { get; set; }
        public bool isShow { get; set; }
    }

    public class SysRole : BaseObj
    {
        public int? LevelNo { get; set; }
        public bool IsDefault { get; set; }
        public bool Enabled { get; set; }
        public int User { get; set; }
    }

    public class SysRoleProg
    {
        public int RoleID { get; set; }
        public int ProgID { get; set; }
        public bool isAllSub { get; set; }
    }

    public class SysRoleProgSub
    {
        public int RoleID { get; set; }
        public int ProgID { get; set; }
        public int SubID { get; set; }
    }

    public class SysRoleUser
    {
        public int RoleID { get; set; }
        public int UserID { get; set; }
    }

    public class SysSetting : BaseObj
    {
        public string Value { get; set; }
        public string Type { get; set; }
        public string Memo { get; set; }
    }

    public class ExeResult
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public void Error(string msg = null) { Success = false; Message = msg ?? DefaultMessage; }
        public void Reset() { Success = true; Message = string.Empty; ; }
        public string ErrorID { get; set; } = string.Empty;

        private static readonly string DefaultMessage = "發生不可預期錯誤"; 
    }

    public class UIMenuMain
    {
        public string Name { get; set; }
        public int Seq { get; set; }
        public List<UIMenuItem> Items { get; set; }
    }

    public class UIMenuItem
    {
        public int ID { get; set; }
        public string Dir { get; set; }
        public string Name { get; set; }
        public int Seq { get; set; }
        public bool Open { get; set; }//另開視窗
    }

    public class FileLog
    {
        readonly List<LogData> List = new List<LogData>();

        public void AddLog(string sLog)
        {
            List.Add(new LogData { Log = sLog });
        }

        public override string ToString()
        {
            return string.Join(System.Environment.NewLine, List.Select(p => $"{p.Time:HH:mm:ss.fff} {p.Log}"));
        }

        class LogData
        {
            public DateTime Time { get; set; } = DateTime.Now;
            public string Log { get; set; }
        }
    }

    public interface ISysCache
    {
        void Clear();
    }

    public class SysCacheObj<T> : ISysCache
    {
        static object gLock = new object();
        object oLock = new object();
        List<T> _List = null;
        string sSQL { get; set; }
        object oSQL { get; set; } = null;

        public SysCacheObj(string s = "", object o = null)
        {
            sSQL = s; oSQL = o;
        }

        public List<T> List
        {
            get
            {
                if (_List == null) { Init(); }
                return _List;
            }
        }

        public void Init(cyc.DB.SqlDapperConn dDB = null, bool IsReset = false)
        {
            lock (oLock)
            {
                if (IsReset && _List != null) { Clear(); _List = null; }
                if (_List == null) { GetList(); }
            }
            void GetList()
            {
                bool IsNew = dDB == null;
                if (IsNew) { dDB = new DB.SqlDapperConn(); }

                var list = dDB.QueryList<T>(sSQL, oSQL);
                if (list != null)
                {
                    _List = list.ToList();
                    Global.AddSysCache(this, typeof(T).Name);
                }
                
                if (IsNew) { dDB.Dispose(); }
            }
        }

        public void Clear()
        {
            if (_List != null) { _List.Clear(); _List = null; }
        }
    }

    public static class Shared
    {
        public static System.Data.DataTable ObjToDataTable<T>(List<T> items)
        {
            System.Data.DataTable dataTable = new System.Data.DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, prop.PropertyType.BaseType);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        public static void CopyObjectValues<T>(T From, T To)
        {
            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < Props.Length; i++)
                Props[i].SetValue(To, Props[i].GetValue(From, null), null);
        }

        public static int GetInt(string sValue) { if (int.TryParse(sValue, out int iValue)) return iValue; else return 0; }
        //public static string SetInt(int sValue) { if (int.TryParse(sValue, out int iValue)) return iValue; else return 0; }

        public static DateTime? GetDate(string sValue) { if (DateTime.TryParse(sValue, out DateTime dValue)) return dValue; else return null; }
        public static TimeSpan? GetTime(string sValue) { if (TimeSpan.TryParse(sValue, out TimeSpan tValue)) return tValue; else return null; }

        public static string SetDate(DateTime? dValue, string sFormat = Format.DateFormat) { if (dValue != null) return ((DateTime)dValue).ToString(sFormat); else return string.Empty; }
        public static string SetTime(TimeSpan? tValue, string sFormat = Format.TimeSpanFormat) { if (tValue != null) return ((TimeSpan)tValue).ToString(sFormat); else return string.Empty; }
        public static string SetDateTime(DateTime? dValue, string sFormat = Format.DateTimeFormat) { if (dValue != null) return ((DateTime)dValue).ToString(sFormat); else return string.Empty; }
    }

    public static class Format
    {
        public const string DateFormat = "yyyy-MM-dd";
        public const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        public const string TimeSpanFormat = @"hh\:mm";
    }
}
