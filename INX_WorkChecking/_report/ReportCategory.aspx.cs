using cyc.Page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._report
{
    public partial class ReportCategory : BasePageGrid
    {
        protected int ReportID = 0;
        protected override string DefaultConntionString() => cyc.DB.ConnString.Report;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (!IsPostBack)
            {
                ddlReport.DataSource = dDB.QueryList<cyc.Data.BaseObj>("select ID,Name from ReportData");
                ddlReport.DataBind();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!IsPostBack && oResult.Success) 
            {
                if (ddlReport.Items.Count > 0)
                {
                    ddlReport.SelectedIndex = 0;
                    ddlLevel01.DataSource = dDB.QueryList<string>("select distinct(Level01) from ReportCategory where ReportID=@ID", new { ID = Convert.ToInt32(ddlReport.SelectedValue) });
                    ddlLevel01.DataBind();
                    ddlLevel01.Items.Insert(0, "");

                    BindGridView(0);
                }
            }
        }

        protected override System.Data.DataTable QuerySourceData(int idx)
        {
            ReportID = Convert.ToInt32(ddlReport.SelectedValue);

            return dDB.QueryDataTable($"select * from ReportCategory where ReportID=@ID {(!string.IsNullOrEmpty(ddlLevel01.SelectedValue) ? "and Level01=@Level" : "")}", new { ID = ReportID, Level = ddlLevel01.SelectedValue });
        }

        protected override GridPageOption SetPageSetting()
        {
            return new GridPageOption() { GridOption = new GridOption[] { new GridOption { AutoBind = false, Grid = GridView1, Pager = ucPager, Query = btnQuery } } };
        }

        protected void ddlReport_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlLevel01.DataSource = dDB.QueryList<string>("select distinct(Level01) from ReportCategory where ReportID=@ID", new { ID = Convert.ToInt32(ddlReport.SelectedValue) });
            ddlLevel01.DataBind();
            ddlLevel01.Items.Insert(0, "");
            BindGridView(0);
        }

        protected void ddlLevel01_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindGridView(0);
        }

        //protected void Button1_Click(object sender, EventArgs e)
        //{
        //    if (decimal.TryParse(TextBox1.Text, out decimal iTest))
        //        TextBox2.Text = iTest.ToString();
        //    else
        //        TextBox2.Text = string.Empty;
        //}
    }
}