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
    public partial class WorkCheckMappFacReport : BasePageGrid
    {
        protected override DataTable QuerySourceData(int idx)
        {
            string sql = string.Format(@"
select A.ID,A.FacCode,C.MS_SYS_NAME+'('+C.MS_SYS_DESC+')' as MappSysName,A.IsEnabled from WorkCheckMappFacReport A
left join MappSetting C on A.MappName=C.MS_SYS_NAME {0} order by A.MappName", ddlEnabled.SelectedValue.Length > 0 ? "where A.IsEnabled=@IsEnabled" : string.Empty);

            return dDB.QueryDataTable(sql, new { IsEnabled = ddlEnabled.SelectedValue == "1" });
        }

        protected override GridPageOption SetPageSetting()
        {
            return new GridPageOption() { GridOption = new GridOption[] { new GridOption { AutoBind = true, Grid = GridView1, Pager = ucPager, Query = btnQuery } } };
        }
    }
}