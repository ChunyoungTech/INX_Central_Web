using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using cyc.Page;
using Dapper;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;
using System.Data.SqlClient;

namespace WebApp._app
{
    public partial class WorkCheckinTemp : cyc.Page.BasePage
    {
        //SqlConnection gConn = new SqlConnection();
        //DataTable dt = new DataTable();
        //string Fac;
        protected override void OnInit(EventArgs e)
        {
            if (!IsPostBack)
            {
                dteDateS.Text = DateTime.Today.ToString("yyyy/MM/dd");
                dteDateE.Text = DateTime.Today.ToString("yyyy/MM/dd");
                Query();
            }
            base.OnInit(e);
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

        protected void btnQuery_Click(object sender, EventArgs e)
        {
            Query();
        }

        private void Query()
        {
            try
            {
                string Fac = Request.QueryString["Fac"];
                DateTime dDateS = Convert.ToDateTime(dteDateS.Text);
                DateTime dDateE = Convert.ToDateTime(dteDateE.Text);

                string sql = string.Format(@"
select distinct A.con_number,A.con_date,A.fac_name,A.vendor_name,B.SEQ_ID,B.checkin_time,B.checkout_time
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
			where A.con_date between @DateS and @DateE {0}
		)
	) X PIVOT (
	COUNT(SEQ_ID)
	FOR CHECK_TYPE IN ([CHECKIN], [CHECKOUT])
) p
) C on B.SEQ_ID=C.WORK_CHECKIN_ID
inner join AccessListMapping F on A.fac_name = F.VNTFAC where A.con_date between @DateS and @DateE  
{0} {1} {2} Order by A.con_date desc", txtNumber.Text.Trim().Length > 0 ? " and (A.con_number like '%'+@Number+'%' or A.vendor_name like '%'+@Number+'%')" : "",
    chkCheckIn.Checked ? "and B.checkin_time is not null" : "", !string.IsNullOrEmpty(Fac) ? " and F.FAC = @Fac": "");

                //SqlCommand cmd = new SqlCommand(sql, gConn);
                //cmd.Parameters.Add("@DateS", SqlDbType.DateTime).Value = dDateS;
                //cmd.Parameters.Add("@DateE", SqlDbType.DateTime).Value = dDateE;
                //cmd.Parameters.Add("@Number", SqlDbType.VarChar).Value = txtNumber.Text.Trim();
                //cmd.Parameters.Add("@Fac", SqlDbType.VarChar).Value = Fac;
                //dt = new DataTable();
                //SqlDataAdapter da = new SqlDataAdapter(cmd);
                //da.Fill(dt);
                //GridView1.DataSource = dt;
                //GridView1.DataBind();

                using (DataTable oDT = dDB.QueryDataTable(sql, new { DateS = dDateS, DateE = dDateE, Number = txtNumber.Text.Trim(), Fac }))
                {
                    if (oDT != null)
                    {
                        GridView1.DataSource = oDT;
                        GridView1.DataBind();
                    }
                }
            }
            catch
            {

            }
        }
    }
}