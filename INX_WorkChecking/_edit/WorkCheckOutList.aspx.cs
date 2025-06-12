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
    public partial class WorkCheckOutList : cyc.Page.BasePageSub
    {
        string ConNumber = string.Empty;
        protected override void OnLoad(EventArgs e)
        {
            ConNumber = Request.QueryString["pa"];
            base.OnLoad(e);
        }

        protected override SubPageOption SetPageOption() => new SubPageOption()
        {
            CheckOpen = "open.aspx;WorkCheckOutList.aspx",
            Confirm = btnConfirm,
            Parameter = "pa",
            IsIntPa = false
        };
        //protected override void LoadData()
        //{
        //    //var xData = CYCloud.WorkCheck.Shared.GetWorkCheckInData(Convert.ToInt32(Request.QueryString["pa"]), bDB);
        //    var xData = CYCloud.WorkCheck.Shared.GetWorkCheckInData(ConNumber, dDB);
        //    if (xData != null && xData.checkin_time != null)
        //    {
        //        var qList = CYCloud.WorkCheck.Shared.GetWorkCheckInLogForLogout(ConNumber, xData.con_date, xData.fac_name, dDB);

        //        System.Text.StringBuilder oStr = new System.Text.StringBuilder("");

        //        var oList = qList.Where(p => (p.WORK_CHECKIN_ID == xData.SEQ_ID && p.CHECK_TYPE == "CHECKOUT"));
        //        foreach (var oData in oList)
        //            oStr.AppendFormat("<li data-id='{0}'><input type='button' class='btnRemove' value='移除' data-code='{3}' />{1}-{2}</li>", oData.IFP_RecognitionAuth_ID, oData.FRUserName, oData.SupplierName, oData.FRUserID);
        //        ltlCheckOut.Text = oStr.ToString();

        //        //oStr.Clear();
        //        //var iList = qList.Where(p => (p.WORK_CHECKIN_ID == xData.SEQ_ID && p.CHECK_TYPE == "CHECKIN"));
        //        //foreach (var oData in iList)
        //        //    oStr.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>", oData.FRUserName, oData.SupplierName, ((DateTime)oData.LogDateTime).ToString("yyyy/MM/dd HH:mm"));
        //        ////oStr.AppendLine(string.Format("<li data-id='{0}'>{1}-{2}</li>", oData.IFP_RecognitionAuth_ID, oData.FRUserName, oData.SupplierName));
        //        //ltlCheckInTable.Text = oStr.ToString();

        //        //var gList = from ils in qList.Where(p => (p.WORK_CHECKIN_ID == xData.SEQ_ID))
        //        //            join qls in qList on ils.FRUserID equals qls.FRUserID
        //        //            where qls.DeviceOut
        //        //            select qls;
        //        var gList = CYCloud.WorkCheck.Shared.GetWorkCheckOutLogByDate(ConNumber, dDB, xData.fac_name);
        //        //var gList = qList.Where(p => (p.WORK_CHECKIN_ID == xData.SEQ_ID && p.CHECK_TYPE == "CHECKOUT"));
        //        GridView1.DataSource = gList;
        //        GridView1.DataBind();

        //    }
        //    else
        //        oResult.Error("查無資料");
        //}
        ////protected override void LoadData()
        ////{
        ////    //var xData = CYCloud.WorkCheck.Shared.GetWorkCheckInData(Convert.ToInt32(Request.QueryString["pa"]), bDB);
        ////    var xData = CYCloud.WorkCheck.Shared.GetWorkCheckInData(ConNumber, bDB);
        ////    if (xData != null && xData.checkin_time != null)
        ////    {
        ////        var qList = CYCloud.WorkCheck.Shared.GetWorkCheckInLogByDate__(ConNumber, bDB, xData.fac_name);

        ////        System.Text.StringBuilder oStr = new System.Text.StringBuilder("");

        ////        var oList = qList.Where(p => (p.WORK_CHECKIN_ID == xData.SEQ_ID && p.CHECK_TYPE == "CHECKOUT"));
        ////        foreach (var oData in oList)
        ////            oStr.AppendFormat("<li data-id='{0}'><input type='button' class='btnRemove' value='移除' data-code='{3}' />{1}-{2}</li>", oData.IFP_RecognitionAuth_ID, oData.FRUserName, oData.SupplierName, oData.FRUserID);
        ////        ltlCheckOut.Text = oStr.ToString();

        ////        //oStr.Clear();
        ////        //var iList = qList.Where(p => (p.WORK_CHECKIN_ID == xData.SEQ_ID && p.CHECK_TYPE == "CHECKIN"));
        ////        //foreach (var oData in iList)
        ////        //    oStr.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>", oData.FRUserName, oData.SupplierName, ((DateTime)oData.LogDateTime).ToString("yyyy/MM/dd HH:mm"));
        ////        ////oStr.AppendLine(string.Format("<li data-id='{0}'>{1}-{2}</li>", oData.IFP_RecognitionAuth_ID, oData.FRUserName, oData.SupplierName));
        ////        //ltlCheckInTable.Text = oStr.ToString();

        ////        //var gList = from ils in qList.Where(p => (p.WORK_CHECKIN_ID == xData.SEQ_ID))
        ////        //            join qls in qList on ils.FRUserID equals qls.FRUserID
        ////        //            where qls.DeviceOut
        ////        //            select qls;
        ////        var gList = CYCloud.WorkCheck.Shared.GetWorkCheckOutLogByDate(ConNumber, bDB, xData.fac_name);
        ////        //var gList = qList.Where(p => (p.WORK_CHECKIN_ID == xData.SEQ_ID && p.CHECK_TYPE == "CHECKOUT"));
        ////        GridView1.DataSource = gList;
        ////        GridView1.DataBind();

        ////    }
        ////    else
        ////        oResult.Error("查無資料");
        ////}

        protected override void LoadData()
        {
            var xData = CYCloud.WorkCheck.Shared.GetWorkCheckInData(ConNumber, dDB);
            if (xData != null && xData.checkin_time != null)
            {
                var gList = CYCloud.WorkCheck.Shared.GetWorkCheckOutLogByDate(ConNumber, dDB, xData.fac_name, xData.con_date);
                if (gList != null)
                {
                    System.Text.StringBuilder oStr = new System.Text.StringBuilder("");
                    foreach (var oData in gList.Where(p => p.CHECK_TYPE == "CHECKOUT"))
                        oStr.AppendFormat("<li data-id='{0}'><input type='button' class='btnRemove' value='移除' data-code='{3}' />{1}-{2}</li>", oData.IFP_RecognitionAuth_ID, oData.FRUserName, oData.SupplierName, oData.FRUserID);
                    ltlCheckOut.Text = oStr.ToString();

                    GridView1.DataSource = gList;
                    GridView1.DataBind();
                }
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

            //var xData = CYCloud.WorkCheck.Shared.GetWorkCheckInData(Convert.ToInt32(Request.QueryString["pa"]), bDB);
            var xData = CYCloud.WorkCheck.Shared.GetWorkCheckInData(ConNumber, dDB);

            if (xData != null)
            {
                var rList = CYCloud.WorkCheck.Shared.GetWorkCheckInLogByDate(xData.con_date, dDB, xData.fac_name);
                var eList = CYCloud.WorkCheck.Shared.GetWorkCheckInLogForLogout(ConNumber, xData.con_date, xData.fac_name, dDB);

                //var qList = CYCloud.WorkCheck.Shared.GetWorkCheckInLogByDate(xData.con_date, bDB);

                ////檢查是否已存在其他表單
                //var eList = from qls in qList
                //            join nls in nList on qls.IFP_RecognitionAuth_ID.ToString() equals nls
                //            where !(qls.WORK_CHECKIN_ID == 0 || (qls.WORK_CHECKIN_ID == xData.SEQ_ID && qls.CHECK_TYPE == "CHECKOUT"))
                //            select qls;
                //if (eList.Count() > 0) { oResult.Error(string.Join("、", eList.Select(p => p.FRUserName)) + "，已存在其它表單"); return; }

                //取得目前選取人員名單，並檢查是否存在不合法資料
                var nList = from nls in wList
                            join rls in rList on nls equals rls.IFP_RecognitionAuth_ID.ToString()
                            select rls;
                if (nList.Count() != wList.Count) { oResult.Error("選取資料不存在系統"); return; }

                //var sList = from nls in wList
                //            join qls in rList on nls equals qls.IFP_RecognitionAuth_ID.ToString() into xx
                //            from qls in xx.DefaultIfEmpty()
                //            select qls.FRUserID ?? "";
                ////檢查是否選取不存在系統編號
                //if (sList.Any(p => p.Length == 0)) { oResult.Error("選取資料不存在系統"); return; }
                ////檢查是否重複選取相同人員
                //if (sList.GroupBy(p => p).Any(q => q.Count() > 1)) { oResult.Error("重複選取相同人員"); return; }

                //原本單人員
                var oList = eList.Where(p => p.WORK_CHECKIN_ID == xData.SEQ_ID && p.CHECK_TYPE == "CHECKOUT").ToList();
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
                             select new CYCloud.WorkCheck.WorkCheckInLog { WORK_CHECKIN_ID = xData.SEQ_ID, CHECK_TYPE = "CHECKOUT", IFP_RecognitionAuth_ID = nls.IFP_RecognitionAuth_ID, FRUserName = nls.FRUserName, update_user = bUser.User.ID }).ToList();

                if (iList.Count + dList.Count > 0)
                {
                    using (var oDB = new cyc.DB.SqlDapperConn(oResult, null, true))
                    {
                        try
                        {
                            if (oResult.Success && dList.Count() > 0)
                                oDB.Execute("delete from WORK_CHECKIN_LOG where SEQ_ID=@SEQ_ID and WORK_CHECKIN_ID=@WORK_CHECKIN_ID", dList);
                            if (oResult.Success && iList.Count() > 0)
                                oDB.Execute("insert into WORK_CHECKIN_LOG (WORK_CHECKIN_ID,CHECK_TYPE,IFP_RecognitionAuth_ID,update_time,update_user) values (@WORK_CHECKIN_ID,@CHECK_TYPE,@IFP_RecognitionAuth_ID,getdate(),@update_user)", iList);

                            if (oResult.Success)
                            {
                                int iCheck = oList.Count() + iList.Count() - dList.Count();
                                if (xData.checkout_time == null && iCheck > 0)
                                    oDB.Execute("update WORK_CHECKIN set checkout_time=getdate(),update_user=@User,update_time=getdate() where SEQ_ID=@ID", new { ID = xData.SEQ_ID, User = bUser.User.ID });
                                else if (xData.checkout_time != null && iCheck == 0)
                                    oDB.Execute("update WORK_CHECKIN set checkout_time=NULL,update_user=@User,update_time=getdate() where SEQ_ID=@ID", new { ID = xData.SEQ_ID, User = bUser.User.ID });
                                if (oResult.Success)
                                {
                                    var oLog = new CYCloud.ExecLog.LogItem { ExecID = Convert.ToInt32(Request.QueryString["app"]), ExecType = "update", UserID = bUser.User.ID };
                                    oLog.ExecDesc = string.Format("工單({0})手動報退：新增-{1}，移除-{2}", xData.con_number, (iList.Count > 0 ? string.Join("、", iList.Select(p => p.FRUserName)) : "無"), (dList.Count > 0 ? string.Join("、", dList.Select(p => p.FRUserName)) : "無"));
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

        //        //var qList = CYCloud.WorkCheck.Shared.GetWorkCheckInLogByDate(xData.con_date, bDB);

        //        ////檢查是否已存在其他表單
        //        //var eList = from qls in qList
        //        //            join nls in nList on qls.IFP_RecognitionAuth_ID.ToString() equals nls
        //        //            where !(qls.WORK_CHECKIN_ID == 0 || (qls.WORK_CHECKIN_ID == xData.SEQ_ID && qls.CHECK_TYPE == "CHECKOUT"))
        //        //            select qls;
        //        //if (eList.Count() > 0) { oResult.Error(string.Join("、", eList.Select(p => p.FRUserName)) + "，已存在其它表單"); return; }

        //        var sList = from nls in nList
        //                    join qls in qList on nls equals qls.IFP_RecognitionAuth_ID.ToString() into xx
        //                    from qls in xx.DefaultIfEmpty()
        //                    select qls.FRUserID ?? "";
        //        //檢查是否選取不存在系統編號
        //        if (sList.Any(p => p.Length == 0)) { oResult.Error("選取資料不存在系統"); return; }
        //        //檢查是否重複選取相同人員
        //        if (sList.GroupBy(p => p).Any(q => q.Count() > 1)) { oResult.Error("重複選取相同人員"); return; }

        //        //原本單人員
        //        var oList = hList.Where(p => p.WORK_CHECKIN_ID == xData.SEQ_ID && p.CHECK_TYPE == "CHECKOUT");
        //        //欲移除人員
        //        var dList = from ols in oList
        //                    join nls in nList on ols.IFP_RecognitionAuth_ID.ToString() equals nls into xx
        //                    from nls in xx.DefaultIfEmpty()
        //                    where nls == null
        //                    select ols;
        //        //欲新增人員
        //        var iList = from nls in nList
        //                    join qls in qList on nls equals qls.IFP_RecognitionAuth_ID.ToString()
        //                    join ols in oList on nls equals ols.IFP_RecognitionAuth_ID.ToString() into xx
        //                    from ols in xx.DefaultIfEmpty()
        //                    where ols == null
        //                    select new CYCloud.WorkCheck.WorkCheckInLog { WORK_CHECKIN_ID = xData.SEQ_ID, CHECK_TYPE = "CHECKOUT", IFP_RecognitionAuth_ID = Convert.ToInt64(nls), update_user = bUser.User.ID };
        //        try
        //        {
        //            if (dList.Count() > 0)
        //                bDB.oConn.Execute("delete from WORK_CHECKIN_LOG where SEQ_ID=@SEQ_ID and WORK_CHECKIN_ID=@WORK_CHECKIN_ID", dList);
        //            if (iList.Count() > 0)
        //                bDB.oConn.Execute("insert into WORK_CHECKIN_LOG (WORK_CHECKIN_ID,CHECK_TYPE,IFP_RecognitionAuth_ID,update_time,update_user) values (@WORK_CHECKIN_ID,@CHECK_TYPE,@IFP_RecognitionAuth_ID,getdate(),@update_user)", iList);

        //            int iCheck = oList.Count() + iList.Count() - dList.Count();

        //            if (xData.checkout_time == null && iCheck > 0)
        //                bDB.oConn.Execute("update WORK_CHECKIN set checkout_time=getdate(),update_user=@User,update_time=getdate() where SEQ_ID=@ID", new { ID = xData.SEQ_ID, User = bUser.User.ID });
        //            else if (xData.checkout_time != null && iCheck == 0)
        //                bDB.oConn.Execute("update WORK_CHECKIN set checkout_time=NULL,update_user=@User,update_time=getdate() where SEQ_ID=@ID", new { ID = xData.SEQ_ID, User = bUser.User.ID });
        //        }
        //        catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex.Message + ":" + ex.StackTrace, oResult); }
        //    }
        //    else
        //        oResult.Error("查無資料");
        //}
    }
}