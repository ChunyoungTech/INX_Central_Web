using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace CYCloud.ChkProj
{
    public class AutoCheckProject : cyc.Auto.AutoJob //IJob
    {
        public readonly static string JobKey = "DoCheckProject";
        public readonly static string JobName = "資料點即時檢核+產生MAPP";

        protected override void Run()
        {
            if (cyc.Auto.Manager.GetExclusive(JobKey))
            {
                try
                {
                    using (var oExec = new CheckExec())
                    {
                        oExec.DoCheck();
                    }
                }
                catch (Exception ex)
                {
                    cyc.Log.WriteSysErrorLog(ex.Message + ":" + ex.StackTrace);
                    oResult.Error(ex.Message);
                }
                finally { cyc.Auto.Manager.CloseExclusive(JobKey, oResult); }
            }
        }
    }

    public class CheckExec : IDisposable
    {
        cyc.DB.SqlDapperConn oDB;
        static DateTime dDate = DateTime.Now;

        public void DoCheck()
        {
            oDB = new cyc.DB.SqlDapperConn();
            dDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
            try
            {
                var pList = oDB.QueryList<CheckProject>("select * from CheckProjects where CP_STOP_FLAG=@False", new { False = false });
                if (pList != null)
                {
                    foreach (var proj in pList.Where(p => p.CP_TYPE == 1 || (p.CP_TYPE == 2 && dDate.Second < 30 && p.CP_CHECK_TIME.Split(';').Contains(dDate.ToString("HH:mm")))))
                        DoProjCheck(proj);
                }
            }
            catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex.Message + ":" + ex.StackTrace); }
            finally { oDB.Dispose(); }
        }

        private void DoProjCheck(CheckProject proj)
        {
            bool ToSendOK = (proj.CP_TYPE == 2 && (proj.CP_OK_MSG ?? "").Trim().Length > 0);

            List<TagStatusChange> cList = new List<TagStatusChange>();

            CYCloud.Mapp.Data.MappMessage mapp = null;
            CheckProjectMappLog log = null;

            var tList = oDB.Connection.Query<CheckProjectTag>(@"
select A.*,B.Tag_Name,B.Unit,B.HiHi_Limit,B.Hi_Limit,B.Lo_Limit,B.LoLo_Limit,B.Tag_Desc,C.Tag_Value
from CheckProjectTags A inner join TagData B on A.CT_TAG_ID=B.ID
left join LiveValues C on B.Tag_Name=C.TAG_NAME
where A.CT_CHECK_PROJECT_ID=@PID", new { PID = proj.CP_SEQ_ID }).ToList();

            foreach (var tag in tList)
            {
                int iCheck = 0;
                if (decimal.TryParse(tag.Tag_Value, out decimal value))
                {
                    if (tag.LoLo_Limit != null && value < tag.LoLo_Limit && proj.CP_STOP_LOLO) { iCheck = 1; }
                    if (tag.HiHi_Limit != null && value > tag.HiHi_Limit && proj.CP_STOP_HIHI) { iCheck = 4; }

                    if (iCheck == 0 && tag.Lo_Limit != null && value < tag.Lo_Limit && proj.CP_STOP_LO) { iCheck = 2; }
                    if (iCheck == 0 && tag.Hi_Limit != null && value > tag.Hi_Limit && proj.CP_STOP_HI) { iCheck = 3; }
                }

                char inAlert = iCheck == 0 ? 'N' : 'Y';
                DateTime? alertTime = dDate;
                if (iCheck == 0) { alertTime = null; }

                if (inAlert == 'Y')
                {
                    if (proj.CP_TYPE == 1)//即時發送
                    {
                        //延遲發送
                        if (proj.CP_DELAY_SEND_TIME != null)
                        {
                            if (tag.CT_ALARM_TIME != null)
                            {
                                if (dDate.AddMinutes(-(int)proj.CP_DELAY_SEND_TIME) >= (DateTime)tag.CT_ALARM_TIME)
                                {
                                    //AddNewMappMessage(proj, tag, iCheck, ref mList, ref lList);
                                    AddOneMappMessage(proj, tag, iCheck, ref mapp, ref log);
                                    inAlert = 'N';
                                    alertTime = null;
                                }
                                else
                                {
                                    alertTime = tag.CT_ALARM_TIME;
                                }
                            }
                        }
                        //即時發送
                        else
                        {
                            if (inAlert == 'Y' && tag.CT_IN_ALARM == 'N')//當狀態 'N'=>'Y'，發送
                            {
                                //AddNewMappMessage(proj, tag, iCheck, ref mList, ref lList);
                                AddOneMappMessage(proj, tag, iCheck, ref mapp, ref log);
                            }
                        }
                        //重覆發送
                        if (proj.CP_RESEND_TIME != null)
                        {
                            if (tag.CT_ALARM_TIME != null)
                            {
                                if (dDate.AddMinutes(-(int)proj.CP_RESEND_TIME) >= (DateTime)tag.CT_ALARM_TIME)
                                {
                                    //AddNewMappMessage(proj, tag, iCheck, ref mList, ref lList);
                                    AddOneMappMessage(proj, tag, iCheck, ref mapp, ref log);
                                }
                                else
                                {
                                    alertTime = tag.CT_ALARM_TIME;
                                }
                            }
                            else
                            {
                                //AddNewMappMessage(proj, tag, iCheck, ref mList, ref lList);
                                AddOneMappMessage(proj, tag, iCheck, ref mapp, ref log);
                            }
                        }
                    }
                    else//定時發送
                    {
                        ToSendOK = false;
                        AddOneMappMessage(proj, tag, iCheck, ref mapp, ref log);
                    }
                }

                if (tag.CT_IN_ALARM != inAlert || tag.CT_ALARM_TIME != alertTime)
                    oDB.Connection.Execute("update CheckProjectTags set CT_IN_ALARM=@Alert,CT_ALARM_TIME=@Time where CT_SEQ_ID=@ID", new { ID = tag.CT_SEQ_ID, Alert = inAlert, Time = alertTime });

                if (mapp != null && log != null)
                {
                    log.MM_SEQ_ID = oDB.Connection.Query<int>(@"
                    insert into MappMessage (MS_SYS_NAME,MM_TYPE,MM_CONTENT_TYPE,MM_TEXT_CONTENT,MM_SUBJECT) 
                    values (@MS_SYS_NAME,@MM_TYPE,@MM_CONTENT_TYPE,@MM_TEXT_CONTENT,@MM_SUBJECT);SELECT CAST(SCOPE_IDENTITY() as int)", mapp).Single();

                    oDB.Connection.Execute(@"
                    insert into CheckProjectMappLog (CM_CHECK_PROJECT_ID,CM_TAG_ID,CM_SEND_TIME,CM_HIHI,CM_HI,CM_LO,CM_LOLO,CM_TAG_VALUE,CM_PROVIDER,CM_DESC,MM_SEQ_ID) 
                    values (@CM_CHECK_PROJECT_ID,@CM_TAG_ID,@CM_SEND_TIME,@CM_HIHI,@CM_HI,@CM_LO,@CM_LOLO,@CM_TAG_VALUE,@CM_PROVIDER,@CM_DESC,@MM_SEQ_ID)", log);
                }
            }

            tList.Clear();

            //發送OK訊息(定時發送)
            if (ToSendOK)
            {
                var iID = oDB.Connection.Query<int>(@"
                    insert into MappMessage (MS_SYS_NAME,MM_TYPE,MM_CONTENT_TYPE,MM_TEXT_CONTENT,MM_SUBJECT) 
                    values (@MS_SYS_NAME,@MM_TYPE,@MM_CONTENT_TYPE,@MM_TEXT_CONTENT,@MM_SUBJECT);SELECT CAST(SCOPE_IDENTITY() as int)",
                    new CYCloud.Mapp.Data.MappMessage()
                    {
                        MS_SYS_NAME = proj.CP_MAPP_TYPE,
                        MM_CONTENT_TYPE = 1,
                        MM_TYPE = 'A',
                        MM_TEXT_CONTENT = proj.CP_OK_MSG,
                        MM_SUBJECT = proj.CP_NAME
                    }).Single();

                oDB.Connection.Execute(@"
                    insert into CheckProjectMappLog (CM_CHECK_PROJECT_ID,CM_TAG_ID,CM_SEND_TIME,CM_HIHI,CM_HI,CM_LO,CM_LOLO,CM_TAG_VALUE,CM_PROVIDER,CM_DESC,MM_SEQ_ID) 
                    values (@CM_CHECK_PROJECT_ID,@CM_TAG_ID,@CM_SEND_TIME,@CM_HIHI,@CM_HI,@CM_LO,@CM_LOLO,@CM_TAG_VALUE,@CM_PROVIDER,@CM_DESC,@MM_SEQ_ID)",
                    new CheckProjectMappLog()
                    {
                        CM_CHECK_PROJECT_ID = proj.CP_SEQ_ID,
                        CM_SEND_TIME = dDate,
                        CM_PROVIDER = proj.CP_MAPP_PROVIDER,
                        CM_DESC = proj.CP_OK_MSG,
                        MM_SEQ_ID = iID
                    });
            }

        }

        private static void AddOneMappMessage(CheckProject proj, CheckProjectTag tag, int iCheck, ref CYCloud.Mapp.Data.MappMessage mapp, ref CheckProjectMappLog log)
        {
            string sContent = string.Format(GetMappContent(proj, iCheck)
                        , tag.Tag_Name
                        , dDate.ToString("yyyy/MM/dd HH:mm:ss")
                        , tag.Tag_Value.Substring(0, 10)
                        , tag.HiHi_Limit.ToString()
                        , tag.Hi_Limit.ToString()
                        , tag.Lo_Limit.ToString()
                        , tag.LoLo_Limit.ToString()
                        , dDate.ToString("yyyy/MM/dd")
                        , dDate.ToString("HH:mm:ss")
                        , tag.Tag_Desc
                        );

            mapp = new CYCloud.Mapp.Data.MappMessage()
            {
                MS_SYS_NAME = proj.CP_MAPP_TYPE,
                MM_CONTENT_TYPE = 1,
                MM_TYPE = 'A',
                MM_TEXT_CONTENT = sContent,
                MM_SUBJECT = proj.CP_NAME
            };
            log = new CheckProjectMappLog()
            {
                CM_CHECK_PROJECT_ID = proj.CP_SEQ_ID,
                CM_TAG_ID = tag.CT_TAG_ID,
                CM_SEND_TIME = dDate,
                CM_HIHI = tag.HiHi_Limit?.ToString(),
                CM_HI = tag.Hi_Limit?.ToString(),
                CM_LO = tag.Lo_Limit?.ToString(),
                CM_LOLO = tag.LoLo_Limit?.ToString(),
                CM_TAG_VALUE = tag.Tag_Value.ToString(),
                CM_PROVIDER = proj.CP_MAPP_PROVIDER,
                CM_DESC = sContent
            };
        }

        private static string GetMappContent(CheckProject proj, int iCheck)
        {
            //string[] strArray = { "{TAG名稱}", "{時間點}", "{檢核值}", "{HIHI管制值}", "{HI管制值}", "{LO管制值}", "{LOLO管制值}" };
            //string strAlert = "{TAG名稱} 於 {時間點} 檢測值為 {檢核值} ，超過管制值";
            int intType = 0;
            string strAlert = "", strType = "";
            switch (iCheck)
            {
                case 1:
                    if ((proj.CP_LOLO_ALARM_MSG ?? "").Trim().Length > 0) { strAlert = proj.CP_LOLO_ALARM_MSG.Trim(); }
                    strType = "LOLO管制值"; intType = 6;
                    break;
                case 2:
                    if ((proj.CP_LO_ALARM_MSG ?? "").Trim().Length > 0) { strAlert = proj.CP_LO_ALARM_MSG.Trim(); }
                    strType = "LO管制值"; intType = 5;
                    break;
                case 3:
                    if ((proj.CP_HI_ALARM_MSG ?? "").Trim().Length > 0) { strAlert = proj.CP_HI_ALARM_MSG.Trim(); }
                    strType = "HI管制值"; intType = 4;
                    break;
                case 4:
                    if ((proj.CP_HIHI_ALARM_MSG ?? "").Trim().Length > 0) { strAlert = proj.CP_HIHI_ALARM_MSG.Trim(); }
                    strType = "HIHI管制值"; intType = 3;
                    break;
            }

            if (strAlert.Length == 0) { strAlert = "{0} 於 {1} 檢測值為 {2} ，超過" + strType + "{" + intType.ToString() + "}"; }
            //for (int idx = 0; idx < strArray.Length; idx++)
            //    strAlert = strAlert.Replace(strArray[idx], "{" + idx.ToString() + "}");

            return strAlert;
        }

        public void Dispose()
        {
            oDB.Dispose();
        }

        class TagStatusChange
        {
            public int ID { get; set; }
            public char Alert { get; set; }
            public DateTime? Time { get; set; }
        }

    }

    #region 類別定義

    public class CheckProject
    {
        public int CP_SEQ_ID { get; set; }
        public string CP_NAME { get; set; }
        public int CP_TYPE { get; set; }
        //public TimeSpan? CP_CHECK_TIME { get; set; }
        public string CP_CHECK_TIME { get; set; }
        public TimeSpan? CP_DATA_TIME { get; set; }
        public string CP_MAPP_PROVIDER { get; set; }
        public string CP_MAPP_TYPE { get; set; }
        public bool CP_STOP_FLAG { get; set; }
        public bool CP_STOP_HIHI { get; set; }
        public bool CP_STOP_HI { get; set; }
        public bool CP_STOP_LO { get; set; }
        public bool CP_STOP_LOLO { get; set; }
        //public bool CP_IS_SEND_OK_MSG { get; set; }
        public string CP_OK_MSG { get; set; }
        public string CP_ALARM_MSG { get; set; }
        public DateTime UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        //public int? CP_EXPORT_HOUR { get; set; }
        public int? CP_RESEND_TIME { get; set; }
        public int? CP_DELAY_SEND_TIME { get; set; }
        public string CP_LOLO_ALARM_MSG { get; set; }
        public string CP_LO_ALARM_MSG { get; set; }
        public string CP_HI_ALARM_MSG { get; set; }
        public string CP_HIHI_ALARM_MSG { get; set; }
        public int CP_DEPT_ID { get; set; }
    }

    public class CheckProjectTag
    {
        public int CT_SEQ_ID { get; set; }
        public int CT_CHECK_PROJECT_ID { get; set; }
        public int CT_TAG_ID { get; set; }
        public char CT_IN_ALARM { get; set; }
        public DateTime? CT_ALARM_TIME { get; set; }
        public string Tag_Name { get; set; }
        public string Unit { get; set; }
        public decimal? HiHi_Limit { get; set; }
        public decimal? Hi_Limit { get; set; }
        public decimal? Lo_Limit { get; set; }
        public decimal? LoLo_Limit { get; set; }
        public string Tag_Value { get; set; }
        public string Tag_Desc { get; set; }
    }

    public class CheckProjectMappLog
    {
        public int CM_SEQ_ID { get; set; }
        public int CM_CHECK_PROJECT_ID { get; set; }
        public int CM_TAG_ID { get; set; }
        public DateTime CM_SEND_TIME { get; set; }
        public string CM_HIHI { get; set; }
        public string CM_HI { get; set; }
        public string CM_LO { get; set; }
        public string CM_LOLO { get; set; }
        public string CM_TAG_VALUE { get; set; }
        public string CM_PROVIDER { get; set; }
        public string CM_DESC { get; set; }
        public string CM_TYPE { get; set; }
        public int MM_SEQ_ID { get; set; }
    }

    public class LiveValue
    {
        public string TAG_NAME { get; set; }
        public string TAG_VALUE { get; set; }
    }

    #endregion
}
