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
    public partial class MappSendLog : BasePageGrid
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (!IsPostBack)
            {
                dteDateS.Text = DateTime.Today.ToString("yyyy/MM/dd");
                dteDateE.Text = DateTime.Today.ToString("yyyy/MM/dd");
                ddlNameQ.DataSource = dDB.QueryDataTable(string.Format("select MS_SYS_NAME as Name,MS_SYS_NAME+' ('+MS_SYS_DESC+')' as NameA from MappSetting where {0}", cyc.UC.DeptControl.GetDeptLimitSQL(bUser, "MS_SYS_DEPT")));
                ddlNameQ.DataBind();
                ddlTypeQ.DataSource = CYCloud.Global.MappSettingType.List;
                ddlTypeQ.DataBind();
            }
        }

        protected override void QueryCheck(int idx)
        {
            if (dteDateS.Text.Trim().Length == 0 || !cyc.Shared.Check.IsDateTime(dteDateS.Text.Trim()) || dteDateE.Text.Trim().Length == 0 || !cyc.Shared.Check.IsDateTime(dteDateE.Text.Trim()))
            { oResult.Error("[發送日期]格式錯誤"); }
        }

        protected override DataTable QuerySourceData(int idx)
        {
            DateTime dDateS = Convert.ToDateTime(dteDateS.Text);
            DateTime dDateE = (Convert.ToDateTime(dteDateE.Text)).AddDays(1).AddMilliseconds(-1);

            bPara.Command = string.Format(@"
select A.MM_SEQ_ID,A.MS_SYS_NAME,A.UPDATE_TIME,C.MS_SYS_DESC,B.ML_SEND_TIME,A.MM_SUBJECT
,case B.ML_IS_SUCCESS when 1 then '是' when 0 then '否' else '' end as ML_IS_SUCCESS,D.Name as MS_SYS_DEPT_NAME
from MappMessage A left join MappSendLog B on A.MM_SEQ_ID=B.MM_SEQ_ID
left join MappSetting C on A.MS_SYS_NAME=C.MS_SYS_NAME
left join SysDept D on C.MS_SYS_DEPT=D.ID
where A.UPDATE_TIME between @DateS and @DateE and {0} {1} {2} order by A.MM_SEQ_ID desc"
, cyc.UC.DeptControl.GetDeptLimitSQL(bUser, "C.MS_SYS_DEPT")
, ddlNameQ.SelectedValue.Trim().Length > 0 ? "and A.MS_SYS_NAME=@Sys" : string.Empty
, ddlTypeQ.SelectedValue.Length > 0 ? "and C.MT_SEQ_ID=@Type" : string.Empty);

            bPara.Parameter.Clear();
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Sys", ddlNameQ.SelectedValue.Trim()));
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Type", ddlTypeQ.SelectedValue));
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("DateS", dDateS));
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("DateE", dDateE));
            return dDB.QueryDataTable(bPara);
        }

        protected override GridPageOption SetPageSetting()
        {
            return new GridPageOption() { GridOption = new GridOption[] { new GridOption { AutoBind = true, Grid = GridView1, Pager = ucPager, Query = btnQuery } } };
        }

        protected void ddlTypeQ_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlNameQ.Items.Clear();
            ddlNameQ.DataSource = dDB.QueryDataTable(string.Format("select MS_SYS_NAME as Name,MS_SYS_NAME+' ('+MS_SYS_DESC+')' as NameA from MappSetting where {0} {1}"
                , cyc.UC.DeptControl.GetDeptLimitSQL(bUser, "MS_SYS_DEPT")
                , ddlTypeQ.SelectedValue.Length > 0 ? "and MT_SEQ_ID=@Type" : string.Empty)
                , new { Type = ddlTypeQ.SelectedValue });
            ddlNameQ.DataBind();
            ddlNameQ.Items.Insert(0, new ListItem("全部", ""));
        }
    }
}