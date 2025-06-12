using cyc.Page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._edit
{
    public partial class MappDisableStopB : BasePageSub
    {
        protected override void OnLoad(EventArgs e)
        {
            if (!IsPostBack)
            {
                ddlNameQ.DataSource = dDB.QueryDataTable(string.Format("select MS_SEQ_ID,MS_SYS_NAME from MappSetting where {0}", cyc.UC.DeptControl.GetDeptLimitSQL(bUser, "MS_SYS_DEPT")));
                ddlNameQ.DataBind();
                ddlTypeQ.DataSource = CYCloud.Global.MappSettingType.List;
                ddlTypeQ.DataBind();
            }
            base.OnLoad(e);
        }

        protected override SubPageOption SetPageOption() => new SubPageOption()
        {
            Confirm = btnConfirm,
            SuccessMsg = "批次解隔離完成",
            CloseWhenSuccess = false,
            GridOption = new GridOption[] { new GridOption { AutoBind = true, Grid = GridView1, Pager = ucPager, Query = null } }
        };

        protected override System.Data.DataTable QuerySourceData(int idx)
        {
            return dDB.QueryDataTable(string.Format(@"
select A.MS_SEQ_ID,A.MD_SEQ_ID,A.MD_DATE_START,A.MD_DATE_END,A.MD_REASON,A.MD_STOP_TIME,C.MS_SYS_NAME
from MappDisable A inner join MappSetting C on A.MS_SEQ_ID=C.MS_SEQ_ID 
where A.MD_STOP_TIME is null and {0} {1} {2} order by A.MD_DATE_END"
, cyc.UC.DeptControl.GetDeptLimitSQL(bUser, "C.MS_SYS_DEPT")
, ddlTypeQ.SelectedValue.Length > 0 ? "and C.MT_SEQ_ID=@Type" : string.Empty
, ddlNameQ.SelectedValue.Length > 0 ? "and A.MS_SEQ_ID=@ID" : string.Empty)
                , new { ID = ddlNameQ.SelectedValue, Type = ddlTypeQ.SelectedValue });
        }

        protected void ddlTypeQ_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlNameQ.Items.Clear();
            ddlNameQ.DataSource = dDB.QueryDataTable(string.Format(@"
select MS_SEQ_ID,MS_SYS_NAME from MappSetting where {0} {1}",
cyc.UC.DeptControl.GetDeptLimitSQL(bUser, "MS_SYS_DEPT"),
ddlTypeQ.SelectedValue.Length > 0 ? "and MT_SEQ_ID=@Type" : string.Empty),
new { Type = ddlTypeQ.SelectedValue });
            ddlNameQ.DataBind();
            ddlNameQ.Items.Insert(0, new ListItem("全部", ""));
            BindGridView();
        }

        protected void ddlNameQ_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindGridView();
        }

        protected override void SaveData()
        {
            var sID = hidSelect.Value.Split(',').Where(p => cyc.Shared.Check.IsInteger(p));
            if (sID.Count() > 0)
            {
                dDB.Execute(string.Format(@"
update A set MD_STOP_TIME=getdate(),MD_STOP_USER=@User
from MappDisable A inner join MappSetting C on A.MS_SEQ_ID=C.MS_SEQ_ID 
where {0} and A.MD_STOP_TIME is null and MD_SEQ_ID in @IDs", cyc.UC.DeptControl.GetDeptLimitSQL(bUser, "C.MS_SYS_DEPT"))
                    , new { User = bUser.User.ID, IDs = sID.Select(p => Convert.ToInt32(p)) });
            }
            else
                oResult.Error("未勾選[解隔離]項目");

            if (oResult.Success) { BindGridView(); }
        }
    }
}