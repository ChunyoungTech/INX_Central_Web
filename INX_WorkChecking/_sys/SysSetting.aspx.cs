using cyc.Page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._sys
{
    public partial class SysSetting : BasePageGrid
    {
        protected override System.Data.DataTable QuerySourceData(int idx)
        {
            return dDB.QueryDataTable("select ID,Code,Name,Memo,case when Type='pwd' then '******' else Value end as Value from SysSetting");
        }
        protected override GridPageOption SetPageSetting()
        {
            return new GridPageOption() { GridOption = new GridOption[] { new GridOption { AutoBind = true, Grid = GridView1, Pager = ucPager, Query = btnQuery } } };
        }
        protected void btnReInit_Click(object sender, EventArgs e)
        {
            cyc.Global.SysSetting.Init(dDB, true);
            BindGridView();
            ShowResult("重新載入完成", false, false);
        }
    }
}