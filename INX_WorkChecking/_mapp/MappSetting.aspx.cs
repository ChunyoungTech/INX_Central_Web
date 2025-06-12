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
    public partial class MappSetting : BasePageGrid
    {
        protected override void OnInit(EventArgs e)
        {
            if (!IsPostBack)
            {
                ddlTypeQ.DataSource = CYCloud.Global.MappSettingType.List;
                ddlTypeQ.DataBind();
            }
            base.OnInit(e);
        }
        protected override DataTable QuerySourceData(int idx)
        {
            bPara.Command = string.Format(@"
select A.MS_SEQ_ID,A.MS_SYS_NAME,A.MS_SYS_DESC,A.MS_MAPP_TEAM_SN,case when A.MS_SYS_STOP='Y' then '是' else '' end as MS_SYS_STOP,B.Name as MS_SYS_DEPT_NAME,C.MT_TYPE_NAME
from MappSetting A left join SysDept B on A.MS_SYS_DEPT=B.ID left join MappSettingType C on A.MT_SEQ_ID=C.MT_SEQ_ID where {0} {1} {2}",
ucDeptQ.GetQuerySQL("A.MS_SYS_DEPT"),
ddlTypeQ.SelectedValue.Length > 0 ? "and A.MT_SEQ_ID=@Type" : string.Empty,
txtNameQ.Text.Trim().Length > 0 ? "and (A.MS_SYS_NAME like '%'+@Name+'%' or A.MS_SYS_DESC like '%'+@Name+'%')" : string.Empty);

//if (txtNameQ.Text.Trim().Length > 0) { bPara.Command += " and A.MS_SYS_NAME like '%'+@Name+'%'"; }
            bPara.Parameter.Clear();
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Type", ddlTypeQ.SelectedValue));
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Name", txtNameQ.Text.Trim()));
            return dDB.QueryDataTable(bPara);
        }

        protected override GridPageOption SetPageSetting()
        {
            return new GridPageOption() { GridOption = new GridOption[] { new GridOption { AutoBind = true, Grid = GridView1, Pager = ucPager, Query = btnQuery } } };
        }

        protected void btnReInit_Click(object sender, EventArgs e)
        {
            CYCloud.Global.MappSettingType.Init(dDB, true);
            CYCloud.Global.MappSetting.Init(dDB, true);
        }
    }
}