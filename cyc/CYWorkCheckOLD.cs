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

namespace CYCloud.WorkCheck.OLD
{
    //警報報表自動發送
    //[DisallowConcurrentExecutionAttribute()]
    public class AutoWorkCheck : cyc.Auto.AutoJob
    {
        static bool IsRunning = false;
        static cyc.Data.ExeResult oResult = new cyc.Data.ExeResult();

        protected override void Run()
        {
            cyc.Auto.Manager.Update("WorkCheckReport");
            if (!IsRunning)
            {
                IsRunning = true;
                try
                {
                    Shared.WorkCheckReport();
                }
                catch (Exception ex)
                {
                    cyc.Log.WriteSysErrorLog(ex.Message + ":" + ex.StackTrace); oResult.Error(ex.Message);
                }
                cyc.Auto.Manager.Update("WorkCheckReport", oResult);
                IsRunning = false;
            }
        }
    }

    public static class Shared
    {
        static string[] DeviceCheckIn = cyc.Shared.SysQuery.GetAppSettingValue("WorkCheckInDeviceCheckIn").Split(';');
        static string[] DeviceCheckOut = cyc.Shared.SysQuery.GetAppSettingValue("WorkCheckInDeviceCheckOut").Split(';');
        static string[] WorkCheckInCode = cyc.Shared.SysQuery.GetAppSettingValue("WorkCheckInCode").Split(';');
        static object oLockAdd = new object();
        static string sWorkAutoCheckIn = cyc.Shared.SysQuery.GetAppSettingValue("WorkAutoCheckIn");
        static string sWorkAutoCheckOut = cyc.Shared.SysQuery.GetAppSettingValue("WorkAutoCheckOut");

        public static WorkCheckIn GetWorkCheckInData(int Number, cyc.DB.SqlDapperConn oDB)
        {
            string qSql = "select A.*,B.con_date,B.fac_name from WORK_CHECKIN A inner join View_VMT_FAC B on A.con_number=B.con_number where A.con_number=@Number";
            WorkCheckIn xData = null;
            lock (oLockAdd) {
                try
                {
                    xData = oDB.QueryOne<WorkCheckIn>(qSql, new { Number });
                    if (xData == null)
                    {
                        //int iID = oDB.Query<int>("insert into WORK_CHECKIN (con_number) values (@Number);SELECT CAST(SCOPE_IDENTITY() as int)", new { Number }).Single();
                        int iID = oDB.Execute("insert into WORK_CHECKIN (con_number) values (@Number)", new { Number }, 0);
                        if (iID > 0)
                            xData = oDB.QueryOne<WorkCheckIn>(qSql, new { Number });
                    }
                }
                catch (Exception ex) { cyc.Log.WriteSysErrorLog("施工管理新增：" + ex.Message); }
            }
            return xData;
        }

        public static WorkKEY[] GetWorkCheckInData()
        {
            cyc.DB.SqlDapperConn oDB = new SqlDapperConn();
            string qSql = "select con_number from View_VMT_FAC where con_date >= convert(varchar(100),GETDATE(),23) + ' 00:00:00' and con_number not in (select con_number From WORK_CHECKIN)";
            WorkKEY[] xData = null;
            lock (oLockAdd)
            {
                try
                {
                    xData = oDB.QueryList<WorkKEY>(qSql).ToArray();
                    if (xData != null)
                    {
                        for (int i = 0; i < xData.Count(); i++)
                        {
                            oDB.Execute("insert into WORK_CHECKIN (con_number) values (@Number)", new { Number = int.Parse(xData[i].con_number.ToString()) });
                            //oDB.Connection.Query<int>("insert into WORK_CHECKIN (con_number) values (@Number)", new { Number = int.Parse(xData[i].con_number.ToString()) });
                        }
                    }
                }
                catch (Exception ex) { cyc.Log.WriteSysErrorLog("施工管理新增：" + ex.Message); }
            }
            return xData;
        }
        //public static IEnumerable<WorkCheckInLogDetail> GetWorkCheckInLog(int iID, string sType, cyc.DB.SqlDBConn oDB)
        //{
        //    return oDB.oConn.Query<WorkCheckInLogDetail>(@"
        //    select A.*,B.FRUserName,B.FRUserID,D.[Name] as SupplierName from WORK_CHECKIN_LOG A 
        //    inner join IFP_RecognitionAuth B on A.IFP_RecognitionAuth_ID=B.ID
        //    left join IFP_SupplierDriver C on B.FRUserID=C.Code and B.FRUserName=C.[Name]
        //    left join IFP_Supplier D on C.SupplierID=D.ID
        //    where A.WORK_CHECKIN_ID=@ID and A.CHECK_TYPE=@Type", new { ID = @iID, Type = sType });
        //}

