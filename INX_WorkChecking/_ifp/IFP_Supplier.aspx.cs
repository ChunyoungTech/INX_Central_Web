using pin.Page;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._ifp
{
    public partial class IFP_Supplier : BasePageGridMulti
    {
        protected override DataTable QuerySourceData(int idx)
        {
            bPara.Command = @"select * from IFP_Supplier where 1=1";
            if (txtNameQ.Text.Trim().Length > 0) { bPara.Command += " and Name like '%'+@Name+'%'"; }
            if (txtDriverQ.Text.Trim().Length > 0) { bPara.Command += " and ID in (select SupplierID from IFP_SupplierDriver where Code=@Driver or Phone=@Driver)"; }
            bPara.Parameter.Clear();
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Name", txtNameQ.Text.Trim()));
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Driver", txtDriverQ.Text.Trim()));
            return bDB.QueryDataTable(bPara);
        }

        protected override GridPageSetting SetPageSetting()
        {
            return new GridPageSetting() { Option = new GridOption[] { new GridOption { AutoBind = true, Grid = GridView1, Pager = ucPager, Query = btnQuery, Refresh = lbRefresh } } };
        }
    }
}