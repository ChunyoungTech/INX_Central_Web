using cyc.Page;
using System;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;

namespace WebApp._idb
{
    public partial class HistorianLive : BasePageGrid
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (!IsPostBack)
            {
                ddlFactory.DataSource = dDB.QueryList<string>("select FacName from IDBFacData order by SeqID");
                ddlFactory.DataBind();
                ddlFactory.Items.Insert(0, "");
            }
        }

        protected override void QueryCheck(int idx)
        {
        }

        protected override DataTable QuerySourceData(int idx)
        {
            if (string.IsNullOrEmpty(ddlFactory.SelectedValue)) return null;
            string table = ddlFactory.SelectedValue.Replace("FAB", "FAC") + "_Historian_Live";
            bPara.Command = $@"select [DateTime],[TagName],[Value],[vValue],[Quality],[QualityDetail],[OPCQuality],[wwTagKey],[wwRetrievalMode],[wwTimeDeadband],[wwValueDeadband],[wwTimeZone],[wwParameters],[SourceTag],[SourceServer],[wwValueSelector],[wwExpression],[wwUnit] from FAC_Loader.dbo.[{table}] where 1=1
{(string.IsNullOrWhiteSpace(txtTagName.Text) ? string.Empty : " and TagName like @Tag")}
{(string.IsNullOrWhiteSpace(txtQuality.Text) ? string.Empty : " and Quality=@Quality")}";
            var param = new { Tag = $"%{txtTagName.Text.Trim()}%", Quality = txtQuality.Text };
            return dDB.QueryDataTable(bPara.Command, param);
        }

        protected override GridPageOption SetPageSetting()
        {
            return new GridPageOption { GridOption = new[] { new GridOption { Grid = GridView1, Pager = ucPager, Query = btnQuery, AutoBind = true } } };
        }
    }
}
