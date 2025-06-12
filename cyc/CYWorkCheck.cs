using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using cyc.DB;
using System.Xml.Linq;
using NPOI.Util;
using cyc.Data;
using System.Data.OleDb;
using NPOI.SS.Formula.Atp;
using System.Runtime.InteropServices;
using System.Collections;
using System.Web.UI;
using NPOI.SS.Formula.Functions;
using System.Web.UI.WebControls;
using static CYCloud.WorkCheck.WorkCheckReport;

namespace CYCloud.WorkCheck
{
    //警報報表自動發送
    public class AutoWorkCheckReport : cyc.Auto.AutoJob
    {
        public const string JobKey = "DoWorkingReport";
        public const string JobName = "總廠+各廠施工管理統計表發送";

        protected override void Run()
        {
            if (cyc.Auto.Manager.GetExclusive(JobKey))
            {
                try
                {
                    WorkCheckReport.WorkCheckReportForMAPP(oResult); //各廠+總廠
                }
                catch (Exception ex) { cyc.Log.WriteSysErrorLog($"{JobName}:{ex.Message}"); oResult.Error(ex.Message); }
                finally { cyc.Auto.Manager.CloseExclusive(JobKey, oResult); }
            }
        }
    }

    //每小時發送施工管理簽到退MAPP
    public class WorkCheckHourMapp : cyc.Auto.AutoJob
    {
        public const string JobKey = "DoWorkCheckHourMapp";
        public const string JobName = "每小時發送施工管理簽到退MAPP";

        List<LogData> LogList = new List<LogData>();

        protected override void Run()
        {
            if (cyc.Auto.Manager.GetExclusive(JobKey))
            {
                DateTime TimeNow = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd HH:00")).AddHours(-1);
                try
                {
                    LogList.Add(new LogData($"[{JobName}]-啟動"));
                    DoMappHourReport(TimeNow);
                }
                catch (Exception ex) { cyc.Log.WriteSysErrorLog($"{JobName}(小時):{ex.Message}"); oResult.Error(ex.Message); }
                finally 
                { 
                    cyc.Auto.Manager.CloseExclusive(JobKey, oResult);
                    if (LogList.Count > 0)
                        cyc.Log.WriteFileLog(string.Join(System.Environment.NewLine, LogList.Select(p => $"{p.Time:HH:mm:ss} {p.Log}")), JobName);
                }
            }
        }

        private void DoMappHourReport(DateTime TimeExe)
        {
            string sFile = $"{cyc.Global.AppBasePath}\\_upload\\WorkCheckHourMapp.xlsx";
            if (File.Exists(sFile))
            {
                DateTime TimeS = TimeExe;
                DateTime TimeE = TimeS.AddHours(1);

                using (var oDB = new cyc.DB.SqlDapperConn(oResult, null, false, 60))
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

                    if (!oResult.Success)
                        LogList.Add(new LogData($"查詢簽到退資料發生錯誤：{oResult.Message}"));
                    else if (cList != null)
                        LogList.Add(new LogData($"查詢簽到退資料：{cList.Count()}筆"));

                    if (cList != null && cList.Any())
                    {
                        foreach (var gList in cList.GroupBy(p => p.MappName))
                        {
                            if (Global.MappSetting.List.FirstOrDefault(p => p.MS_SYS_NAME == gList.Key) != null)
                            {
                                LogList.Add(new LogData($"處理MAPP-[{gList.Key}]"));

                                //每小時 有報到退 => 發送群組所有工單報到退資料 2024-11-14 修改規則
                                if (gList.Any(p => (p.CheckInTime >= @TimeS && p.CheckInTime < TimeE) || (p.CheckOutTime >= @TimeS && p.CheckOutTime < TimeE)))
                                {
                                    //當日 累計報到退
                                    CreateMapp(gList.Where(p => p.CheckInTime < TimeE || p.CheckOutTime < TimeE), "報到退");
                                }

                                //當日 未報退
                                var noList = gList.Where(p => p.END_TIME > TimeS && p.END_TIME <= TimeE && p.CheckOutTime == null);
                                if (noList.Any()) { CreateMapp(noList, "未報退"); }

                                //產製檔案及寫入MappMessage
                                void CreateMapp(IEnumerable<WorkCheckHourData> xList, string sType)
                                {
                                    var oFile = Shared.WorkCheckHourFile(xList, sFile, $"群創{(xList.First().Fac == "FAC8" ? "八廠" : "六廠")}施工{sType}表");

                                    string sSubject = $"施工管理{(sType == "報到退" ? $"{TimeS:yyyyMMddHHmm}" : $"{TimeS:yyyyMMdd}")}{sType}通知";

                                    if (oFile != null)
                                    {
                                        try
                                        {
                                            using (FileStream oStream = new FileStream($"{cyc.Global.AppBasePath}\\_logFile\\WorkCheck\\{gList.Key}_{sSubject}.xlsx", FileMode.Create, FileAccess.ReadWrite))
                                            {
                                                oStream.Write(oFile, 0, oFile.Length);
                                                oStream.Close();
                                            }
                                        }
                                        catch (Exception ex) { oResult.Error(ex.Message); }
                                        LogList.Add(new LogData($"[{gList.Key}]_{sSubject}-檔案寫入{(oResult.Success ? "成功" : "失敗")}"));

                                        try
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
                                        }
                                        catch (Exception ex) { oResult.Error(ex.Message); }
                                        LogList.Add(new LogData($"[{gList.Key}]_{sSubject}-資料庫寫入{(oResult.Success ? "成功" : "失敗")}"));
                                    }
                                    else
                                    {
                                        LogList.Add(new LogData($"[{gList.Key}]_{sSubject}-檔案產製失敗"));
                                    }
                                }
                            }
                        }
                    }

                    //清除實體檔案(超過7天)
                    try
                    {
                        DirectoryInfo oDir = new DirectoryInfo(Path.Combine(cyc.Global.AppBasePath, "_logFile", "WorkCheck"));
                        if (oDir != null)
                        {
                            FileInfo[] oFiles = oDir.GetFiles("*", SearchOption.TopDirectoryOnly);
                            for (int i = 0; i < oFiles.Length; i++)
                            {
                                if (oFiles[i].CreationTime < TimeExe.AddDays(-7))
                                    System.IO.File.Delete(oFiles[i].FullName);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        LogList.Add(new LogData($"清除過期檔案發生錯誤：{ex.Message}"));
                    }
                }
            }
            else
            {
                oResult.Error($"範本檔案[{sFile}]不存在");
                LogList.Add(new LogData($"範本檔案[{sFile}]不存在"));
            }
        }

        //        private void DoMapp2(DateTime TimeExe)
        //        {
        //            string sFile = $"{cyc.Global.AppBasePath}\\_upload\\WorkCheckHourMapp.xlsx";
        //            if (File.Exists(sFile))
        //            {
        //                DateTime TimeS = TimeExe;
        //                DateTime TimeE = TimeS.AddHours(1);

        //                using (var oDB = new cyc.DB.SqlDapperConn(oResult, null, false, 60))
        //                {
        //                    IEnumerable<WorkCheckHourData> cList = oDB.QueryList<WorkCheckHourData>($@"
        //with VMT as (
        //	select A.fac_code as Fac,A.con_number as ConNumber,A.main_area as Area,A.vendor_name as Vendor,A.con_conten as ConContent
        //	,replace(SUBSTRING(A.engineer,0,5),'(','') as UserName,A.vendor_pe as VendorMain,A.END_TIME
        //	from View_VMT_FAC A where con_date=@Date and (fac_code='FAC8' or fac_code='FAC6')
        //),CHKALL as (
        //	select A.ConNumber,C.CHECK_TYPE as CheckType,D.FRUserID,D.FRUserName,D.LogDateTime
        //	from VMT A
        //	inner join WORK_CHECKIN B on A.ConNumber=B.con_number
        //	inner join WORK_CHECKIN_LOG C on B.SEQ_ID=C.WORK_CHECKIN_ID
        //	inner join RecognitionAuth D on C.IFP_RecognitionAuth_ID=D.ID
        //)

        //select C.ConNumber,C.Vendor,C.Area,C.ConContent,C.UserName,C.VendorMain,A.FRUserName,A.LogDateTime as CheckInTime,B.LogDateTime as CheckOutTime,E.Name as DeptName,F.MappName,C.END_TIME
        //from CHKALL A left join CHKALL B on B.CheckType='CHECKOUT' and A.ConNumber=B.ConNumber and A.FRUserID=B.FRUserID
        //inner join VMT C on A.ConNumber=C.ConNumber
        //inner join SysUser D on C.UserName=D.Name
        //inner join SysDept E on D.DeptID=E.ID
        //inner join WorkCheckMappSetting F on E.ID=F.DeptID
        //where A.CheckType='CHECKIN' and F.IsEnabled=1
        //order by A.ConNumber,A.LogDateTime,B.LogDateTime", new { TimeS.Date });

        //                    if (!oResult.Success)
        //                        LogList.Add(new LogData($"查詢簽到退資料發生錯誤：{oResult.Message}"));
        //                    else if (cList != null)
        //                        LogList.Add(new LogData($"查詢簽到退資料：{cList.Count()}筆"));

        //                    if (cList != null && cList.Any())
        //                    {
        //                        foreach (var gList in cList.GroupBy(p => p.MappName))
        //                        {
        //                            if (Global.MappSetting.List.FirstOrDefault(p => p.MS_SYS_NAME == gList.Key) != null)
        //                            {
        //                                LogList.Add(new LogData($"處理MAPP-[{gList.Key}]"));

        //                                //每小時 報到退
        //                                var hourList = gList.Where(p => (p.CheckInTime >= @TimeS && p.CheckInTime < TimeE) || (p.CheckOutTime >= @TimeS && p.CheckOutTime < TimeE));
        //                                if (hourList.Any())
        //                                {
        //                                    CreateMapp(hourList, "小時");

        //                                    //當日 報到退
        //                                    List<WorkCheckHourData> dayList = new List<WorkCheckHourData>();
        //                                    foreach (var x in hourList.GroupBy(p => p.ConNumber).Select(p => p.Key))
        //                                        dayList.AddRange(gList.Where(p => p.ConNumber == x));
        //                                    if (dayList.Any())
        //                                        CreateMapp(dayList, "當日");
        //                                }

        //                                //當日 未報退
        //                                var noList = gList.Where(p => p.END_TIME > TimeS && p.END_TIME <= TimeE && p.CheckOutTime == null);
        //                                if (noList.Any())
        //                                    CreateMapp(noList, "未報退");

        //                                void CreateMapp(IEnumerable<WorkCheckHourData> xList, string sType)
        //                                {
        //                                    var oFile = Shared.WorkCheckHourFile(xList, sFile, $"群創施工報到退表({sType})");

        //                                    string sSubject = $"施工管理{(sType == "小時" ? $"{TimeS:yyyyMMddHHmm}" : $"{TimeS:yyyyMMdd}")}{sType}通知";

        //                                    if (oFile != null)
        //                                    {
        //                                        try
        //                                        {
        //                                            using (FileStream oStream = new FileStream($"{cyc.Global.AppBasePath}\\_logFile\\WorkCheck\\{gList.Key}_{sSubject}.xlsx", FileMode.Create, FileAccess.ReadWrite))
        //                                            {
        //                                                oStream.Write(oFile, 0, oFile.Length);
        //                                                oStream.Close();
        //                                            }
        //                                        }
        //                                        catch (Exception ex) { oResult.Error(ex.Message); }
        //                                        LogList.Add(new LogData($"[{gList.Key}]_{sSubject}-檔案寫入{(oResult.Success ? "成功" : "失敗")}"));

        //                                        try
        //                                        {
        //                                            var oMapp = new CYCloud.Mapp.Data.MappMessage
        //                                            {
        //                                                MS_SYS_NAME = gList.Key,
        //                                                MM_CONTENT_TYPE = 3,
        //                                                MM_SUBJECT = sSubject,
        //                                                MM_TYPE = 'A',
        //                                                MM_MEDIA_CONTENT = oFile,
        //                                                MM_ExtFileName = "xlsx",
        //                                                MM_FILE_SHOW_NAME = $"{sSubject}.xlsx"
        //                                            };

        //                                            oDB.Execute(@"
        //insert Into MappMessage(MS_SYS_NAME,MM_CONTENT_TYPE,MM_SUBJECT,MM_TYPE,MM_MEDIA_CONTENT,MM_ExtFileName,MM_FILE_SHOW_NAME)
        //values (@MS_SYS_NAME,@MM_CONTENT_TYPE,@MM_SUBJECT,@MM_TYPE,@MM_MEDIA_CONTENT,@MM_ExtFileName,@MM_FILE_SHOW_NAME)", oMapp);
        //                                        }
        //                                        catch (Exception ex) { oResult.Error(ex.Message); }
        //                                        LogList.Add(new LogData($"[{gList.Key}]_{sSubject}-資料庫寫入{(oResult.Success ? "成功" : "失敗")}"));
        //                                    }
        //                                    else
        //                                    {
        //                                        LogList.Add(new LogData($"[{gList.Key}]_{sSubject}-檔案產製失敗"));
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                oResult.Error($"範本檔案[{sFile}]不存在");
        //                LogList.Add(new LogData($"範本檔案[{sFile}]不存在"));
        //            }
        //        }

        //        private void DoHourMapp(DateTime TimeNow, bool IsHour)
        //        {
        //            string sFile = $"{cyc.Global.AppBasePath}\\_upload\\WorkCheckHourMapp.xlsx";
        //            if (File.Exists(sFile))
        //            {
        //                DateTime TimeS = IsHour ? Convert.ToDateTime(TimeNow.ToString("yyyy/MM/dd HH:00")).AddHours(-1) : TimeNow.AddHours(-1).Date;
        //                DateTime TimeE = IsHour ? TimeS.AddHours(1).AddSeconds(-1) : TimeS.AddDays(1).AddSeconds(-1);

        //                using (var oDB = new cyc.DB.SqlDapperConn(oResult, null, false, 60))
        //                {
        //                    IEnumerable<WorkCheckHourData> cList = oDB.QueryList<WorkCheckHourData>($@"
        //with VMT as (
        //	select A.fac_code as Fac,A.con_number as ConNumber,A.main_area as Area,A.vendor_name as Vendor,A.con_conten as ConContent
        //	,replace(SUBSTRING(A.engineer,0,5),'(','') as UserName,A.vendor_pe as VendorMain,A.END_TIME
        //	from View_VMT_FAC A where con_date=@Date and (fac_code='FAC8' or fac_code='FAC6')
        //),CHKALL as (
        //	select A.ConNumber,C.CHECK_TYPE as CheckType,D.FRUserID,D.FRUserName,D.LogDateTime
        //	from VMT A
        //	inner join WORK_CHECKIN B on A.ConNumber=B.con_number
        //	inner join WORK_CHECKIN_LOG C on B.SEQ_ID=C.WORK_CHECKIN_ID
        //	inner join RecognitionAuth D on C.IFP_RecognitionAuth_ID=D.ID
        //)

        //select C.ConNumber,C.Vendor,C.Area,C.ConContent,C.UserName,C.VendorMain,A.FRUserName,A.LogDateTime as CheckInTime,B.LogDateTime as CheckOutTime,E.Name as DeptName,F.MappName
        //from CHKALL A left join CHKALL B on B.CheckType='CHECKOUT' and A.ConNumber=B.ConNumber and A.FRUserID=B.FRUserID
        //inner join VMT C on A.ConNumber=C.ConNumber
        //inner join SysUser D on C.UserName=D.Name
        //inner join SysDept E on D.DeptID=E.ID
        //inner join WorkCheckMappSetting F on E.ID=F.DeptID
        //where A.CheckType='CHECKIN' {(IsHour ? "and ((A.LogDateTime between @TimeS and @TimeE) or (B.LogDateTime between @TimeS and @TimeE))" : string.Empty)} and F.IsEnabled=1
        //order by A.ConNumber,A.LogDateTime,B.LogDateTime", new { TimeS.Date, TimeS, TimeE });

        //                    if (cList != null && cList.Any())
        //                    {
        //                        foreach (var mData in cList.GroupBy(p => p.MappName))
        //                        {
        //                            var oSetting = Global.MappSetting.List.FirstOrDefault(p => p.MS_SYS_NAME == mData.Key);
        //                            if (oSetting != null)
        //                            {
        //                                if (IsHour)
        //                                {
        //                                    var oFile = Shared.WorkCheckHourFile(mData, sFile, "群創施工報到退表(小時)");
        //                                    if (oFile != null)
        //                                    {
        //                                        var oMapp = new CYCloud.Mapp.Data.MappMessage
        //                                        {
        //                                            MS_SYS_NAME = mData.Key,
        //                                            MM_CONTENT_TYPE = 3,
        //                                            MM_SUBJECT = $"施工管理{TimeS:yyyy-MM-dd HH:mm}~{TimeE:HH:mm}簽到退通知",
        //                                            MM_TYPE = 'A',
        //                                            MM_MEDIA_CONTENT = oFile,
        //                                            MM_ExtFileName = "xlsx",
        //                                            MM_FILE_SHOW_NAME = $"施工管理{TimeS:yyyyMMddHHmm}簽到退通知.xlsx"
        //                                        };

        //                                        oDB.Execute(@"
        //insert Into MappMessage(MS_SYS_NAME,MM_CONTENT_TYPE,MM_SUBJECT,MM_TYPE,MM_MEDIA_CONTENT,MM_ExtFileName,MM_FILE_SHOW_NAME)
        //values (@MS_SYS_NAME,@MM_CONTENT_TYPE,@MM_SUBJECT,@MM_TYPE,@MM_MEDIA_CONTENT,@MM_ExtFileName,@MM_FILE_SHOW_NAME)", oMapp);
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    foreach (var xData in mData.GroupBy(p => p.END_TIME > TimeNow)) //依 工單時間是否到期 分組
        //                                    {
        //                                        byte[] oFile = null;
        //                                        if (xData.Key) //未到期，發送全部清單
        //                                        {
        //                                            oFile = Shared.WorkCheckHourFile(xData, sFile, "群創施工報到退表(當日)");
        //                                        }
        //                                        else if (xData.Any(p => p.CheckOutTime == null)) //已到期，發送未報退清單
        //                                        {
        //                                            oFile = Shared.WorkCheckHourFile(xData.Where(p => p.CheckOutTime == null), sFile, "群創施工報到退表(未報退)");
        //                                        }

        //                                        if (oFile != null)
        //                                        {
        //                                            var oMapp = new CYCloud.Mapp.Data.MappMessage
        //                                            {
        //                                                MS_SYS_NAME = mData.Key,
        //                                                MM_CONTENT_TYPE = 3,
        //                                                MM_SUBJECT = $"施工管理{TimeS:yyyy-MM-dd}{(xData.Key ? "報到退" : "未報退")}通知",
        //                                                MM_TYPE = 'A',
        //                                                MM_MEDIA_CONTENT = oFile,
        //                                                MM_ExtFileName = "xlsx",
        //                                                MM_FILE_SHOW_NAME = $"施工管理{TimeS:yyyyMMdd}{(xData.Key ? "報到退" : "未報退")}通知.xlsx"
        //                                            };

        //                                            oDB.Execute(@"
        //insert Into MappMessage(MS_SYS_NAME,MM_CONTENT_TYPE,MM_SUBJECT,MM_TYPE,MM_MEDIA_CONTENT,MM_ExtFileName,MM_FILE_SHOW_NAME)
        //values (@MS_SYS_NAME,@MM_CONTENT_TYPE,@MM_SUBJECT,@MM_TYPE,@MM_MEDIA_CONTENT,@MM_ExtFileName,@MM_FILE_SHOW_NAME)", oMapp);
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //                oResult.Error($"範本檔案[{sFile}]不存在");
        //        }

        //        private void DoMapp(DateTime TimeExe)
        //        {
        //            string sFile = $"{cyc.Global.AppBasePath}\\_upload\\WorkCheckHourMapp.xlsx";
        //            if (File.Exists(sFile))
        //            {
        //                DateTime TimeS = TimeExe;
        //                DateTime TimeE = TimeS.AddHours(1);

        //                using (var oDB = new cyc.DB.SqlDapperConn(oResult, null, false, 60))
        //                {
        //                    IEnumerable<WorkCheckHourData> cList = oDB.QueryList<WorkCheckHourData>($@"
        //with VMT as (
        //	select A.fac_code as Fac,A.con_number as ConNumber,A.main_area as Area,A.vendor_name as Vendor,A.con_conten as ConContent
        //	,replace(SUBSTRING(A.engineer,0,5),'(','') as UserName,A.vendor_pe as VendorMain,A.END_TIME
        //	from View_VMT_FAC A where con_date=@Date and (fac_code='FAC8' or fac_code='FAC6')
        //),CHKALL as (
        //	select A.ConNumber,C.CHECK_TYPE as CheckType,D.FRUserID,D.FRUserName,D.LogDateTime
        //	from VMT A
        //	inner join WORK_CHECKIN B on A.ConNumber=B.con_number
        //	inner join WORK_CHECKIN_LOG C on B.SEQ_ID=C.WORK_CHECKIN_ID
        //	inner join RecognitionAuth D on C.IFP_RecognitionAuth_ID=D.ID
        //)

        //select C.ConNumber,C.Vendor,C.Area,C.ConContent,C.UserName,C.VendorMain,A.FRUserName,A.LogDateTime as CheckInTime,B.LogDateTime as CheckOutTime,E.Name as DeptName,F.MappName
        //from CHKALL A left join CHKALL B on B.CheckType='CHECKOUT' and A.ConNumber=B.ConNumber and A.FRUserID=B.FRUserID
        //inner join VMT C on A.ConNumber=C.ConNumber
        //inner join SysUser D on C.UserName=D.Name
        //inner join SysDept E on D.DeptID=E.ID
        //inner join WorkCheckMappSetting F on E.ID=F.DeptID
        //where A.CheckType='CHECKIN' and F.IsEnabled=1
        //order by A.ConNumber,A.LogDateTime,B.LogDateTime", new { TimeS.Date });

        //                    if (cList != null && cList.Any())
        //                    {
        //                        foreach (var mData in cList.GroupBy(p => p.MappName))
        //                        {
        //                            var oSetting = Global.MappSetting.List.FirstOrDefault(p => p.MS_SYS_NAME == mData.Key);
        //                            if (oSetting != null)
        //                            {
        //                                //每小時 報到退
        //                                var hList = mData.Where(p => (p.CheckInTime >= @TimeS && p.CheckInTime < TimeE) || (p.CheckOutTime >= @TimeS && p.CheckOutTime < TimeE));
        //                                if (hList.Any()) { CreateMapp(hList, "小時"); }

        //                                //當日 報到退
        //                                var aList = mData.Where(p => p.END_TIME >= TimeExe);
        //                                if (aList.Any()) { CreateMapp(aList, "當日"); }

        //                                //當日 未報退
        //                                var nList = mData.Where(p => p.END_TIME <= TimeExe && p.CheckOutTime == null);
        //                                if (nList.Any()) { CreateMapp(nList, "未報退"); }

        //                                void CreateMapp(IEnumerable<WorkCheckHourData> xList, string sType)
        //                                {
        //                                    var oFile = Shared.WorkCheckHourFile(xList, sFile, $"群創施工報到退表({sType})");
        //                                    if (oFile != null)
        //                                    {
        //                                        var oMapp = new CYCloud.Mapp.Data.MappMessage
        //                                        {
        //                                            MS_SYS_NAME = mData.Key,
        //                                            MM_CONTENT_TYPE = 3,
        //                                            MM_SUBJECT = $"施工管理{(sType== "小時" ? $"{TimeS:yyyy-MM-dd HH:mm}" : $"{TimeS:yyyy-MM-dd}")}{sType}通知",
        //                                            MM_TYPE = 'A',
        //                                            MM_MEDIA_CONTENT = oFile,
        //                                            MM_ExtFileName = "xlsx",
        //                                            MM_FILE_SHOW_NAME = $"施工管理{(sType == "小時" ? $"{TimeS:yyyyMMddHHmm}" : $"{TimeS:yyyyMMdd}")}{sType}通知.xlsx"
        //                                        };

        //                                        oDB.Execute(@"
        //insert Into MappMessage(MS_SYS_NAME,MM_CONTENT_TYPE,MM_SUBJECT,MM_TYPE,MM_MEDIA_CONTENT,MM_ExtFileName,MM_FILE_SHOW_NAME)
        //values (@MS_SYS_NAME,@MM_CONTENT_TYPE,@MM_SUBJECT,@MM_TYPE,@MM_MEDIA_CONTENT,@MM_ExtFileName,@MM_FILE_SHOW_NAME)", oMapp);
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //                oResult.Error($"範本檔案[{sFile}]不存在");
        //        }
    }

