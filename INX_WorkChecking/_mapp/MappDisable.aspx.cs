using cyc.Page;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._mapp
{
    public partial class MappDisable : BasePageGrid
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (!IsPostBack)
            {
                ddlSetting.DataSource = dDB.QueryDataTable(string.Format("select MS_SEQ_ID as ID,MS_SYS_NAME+' ('+MS_SYS_DESC+')' as Name from MappSetting where {0}", cyc.UC.DeptControl.GetDeptLimitSQL(bUser, "MS_SYS_DEPT")));
                ddlSetting.DataBind();
                ddlTypeQ.DataSource = CYCloud.Global.MappSettingType.List;
                ddlTypeQ.DataBind();
                dteDateS.Value = DateTime.Today;
                dteDateE.Value = DateTime.Today;
            }
        }

        protected override void QueryCheck(int idx)
        {
            if (dteDateS.Value == null || dteDateE.Value == null)
                oResult.Error("[隔離期間]不可空白且須為日期格式");
        }

        protected override DataTable QuerySourceData(int idx)
        {
            DateTime DateS = ((DateTime)dteDateS.Value).Date;
            DateTime DateE = ((DateTime)dteDateE.Value).Date.AddDays(1).AddMinutes(-1);
            return dDB.QueryDataTable(string.Format(@"
select A.MS_SEQ_ID,A.MD_SEQ_ID,A.MD_DATE_START,A.MD_DATE_END,A.MD_REASON,B.Name as MD_STOP_USER_NAME,A.MD_STOP_TIME,C.MS_SYS_NAME,ISNULL(A.MD_STOP_USER,0)as MD_STOP_USER
from MappDisable A left join SysUser B on A.MD_STOP_USER=B.ID
inner join MappSetting C on A.MS_SEQ_ID=C.MS_SEQ_ID where {0} and not (A.MD_DATE_START>@DateE or A.MD_DATE_END<@DateS) {1} {2} {3} order by A.MD_DATE_START desc"
, cyc.UC.DeptControl.GetDeptLimitSQL(bUser, "C.MS_SYS_DEPT")
, ddlSetting.SelectedValue != "0" ? "and A.MS_SEQ_ID=@ID" : ""
, ddlStop.SelectedValue.Length > 0 ? (ddlStop.SelectedValue == "1" ? "and A.MD_STOP_TIME is null" : "and A.MD_STOP_TIME is not null") : string.Empty
, ddlTypeQ.SelectedValue.Length > 0 ? "and C.MT_SEQ_ID=@Type" : string.Empty)
                , new { ID = Convert.ToInt32(ddlSetting.SelectedValue), Type = ddlTypeQ.SelectedValue, DateS, DateE });
        }

        protected override GridPageOption SetPageSetting()
        {
            return new GridPageOption() { GridOption = new GridOption[] { new GridOption { AutoBind = true, Grid = GridView1, Pager = ucPager, Query = btnQuery } } };
        }

        protected void ddlTypeQ_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlSetting.Items.Clear();
            ddlSetting.DataSource = dDB.QueryDataTable(string.Format("select MS_SEQ_ID as ID,MS_SYS_NAME+'('+MS_SYS_DESC+')' as Name from MappSetting where {0} {1}"
                , cyc.UC.DeptControl.GetDeptLimitSQL(bUser, "MS_SYS_DEPT")
                , ddlTypeQ.SelectedValue.Length > 0 ? "and MT_SEQ_ID=@Type" : string.Empty)
                , new { Type = ddlTypeQ.SelectedValue });
            ddlSetting.DataBind();
            ddlSetting.Items.Insert(0, new ListItem("全部", "0"));
        }
    }
}