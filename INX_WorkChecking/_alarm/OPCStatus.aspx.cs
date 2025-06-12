using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using cyc.Page;

namespace WebApp._alarm
{
    public partial class OPCStatus : BasePageGrid
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                ddlDeptQ.DeptID = bUser.Dept.ID;
        }

        protected override DataTable QuerySourceData(int idx)
        {
            System.Text.StringBuilder str = new System.Text.StringBuilder(@"
                    select *
                    from OPC_server A 
                    where 1=1");
            if (opcName.Text.Trim().Length > 0) { str.Append(" and A.server_group like '%'+@Name+'%'"); }
            bPara.Command = str.ToString();
            bPara.Parameter.Clear();
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Name", opcName.Text.Trim()));
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Dept", ddlDeptQ.DeptID));
            return dDB.QueryDataTable(bPara);
        }

        protected override GridPageOption SetPageSetting()
        {
            return new GridPageOption() { GridOption = new GridOption[] { new GridOption { AutoBind = false, Grid = GridView1, Pager = ucPager, Query = btnQuery } } };
        }

        protected void GridView1_DataBound(object sender, EventArgs e)
        {

        }

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {

        }
    }
}