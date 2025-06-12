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
    public partial class SysDept : BasePageGrid
    {
        protected override DataTable QuerySourceData(int idx)
        {
            //bPara.Command = @"select A.*,B.Name as UpperName from SysDept A left join SysDept B on A.UpperID=B.ID where 1=1";
            //bPara.Command += ddlDeptQ.GetQuerySQL("A.ID", "and");
            //return bDB.QueryDT(bPara);
            //bPara.Command = @"select ID,Code,Name,NameAll,LevelNo from View_SysDeptLevel A where 1=1";
            //bPara.Command += ddlDeptQ.GetQuerySQL("A.ID", "and");
            return dDB.QueryDataTable("select ID,Code,Name,NameAll,LevelNo from View_SysDeptLevel A where 1=1" + ddlDeptQ.GetQuerySQL("A.ID", "and"));
        }

        protected override GridPageOption SetPageSetting()
        {
            return new GridPageOption() { GridOption = new GridOption[] { new GridOption { AutoBind = true, Grid = GridView1, Pager = ucPager, Query = btnQuery } } };
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            cyc.Global.SysDept.Init(dDB, true);
            BindGridView();
            ShowResult("重新載入完成", false, false);
        }

        //protected void lbRefresh_Click(object sender, EventArgs e)
        //{
        //    int iID = ddlDeptQ.DeptID;
        //    ddlDeptQ.Reset();
        //    ddlDeptQ.DeptID = iID;

        //    BindGridView();
        //}
    }
}