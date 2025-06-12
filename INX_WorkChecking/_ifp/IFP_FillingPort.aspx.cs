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
    public partial class IFP_FillingPort : BasePageGridMulti
    {
        protected override void OnInit(EventArgs e)
        {
            if (!IsPostBack)
            {
                var list = bDB.oConn.Query<CYCloud.IFP.BaseData>("select ID,Name from IFP_FillingArea where IsEnabled=1");
                ddlAreaQ.DataSource = list;
                ddlAreaQ.DataBind();
                ddlAreaQ.Items.Insert(0, "");
            }
            base.OnInit(e);
        }

        protected override DataTable QuerySourceData(int idx)
        {
            bPara.Command = @"
select A.*,B.Name as Material,C.Name as Supplier,D.Name as AreaName from IFP_FillingPort A 
left join IFP_Material B on A.MaterialID=B.ID
left join IFP_Supplier C on B.SupplierID=C.ID
left join IFP_FillingArea D on A.AreaID=D.ID
where 1=1";
            if (ddlAreaQ.SelectedValue.Length > 0) { bPara.Command += " and A.AreaID=@Area"; }
            if (txtNameQ.Text.Trim().Length > 0) { bPara.Command += " and A.Name like '%'+@Name+'%'"; }
            bPara.Parameter.Clear();
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Area", ddlAreaQ.SelectedValue));
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Name", txtNameQ.Text.Trim()));
            return bDB.QueryDT(bPara);
        }

        protected override GridPageSetting SetPageSetting()
        {
            return new GridPageSetting() { Option = new GridOption[] { new GridOption { AutoBind = true, Grid = GridView1, Pager = ucPager, Query = btnQuery, Refresh = lbRefresh } } };
        }

        protected void lblReType_Click(object sender, EventArgs e)
        {
            var x = ddlAreaQ.SelectedValue;
            var list = bDB.oConn.Query<CYCloud.IFP.BaseData>("select ID,Name from IFP_FillingArea where IsEnabled=1");
            ddlAreaQ.Items.Clear();
            ddlAreaQ.DataSource = list;
            ddlAreaQ.DataBind();
            ddlAreaQ.Items.Insert(0, "");
            if (ddlAreaQ.Items.FindByValue(x) != null) { ddlAreaQ.SelectedValue = x; }
        }
    }
}