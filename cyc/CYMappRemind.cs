using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CYCloud
{
    public class CYMappRemind : cyc.Auto.AutoJob //IJob
    {
        //static bool IsRunning { get; set; } = false;
        public static readonly string JobKey = "DoMappDisableRemind";
        public static readonly string JobName = "MAPP未解隔通知";
        protected override void Run()
        {
            if (cyc.Auto.Manager.GetExclusive(JobKey))
            {
                try
                {
                    string sMins = cyc.Shared.SysQuery.GetSysSettingValue("MappDisableRemind");
                    DateTime nDate = DateTime.Now;
                    int iMins = 60;//預設60分鐘
                    if (!int.TryParse(sMins, out iMins) || iMins <= 0)
                        iMins = 60;

                    IEnumerable<DisableRemind> dList = null;
                    using (var oDB = new cyc.DB.SqlDapperConn(oResult))
                    {
                        dList = oDB.QueryList<DisableRemind>(@"
select B.MS_SYS_NAME,A.MD_DATE_END,A.MD_SEQ_ID,ISNULL(A.MD_LAST_REMIND,MD_DATE_END)as MD_LAST_REMIND
,case when A.MD_REMIND_MIN>0 then A.MD_REMIND_MIN else @Mins end as MD_REMIND_MIN
,C.MS_SYS_NAME as MD_REMIND_SETTING
from MappDisable A inner join MappSetting B on A.MS_SEQ_ID=B.MS_SEQ_ID
inner join MappSetting C on A.MD_REMIND_SETTING=C.MS_SEQ_ID
where A.MD_STOP_TIME is null and A.MD_DATE_END<getdate() and A.MD_REMIND_SETTING<>0", new { Mins = iMins });
                    }
                    if (dList != null && dList.Count() > 0)
                    {
                        var qList = dList.Where(p => p.MD_LAST_REMIND.AddMinutes(p.MD_REMIND_MIN) < nDate).ToList();
                        if (qList.Count > 0)
                        {
                            foreach (var q in qList)
                            {
                                q.MM_TEXT_CONTENT = string.Format("[{0}]隔離結束時間{1}，尚未解除隔離", q.MS_SYS_NAME, q.MD_DATE_END);
                                q.MM_subject = "MAPP隔離到期，未解隔通知";
                                q.MD_LAST_REMIND = nDate;
                            }

                            using (var dDB = new cyc.DB.SqlDapperConn(oResult, null, true))
                            {
                                //dDB.Execute("insert into MappMessage (MS_SYS_NAME,MM_CONTENT_TYPE,MM_TEXT_CONTENT,MM_subject,MM_TYPE) values (@MS_SYS_NAME,1,@MM_TEXT_CONTENT,@MM_subject,'A')", qList);
                                dDB.Execute("insert into MappMessage (MS_SYS_NAME,MM_CONTENT_TYPE,MM_TEXT_CONTENT,MM_subject,MM_TYPE) values (@MD_REMIND_SETTING,1,@MM_TEXT_CONTENT,@MM_subject,'A')", qList);
                                if (oResult.Success) { dDB.Execute("update MappDisable set MD_LAST_REMIND=@MD_LAST_REMIND where MD_SEQ_ID=@MD_SEQ_ID", qList); }

                                dDB.ResultTransaction();
                            }
                        }
                    }
                }
                catch (Exception ex) { cyc.Log.WriteSysErrorLog($"{JobName}:{ex.Message}"); oResult.Error(ex.Message); }
                finally { cyc.Auto.Manager.CloseExclusive(JobKey, oResult); }
            }

            #region OLD
            //            cyc.Auto.Manager.Update("MappDisableRemind");
            //            if (!IsRunning)
            //            {
            //                IsRunning = true;
            //                cyc.Data.ExeResult oResult = new cyc.Data.ExeResult();
            //                try
            //                {
            //                    string sMins = cyc.Shared.SysQuery.GetSysSettingValue("MappDisableRemind");
            //                    DateTime nDate = DateTime.Now;
            //                    int iMins = 60;//預設60分鐘
            //                    if (!int.TryParse(sMins, out iMins) || iMins <= 0)
            //                        iMins = 60;

            //                    IEnumerable<DisableRemind> dList = null;
            //                    using (var oDB = new cyc.DB.SqlDapperConn(oResult))
            //                    {
            //                        dList = oDB.QueryList<DisableRemind>(@"
            //select B.MS_SYS_NAME,A.MD_DATE_END,A.MD_SEQ_ID,ISNULL(A.MD_LAST_REMIND,MD_DATE_END)as MD_LAST_REMIND
            //,case when A.MD_REMIND_MIN>0 then A.MD_REMIND_MIN else @Mins end as MD_REMIND_MIN
            //,C.MS_SYS_NAME as MD_REMIND_SETTING
            //from MappDisable A inner join MappSetting B on A.MS_SEQ_ID=B.MS_SEQ_ID
            //inner join MappSetting C on A.MD_REMIND_SETTING=C.MS_SEQ_ID
            //where A.MD_STOP_TIME is null and A.MD_DATE_END<getdate() and A.MD_REMIND_SETTING<>0", new { Mins = iMins });
            //                    }
            //                    if (dList != null && dList.Count() > 0)
            //                    {
            //                        var qList = dList.Where(p => p.MD_LAST_REMIND.AddMinutes(p.MD_REMIND_MIN) < nDate).ToList();
            //                        if (qList.Count > 0)
            //                        {
            //                            foreach (var q in qList)
            //                            {
            //                                q.MM_TEXT_CONTENT = string.Format("[{0}]隔離結束時間{1}，尚未解除隔離", q.MS_SYS_NAME, q.MD_DATE_END);
            //                                q.MM_subject = "MAPP隔離到期，未解隔通知";
            //                                q.MD_LAST_REMIND = nDate;
            //                            }

            //                            using (var dDB = new cyc.DB.SqlDapperConn(oResult, null, true))
            //                            {
            //                                //dDB.Execute("insert into MappMessage (MS_SYS_NAME,MM_CONTENT_TYPE,MM_TEXT_CONTENT,MM_subject,MM_TYPE) values (@MS_SYS_NAME,1,@MM_TEXT_CONTENT,@MM_subject,'A')", qList);
            //                                dDB.Execute("insert into MappMessage (MS_SYS_NAME,MM_CONTENT_TYPE,MM_TEXT_CONTENT,MM_subject,MM_TYPE) values (@MD_REMIND_SETTING,1,@MM_TEXT_CONTENT,@MM_subject,'A')", qList);
            //                                if (oResult.Success) { dDB.Execute("update MappDisable set MD_LAST_REMIND=@MD_LAST_REMIND where MD_SEQ_ID=@MD_SEQ_ID", qList); }

            //                                dDB.ResultTransaction();
            //                            }
            //                        }
            //                    }
            //                }
            //                catch (Exception ex) { cyc.Log.WriteSysErrorLog("Mapp隔離到期通知" + ex.Message); oResult.Error(ex.Message); }
            //                cyc.Auto.Manager.Update("MappDisableRemind", oResult);
            //                IsRunning = false;
            //            }
            #endregion
        }

        class DisableRemind
        {
            public string MS_SYS_NAME { get; set; } //被隔離群組MS_SYS_NAME
            public string MM_TEXT_CONTENT { get; set; }
            public string MM_subject { get; set; }
            public DateTime MD_DATE_END { get; set; }
            public int MD_SEQ_ID { get; set; }
            public DateTime MD_LAST_REMIND { get; set; } //最近一次通知時間
            public int MD_REMIND_MIN { get; set; } //通知頻率
            public string MD_REMIND_SETTING { get; set; } //逾時未解隔通知群組MS_SYS_NAME
        }
    }
}
