using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using cyc.Page;
using Dapper;

namespace WebApp._app
{
    public partial class WorkCheckin : BasePageGrid
    {
        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            if (bUser != null && bUser.From == 1) this.MasterPageFile = "~/_master/Vendor.Master";
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (!IsPostBack)
            {
                dteDateS.Text = DateTime.Today.ToString("yyyy/MM/dd");
                dteDateE.Text = DateTime.Today.ToString("yyyy/MM/dd");
                if (bUser != null)
                {
                    ddlFAC.DataSource = cyc.UC.DeptControl.GetFacCode(bUser.User.DeptLevel).OrderBy(p => p);
                    ddlFAC.DataBind();
                    if (bUser.From == 1)
                    {
                        chkAuto.Visible = false;
                        btnExport.Visible = false;
                    }
                }
            }
        }

        protected override void QueryCheck(int idx)
        {
            if (dteDateS.Text.Trim().Length == 0 || !cyc.Shared.Check.IsDateTime(dteDateS.Text.Trim()) || dteDateE.Text.Trim().Length == 0 || !cyc.Shared.Check.IsDateTime(dteDateE.Text.Trim()))
            { oResult.Error("[施工日期]格式錯誤"); }
        }

        protected override DataTable QuerySourceData(int idx)
        {
            DateTime dDateS = Convert.ToDateTime(dteDateS.Text);
            DateTime dDateE = Convert.ToDateTime(dteDateE.Text);

            //            bPara.Command = string.Format(@"
            //select distinct A.con_number,A.con_date,A.fac_name,A.vendor_name,B.SEQ_ID,B.checkin_time,B.checkout_time
            //,isnull(C.CHECKIN,0)as checkin_count,isnull(C.CHECKOUT,0)as checkout_count
            //,case when B.checkin_time is null then 0 else 1 end as CheckOut
            //,(Select Count(distinct ID) From AccessList2 Where AccessList2.APPLY_PK = D.APPLY_PK and Direction = 1 ) as 哨口進廠人數
            //,(Select Top 1 EV_DATE From AccessList2 Where AccessList2.APPLY_PK = D.APPLY_PK and Direction = 1  order by EV_DATE) as 哨口進廠時間
            //,(Select Count(distinct ID) From AccessList2 Where AccessList2.APPLY_PK = D.APPLY_PK and Direction = 0 ) as 哨口出廠人數
            //,(Select Top 1 EV_DATE From AccessList2 Where AccessList2.APPLY_PK = D.APPLY_PK and Direction = 0 order by EV_DATE desc) as 哨口出廠時間
            //from View_VMT_FAC A left join WORK_CHECKIN B on A.con_number=B.con_number 
            //left join AccessList2 D on A.con_number=D.APPLY_PK 
            //inner join SysUser E on replace(SUBSTRING(A.engineer,0,5),'(','') = E.Name
            //left join (
            //	select * from (
            //		select SEQ_ID,WORK_CHECKIN_ID,CHECK_TYPE from WORK_CHECKIN_LOG where WORK_CHECKIN_ID in (
            //			select B.SEQ_ID from View_VMT_FAC A left join WORK_CHECKIN B on A.con_number=B.con_number
            //			where A.fac_code=@FacCode and A.con_date between @DateS and @DateE {0}
            //		)
            //	) X PIVOT (
            //	    COUNT(SEQ_ID)
            //	    FOR CHECK_TYPE IN ([CHECKIN], [CHECKOUT])
            //    ) p
            //) C on B.SEQ_ID=C.WORK_CHECKIN_ID where A.fac_code=@FacCode and A.con_date between @DateS and @DateE {0} {1} order by A.con_date,A.fac_name",
            //(txtNumber.Text.Trim().Length > 0 ? "and (A.con_number like '%'+@Number+'%' or A.vendor_name like '%'+@Number+'%')" : ""),
            //(chkCheckIn.Checked ? "and B.checkin_time is not null " : ""));

            //            bPara.Command = string.Format(@"
            //with CTE as (
            //	select A.con_number,B.ID,B.EV_DATE,B.Direction
            //	from View_VMT_FAC A 
            //	inner join AccessList2 B on A.con_number=B.APPLY_PK
            //	inner join SysUser E on replace(SUBSTRING(A.engineer,0,5),'(','') = E.Name
            //	where A.fac_code=@FacCode and A.con_date between @DateS and @DateE
            //)

            //select A.con_number,A.con_date,A.fac_name,A.vendor_name,B.SEQ_ID,B.checkin_time,B.checkout_time
            //,isnull(C.CHECKIN,0)as checkin_count,isnull(C.CHECKOUT,0)as checkout_count
            //,case when B.checkin_time is null then 0 else 1 end as CheckOut
            //,(Select Count(distinct ID) From CTE Where con_number = A.con_number and Direction = 1 ) as 哨口進廠人數
            //,(Select MIN(EV_DATE) From CTE Where con_number = A.con_number and Direction = 1) as 哨口進廠時間
            //,(Select Count(distinct ID) From CTE Where con_number = A.con_number and Direction = 0 ) as 哨口出廠人數
            //,(Select MAX(EV_DATE) From CTE Where con_number = A.con_number and Direction = 0) as 哨口出廠時間
            //from View_VMT_FAC A left join WORK_CHECKIN B on A.con_number=B.con_number 
            //inner join SysUser E on replace(SUBSTRING(A.engineer,0,5),'(','') = E.Name
            //left join (
            //	select * from (
            //		select SEQ_ID,WORK_CHECKIN_ID,CHECK_TYPE from WORK_CHECKIN_LOG where WORK_CHECKIN_ID in (
            //			select B.SEQ_ID from View_VMT_FAC A left join WORK_CHECKIN B on A.con_number=B.con_number
            //			where A.fac_code=@FacCode and A.con_date between @DateS and @DateE {0}
            //		)
            //	) X PIVOT (
            //	    COUNT(SEQ_ID)
            //	    FOR CHECK_TYPE IN ([CHECKIN], [CHECKOUT])
            //    ) p
            //) C on B.SEQ_ID=C.WORK_CHECKIN_ID where A.fac_code=@FacCode and A.con_date between @DateS and @DateE {0} {1} order by A.con_date,A.fac_name",
            //(txtNumber.Text.Trim().Length > 0 ? "and (A.con_number like '%'+@Number+'%' or A.vendor_name like '%'+@Number+'%')" : ""),
            //(chkCheckIn.Checked ? "and B.checkin_time is not null " : ""));

            bPara.Command = string.Format(@"
with VMT as (
	select A.*
	from WORK_CHECKIN A inner join SysUser E on A.engineer=E.Name
	where A.con_date between @DateS and @DateE and A.fac_code=@FacCode {1} {0}
), ACC as (
	select A.con_number,B.Direction,COUNT(distinct B.ID)as AccCnt,MIN(B.EV_DATE)as AccMin,MAX(B.EV_DATE)as AccMax
	from VMT A inner join AccessList2 B on A.con_number=B.APPLY_PK
	group by A.con_number,B.Direction
), CHK as (
	select A.SEQ_ID,B.CHECK_TYPE,A.checkin_time,A.checkout_time,COUNT(1)as ChkCnt
	from VMT A inner join WORK_CHECKIN_LOG B on A.SEQ_ID=B.WORK_CHECKIN_ID
	group by A.SEQ_ID,B.CHECK_TYPE,A.checkin_time,A.checkout_time
)
select A.con_date,A.con_number,A.fac_name,A.vendor_name,ISNULL(B1.AccCnt,0) as [哨口進廠人數],B1.AccMin as [哨口進廠時間],ISNULL(B0.AccCnt,0)as [哨口出廠人數],B0.AccMax as [哨口出廠時間]
,C0.checkin_time,C1.checkout_time,ISNULL(C0.ChkCnt,0) as checkin_count,ISNULL(C1.ChkCnt,0) as checkout_count,case when C0.checkin_time is null then 0 else 1 end as CheckOut
from VMT A 
inner join ACC B1 on A.con_number=B1.con_number and B1.Direction=1
left join ACC B0 on A.con_number=B0.con_number and B0.Direction=0
left join CHK C0 on A.SEQ_ID=C0.SEQ_ID and C0.CHECK_TYPE='CHECKIN'
left join CHK C1 on A.SEQ_ID=C1.SEQ_ID and C1.CHECK_TYPE='CHECKOUT'
order by A.con_date,A.con_number",
(txtNumber.Text.Trim().Length > 0 ? "and (A.con_number like '%'+@Number+'%' or A.vendor_name like '%'+@Number+'%')" : ""),
(chkCheckIn.Checked ? "and A.checkin_time is not null " : ""));

            bPara.Parameter.Clear();
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("DateS", dDateS));
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("DateE", dDateE));
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("FacCode", ddlFAC.SelectedValue));
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Number", txtNumber.Text.Trim()));
            return dDB.QueryDataTable(bPara);
        }

        protected override GridPageOption SetPageSetting()
        {
            return new GridPageOption() { GridOption = new GridOption[] { new GridOption { AutoBind = true, Grid = GridView1, Pager = ucPager, Query = btnQuery } } };
        }

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //DataRowView drv = (DataRowView)e.Row.DataItem;
                ////廠務報到時間、廠部報退時間 都不是空白 and 廠務報到人數 不等於 廠務報退人數
                //if (drv["checkin_count"].ToString() != drv["checkout_count"].ToString() && drv["checkin_time"] != System.DBNull.Value && drv["checkout_time"] != System.DBNull.Value)
                //    e.Row.BackColor = System.Drawing.Color.LightPink;
                ////廠務報到時間不是空白 and 廠務報到人數 不等於 哨口報到人數
                //if (drv["checkin_count"].ToString() != drv["哨口進廠人數"].ToString() && drv["checkin_time"] != System.DBNull.Value)
                //    e.Row.BackColor = System.Drawing.Color.LightPink;
                ////廠務報到時間、廠務報退時間 都不是空白 and 廠務報退人數 不等於 哨口報退人數
                //if (drv["checkout_count"].ToString() != drv["哨口出廠人數"].ToString() && drv["checkin_time"] != System.DBNull.Value && drv["checkout_time"] != System.DBNull.Value)
                //    e.Row.BackColor = System.Drawing.Color.LightPink;
                DataRowView row = (DataRowView)e.Row.DataItem;
                if (row["checkin_count"].ToString() != row["哨口進廠人數"].ToString() || row["checkout_count"].ToString() != row["哨口出廠人數"].ToString())
                    e.Row.BackColor = System.Drawing.Color.LightPink;
            }
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            string sSQL = @"
select A.update_time,A.CHECK_TYPE,B.FRUserID,B.FRUserName,D.Name as SupplierName
from WORK_CHECKIN_LOG A inner join IFP_RecognitionAuth B on A.IFP_RecognitionAuth_ID=B.ID
left join IFP_SupplierDriver C on B.FRUserID=C.Code and (C.StopDate is null or C.StopDate>=B.LogDateTime)
left join IFP_Supplier D on C.SupplierID=D.ID
where A.WORK_CHECKIN_ID=@ID";
            string sWorkCheck = "施工日期,工單號碼,施工廠別,無塵室名稱,主要區域,次要區域,廠商名稱,作業類別1,作業類別2,作業類別3,作業類別4,作業類別5,施作內容,群創工程師,施工負責人,簽到時間,簽退時間";
            string sWorkChecklog = "施工廠商,施工人員ID,施工人員姓名,登記時間";

            DateTime DateS = Convert.ToDateTime(dteDateS.Text);
            DateTime DateE = Convert.ToDateTime(dteDateE.Text);


            var mList = dDB.QueryList<CYCloud.WorkCheck.VMT_FAC4>(string.Format(@"
select A.*,B.checkin_time,B.checkout_time,isnull(B.SEQ_ID,0)as SEQ_ID from View_VMT_FAC A left join WORK_CHECKIN B on A.con_number=B.con_number
where A.con_date between @DateS and @DateE {0}", txtNumber.Text.Trim().Length > 0 ? "and A.con_number=@Number" : ""),
new { DateS, DateE, Number = txtNumber.Text.Trim() });

            Response.Clear();
            Response.AddHeader("Content-Disposition", "attachment;filename=" + System.Web.HttpUtility.UrlEncode("每日施工管理") + ".csv");
            Response.ContentType = "application/octet-stream";

            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(Response.OutputStream, System.Text.Encoding.UTF8))
            {
                foreach (var mData in mList)
                {
                    sw.WriteLine("工單資料");
                    sw.WriteLine(sWorkCheck);
                    sw.WriteLine(string.Join(",", new List<string>
                    {
                        mData.con_date.ToString("yyyy/MM/dd"),
                        mData.con_number.ToString(),
                        mData.fac_name ?? "",
                        mData.fab_name ?? "",
                        (mData.main_area ?? "").Replace(",", "，"),
                        (mData.second_area ?? "").Replace(",", "，"),
                        (mData.vendor_name ?? "").Replace(",", "，"),
                        (mData.type1 ?? "").Replace(",", "，"),
                        (mData.type2 ?? "").Replace(",", "，"),
                        (mData.type3 ?? "").Replace(",", "，"),
                        (mData.type4 ?? "").Replace(",", "，"),
                        (mData.type5 ?? "").Replace(",", "，"),
                        (mData.con_conten ?? "").Replace(",", "，"),
                        mData.engineer ?? "",
                        mData.vendor_pe ?? "",
                        mData.checkin_time == null ? "" : ((DateTime)mData.checkin_time).ToString("yyyy/MM/dd HH:mm"),
                        mData.checkout_time == null ? "" : ((DateTime)mData.checkout_time).ToString("yyyy/MM/dd HH:mm")
                    }));

                    if (mData.SEQ_ID > 0)
                    {
                        var dList = dDB.QueryList<CYCloud.WorkCheck.WorkCheckInLogDetail>(sSQL, new { ID = mData.SEQ_ID });

                        var inList = dList.Where(p => p.CHECK_TYPE == "CHECKIN").OrderBy(p => p.update_time);
                        if (inList.Count() > 0)
                        {
                            sw.WriteLine("簽到人員");
                            sw.WriteLine(sWorkChecklog);
                            foreach (var x in inList)
                            {
                                sw.WriteLine(string.Join(",", new List<string>
                                {
                                    x.SupplierName,
                                    x.FRUserID.ToString(),
                                    x.FRUserName,
                                    (Convert.ToDateTime(x.update_time)).ToString("yyyy/MM/dd HH:mm")
                                }));
                            }
                        }
                        var ouList = dList.Where(p => p.CHECK_TYPE == "CHECKOUT").OrderBy(p => p.update_time);
                        if (ouList.Count() > 0)
                        {
                            sw.WriteLine("簽退人員");
                            sw.WriteLine(sWorkChecklog);
                            foreach (var x in ouList)
                            {
                                sw.WriteLine(string.Join(",", new List<string>
                                {
                                    x.SupplierName,
                                    x.FRUserID.ToString(),
                                    x.FRUserName,
                                    (Convert.ToDateTime(x.update_time)).ToString("yyyy/MM/dd HH:mm")
                                }));
                            }
                        }
                    }
                    sw.WriteLine("");
                }

                sw.Flush();

                Response.Flush();
                Response.End();
            }
        }

        protected void chkAuto_CheckedChanged(object sender, EventArgs e)
        {
            Timer1.Enabled = chkAuto.Checked;
        }

        protected void Timer1_Tick(object sender, EventArgs e)
        {
            BindGridView();
        }



    }
}