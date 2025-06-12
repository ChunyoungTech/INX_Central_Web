using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using cyc.Page;

namespace WebApp._sys
{
    public partial class SysUser : BasePageGrid
    {
        protected override DataTable QuerySourceData(int idx)
        {
            bPara.Command = @"select A.ID,A.Code,A.Name,A.Enabled,B.Name as DeptName,C.Name as DeptLevel from SysUser A 
left join View_SysDeptLevel B on A.DeptID=B.ID
left join View_SysDeptLevel C on ISNULL(A.DeptLevel,A.DeptID)=C.ID where A.ID<>1";
            bPara.Command += ddlDeptQ.GetQuerySQL("A.DeptID", "and");
            if (txtCodeQ.Text.Trim().Length > 0) { bPara.Command += " and A.Code like '%'+@Code+'%'"; }
            if (txtNameQ.Text.Trim().Length > 0) { bPara.Command += " and A.Name like '%'+@Name+'%'"; }
            if (ddlEnabled.SelectedValue.Length > 0) { bPara.Command += " and A.Enabled=@Enabled"; }
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Code", txtCodeQ.Text.Trim()));
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Name", txtNameQ.Text.Trim()));
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Enabled", ddlEnabled.SelectedValue == "1"));
            return dDB.QueryDataTable(bPara);
        }

        protected override GridPageOption SetPageSetting()
        {
            return new GridPageOption() { GridOption = new GridOption[] { new GridOption { AutoBind = true, Grid = GridView1, Pager = ucPager, Query = btnQuery } } };
        }
    }
}