using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cyc.Data;
using Quartz;

namespace cyc.Auto
{
    public static class Manager
    {
        static IScheduler Scheduler { get; } = new Quartz.Impl.StdSchedulerFactory().GetScheduler();
        static List<JobData> _List { get; } = new List<JobData>();

        public static void Create<T>(string sKey, string sName, string sCron) where T : AutoJob
        {
            if (!_List.Any(p => p.Key == sKey))
            {
                Scheduler.ScheduleJob(JobBuilder.Create<T>().WithIdentity(sKey).Build(), TriggerBuilder.Create().WithCronSchedule(sCron).WithIdentity(sKey + "Trigger").Build());
                _List.Add(new JobData { Key = sKey, Name = sName });
            }
        }
        public static void Start()
        {
            if (Scheduler != null && !Scheduler.IsStarted) Scheduler.Start();

            //cyc.Global.ApplicationEnd += Stop; //註冊 Application_End 委派事件
        }

        public static void Stop()
        {
            if (Scheduler != null && !Scheduler.IsShutdown) Scheduler.Shutdown(true);
        }
        public static void Update(string sKey, cyc.Data.ExeResult oResult = null)
        {
            var oJob = _List.FirstOrDefault(p => p.Key == sKey);
            if (oJob != null)
            {
                if (oResult == null)
                    oJob.LastCall = DateTime.Now;
                else
                {
                    oJob.LastExec = DateTime.Now;
                    oJob.Success = oResult.Success;
                    oJob.Message = oResult.Message;
                }
            }
        }
        public static List<JobData> List()
        {
            return _List;
        }


        static List<string> RunningList { get; } = new List<string>();
        static object Lock { get; } = new object();
        public static bool GetExclusive(string Key)//取得獨佔執行
        {
            lock (Lock)
            {
                if (!RunningList.Contains(Key))
                {
                    RunningList.Add(Key);
                    Update(Key);
                    return true;
                }
                return false;
            }
        }
        public static void CloseExclusive(string Key, ExeResult Result = null)//釋放獨佔執行
        {
            lock (Lock)
            {
                RunningList.Remove(Key);
                if (Result != null) Update(Key, Result);
            }
        }
    }

    public class JobData
    {
        public string Key { get; set; }
        public string Name { get; set; }
        //public string Cron { get; set; }
        public DateTime? LastCall { get; set; }
        public DateTime? LastExec { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public abstract class AutoJob : IJob
    {
        protected cyc.Data.ExeResult oResult = new cyc.Data.ExeResult();
        protected abstract void Run();

        void IJob.Execute(IJobExecutionContext context)
        {
            Run();
        }
    }

    public class KeepAlive : cyc.Auto.AutoJob
    {
        public const string JobKey = "KeepAliveURL";
        public const string JobName = "KeepAlive";
        protected override void Run()
        {
            if (cyc.Auto.Manager.GetExclusive(JobKey))
            {
                try
                {
                    var sURL = cyc.Shared.SysQuery.GetAppSettingValue(JobKey);
                    if (!string.IsNullOrWhiteSpace(sURL))
                    {
                        System.Net.HttpWebRequest oRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(sURL);
                        using (System.Net.HttpWebResponse oResponse = (System.Net.HttpWebResponse)oRequest.GetResponse())
                        {
                            oResponse.Close();
                        }
                    }
                }
                catch (Exception ex) { oResult.Error(ex.Message); }
                finally { cyc.Auto.Manager.CloseExclusive(JobKey, oResult); }
            }
        }
    }
}
