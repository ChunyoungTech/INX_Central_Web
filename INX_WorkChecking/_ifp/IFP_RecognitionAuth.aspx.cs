using pin.Page;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Dapper;

namespace WebApp._ifp
{
    public partial class IFP_RecognitionAuth : BasePageGridMulti
    {
        protected override void OnInit(EventArgs e)
        {
            if (!IsPostBack)
            {
                dteDateE.Text = DateTime.Today.ToString("yyyy/MM/dd");
                dteDateS.Text = DateTime.Today.AddDays(-6).ToString("yyyy/MM/dd");
            }
            base.OnInit(e);
        }

        protected override DataTable QuerySourceData(int idx)
        {
            if (!pin.Comm.Check.IsDateTime(dteDateS.Text)) { DateTime.Today.AddDays(-13).ToString("yyyy/MM/dd"); }
            if (!pin.Comm.Check.IsDateTime(dteDateE.Text)) { DateTime.Today.ToString("yyyy/MM/dd"); }
            DateTime DateS = Convert.ToDateTime(dteDateS.Text);
            DateTime DateE = DateE = Convert.ToDateTime(dteDateE.Text).AddDays(1).AddMilliseconds(-1);

            bPara.Command = @"
select A.*,
        case when A.FRUserName='' 
        then isnull(B.Name,'') 
        else A.FRUserName end as UserName
from IFP_RecognitionAuth A left join IFP_SupplierDriver B on A.FRUserID=B.[Code]
where A.LogDateTime between @DateS and @DateE";
            if (txtDeviceName.Text.Trim().Length > 0) { bPara.Command += " and A.DeviceName like '%'+@Name+'%'"; }
            if (chkNotUserEmpty.Checked) { bPara.Command += " and A.FRUserID<>''"; }
            bPara.Parameter.Clear();
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("DateS", DateS));
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("DateE", DateE));
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Name", txtDeviceName.Text.Trim()));
            return bDB.QueryDataTable(bPara);
        }

        protected override GridPageSetting SetPageSetting()
        {
            return new GridPageSetting() { Option = new GridOption[] { new GridOption { AutoBind = true, Grid = GridView1, Pager = ucPager, Query = btnQuery, Refresh = lbRefresh } } };
        }
    }
}