        //public static IEnumerable<WorkCheckInLogDetail> GetRecognitionAuthCheckIn(DateTime dDate, cyc.DB.SqlDBConn oDB, int iID = 0)
        //{
        //    DateTime DateS = dDate;
        //    DateTime DateE = dDate.AddDays(1).AddMilliseconds(-1);

        //    return oDB.oConn.Query<WorkCheckInLogDetail>(@"
        //    select A.ID as IFP_RecognitionAuth_ID,A.LogDateTime,A.FRUserID,A.FRUserName
        //    ,C.[Name] as SupplierName,A.[Authorization] from IFP_RecognitionAuth A
        //    left join IFP_SupplierDriver B on A.FRUserID=B.Code and A.FRUserName=B.[Name]
        //    left join IFP_Supplier C on B.SupplierID=C.ID
        //    where A.LogDateTime between @DateS and @DateE and A.Code=@Code and A.DeviceName=@Device
        //    and not A.ID in (
        //        select A.IFP_RecognitionAuth_ID from WORK_CHECKIN_LOG A inner join WORK_CHECKIN B on A.WORK_CHECKIN_ID=B.SEQ_ID 
        //        where B.con_number in (select con_number from View_VMT_FAC4 where con_date=@Date) and B.SEQ_ID<>@ID
        //    )", new { DateS, DateE, Code = WorkCheckInCode, Device = DeviceCheckIn, Date = dDate, ID = iID });
        //}

        //public static IEnumerable<WorkCheckInLogDetail> GetRecognitionAuthCheckOut(DateTime dDate, int iID, cyc.DB.SqlDBConn oDB)
        //{
        //    DateTime DateS = dDate, DateE = dDate.AddDays(1).AddMilliseconds(-1);
        //    return oDB.oConn.Query<WorkCheckInLogDetail>(@"
        //    select A.ID as IFP_RecognitionAuth_ID,A.LogDateTime,A.FRUserID,A.FRUserName
        //    ,C.[Name] as SupplierName,A.[Authorization] from IFP_RecognitionAuth A
        //    left join IFP_SupplierDriver B on A.FRUserID=B.Code and A.FRUserName=B.[Name]
        //    left join IFP_Supplier C on B.SupplierID=C.ID
        //    where A.LogDateTime between @DateS and @DateE and A.Code=@Code and A.DeviceName=@Device
        //    and A.FRUserID in (select B.FRUserID from WORK_CHECKIN_LOG A inner join IFP_RecognitionAuth B on A.IFP_RecognitionAuth_ID=B.ID where A.WORK_CHECKIN_ID=@ID and A.CHECK_TYPE='CHECKIN')
        //    and not A.ID in (select IFP_RecognitionAuth_ID from WORK_CHECKIN_LOG where WORK_CHECKIN_ID=@ID and CHECK_TYPE='CHECKIN')
        //    ", new { DateS, DateE, Code = WorkCheckInCode, Device = DeviceCheckOut, ID = iID });
        //}

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

