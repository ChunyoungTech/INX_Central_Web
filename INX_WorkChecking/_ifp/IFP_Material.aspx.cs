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
    public partial class IFP_Material : BasePageGridMulti
    {
        protected override void OnInit(EventArgs e)
        {
            if (!IsPostBack)
            {
                var list = bDB.QueryList<string>("select Name from IFP_MaterialType where IsEnabled=1");
                ddlTypeQ.DataSource = list;
                ddlTypeQ.DataBind();
                ddlTypeQ.Items.Insert(0, "");
            }
            base.OnInit(e);
        }

        protected override DataTable QuerySourceData(int idx)
        {
            bPara.Command = @"
select A.*,B.Name as Supplier,D.Name as TypeName,A.PortNo,A.FaceDevice from IFP_Material A
left join SysDept D on A.TypeID=D.ID
left join IFP_Supplier B on A.SupplierID=B.ID" + ddlDeptQ.GetQuerySQL("A.TypeID", "where");
            if (txtNameQ.Text.Trim().Length > 0) { bPara.Command += " and A.Name like '%'+@Name+'%'"; }
            bPara.Parameter.Clear();
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Type", ddlDeptQ.DeptID));
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Name", txtNameQ.Text.Trim()));
            return bDB.QueryDataTable(bPara);
        }

        protected override GridPageSetting SetPageSetting()
        {
            return new GridPageSetting() { Option = new GridOption[] { new GridOption { AutoBind = true, Grid = GridView1, Pager = ucPager, Query = btnQuery, Refresh = lbRefresh } } };
        }
    }
}