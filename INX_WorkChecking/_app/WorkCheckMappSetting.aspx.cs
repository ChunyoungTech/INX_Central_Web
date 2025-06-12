using cyc.Page;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using WebApp._uc;

namespace WebApp._app
{
    public partial class WorkCheckMappSetting : BasePageGrid
    {
        protected override DataTable QuerySourceData(int idx)
        {
            string sql = string.Format(@"
select A.ID,A.IsEnabled,B.NameAll as DeptName,C.MS_SYS_NAME+'('+C.MS_SYS_DESC+')' as MappSysName from WorkCheckMappSetting A
inner join View_SysDeptLevel B on A.DeptID=B.ID
left join MappSetting C on A.MappName=C.MS_SYS_NAME {0} order by A.MappName", ddlEnabled.SelectedValue.Length > 0 ? "where A.IsEnabled=@IsEnabled" : string.Empty);

            return dDB.QueryDataTable(sql, new { IsEnabled = ddlEnabled.SelectedValue == "1" });
        }

        protected override GridPageOption SetPageSetting()
        {
            return new GridPageOption() { GridOption = new GridOption[] { new GridOption { AutoBind = true, Grid = GridView1, Pager = ucPager, Query = btnQuery } } };
        }
    }
}