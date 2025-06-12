using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;

namespace CYCloud.Service
{
    public static class ValueService
    {
        public static class Tokens
        {
            private static object oLock = new object();
            private static List<Token> List = new List<Token>();

            public static void Add(Token oToken)
            {
                if (List == null) { List = new List<Token>(); }

                lock (oLock)
                {
                    List.Add(oToken);
                }
            }

            public static void Clean()
            {
                if (List != null && List.Count > 0)
                {
                    lock (oLock)
                    {
                        foreach (var oItem in List.Where(p => p.ExpireDate < DateTime.Now))
                        {
                            List.Remove(oItem);
                        }
                    }
                }
            }

            public static bool CheckExist(string Guid)
            {
                return List.Any(p => p.Guid == Guid);
            }

            public static bool CheckDuplicate(int ID)
            {
                return List.Any(p => p.ID == ID);
            }
        }

        public static class Tasks
        {
            private static object oLock = new object();
            private static Queue<Task> List = new Queue<Task>();
            private static bool isRunning = false;
            public static int Count { get { if (List != null) return List.Count; else return 0; } }

            public static void Add(Task oTask)
            {
                lock (oLock) { List.Enqueue(oTask); }
            }

            public static Task Get()
            {
                lock (oLock) { if (List.Count > 0) return List.Dequeue(); }
                return null;
            }

            public static void Run()
            {
                if (!isRunning)
                {
                    isRunning = true;

                    try
                    {
                        List<Task> oList = new List<Task>();
                        Task oTask = Get();
                        while (oTask != null)
                        {
                            oList.Add(oTask);
                            oTask = Get();
                        }
                        if (oList.Count > 0) { Execute(oList); }
                    }
                    catch (Exception ex) { pin.gObj.WriteError(ex.Message); }

                    isRunning = false;
                }
            }

            private static void Execute(List<Task> oList)
            {
                var gList = oList.GroupBy(p => p.Date);
                System.Threading.Tasks.Parallel.ForEach(gList, (gItem) =>
                {
                    var xList = new List<DataValue>();
                    foreach (var g in gItem)
                    {
                        xList.AddRange(g.List.Where(p => p.Time.Date == gItem.Key));
                    }
                    xList = xList.GroupBy(p => new { p.ID, p.Time }).Select(p => p.LastOrDefault()).ToList();

                    if (xList.Count > 0)
                    {
                        using (pin.DB.SqlDBConn oDB = new pin.DB.SqlDBConn())
                        {
                            var qList = oDB.oConn.Query<DataValue>(@"
select tag_id as ID,tag_value as Value,value_datetime as Time from TagValues where value_datetime between @DateS and @DateE"
, new { DateS = gItem.Key, DateE = gItem.Key.AddDays(1).AddSeconds(-1) }).ToList();

                            var ins = from lsX in xList
                                      join lsQ in qList on new { lsX.ID, lsX.Time } equals new { lsQ.ID, lsQ.Time } into QQ
                                      from lsQ in QQ
                                      where lsQ.Value == null
                                      select lsX;

                            var upd = from lsX in xList
                                      join lsQ in qList on new { lsX.ID, lsX.Time } equals new { lsQ.ID, lsQ.Time }
                                      where lsX.Value != lsQ.Value
                                      select lsX;

                            if (ins.Count() > 0)
                                oDB.oConn.Execute("insert into TagValues (tag_id,tag_value,value_datetime) values (@ID,@Value,@Time)", ins);
                            if (upd.Count() > 0)
                                oDB.oConn.Execute("update TagValues set tag_value=@Value where tag_id=@ID and value_datetime=@Time", upd);
                        }
                    }
                });
            }

            
        }



        public static class Auths
        {
            private static List<Auth> List;
            public static void Init()
            {
                Clear();
                List = pin.gObj.gDB.oConn.Query<Auth>("select * from TagValueAuth").ToList();
            }
            public static void Clear()
            {
                if (List != null && List.Count > 0) { List.Clear(); }
            }
            public static bool Check(Token oToken)
            {
                if (List != null)
                    return List.Any(p => p.ID == oToken.ID && p.Key == oToken.Key && p.Enabled);
                return false;
            }
        }
    }
   
    #region 類別定義
    public class Auth
    {
        public int ID { get; set; }
        public string Key { get; set; }
        public bool Enabled { get; set; }
    }

    public class Token
    {
        public int ID { get; set; }
        public string Key { get; set; }
        public string Guid { get; set; }
        public DateTime ExpireDate { get; set; }
        public pin.ExeResult Result { get; set; }
    }

    public class Task
    {
        public Token Token { get; set; }
        public DateTime Date { get; set; }
        public List<DataValue> List { get; set; }
    }

    public class DataValue
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public DateTime Time { get; set; }
    }
    #endregion
}
