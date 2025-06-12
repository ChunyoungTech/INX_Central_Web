using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using cyc.Page;
using Dapper;

namespace WebApp._edit
{
    public partial class WorkCheckInList : cyc.Page.BasePageSub
    {
        string ConNumber = string.Empty;
        public class WorkCheckInLogDetail
        {
            public string SupplierName { get; set; }
            public string FRUserID { get; set; }
            public string FRUserName { get; set; }
            public DateTime? LogDateTime { get; set; }
            public bool DeviceIn { get; set; }
            public bool DeviceOut { get; set; }
            public int SEQ_ID { get; set; }
            public int WORK_CHECKIN_ID { get; set; }
            public string CHECK_TYPE { get; set; }
            public long IFP_RecognitionAuth_ID { get; set; }
            public DateTime? update_time { get; set; }
            public int? update_user { get; set; }
        }

        protected override void OnLoad(EventArgs e)
        {
            ConNumber = Request.QueryString["pa"];
            base.OnLoad(e);
        }

        protected override SubPageOption SetPageOption() => new SubPageOption()
        {
            CheckOpen = "open.aspx;WorkCheckInList.aspx",
            Confirm = btnConfirm,
            Parameter = "pa",
            IsIntPa = false
        };
        protected override void LoadData()
        {
            //var xData = CYCloud.WorkCheck.Shared.GetWorkCheckInData(Convert.ToInt32(Request.QueryString["pa"]), bDB);
            var xData = CYCloud.WorkCheck.Shared.GetWorkCheckInData(ConNumber, dDB);
            if (xData != null && xData.con_date != null)
            {
                //施工日期
                ViewState["date"] = xData;

                ////載入廠商
                //var pList = CYCloud.WorkCheck.Shared.GetWorkCheckInSupplier(xData.con_date, xData.fac_name, bDB);
                //foreach (var q in pList)
                //    ddlSupplier.Items.Add(q);
                //ddlSupplier.Items.Insert(0, "");

                //可挑選名單
                var qList = CYCloud.WorkCheck.Shared.GetWorkCheckInLogByDate(xData.con_date, dDB, xData.fac_name);
                //載入廠商
                foreach (var q in qList.Select(p => p.SupplierName).Distinct())
                    ddlSupplier.Items.Add(q);
                ddlSupplier.Items.Insert(0, "");

                var gList = qList;
                GridView1.DataSource = gList;
                GridView1.DataBind();

                //已挑選名單
                var cList = CYCloud.WorkCheck.Shared.GetWorkCheckInLogByConNumber(ConNumber, dDB);
                if (cList != null && cList.Count() > 0)
                {
                    System.Text.StringBuilder oStr = new System.Text.StringBuilder("");
                    var sList = from c in cList
                                join q in qList on c.IFP_RecognitionAuth_ID equals q.IFP_RecognitionAuth_ID
                                select new WorkCheckInLogDetail
                                { IFP_RecognitionAuth_ID = c.IFP_RecognitionAuth_ID, FRUserID = c.FRUserID, FRUserName = q.FRUserName, SupplierName = q.SupplierName };
                    foreach (var oData in sList)
                        oStr.AppendFormat("<li data-id='{0}'><input type='button' class='btnRemove' value='移除' data-code='{3}' {4} />{1}-{2}</li>", oData.IFP_RecognitionAuth_ID, oData.FRUserName, oData.SupplierName, oData.FRUserID, qList.Any(p => p.WORK_CHECKIN_ID == xData.SEQ_ID && p.CHECK_TYPE == "CHECKOUT" && p.FRUserID == oData.FRUserID) ? "disabled" : "");
                    ltlCont.Text = oStr.ToString();
                }

                //System.Text.StringBuilder oStr = new System.Text.StringBuilder("");
                //foreach (var oData in gList)
                //{
                //    var hList = CYCloud.WorkCheck.Shared.GetWorkCheckInLogByDate_(oData.IFP_RecognitionAuth_ID, ConNumber, bDB);
                //    var sList = hList.Where(p => p.CHECK_TYPE == "CHECKIN");
                //    foreach (var oData2 in sList)
                //        oStr.AppendFormat("<li data-id='{0}'><input type='button' class='btnRemove' value='移除' data-code='{3}' {4} />{1}-{2}</li>", oData2.IFP_RecognitionAuth_ID, oData2.FRUserName, oData2.SupplierName, oData2.FRUserID, qList.Any(p => p.WORK_CHECKIN_ID == xData.SEQ_ID && p.CHECK_TYPE == "CHECKOUT" && p.FRUserID == oData.FRUserID) ? "disabled" : "");
                //}
                //ltlCont.Text = oStr.ToString();
            }
            else
                oResult.Error("查無資料");
        }
        protected override void SaveCheck()
        {
            var sIDs = hidValue.Value.Split(',').Where(p => p.Trim().Length > 0);
            foreach (var sID in sIDs)
            {
                if (!cyc.Shared.Check.IsInteger(sID))
                { oResult.Error("參數錯誤"); break; }
            }
        }
        protected override void SaveData()
        {
            var wList = hidValue.Value.Split(',').Where(p => p.Trim().Length > 0).ToList();

            var xData = CYCloud.WorkCheck.Shared.GetWorkCheckInData(ConNumber, dDB);

            if (xData != null)
            {
                //當日有紀錄人員
                var rList = CYCloud.WorkCheck.Shared.GetWorkCheckInLogByDate(xData.con_date, dDB, xData.fac_name);
                //目前工單已登錄人員
                var eList = CYCloud.WorkCheck.Shared.GetWorkCheckInLogForLogout(ConNumber, xData.con_date, xData.fac_name, dDB);

                ////檢查是否已存在其他表單
                //var eList = from qls in qList
                //            join nls in nList on qls.IFP_RecognitionAuth_ID.ToString() equals nls
                //            where !(qls.WORK_CHECKIN_ID == 0 || (qls.WORK_CHECKIN_ID == xData.SEQ_ID && qls.CHECK_TYPE == "CHECKIN"))
                //            select qls;
                //if (eList.Count() > 0) { oResult.Error(string.Join("、", eList.Select(p => p.FRUserName)) + "，已存在其它表單"); return; }

                //取得目前選取人員名單，並檢查是否存在不合法資料
                var nList = from w in wList
                            join r in rList on w equals r.IFP_RecognitionAuth_ID.ToString()
                            select r;
                if (nList.Count() != wList.Count) { oResult.Error("選取資料不存在系統"); return; }

                //var sList = from n in wList
                //            join r in rList on n equals r.IFP_RecognitionAuth_ID.ToString() into xx
                //            from r in xx.DefaultIfEmpty()
                //            select r.FRUserID ?? "";
                ////檢查是否選取不存在系統編號
                //if (sList.Any(p => p.Length == 0)) { oResult.Error("選取資料不存在系統"); return; }

                ////檢查是否重複選取相同人員
                //if (sList.GroupBy(p => p).Any(q => q.Count() > 1)) { oResult.Error("重複選取相同人員"); return; }

                //原本單人員
                var oList = eList.Where(p => p.WORK_CHECKIN_ID == xData.SEQ_ID && p.CHECK_TYPE == "CHECKIN").ToList();
                //欲移除人員
                var dList = (from ols in oList
                             join nls in nList on ols.IFP_RecognitionAuth_ID equals nls.IFP_RecognitionAuth_ID into xx
                             from nls in xx.DefaultIfEmpty()
                             where nls == null
                             select ols).ToList();
                //欲新增人員
                var iList = (from nls in nList
                             join ols in oList on nls.IFP_RecognitionAuth_ID equals ols.IFP_RecognitionAuth_ID into xx
                             from ols in xx.DefaultIfEmpty()
                             where ols == null
                             select new CYCloud.WorkCheck.WorkCheckInLog { WORK_CHECKIN_ID = xData.SEQ_ID, CHECK_TYPE = "CHECKIN", IFP_RecognitionAuth_ID = nls.IFP_RecognitionAuth_ID, FRUserName = nls.FRUserName, update_user = bUser.User.ID }).ToList();

                if (iList.Count + dList.Count > 0)
                {
                    using (var oDB = new cyc.DB.SqlDapperConn(oResult, null, true))
                    {
                        try
                        {
                            if (dList.Count > 0)
                                oDB.Execute("delete from WORK_CHECKIN_LOG where SEQ_ID=@SEQ_ID and WORK_CHECKIN_ID=@WORK_CHECKIN_ID", dList);

                            if (oResult.Success && iList.Count > 0)
                                oDB.Execute("insert into WORK_CHECKIN_LOG (WORK_CHECKIN_ID,CHECK_TYPE,IFP_RecognitionAuth_ID,update_time,update_user) values (@WORK_CHECKIN_ID,@CHECK_TYPE,@IFP_RecognitionAuth_ID,getdate(),@update_user)", iList);

                            if (oResult.Success)
                            {
                                int iCheck = oList.Count + iList.Count - dList.Count;
                                if (xData.checkin_time == null && iCheck > 0)
                                    oDB.Execute("update WORK_CHECKIN set checkin_time=getdate(),update_user=@User,update_time=getdate() where SEQ_ID=@ID", new { ID = xData.SEQ_ID, User = bUser.User.ID });
                                else if (xData.checkin_time != null && iCheck == 0)
                                    oDB.Execute("update WORK_CHECKIN set checkin_time=NULL,update_user=@User,update_time=getdate() where SEQ_ID=@ID", new { ID = xData.SEQ_ID, User = bUser.User.ID });

                                if (oResult.Success)
                                {
                                    var oLog = new CYCloud.ExecLog.LogItem { ExecID = Convert.ToInt32(Request.QueryString["app"]), ExecType = "update", UserID = bUser.User.ID };
                                    oLog.ExecDesc = string.Format("工單({0})手動報到：新增-{1}，移除-{2}", xData.con_number, (iList.Count > 0 ? string.Join("、", iList.Select(p => p.FRUserName)) : "無"), (dList.Count > 0 ? string.Join("、", dList.Select(p => p.FRUserName)) : "無"));
                                    CYCloud.ExecLog.WriteLog(oLog, oDB);
                                }
                            }
                        }
                        catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex.Message + ":" + ex.StackTrace, oResult); }
                        finally { oDB.ResultTransaction(); }
                    }
                }
            }
            else
                oResult.Error("查無資料");
        }
        //protected override void SaveData()
        //{
        //    var nList = hidValue.Value.Split(',').Where(p => p.Trim().Length > 0).ToList();

        //    //var xData = CYCloud.WorkCheck.Shared.GetWorkCheckInData(Convert.ToInt32(Request.QueryString["pa"]), bDB);
        //    var xData = CYCloud.WorkCheck.Shared.GetWorkCheckInData(ConNumber, bDB);

        //    if (xData != null)
        //    {
        //        var qList = CYCloud.WorkCheck.Shared.GetWorkCheckInLogByDate(xData.con_date, bDB, xData.fac_name);
        //        var hList = CYCloud.WorkCheck.Shared.GetWorkCheckInLogByDate__(ConNumber, bDB, xData.fac_name);

        //        ////檢查是否已存在其他表單
        //        //var eList = from qls in qList
        //        //            join nls in nList on qls.IFP_RecognitionAuth_ID.ToString() equals nls
        //        //            where !(qls.WORK_CHECKIN_ID == 0 || (qls.WORK_CHECKIN_ID == xData.SEQ_ID && qls.CHECK_TYPE == "CHECKIN"))
        //        //            select qls;
        //        //if (eList.Count() > 0) { oResult.Error(string.Join("、", eList.Select(p => p.FRUserName)) + "，已存在其它表單"); return; }

        //        var sList = from nls in nList
        //                    join qls in qList on nls equals qls.IFP_RecognitionAuth_ID.ToString() into xx
        //                    from qls in xx.DefaultIfEmpty()
        //                    select qls.FRUserID ?? "";
        //        //檢查是否選取不存在系統編號
        //        if (sList.Any(p => p.Length == 0)) { oResult.Error("選取資料不存在系統"); return; }
        //        ////檢查是否重複選取相同人員
        //        //if (sList.GroupBy(p => p).Any(q => q.Count() > 1)) { oResult.Error("重複選取相同人員"); return; }

        //        //原本單人員
        //        var oList = hList.Where(p => p.WORK_CHECKIN_ID == xData.SEQ_ID && p.CHECK_TYPE == "CHECKIN");
        //        //欲移除人員
        //        var dList = from ols in oList
        //                    join nls in nList on ols.IFP_RecognitionAuth_ID.ToString() equals nls into xx
        //                    from nls in xx.DefaultIfEmpty()
        //                    where nls == null
        //                    select ols;
        //        //欲新增人員
        //        var iList = from nls in nList
        //                    join ols in oList on nls equals ols.IFP_RecognitionAuth_ID.ToString() into xx
        //                    from ols in xx.DefaultIfEmpty()
        //                    where ols == null
        //                    select new CYCloud.WorkCheck.WorkCheckInLog { WORK_CHECKIN_ID = xData.SEQ_ID, CHECK_TYPE = "CHECKIN", IFP_RecognitionAuth_ID = Convert.ToInt64(nls), update_user = bUser.User.ID };
        //        try
        //        {
        //            if (dList.Count() > 0)
        //                bDB.oConn.Execute("delete from WORK_CHECKIN_LOG where SEQ_ID=@SEQ_ID and WORK_CHECKIN_ID=@WORK_CHECKIN_ID", dList);
        //            if (iList.Count() > 0)
        //                bDB.oConn.Execute("insert into WORK_CHECKIN_LOG (WORK_CHECKIN_ID,CHECK_TYPE,IFP_RecognitionAuth_ID,update_time,update_user) values (@WORK_CHECKIN_ID,@CHECK_TYPE,@IFP_RecognitionAuth_ID,getdate(),@update_user)", iList);

        //            int iCheck = oList.Count() + iList.Count() - dList.Count();
        //            if (xData.checkin_time == null && iCheck > 0)
        //                bDB.oConn.Execute("update WORK_CHECKIN set checkin_time=getdate(),update_user=@User,update_time=getdate() where SEQ_ID=@ID", new { ID = xData.SEQ_ID, User = bUser.User.ID });
        //            else if (xData.checkin_time != null && iCheck == 0)
        //                bDB.oConn.Execute("update WORK_CHECKIN set checkin_time=NULL,update_user=@User,update_time=getdate() where SEQ_ID=@ID", new { ID = xData.SEQ_ID, User = bUser.User.ID });
        //        }
        //        catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex.Message + ":" + ex.StackTrace, oResult); }
        //    }
        //    else
        //        oResult.Error("查無資料");
        //}

        protected void ddlSupplier_SelectedIndexChanged(object sender, EventArgs e)
        {
            var xData = (CYCloud.WorkCheck.WorkCheckIn)ViewState["date"];

            var qList = CYCloud.WorkCheck.Shared.GetWorkCheckInLogByDate(xData.con_date, dDB, xData.fac_name);

            var gList = qList.Where(p => p.SEQ_ID == 0 || p.DeviceIn).Where(p => p.SupplierName == ddlSupplier.SelectedValue || ddlSupplier.SelectedValue.Length == 0);
            GridView1.DataSource = gList;
            GridView1.DataBind();
        }
        //public static IEnumerable<string> GetWorkCheckInSupplier(DateTime dDate, cyc.DB.SqlDBConn oDB)
        //{
        //    DateTime DateS = dDate, DateE = dDate.AddDays(1).AddMilliseconds(-1);
        //    return oDB.oConn.InfluxDB<string>(@"
        //    select distinct A.SHORT_NAME from AccessList2 A
        //    where A.EV_DATE between @DateS and @DateE 
        //    ", new { DateS, DateE});
        //}
        //public static IEnumerable<WorkCheckInLogDetail> GetWorkCheckInLogByDate(DateTime dDate, cyc.DB.SqlDBConn oDB)
        //{
        //    DateTime DateS = dDate, DateE = dDate.AddDays(1).AddMilliseconds(-1);
        //    return oDB.oConn.InfluxDB<WorkCheckInLogDetail>(@"
        //    select A.ID as IFP_RecognitionAuth_ID,A.FRUserID,A.FRUserName,A.LogDateTime
        //    ,B.SHORT_NAME as SupplierName
        //    from RecognitionAuth A left join AccessList2 B on A.FRUserID = B.ID
        //    where A.LogDateTime between @DateS and @DateE 
        //    ", new { DateS, DateE});
        //}
        //public static IEnumerable<WorkCheckInLogDetail> GetWorkCheckInLogByDate_(string ID, string WorkID, cyc.DB.SqlDBConn oDB)
        //{
        //    return oDB.oConn.InfluxDB<WorkCheckInLogDetail>(@"
        //    select A.ID as IFP_RecognitionAuth_ID,A.FRUserID,A.FRUserName,A.LogDateTime
        //    ,B.WORK_CHECKIN_ID,ISNULL(B.CHECK_TYPE,'')as CHECK_TYPE,B.SEQ_ID,F.[Name] as SupplierName
        //    ,case when A.DeviceName in @DeviceIn then 1 else 0 end as DeviceIn
        //    ,case when A.DeviceName in @DeviceOut then 1 else 0 end as DeviceOut
        //    from IFP_RecognitionAuth A
        //    inner join IFP_SupplierDriver E on (A.FRUserID=E.Code) and (E.StopDate is null or E.StopDate>=A.LogDateTime)
        //    left join IFP_Supplier F on E.SupplierID=F.ID
        //    inner join WORK_CHECKIN_LOG B on A.ID=B.IFP_RecognitionAuth_ID
        //    inner join WORK_CHECKIN C on C.SEQ_ID = B.WORK_CHECKIN_ID
        //    where A.ID =@ID_ and A.DeviceName in @DeviceInOut and C.con_number = @WorkID_
        //    ", new { WorkID_ = WorkID, ID_ = ID });
        //}

    }
}