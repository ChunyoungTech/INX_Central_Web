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
    public partial class IFP_MaterialOrder : BasePageGridMulti
    {
        protected override void OnInit(EventArgs e)
        {
            if (!IsPostBack)
            {
                //var list = bDB.oConn.Query<CYCloud.IFP.Material>("select ID,Name from IFP_Material");
                //ddlTypeQ.DataSource = list;
                //ddlTypeQ.DataBind();
                //ddlTypeQ.Items.Insert(0, "");
                dteDateE.Text = DateTime.Today.ToString("yyyy/MM/dd");
                dteDateS.Text = DateTime.Today.AddDays(-13).ToString("yyyy/MM/dd");
            }
            base.OnInit(e);
        }

        protected override DataTable QuerySourceData(int idx)
        {
            if (!pin.Comm.Check.IsDateTime(dteDateS.Text)) { DateTime.Today.AddDays(-13).ToString("yyyy/MM/dd"); }
            if (!pin.Comm.Check.IsDateTime(dteDateE.Text)) { DateTime.Today.ToString("yyyy/MM/dd"); }
            DateTime DateS = Convert.ToDateTime(dteDateS.Text);
            DateTime DateE = DateE = Convert.ToDateTime(dteDateE.Text).AddDays(1).AddMilliseconds(-1);

            bPara.Command = string.Format(@"
select A.*,B.Name as Material,C.Name as Supplier,D.Name as UserName
,Convert(varchar(20),EstimateDate,111)as EDate,FillingDate
,case when CancelDate is null then '' else '是' end as IsCancel
from IFP_MaterialOrder A left join IFP_Material B on A.MaterialID=B.ID
left join IFP_Supplier C on B.SupplierID=C.ID
left join SysUser D on A.OrderUser=D.ID where A.EstimateDate between @DateS and @DateE
and A.MaterialID in (select ID from IFP_Material where {0}) {1}",
CYCloud.DeptControl.GetDeptLimitSQL(bUser, "TypeID"),
(txtNameQ.Text.Trim().Length > 0 ? " and B.Name like '%'+@Name+'%'" : "")
);
            //bPara.Command += " and A.MaterialID in (select ID from IFP_Material where " + CYCloud.DeptControl.GetDeptLimitSQL(bUser, "TypeID") + ")";

            //if (txtNameQ.Text.Trim().Length > 0) { bPara.Command += " and B.Name like '%'+@Name+'%'"; }

            bPara.Parameter.Clear();
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("DateS", DateS));
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("DateE", DateE));
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Name", txtNameQ.Text.Trim()));
            return bDB.QueryDataTable(bPara);
        }

        protected override GridPageSetting SetPageSetting()
        {
            return new GridPageSetting() { Option = new GridOption[] { new GridOption { AutoBind = true, Grid = GridView1, Pager = ucPager, Query = btnQuery, Refresh = lbRefresh } } };
        }
    }
}