        public static IEnumerable<WorkCheckInLogDetail> GetWorkCheckInLogByConNumber(int ConNumber, cyc.DB.SqlDapperConn oDB)
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
            group by A.ID, A.FRUserID, B.P_NAME, A.LogDateTime, B.SHORT_NAME
            ", new { DateS, DateE, Fac });
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

        public static IEnumerable<WorkCheckInLogDetail> GetWorkCheckInLogForLogout(int WorkID, DateTime cDate, string Fac, cyc.DB.SqlDapperConn oDB)
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
where  C.con_number = @WorkID and M.VNTFAC = @Fac
group by A.ID, A.FRUserID, E.P_NAME, A.LogDateTime, B.WORK_CHECKIN_ID, B.CHECK_TYPE, B.SEQ_ID,E.SHORT_NAME
            ", new { WorkID, Fac, DateS = cDate, DateE = cDate.AddDays(1).AddMilliseconds(-1)  });
        }

        public static IEnumerable<WorkCheckInLogDetail> GetWorkCheckInLogByDate__(int WorkID, cyc.DB.SqlDapperConn oDB, string Fac)
        {

            return oDB.QueryList<WorkCheckInLogDetail>(@"
            select A.ID as IFP_RecognitionAuth_ID,A.FRUserID,E.P_NAME as FRUserName,A.LogDateTime
            ,B.WORK_CHECKIN_ID,ISNULL(B.CHECK_TYPE,'')as CHECK_TYPE,B.SEQ_ID,E.SHORT_NAME as SupplierName
            from RecognitionAuth A
            inner join WORK_CHECKIN_LOG B on A.ID=B.IFP_RecognitionAuth_ID
            inner join WORK_CHECKIN C on C.SEQ_ID = B.WORK_CHECKIN_ID
            left join AccessList2 E on A.FRUserID = E.ID
            inner join AccessListMapping on E.LOCATION = AccessListMapping.LOCATION and A.Fac = AccessListMapping.FAC
            where  C.con_number = @WorkID and AccessListMapping.VNTFAC = @Fac
            group by A.ID, A.FRUserID, E.P_NAME, A.LogDateTime, B.WORK_CHECKIN_ID, B.CHECK_TYPE, B.SEQ_ID,E.SHORT_NAME
            ", new { WorkID , Fac });
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

        public static IEnumerable<WorkCheckOutLogDetail> GetWorkCheckOutLogByDate(int WorkID, cyc.DB.SqlDapperConn oDB, string Fac)
        {
            return oDB.QueryList<WorkCheckOutLogDetail>(@"
			select Distinct A.ID as IFP_RecognitionAuth_ID,A.FRUserID,E.P_NAME as FRUserName,A.LogDateTime
            ,E.SHORT_NAME as SupplierName
            from RecognitionAuth A
            left join AccessList2 E on A.FRUserID = E.ID
            inner join AccessListMapping on E.LOCATION = AccessListMapping.LOCATION and A.Fac = AccessListMapping.FAC
			where A.FRUserID in (            select A.FRUserID
            from RecognitionAuth A
            inner join WORK_CHECKIN_LOG B on A.ID=B.IFP_RecognitionAuth_ID
            inner join WORK_CHECKIN C on C.SEQ_ID = B.WORK_CHECKIN_ID
            inner join AccessListMapping on E.LOCATION = AccessListMapping.LOCATION and A.Fac = AccessListMapping.FAC
			where C.con_number = @WorkID	and b.CHECK_TYPE = 'CHECKIN')	and DATEADD(MINUTE,-1, A.LogDateTime) > (select checkin_time From WORK_CHECKIN where con_number = @WorkID)
            and E.EV_DATE >= convert(varchar(100),GETDATE(),23) + ' 00:00:00'
			and A.ID not in (select IFP_RecognitionAuth_ID From WORK_CHECKIN_LOG Where CHECK_TYPE = 'CHECKIN') and AccessListMapping.VNTFAC = @Fac
            ", new { WorkID, Fac });
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
        public static IEnumerable<string> GetWorkCheckInSupplier(DateTime dDate, string facName, cyc.DB.SqlDapperConn oDB)
        {
            DateTime DateS = dDate, DateE = dDate.AddDays(1).AddMilliseconds(-1);
            
            return oDB.Connection.Query<string>(@"
            select distinct A.SHORT_NAME from RecognitionAuth C
            left join AccessList2 A on A.ID = C.FRUserID
            inner join AccessListMapping B on A.LOCATION = B.LOCATION and C.Fac = B.FAC
            where A.EV_DATE between @DateS and @DateE and B.VNTFAC = @facName
            ", new { DateS, DateE , facName });
        }

        public static void AutoCheckIn(List<CYCloud.IFP.RecognitionAuth> AuthList)
        {
            if (sWorkAutoCheckIn != "1") { return; }

            //篩選 符合CheckIn資料
            var xData = CYCloud.WorkCheck.Shared.GetWorkCheckInData();
            var WorkList = AuthList.OrderBy(p => p.LogDateTime);

            if (WorkList != null && WorkList.Count() > 0)
            {
                try
                {
                    using (cyc.DB.SqlDapperConn oDB = new SqlDapperConn())
                    {
                        foreach (var WorkData in WorkList)
                        {
                            var ExistList = oDB.Connection.Query<CheckInData>(@"
select distinct WORK_CHECKIN.SEQ_ID as WORK_CHECKIN_ID, RecognitionAuth.ID as IFP_RecognitionAuth_ID, FRUserID From WORK_CHECKIN 
Left Join AccessList2 on WORK_CHECKIN.con_number = AccessList2.APPLY_PK 
inner join RecognitionAuth on RecognitionAuth.FRUserID = AccessList2.ID 
inner join AccessListMapping on AccessList2.LOCATION = AccessListMapping.LOCATION and RecognitionAuth.Fac = AccessListMapping.FAC
inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number and View_VMT_FAC.fac_name = AccessListMapping.VNTFAC
where  Direction = 1 and RecognitionAuth.ID = @ID and RecognitionAuth.Fac = @Fac and RecognitionAuth.LogDateTime >= AccessList2.EV_DATE 
and View_VMT_FAC.con_date >= convert(varchar(100),GETDATE(),23) + ' 00:00:00'", new { ID = WorkData.ID, Fac = WorkData.Fac }).ToList();
                            foreach (var inData in ExistList)
                            {
                                oDB.Connection.Execute("update WORK_CHECKIN set checkin_time=@Date, update_time = @Date, update_user = 0 where SEQ_ID=@ID and checkin_time is null", new { ID = inData.WORK_CHECKIN_ID, Date = DateTime.Now });
                                var dList = oDB.Connection.Query<CYCloud.WorkCheck.WorkConNumber>("Select count(*) as count From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN_LOG.WORK_CHECKIN_ID = WORK_CHECKIN.SEQ_ID inner join RecognitionAuth on WORK_CHECKIN_LOG.IFP_RecognitionAuth_ID = RecognitionAuth.ID  Where RecognitionAuth.FRUserID = @FRUserID and WORK_CHECKIN.SEQ_ID = @WORK_CHECKIN_ID", new { FRUserID = inData.FRUserID, WORK_CHECKIN_ID = inData.WORK_CHECKIN_ID }).ToList();
                                foreach (var ddata in dList)
                                {
                                    if (ddata.Count < 1)
                                    {
                                        oDB.Connection.Execute(@"insert into WORK_CHECKIN_LOG (WORK_CHECKIN_ID,CHECK_TYPE,IFP_RecognitionAuth_ID,update_time,update_user) values( @WORK_CHECKIN_ID, 'CHECKIN', @IFP_RecognitionAuth_ID, @Date, @update_user )", new { work_checkin_ID = inData.WORK_CHECKIN_ID, IFP_RecognitionAuth_ID = inData.IFP_RecognitionAuth_ID, update_user = 0, Date = DateTime.Now });
                                        oDB.Connection.Execute(@"Update RecognitionAuth  set UseFlag = 'Y' Where ID = @IFP_RecognitionAuth_ID", new { IFP_RecognitionAuth_ID = inData.IFP_RecognitionAuth_ID });

                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex) { cyc.Log.WriteSysErrorLog("自動簽到：" + ex.Message); }
            }
        }


        public static void AutoCheckOut(List<CYCloud.IFP.RecognitionAuth> AuthList)
        {
            if (sWorkAutoCheckOut != "1") { return; }

            //篩選 符合Checkout資料
            var WorkList = AuthList.OrderBy(p => p.LogDateTime);

            if (WorkList != null && WorkList.Count() > 0)
            {
                try
                {
                    DateTime DateS = DateTime.Today, DateE = DateS.AddDays(1).AddMilliseconds(-1);
                    using (cyc.DB.SqlDapperConn oDB = new SqlDapperConn())
                    {
                        var ExistList = oDB.Connection.Query<CheckOutData>(@"
                    select A.SEQ_ID as MainID,A.checkout_time as MainCheckOut,B.CHECK_TYPE as Type,B.IFP_RecognitionAuth_ID as AuthID,C.LogDateTime as LogDate,C.FRUserID as UserID, View_WORKLOG.Fac 
                    from WORK_CHECKIN A
					inner join View_WORKLOG on A.con_number = View_WORKLOG.con_number 
                    inner join WORK_CHECKIN_LOG B on A.SEQ_ID=b.WORK_CHECKIN_ID
                    inner join RecognitionAuth C on B.IFP_RecognitionAuth_ID=C.ID
                    where checkin_time between @DateS and @DateE ", new { DateS, DateE}).ToList();
                        var OutList = new List<WorkCheckInLog>();

                        foreach (var WorkData in WorkList)
                        {
                    //        var ExistList = oDB.oConn.Query<CheckOutData>(@"
                    //select A.SEQ_ID as MainID,A.checkout_time as MainCheckOut,B.CHECK_TYPE as Type,B.IFP_RecognitionAuth_ID as AuthID,C.LogDateTime as LogDate,C.FRUserID as UserID
                    //from WORK_CHECKIN A
                    //inner join WORK_CHECKIN_LOG B on A.SEQ_ID=b.WORK_CHECKIN_ID
                    //inner join IFP_RecognitionAuth C on B.IFP_RecognitionAuth_ID=C.ID
                    //where C.LogContent = 'Face Identify Pass' and C.FRUserID = @ID", new {ID = WorkData.FRUserID}).ToList();
                            foreach (var InData in ExistList.Where(p => p.Type == "CHECKIN" && p.UserID.Trim() == WorkData.FRUserID.Trim() && p.LogDate.AddMinutes(10) < WorkData.LogDateTime && p.FAC == WorkData.Fac))
                            {
                                if (!ExistList.Any(p => p.MainID == InData.MainID && p.Type == "CHECKOUT" && p.UserID.Trim() == WorkData.FRUserID.Trim()))
                                {
                                    var dList = oDB.Connection.Query<CYCloud.WorkCheck.WorkConNumber>("Select count(*) as count From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN_LOG.WORK_CHECKIN_ID = WORK_CHECKIN.SEQ_ID inner join RecognitionAuth on WORK_CHECKIN_LOG.IFP_RecognitionAuth_ID = RecognitionAuth.ID  Where RecognitionAuth.FRUserID = @FRUserID and WORK_CHECKIN.SEQ_ID = @WORK_CHECKIN_ID and WORK_CHECKIN_LOG.CHECK_TYPE = 'CHECKOUT'", new { FRUserID = InData.UserID, WORK_CHECKIN_ID = InData.MainID }).ToList();
                                    foreach (var ddata in dList)
                                    {
                                        if (ddata.Count < 1)
                                        {
                                            oDB.Connection.Execute(@"
                                    insert into WORK_CHECKIN_LOG (WORK_CHECKIN_ID,CHECK_TYPE,IFP_RecognitionAuth_ID,update_time,update_user)
                                    values (@WORK_CHECKIN_ID,@CHECK_TYPE,@IFP_RecognitionAuth_ID,getdate(),@update_user)", new { WORK_CHECKIN_ID = InData.MainID, CHECK_TYPE = "CHECKOUT", IFP_RecognitionAuth_ID = WorkData.ID, update_user = 0 });

                                            //OutList.Add(new WorkCheckInLog() { WORK_CHECKIN_ID = InData.MainID, CHECK_TYPE = "CHECKOUT", IFP_RecognitionAuth_ID = WorkData.ID, update_user = 0 });
                                        }
                                    }

                                    if (InData.MainCheckOut == null)//如果沒CheckOut
                                    {
                                        //將所有相同WorkCheckIn的
                                        foreach (var m in ExistList.Where(p => p.MainID == InData.MainID)) { m.MainCheckOut = DateTime.Now; }
                                        oDB.Connection.Execute("update WORK_CHECKIN set checkout_time=@Date where SEQ_ID=@ID and checkout_time is null", new { ID = InData.MainID, Date = DateTime.Now });
                        //                oDB.oConn.Execute(@"Update RecognitionAuth  set UseFlag = 'Y' Where ID = @IFP_RecognitionAuth_ID", new { IFP_RecognitionAuth_ID = WorkData.ID });
                        //                oDB.oConn.Execute(@"
                        //insert into WORK_CHECKIN_LOG (WORK_CHECKIN_ID,CHECK_TYPE,IFP_RecognitionAuth_ID,update_time,update_user)
                        //values (@WORK_CHECKIN_ID,@CHECK_TYPE,@IFP_RecognitionAuth_ID,getdate(),@update_user)", new { WORK_CHECKIN_ID = InData.MainID, CHECK_TYPE = "CHECKOUT", IFP_RecognitionAuth_ID = WorkData.ID, update_user = 0 });

                                    }

                                    //oDB.oConn.Execute(@"Update RecognitionAuth  set UseFlag = 'Y' Where ID = @IFP_RecognitionAuth_ID", new { IFP_RecognitionAuth_ID = WorkData.ID });
                                    //oDB.oConn.Execute(@"
                                    //insert into WORK_CHECKIN_LOG (WORK_CHECKIN_ID,CHECK_TYPE,IFP_RecognitionAuth_ID,update_time,update_user)
                                    //values (@WORK_CHECKIN_ID,@CHECK_TYPE,@IFP_RecognitionAuth_ID,getdate(),@update_user)", new { WORK_CHECKIN_ID = InData.MainID, CHECK_TYPE = "CHECKOUT", IFP_RecognitionAuth_ID = WorkData.ID, update_user = 0 });

                                    //var dList = oDB.oConn.Query<CYCloud.WorkCheck.WorkConNumber>("Select count(*) as count From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN_LOG.WORK_CHECKIN_ID = WORK_CHECKIN.SEQ_ID inner join IFP_RecognitionAuth on WORK_CHECKIN_LOG.IFP_RecognitionAuth_ID = IFP_RecognitionAuth.ID  Where IFP_RecognitionAuth.FRUserID = @FRUserID and WORK_CHECKIN.SEQ_ID = @WORK_CHECKIN_ID", new { FRUserID = InData.UserID, WORK_CHECKIN_ID = InData.MainID }).ToList();
                                    //foreach (var ddata in dList)
                                    //{
                                    //    if (ddata.Count > 0)
                                    //    {
                                    //        ExistList.Remove(InData);//移除，避免重複
                                    //    }
                                    //}
                                    //break;//有找到就離開foreach
                                }
                            }
                            oDB.Connection.Execute(@"Update RecognitionAuth  set UseFlag2 = 'Y' Where ID = @IFP_RecognitionAuth_ID", new { IFP_RecognitionAuth_ID = WorkData.ID });
                        }
                    }

                }
                catch (Exception ex) { cyc.Log.WriteSysErrorLog("自動簽退：" + ex.Message); }
            }
        }

        public static void WorkCheckReport()
        {
            try
            {
                //if (DateTime.Now.Hour == 18 && DateTime.Now.Minute == 00)
                //{
                    SqlConnection gConn = new SqlConnection();
                    DataTable dt = new DataTable();
                    DataTable dt2 = new DataTable();
                    SqlDataAdapter da = new SqlDataAdapter();
                    gConn = new SqlConnection(cyc.DB.ConnString.Main);
                    if (gConn.State == ConnectionState.Closed)
                    {
                        gConn.Open();
                    }
                    string sql = @"select 'FAC1', (select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + @"' and fac_name = 'FAB1廠') as FAC1施工總數, 
                (select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + "' and fac_name = 'FAB1廠') as FAC1刷臉報到總數, " +
                                   "round(cast((select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + "' and fac_name = 'FAB1廠') as Float) / cast ((select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + @"' and fac_name = 'FAB1廠') as float) ,2)";
                    da = new SqlDataAdapter(sql, gConn); da = new SqlDataAdapter(sql, gConn);
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

                    sql = @"select 'FAC2', (select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + @"' and fac_name = 'FAB2廠') as FAC2施工總數, 
                (select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + "' and fac_name = 'FAB2廠') as FAC2刷臉報到總數, " +
                                   "round(cast((select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + "' and fac_name = 'FAB2廠') as Float) / cast ((select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + @"' and fac_name = 'FAB2廠') as float) ,2)";
                    da = new SqlDataAdapter(sql, gConn); da = new SqlDataAdapter(sql, gConn);
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

                    sql = @"select 'FAC3', (select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + @"' and fac_name = 'FAB3廠') as FAC3施工總數, 
                (select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + "' and fac_name = 'FAB3廠') as FAC3刷臉報到總數, " +
                   "round(cast((select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + "' and fac_name = 'FAB3廠') as Float) / cast ((select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + @"' and fac_name = 'FAB3廠') as float) ,2)";
                    da = new SqlDataAdapter(sql, gConn); da = new SqlDataAdapter(sql, gConn);
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

                    sql = @"select 'FAC5', (select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + @"' and fac_name = 'FAB5廠') as FAC5施工總數, 
                (select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + "' and fac_name = 'FAB5廠') as FAC5刷臉報到總數, " +
                   "round(cast((select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + "' and fac_name = 'FAB5廠') as Float) / cast ((select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + @"' and fac_name = 'FAB5廠') as float) ,2)";
                    da = new SqlDataAdapter(sql, gConn); da = new SqlDataAdapter(sql, gConn);
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
                    sql = @"select 'FAC6', (select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + @"' and fac_name = 'FAB6廠') as FAC6施工總數, 
                (select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + "' and fac_name = 'FAB6廠') as FAC6刷臉報到總數, " +
                   "round(cast((select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + "' and fac_name = 'FAB6廠') as Float) / cast ((select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + @"' and fac_name = 'FAB6廠') as float) ,2)";
                    da = new SqlDataAdapter(sql, gConn); da = new SqlDataAdapter(sql, gConn);
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
                    sql = @"select 'FAC7', (select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + @"' and fac_name = 'FAB7廠') as FAC7施工總數, 
                (select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + "' and fac_name = 'FAB7廠') as FAC7刷臉報到總數, " +
                   "round(cast((select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + "' and fac_name = 'FAB7廠') as Float) / cast ((select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + @"' and fac_name = 'FAB7廠') as float) ,2)";
                    da = new SqlDataAdapter(sql, gConn); da = new SqlDataAdapter(sql, gConn);
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
                    sql = @"select 'FAC8', (select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + @"' and (fac_name = 'FAB8廠' or fac_name = 'T6廠')) as FAC8施工總數, 
                (select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + "' and (fac_name = 'FAB8廠' or fac_name = 'T6廠')) as FAC8刷臉報到總數, " +
                   "round(cast((select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + "' and (fac_name = 'FAB8廠' or fac_name = 'T6廠')) as Float) / cast ((select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + @"' and (fac_name = 'FAB8廠' or fac_name = 'T6廠')) as float) ,2)";
                    da = new SqlDataAdapter(sql, gConn); da = new SqlDataAdapter(sql, gConn);
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
                    sql = @"select 'FACC', (select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + @"' and (fac_name = 'FAB C廠' or fac_name = 'C3&CG' or fac_name = 'C3&CG廠' or fac_name = 'FAB C' or fac_name = 'MOD2' or fac_name = 'MOD2$MOD4廠' or fac_name = '科九廠' or fac_name = '群豐駿')) as FACC施工總數, 
                (select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + "' and (fac_name = 'FAB C廠' or fac_name = 'C3&CG' or fac_name = 'C3&CG廠' or fac_name = 'FAB C' or fac_name = 'MOD2' or fac_name = 'MOD2$MOD4廠' or fac_name = '科九廠' or fac_name = '群豐駿')) as FACC刷臉報到總數, " +
    "round(cast((select count(distinct(WORK_CHECKIN.con_number)) From WORK_CHECKIN_LOG inner join WORK_CHECKIN on WORK_CHECKIN.SEQ_ID = WORK_CHECKIN_LOG.WORK_CHECKIN_ID inner join View_VMT_FAC on View_VMT_FAC.con_number = WORK_CHECKIN.con_number inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name  where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + "' and (fac_name = 'FAB C廠' or fac_name = 'C3&CG' or fac_name = 'C3&CG廠' or fac_name = 'FAB C' or fac_name = 'MOD2' or fac_name = 'MOD2$MOD4廠' or fac_name = '科九廠' or fac_name = '群豐駿')) as Float) / cast ((select count(*) From View_VMT_FAC inner join SysUser on replace(SUBSTRING(View_VMT_FAC.engineer,0,5),'(','') = SysUser.Name where con_date >= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00" + @"' and con_date <= '" + DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:59" + @"' and (fac_name = 'FAB C廠' or fac_name = 'C3&CG' or fac_name = 'C3&CG廠' or fac_name = 'FAB C' or fac_name = 'MOD2' or fac_name = 'MOD2$MOD4廠' or fac_name = '科九廠' or fac_name = '群豐駿')) as float) ,2)";
                    da = new SqlDataAdapter(sql, gConn); da = new SqlDataAdapter(sql, gConn);
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
                    if (gConn.State == ConnectionState.Open)
                    {
                        gConn.Close();
                    }
                //}

            }
            catch (Exception ex) { cyc.Log.WriteSysErrorLog("施工管理報表新增：" + ex.Message); }
        }
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

    }

    #region 類別定義

    class CheckInData
    {
        public int WORK_CHECKIN_ID { get; set; }
        public int IFP_RecognitionAuth_ID { get; set; }
        public string FRUserID { get; set; }
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

    [Serializable]
    public class WorkCheckIn
    {
        public int SEQ_ID { get; set; }
        public int con_number { get; set; }
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

    public class WorkCheckOutLogDetail
    {
        public string SupplierName { get; set; }
        public string FRUserID { get; set; }
        public string FRUserName { get; set; }
        public DateTime? LogDateTime { get; set; }
        public bool DeviceIn { get; set; }
        public bool DeviceOut { get; set; }
        public long IFP_RecognitionAuth_ID { get; set; }
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
    #endregion
}
