using Dapper;
using cyc.Page;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CYCloud.WorkCheck;

namespace WebApp._edit
{
    public partial class WorkCheckListTemp : cyc.Page.BasePage
    {
        #region #繼承
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!IsPostBack) { LoadData(); }
        }
        #endregion

        private void LoadData()
        {
            var oList = CYCloud.WorkCheck.Shared.GetWorkCheckAuthList(Request.QueryString["pa"], dDB);
            if (oList != null && oList.Any())
            {
                GridView1.DataSource = oList.Where(p => p.Type == "ACC" && p.InOut == 1).Select(p => new WorkCheckAuthData { Date = p.Date, Supplier = p.Supplier, Name = NameReplace(p.Name) }).OrderBy(p => p.Date);
                GridView1.DataBind();
                GridView4.DataSource = oList.Where(p => p.Type == "ACC" && p.InOut == 0).Select(p => new WorkCheckAuthData { Date = p.Date, Supplier = p.Supplier, Name = NameReplace(p.Name) }).OrderBy(p => p.Date);
                GridView4.DataBind();
                GridView2.DataSource = oList.Where(p => p.Type == "CHK" && p.InOut == 1).Select(p => new WorkCheckAuthData { Date = p.Date, Supplier = p.Supplier, Name = NameReplace(p.Name) }).OrderBy(p => p.Date);
                GridView2.DataBind();
                GridView3.DataSource = oList.Where(p => p.Type == "CHK" && p.InOut == 0).Select(p => new WorkCheckAuthData { Date = p.Date, Supplier = p.Supplier, Name = NameReplace(p.Name) }).OrderBy(p => p.Date);
                GridView3.DataBind();
            }
        }

        private string NameReplace(string name)
        {
            if (name.Length >= 2)
                return name.Replace(name.Substring(1, 1), "*");
            else
                return name;
        }
    }
}