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
    public partial class SysRole : BasePageGrid
    {
        protected override DataTable QuerySourceData(int idx)
        {
            return cyc.Data.Shared.ObjToDataTable<cyc.Data.SysRole>(cyc.Global.SysRole.List);
        }

        protected override GridPageOption SetPageSetting()
        {
            return new GridPageOption() { GridOption = new GridOption[] { new GridOption { AutoBind = true, Query = btnQuery, Grid = GridView1, Pager = ucPager } } };
        }

        protected void btnReInit_Click(object sender, EventArgs e)
        {
            //CYCloud.SysInit.InitSysRole();
            cyc.Global.SysRole.Init(dDB, true);
        }
    }
}