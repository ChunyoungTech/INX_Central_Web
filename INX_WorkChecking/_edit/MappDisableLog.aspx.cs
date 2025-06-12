using cyc.Page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._edit
{
    public partial class MappDisableLog : BasePageSub
    {
        int iID = 0;
        protected override void OnLoad(EventArgs e)
        {
            iID = Convert.ToInt32(Request.QueryString["pa"]);
            base.OnLoad(e);
        }

        protected override SubPageOption SetPageOption() => new SubPageOption()
        {
            Confirm = null,
            Parameter = "pa",
            GridOption = new GridOption[] { new GridOption { AutoBind = true, Grid = GridView1, Pager = ucPager, Query = null } }
        };
        protected override System.Data.DataTable QuerySourceData(int idx)
        {
            return dDB.QueryDataTable(@"
select C.MS_SYS_NAME,A.MD_DATE_START,A.MD_DATE_END,A.MD_REASON,B.Name as UPDATE_USER,A.UPDATE_TIME
,A.MD_REMIND_MIN,D.MS_SYS_NAME as MD_REMIND_SETTING,E.MS_SYS_NAME as MD_TRANS_NAME
from (
    select MS_SEQ_ID,MD_DATE_START,MD_DATE_END,MD_REASON,UPDATE_USER,UPDATE_TIME,MD_REMIND_MIN,MD_REMIND_SETTING,MD_TRANS_ID from MappDisableLog where MD_SEQ_ID=@ID
    union
    select MS_SEQ_ID,MD_DATE_START,MD_DATE_END,MD_REASON,UPDATE_USER,UPDATE_TIME,MD_REMIND_MIN,MD_REMIND_SETTING,MD_TRANS_ID from MappDisable where MD_SEQ_ID=@ID
) A inner join MappSetting C on A.MS_SEQ_ID=C.MS_SEQ_ID
left join MappSetting D on A.MD_REMIND_SETTING=D.MS_SEQ_ID
left join MappSetting E on A.MD_TRANS_ID=E.MS_SEQ_ID
left join SysUser B on A.UPDATE_USER=B.ID
order by A.UPDATE_TIME", new { ID = iID });
        }
    }
}