    //施工管理統計表
    public static class WorkCheckReport
    {
        public static void WorkCheckReportForMAPP(ExeResult oResult, DateTime? dDate = null)
        {
            DateTime tDate = DateTime.Today;
            if (dDate != null) tDate = ((DateTime)dDate).Date;

            using (cyc.DB.SqlDapperConn oDB = new SqlDapperConn(oResult, null, false, 60))
            {
                //廠別發送設定(各廠+總廠)
                var mList = oDB.QueryList<cyc.Data.BaseObj>(@"
select A.FacCode as Code,A.MappName as Name from WorkCheckMappFacReport A
inner join MappSetting B on A.MappName=B.MS_SYS_NAME where A.IsEnabled=1");
                if (oResult.Success && mList.Any())
                {
                    var xList = GetQueryList(tDate, tDate, oDB, "ALL");//查詢原始資料(全部)
                    if (xList != null && xList.Any())
                    {
                        foreach (var x in xList.GroupBy(p => p.Fac))//原始資料依廠別分組
                        {
                            if (mList.Any(p => p.Code == x.Key))//有設定的廠別
                            {
                                var qList = GetSummaryData(x, x.Key);//彙整資料(各廠方式)
                                if (qList != null && qList.Any())
                                {
                                    CreateMAPP(qList, x.Key, mList.Where(p => p.Code == x.Key));
                                }
                            }
                        }

                        //總廠有設定發送
                        if (mList.Any(p => p.Code == "ALL"))
                        {
                            var qList = GetSummaryData(xList, "ALL");//彙整資料(總廠方式)
                            if (qList != null && qList.Any())
                            {
                                CreateMAPP(qList, "ALL", mList.Where(p => p.Code == "ALL"));
                            }
                        }

                        void CreateMAPP(List<QryData> sList, string sFac, IEnumerable<BaseObj> nList)
                        {
                            byte[] oFile = CreateExcelFile(sList, sFac);//產製檔案
                            if (oFile != null)
                            {
                                foreach (var nData in nList)//MAPP設定
                                {
                                    var oMapp = new CYCloud.Mapp.Data.MappMessage
                                    {
                                        MS_SYS_NAME = nData.Name,
                                        MM_CONTENT_TYPE = 3,
                                        MM_SUBJECT = $"{(sFac == "ALL" ? "總廠" : sFac)}施工管理報表",
                                        MM_TYPE = 'A',
                                        MM_MEDIA_CONTENT = oFile,
                                        MM_ExtFileName = "xlsx",
                                        MM_FILE_SHOW_NAME = $"{(sFac == "ALL" ? "總廠" : sFac)}施工管理報表({tDate:yyyy-MM-dd}).xlsx"
                                    };

                                    oDB.Execute(@"
insert Into MappMessage(MS_SYS_NAME,MM_CONTENT_TYPE,MM_SUBJECT,MM_TYPE,MM_MEDIA_CONTENT,MM_ExtFileName,MM_FILE_SHOW_NAME)
values (@MS_SYS_NAME,@MM_CONTENT_TYPE,@MM_SUBJECT,@MM_TYPE,@MM_MEDIA_CONTENT,@MM_ExtFileName,@MM_FILE_SHOW_NAME)", oMapp);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static byte[] CreateExcelFile(List<QryData> qList, string sFac)
        {
            byte[] oFile = null;
            try
            {
                IWorkbook wk = cyc.Shared.NPOI.GetWorkbook($"{cyc.Global.AppBasePath}\\_upload\\總廠施工統計報表.xlsx");
                ISheet oSheet = wk.GetSheetAt(0); //範本頁籤

                //var tData = qList.FirstOrDefault(p => p.Vendor == "總計");
                //if (tData != null)
                //{
                //    IRow rowH = oSheet.GetRow(0);
                //    rowH.GetCell(1).SetCellValue(sFac);
                //    rowH.GetCell(3).SetCellValue(tData.Cnt01);
                //    rowH.GetCell(5).SetCellValue(tData.Cnt02);
                //    rowH.GetCell(7).SetCellValue(tData.Cnt02 / (float)tData.Cnt01);
                //}
                IRow rowH = oSheet.GetRow(0);
                rowH.GetCell(1).SetCellValue(sFac);
                rowH.GetCell(3).SetCellValue(qList.Sum(p => p.Cnt01));
                rowH.GetCell(5).SetCellValue(qList.Sum(p => p.Cnt02));
                rowH.GetCell(7).SetCellValue(qList.Sum(p => p.Cnt02) / (float)qList.Sum(p => p.Cnt01));

                oSheet.GetRow(4).GetCell(0).SetCellValue(sFac == "ALL" ? "施工廠區" : "施工廠商");

                IRow rowT = oSheet.GetRow(5);
                int idx = 6;
                foreach (var q in qList)
                {
                    IRow rowQ = rowT.CopyRowTo(idx++);
                    rowQ.GetCell(0).SetCellValue(q.Vendor);
                    rowQ.GetCell(1).SetCellValue(q.Cnt01); //申請工單數
                    rowQ.GetCell(2).SetCellValue(q.Cnt02); //報到工單數
                    rowQ.GetCell(3).SetCellValue(q.AccCnt); //哨口報到人數
                    rowQ.GetCell(4).SetCellValue(q.ChkCnt); //廠務報到人數
                    rowQ.GetCell(5).SetCellValue(q.Rate01); //工單報到率
                    rowQ.GetCell(6).SetCellValue(q.Rate02); //人數報到率
                    rowQ.GetCell(7).SetCellValue(q.T01); //一般作業
                    rowQ.GetCell(8).SetCellValue(q.T02); //動火作業
                    rowQ.GetCell(9).SetCellValue(q.T03); //送電、活線作業或活線接近作業
                    rowQ.GetCell(10).SetCellValue(q.T04); //高架作業
                    rowQ.GetCell(11).SetCellValue(q.T05); //吊掛作業
                    rowQ.GetCell(12).SetCellValue(q.T06); //局限空間作業
                    rowQ.GetCell(13).SetCellValue(q.T07); //路面開挖作業
                    rowQ.GetCell(14).SetCellValue(q.T08); //Inter-Lock by pass
                    rowQ.GetCell(15).SetCellValue(q.T09); //安全防護系統中斷/隔離作業
                    rowQ.GetCell(16).SetCellValue(q.T10); //危險管路拆卸鑽孔作業與化學品塗佈作業
                    rowQ.GetCell(17).SetCellValue(q.T11); //開孔/防墬安全設施拆除作業
                }
                oSheet.ShiftRows(6, oSheet.LastRowNum, -1);//向上移動一列，移除範本列

                using (MemoryStream ms = new MemoryStream())
                {
                    wk.Write(ms);
                    oFile = ms.ToArray();
                    ms.Close();
                }
                wk = null;
            }
            catch (Exception ex) { cyc.Log.WriteSysErrorLog($"施工管理統計表匯出:{ex.Message}"); }
            return oFile;
        }

        public static IEnumerable<QryData> GetQueryList(DateTime DateS, DateTime DateE, cyc.DB.SqlDapperConn oDB, string sFac = "ALL", string sVendor = null, string sType = null)
        {
            var oList = oDB.QueryList<QryData>($@"
with VMT as (
	select A.fac_code as Fac,A.vendor_name,A.con_number,A.SEQ_ID,A.con_date,T01,T02,T03,T04,T05,T06,T07,T08,T09,T10,T11
	from WORK_CHECKIN A inner join SysUser B on A.engineer=B.Name
    where A.con_date between @DateS and @DateE {(sFac == "ALL" ? string.Empty : "and A.fac_code=@Fac")} {(string.IsNullOrEmpty(sVendor) ? string.Empty : (sFac == "ALL" ? "and A.fac_code=@Vendor" : "and A.vendor_name=@Vendor"))}
), ACCESS as (
	select A.con_number,count(distinct B.ID) as AccCnt from VMT A
	inner join AccessList2 B on A.con_number=B.APPLY_PK group by A.con_number
), CHECKIN as (
	select A.con_number,count(distinct C.FRUserID) as ChkCnt from VMT A 
	inner join WORK_CHECKIN_LOG B on A.SEQ_ID=B.WORK_CHECKIN_ID 
	inner join RecognitionAuth C on B.IFP_RecognitionAuth_ID=C.ID
	group by A.con_number
)
select A.con_number,A.con_date,A.Fac,A.vendor_name as Vendor,ISNULL(B.AccCnt,0)as AccCnt,ISNULL(C.ChkCnt,0)as ChkCnt
,A.T01,A.T02,A.T03,A.T04,A.T05,A.T06,A.T07,A.T08,A.T09,A.T10,A.T11
from VMT A 
left join ACCESS B on A.con_number=B.con_number
left join CHECKIN C on A.con_number=C.con_number {(string.IsNullOrEmpty(sType) ? "" : $"where A.{sType}=1")}", new { DateS, DateE, Fac = sFac, Vendor = sVendor });

            if (oDB.Result.Success)
                return oList;
            return null;
        }

        public static List<QryData> GetSummaryData(IEnumerable<QryData> oList, string sFac = "ALL")
        {
            List<QryData> qList = new List<QryData>();
            if (sFac == "ALL")
            {
                foreach (var x in oList.GroupBy(p => p.Fac).OrderBy(p => p.Key))
                    qList.Add(AddItem(x));
            }
            else
            {
                foreach (var x in oList.GroupBy(p => p.Vendor).OrderBy(p => p.Key))
                    qList.Add(AddItem(x));
            }
            //AddSumData();

            //void AddSumData(){
            //    int tCnt = qList.Sum(p => p.Cnt01);
            //    int tAcc = qList.Sum(p => p.Cnt02);
            //    var tData = new QryData
            //    {
            //        Vendor = "總計",
            //        Cnt01 = tCnt,
            //        Cnt02 = tAcc,
            //        AccCnt = qList.Sum(p => p.AccCnt),
            //        ChkCnt = qList.Sum(p => p.ChkCnt),
            //        T01 = qList.Sum(p => p.T01),
            //        T02 = qList.Sum(p => p.T02),
            //        T03 = qList.Sum(p => p.T03),
            //        T04 = qList.Sum(p => p.T04),
            //        T05 = qList.Sum(p => p.T05),
            //        T06 = qList.Sum(p => p.T06),
            //        T07 = qList.Sum(p => p.T07),
            //        T08 = qList.Sum(p => p.T08),
            //        T09 = qList.Sum(p => p.T09),
            //        T10 = qList.Sum(p => p.T10),
            //        T11 = qList.Sum(p => p.T11),
            //        Rate01 = tAcc / (float)tCnt,
            //    };
            //    if (tData.AccCnt > 0) tData.Rate02 = tData.ChkCnt / (float)tData.AccCnt;
            //    qList.Add(tData);
            //}

            QryData AddItem(IGrouping<string, QryData> x)
            {
                int iCnt = x.Count();
                int iAcc = x.Sum(p => p.AccCnt);
                int iChk = x.Sum(p => p.ChkCnt);

                QryData qData = new QryData
                {
                    Vendor = x.Key,
                    Cnt01 = iCnt,
                    Cnt02 = x.Count(p => p.AccCnt > 0),
                    AccCnt = iAcc,
                    ChkCnt = iChk,
                    T01 = x.Where(p => p.AccCnt > 0).Sum(p => p.T01),
                    T02 = x.Where(p => p.AccCnt > 0).Sum(p => p.T02),
                    T03 = x.Where(p => p.AccCnt > 0).Sum(p => p.T03),
                    T04 = x.Where(p => p.AccCnt > 0).Sum(p => p.T04),
                    T05 = x.Where(p => p.AccCnt > 0).Sum(p => p.T05),
                    T06 = x.Where(p => p.AccCnt > 0).Sum(p => p.T06),
                    T07 = x.Where(p => p.AccCnt > 0).Sum(p => p.T07),
                    T08 = x.Where(p => p.AccCnt > 0).Sum(p => p.T08),
                    T09 = x.Where(p => p.AccCnt > 0).Sum(p => p.T09),
                    T10 = x.Where(p => p.AccCnt > 0).Sum(p => p.T10),
                    T11 = x.Where(p => p.AccCnt > 0).Sum(p => p.T11),
                    Rate01 = x.Count(p => p.AccCnt > 0) / (float)iCnt
                };
                if (iAcc > 0) qData.Rate02 = iChk / (float)iAcc;

                return qData;
            }
            return qList;
        }

        public class QryData
        {
            public string con_number { get; set; }
            public DateTime con_date { get; set; }
            public string Fac { get; set; }
            public string Vendor { get; set; }
            public int T01 { get; set; }
            public int T02 { get; set; }
            public int T03 { get; set; }
            public int T04 { get; set; }
            public int T05 { get; set; }
            public int T06 { get; set; }
            public int T07 { get; set; }
            public int T08 { get; set; }
            public int T09 { get; set; }
            public int T10 { get; set; }
            public int T11 { get; set; }
            public int AccCnt { get; set; } //哨口報到人數
            public int ChkCnt { get; set; } //廠務報到人數
            public float Rate01 { get; set; } //工單報到率 = 報到工單數 / 總單數
            public float Rate02 { get; set; } //人數報到率 = 廠務報到人數 / 哨口報到人數
            public int Cnt01 { get; set; } //總單數
            public int Cnt02 { get; set; } //(哨口)報到工單數
        }
    }

    //自動報到+報退
    public static class AutoCheckInOut
    {
        //自動報到
        public static void AutoCheckIn(DateTime TimeNow)
        {
            //if (sWorkAutoCheckIn != "1") { return; }
            DateTime DateS = TimeNow.Date;
            DateTime DateE = DateS.AddDays(1).AddSeconds(-1);

            using (SqlDapperConn oDB = new SqlDapperConn(null, "", false, 60))
            {
                ////自動新增今日[WORK_CHECKIN].[con_number]資料
                //ImportWorkCheck(TimeNow, oDB);

                //20240611 Auto Import RecognitionAuth From AccessList For [FAC8] and [FAC6]
                ImportRecognitionAuthFromAccessListForCheckin(TimeNow, oDB);

                //查詢待處理資料
                var xList = oDB.QueryList<CheckInData>(@"
select C.SEQ_ID as WORK_CHECKIN_ID, A.ID as IFP_RecognitionAuth_ID,rtrim(A.FRUserID)as FRUserID,C.checkin_time as CheckinTime
from (
	select A.ID,A.FRUserID,A.LogDateTime,A.Fac from RecognitionAuth A where A.LogDateTime between @DateS and @DateE and UseFlag='N'
) A 
inner join (
	select ID,EV_DATE,APPLY_PK,[LOCATION] from AccessList2 where EV_DATE between @DateS and @DateE and Direction=1
) B on A.FRUserID=B.ID and A.LogDateTime>=B.EV_DATE
inner join (
	select B.SEQ_ID,B.con_number,B.checkin_time,A.fac_name from View_VMT_FAC A 
    inner join WORK_CHECKIN B on A.con_number=B.con_number where A.con_date=@DateS
) C on B.APPLY_PK=C.con_number
inner join AccessListMapping D on A.Fac=D.FAC and C.fac_name=D.VNTFAC and B.[LOCATION]=D.[LOCATION]
order by C.SEQ_ID,A.LogDateTime", new { DateS, DateE });

                if (xList != null && xList.Any())
                {
                    List<WorkCheckInLog> logList = new List<WorkCheckInLog>();

                    //依[工單]分組
                    foreach (var gData in xList.GroupBy(p => p.WORK_CHECKIN_ID))
                    {
                        //[工單]無報到時間
                        if (gData.First().CheckinTime == null)
                            oDB.Execute("update WORK_CHECKIN set checkin_time=@Time where SEQ_ID=@ID and checkin_time is null", new { ID = gData.Key, Time = TimeNow });

                        //[工單]已報到名單
                        var inList = oDB.QueryList<string>(@"
select rtrim(B.FRUserID)as FRUserID from WORK_CHECKIN_LOG A inner join RecognitionAuth B on A.IFP_RecognitionAuth_ID=B.ID 
where A.WORK_CHECKIN_ID=@ID and A.CHECK_TYPE='CHECKIN'", new { ID = gData.Key }).ToList();

                        foreach (var x in gData)
                        {
                            //名單無相同人員資料
                            if (!inList.Any(p => p == x.FRUserID))
                            {
                                //加入名單
                                inList.Add(x.FRUserID);
                                //新增至[WORK_CHECKIN_LOG]
                                logList.Add(new WorkCheckInLog { WORK_CHECKIN_ID = x.WORK_CHECKIN_ID, IFP_RecognitionAuth_ID = x.IFP_RecognitionAuth_ID, CHECK_TYPE = "CHECKIN", update_time = TimeNow, update_user = 0 });
                                //oDB.Execute("insert into WORK_CHECKIN_LOG (WORK_CHECKIN_ID,CHECK_TYPE,IFP_RecognitionAuth_ID,update_time,update_user) values ( @WORK_CHECKIN_ID,'CHECKIN',@IFP_RecognitionAuth_ID,@Time,@User )", new { x.WORK_CHECKIN_ID, x.IFP_RecognitionAuth_ID, User = 0, Time = TimeNow });
                                //更新註記
                                //oDB.Execute("update RecognitionAuth set UseFlag='Y' where ID=@IFP_RecognitionAuth_ID", new { x.IFP_RecognitionAuth_ID });
                            }
                        }
                    }

                    if (logList.Any())
                    {
                        oDB.Execute(@"
insert into WORK_CHECKIN_LOG (WORK_CHECKIN_ID,CHECK_TYPE,IFP_RecognitionAuth_ID,update_time,update_user) 
values ( @WORK_CHECKIN_ID,@CHECK_TYPE,@IFP_RecognitionAuth_ID,@update_time,@update_user )", logList);

                        oDB.Execute("update RecognitionAuth set UseFlag='Y' where ID in @IDs and UseFlag='N'", new { IDs = logList.Select(p => p.IFP_RecognitionAuth_ID) });
                    }
                }
            }
        }
        //自動報退
        public static void AutoCheckOut(DateTime TimeNow)
        {
            DateTime DateS = TimeNow.Date;
            DateTime DateE = DateS.AddDays(1).AddSeconds(-1);

            using (SqlDapperConn oDB = new SqlDapperConn(null, "", false, 60))
            {
                //20240611 Auto Import RecognitionAuth From AccessList For [FAC8] and [FAC6]
                ImportRecognitionAuthFromAccessListForCheckout(TimeNow, oDB);

                var qData = oDB.QueryMultiple(@"
select A.ID,A.Fac,A.LogDateTime,A.FRUserID,A.FRUserName from RecognitionAuth A
where A.LogDateTime between @DateS and @DateE and A.UseFlag='N'
order by A.LogDateTime desc
;
select A.ID,A.Fac,A.LogDateTime,A.FRUserID,B.WORK_CHECKIN_ID,B.CHECK_TYPE,C.checkout_time
from RecognitionAuth A
inner join WORK_CHECKIN_LOG B on A.ID=B.IFP_RecognitionAuth_ID
inner join WORK_CHECKIN C on B.WORK_CHECKIN_ID=C.SEQ_ID
where A.LogDateTime between @DateS and @DateE", new { DateS, DateE });

                if (qData != null)
                {
                    var sList = qData.Read<RecognitionAuth>(); //未被註記資料
                    var qList = qData.Read<CheckOutTemp2>(); //目前已[報到]+[報退]資料

                    if (qList.Any())
                    {
                        foreach (var wList in qList.GroupBy(p => p.WORK_CHECKIN_ID)) //依[工單]分組
                        {
                            //[工單]報退時間
                            DateTime CheckOutTime = DateTime.MinValue;

                            //已報到資料
                            foreach (var inData in wList.Where(p => p.CHECK_TYPE == "CHECKIN"))
                            {
                                //原 報退資料
                                var outData = wList.FirstOrDefault(p => p.CHECK_TYPE == "CHECKOUT" && p.FRUserID == inData.FRUserID);
                                //最新一筆 報退資料
                                var newData = sList.FirstOrDefault(p => p.Fac == inData.Fac && p.FRUserID == inData.FRUserID && p.LogDateTime >= inData.LogDateTime.AddMinutes(10));

                                if (outData == null && newData != null)
                                {
                                    if (CheckOutTime < newData.LogDateTime) CheckOutTime = newData.LogDateTime;
                                    //新增報退LOG
                                    oDB.Execute("insert into WORK_CHECKIN_LOG (WORK_CHECKIN_ID,CHECK_TYPE,IFP_RecognitionAuth_ID) values (@wID,'CHECKOUT',@rID)", new { wID = wList.Key, rID = newData.ID });
                                    oDB.Execute("update RecognitionAuth set UseFlag='Y' where ID=@ID and UseFlag<>'Y'", new { newData.ID });
                                }
                                else if (outData != null && newData != null && newData.LogDateTime > outData.LogDateTime)
                                {
                                    if (CheckOutTime < newData.LogDateTime) CheckOutTime = newData.LogDateTime;
                                    //更新報退LOG
                                    oDB.Execute("update WORK_CHECKIN_LOG set IFP_RecognitionAuth_ID=@rID where SEQ_ID=@SEQ_ID", new { outData.SEQ_ID, rID = newData.ID });
                                    oDB.Execute("update RecognitionAuth set UseFlag='Y' where ID=@ID and UseFlag<>'Y'", new { newData.ID });
                                }
                            }

                            //更新 工單 報退時間
                            if (CheckOutTime > (wList.First().checkout_time ?? DateTime.MinValue))
                                oDB.Execute("update WORK_CHECKIN set checkout_time=@CheckOutTime where SEQ_ID=@ID", new { CheckOutTime, ID = wList.Key });
                        }
                    }
                }
            }
        }

        //自動新增 [WORK_CHECKIN]資料，from[View_VMT_FAC]
        internal static void ImportWorkCheck(DateTime TimeNow)
        {
            using (SqlDapperConn oDB = new SqlDapperConn(null, "", false, 60))
            {
                //新增當日，尚未匯入資料
                oDB.Execute(@"
WITH VMT as (
	select distinct A.con_number,cast(A.con_date as date)as con_date,A.fac_code,A.fac_name,REPLACE(SUBSTRING(A.engineer,1,5),'(','')as engineer,A.vendor_name,A.vendor_pe
	,ISNULL(type1,'')+','+ISNULL(type2,'')+','+ISNULL(type3,'')+','+ISNULL(type4,'')+','+ISNULL(type5,'')+','+ISNULL(type6,'')+','+ISNULL(type7,'') as typeT
	from View_VMT_FAC A left join WORK_CHECKIN B on A.con_number=B.con_number where A.con_date=@Date and B.con_number is null
)
insert into WORK_CHECKIN (con_number,con_date,fac_code,fac_name,engineer,vendor_name,vendor_pe,T01,T02,T03,T04,T05,T06,T07,T08,T09,T10,T11)
select con_number,con_date,fac_code,fac_name,engineer,vendor_name,vendor_pe
,case when typeT like '%一般作業%' then 1 else 0 end as T01
,case when typeT like '%動火作業%' then 1 else 0 end as T02
,case when typeT like '%送電、活線作業或活線接近作業%%' then 1 else 0 end as T03
,case when typeT like '%高架作業%' then 1 else 0 end as T04
,case when typeT like '%吊掛作業%' then 1 else 0 end as T05
,case when typeT like '%局限空間作業%' then 1 else 0 end as T06
,case when typeT like '%路面開挖作業%' then 1 else 0 end as T07
,case when typeT like '%Inter-Lock by pass%' then 1 else 0 end as T08
,case when typeT like '%安全防護系統中斷/隔離作業%' then 1 else 0 end as T09
,case when typeT like '%危險管路拆卸鑽孔作業與化學品塗佈作業%' then 1 else 0 end as T10
,case when typeT like '%開孔/防墬安全設施拆除作業%' then 1 else 0 end as T11
from VMT A inner join SysUser B on A.engineer=B.[Name]", new { TimeNow.Date });

                //補齊舊資料，將尚未填入相關資料，一次10000筆
                oDB.Execute(@"
update B set B.con_date=A.con_date,B.fac_code=A.fac_code,B.fac_name=A.fac_name,B.engineer=A.engineer,B.vendor_name=A.vendor_name,B.vendor_pe=A.vendor_pe
,T01=case when A.typeT like '%一般作業%' then 1 else 0 end
,T02=case when A.typeT like '%動火作業%' then 1 else 0 end
,T03=case when A.typeT like '%送電、活線作業或活線接近作業%' then 1 else 0 end
,T04=case when A.typeT like '%高架作業%' then 1 else 0 end
,T05=case when A.typeT like '%吊掛作業%' then 1 else 0 end
,T06=case when A.typeT like '%局限空間作業%' then 1 else 0 end
,T07=case when A.typeT like '%路面開挖作業%' then 1 else 0 end
,T08=case when A.typeT like '%Inter-Lock by pass%' then 1 else 0 end
,T09=case when A.typeT like '%安全防護系統中斷/隔離作業%' then 1 else 0 end
,T10=case when A.typeT like '%危險管路拆卸鑽孔作業與化學品塗佈作業%' then 1 else 0 end
,T11=case when A.typeT like '%開孔/防墬安全設施拆除作業%' then 1 else 0 end
from (
	select top 10000 A.SEQ_ID,B.con_number,B.fac_code,B.fac_name,B.con_date,SUBSTRING(B.vendor_name,0,20)as vendor_name,replace(SUBSTRING(B.engineer,0,5),'(','')engineer,B.vendor_pe
	,ISNULL(type1,'')+','+ISNULL(type2,'')+','+ISNULL(type3,'')+','+ISNULL(type4,'')+','+ISNULL(type5,'')+','+ISNULL(type6,'')+','+ISNULL(type7,'') as typeT
	from WORK_CHECKIN A
	inner join View_VMT_FAC B on A.con_number=B.con_number
	where A.con_date is null order by A.SEQ_ID
) A inner join WORK_CHECKIN B on A.SEQ_ID=B.SEQ_ID", null);
            }
        }
        //自動更新 [WORK_CHECKIN]資料，from[View_VMT_FAC]
        internal static void UpdateWorkCheck(DateTime TimeNow)
        {
            using (SqlDapperConn oDB = new SqlDapperConn(null, "", false, 60))
            {
                for (int idx = 0; idx < 3; idx++)
                {
                    oDB.Execute(@"
WITH VMT as (
	select distinct A.con_number,cast(A.con_date as date)as con_date,A.fac_code,A.fac_name,REPLACE(SUBSTRING(A.engineer,1,5),'(','')as engineer,A.vendor_name,A.vendor_pe
	,case when ISNULL(type1,'')+','+ISNULL(type2,'')+','+ISNULL(type3,'')+','+ISNULL(type4,'')+','+ISNULL(type5,'')+','+ISNULL(type6,'')+','+ISNULL(type7,'') like '%一般作業%' then 1 else 0 end as T01
	,case when ISNULL(type1,'')+','+ISNULL(type2,'')+','+ISNULL(type3,'')+','+ISNULL(type4,'')+','+ISNULL(type5,'')+','+ISNULL(type6,'')+','+ISNULL(type7,'') like '%動火作業%' then 1 else 0 end as T02
	,case when ISNULL(type1,'')+','+ISNULL(type2,'')+','+ISNULL(type3,'')+','+ISNULL(type4,'')+','+ISNULL(type5,'')+','+ISNULL(type6,'')+','+ISNULL(type7,'') like '%送電、活線作業或活線接近作業%%' then 1 else 0 end as T03
	,case when ISNULL(type1,'')+','+ISNULL(type2,'')+','+ISNULL(type3,'')+','+ISNULL(type4,'')+','+ISNULL(type5,'')+','+ISNULL(type6,'')+','+ISNULL(type7,'') like '%高架作業%' then 1 else 0 end as T04
	,case when ISNULL(type1,'')+','+ISNULL(type2,'')+','+ISNULL(type3,'')+','+ISNULL(type4,'')+','+ISNULL(type5,'')+','+ISNULL(type6,'')+','+ISNULL(type7,'') like '%吊掛作業%' then 1 else 0 end as T05
	,case when ISNULL(type1,'')+','+ISNULL(type2,'')+','+ISNULL(type3,'')+','+ISNULL(type4,'')+','+ISNULL(type5,'')+','+ISNULL(type6,'')+','+ISNULL(type7,'') like '%局限空間作業%' then 1 else 0 end as T06
	,case when ISNULL(type1,'')+','+ISNULL(type2,'')+','+ISNULL(type3,'')+','+ISNULL(type4,'')+','+ISNULL(type5,'')+','+ISNULL(type6,'')+','+ISNULL(type7,'') like '%路面開挖作業%' then 1 else 0 end as T07
	,case when ISNULL(type1,'')+','+ISNULL(type2,'')+','+ISNULL(type3,'')+','+ISNULL(type4,'')+','+ISNULL(type5,'')+','+ISNULL(type6,'')+','+ISNULL(type7,'') like '%Inter-Lock by pass%' then 1 else 0 end as T08
	,case when ISNULL(type1,'')+','+ISNULL(type2,'')+','+ISNULL(type3,'')+','+ISNULL(type4,'')+','+ISNULL(type5,'')+','+ISNULL(type6,'')+','+ISNULL(type7,'') like '%安全防護系統中斷/隔離作業%' then 1 else 0 end as T09
	,case when ISNULL(type1,'')+','+ISNULL(type2,'')+','+ISNULL(type3,'')+','+ISNULL(type4,'')+','+ISNULL(type5,'')+','+ISNULL(type6,'')+','+ISNULL(type7,'') like '%危險管路拆卸鑽孔作業與化學品塗佈作業%' then 1 else 0 end as T10
	,case when ISNULL(type1,'')+','+ISNULL(type2,'')+','+ISNULL(type3,'')+','+ISNULL(type4,'')+','+ISNULL(type5,'')+','+ISNULL(type6,'')+','+ISNULL(type7,'') like '%開孔/防墬安全設施拆除作業%' then 1 else 0 end as T11
	from View_VMT_FAC A where A.con_date=@Date
)
update B set B.con_date=A.con_date,B.fac_code=A.fac_code,B.fac_name=A.fac_name,B.engineer=A.engineer,B.vendor_name=A.vendor_name,B.vendor_pe=A.vendor_pe
,B.T01=A.T01,B.T02=A.T02,B.T03=A.T03,B.T04=A.T04,B.T05=A.T05,B.T06=A.T06,B.T07=A.T07,B.T08=A.T08,B.T09=A.T09,B.T10=A.T10,B.T11=A.T11
from VMT A inner join WORK_CHECKIN B on A.con_number=B.con_number
where A.con_date<>B.con_date or A.fac_code<>B.fac_code or A.fac_name<>B.fac_name or A.engineer<>B.engineer or A.vendor_name<>B.vendor_name or A.vendor_pe<>B.vendor_pe
or A.T01<>B.T01 or A.T02<>B.T02 or A.T03<>B.T03 or A.T04<>B.T04 or A.T05<>B.T05 or A.T06<>B.T06 or A.T07<>B.T07 or A.T08<>B.T08 or A.T09<>B.T09 or A.T10<>B.T10 or A.T11<>B.T11", new { Date = TimeNow.Date.AddDays(-idx) });
                }
            }
        }

        //20240611 FAC8 and FAC6  Import RecognitionAuth From AccessList For CheckIn
        private static void ImportRecognitionAuthFromAccessListForCheckin(DateTime TimeNow, SqlDapperConn oDB)
        {
            DateTime TimeS = TimeNow.Date;
            DateTime TimeE = TimeS.AddDays(1).AddSeconds(-1);
            var xData = oDB.QueryMultiple(@"
select A.fac_code as Fac,C.EV_DATE as LogDateTime,C.ID as FRUserID,C.P_NAME as FRUserName
from View_VMT_FAC A
inner join AccessListMapping B on A.fac_name=B.VNTFAC
inner join AccessList2 C on A.con_number=C.APPLY_PK and B.[LOCATION]=C.[LOCATION]
where A.con_date=@Date and C.EV_DATE between @TimeS and @TimeE and C.direction=1 and A.fac_code in ('FAC6','FAC8')
group by A.fac_code,C.ID,C.P_NAME,C.EV_DATE
;
select A.Fac,A.LogDateTime,A.FRUserID,A.FRUserName from RecognitionAuth A
where A.LogDateTime between @TimeS and @TimeE and A.Fac in ('FAC6','FAC8')", new { TimeNow.Date, TimeS, TimeE });
            //            var xData = oDB.QueryMultiple(@"
            //select A.fac_code as Fac,MIN(C.EV_DATE) as LogDateTime,C.ID as FRUserID,C.P_NAME as FRUserName
            //from View_VMT_FAC A
            //inner join AccessListMapping B on A.fac_name=B.VNTFAC
            //inner join AccessList2 C on A.con_number=C.APPLY_PK and B.[LOCATION]=C.[LOCATION]
            //where A.con_date=@Date and C.EV_DATE between @TimeS and @TimeE and C.direction=1 and A.fac_code in ('FAC6','FAC8')
            //group by A.fac_code,C.ID,C.P_NAME
            //;
            //select A.Fac,A.LogDateTime,A.FRUserID,A.FRUserName from RecognitionAuth A
            //where A.LogDateTime between @TimeS and @TimeE and A.Fac in ('FAC6','FAC8')", new { TimeNow.Date, TimeS, TimeE });

            if (xData != null)
            {
                var inList = xData.Read<CYCloud.RecognitionAuth>();
                var nowList = xData.Read<CYCloud.RecognitionAuth>();

                foreach (var oData in inList)
                {
                    if (!nowList.Any(p => p.Fac == oData.Fac && p.FRUserID == oData.FRUserID && p.LogDateTime == oData.LogDateTime))
                        oDB.Execute("insert into RecognitionAuth (Fac,LogDateTime,FRUserID,FRUserName) values (@Fac,@LogDateTime,@FRUserID,@FRUserName)", oData);
                }
            }
        }
        //20240611 FAC8 and FAC6  Import RecognitionAuth From AccessList For CheckOut
        private static void ImportRecognitionAuthFromAccessListForCheckout(DateTime TimeNow, SqlDapperConn oDB)
        {
            DateTime TimeS = TimeNow.Date;
            DateTime TimeE = TimeS.AddDays(1).AddSeconds(-1);
            var xData = oDB.QueryMultiple(@"
select A.fac_code as Fac,MAX(C.EV_DATE) as LogDateTime,C.ID as FRUserID,C.P_NAME as FRUserName
from View_VMT_FAC A
inner join AccessListMapping B on A.fac_name=B.VNTFAC
inner join AccessList2 C on A.con_number=C.APPLY_PK and B.[LOCATION]=C.[LOCATION]
where A.con_date=@Date and C.EV_DATE between @TimeS and @TimeE and C.direction=0 and A.fac_code in ('FAC6','FAC8')
group by A.fac_code,C.ID,C.P_NAME
;
select A.Fac,A.LogDateTime,A.FRUserID,A.FRUserName from RecognitionAuth A
where A.LogDateTime between @TimeS and @TimeE and A.Fac in ('FAC6','FAC8')", new { TimeNow.Date, TimeS, TimeE });

            if (xData != null)
            {
                var outList = xData.Read<CYCloud.RecognitionAuth>();
                var nowList = xData.Read<CYCloud.RecognitionAuth>();

                foreach (var oData in outList)
                {
                    if (!nowList.Any(p => p.Fac == oData.Fac && p.FRUserID == oData.FRUserID && p.LogDateTime == oData.LogDateTime))
                        oDB.Execute("insert into RecognitionAuth (Fac,LogDateTime,FRUserID,FRUserName) values (@Fac,@LogDateTime,@FRUserID,@FRUserName)", oData);
                }
            }
        }

        #region 類別定義
        class CheckOutTemp2 : RecognitionAuth
        {
            public int WORK_CHECKIN_ID { get; set; }
            public string CHECK_TYPE { get; set; }
            public DateTime? checkout_time { get; set; } //[WORK_CHECKIN].[checkout_time]
            public int SEQ_ID { get; set; }
        }
        class CheckInData
        {
            public int WORK_CHECKIN_ID { get; set; }
            public int IFP_RecognitionAuth_ID { get; set; }
            public string FRUserID { get; set; }
            public DateTime? CheckinTime { get; set; }
        }
        class CheckOutData
        {
            public int MainID { get; set; }
            public string Type { get; set; }
            public int AuthID { get; set; }
            public DateTime LogDate { get; set; }
            public string UserID { get; set; }
            public DateTime? MainCheckOut { get; set; }
            public string FAC { get; set; }
        }
        #endregion
    }

    public static class Shared
    {
        static object oLockAdd = new object();

        //建立每小時簽到退通知excel檔案
        public static byte[] WorkCheckHourFile(IEnumerable<WorkCheckHourData> xList, string sFile, string sTitle)
        {
            byte[] oFile = null;
            try
            {
                IWorkbook oBook = cyc.Shared.NPOI.GetWorkbook(sFile);
                ISheet oSheet = oBook.GetSheetAt(0);

                if (!string.IsNullOrWhiteSpace(sTitle)) 
                {
                    IRow hRow = oSheet.GetRow(0);
                    hRow.GetCell(0).SetCellValue(sTitle);
                }

                IRow tRow = oSheet.GetRow(4);//範本列
                int idx = 5;
                foreach (var oData in xList)
                {
                    IRow oRow = tRow.CopyRowTo(idx++);
                    oRow.GetCell(0).SetCellValue(oData.Vendor);
                    oRow.GetCell(1).SetCellValue(oData.ConNumber);
                    oRow.GetCell(2).SetCellValue(oData.Area);
                    oRow.GetCell(3).SetCellValue(oData.ConContent);
                    oRow.GetCell(4).SetCellValue(oData.DeptName);
                    oRow.GetCell(5).SetCellValue(oData.UserName);
                    oRow.GetCell(6).SetCellValue(oData.VendorMain);
                    oRow.GetCell(7).SetCellValue(oData.FRUserName);
                    oRow.GetCell(8).SetCellValue($"{oData.CheckInTime:yyyy/MM/dd HH:mm}");
                    if (oData.CheckOutTime != null) oRow.GetCell(9).SetCellValue($"{oData.CheckOutTime:yyyy/MM/dd HH:mm}");
                }

                //oSheet.RemoveRow(tRow);//移除範本列
                oSheet.ShiftRows(5, oSheet.LastRowNum, -1);//向上移動一列，移除範本列

                //20250106 增加 [報到人數]、[報退人數]、[尚未離廠] 統計
                if (sTitle.Contains("報到退"))
                {
                    IRow sRow = oSheet.GetRow(2);
                    sRow.GetCell(8).SetCellValue(xList.Count());
                    sRow.GetCell(9).SetCellValue(xList.Count(p => p.CheckOutTime != null));
                    sRow.GetCell(10).SetCellValue(xList.Count(p => p.CheckOutTime == null));
                }
                else
                {
                    oSheet.ShiftRows(2, oSheet.LastRowNum, -1);//向上移動一列，移除統計列標題
                }

                using (MemoryStream oStream = new MemoryStream())
                {
                    oBook.Write(oStream);
                    oFile = oStream.ToArray();
                    oStream.Close();
                }
            }
            catch { }
            return oFile;
        }

        public static WorkCheckIn GetWorkCheckInData(string ConNumber, cyc.DB.SqlDapperConn oDB)
        {
            string qSql = "select A.* from WORK_CHECKIN A where A.con_number=@ConNumber";
            WorkCheckIn xData = null;
            lock (oLockAdd)
            {
                try
                {
                    xData = oDB.QueryOne<WorkCheckIn>(qSql, new { ConNumber });
                    if (xData == null)
                    {
                        int iID = oDB.Execute("insert into WORK_CHECKIN (con_number) values (@ConNumber)", new { ConNumber }, 0);
                        if (iID > 0)
                            xData = oDB.QueryOne<WorkCheckIn>(qSql, new { ConNumber });
                    }
                }
                catch (Exception ex) { cyc.Log.WriteSysErrorLog("施工管理新增：" + ex.Message); }
            }
            return xData;
        }

        public static List<WorkCheckAuthData> GetWorkCheckAuthList(string sNumber, cyc.DB.SqlDapperConn oDB)
        {
            List<WorkCheckAuthData> oList = new List<WorkCheckAuthData>();
            var cData = Shared.GetWorkCheckInData(sNumber, oDB);
            if (cData != null && cData.con_date != null)
            {
                var xData = oDB.QueryMultiple(@"
select A.P_NAME as Name, A.SHORT_NAME as Supplier, A.EV_DATE as Date, case when Direction=1 then 1 else 0 end as InOut,'ACC' as [Type]
from AccessList2 A where A.APPLY_PK=@Number
;
select C.P_NAME as Name, C.SHORT_NAME as Supplier, LogdateTime as Date, case when B.CHECK_TYPE='CHECKIN' then 1 else 0 end as InOut,'CHK' as [Type]
from RecognitionAuth A
left join WORK_CHECKIN_LOG B on A.ID=B.IFP_RecognitionAuth_ID
inner join WORK_CHECKIN W on W.SEQ_ID = B.WORK_CHECKIN_ID 
inner join
(
	select ID,P_NAME,SHORT_NAME from AccessList2
	where EV_DATE between @DateS and @DateE
	group by ID,P_NAME,SHORT_NAME
) C on A.FRUserID=C.ID
where W.con_number=@Number Group by C.P_NAME, C.SHORT_NAME, LogdateTime,B.CHECK_TYPE
", new { Number = sNumber, DateS = cData.con_date, DateE = cData.con_date.AddDays(1).AddMilliseconds(-1) });

                if (xData != null)
                {
                    oList.AddRange(xData.Read<WorkCheckAuthData>());
                    oList.AddRange(xData.Read<WorkCheckAuthData>());
                }
            }
            return oList;
        }

        public static IEnumerable<WorkCheckInLogDetail> GetWorkCheckInLogByDate_(long ID, int WorkID, cyc.DB.SqlDapperConn oDB)
        {
            return oDB.QueryList<WorkCheckInLogDetail>(@"
            select A.ID as IFP_RecognitionAuth_ID,A.FRUserID,E.P_NAME as FRUserName,A.LogDateTime
            ,B.WORK_CHECKIN_ID,ISNULL(B.CHECK_TYPE,'')as CHECK_TYPE,B.SEQ_ID,E.SHORT_NAME as SupplierName
            from RecognitionAuth A
            left join AccessList2 E on A.FRUserID = E.ID
            inner join WORK_CHECKIN_LOG B on A.ID=B.IFP_RecognitionAuth_ID
            inner join WORK_CHECKIN C on C.SEQ_ID = B.WORK_CHECKIN_ID
            inner join AccessListMapping on E.LOCATION = AccessListMapping.LOCATION and A.Fac = AccessListMapping.FAC
            inner join View_VMT_FAC on E.APPLY_PK = View_VMT_FAC.con_number
            where A.ID =@ID and C.con_number = @WorkID
            group by A.ID, A.FRUserID, E.P_NAME, A.LogDateTime, B.WORK_CHECKIN_ID, B.CHECK_TYPE, B.SEQ_ID, E.SHORT_NAME 
            ", new { WorkID, ID });
            //return oDB.oConn.Query<WorkCheckInLogDetail>(@"
            //select A.ID as IFP_RecognitionAuth_ID,A.FRUserID,A.FRUserName,A.LogDateTime
            //,B.WORK_CHECKIN_ID,ISNULL(B.CHECK_TYPE,'')as CHECK_TYPE,B.SEQ_ID,F.[Name] as SupplierName
            //,case when A.DeviceName in @DeviceIn then 1 else 0 end as DeviceIn
            //,case when A.DeviceName in @DeviceOut then 1 else 0 end as DeviceOut
            //from IFP_RecognitionAuth A
            //inner join IFP_SupplierDriver E on (A.FRUserID=E.Code) and (E.StopDate is null or E.StopDate>=A.LogDateTime)
            //left join IFP_Supplier F on E.SupplierID=F.ID
            //left join WORK_CHECKIN_LOG B on A.ID=B.IFP_RecognitionAuth_ID
            //where A.LogDateTime between @DateS and @DateE and A.LogContent in @Code and A.DeviceName in @DeviceInOut
            //", new { DateS, DateE, Code = WorkCheckInCode, DeviceIn = DeviceCheckIn, DeviceOut = DeviceCheckOut, DeviceInOut = DeviceCheckIn.Concat(DeviceCheckOut) });
        }

        public static IEnumerable<WorkCheckInLogDetail> GetWorkCheckInLogByConNumber(string ConNumber, cyc.DB.SqlDapperConn oDB)
        {
            return oDB.QueryList<WorkCheckInLogDetail>(@"
select C.ID as IFP_RecognitionAuth_ID,C.FRUserID,C.FRUserName,C.LogDateTime
from WORK_CHECKIN A
inner join WORK_CHECKIN_LOG B on A.SEQ_ID=B.WORK_CHECKIN_ID
inner join RecognitionAuth C on B.IFP_RecognitionAuth_ID=C.ID
where A.con_number=@ConNumber and B.CHECK_TYPE='CHECKIN'
            ", new { ConNumber });
        }

        public static IEnumerable<WorkCheckInLogDetail> GetWorkCheckInLogByDate(DateTime dDate, cyc.DB.SqlDapperConn oDB, string Fac)
        {
            DateTime DateS = dDate, DateE = dDate.AddDays(1).AddMilliseconds(-1);
            return oDB.QueryList<WorkCheckInLogDetail>(@"
select A.ID as IFP_RecognitionAuth_ID,A.FRUserID,B.P_NAME as FRUserName,A.LogDateTime,B.SHORT_NAME as SupplierName
from RecognitionAuth A
left join AccessList2 B on A.FRUserID = B.ID
inner join AccessListMapping M on B.LOCATION = M.LOCATION and A.Fac = M.FAC
where A.LogDateTime between @DateS and @DateE and M.VNTFAC = @Fac
and B.EV_DATE between @DateS and @DateE
group by A.ID,A.FRUserID,B.P_NAME,A.LogDateTime,B.SHORT_NAME
            ", new { DateS, DateE, Fac });
        }

        public static IEnumerable<WorkCheckInLogDetail> GetWorkCheckInLogForLogout(string ConNumber, DateTime cDate, string Fac, cyc.DB.SqlDapperConn oDB)
        {
            return oDB.QueryList<WorkCheckInLogDetail>(@"
select A.ID as IFP_RecognitionAuth_ID,A.FRUserID,E.P_NAME as FRUserName,A.LogDateTime
,B.WORK_CHECKIN_ID,ISNULL(B.CHECK_TYPE,'')as CHECK_TYPE,B.SEQ_ID,E.SHORT_NAME as SupplierName
from RecognitionAuth A
inner join WORK_CHECKIN_LOG B on A.ID=B.IFP_RecognitionAuth_ID
inner join WORK_CHECKIN C on C.SEQ_ID = B.WORK_CHECKIN_ID
left join 
(
	select ID,P_NAME,SHORT_NAME,[LOCATION] from AccessList2
	where EV_DATE between @DateS and @DateE
	group by ID,P_NAME,SHORT_NAME,[LOCATION]
) E on A.FRUserID=E.ID
inner join AccessListMapping M on E.[LOCATION] = M.[LOCATION] and A.Fac = M.FAC
where  C.con_number = @ConNumber and M.VNTFAC = @Fac
group by A.ID, A.FRUserID, E.P_NAME, A.LogDateTime, B.WORK_CHECKIN_ID, B.CHECK_TYPE, B.SEQ_ID,E.SHORT_NAME
            ", new { ConNumber, Fac, DateS = cDate, DateE = cDate.AddDays(1).AddMilliseconds(-1) });
        }

        //public static IEnumerable<WorkCheckInLogDetail> GetWorkCheckInLogByDate__(int WorkID, cyc.DB.SqlDapperConn oDB, string Fac)
        //{

        //    return oDB.QueryList<WorkCheckInLogDetail>(@"
        //    select A.ID as IFP_RecognitionAuth_ID,A.FRUserID,E.P_NAME as FRUserName,A.LogDateTime
        //    ,B.WORK_CHECKIN_ID,ISNULL(B.CHECK_TYPE,'')as CHECK_TYPE,B.SEQ_ID,E.SHORT_NAME as SupplierName
        //    from RecognitionAuth A
        //    inner join WORK_CHECKIN_LOG B on A.ID=B.IFP_RecognitionAuth_ID
        //    inner join WORK_CHECKIN C on C.SEQ_ID = B.WORK_CHECKIN_ID
        //    left join AccessList2 E on A.FRUserID = E.ID
        //    inner join AccessListMapping on E.LOCATION = AccessListMapping.LOCATION and A.Fac = AccessListMapping.FAC
        //    where  C.con_number = @WorkID and AccessListMapping.VNTFAC = @Fac
        //    group by A.ID, A.FRUserID, E.P_NAME, A.LogDateTime, B.WORK_CHECKIN_ID, B.CHECK_TYPE, B.SEQ_ID,E.SHORT_NAME
        //    ", new { WorkID, Fac });
        //    //return oDB.oConn.Query<WorkCheckInLogDetail>(@"
        //    //select A.ID as IFP_RecognitionAuth_ID,A.FRUserID,A.FRUserName,A.LogDateTime
        //    //,B.WORK_CHECKIN_ID,ISNULL(B.CHECK_TYPE,'')as CHECK_TYPE,B.SEQ_ID,F.[Name] as SupplierName
        //    //,case when A.DeviceName in @DeviceIn then 1 else 0 end as DeviceIn
        //    //,case when A.DeviceName in @DeviceOut then 1 else 0 end as DeviceOut
        //    //from IFP_RecognitionAuth A
        //    //inner join IFP_SupplierDriver E on (A.FRUserID=E.Code) and (E.StopDate is null or E.StopDate>=A.LogDateTime)
        //    //left join IFP_Supplier F on E.SupplierID=F.ID
        //    //left join WORK_CHECKIN_LOG B on A.ID=B.IFP_RecognitionAuth_ID
        //    //where A.LogDateTime between @DateS and @DateE and A.LogContent in @Code and A.DeviceName in @DeviceInOut
        //    //", new { DateS, DateE, Code = WorkCheckInCode, DeviceIn = DeviceCheckIn, DeviceOut = DeviceCheckOut, DeviceInOut = DeviceCheckIn.Concat(DeviceCheckOut) });
        //}

        public static IEnumerable<WorkCheckOutLogDetail> GetWorkCheckOutLogByDate(string ConNumber, cyc.DB.SqlDapperConn oDB, string Fac, DateTime dDate)
        {
            return oDB.QueryList<WorkCheckOutLogDetail>(@"
with cte as (
	select B.WORK_CHECKIN_ID,B.CHECK_TYPE,C.ID as IFP_RecognitionAuth_ID,C.Fac,C.FRUserID,C.FRUserName,C.LogDateTime,D.SHORT_NAME as SupplierName
	from WORK_CHECKIN A 
	inner join WORK_CHECKIN_LOG B on A.SEQ_ID=B.WORK_CHECKIN_ID 
	inner join RecognitionAuth C on B.IFP_RecognitionAuth_ID=C.ID
	left join AccessList2 D on A.con_number=D.APPLY_PK and C.FRUserID=D.ID
	where A.con_number=@ConNumber
)
select A.IFP_RecognitionAuth_ID,A.FRUserID,A.FRUserName,A.LogDateTime,A.CHECK_TYPE,A.SupplierName
from cte A where A.CHECK_TYPE='CHECKOUT'
union
select A.ID as IFP_RecognitionAuth_ID,A.FRUserID,A.FRUserName,A.LogDateTime,'' as CHECK_TYPE,B.SupplierName
from RecognitionAuth A
inner join cte B on A.Fac=B.Fac and A.FRUserID=B.FRUserID and B.CHECK_TYPE='CHECKIN'
where A.LogDateTime between @DateS and @DateE
and A.ID not in (select IFP_RecognitionAuth_ID from cte)", new { ConNumber, DateS = dDate.Date, DateE = dDate.Date.AddDays(1).AddSeconds(-1) });
        }
        
        //public static IEnumerable<string> GetWorkCheckInSupplier(DateTime dDate, string facName, cyc.DB.SqlDapperConn oDB)
        //{
        //    DateTime DateS = dDate, DateE = dDate.AddDays(1).AddMilliseconds(-1);

        //    return oDB.Connection.Query<string>(@"
        //    select distinct A.SHORT_NAME from RecognitionAuth C
        //    left join AccessList2 A on A.ID = C.FRUserID
        //    inner join AccessListMapping B on A.LOCATION = B.LOCATION and C.Fac = B.FAC
        //    where A.EV_DATE between @DateS and @DateE and B.VNTFAC = @facName
        //    ", new { DateS, DateE, facName });
        //}

        #region 自動報到+報退 Backup
        //#region 20240612 Backup
        ////        public static void AutoCheckOut(DateTime TimeNow)
        ////        {
        ////            //if (sWorkAutoCheckOut != "1") { return; }
        ////            DateTime DateS = TimeNow.Date;
        ////            DateTime DateE = DateS.AddDays(1).AddSeconds(-1);

        ////            using (SqlDapperConn oDB = new SqlDapperConn(null, "", false, 60))
        ////            {
        ////                var xList = oDB.QueryList<CheckOutTemp>(@"
        ////with cte as (
        ////	select A.ID,A.Fac,A.LogDateTime,A.FRUserID,B.WORK_CHECKIN_ID,B.CHECK_TYPE,C.checkin_time,C.checkout_time
        ////	from RecognitionAuth A
        ////	inner join WORK_CHECKIN_LOG B on A.ID=B.IFP_RecognitionAuth_ID
        ////	inner join WORK_CHECKIN C on B.WORK_CHECKIN_ID=C.SEQ_ID
        ////	where A.LogDateTime between @DateS and @DateE
        ////), cte2 as (
        ////	select * from RecognitionAuth A where A.LogDateTime between @DateS and @DateE and A.UseFlag='N'
        ////)
        ////select A.WORK_CHECKIN_ID,A.checkout_time,A.Fac,A.ID as InID,A.LogDateTime as InTime,A.FRUserID,B.ID as OutID,B.LogDateTime as OutTime,C.ID as [NewID],C.LogDateTime as NewTime
        ////from cte A 
        ////left join cte B on A.WORK_CHECKIN_ID=B.WORK_CHECKIN_ID and A.FRUserID=B.FRUserID and B.CHECK_TYPE='CHECKOUT'
        ////left join cte2 C on A.FRUserID=C.FRUserID and A.Fac=C.Fac and DATEADD(mi,10,A.LogDateTime)<C.LogDateTime
        ////where A.CHECK_TYPE='CHECKIN' and B.ID is null and C.ID is not null
        ////order by A.WORK_CHECKIN_ID,A.ID,C.LogDateTime", new { DateS, DateE });

        ////                if (xList != null && xList.Any())
        ////                {
        ////                    //依[工單]分組
        ////                    foreach (var gData in xList.GroupBy(p => p.WORK_CHECKIN_ID))
        ////                    {
        ////                        if (gData.First().checkout_time == null)
        ////                            oDB.Execute("update WORK_CHECKIN set checkout_time=@TimeNow where SEQ_ID=@WORK_CHECKIN_ID and checkout_time is null", new { TimeNow, gData.First().WORK_CHECKIN_ID });

        ////                        int CheckInID = 0; //[WORK_CHECKIN_LOG].[IFP_RecognitionAuth_ID]
        ////                        foreach (var xData in gData)
        ////                        {
        ////                            if (CheckInID != xData.InID) //一筆checkin => 一筆checkout
        ////                            {
        ////                                CheckInID = xData.InID;
        ////                                oDB.Execute(@"insert into WORK_CHECKIN_LOG (WORK_CHECKIN_ID,CHECK_TYPE,IFP_RecognitionAuth_ID) values (@WORK_CHECKIN_ID,'CHECKOUT',@ID)", new { xData.WORK_CHECKIN_ID, ID = xData.NewID });
        ////                            }
        ////                            //更新註記
        ////                            oDB.Execute("update RecognitionAuth set UseFlag='Y' where ID=@ID and UseFlag<>'Y'", new { ID = xData.NewID });
        ////                        }
        ////                    }
        ////                }
        ////            }
        ////        }
        //#endregion

        //#region "Backup AutoCheckIn & AutoCheckOut"
        ////   public static void AutoCheckIn(List<IFP.RecognitionAuth> AuthList)
        ////   {
        ////       if (sWorkAutoCheckIn != "1") { return; }
        ////       var WorkList = AuthList.OrderBy(p => p.LogDateTime);

        ////       if (WorkList != null && WorkList.Count() > 0)
        ////       {
        ////           try
        ////           {
        ////               using (SqlDapperConn oDB = new SqlDapperConn(null, "", false, 300))
        ////               {
        ////                   foreach (var WorkData in WorkList)
        ////                   {
        ////                       var ExistList = oDB.Connection.Query<CheckInData>(@"
        ////   select distinct WORK_CHECKIN.SEQ_ID as WORK_CHECKIN_ID, RecognitionAuth.ID as IFP_RecognitionAuth_ID, FRUserID From WORK_CHECKIN 
        ////   Left Join AccessList2 on WORK_CHECKIN.con_number = AccessList2.APPLY_PK 
        ////   inner join RecognitionAuth on RecognitionAuth.FRUserID = AccessList2.ID 
        ////   inner join AccessListMapping on AccessList2.LOCATION = AccessListMapping.LOCATION and RecognitionAuth.Fac = AccessListMapping.FAC
        ////   inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number and View_VMT_FAC.fac_name = AccessListMapping.VNTFAC
        ////   where  Direction = 1 and RecognitionAuth.ID = @ID and RecognitionAuth.Fac = @Fac and RecognitionAuth.LogDateTime >= AccessList2.EV_DATE 
        ////   and View_VMT_FAC.con_date >= convert(varchar(100),GETDATE(),23) + ' 00:00:00'", new { ID = WorkData.ID, Fac = WorkData.Fac }).ToList();
        ////                       foreach (var inData in ExistList)
        ////                       {
        ////                           oDB.Connection.Execute("update WORK_CHECKIN set checkin_time=@Date, update_time = @Date, update_user = 0 where SEQ_ID=@ID and checkin_time is null", new { ID = inData.WORK_CHECKIN_ID, Date = DateTime.Now });
        ////                           var dList = oDB.Connection.Query<CYCloud.WorkCheck.WorkConNumber>("Select count(*) as count From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN_LOG.WORK_CHECKIN_ID = WORK_CHECKIN.SEQ_ID inner join RecognitionAuth on WORK_CHECKIN_LOG.IFP_RecognitionAuth_ID = RecognitionAuth.ID  Where RecognitionAuth.FRUserID = @FRUserID and WORK_CHECKIN.SEQ_ID = @WORK_CHECKIN_ID", new { FRUserID = inData.FRUserID, WORK_CHECKIN_ID = inData.WORK_CHECKIN_ID }).ToList();
        ////                           foreach (var ddata in dList)
        ////                           {
        ////                               if (ddata.Count < 1)
        ////                               {
        ////                                   oDB.Connection.Execute(@"insert into WORK_CHECKIN_LOG (WORK_CHECKIN_ID,CHECK_TYPE,IFP_RecognitionAuth_ID,update_time,update_user) values( @WORK_CHECKIN_ID, 'CHECKIN', @IFP_RecognitionAuth_ID, @Date, @update_user )", new { work_checkin_ID = inData.WORK_CHECKIN_ID, IFP_RecognitionAuth_ID = inData.IFP_RecognitionAuth_ID, update_user = 0, Date = DateTime.Now });
        ////                                   oDB.Connection.Execute(@"Update RecognitionAuth  set UseFlag = 'Y' Where ID = @IFP_RecognitionAuth_ID", new { IFP_RecognitionAuth_ID = inData.IFP_RecognitionAuth_ID });
        ////                               }
        ////                           }
        ////                       }
        ////                   }
        ////               }
        ////           }
        ////           catch (Exception ex) { cyc.Log.WriteSysErrorLog("自動簽到：" + ex.Message); }
        ////       }
        ////   }

        ////   public static void AutoCheckOut(List<CYCloud.IFP.RecognitionAuth> AuthList)
        ////   {
        ////       if (sWorkAutoCheckOut != "1") { return; }

        ////       //篩選 符合Checkout資料
        ////       var WorkList = AuthList.OrderBy(p => p.LogDateTime);

        ////       if (WorkList != null && WorkList.Count() > 0)
        ////       {
        ////           try
        ////           {
        ////               DateTime DateS = DateTime.Today, DateE = DateS.AddDays(1).AddMilliseconds(-1);
        ////               using (cyc.DB.SqlDapperConn oDB = new SqlDapperConn())
        ////               {
        ////                   var ExistList = oDB.Connection.Query<CheckOutData>(@"
        ////               select A.SEQ_ID as MainID,A.checkout_time as MainCheckOut,B.CHECK_TYPE as Type,B.IFP_RecognitionAuth_ID as AuthID,C.LogDateTime as LogDate,C.FRUserID as UserID, View_WORKLOG.Fac 
        ////               from WORK_CHECKIN A
        ////inner join View_WORKLOG on A.con_number = View_WORKLOG.con_number 
        ////               inner join WORK_CHECKIN_LOG B on A.SEQ_ID=b.WORK_CHECKIN_ID
        ////               inner join RecognitionAuth C on B.IFP_RecognitionAuth_ID=C.ID
        ////               where checkin_time between @DateS and @DateE ", new { DateS, DateE }).ToList();
        ////                   var OutList = new List<WorkCheckInLog>();

        ////                   foreach (var WorkData in WorkList)
        ////                   {
        ////                       //        var ExistList = oDB.oConn.Query<CheckOutData>(@"
        ////                       //select A.SEQ_ID as MainID,A.checkout_time as MainCheckOut,B.CHECK_TYPE as Type,B.IFP_RecognitionAuth_ID as AuthID,C.LogDateTime as LogDate,C.FRUserID as UserID
        ////                       //from WORK_CHECKIN A
        ////                       //inner join WORK_CHECKIN_LOG B on A.SEQ_ID=b.WORK_CHECKIN_ID
        ////                       //inner join IFP_RecognitionAuth C on B.IFP_RecognitionAuth_ID=C.ID
        ////                       //where C.LogContent = 'Face Identify Pass' and C.FRUserID = @ID", new {ID = WorkData.FRUserID}).ToList();
        ////                       foreach (var InData in ExistList.Where(p => p.Type == "CHECKIN" && p.UserID.Trim() == WorkData.FRUserID.Trim() && p.LogDate.AddMinutes(10) < WorkData.LogDateTime && p.FAC == WorkData.Fac))
        ////                       {
        ////                           if (!ExistList.Any(p => p.MainID == InData.MainID && p.Type == "CHECKOUT" && p.UserID.Trim() == WorkData.FRUserID.Trim()))
        ////                           {
        ////                               var dList = oDB.Connection.Query<CYCloud.WorkCheck.WorkConNumber>("Select count(*) as count From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN_LOG.WORK_CHECKIN_ID = WORK_CHECKIN.SEQ_ID inner join RecognitionAuth on WORK_CHECKIN_LOG.IFP_RecognitionAuth_ID = RecognitionAuth.ID  Where RecognitionAuth.FRUserID = @FRUserID and WORK_CHECKIN.SEQ_ID = @WORK_CHECKIN_ID and WORK_CHECKIN_LOG.CHECK_TYPE = 'CHECKOUT'", new { FRUserID = InData.UserID, WORK_CHECKIN_ID = InData.MainID }).ToList();
        ////                               foreach (var ddata in dList)
        ////                               {
        ////                                   if (ddata.Count < 1)
        ////                                   {
        ////                                       oDB.Connection.Execute(@"
        ////                               insert into WORK_CHECKIN_LOG (WORK_CHECKIN_ID,CHECK_TYPE,IFP_RecognitionAuth_ID,update_time,update_user)
        ////                               values (@WORK_CHECKIN_ID,@CHECK_TYPE,@IFP_RecognitionAuth_ID,getdate(),@update_user)", new { WORK_CHECKIN_ID = InData.MainID, CHECK_TYPE = "CHECKOUT", IFP_RecognitionAuth_ID = WorkData.ID, update_user = 0 });

        ////                                       //OutList.Add(new WorkCheckInLog() { WORK_CHECKIN_ID = InData.MainID, CHECK_TYPE = "CHECKOUT", IFP_RecognitionAuth_ID = WorkData.ID, update_user = 0 });
        ////                                   }
        ////                               }

        ////                               if (InData.MainCheckOut == null)//如果沒CheckOut
        ////                               {
        ////                                   //將所有相同WorkCheckIn的
        ////                                   foreach (var m in ExistList.Where(p => p.MainID == InData.MainID)) { m.MainCheckOut = DateTime.Now; }
        ////                                   oDB.Connection.Execute("update WORK_CHECKIN set checkout_time=@Date where SEQ_ID=@ID and checkout_time is null", new { ID = InData.MainID, Date = DateTime.Now });
        ////                                   //                oDB.oConn.Execute(@"Update RecognitionAuth  set UseFlag = 'Y' Where ID = @IFP_RecognitionAuth_ID", new { IFP_RecognitionAuth_ID = WorkData.ID });
        ////                                   //                oDB.oConn.Execute(@"
        ////                                   //insert into WORK_CHECKIN_LOG (WORK_CHECKIN_ID,CHECK_TYPE,IFP_RecognitionAuth_ID,update_time,update_user)
        ////                                   //values (@WORK_CHECKIN_ID,@CHECK_TYPE,@IFP_RecognitionAuth_ID,getdate(),@update_user)", new { WORK_CHECKIN_ID = InData.MainID, CHECK_TYPE = "CHECKOUT", IFP_RecognitionAuth_ID = WorkData.ID, update_user = 0 });

        ////                               }

        ////                               //oDB.oConn.Execute(@"Update RecognitionAuth  set UseFlag = 'Y' Where ID = @IFP_RecognitionAuth_ID", new { IFP_RecognitionAuth_ID = WorkData.ID });
        ////                               //oDB.oConn.Execute(@"
        ////                               //insert into WORK_CHECKIN_LOG (WORK_CHECKIN_ID,CHECK_TYPE,IFP_RecognitionAuth_ID,update_time,update_user)
        ////                               //values (@WORK_CHECKIN_ID,@CHECK_TYPE,@IFP_RecognitionAuth_ID,getdate(),@update_user)", new { WORK_CHECKIN_ID = InData.MainID, CHECK_TYPE = "CHECKOUT", IFP_RecognitionAuth_ID = WorkData.ID, update_user = 0 });

        ////                               //var dList = oDB.oConn.Query<CYCloud.WorkCheck.WorkConNumber>("Select count(*) as count From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN_LOG.WORK_CHECKIN_ID = WORK_CHECKIN.SEQ_ID inner join IFP_RecognitionAuth on WORK_CHECKIN_LOG.IFP_RecognitionAuth_ID = IFP_RecognitionAuth.ID  Where IFP_RecognitionAuth.FRUserID = @FRUserID and WORK_CHECKIN.SEQ_ID = @WORK_CHECKIN_ID", new { FRUserID = InData.UserID, WORK_CHECKIN_ID = InData.MainID }).ToList();
        ////                               //foreach (var ddata in dList)
        ////                               //{
        ////                               //    if (ddata.Count > 0)
        ////                               //    {
        ////                               //        ExistList.Remove(InData);//移除，避免重複
        ////                               //    }
        ////                               //}
        ////                               //break;//有找到就離開foreach
        ////                           }
        ////                       }
        ////                       oDB.Connection.Execute(@"Update RecognitionAuth  set UseFlag2 = 'Y' Where ID = @IFP_RecognitionAuth_ID", new { IFP_RecognitionAuth_ID = WorkData.ID });
        ////                   }
        ////               }

        ////           }
        ////           catch (Exception ex) { cyc.Log.WriteSysErrorLog("自動簽退：" + ex.Message); }
        ////       }
        ////   }
        //#endregion
        #endregion

        public static void WorkCheckReport()
        {
            SqlConnection gConn = new SqlConnection(cyc.DB.ConnString.Main);
            DataTable dt;
            DataTable dt2;
            try
            {
                SqlDataAdapter da = new SqlDataAdapter();
                if (gConn.State == ConnectionState.Closed) { gConn.Open(); }

                string sToday = DateTime.Now.ToString("yyyy/MM/dd");
                #region "FAC1"
                string sql = $@"select 'FAC1', (select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date>='{DateTime.Now:yyyy/MM/dd} 00:00:00' and con_date<='{DateTime.Now:yyyy/MM/dd} 23:59:59' and fac_name = 'FAB1廠') as FAC1施工總數,(select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date>='{DateTime.Now:yyyy/MM/dd} 00:00:00' and con_date<='{DateTime.Now:yyyy/MM/dd} 23:59:59' and fac_name = 'FAB1廠') as FAC1刷臉報到總數, round(cast((select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date>='{DateTime.Now:yyyy/MM/dd} 00:00:00' and con_date<='{DateTime.Now:yyyy/MM/dd} 23:59:59' and fac_name = 'FAB1廠') as Float) / cast ((select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date>='{DateTime.Now:yyyy/MM/dd} 00:00:00' and con_date<='{DateTime.Now:yyyy/MM/dd} 23:59:59' and fac_name = 'FAB1廠') as float) ,2)";
                da = new SqlDataAdapter(sql, gConn);
                da.SelectCommand.CommandType = CommandType.Text;
                dt = new DataTable();
                da.Fill(dt);
                sql = "Select vendor_name,Count(distinct AccessList2.ID), count(distinct RecognitionAuth.FRUserID), count(distinct View_VMT_FAC.con_number), count(distinct WORK_CHECKIN.con_number)," +
                     "  round (cast(count(distinct WORK_CHECKIN.con_number) as float) / nullif(count(distinct View_VMT_FAC.con_number),0), 2), " +
 " isnull(round(cast(count(distinct RecognitionAuth.FRUserID) as float) / nullif(Count(distinct AccessList2.ID), 0), 2),0) From View_VMT_FAC " +
                     "left join AccessList2 on AccessList2.APPLY_PK = View_VMT_FAC.con_number " +
                     "inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name " +
                     "left join WORK_CHECKIN on WORK_CHECKIN.con_number = View_VMT_FAC.con_number and WORK_CHECKIN.checkin_time is not null " +
                     "left join WORK_CHECKIN_LOG on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID " +
                     "left join RecognitionAuth on RecognitionAuth.ID = WORK_CHECKIN_LOG.IFP_RecognitionAuth_ID and RecognitionAuth.FRUserID = AccessList2.ID " +
                     " Where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 18:00:00" + @"' and fac_name = 'FAB1廠' group by vendor_name order by count(distinct WORK_CHECKIN.con_number) desc";
                da = new SqlDataAdapter(sql, gConn);
                da.SelectCommand.CommandType = CommandType.Text;
                dt2 = new DataTable();
                da.Fill(dt2);
                IWorkbook wk = cyc.Shared.NPOI.GetWorkbook(@"D:\中央施工管理系統\ReportTemplates\施工管理報表.xlsx");
                ISheet sheet1 = wk.GetSheet("Sheet1");
                sheet1.GetRow(0).CreateCell(1).SetCellValue(dt.Rows[0][0].ToString());
                sheet1.GetRow(0).CreateCell(3).SetCellValue(int.Parse(dt.Rows[0][1].ToString()));
                sheet1.GetRow(0).CreateCell(5).SetCellValue(int.Parse(dt.Rows[0][2].ToString()));
                sheet1.GetRow(0).CreateCell(7).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt.Rows[0][3].ToString())));
                for (int i = 0; i < dt2.Rows.Count; i++)
                {
                    sheet1.GetRow(5 + i).CreateCell(0).SetCellValue(dt2.Rows[i][0].ToString());
                    sheet1.GetRow(5 + i).CreateCell(3).SetCellValue(int.Parse(dt2.Rows[i][1].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(4).SetCellValue(int.Parse(dt2.Rows[i][2].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(1).SetCellValue(int.Parse(dt2.Rows[i][3].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(2).SetCellValue(int.Parse(dt2.Rows[i][4].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(5).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt2.Rows[i][5].ToString())));
                    sheet1.GetRow(5 + i).CreateCell(6).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt2.Rows[i][6].ToString())));

                }
                sheet1.ForceFormulaRecalculation = true;
                string sFileName = @"D:\中央施工管理系統\INX_WorkChecking\File\FAB1施工管理報表_" + DateTime.Now.ToString("yyyy_MM_dd") + @".xlsx";
                using (FileStream fileStream = File.Open(sFileName, FileMode.Create, FileAccess.ReadWrite))
                {
                    wk.Write(fileStream);
                    fileStream.Close();
                }

                SqlCommand sqlCommand = gConn.CreateCommand();
                byte[] MappFileNameByte = File.ReadAllBytes(sFileName);
                sql = @"insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                                'FAC1_HVAC', 'FAB1施工管理報表', '3', 'FAB1施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
                                insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                                'FAC1_WATER', 'FAB1施工管理報表', '3', 'FAB1施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
                                insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                                'FAC1_GAS', 'FAB1施工管理報表', '3', 'FAB1施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
                                insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                                'FAC1_POWER', 'FAB1施工管理報表', '3', 'FAB1施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);";
                sqlCommand = gConn.CreateCommand();
                sqlCommand.Parameters.Add("@MappFileNameByte", SqlDbType.Binary, MappFileNameByte.Length).Value = MappFileNameByte;
                sqlCommand.Parameters.Add("@MM_FILE_SHOW_NAME", SqlDbType.VarChar, 50).Value = "FAB1施工報表_" + DateTime.Now.ToString("yyyy_MM_dd") + @".xlsx";
                sqlCommand.CommandText = sql;
                sqlCommand.ExecuteNonQuery();
                #endregion

                #region "FAC2"
                sql = @"select 'FAC2', (select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + @"' and fac_name = 'FAB2廠') as FAC2施工總數, 
                        (select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + "' and fac_name = 'FAB2廠') as FAC2刷臉報到總數, " +
                               "round(cast((select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + "' and fac_name = 'FAB2廠') as Float) / cast ((select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + @"' and fac_name = 'FAB2廠') as float) ,2)";
                da = new SqlDataAdapter(sql, gConn);
                da.SelectCommand.CommandType = CommandType.Text;
                dt = new DataTable();
                da.Fill(dt);
                sql = "Select vendor_name,Count(distinct AccessList2.ID), count(distinct RecognitionAuth.FRUserID), count(distinct View_VMT_FAC.con_number), count(distinct WORK_CHECKIN.con_number)," +
                     "  round (cast(count(distinct WORK_CHECKIN.con_number) as float) / nullif(count(distinct View_VMT_FAC.con_number),0), 2), " +
 " isnull(round(cast(count(distinct RecognitionAuth.FRUserID) as float) / nullif(Count(distinct AccessList2.ID), 0), 2),0) From View_VMT_FAC " +
                     "left join AccessList2 on AccessList2.APPLY_PK = View_VMT_FAC.con_number " +
                     "inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name " +
                     "left join WORK_CHECKIN on WORK_CHECKIN.con_number = View_VMT_FAC.con_number and WORK_CHECKIN.checkin_time is not null " +
                     "left join WORK_CHECKIN_LOG on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID " +
                     "left join RecognitionAuth on RecognitionAuth.ID = WORK_CHECKIN_LOG.IFP_RecognitionAuth_ID and RecognitionAuth.FRUserID = AccessList2.ID " +
                     " Where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 18:00:00" + @"' and fac_name = 'FAB2廠' group by vendor_name order by count(distinct WORK_CHECKIN.con_number) desc";
                da = new SqlDataAdapter(sql, gConn);
                da.SelectCommand.CommandType = CommandType.Text;
                dt2 = new DataTable();
                da.Fill(dt2);
                wk = cyc.Shared.NPOI.GetWorkbook(@"D:\中央施工管理系統\ReportTemplates\施工管理報表.xlsx");
                sheet1 = wk.GetSheet("Sheet1");
                sheet1.GetRow(0).CreateCell(1).SetCellValue(dt.Rows[0][0].ToString());
                sheet1.GetRow(0).CreateCell(3).SetCellValue(int.Parse(dt.Rows[0][1].ToString()));
                sheet1.GetRow(0).CreateCell(5).SetCellValue(int.Parse(dt.Rows[0][2].ToString()));
                sheet1.GetRow(0).CreateCell(7).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt.Rows[0][3].ToString())));
                for (int i = 0; i < dt2.Rows.Count; i++)
                {
                    sheet1.GetRow(5 + i).CreateCell(0).SetCellValue(dt2.Rows[i][0].ToString());
                    sheet1.GetRow(5 + i).CreateCell(3).SetCellValue(int.Parse(dt2.Rows[i][1].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(4).SetCellValue(int.Parse(dt2.Rows[i][2].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(1).SetCellValue(int.Parse(dt2.Rows[i][3].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(2).SetCellValue(int.Parse(dt2.Rows[i][4].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(5).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt2.Rows[i][5].ToString())));
                    sheet1.GetRow(5 + i).CreateCell(6).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt2.Rows[i][6].ToString())));

                }
                sheet1.ForceFormulaRecalculation = true;
                sFileName = @"D:\中央施工管理系統\INX_WorkChecking\File\FAB2施工管理報表_" + DateTime.Now.ToString("yyyy_MM_dd") + @".xlsx";
                using (FileStream fileStream = File.Open(sFileName, FileMode.Create, FileAccess.ReadWrite))
                {
                    wk.Write(fileStream);
                    fileStream.Close();
                }
                sqlCommand = gConn.CreateCommand();
                MappFileNameByte = File.ReadAllBytes(sFileName);
                sql = @"insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                                'FAC2_HVAC', 'FAB2施工管理報表', '3', 'FAB2施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
                                insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                                'FAC2_WATER', 'FAB2施工管理報表', '3', 'FAB2施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
                                insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                                'FAC2_GAS', 'FAB2施工管理報表', '3', 'FAB2施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
                                insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                                'FAC2_POWER', 'FAB2施工管理報表', '3', 'FAB2施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);";
                sqlCommand = gConn.CreateCommand();
                sqlCommand.Parameters.Add("@MappFileNameByte", SqlDbType.Binary, MappFileNameByte.Length).Value = MappFileNameByte;
                sqlCommand.Parameters.Add("@MM_FILE_SHOW_NAME", SqlDbType.VarChar, 50).Value = "FAB2施工報表_" + DateTime.Now.ToString("yyyy_MM_dd") + @".xlsx";
                sqlCommand.CommandText = sql;
                sqlCommand.ExecuteNonQuery();
                #endregion

                #region "FAC3"
                sql = @"select 'FAC3', (select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + @"' and fac_name = 'FAB3廠') as FAC3施工總數, 
                        (select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + "' and fac_name = 'FAB3廠') as FAC3刷臉報到總數, " +
               "round(cast((select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + "' and fac_name = 'FAB3廠') as Float) / cast ((select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + @"' and fac_name = 'FAB3廠') as float) ,2)";
                da = new SqlDataAdapter(sql, gConn);
                da.SelectCommand.CommandType = CommandType.Text;
                dt = new DataTable();
                da.Fill(dt);
                sql = "Select vendor_name,Count(distinct AccessList2.ID), count(distinct RecognitionAuth.FRUserID), count(distinct View_VMT_FAC.con_number), count(distinct WORK_CHECKIN.con_number)," +
                     "  round (cast(count(distinct WORK_CHECKIN.con_number) as float) / nullif(count(distinct View_VMT_FAC.con_number),0), 2), " +
 " isnull(round(cast(count(distinct RecognitionAuth.FRUserID) as float) / nullif(Count(distinct AccessList2.ID), 0), 2),0) From View_VMT_FAC " +
                     "left join AccessList2 on AccessList2.APPLY_PK = View_VMT_FAC.con_number " +
                     "inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name " +
                     "left join WORK_CHECKIN on WORK_CHECKIN.con_number = View_VMT_FAC.con_number and WORK_CHECKIN.checkin_time is not null " +
                     "left join WORK_CHECKIN_LOG on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID " +
                     "left join RecognitionAuth on RecognitionAuth.ID = WORK_CHECKIN_LOG.IFP_RecognitionAuth_ID and RecognitionAuth.FRUserID = AccessList2.ID " +
                     " Where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 18:00:00" + @"' and fac_name = 'FAB3廠' group by vendor_name order by count(distinct WORK_CHECKIN.con_number) desc";
                da = new SqlDataAdapter(sql, gConn);
                da.SelectCommand.CommandType = CommandType.Text;
                dt2 = new DataTable();
                da.Fill(dt2);
                wk = cyc.Shared.NPOI.GetWorkbook(@"D:\中央施工管理系統\ReportTemplates\施工管理報表.xlsx");
                sheet1 = wk.GetSheet("Sheet1");
                sheet1.GetRow(0).CreateCell(1).SetCellValue(dt.Rows[0][0].ToString());
                sheet1.GetRow(0).CreateCell(3).SetCellValue(int.Parse(dt.Rows[0][1].ToString()));
                sheet1.GetRow(0).CreateCell(5).SetCellValue(int.Parse(dt.Rows[0][2].ToString()));
                sheet1.GetRow(0).CreateCell(7).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt.Rows[0][3].ToString())));
                for (int i = 0; i < dt2.Rows.Count; i++)
                {
                    sheet1.GetRow(5 + i).CreateCell(0).SetCellValue(dt2.Rows[i][0].ToString());
                    sheet1.GetRow(5 + i).CreateCell(3).SetCellValue(int.Parse(dt2.Rows[i][1].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(4).SetCellValue(int.Parse(dt2.Rows[i][2].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(1).SetCellValue(int.Parse(dt2.Rows[i][3].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(2).SetCellValue(int.Parse(dt2.Rows[i][4].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(5).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt2.Rows[i][5].ToString())));
                    sheet1.GetRow(5 + i).CreateCell(6).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt2.Rows[i][6].ToString())));

                }
                sheet1.ForceFormulaRecalculation = true;
                sFileName = @"D:\中央施工管理系統\INX_WorkChecking\File\FAB3施工管理報表_" + DateTime.Now.ToString("yyyy_MM_dd") + @".xlsx";
                using (FileStream fileStream = File.Open(sFileName, FileMode.Create, FileAccess.ReadWrite))
                {
                    wk.Write(fileStream);
                    fileStream.Close();
                }
                sqlCommand = gConn.CreateCommand();
                MappFileNameByte = File.ReadAllBytes(sFileName);
                sql = @"insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                                'FAC3_HVAC', 'FAB3施工管理報表', '3', 'FAB3施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
                                insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                                'FAC3_WATER', 'FAB3施工管理報表', '3', 'FAB3施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
                                insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                                'FAC3_GAS', 'FAB3施工管理報表', '3', 'FAB3施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
                                insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                                'FAC3_POWER', 'FAB3施工管理報表', '3', 'FAB3施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);";
                sqlCommand = gConn.CreateCommand();
                sqlCommand.Parameters.Add("@MappFileNameByte", SqlDbType.Binary, MappFileNameByte.Length).Value = MappFileNameByte;
                sqlCommand.Parameters.Add("@MM_FILE_SHOW_NAME", SqlDbType.VarChar, 50).Value = "FAB3施工報表_" + DateTime.Now.ToString("yyyy_MM_dd") + @".xlsx";
                sqlCommand.CommandText = sql;
                sqlCommand.ExecuteNonQuery();
                #endregion

                #region "FAC5"
                sql = @"select 'FAC5', (select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + @"' and fac_name = 'FAB5廠') as FAC5施工總數, 
                        (select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + "' and fac_name = 'FAB5廠') as FAC5刷臉報到總數, " +
               "round(cast((select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + "' and fac_name = 'FAB5廠') as Float) / cast ((select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + @"' and fac_name = 'FAB5廠') as float) ,2)";
                da = new SqlDataAdapter(sql, gConn);
                da.SelectCommand.CommandType = CommandType.Text;
                dt = new DataTable();
                da.Fill(dt);
                sql = "Select vendor_name,Count(distinct AccessList2.ID), count(distinct RecognitionAuth.FRUserID), count(distinct View_VMT_FAC.con_number), count(distinct WORK_CHECKIN.con_number)," +
                     "  round (cast(count(distinct WORK_CHECKIN.con_number) as float) / nullif(count(distinct View_VMT_FAC.con_number),0), 2), " +
 " isnull(round(cast(count(distinct RecognitionAuth.FRUserID) as float) / nullif(Count(distinct AccessList2.ID), 0), 2),0) From View_VMT_FAC " +
                     "left join AccessList2 on AccessList2.APPLY_PK = View_VMT_FAC.con_number " +
                     "inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name " +
                     "left join WORK_CHECKIN on WORK_CHECKIN.con_number = View_VMT_FAC.con_number and WORK_CHECKIN.checkin_time is not null " +
                     "left join WORK_CHECKIN_LOG on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID " +
                     "left join RecognitionAuth on RecognitionAuth.ID = WORK_CHECKIN_LOG.IFP_RecognitionAuth_ID and RecognitionAuth.FRUserID = AccessList2.ID " +
                     " Where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 18:00:00" + @"' and fac_name = 'FAB5廠' group by vendor_name order by count(distinct WORK_CHECKIN.con_number) desc";
                da = new SqlDataAdapter(sql, gConn);
                da.SelectCommand.CommandType = CommandType.Text;
                dt2 = new DataTable();
                da.Fill(dt2);
                wk = cyc.Shared.NPOI.GetWorkbook(@"D:\中央施工管理系統\ReportTemplates\施工管理報表.xlsx");
                sheet1 = wk.GetSheet("Sheet1");
                sheet1.GetRow(0).CreateCell(1).SetCellValue(dt.Rows[0][0].ToString());
                sheet1.GetRow(0).CreateCell(3).SetCellValue(int.Parse(dt.Rows[0][1].ToString()));
                sheet1.GetRow(0).CreateCell(5).SetCellValue(int.Parse(dt.Rows[0][2].ToString()));
                sheet1.GetRow(0).CreateCell(7).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt.Rows[0][3].ToString())));
                for (int i = 0; i < dt2.Rows.Count; i++)
                {
                    sheet1.GetRow(5 + i).CreateCell(0).SetCellValue(dt2.Rows[i][0].ToString());
                    sheet1.GetRow(5 + i).CreateCell(3).SetCellValue(int.Parse(dt2.Rows[i][1].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(4).SetCellValue(int.Parse(dt2.Rows[i][2].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(1).SetCellValue(int.Parse(dt2.Rows[i][3].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(2).SetCellValue(int.Parse(dt2.Rows[i][4].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(5).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt2.Rows[i][5].ToString())));
                    sheet1.GetRow(5 + i).CreateCell(6).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt2.Rows[i][6].ToString())));

                }
                sheet1.ForceFormulaRecalculation = true;
                sFileName = @"D:\中央施工管理系統\INX_WorkChecking\File\FAB5施工管理報表_" + DateTime.Now.ToString("yyyy_MM_dd") + @".xlsx";
                using (FileStream fileStream = File.Open(sFileName, FileMode.Create, FileAccess.ReadWrite))
                {
                    wk.Write(fileStream);
                    fileStream.Close();
                }
                sqlCommand = gConn.CreateCommand();
                MappFileNameByte = File.ReadAllBytes(sFileName);
                sql = @"insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                                'FAC5_HVAC', 'FAB5施工管理報表', '3', 'FAB5施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
                                insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                                'FAC5_WATER', 'FAB5施工管理報表', '3', 'FAB5施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
                                insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                                'FAC5_GAS', 'FAB5施工管理報表', '3', 'FAB5施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
                                insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                                'FAC5_POWER', 'FAB5施工管理報表', '3', 'FAB5施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);";
                sqlCommand = gConn.CreateCommand();
                sqlCommand.Parameters.Add("@MappFileNameByte", SqlDbType.Binary, MappFileNameByte.Length).Value = MappFileNameByte;
                sqlCommand.Parameters.Add("@MM_FILE_SHOW_NAME", SqlDbType.VarChar, 50).Value = "FAB5施工報表_" + DateTime.Now.ToString("yyyy_MM_dd") + @".xlsx";
                sqlCommand.CommandText = sql;
                sqlCommand.ExecuteNonQuery();
                #endregion

                #region "FAC6"
                sql = @"select 'FAC6', (select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + @"' and fac_name = 'FAB6廠') as FAC6施工總數, 
                        (select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + "' and fac_name = 'FAB6廠') as FAC6刷臉報到總數, " +
               "round(cast((select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + "' and fac_name = 'FAB6廠') as Float) / cast ((select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + @"' and fac_name = 'FAB6廠') as float) ,2)";
                da = new SqlDataAdapter(sql, gConn);
                da.SelectCommand.CommandType = CommandType.Text;
                dt = new DataTable();
                da.Fill(dt);
                sql = "Select vendor_name,Count(distinct AccessList2.ID), count(distinct RecognitionAuth.FRUserID), count(distinct View_VMT_FAC.con_number), count(distinct WORK_CHECKIN.con_number)," +
                     "  round (cast(count(distinct WORK_CHECKIN.con_number) as float) / nullif(count(distinct View_VMT_FAC.con_number),0), 2), " +
 " isnull(round(cast(count(distinct RecognitionAuth.FRUserID) as float) / nullif(Count(distinct AccessList2.ID), 0), 2),0) From View_VMT_FAC " +
                     "left join AccessList2 on AccessList2.APPLY_PK = View_VMT_FAC.con_number " +
                     "inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name " +
                     "left join WORK_CHECKIN on WORK_CHECKIN.con_number = View_VMT_FAC.con_number and WORK_CHECKIN.checkin_time is not null " +
                     "left join WORK_CHECKIN_LOG on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID " +
                     "left join RecognitionAuth on RecognitionAuth.ID = WORK_CHECKIN_LOG.IFP_RecognitionAuth_ID and RecognitionAuth.FRUserID = AccessList2.ID " +
                     " Where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 18:00:00" + @"' and fac_name = 'FAB6廠' group by vendor_name order by count(distinct WORK_CHECKIN.con_number) desc";
                da = new SqlDataAdapter(sql, gConn);
                da.SelectCommand.CommandType = CommandType.Text;
                dt2 = new DataTable();
                da.Fill(dt2);
                wk = cyc.Shared.NPOI.GetWorkbook(@"D:\中央施工管理系統\ReportTemplates\施工管理報表.xlsx");
                sheet1 = wk.GetSheet("Sheet1");
                sheet1.GetRow(0).CreateCell(1).SetCellValue(dt.Rows[0][0].ToString());
                sheet1.GetRow(0).CreateCell(3).SetCellValue(int.Parse(dt.Rows[0][1].ToString()));
                sheet1.GetRow(0).CreateCell(5).SetCellValue(int.Parse(dt.Rows[0][2].ToString()));
                sheet1.GetRow(0).CreateCell(7).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt.Rows[0][3].ToString())));
                for (int i = 0; i < dt2.Rows.Count; i++)
                {
                    sheet1.GetRow(5 + i).CreateCell(0).SetCellValue(dt2.Rows[i][0].ToString());
                    sheet1.GetRow(5 + i).CreateCell(3).SetCellValue(int.Parse(dt2.Rows[i][1].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(4).SetCellValue(int.Parse(dt2.Rows[i][2].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(1).SetCellValue(int.Parse(dt2.Rows[i][3].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(2).SetCellValue(int.Parse(dt2.Rows[i][4].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(5).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt2.Rows[i][5].ToString())));
                    sheet1.GetRow(5 + i).CreateCell(6).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt2.Rows[i][6].ToString())));

                }
                sheet1.ForceFormulaRecalculation = true;
                sFileName = @"D:\中央施工管理系統\INX_WorkChecking\File\FAB6施工管理報表_" + DateTime.Now.ToString("yyyy_MM_dd") + @".xlsx";
                using (FileStream fileStream = File.Open(sFileName, FileMode.Create, FileAccess.ReadWrite))
                {
                    wk.Write(fileStream);
                    fileStream.Close();
                }
                sqlCommand = gConn.CreateCommand();
                MappFileNameByte = File.ReadAllBytes(sFileName);
                sql = @"insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                                'FAC6_HVAC', 'FAB6施工管理報表', '3', 'FAB6施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
                                insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                                'FAC6_WATER', 'FAB6施工管理報表', '3', 'FAB6施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
                                insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                                'FAC6_GAS', 'FAB6施工管理報表', '3', 'FAB6施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
                                insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                                'FAC6_POWER', 'FAB6施工管理報表', '3', 'FAB6施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);";
                sqlCommand = gConn.CreateCommand();
                sqlCommand.Parameters.Add("@MappFileNameByte", SqlDbType.Binary, MappFileNameByte.Length).Value = MappFileNameByte;
                sqlCommand.Parameters.Add("@MM_FILE_SHOW_NAME", SqlDbType.VarChar, 50).Value = "FAB6施工報表_" + DateTime.Now.ToString("yyyy_MM_dd") + @".xlsx";
                sqlCommand.CommandText = sql;
                sqlCommand.ExecuteNonQuery();
                #endregion

                #region "FAC7"
                sql = @"select 'FAC7', (select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + @"' and fac_name = 'FAB7廠') as FAC7施工總數, 
                        (select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + "' and fac_name = 'FAB7廠') as FAC7刷臉報到總數, " +
               "round(cast((select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + "' and fac_name = 'FAB7廠') as Float) / cast ((select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + @"' and fac_name = 'FAB7廠') as float) ,2)";
                da = new SqlDataAdapter(sql, gConn);
                da.SelectCommand.CommandType = CommandType.Text;
                dt = new DataTable();
                da.Fill(dt);
                sql = "Select vendor_name,Count(distinct AccessList2.ID), count(distinct RecognitionAuth.FRUserID), count(distinct View_VMT_FAC.con_number), count(distinct WORK_CHECKIN.con_number)," +
                     "  round (cast(count(distinct WORK_CHECKIN.con_number) as float) / nullif(count(distinct View_VMT_FAC.con_number),0), 2), " +
 " isnull(round(cast(count(distinct RecognitionAuth.FRUserID) as float) / nullif(Count(distinct AccessList2.ID), 0), 2),0) From View_VMT_FAC " +
                     "left join AccessList2 on AccessList2.APPLY_PK = View_VMT_FAC.con_number " +
                     "inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name " +
                     "left join WORK_CHECKIN on WORK_CHECKIN.con_number = View_VMT_FAC.con_number and WORK_CHECKIN.checkin_time is not null " +
                     "left join WORK_CHECKIN_LOG on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID " +
                     "left join RecognitionAuth on RecognitionAuth.ID = WORK_CHECKIN_LOG.IFP_RecognitionAuth_ID and RecognitionAuth.FRUserID = AccessList2.ID " +
                     " Where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 18:00:00" + @"' and fac_name = 'FAB7廠' group by vendor_name order by count(distinct WORK_CHECKIN.con_number) desc";
                da = new SqlDataAdapter(sql, gConn);
                da.SelectCommand.CommandType = CommandType.Text;
                dt2 = new DataTable();
                da.Fill(dt2);
                wk = cyc.Shared.NPOI.GetWorkbook(@"D:\中央施工管理系統\ReportTemplates\施工管理報表.xlsx");
                sheet1 = wk.GetSheet("Sheet1");
                sheet1.GetRow(0).CreateCell(1).SetCellValue(dt.Rows[0][0].ToString());
                sheet1.GetRow(0).CreateCell(3).SetCellValue(int.Parse(dt.Rows[0][1].ToString()));
                sheet1.GetRow(0).CreateCell(5).SetCellValue(int.Parse(dt.Rows[0][2].ToString()));
                sheet1.GetRow(0).CreateCell(7).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt.Rows[0][3].ToString())));
                for (int i = 0; i < dt2.Rows.Count; i++)
                {
                    sheet1.GetRow(5 + i).CreateCell(0).SetCellValue(dt2.Rows[i][0].ToString());
                    sheet1.GetRow(5 + i).CreateCell(3).SetCellValue(int.Parse(dt2.Rows[i][1].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(4).SetCellValue(int.Parse(dt2.Rows[i][2].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(1).SetCellValue(int.Parse(dt2.Rows[i][3].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(2).SetCellValue(int.Parse(dt2.Rows[i][4].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(5).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt2.Rows[i][5].ToString())));
                    sheet1.GetRow(5 + i).CreateCell(6).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt2.Rows[i][6].ToString())));

                }
                sheet1.ForceFormulaRecalculation = true;
                sFileName = @"D:\中央施工管理系統\INX_WorkChecking\File\FAB7施工管理報表_" + DateTime.Now.ToString("yyyy_MM_dd") + @".xlsx";
                using (FileStream fileStream = File.Open(sFileName, FileMode.Create, FileAccess.ReadWrite))
                {
                    wk.Write(fileStream);
                    fileStream.Close();
                }
                sqlCommand = gConn.CreateCommand();
                MappFileNameByte = File.ReadAllBytes(sFileName);
                sql = @"insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                                'FAC7_HVAC', 'FAB7施工管理報表', '3', 'FAB7施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
                                insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                                'FAC7_WATER', 'FAB7施工管理報表', '3', 'FAB7施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
                                insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                                'FAC7_GAS', 'FAB7施工管理報表', '3', 'FAB7施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
                                insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                                'FAC7_POWER', 'FAB7施工管理報表', '3', 'FAB7施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);";
                sqlCommand = gConn.CreateCommand();
                sqlCommand.Parameters.Add("@MappFileNameByte", SqlDbType.Binary, MappFileNameByte.Length).Value = MappFileNameByte;
                sqlCommand.Parameters.Add("@MM_FILE_SHOW_NAME", SqlDbType.VarChar, 50).Value = "FAB7施工報表_" + DateTime.Now.ToString("yyyy_MM_dd") + @".xlsx";
                sqlCommand.CommandText = sql;
                sqlCommand.ExecuteNonQuery();
                #endregion

                #region "FAC8"
                sql = @"select 'FAC8', (select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + @"' and (fac_name = 'FAB8廠' or fac_name = 'T6廠')) as FAC8施工總數, 
                        (select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + "' and (fac_name = 'FAB8廠' or fac_name = 'T6廠')) as FAC8刷臉報到總數, " +
               "round(cast((select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + "' and (fac_name = 'FAB8廠' or fac_name = 'T6廠')) as Float) / cast ((select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + @"' and (fac_name = 'FAB8廠' or fac_name = 'T6廠')) as float) ,2)";
                da = new SqlDataAdapter(sql, gConn);
                da.SelectCommand.CommandType = CommandType.Text;
                dt = new DataTable();
                da.Fill(dt);
                sql = "Select vendor_name,Count(distinct AccessList2.ID), count(distinct RecognitionAuth.FRUserID), count(distinct View_VMT_FAC.con_number), count(distinct WORK_CHECKIN.con_number)," +
                     "  round (cast(count(distinct WORK_CHECKIN.con_number) as float) / nullif(count(distinct View_VMT_FAC.con_number),0), 2), " +
 " isnull(round(cast(count(distinct RecognitionAuth.FRUserID) as float) / nullif(Count(distinct AccessList2.ID), 0), 2),0) From View_VMT_FAC " +
                     "left join AccessList2 on AccessList2.APPLY_PK = View_VMT_FAC.con_number " +
                     "inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name " +
                     "left join WORK_CHECKIN on WORK_CHECKIN.con_number = View_VMT_FAC.con_number and WORK_CHECKIN.checkin_time is not null " +
                     "left join WORK_CHECKIN_LOG on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID " +
                     "left join RecognitionAuth on RecognitionAuth.ID = WORK_CHECKIN_LOG.IFP_RecognitionAuth_ID and RecognitionAuth.FRUserID = AccessList2.ID " +
                     " Where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 18:00:00" + @"' and (fac_name = 'FAB8廠' or fac_name = 'T6廠') group by vendor_name order by count(distinct WORK_CHECKIN.con_number) desc";
                da = new SqlDataAdapter(sql, gConn);
                da.SelectCommand.CommandType = CommandType.Text;
                dt2 = new DataTable();
                da.Fill(dt2);
                wk = cyc.Shared.NPOI.GetWorkbook(@"D:\中央施工管理系統\ReportTemplates\施工管理報表.xlsx");
                sheet1 = wk.GetSheet("Sheet1");
                sheet1.GetRow(0).CreateCell(1).SetCellValue(dt.Rows[0][0].ToString());
                sheet1.GetRow(0).CreateCell(3).SetCellValue(int.Parse(dt.Rows[0][1].ToString()));
                sheet1.GetRow(0).CreateCell(5).SetCellValue(int.Parse(dt.Rows[0][2].ToString()));
                sheet1.GetRow(0).CreateCell(7).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt.Rows[0][3].ToString())));
                for (int i = 0; i < dt2.Rows.Count; i++)
                {
                    sheet1.GetRow(5 + i).CreateCell(0).SetCellValue(dt2.Rows[i][0].ToString());
                    sheet1.GetRow(5 + i).CreateCell(3).SetCellValue(int.Parse(dt2.Rows[i][1].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(4).SetCellValue(int.Parse(dt2.Rows[i][2].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(1).SetCellValue(int.Parse(dt2.Rows[i][3].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(2).SetCellValue(int.Parse(dt2.Rows[i][4].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(5).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt2.Rows[i][5].ToString())));
                    sheet1.GetRow(5 + i).CreateCell(6).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt2.Rows[i][6].ToString())));
                }
                sheet1.ForceFormulaRecalculation = true;
                sFileName = @"D:\中央施工管理系統\INX_WorkChecking\File\FAB8施工管理報表_" + DateTime.Now.ToString("yyyy_MM_dd") + @".xlsx";
                using (FileStream fileStream = File.Open(sFileName, FileMode.Create, FileAccess.ReadWrite))
                {
                    wk.Write(fileStream);
                    fileStream.Close();
                }
                sqlCommand = gConn.CreateCommand();
                MappFileNameByte = File.ReadAllBytes(sFileName);
                sql = @"insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                                'FAC8_HVAC', 'FAB8施工管理報表', '3', 'FAB8施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
                                insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                                'FAC8_WATER', 'FAB8施工管理報表', '3', 'FAB8施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
                                insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                                'FAC8_GAS', 'FAB8施工管理報表', '3', 'FAB8施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
                                insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                                'FAC8_POWER', 'FAB8施工管理報表', '3', 'FAB8施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
                                insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                                'FACT6_POWER', 'FAB8施工管理報表', '3', 'FAB8施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME)
                                insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                                'FACT6_WATER', 'FAB8施工管理報表', '3', 'FAB8施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME)";
                sqlCommand = gConn.CreateCommand();
                sqlCommand.Parameters.Add("@MappFileNameByte", SqlDbType.Binary, MappFileNameByte.Length).Value = MappFileNameByte;
                sqlCommand.Parameters.Add("@MM_FILE_SHOW_NAME", SqlDbType.VarChar, 50).Value = "FAB8施工報表_" + DateTime.Now.ToString("yyyy_MM_dd") + @".xlsx";
                sqlCommand.CommandText = sql;
                sqlCommand.ExecuteNonQuery();
                #endregion

                #region "FACC"
                sql = @"select 'FACC', (select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + @"' and (fac_name = 'FAB C廠' or fac_name = 'C3&CG' or fac_name = 'C3&CG廠' or fac_name = 'FAB C' or fac_name = 'MOD2' or fac_name = 'MOD2$MOD4廠' or fac_name = '科九廠' or fac_name = '群豐駿')) as FACC施工總數, 
                        (select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + "' and (fac_name = 'FAB C廠' or fac_name = 'C3&CG' or fac_name = 'C3&CG廠' or fac_name = 'FAB C' or fac_name = 'MOD2' or fac_name = 'MOD2$MOD4廠' or fac_name = '科九廠' or fac_name = '群豐駿')) as FACC刷臉報到總數, " +
"round(cast((select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + "' and (fac_name = 'FAB C廠' or fac_name = 'C3&CG' or fac_name = 'C3&CG廠' or fac_name = 'FAB C' or fac_name = 'MOD2' or fac_name = 'MOD2$MOD4廠' or fac_name = '科九廠' or fac_name = '群豐駿')) as Float) / cast ((select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + @"' and (fac_name = 'FAB C廠' or fac_name = 'C3&CG' or fac_name = 'C3&CG廠' or fac_name = 'FAB C' or fac_name = 'MOD2' or fac_name = 'MOD2$MOD4廠' or fac_name = '科九廠' or fac_name = '群豐駿')) as float) ,2)";
                da = new SqlDataAdapter(sql, gConn);
                da.SelectCommand.CommandType = CommandType.Text;
                dt = new DataTable();
                da.Fill(dt);
                sql = "Select vendor_name,Count(distinct AccessList2.ID), count(distinct RecognitionAuth.FRUserID), count(distinct View_VMT_FAC.con_number), count(distinct WORK_CHECKIN.con_number)," +
                     "  round (cast(count(distinct WORK_CHECKIN.con_number) as float) / nullif(count(distinct View_VMT_FAC.con_number),0), 2), " +
 " isnull(round(cast(count(distinct RecognitionAuth.FRUserID) as float) / nullif(Count(distinct AccessList2.ID), 0), 2),0) From View_VMT_FAC " +
                     "left join AccessList2 on AccessList2.APPLY_PK = View_VMT_FAC.con_number " +
                     "inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name " +
                     "left join WORK_CHECKIN on WORK_CHECKIN.con_number = View_VMT_FAC.con_number and WORK_CHECKIN.checkin_time is not null " +
                     "left join WORK_CHECKIN_LOG on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID " +
                     "left join RecognitionAuth on RecognitionAuth.ID = WORK_CHECKIN_LOG.IFP_RecognitionAuth_ID and RecognitionAuth.FRUserID = AccessList2.ID " +
                     " Where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 18:00:00" + @"' and (fac_name = 'FAB C廠' or fac_name = 'C3&CG' or fac_name = 'C3&CG廠' or fac_name = 'FAB C' or fac_name = 'MOD2' or fac_name = 'MOD2$MOD4廠' or fac_name = '科九廠' or fac_name = '群豐駿') group by vendor_name order by count(distinct WORK_CHECKIN.con_number) desc";
                da = new SqlDataAdapter(sql, gConn);
                da.SelectCommand.CommandType = CommandType.Text;
                dt2 = new DataTable();
                da.Fill(dt2);
                wk = cyc.Shared.NPOI.GetWorkbook(@"D:\中央施工管理系統\ReportTemplates\施工管理報表.xlsx");
                sheet1 = wk.GetSheet("Sheet1");
                sheet1.GetRow(0).CreateCell(1).SetCellValue(dt.Rows[0][0].ToString());
                sheet1.GetRow(0).CreateCell(3).SetCellValue(int.Parse(dt.Rows[0][1].ToString()));
                sheet1.GetRow(0).CreateCell(5).SetCellValue(int.Parse(dt.Rows[0][2].ToString()));
                sheet1.GetRow(0).CreateCell(7).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt.Rows[0][3].ToString())));
                for (int i = 0; i < dt2.Rows.Count; i++)
                {
                    sheet1.GetRow(5 + i).CreateCell(0).SetCellValue(dt2.Rows[i][0].ToString());
                    sheet1.GetRow(5 + i).CreateCell(3).SetCellValue(int.Parse(dt2.Rows[i][1].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(4).SetCellValue(int.Parse(dt2.Rows[i][2].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(1).SetCellValue(int.Parse(dt2.Rows[i][3].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(2).SetCellValue(int.Parse(dt2.Rows[i][4].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(5).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt2.Rows[i][5].ToString())));
                    sheet1.GetRow(5 + i).CreateCell(6).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt2.Rows[i][6].ToString())));
                }
                sheet1.ForceFormulaRecalculation = true;
                sFileName = @"D:\中央施工管理系統\INX_WorkChecking\File\FABC施工管理報表_" + DateTime.Now.ToString("yyyy_MM_dd") + @".xlsx";
                using (FileStream fileStream = File.Open(sFileName, FileMode.Create, FileAccess.ReadWrite))
                {
                    wk.Write(fileStream);
                    fileStream.Close();
                }
                sqlCommand = gConn.CreateCommand();
                MappFileNameByte = File.ReadAllBytes(sFileName);
                sql = @"insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                                'FACC_HVAC', 'FABC施工管理報表', '3', 'FABC施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
                                insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                                'FACC_WATER', 'FABC施工管理報表', '3', 'FABC施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
                                insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                                'FACC_GAS', 'FABC施工管理報表', '3', 'FABC施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
                                insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                                'FACC_POWER', 'FABC施工管理報表', '3', 'FABC施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);";
                sqlCommand = gConn.CreateCommand();
                sqlCommand.Parameters.Add("@MappFileNameByte", SqlDbType.Binary, MappFileNameByte.Length).Value = MappFileNameByte;
                sqlCommand.Parameters.Add("@MM_FILE_SHOW_NAME", SqlDbType.VarChar, 50).Value = "FABC施工報表_" + DateTime.Now.ToString("yyyy_MM_dd") + @".xlsx";
                sqlCommand.CommandText = sql;
                sqlCommand.ExecuteNonQuery();
                #endregion

                //if (gConn.State == ConnectionState.Open) { gConn.Close(); }
            }
            catch (Exception ex)
            {
                cyc.Log.WriteSysErrorLog("施工管理報表新增：" + ex.Message);
            }
            finally
            {
                if (gConn != null)
                {
                    if (gConn.State == ConnectionState.Open) { gConn.Close(); }
                    gConn.Dispose();
                }
            }
        }

//        private static void WorkCheckReportFac(DateTime tDate, string sFac, string sFab, string sFacName, string sMappTo)
//        {
//            ExeResult oResult = new ExeResult();
//            string sTemplate = @"D:\中央施工管理系統\ReportTemplates\施工管理報表.xlsx";

//            string[] FacNames = sFacName.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

//            using (var oDB = new cyc.DB.SqlDapperConn(oResult, null, false, 120))
//            {
//                string sql = $@"select
// (select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date=@Date and fac_name in @Facs)
//,(select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date=@Date and fac_name in @Facs)
//,round(cast((select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date=@Date and fac_name in @Facs) as Float) / cast ((select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date=@Date and fac_name in @Facs) as float) ,2)";

//                using (DataTable dt = oDB.QueryDataTable(sql, new { Date = tDate, Facs = FacNames }))
//                {
//                    if (oResult.Success && dt != null)
//                    {
//                        sql = $@"
//Select vendor_name,Count(distinct AccessList2.ID), count(distinct RecognitionAuth.FRUserID), count(distinct View_VMT_FAC.con_number), count(distinct WORK_CHECKIN.con_number),
//round (cast(count(distinct WORK_CHECKIN.con_number) as float) / nullif(count(distinct View_VMT_FAC.con_number),0), 2), 
//isnull(round(cast(count(distinct RecognitionAuth.FRUserID) as float) / nullif(Count(distinct AccessList2.ID), 0), 2),0) From View_VMT_FAC
//left join AccessList2 on AccessList2.APPLY_PK = View_VMT_FAC.con_number
//inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name
//left join WORK_CHECKIN on WORK_CHECKIN.con_number = View_VMT_FAC.con_number and WORK_CHECKIN.checkin_time is not null
//left join WORK_CHECKIN_LOG on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID 
//left join RecognitionAuth on RecognitionAuth.ID = WORK_CHECKIN_LOG.IFP_RecognitionAuth_ID and RecognitionAuth.FRUserID = AccessList2.ID
//Where con_date=@Date and fac_name in @Facs group by vendor_name order by count(distinct WORK_CHECKIN.con_number) desc";

//                        using (DataTable dt2 = oDB.QueryDataTable(sql, new { Date = tDate, Facs = FacNames }))
//                        {
//                            if (oResult.Success && dt2 != null)
//                            {
//                                try
//                                {
//                                    IWorkbook wk = cyc.Shared.NPOI.GetWorkbook(sTemplate);
//                                    ISheet sheet1 = wk.GetSheet("Sheet1");
//                                    sheet1.GetRow(0).CreateCell(1).SetCellValue(sFac);
//                                    sheet1.GetRow(0).CreateCell(3).SetCellValue(int.Parse(dt.Rows[0][0].ToString()));
//                                    sheet1.GetRow(0).CreateCell(5).SetCellValue(int.Parse(dt.Rows[0][1].ToString()));
//                                    sheet1.GetRow(0).CreateCell(7).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt.Rows[0][2].ToString())));
                                    
//                                    for (int i = 0; i < dt2.Rows.Count; i++)
//                                    {
//                                        sheet1.GetRow(5 + i).CreateCell(0).SetCellValue(dt2.Rows[i][0].ToString());
//                                        sheet1.GetRow(5 + i).CreateCell(3).SetCellValue(int.Parse(dt2.Rows[i][1].ToString()));
//                                        sheet1.GetRow(5 + i).CreateCell(4).SetCellValue(int.Parse(dt2.Rows[i][2].ToString()));
//                                        sheet1.GetRow(5 + i).CreateCell(1).SetCellValue(int.Parse(dt2.Rows[i][3].ToString()));
//                                        sheet1.GetRow(5 + i).CreateCell(2).SetCellValue(int.Parse(dt2.Rows[i][4].ToString()));
//                                        sheet1.GetRow(5 + i).CreateCell(5).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt2.Rows[i][5].ToString())));
//                                        sheet1.GetRow(5 + i).CreateCell(6).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt2.Rows[i][6].ToString())));
//                                    }
//                                    sheet1.ForceFormulaRecalculation = true;

//                                    string FileName = $@"{sFab}施工管理報表_{DateTime.Now:yyyy_MM_dd}.xlsx";
//                                    string FilePath = Path.Combine(@"D:\中央施工管理系統\INX_WorkChecking\File\", FileName);
//                                    using (FileStream fileStream = File.Open(FilePath, FileMode.Create, FileAccess.ReadWrite))
//                                    {
//                                        wk.Write(fileStream);
//                                        fileStream.Close();
//                                    }

//                                    byte[] MappFile = File.ReadAllBytes(FilePath);

//                                    foreach (string MappSys in sMappTo.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
//                                    {
//                                        oDB.Execute($@"
//insert Into MappMessage(MS_SYS_NAME,MM_TEXT_CONTENT,MM_CONTENT_TYPE,MM_subject,MM_TYPE,MM_MEDIA_CONTENT,MM_ExtFileName,MM_FILE_SHOW_NAME) 
//values (@MappSys,'{sFab}施工管理報表','3','{sFab}施工管理報表','A',@MappFile,'xlsx',@FileName)", new { MappSys, MappFile, FileName });
//                                    }
//                                }
//                                catch (Exception ex) { cyc.Log.WriteSysErrorLog($"施工管理報表{sFac}:{ex.Message}"); }
//                            }
//                        }
//                    }
//                }
//            }
//        }

//        public static void WorkCheckReport3()
//        {
//            DateTime tDate = DateTime.Today;

//            //FAC1
//            WorkCheckReportFac(tDate, "FAC1", "FAB1", "FAB1廠", "FAC1_HVAC,FAC1_WATER,FAC1_GAS,FAC1_POWER");

//            //FAC2
//            WorkCheckReportFac(tDate, "FAC2", "FAB2", "FAB2廠", "FAC2_HVAC,FAC2_WATER,FAC2_GAS,FAC2_POWER");

//            //FAC3
//            WorkCheckReportFac(tDate, "FAC3", "FAB3", "FAB3廠", "FAC3_HVAC,FAC3_WATER,FAC3_GAS,FAC3_POWER");

//            //FAC5
//            WorkCheckReportFac(tDate, "FAC5", "FAB5", "FAB5廠", "FAC5_HVAC,FAC5_WATER,FAC5_GAS,FAC5_POWER");

//            //FAC6
//            WorkCheckReportFac(tDate, "FAC6", "FAB6", "FAB6廠", "FAC6_HVAC,FAC6_WATER,FAC6_GAS,FAC6_POWER");

//            //FAC7
//            WorkCheckReportFac(tDate, "FAC7", "FAB7", "FAB7廠", "FAC7_HVAC,FAC7_WATER,FAC7_GAS,FAC7_POWER");

//            //FAC8
//            WorkCheckReportFac(tDate, "FAC8", "FAB8", "FAB8廠,T6廠", "FAC8_HVAC,FAC8_WATER,FAC8_GAS,FAC8_POWER,FACT6_POWER,FACT6_WATER");

//            //FACC
//            WorkCheckReportFac(tDate, "FACC", "FABC", "FAB C廠,C3&CG,C3&CG廠,FAB C,MOD2,MOD2$MOD4廠,科九廠,群豐駿", "FACC_HVAC,FACC_WATER,FACC_GAS,FACC_POWER");
//        }

//        public static void WorkCheckReport2()
//        {
//            string sFileTemplate = @"D:\中央施工管理系統\ReportTemplates\施工管理報表.xlsx";

//            SqlConnection gConn = new SqlConnection(cyc.DB.ConnString.Main);
//            DataTable dt;
//            DataTable dt2;
//            try
//            {
//                SqlDataAdapter da = new SqlDataAdapter();
//                if (gConn.State == ConnectionState.Closed) { gConn.Open(); }

//                string sToday = DateTime.Now.ToString("yyyy/MM/dd");
//                #region "FAC1"
//                string sql = $@"select 'FAC1'
//        ,(select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date>='{sToday} 00:00:00' and con_date<='{sToday} 23:59:59' and fac_name = 'FAB1廠') as FAC1施工總數
//        ,(select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date>='{sToday} 00:00:00' and con_date<='{sToday} 23:59:59' and fac_name = 'FAB1廠') as FAC1刷臉報到總數
//        ,round(cast((select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date>='{sToday} 00:00:00' and con_date<='{sToday} 23:59:59' and fac_name = 'FAB1廠') as Float) / cast ((select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date>='{sToday} 00:00:00' and con_date<='{sToday} 23:59:59' and fac_name = 'FAB1廠') as float) ,2)";
//                da = new SqlDataAdapter(sql, gConn);
//                da.SelectCommand.CommandType = CommandType.Text;
//                dt = new DataTable();
//                da.Fill(dt);
//                sql = $@"
//        Select vendor_name,Count(distinct AccessList2.ID), count(distinct RecognitionAuth.FRUserID), count(distinct View_VMT_FAC.con_number), count(distinct WORK_CHECKIN.con_number),
//        round (cast(count(distinct WORK_CHECKIN.con_number) as float) / nullif(count(distinct View_VMT_FAC.con_number),0), 2), 
//        isnull(round(cast(count(distinct RecognitionAuth.FRUserID) as float) / nullif(Count(distinct AccessList2.ID), 0), 2),0) From View_VMT_FAC
//        left join AccessList2 on AccessList2.APPLY_PK = View_VMT_FAC.con_number
//        inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name
//        left join WORK_CHECKIN on WORK_CHECKIN.con_number = View_VMT_FAC.con_number and WORK_CHECKIN.checkin_time is not null
//        left join WORK_CHECKIN_LOG on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID 
//        left join RecognitionAuth on RecognitionAuth.ID = WORK_CHECKIN_LOG.IFP_RecognitionAuth_ID and RecognitionAuth.FRUserID = AccessList2.ID
//        Where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 18:00:00' and fac_name = 'FAB1廠' group by vendor_name order by count(distinct WORK_CHECKIN.con_number) desc";
//                da = new SqlDataAdapter(sql, gConn);
//                da.SelectCommand.CommandType = CommandType.Text;
//                dt2 = new DataTable();
//                da.Fill(dt2);
//                IWorkbook wk = cyc.Shared.NPOI.GetWorkbook(sFileTemplate);
//                ISheet sheet1 = wk.GetSheet("Sheet1");
//                sheet1.GetRow(0).CreateCell(1).SetCellValue(dt.Rows[0][0].ToString());
//                sheet1.GetRow(0).CreateCell(3).SetCellValue(int.Parse(dt.Rows[0][1].ToString()));
//                sheet1.GetRow(0).CreateCell(5).SetCellValue(int.Parse(dt.Rows[0][2].ToString()));
//                sheet1.GetRow(0).CreateCell(7).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt.Rows[0][3].ToString())));
//                for (int i = 0; i < dt2.Rows.Count; i++)
//                {
//                    sheet1.GetRow(5 + i).CreateCell(0).SetCellValue(dt2.Rows[i][0].ToString());
//                    sheet1.GetRow(5 + i).CreateCell(3).SetCellValue(int.Parse(dt2.Rows[i][1].ToString()));
//                    sheet1.GetRow(5 + i).CreateCell(4).SetCellValue(int.Parse(dt2.Rows[i][2].ToString()));
//                    sheet1.GetRow(5 + i).CreateCell(1).SetCellValue(int.Parse(dt2.Rows[i][3].ToString()));
//                    sheet1.GetRow(5 + i).CreateCell(2).SetCellValue(int.Parse(dt2.Rows[i][4].ToString()));
//                    sheet1.GetRow(5 + i).CreateCell(5).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt2.Rows[i][5].ToString())));
//                    sheet1.GetRow(5 + i).CreateCell(6).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt2.Rows[i][6].ToString())));

//                }
//                sheet1.ForceFormulaRecalculation = true;
//                string sFileName = $@"D:\中央施工管理系統\INX_WorkChecking\File\FAB1施工管理報表_{DateTime.Now:yyyy_MM_dd}.xlsx";
//                using (FileStream fileStream = File.Open(sFileName, FileMode.Create, FileAccess.ReadWrite))
//                {
//                    wk.Write(fileStream);
//                    fileStream.Close();
//                }

//                SqlCommand sqlCommand = gConn.CreateCommand();
//                byte[] MappFileNameByte = File.ReadAllBytes(sFileName);
//                sql = @"
//        insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME)
//        values ('FAC1_HVAC', 'FAB1施工管理報表', '3', 'FAB1施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
//        insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME)
//        values ('FAC1_WATER', 'FAB1施工管理報表', '3', 'FAB1施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
//        insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME)
//        values ('FAC1_GAS', 'FAB1施工管理報表', '3', 'FAB1施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
//        insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME)
//        values ('FAC1_POWER', 'FAB1施工管理報表', '3', 'FAB1施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);";
//                sqlCommand = gConn.CreateCommand();
//                sqlCommand.Parameters.Add("@MappFileNameByte", SqlDbType.Binary, MappFileNameByte.Length).Value = MappFileNameByte;
//                sqlCommand.Parameters.Add("@MM_FILE_SHOW_NAME", SqlDbType.VarChar, 50).Value = "FAB1施工報表_" + DateTime.Now.ToString("yyyy_MM_dd") + @".xlsx";
//                sqlCommand.CommandText = sql;
//                sqlCommand.ExecuteNonQuery();
//                #endregion

//                #region "FAC2"
//                sql = $@"select 'FAC2'
//        ,(select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 23:59:59' and fac_name = 'FAB2廠') as FAC2施工總數 
//        ,(select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 23:59:59' and fac_name = 'FAB2廠') as FAC2刷臉報到總數
//        ,round(cast((select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 23:59:59' and fac_name = 'FAB2廠') as Float) / cast ((select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 23:59:59' and fac_name = 'FAB2廠') as float) ,2)";
//                da = new SqlDataAdapter(sql, gConn);
//                da.SelectCommand.CommandType = CommandType.Text;
//                dt = new DataTable();
//                da.Fill(dt);
//                sql = $@"
//        Select vendor_name,Count(distinct AccessList2.ID), count(distinct RecognitionAuth.FRUserID), count(distinct View_VMT_FAC.con_number), count(distinct WORK_CHECKIN.con_number),
//        round (cast(count(distinct WORK_CHECKIN.con_number) as float) / nullif(count(distinct View_VMT_FAC.con_number),0), 2), 
//        isnull(round(cast(count(distinct RecognitionAuth.FRUserID) as float) / nullif(Count(distinct AccessList2.ID), 0), 2),0) From View_VMT_FAC 
//        left join AccessList2 on AccessList2.APPLY_PK = View_VMT_FAC.con_number 
//        inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name 
//        left join WORK_CHECKIN on WORK_CHECKIN.con_number = View_VMT_FAC.con_number and WORK_CHECKIN.checkin_time is not null 
//        left join WORK_CHECKIN_LOG on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID 
//        left join RecognitionAuth on RecognitionAuth.ID = WORK_CHECKIN_LOG.IFP_RecognitionAuth_ID and RecognitionAuth.FRUserID = AccessList2.ID 
//        Where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 18:00:00' and fac_name = 'FAB2廠' group by vendor_name order by count(distinct WORK_CHECKIN.con_number) desc";
//                da = new SqlDataAdapter(sql, gConn);
//                da.SelectCommand.CommandType = CommandType.Text;
//                dt2 = new DataTable();
//                da.Fill(dt2);
//                wk = cyc.Shared.NPOI.GetWorkbook(sFileTemplate);
//                sheet1 = wk.GetSheet("Sheet1");
//                sheet1.GetRow(0).CreateCell(1).SetCellValue(dt.Rows[0][0].ToString());
//                sheet1.GetRow(0).CreateCell(3).SetCellValue(int.Parse(dt.Rows[0][1].ToString()));
//                sheet1.GetRow(0).CreateCell(5).SetCellValue(int.Parse(dt.Rows[0][2].ToString()));
//                sheet1.GetRow(0).CreateCell(7).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt.Rows[0][3].ToString())));
//                for (int i = 0; i < dt2.Rows.Count; i++)
//                {
//                    sheet1.GetRow(5 + i).CreateCell(0).SetCellValue(dt2.Rows[i][0].ToString());
//                    sheet1.GetRow(5 + i).CreateCell(3).SetCellValue(int.Parse(dt2.Rows[i][1].ToString()));
//                    sheet1.GetRow(5 + i).CreateCell(4).SetCellValue(int.Parse(dt2.Rows[i][2].ToString()));
//                    sheet1.GetRow(5 + i).CreateCell(1).SetCellValue(int.Parse(dt2.Rows[i][3].ToString()));
//                    sheet1.GetRow(5 + i).CreateCell(2).SetCellValue(int.Parse(dt2.Rows[i][4].ToString()));
//                    sheet1.GetRow(5 + i).CreateCell(5).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt2.Rows[i][5].ToString())));
//                    sheet1.GetRow(5 + i).CreateCell(6).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt2.Rows[i][6].ToString())));

//                }
//                sheet1.ForceFormulaRecalculation = true;
//                sFileName = @"D:\中央施工管理系統\INX_WorkChecking\File\FAB2施工管理報表_" + DateTime.Now.ToString("yyyy_MM_dd") + @".xlsx";
//                using (FileStream fileStream = File.Open(sFileName, FileMode.Create, FileAccess.ReadWrite))
//                {
//                    wk.Write(fileStream);
//                    fileStream.Close();
//                }
//                sqlCommand = gConn.CreateCommand();
//                MappFileNameByte = File.ReadAllBytes(sFileName);
//                sql = @"
//        insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
//        'FAC2_HVAC', 'FAB2施工管理報表', '3', 'FAB2施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
//        insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
//        'FAC2_WATER', 'FAB2施工管理報表', '3', 'FAB2施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
//        insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
//        'FAC2_GAS', 'FAB2施工管理報表', '3', 'FAB2施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
//        insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
//        'FAC2_POWER', 'FAB2施工管理報表', '3', 'FAB2施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);";
//                sqlCommand = gConn.CreateCommand();
//                sqlCommand.Parameters.Add("@MappFileNameByte", SqlDbType.Binary, MappFileNameByte.Length).Value = MappFileNameByte;
//                sqlCommand.Parameters.Add("@MM_FILE_SHOW_NAME", SqlDbType.VarChar, 50).Value = "FAB2施工報表_" + DateTime.Now.ToString("yyyy_MM_dd") + @".xlsx";
//                sqlCommand.CommandText = sql;
//                sqlCommand.ExecuteNonQuery();
//                #endregion

//                #region "FAC3"
//                sql = $@"select 'FAC3'
//        ,(select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 23:59:59' and fac_name = 'FAB3廠') as FAC3施工總數
//        ,(select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 23:59:59' and fac_name = 'FAB3廠') as FAC3刷臉報到總數
//        ,round(cast((select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 23:59:59' and fac_name = 'FAB3廠') as Float) / cast ((select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 23:59:59' and fac_name = 'FAB3廠') as float) ,2)";
//                da = new SqlDataAdapter(sql, gConn);
//                da.SelectCommand.CommandType = CommandType.Text;
//                dt = new DataTable();
//                da.Fill(dt);
//                sql = $@"
//        Select vendor_name,Count(distinct AccessList2.ID), count(distinct RecognitionAuth.FRUserID), count(distinct View_VMT_FAC.con_number), count(distinct WORK_CHECKIN.con_number),
//        round (cast(count(distinct WORK_CHECKIN.con_number) as float) / nullif(count(distinct View_VMT_FAC.con_number),0), 2),
//        isnull(round(cast(count(distinct RecognitionAuth.FRUserID) as float) / nullif(Count(distinct AccessList2.ID), 0), 2),0) From View_VMT_FAC
//        left join AccessList2 on AccessList2.APPLY_PK = View_VMT_FAC.con_number
//        inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name
//        left join WORK_CHECKIN on WORK_CHECKIN.con_number = View_VMT_FAC.con_number and WORK_CHECKIN.checkin_time is not null
//        left join WORK_CHECKIN_LOG on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID
//        left join RecognitionAuth on RecognitionAuth.ID = WORK_CHECKIN_LOG.IFP_RecognitionAuth_ID and RecognitionAuth.FRUserID = AccessList2.ID
//        Where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 18:00:00' and fac_name = 'FAB3廠' group by vendor_name order by count(distinct WORK_CHECKIN.con_number) desc";
//                da = new SqlDataAdapter(sql, gConn);
//                da.SelectCommand.CommandType = CommandType.Text;
//                dt2 = new DataTable();
//                da.Fill(dt2);
//                wk = cyc.Shared.NPOI.GetWorkbook(sFileTemplate);
//                sheet1 = wk.GetSheet("Sheet1");
//                sheet1.GetRow(0).CreateCell(1).SetCellValue(dt.Rows[0][0].ToString());
//                sheet1.GetRow(0).CreateCell(3).SetCellValue(int.Parse(dt.Rows[0][1].ToString()));
//                sheet1.GetRow(0).CreateCell(5).SetCellValue(int.Parse(dt.Rows[0][2].ToString()));
//                sheet1.GetRow(0).CreateCell(7).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt.Rows[0][3].ToString())));
//                for (int i = 0; i < dt2.Rows.Count; i++)
//                {
//                    sheet1.GetRow(5 + i).CreateCell(0).SetCellValue(dt2.Rows[i][0].ToString());
//                    sheet1.GetRow(5 + i).CreateCell(3).SetCellValue(int.Parse(dt2.Rows[i][1].ToString()));
//                    sheet1.GetRow(5 + i).CreateCell(4).SetCellValue(int.Parse(dt2.Rows[i][2].ToString()));
//                    sheet1.GetRow(5 + i).CreateCell(1).SetCellValue(int.Parse(dt2.Rows[i][3].ToString()));
//                    sheet1.GetRow(5 + i).CreateCell(2).SetCellValue(int.Parse(dt2.Rows[i][4].ToString()));
//                    sheet1.GetRow(5 + i).CreateCell(5).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt2.Rows[i][5].ToString())));
//                    sheet1.GetRow(5 + i).CreateCell(6).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt2.Rows[i][6].ToString())));

//                }
//                sheet1.ForceFormulaRecalculation = true;
//                sFileName = @"D:\中央施工管理系統\INX_WorkChecking\File\FAB3施工管理報表_" + DateTime.Now.ToString("yyyy_MM_dd") + @".xlsx";
//                using (FileStream fileStream = File.Open(sFileName, FileMode.Create, FileAccess.ReadWrite))
//                {
//                    wk.Write(fileStream);
//                    fileStream.Close();
//                }
//                sqlCommand = gConn.CreateCommand();
//                MappFileNameByte = File.ReadAllBytes(sFileName);
//                sql = @"
//        insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
//        'FAC3_HVAC', 'FAB3施工管理報表', '3', 'FAB3施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
//        insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
//        'FAC3_WATER', 'FAB3施工管理報表', '3', 'FAB3施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
//        insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
//        'FAC3_GAS', 'FAB3施工管理報表', '3', 'FAB3施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
//        insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
//        'FAC3_POWER', 'FAB3施工管理報表', '3', 'FAB3施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);";
//                sqlCommand = gConn.CreateCommand();
//                sqlCommand.Parameters.Add("@MappFileNameByte", SqlDbType.Binary, MappFileNameByte.Length).Value = MappFileNameByte;
//                sqlCommand.Parameters.Add("@MM_FILE_SHOW_NAME", SqlDbType.VarChar, 50).Value = "FAB3施工報表_" + DateTime.Now.ToString("yyyy_MM_dd") + @".xlsx";
//                sqlCommand.CommandText = sql;
//                sqlCommand.ExecuteNonQuery();
//                #endregion

//                #region "FAC5"
//                sql = $@"select 'FAC5'
//        ,(select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 23:59:59' and fac_name = 'FAB5廠') as FAC5施工總數
//        ,(select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 23:59:59' and fac_name = 'FAB5廠') as FAC5刷臉報到總數
//        ,round(cast((select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 23:59:59' and fac_name = 'FAB5廠') as Float) / cast ((select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 23:59:59' and fac_name = 'FAB5廠') as float) ,2)";
//                da = new SqlDataAdapter(sql, gConn);
//                da.SelectCommand.CommandType = CommandType.Text;
//                dt = new DataTable();
//                da.Fill(dt);
//                sql = $@"
//        Select vendor_name,Count(distinct AccessList2.ID), count(distinct RecognitionAuth.FRUserID), count(distinct View_VMT_FAC.con_number), count(distinct WORK_CHECKIN.con_number),
//        round (cast(count(distinct WORK_CHECKIN.con_number) as float) / nullif(count(distinct View_VMT_FAC.con_number),0), 2), 
//        isnull(round(cast(count(distinct RecognitionAuth.FRUserID) as float) / nullif(Count(distinct AccessList2.ID), 0), 2),0) From View_VMT_FAC 
//        left join AccessList2 on AccessList2.APPLY_PK = View_VMT_FAC.con_number 
//        inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name 
//        left join WORK_CHECKIN on WORK_CHECKIN.con_number = View_VMT_FAC.con_number and WORK_CHECKIN.checkin_time is not null 
//        left join WORK_CHECKIN_LOG on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID 
//        left join RecognitionAuth on RecognitionAuth.ID = WORK_CHECKIN_LOG.IFP_RecognitionAuth_ID and RecognitionAuth.FRUserID = AccessList2.ID 
//        Where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 18:00:00' and fac_name = 'FAB5廠' group by vendor_name order by count(distinct WORK_CHECKIN.con_number) desc";
//                da = new SqlDataAdapter(sql, gConn);
//                da.SelectCommand.CommandType = CommandType.Text;
//                dt2 = new DataTable();
//                da.Fill(dt2);
//                wk = cyc.Shared.NPOI.GetWorkbook(sFileTemplate);
//                sheet1 = wk.GetSheet("Sheet1");
//                sheet1.GetRow(0).CreateCell(1).SetCellValue(dt.Rows[0][0].ToString());
//                sheet1.GetRow(0).CreateCell(3).SetCellValue(int.Parse(dt.Rows[0][1].ToString()));
//                sheet1.GetRow(0).CreateCell(5).SetCellValue(int.Parse(dt.Rows[0][2].ToString()));
//                sheet1.GetRow(0).CreateCell(7).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt.Rows[0][3].ToString())));
//                for (int i = 0; i < dt2.Rows.Count; i++)
//                {
//                    sheet1.GetRow(5 + i).CreateCell(0).SetCellValue(dt2.Rows[i][0].ToString());
//                    sheet1.GetRow(5 + i).CreateCell(3).SetCellValue(int.Parse(dt2.Rows[i][1].ToString()));
//                    sheet1.GetRow(5 + i).CreateCell(4).SetCellValue(int.Parse(dt2.Rows[i][2].ToString()));
//                    sheet1.GetRow(5 + i).CreateCell(1).SetCellValue(int.Parse(dt2.Rows[i][3].ToString()));
//                    sheet1.GetRow(5 + i).CreateCell(2).SetCellValue(int.Parse(dt2.Rows[i][4].ToString()));
//                    sheet1.GetRow(5 + i).CreateCell(5).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt2.Rows[i][5].ToString())));
//                    sheet1.GetRow(5 + i).CreateCell(6).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt2.Rows[i][6].ToString())));

//                }
//                sheet1.ForceFormulaRecalculation = true;
//                sFileName = @"D:\中央施工管理系統\INX_WorkChecking\File\FAB5施工管理報表_" + DateTime.Now.ToString("yyyy_MM_dd") + @".xlsx";
//                using (FileStream fileStream = File.Open(sFileName, FileMode.Create, FileAccess.ReadWrite))
//                {
//                    wk.Write(fileStream);
//                    fileStream.Close();
//                }
//                sqlCommand = gConn.CreateCommand();
//                MappFileNameByte = File.ReadAllBytes(sFileName);
//                sql = @"
//        insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
//        'FAC5_HVAC', 'FAB5施工管理報表', '3', 'FAB5施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
//        insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
//        'FAC5_WATER', 'FAB5施工管理報表', '3', 'FAB5施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
//        insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
//        'FAC5_GAS', 'FAB5施工管理報表', '3', 'FAB5施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
//        insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
//        'FAC5_POWER', 'FAB5施工管理報表', '3', 'FAB5施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);";
//                sqlCommand = gConn.CreateCommand();
//                sqlCommand.Parameters.Add("@MappFileNameByte", SqlDbType.Binary, MappFileNameByte.Length).Value = MappFileNameByte;
//                sqlCommand.Parameters.Add("@MM_FILE_SHOW_NAME", SqlDbType.VarChar, 50).Value = "FAB5施工報表_" + DateTime.Now.ToString("yyyy_MM_dd") + @".xlsx";
//                sqlCommand.CommandText = sql;
//                sqlCommand.ExecuteNonQuery();
//                #endregion

//                #region "FAC6"
//                sql = $@"select 'FAC6'
//        ,(select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 23:59:59' and fac_name = 'FAB6廠') as FAC6施工總數
//        ,(select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 23:59:59' and fac_name = 'FAB6廠') as FAC6刷臉報到總數
//        ,round(cast((select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 23:59:59' and fac_name = 'FAB6廠') as Float) / cast ((select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 23:59:59' and fac_name = 'FAB6廠') as float) ,2)";
//                da = new SqlDataAdapter(sql, gConn);
//                da.SelectCommand.CommandType = CommandType.Text;
//                dt = new DataTable();
//                da.Fill(dt);
//                sql = $@"
//        Select vendor_name,Count(distinct AccessList2.ID), count(distinct RecognitionAuth.FRUserID), count(distinct View_VMT_FAC.con_number), count(distinct WORK_CHECKIN.con_number),
//        round (cast(count(distinct WORK_CHECKIN.con_number) as float) / nullif(count(distinct View_VMT_FAC.con_number),0), 2),
//        isnull(round(cast(count(distinct RecognitionAuth.FRUserID) as float) / nullif(Count(distinct AccessList2.ID), 0), 2),0) From View_VMT_FAC
//        left join AccessList2 on AccessList2.APPLY_PK = View_VMT_FAC.con_number 
//        inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name 
//        left join WORK_CHECKIN on WORK_CHECKIN.con_number = View_VMT_FAC.con_number and WORK_CHECKIN.checkin_time is not null 
//        left join WORK_CHECKIN_LOG on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID 
//        left join RecognitionAuth on RecognitionAuth.ID = WORK_CHECKIN_LOG.IFP_RecognitionAuth_ID and RecognitionAuth.FRUserID = AccessList2.ID 
//        Where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 18:00:00' and fac_name = 'FAB6廠' group by vendor_name order by count(distinct WORK_CHECKIN.con_number) desc";
//                da = new SqlDataAdapter(sql, gConn);
//                da.SelectCommand.CommandType = CommandType.Text;
//                dt2 = new DataTable();
//                da.Fill(dt2);
//                wk = cyc.Shared.NPOI.GetWorkbook(sFileTemplate);
//                sheet1 = wk.GetSheet("Sheet1");
//                sheet1.GetRow(0).CreateCell(1).SetCellValue(dt.Rows[0][0].ToString());
//                sheet1.GetRow(0).CreateCell(3).SetCellValue(int.Parse(dt.Rows[0][1].ToString()));
//                sheet1.GetRow(0).CreateCell(5).SetCellValue(int.Parse(dt.Rows[0][2].ToString()));
//                sheet1.GetRow(0).CreateCell(7).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt.Rows[0][3].ToString())));
//                for (int i = 0; i < dt2.Rows.Count; i++)
//                {
//                    sheet1.GetRow(5 + i).CreateCell(0).SetCellValue(dt2.Rows[i][0].ToString());
//                    sheet1.GetRow(5 + i).CreateCell(3).SetCellValue(int.Parse(dt2.Rows[i][1].ToString()));
//                    sheet1.GetRow(5 + i).CreateCell(4).SetCellValue(int.Parse(dt2.Rows[i][2].ToString()));
//                    sheet1.GetRow(5 + i).CreateCell(1).SetCellValue(int.Parse(dt2.Rows[i][3].ToString()));
//                    sheet1.GetRow(5 + i).CreateCell(2).SetCellValue(int.Parse(dt2.Rows[i][4].ToString()));
//                    sheet1.GetRow(5 + i).CreateCell(5).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt2.Rows[i][5].ToString())));
//                    sheet1.GetRow(5 + i).CreateCell(6).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt2.Rows[i][6].ToString())));

//                }
//                sheet1.ForceFormulaRecalculation = true;
//                sFileName = @"D:\中央施工管理系統\INX_WorkChecking\File\FAB6施工管理報表_" + DateTime.Now.ToString("yyyy_MM_dd") + @".xlsx";
//                using (FileStream fileStream = File.Open(sFileName, FileMode.Create, FileAccess.ReadWrite))
//                {
//                    wk.Write(fileStream);
//                    fileStream.Close();
//                }
//                sqlCommand = gConn.CreateCommand();
//                MappFileNameByte = File.ReadAllBytes(sFileName);
//                sql = @"
//        insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
//        'FAC6_HVAC', 'FAB6施工管理報表', '3', 'FAB6施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
//        insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
//        'FAC6_WATER', 'FAB6施工管理報表', '3', 'FAB6施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
//        insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
//        'FAC6_GAS', 'FAB6施工管理報表', '3', 'FAB6施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
//        insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
//        'FAC6_POWER', 'FAB6施工管理報表', '3', 'FAB6施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);";
//                sqlCommand = gConn.CreateCommand();
//                sqlCommand.Parameters.Add("@MappFileNameByte", SqlDbType.Binary, MappFileNameByte.Length).Value = MappFileNameByte;
//                sqlCommand.Parameters.Add("@MM_FILE_SHOW_NAME", SqlDbType.VarChar, 50).Value = "FAB6施工報表_" + DateTime.Now.ToString("yyyy_MM_dd") + @".xlsx";
//                sqlCommand.CommandText = sql;
//                sqlCommand.ExecuteNonQuery();
//                #endregion

//                #region "FAC7"
//                sql = $@"select 'FAC7'
//        ,(select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 23:59:59' and fac_name = 'FAB7廠') as FAC7施工總數
//        ,(select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 23:59:59' and fac_name = 'FAB7廠') as FAC7刷臉報到總數
//        ,round(cast((select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 23:59:59' and fac_name = 'FAB7廠') as Float) / cast ((select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 23:59:59' and fac_name = 'FAB7廠') as float) ,2)";
//                da = new SqlDataAdapter(sql, gConn);
//                da.SelectCommand.CommandType = CommandType.Text;
//                dt = new DataTable();
//                da.Fill(dt);
//                sql = $@"
//        Select vendor_name,Count(distinct AccessList2.ID), count(distinct RecognitionAuth.FRUserID), count(distinct View_VMT_FAC.con_number), count(distinct WORK_CHECKIN.con_number),
//        round (cast(count(distinct WORK_CHECKIN.con_number) as float) / nullif(count(distinct View_VMT_FAC.con_number),0), 2),
//        isnull(round(cast(count(distinct RecognitionAuth.FRUserID) as float) / nullif(Count(distinct AccessList2.ID), 0), 2),0) From View_VMT_FAC
//        left join AccessList2 on AccessList2.APPLY_PK = View_VMT_FAC.con_number
//        inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name 
//        left join WORK_CHECKIN on WORK_CHECKIN.con_number = View_VMT_FAC.con_number and WORK_CHECKIN.checkin_time is not null 
//        left join WORK_CHECKIN_LOG on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID 
//        left join RecognitionAuth on RecognitionAuth.ID = WORK_CHECKIN_LOG.IFP_RecognitionAuth_ID and RecognitionAuth.FRUserID = AccessList2.ID 
//        Where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 18:00:00' and fac_name = 'FAB7廠' group by vendor_name order by count(distinct WORK_CHECKIN.con_number) desc";
//                da = new SqlDataAdapter(sql, gConn);
//                da.SelectCommand.CommandType = CommandType.Text;
//                dt2 = new DataTable();
//                da.Fill(dt2);
//                wk = cyc.Shared.NPOI.GetWorkbook(sFileTemplate);
//                sheet1 = wk.GetSheet("Sheet1");
//                sheet1.GetRow(0).CreateCell(1).SetCellValue(dt.Rows[0][0].ToString());
//                sheet1.GetRow(0).CreateCell(3).SetCellValue(int.Parse(dt.Rows[0][1].ToString()));
//                sheet1.GetRow(0).CreateCell(5).SetCellValue(int.Parse(dt.Rows[0][2].ToString()));
//                sheet1.GetRow(0).CreateCell(7).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt.Rows[0][3].ToString())));
//                for (int i = 0; i < dt2.Rows.Count; i++)
//                {
//                    sheet1.GetRow(5 + i).CreateCell(0).SetCellValue(dt2.Rows[i][0].ToString());
//                    sheet1.GetRow(5 + i).CreateCell(3).SetCellValue(int.Parse(dt2.Rows[i][1].ToString()));
//                    sheet1.GetRow(5 + i).CreateCell(4).SetCellValue(int.Parse(dt2.Rows[i][2].ToString()));
//                    sheet1.GetRow(5 + i).CreateCell(1).SetCellValue(int.Parse(dt2.Rows[i][3].ToString()));
//                    sheet1.GetRow(5 + i).CreateCell(2).SetCellValue(int.Parse(dt2.Rows[i][4].ToString()));
//                    sheet1.GetRow(5 + i).CreateCell(5).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt2.Rows[i][5].ToString())));
//                    sheet1.GetRow(5 + i).CreateCell(6).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt2.Rows[i][6].ToString())));

//                }
//                sheet1.ForceFormulaRecalculation = true;
//                sFileName = @"D:\中央施工管理系統\INX_WorkChecking\File\FAB7施工管理報表_" + DateTime.Now.ToString("yyyy_MM_dd") + @".xlsx";
//                using (FileStream fileStream = File.Open(sFileName, FileMode.Create, FileAccess.ReadWrite))
//                {
//                    wk.Write(fileStream);
//                    fileStream.Close();
//                }
//                sqlCommand = gConn.CreateCommand();
//                MappFileNameByte = File.ReadAllBytes(sFileName);
//                sql = @"
//        insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
//        'FAC7_HVAC', 'FAB7施工管理報表', '3', 'FAB7施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
//        insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
//        'FAC7_WATER', 'FAB7施工管理報表', '3', 'FAB7施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
//        insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
//        'FAC7_GAS', 'FAB7施工管理報表', '3', 'FAB7施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
//        insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
//        'FAC7_POWER', 'FAB7施工管理報表', '3', 'FAB7施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);";
//                sqlCommand = gConn.CreateCommand();
//                sqlCommand.Parameters.Add("@MappFileNameByte", SqlDbType.Binary, MappFileNameByte.Length).Value = MappFileNameByte;
//                sqlCommand.Parameters.Add("@MM_FILE_SHOW_NAME", SqlDbType.VarChar, 50).Value = "FAB7施工報表_" + DateTime.Now.ToString("yyyy_MM_dd") + @".xlsx";
//                sqlCommand.CommandText = sql;
//                sqlCommand.ExecuteNonQuery();
//                #endregion

//                #region "FAC8"
//                sql = $@"select 'FAC8'
//        ,(select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 23:59:59' and (fac_name = 'FAB8廠' or fac_name = 'T6廠')) as FAC8施工總數
//        ,(select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 23:59:59' and (fac_name = 'FAB8廠' or fac_name = 'T6廠')) as FAC8刷臉報到總數
//        ,round(cast((select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 23:59:59' and (fac_name = 'FAB8廠' or fac_name = 'T6廠')) as Float) / cast ((select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 23:59:59' and (fac_name = 'FAB8廠' or fac_name = 'T6廠')) as float) ,2)";
//                da = new SqlDataAdapter(sql, gConn);
//                da.SelectCommand.CommandType = CommandType.Text;
//                dt = new DataTable();
//                da.Fill(dt);
//                sql = $@"Select vendor_name,Count(distinct AccessList2.ID), count(distinct RecognitionAuth.FRUserID), count(distinct View_VMT_FAC.con_number), count(distinct WORK_CHECKIN.con_number),
//        round (cast(count(distinct WORK_CHECKIN.con_number) as float) / nullif(count(distinct View_VMT_FAC.con_number),0), 2),
//        isnull(round(cast(count(distinct RecognitionAuth.FRUserID) as float) / nullif(Count(distinct AccessList2.ID), 0), 2),0) From View_VMT_FAC 
//        left join AccessList2 on AccessList2.APPLY_PK = View_VMT_FAC.con_number 
//        inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name 
//        left join WORK_CHECKIN on WORK_CHECKIN.con_number = View_VMT_FAC.con_number and WORK_CHECKIN.checkin_time is not null
//        left join WORK_CHECKIN_LOG on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID 
//        left join RecognitionAuth on RecognitionAuth.ID = WORK_CHECKIN_LOG.IFP_RecognitionAuth_ID and RecognitionAuth.FRUserID = AccessList2.ID
//        Where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 18:00:00' and (fac_name = 'FAB8廠' or fac_name = 'T6廠') group by vendor_name order by count(distinct WORK_CHECKIN.con_number) desc";
//                da = new SqlDataAdapter(sql, gConn);
//                da.SelectCommand.CommandType = CommandType.Text;
//                dt2 = new DataTable();
//                da.Fill(dt2);
//                wk = cyc.Shared.NPOI.GetWorkbook(sFileTemplate);
//                sheet1 = wk.GetSheet("Sheet1");
//                sheet1.GetRow(0).CreateCell(1).SetCellValue(dt.Rows[0][0].ToString());
//                sheet1.GetRow(0).CreateCell(3).SetCellValue(int.Parse(dt.Rows[0][1].ToString()));
//                sheet1.GetRow(0).CreateCell(5).SetCellValue(int.Parse(dt.Rows[0][2].ToString()));
//                sheet1.GetRow(0).CreateCell(7).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt.Rows[0][3].ToString())));
//                for (int i = 0; i < dt2.Rows.Count; i++)
//                {
//                    sheet1.GetRow(5 + i).CreateCell(0).SetCellValue(dt2.Rows[i][0].ToString());
//                    sheet1.GetRow(5 + i).CreateCell(3).SetCellValue(int.Parse(dt2.Rows[i][1].ToString()));
//                    sheet1.GetRow(5 + i).CreateCell(4).SetCellValue(int.Parse(dt2.Rows[i][2].ToString()));
//                    sheet1.GetRow(5 + i).CreateCell(1).SetCellValue(int.Parse(dt2.Rows[i][3].ToString()));
//                    sheet1.GetRow(5 + i).CreateCell(2).SetCellValue(int.Parse(dt2.Rows[i][4].ToString()));
//                    sheet1.GetRow(5 + i).CreateCell(5).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt2.Rows[i][5].ToString())));
//                    sheet1.GetRow(5 + i).CreateCell(6).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt2.Rows[i][6].ToString())));
//                }
//                sheet1.ForceFormulaRecalculation = true;
//                sFileName = @"D:\中央施工管理系統\INX_WorkChecking\File\FAB8施工管理報表_" + DateTime.Now.ToString("yyyy_MM_dd") + @".xlsx";
//                using (FileStream fileStream = File.Open(sFileName, FileMode.Create, FileAccess.ReadWrite))
//                {
//                    wk.Write(fileStream);
//                    fileStream.Close();
//                }
//                sqlCommand = gConn.CreateCommand();
//                MappFileNameByte = File.ReadAllBytes(sFileName);
//                sql = @"
//        insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
//        'FAC8_HVAC', 'FAB8施工管理報表', '3', 'FAB8施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
//        insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
//        'FAC8_WATER', 'FAB8施工管理報表', '3', 'FAB8施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
//        insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
//        'FAC8_GAS', 'FAB8施工管理報表', '3', 'FAB8施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
//        insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
//        'FAC8_POWER', 'FAB8施工管理報表', '3', 'FAB8施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
//        insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
//        'FACT6_POWER', 'FAB8施工管理報表', '3', 'FAB8施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME)
//        insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
//        'FACT6_WATER', 'FAB8施工管理報表', '3', 'FAB8施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME)";
//                sqlCommand = gConn.CreateCommand();
//                sqlCommand.Parameters.Add("@MappFileNameByte", SqlDbType.Binary, MappFileNameByte.Length).Value = MappFileNameByte;
//                sqlCommand.Parameters.Add("@MM_FILE_SHOW_NAME", SqlDbType.VarChar, 50).Value = "FAB8施工報表_" + DateTime.Now.ToString("yyyy_MM_dd") + @".xlsx";
//                sqlCommand.CommandText = sql;
//                sqlCommand.ExecuteNonQuery();
//                #endregion

//                #region "FACC"
//                sql = $@"select 'FACC'
//        ,(select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 23:59:59' and (fac_name = 'FAB C廠' or fac_name = 'C3&CG' or fac_name = 'C3&CG廠' or fac_name = 'FAB C' or fac_name = 'MOD2' or fac_name = 'MOD2$MOD4廠' or fac_name = '科九廠' or fac_name = '群豐駿')) as FACC施工總數
//        ,(select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 23:59:59' and (fac_name = 'FAB C廠' or fac_name = 'C3&CG' or fac_name = 'C3&CG廠' or fac_name = 'FAB C' or fac_name = 'MOD2' or fac_name = 'MOD2$MOD4廠' or fac_name = '科九廠' or fac_name = '群豐駿')) as FACC刷臉報到總數
//        ,round(cast((select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 23:59:59' and (fac_name = 'FAB C廠' or fac_name = 'C3&CG' or fac_name = 'C3&CG廠' or fac_name = 'FAB C' or fac_name = 'MOD2' or fac_name = 'MOD2$MOD4廠' or fac_name = '科九廠' or fac_name = '群豐駿')) as Float) / cast ((select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 23:59:59' and (fac_name = 'FAB C廠' or fac_name = 'C3&CG' or fac_name = 'C3&CG廠' or fac_name = 'FAB C' or fac_name = 'MOD2' or fac_name = 'MOD2$MOD4廠' or fac_name = '科九廠' or fac_name = '群豐駿')) as float) ,2)";
//                da = new SqlDataAdapter(sql, gConn);
//                da.SelectCommand.CommandType = CommandType.Text;
//                dt = new DataTable();
//                da.Fill(dt);
//                sql = $@"
//        Select vendor_name,Count(distinct AccessList2.ID), count(distinct RecognitionAuth.FRUserID), count(distinct View_VMT_FAC.con_number), count(distinct WORK_CHECKIN.con_number),
//        round (cast(count(distinct WORK_CHECKIN.con_number) as float) / nullif(count(distinct View_VMT_FAC.con_number),0), 2),
//        isnull(round(cast(count(distinct RecognitionAuth.FRUserID) as float) / nullif(Count(distinct AccessList2.ID), 0), 2),0) From View_VMT_FAC 
//        left join AccessList2 on AccessList2.APPLY_PK = View_VMT_FAC.con_number 
//        inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name 
//        left join WORK_CHECKIN on WORK_CHECKIN.con_number = View_VMT_FAC.con_number and WORK_CHECKIN.checkin_time is not null 
//        left join WORK_CHECKIN_LOG on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID 
//        left join RecognitionAuth on RecognitionAuth.ID = WORK_CHECKIN_LOG.IFP_RecognitionAuth_ID and RecognitionAuth.FRUserID = AccessList2.ID
//        Where con_date >= '{sToday} 00:00:00' and con_date <= '{sToday} 18:00:00' and (fac_name = 'FAB C廠' or fac_name = 'C3&CG' or fac_name = 'C3&CG廠' or fac_name = 'FAB C' or fac_name = 'MOD2' or fac_name = 'MOD2$MOD4廠' or fac_name = '科九廠' or fac_name = '群豐駿') group by vendor_name order by count(distinct WORK_CHECKIN.con_number) desc";
//                da = new SqlDataAdapter(sql, gConn);
//                da.SelectCommand.CommandType = CommandType.Text;
//                dt2 = new DataTable();
//                da.Fill(dt2);
//                wk = cyc.Shared.NPOI.GetWorkbook(sFileTemplate);
//                sheet1 = wk.GetSheet("Sheet1");
//                sheet1.GetRow(0).CreateCell(1).SetCellValue(dt.Rows[0][0].ToString());
//                sheet1.GetRow(0).CreateCell(3).SetCellValue(int.Parse(dt.Rows[0][1].ToString()));
//                sheet1.GetRow(0).CreateCell(5).SetCellValue(int.Parse(dt.Rows[0][2].ToString()));
//                sheet1.GetRow(0).CreateCell(7).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt.Rows[0][3].ToString())));
//                for (int i = 0; i < dt2.Rows.Count; i++)
//                {
//                    sheet1.GetRow(5 + i).CreateCell(0).SetCellValue(dt2.Rows[i][0].ToString());
//                    sheet1.GetRow(5 + i).CreateCell(3).SetCellValue(int.Parse(dt2.Rows[i][1].ToString()));
//                    sheet1.GetRow(5 + i).CreateCell(4).SetCellValue(int.Parse(dt2.Rows[i][2].ToString()));
//                    sheet1.GetRow(5 + i).CreateCell(1).SetCellValue(int.Parse(dt2.Rows[i][3].ToString()));
//                    sheet1.GetRow(5 + i).CreateCell(2).SetCellValue(int.Parse(dt2.Rows[i][4].ToString()));
//                    sheet1.GetRow(5 + i).CreateCell(5).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt2.Rows[i][5].ToString())));
//                    sheet1.GetRow(5 + i).CreateCell(6).SetCellValue(string.Format("{0:0.00%}", float.Parse(dt2.Rows[i][6].ToString())));
//                }
//                sheet1.ForceFormulaRecalculation = true;
//                sFileName = @"D:\中央施工管理系統\INX_WorkChecking\File\FABC施工管理報表_" + DateTime.Now.ToString("yyyy_MM_dd") + @".xlsx";
//                using (FileStream fileStream = File.Open(sFileName, FileMode.Create, FileAccess.ReadWrite))
//                {
//                    wk.Write(fileStream);
//                    fileStream.Close();
//                }
//                sqlCommand = gConn.CreateCommand();
//                MappFileNameByte = File.ReadAllBytes(sFileName);
//                sql = @"
//        insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
//        'FACC_HVAC', 'FABC施工管理報表', '3', 'FABC施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
//        insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
//        'FACC_WATER', 'FABC施工管理報表', '3', 'FABC施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
//        insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
//        'FACC_GAS', 'FABC施工管理報表', '3', 'FABC施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);
//        insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
//        'FACC_POWER', 'FABC施工管理報表', '3', 'FABC施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME);";
//                sqlCommand = gConn.CreateCommand();
//                sqlCommand.Parameters.Add("@MappFileNameByte", SqlDbType.Binary, MappFileNameByte.Length).Value = MappFileNameByte;
//                sqlCommand.Parameters.Add("@MM_FILE_SHOW_NAME", SqlDbType.VarChar, 50).Value = "FABC施工報表_" + DateTime.Now.ToString("yyyy_MM_dd") + @".xlsx";
//                sqlCommand.CommandText = sql;
//                sqlCommand.ExecuteNonQuery();
//                #endregion

//                //if (gConn.State == ConnectionState.Open) { gConn.Close(); }
//            }
//            catch (Exception ex)
//            {
//                cyc.Log.WriteSysErrorLog("施工管理報表新增：" + ex.Message);
//            }
//            finally
//            {
//                if (gConn != null)
//                {
//                    if (gConn.State == ConnectionState.Open) { gConn.Close(); }
//                    gConn.Dispose();
//                }
//            }
//        }

        public static void TOCWorkCheckReport()
        {
            if (DateTime.Now.Hour == 18 && DateTime.Now.Minute == 30)
            {
                SqlConnection gConn = new SqlConnection();
                DataTable dt = new DataTable();
                DataTable dt2 = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter();
                gConn = new SqlConnection(cyc.DB.ConnString.Main2);
                if (gConn.State == ConnectionState.Closed)
                {
                    gConn.Open();
                }
                string sql = @"select 'TOC', (select count(*) From View_VMT_FAC4 inner join SysUser on replace(SUBSTRING(View_VMT_FAC4.engineer,0,5),'(','') = SysUser.Name where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + @"' and fac_name = 'TOC廠') as TOC施工總數, 
                (select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC4 on View_VMT_FAC4.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC4.engineer,0,5),'(','') = SysUser.Name  where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + "' and fac_name = 'TOC廠') as TOC刷臉報到總數";
                da = new SqlDataAdapter(sql, gConn);
                da.SelectCommand.CommandType = CommandType.Text;
                dt = new DataTable();
                da.Fill(dt);
                sql = "Select vendor_name,Count(distinct AccessList2.ID), count(distinct IFP_RecognitionAuth.FRUserID), count(distinct View_VMT_FAC4.con_number), count(distinct WORK_CHECKIN.con_number) From View_VMT_FAC4 " +
                    "left join AccessList2 on AccessList2.APPLY_PK = View_VMT_FAC4.con_number " +
                    "inner join SysUser on replace(SUBSTRING(View_VMT_FAC4.engineer,0,5),'(','') = SysUser.Name " +
                    "left join WORK_CHECKIN on WORK_CHECKIN.con_number = View_VMT_FAC4.con_number and WORK_CHECKIN.checkin_time is not null " +
                    "left join WORK_CHECKIN_LOG on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID " +
                    "left join IFP_RecognitionAuth on IFP_RecognitionAuth.ID = WORK_CHECKIN_LOG.IFP_RecognitionAuth_ID and IFP_RecognitionAuth.FRUserID = AccessList2.ID " +
                    " Where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 18:00:00" + @"' and fac_name = 'TOC廠' group by vendor_name order by count(distinct WORK_CHECKIN.con_number) desc";
                da = new SqlDataAdapter(sql, gConn);
                da.SelectCommand.CommandType = CommandType.Text;
                dt2 = new DataTable();
                da.Fill(dt2);
                IWorkbook wk = cyc.Shared.NPOI.GetWorkbook(@"D:\ReportTemplates\施工管理報表.xlsx");
                ISheet sheet1 = wk.GetSheet("Sheet1");
                sheet1.GetRow(0).CreateCell(1).SetCellValue(dt.Rows[0][0].ToString());
                sheet1.GetRow(0).CreateCell(3).SetCellValue(int.Parse(dt.Rows[0][1].ToString()));
                sheet1.GetRow(0).CreateCell(5).SetCellValue(int.Parse(dt.Rows[0][2].ToString()));
                for (int i = 0; i < dt2.Rows.Count; i++)
                {
                    sheet1.GetRow(5 + i).CreateCell(0).SetCellValue(dt2.Rows[i][0].ToString());
                    sheet1.GetRow(5 + i).CreateCell(3).SetCellValue(int.Parse(dt2.Rows[i][1].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(4).SetCellValue(int.Parse(dt2.Rows[i][2].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(1).SetCellValue(int.Parse(dt2.Rows[i][3].ToString()));
                    sheet1.GetRow(5 + i).CreateCell(2).SetCellValue(int.Parse(dt2.Rows[i][4].ToString()));
                }
                sheet1.ForceFormulaRecalculation = true;
                string sFileName = @"D:\CYCloudReport\File\TOC施工管理報表_" + DateTime.Now.ToString("yyyy_MM_dd") + @".xlsx";
                using (FileStream fileStream = File.Open(sFileName, FileMode.Create, FileAccess.ReadWrite))
                {
                    wk.Write(fileStream);
                    fileStream.Close();
                }
                SqlCommand sqlCommand = gConn.CreateCommand();
                byte[] MappFileNameByte = File.ReadAllBytes(sFileName);
                sql = @"insert Into MappMessage(MS_SYS_NAME, MM_TEXT_CONTENT, MM_CONTENT_TYPE, MM_subject, MM_TYPE, MM_MEDIA_CONTENT, MM_ExtFileName, MM_FILE_SHOW_NAME) values (
                        'TEST1', 'TOC施工管理報表', '3', 'TOC施工管理報表', 'A', @MappFileNameByte, 'xlsx', @MM_FILE_SHOW_NAME)";
                sqlCommand = gConn.CreateCommand();
                sqlCommand.Parameters.Add("@MappFileNameByte", SqlDbType.Binary, MappFileNameByte.Length).Value = MappFileNameByte;
                sqlCommand.Parameters.Add("@MM_FILE_SHOW_NAME", SqlDbType.VarChar, 50).Value = "TOC施工管理報表_" + DateTime.Now.ToString("yyyy_MM_dd") + @".xlsx";
                sqlCommand.CommandText = sql;
                sqlCommand.ExecuteNonQuery();
                if (gConn.State == ConnectionState.Open)
                {
                    gConn.Close();
                }
            }
        }



        class ReportTempA
        {
            public string FAC { get; set; }
            public string Vendor { get; set; }
            public int AccCnt { get; set; }
            public int ChkCnt { get; set; }
            public int T01 { get; set; }
            public int T02 { get; set; }
            public int T03 { get; set; }
            public int T04 { get; set; }
            public int T05 { get; set; }
            public int T06 { get; set; }
            public int T07 { get; set; }
            public int T08 { get; set; }
            public int T09 { get; set; }
            public int T10 { get; set; }
            public int T11 { get; set; }
        }

        class ReportTemp
        {
            public DateTime ConDate { get; set; }
            public string FacCode { get; set; }
            public string Vendor { get; set; }
            public int AccCnt { get; set; }
            public int ChkCnt { get; set; }
        }

        class CheckOutTemp
        {
            public int WORK_CHECKIN_ID { get; set; }
            public DateTime? checkout_time { get; set; } //[WORK_CHECKIN].[checkout_time]
            public string Fac { get; set; }
            public int InID { get; set; } //CHECKIN 的 IFP_RecognitionAuth_ID
            public DateTime InTime { get; set; }
            public string FRUserID { get; set; }
            public int OutID { get; set; } //CHECKOUT 的 IFP_RecognitionAuth_ID，NULL才取 [NewID]
            public DateTime OutTime { get; set; }
            public int NewID { get; set; } //CHECKOUT 的 IFP_RecognitionAuth_ID，
            public DateTime NewTime { get; set; }
        }
    }

    #region 類別定義

    [Serializable]
    public class WorkCheckIn
    {
        public int SEQ_ID { get; set; }
        public string con_number { get; set; }
        public DateTime con_date { get; set; }
        public DateTime? checkin_time { get; set; }
        public DateTime? checkout_time { get; set; }
        public string remark { get; set; }
        public DateTime? update_time { get; set; }
        public int? update_user { get; set; }
        public string fac_name { get; set; }
    }

    public class WorkKEY
    {
        public int con_number { get; set; }
    }

    public class WorkConNumber
    {
        public int Count { get; set; }
    }

    public class WorkCheckInLog
    {
        public int SEQ_ID { get; set; }
        public int WORK_CHECKIN_ID { get; set; }
        public string CHECK_TYPE { get; set; }
        public long IFP_RecognitionAuth_ID { get; set; }
        public DateTime? update_time { get; set; }
        public int? update_user { get; set; }

        public string FRUserName { get; set; }
    }

    public class WorkCheckAuthData
    {
        public string Name { get; set; }
        public string Supplier { get; set; }
        public DateTime Date { get; set; }
        public int InOut { get; set; }
        public string Type { get; set; }
    }

    public class WorkCheckOutLogDetail
    {
        public long IFP_RecognitionAuth_ID { get; set; }
        public string FRUserID { get; set; }
        public string FRUserName { get; set; }
        public DateTime? LogDateTime { get; set; }
        public string SupplierName { get; set; }
        public string CHECK_TYPE { get; set; }
        //public bool DeviceIn { get; set; }
        //public bool DeviceOut { get; set; }
    }

    public class WorkCheckInLogDetail : WorkCheckInLog
    {
        public string SupplierName { get; set; }
        public string FRUserID { get; set; }
        //public string FRUserName { get; set; }
        public DateTime? LogDateTime { get; set; }
        public bool DeviceIn { get; set; }
        public bool DeviceOut { get; set; }
    }
    public class WorkCheckInLogDetail_ : WorkCheckInLog
    {
        public string SupplierName { get; set; }
        public string FRUserID { get; set; }
        //public string FRUserName { get; set; }
        public DateTime? LogDateTime { get; set; }
        public bool DeviceIn { get; set; }
        public bool DeviceOut { get; set; }
    }

    public class VMT_FAC4
    {
        public int con_number { get; set; }
        public DateTime con_date { get; set; }
        public string fac_name { get; set; }
        public string fab_name { get; set; }
        public string main_area { get; set; }
        public string second_area { get; set; }
        public string vendor_name { get; set; }
        public string type1 { get; set; }
        public string type2 { get; set; }
        public string type3 { get; set; }
        public string type4 { get; set; }
        public string type5 { get; set; }
        public string con_conten { get; set; }
        public string engineer { get; set; }
        public string vendor_pe { get; set; }
        public int SEQ_ID { get; set; }
        public DateTime? checkin_time { get; set; }
        public DateTime? checkout_time { get; set; }
    }
    public class FACEINDATA
    {
        public string FRUserName { get; set; }
        public DateTime FaceinTime { get; set; }
        public string SupplierName { get; set; }

    }
    public class FACEOutDATA
    {
        public string FRUserName { get; set; }
        public DateTime FaceOutTime { get; set; }
        public string SupplierName { get; set; }

    }
    public class D2InDATA
    {
        public string FRUserName { get; set; }
        public DateTime D2LoginTime { get; set; }
        public string SupplierName { get; set; }
    }
    public class D2OutDATA
    {
        public string FRUserName { get; set; }
        public DateTime D2LogoutTime { get; set; }
        public string SupplierName { get; set; }

    }
    public class WorkCheckHourData
    {
        public string ConNumber { get; set; }
        public string Fac { get; set; }
        public string Area { get; set; }
        public string ConContent { get; set; }
        public string Vendor { get; set; }
        public string UserName { get; set; }
        public string VendorMain { get; set; }
        public string FRUserName { get; set; }
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string DeptName { get; set; }
        public string MappName { get; set; }
        //public DateTime BEGIN_TIME { get; set; }
        public DateTime END_TIME { get; set; }
    }
    public class LogData
    {
        public DateTime Time { get; set; } = DateTime.Now;
        public string Log { get; set; }

        public LogData(string log)
        {
            Log = log;
            Time = DateTime.Now;
        }
    }
    #endregion
}
