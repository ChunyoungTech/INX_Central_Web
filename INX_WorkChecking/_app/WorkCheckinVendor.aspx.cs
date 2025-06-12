using cyc.Page;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._app
{
    public partial class WorkCheckinVendor : cyc.Page.BasePageGrid
    {
        string FacCode = "";
        protected override void OnInit(EventArgs e)
        {
            if (!string.IsNullOrEmpty(Request.QueryString["fac"]))
                FacCode = Request.QueryString["fac"];
            if (!IsPostBack)
            {
                dteDateS.Text = DateTime.Today.ToString("yyyy/MM/dd");
                dteDateE.Text = DateTime.Today.ToString("yyyy/MM/dd");
            }
            base.OnInit(e);
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

            bPara.Command = string.Format(@"select distinct A.con_number,A.con_date,A.fac_name,A.vendor_name,B.SEQ_ID,B.checkin_time,B.checkout_time
,isnull(C.CHECKIN,0)as checkin_count,isnull(C.CHECKOUT,0)as checkout_count
,case when B.checkin_time is null then 0 else 1 end as CheckOut
,(Select Count(distinct ID) From AccessList2 Where AccessList2.APPLY_PK = D.APPLY_PK and Direction = 1 ) as 哨口進廠人數
,(Select Top 1 EV_DATE From AccessList2 Where AccessList2.APPLY_PK = D.APPLY_PK and Direction = 1  order by EV_DATE) as 哨口進廠時間
,(Select Count(distinct ID) From AccessList2 Where AccessList2.APPLY_PK = D.APPLY_PK and Direction = 0 ) as 哨口出廠人數
,(Select Top 1 EV_DATE From AccessList2 Where AccessList2.APPLY_PK = D.APPLY_PK and Direction = 0 order by EV_DATE desc) as 哨口出廠時間
from View_VMT_FAC A left join WORK_CHECKIN B on A.con_number=B.con_number left join AccessList2 as D on A.con_number=D.APPLY_PK inner join SysUser E on replace(SUBSTRING(A.engineer,0,5),'(','') = E.Name
left join (
	select * from (
		select SEQ_ID,WORK_CHECKIN_ID,CHECK_TYPE from WORK_CHECKIN_LOG where WORK_CHECKIN_ID in (
			select B.SEQ_ID from View_VMT_FAC A left join WORK_CHECKIN B on A.con_number=B.con_number
			where {0} {1}
		)
	) X PIVOT (
	COUNT(SEQ_ID)
	FOR CHECK_TYPE IN ([CHECKIN], [CHECKOUT])
) p
) C on B.SEQ_ID=C.WORK_CHECKIN_ID
inner join AccessListMapping F on A.fac_name = F.VNTFAC 
where {0} {1} {2} Order by A.con_date desc",
"A.fac_code=@FacCode and A.con_date between @DateS and @DateE",
txtNumber.Text.Trim().Length > 0 ? "and (A.con_number like '%'+@Number+'%' or A.vendor_name like '%'+@Number+'%')" : "",
chkCheckIn.Checked ? "and B.checkin_time is not null" : "");
            bPara.Parameter.Clear();
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("DateS", dDateS));
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("DateE", dDateE));
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("FacCode", FacCode.ToUpper()));
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Number", txtNumber.Text.Trim()));
            return dDB.QueryDataTable(bPara);
        }

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DataRowView drv = (DataRowView)e.Row.DataItem;
                if (drv["checkin_count"].ToString() != drv["checkout_count"].ToString() && drv["checkin_time"] != System.DBNull.Value && drv["checkout_time"] != System.DBNull.Value)
                    e.Row.BackColor = System.Drawing.Color.LightPink;
                if (drv["checkin_count"].ToString() != drv["哨口進廠人數"].ToString() && drv["checkin_time"] != System.DBNull.Value)
                    e.Row.BackColor = System.Drawing.Color.LightPink;
                if (drv["checkout_count"].ToString() != drv["哨口出廠人數"].ToString() && drv["checkin_time"] != System.DBNull.Value && drv["checkout_time"] != System.DBNull.Value)
                    e.Row.BackColor = System.Drawing.Color.LightPink;
            }
        }

        protected override GridPageOption SetPageSetting()
        {
            return new GridPageOption() { CheckSession = false, CheckOpen = "", GridOption = new GridOption[] { new GridOption { AutoBind = true, Grid = GridView1, Pager = ucPager, Query = btnQuery } } };
        }
    }
}