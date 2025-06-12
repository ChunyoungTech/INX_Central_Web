using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using cyc.Page;

namespace WebApp._sys
{
    public partial class SysProg : BasePageGrid
    {
        protected override void OnInit(EventArgs e)
        {
            if (!IsPostBack)
            {
                this.ddlDirQ.DataSource = cyc.Global.SysDir.List;
                this.ddlDirQ.DataBind();
                this.ddlDirQ.Items.Insert(0, "");
            }
            base.OnInit(e);
        }
        protected override DataTable QuerySourceData(int idx)
        {
            return cyc.Data.Shared.ObjToDataTable<cyc.Data.SysProg>(cyc.Global.SysProg.List.Where
                (p => (ddlDirQ.SelectedValue.Length > 0 ? p.DirID == Convert.ToInt32(ddlDirQ.SelectedValue) : true)
                && (ddlEnabledQ.SelectedValue.Length > 0 ? p.Enabled == Convert.ToBoolean(ddlEnabledQ.SelectedValue) : true)).OrderBy(p => p.DirID).ThenBy(p => p.Seq).ToList());
        }

        protected override GridPageOption SetPageSetting()
        {
            return new GridPageOption() { GridOption = new GridOption[] { new GridOption { AutoBind = true, Excel = btnExport, Grid = GridView1, Pager = ucPager, Query = btnQuery } } };
        }

        protected void btnReInit_Click(object sender, EventArgs e)
        {
            cyc.Global.SysDir.Init(dDB, true);
            cyc.Global.SysProg.Init(dDB, true);
            cyc.Global.SysProgSub.Init(dDB, true);
            BindGridView();
            ShowResult("重新載入完成", false, false);
        }
    }
}