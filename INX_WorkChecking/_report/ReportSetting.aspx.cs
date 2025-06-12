using cyc.Page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._report
{
    public partial class ReportSetting : BasePageGrid
    {
        protected override string DefaultConntionString() => cyc.DB.ConnString.Report;

        protected override System.Data.DataTable QuerySourceData(int idx)
        {
            return dDB.QueryDataTable("select * from ReportData");
        }

        protected override GridPageOption SetPageSetting()
        {
            return new GridPageOption() { GridOption = new GridOption[] { new GridOption { AutoBind = true, Grid = GridView1, Pager = ucPager, Query = btnQuery } } };
        }
    }
}