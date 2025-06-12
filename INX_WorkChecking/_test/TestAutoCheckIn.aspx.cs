using cyc.Data;
using CYCloud.WorkCheck;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._test
{
    public partial class TestAutoCheckIn : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                TextBox1.Text = DateTime.Today.ToString("yyyy/M/dd");
                TextBox2.Text = DateTime.Today.ToString("yyyy/M/dd");
                TextBox3.Text = "2024/03/11";
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            if (DateTime.TryParse(TextBox1.Text, out DateTime dt))
            {
                CYCloud.WorkCheck.AutoCheckInOut.AutoCheckIn(dt);
            }
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            if (DateTime.TryParse(TextBox1.Text, out DateTime dt))
            {
                CYCloud.WorkCheck.AutoCheckInOut.AutoCheckOut(dt);
            }
        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            string sFile = $"{cyc.Global.AppBasePath}\\_upload\\WorkCheckHourMapp.xlsx";

            if (DateTime.TryParse(TextBox2.Text, out DateTime TimeExe))
            {
                DateTime TimeS = TimeExe.AddHours(-1);
                DateTime TimeE = TimeS.AddHours(1);

                using (var oDB = new cyc.DB.SqlDapperConn(null, null, false, 60))
                {
                    IEnumerable<WorkCheckHourData> cList = oDB.QueryList<WorkCheckHourData>($@"
                    with VMT as (
                    	select A.fac_code as Fac,A.con_number as ConNumber,A.main_area as Area,A.vendor_name as Vendor,A.con_conten as ConContent
                    	,replace(SUBSTRING(A.engineer,0,5),'(','') as UserName,A.vendor_pe as VendorMain,A.END_TIME
                    	from View_VMT_FAC A where con_date=@Date and (fac_code='FAC8' or fac_code='FAC6')
                    ),CHKALL as (
                    	select A.ConNumber,C.CHECK_TYPE as CheckType,D.FRUserID,D.FRUserName,D.LogDateTime
                    	from VMT A
                    	inner join WORK_CHECKIN B on A.ConNumber=B.con_number
                    	inner join WORK_CHECKIN_LOG C on B.SEQ_ID=C.WORK_CHECKIN_ID
                    	inner join RecognitionAuth D on C.IFP_RecognitionAuth_ID=D.ID
                    )

                    select C.ConNumber,C.Fac,C.Vendor,C.Area,C.ConContent,C.UserName,C.VendorMain,A.FRUserName,A.LogDateTime as CheckInTime,B.LogDateTime as CheckOutTime,E.Name as DeptName,F.MappName,C.END_TIME
                    from CHKALL A left join CHKALL B on B.CheckType='CHECKOUT' and A.ConNumber=B.ConNumber and A.FRUserID=B.FRUserID
                    inner join VMT C on A.ConNumber=C.ConNumber
                    inner join SysUser D on C.UserName=D.Name
                    inner join SysDept E on D.DeptID=E.ID
                    inner join WorkCheckMappSetting F on E.ID=F.DeptID
                    where A.CheckType='CHECKIN' and F.IsEnabled=1
                    order by A.ConNumber,A.LogDateTime,B.LogDateTime", new { TimeS.Date });

                    //IEnumerable<WorkCheckHourData> cList = oDB.QueryList<WorkCheckHourData>("select * from WorkCheckMappTestData order by ConNumber", new { TimeS.Date });

                    if (cList != null && cList.Any())
                    {
                        foreach (var gList in cList.GroupBy(p => p.MappName))
                        {
                            var oSetting = CYCloud.Global.MappSetting.List.FirstOrDefault(p => p.MS_SYS_NAME == gList.Key);
                            if (oSetting != null)
                            {
                                //每小時 有報到退 => 發送群組所有工單報到退資料 2024-11-14 修改規則
                                if (gList.Any(p => (p.CheckInTime >= @TimeS && p.CheckInTime < TimeE) || (p.CheckOutTime >= @TimeS && p.CheckOutTime < TimeE)))
                                {
                                    //當日 累計報到退
                                    CreateMapp(gList.Where(p => p.CheckInTime < TimeE || p.CheckOutTime < TimeE), "報到退");
                                }
                                //var hList = gList.Where(p => (p.CheckInTime >= @TimeS && p.CheckInTime < TimeE) || (p.CheckOutTime >= @TimeS && p.CheckOutTime < TimeE)).GroupBy(p => p.ConNumber).Select(p => p.Key);
                                //if (hList.Any())
                                //{
                                //    //當日 累計報到退
                                //    List<WorkCheckHourData> dList = new List<WorkCheckHourData>();
                                //    foreach (var x in hList.OrderBy(p => p))
                                //        dList.AddRange(gList.Where(p => p.ConNumber == x && (p.CheckInTime < TimeE || p.CheckOutTime < TimeE)));

                                //    if (dList.Any())
                                //        CreateMapp(dList, "報到退");
                                //}

                                //當日 未報退
                                var noList = gList.Where(p => p.END_TIME > TimeS && p.END_TIME <= TimeE && p.CheckOutTime == null);
                                if (noList.Any())
                                    CreateMapp(noList, "未報退");

                                //產生檔案及MAPP
                                void CreateMapp(IEnumerable<WorkCheckHourData> xList, string sType)
                                {
                                    var oFile = CYCloud.WorkCheck.Shared.WorkCheckHourFile(xList, sFile, $"群創{(xList.First().Fac == "FAC8" ? "八廠" : "六廠")}施工{sType}表");

                                    string sSubject = $"施工管理{(sType == "報到退" ? $"{TimeS:yyyyMMddHHmm}" : $"{TimeS:yyyyMMdd}")}{sType}通知";

                                    if (oFile != null)
                                    {
                                        var oMapp = new CYCloud.Mapp.Data.MappMessage
                                        {
                                            MS_SYS_NAME = gList.Key,
                                            MM_CONTENT_TYPE = 3,
                                            MM_SUBJECT = sSubject,
                                            MM_TYPE = 'A',
                                            MM_MEDIA_CONTENT = oFile,
                                            MM_ExtFileName = "xlsx",
                                            MM_FILE_SHOW_NAME = $"{sSubject}.xlsx"
                                        };

                                        oDB.Execute(@"
insert Into MappMessage(MS_SYS_NAME,MM_CONTENT_TYPE,MM_SUBJECT,MM_TYPE,MM_MEDIA_CONTENT,MM_ExtFileName,MM_FILE_SHOW_NAME)
values (@MS_SYS_NAME,@MM_CONTENT_TYPE,@MM_SUBJECT,@MM_TYPE,@MM_MEDIA_CONTENT,@MM_ExtFileName,@MM_FILE_SHOW_NAME)", oMapp);

                                        using (FileStream oStream = new FileStream($"{cyc.Global.AppBasePath}\\_upload\\{oMapp.MS_SYS_NAME}_{DateTime.Now:yyyyMMddHHmmss}_{oMapp.MM_FILE_SHOW_NAME}", FileMode.Create, FileAccess.ReadWrite))
                                        {
                                            oStream.Write(oFile, 0, oFile.Length);
                                            oStream.Close();
                                        }
                                    }
                                }
                            }
                        }
                    }

                    //if (cList != null && cList.Any())
                    //{
                    //    string sFile = $"{cyc.Global.AppBasePath}\\_upload\\WorkCheckHourMapp.xlsx";

                    //    var mGroup = cList.GroupBy(p => p.MappName);
                    //    foreach (var m in mGroup)
                    //    {
                    //        var oSetting =  CYCloud.Global.MappSetting.List.FirstOrDefault(p => p.MS_SYS_NAME == m.Key);
                    //        if (oSetting != null)
                    //        {
                    //            var oFile = CYCloud.WorkCheck.Shared.WorkCheckHourFile(m, sFile, "");
                    //            if (oFile != null)
                    //            {
                    //                var mData = new CYCloud.Mapp.Data.MappMessage
                    //                {
                    //                    MS_SYS_NAME = m.Key,
                    //                    MM_CONTENT_TYPE = 3,
                    //                    MM_SUBJECT = $"施工管理簽到退通知{TimeS:yyyy-MM-dd HH:mm}~{TimeE:HH:mm}",
                    //                    MM_TYPE = 'A',
                    //                    MM_MEDIA_CONTENT = oFile,
                    //                    MM_ExtFileName = "xlsx",
                    //                    MM_FILE_SHOW_NAME = $"施工管理簽到退通知-{m.Key}-{TimeS:yyyyMMddHHmm}.xlsx"
                    //                };

                    //                //                                    oDB.Execute(@"
                    //                //insert Into MappMessage(MS_SYS_NAME,MM_CONTENT_TYPE,MM_SUBJECT,MM_TYPE,MM_MEDIA_CONTENT,MM_ExtFileName,MM_FILE_SHOW_NAME)
                    //                //values (@MS_SYS_NAME,@MM_CONTENT_TYPE,@MM_SUBJECT,@MM_TYPE,@MM_MEDIA_CONTENT,@MM_ExtFileName,@MM_FILE_SHOW_NAME)", mData);

                    //                using (FileStream oStream = new FileStream($"{cyc.Global.AppBasePath}\\_upload\\{mData.MM_FILE_SHOW_NAME}", FileMode.Create, FileAccess.ReadWrite))
                    //                {
                    //                    oStream.Write(oFile, 0, oFile.Length);
                    //                    oStream.Close();
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                }
            }
        }

        protected void Button4_Click(object sender, EventArgs e)
        {
            if (DateTime.TryParse(TextBox3.Text, out DateTime Date))
            {
                ExeResult oResult = new ExeResult();
                CYCloud.WorkCheck.WorkCheckReport.WorkCheckReportForMAPP(oResult, Date);
            }
        }

        //protected void Button5_Click(object sender, EventArgs e)
        //{
        //    if (DateTime.TryParse(TextBox3.Text, out DateTime Date))
        //    {
        //        ExeResult oResult = new ExeResult();
        //        //CYCloud.WorkCheck.Shared.WorkCheckReportForFAC(oResult, Date);
        //    }
        //}
    }
}