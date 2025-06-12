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
    public partial class WorkCkeckList : cyc.Page.BasePageSub
    {
        string ConNumber = string.Empty;
        protected override void OnLoad(EventArgs e)
        {
            ConNumber = Request.QueryString["pa"];
            base.OnLoad(e);
        }

        #region #繼承
        protected override SubPageOption SetPageOption() => new SubPageOption()
        {
            CheckOpen = "open.aspx;WorkCkeckList.aspx",
            Confirm = null,
            Parameter = "pa",
            IsIntPa = false
        };

        protected override void LoadData()
        {
            var oList = CYCloud.WorkCheck.Shared.GetWorkCheckAuthList(ConNumber, dDB);
            if (oList != null && oList.Any())
            {
                GridView1.DataSource = oList.Where(p => p.Type == "ACC" && p.InOut == 1).OrderBy(p => p.Date);
                GridView1.DataBind();
                GridView4.DataSource = oList.Where(p => p.Type == "ACC" && p.InOut == 0).OrderBy(p => p.Date);
                GridView4.DataBind();
                GridView2.DataSource = oList.Where(p => p.Type == "CHK" && p.InOut == 1).OrderBy(p => p.Date);
                GridView2.DataBind();
                GridView3.DataSource = oList.Where(p => p.Type == "CHK" && p.InOut == 0).OrderBy(p => p.Date);
                GridView3.DataBind();
            }
        }
        #endregion
    }
}