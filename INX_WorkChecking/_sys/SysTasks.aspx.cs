using cyc.Page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._sys
{
    public partial class SysTasks : cyc.Page.BasePageGrid
    {
        protected override System.Data.DataTable QuerySourceData(int idx)
        {
            GridView1.DataSource = cyc.Auto.Manager.List();
            GridView1.DataBind();
            return null;
        }

        protected override GridPageOption SetPageSetting()
        {
            return new GridPageOption() { CheckOpen = "", GridOption = new GridOption[] { new GridOption { AutoBind = true, Grid = GridView1, Pager = null } } };
        }
    }
}