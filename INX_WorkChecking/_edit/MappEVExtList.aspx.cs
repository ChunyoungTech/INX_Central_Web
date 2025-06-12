using cyc.Page;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._edit
{
    public partial class MappEVExtList : BasePageSub
    {
        protected int iID = 0;
        protected override void OnLoad(EventArgs e)
        {
            iID = Convert.ToInt32(Request.QueryString["pa"]);
            base.OnLoad(e);
        }

        protected override SubPageOption SetPageOption() => new SubPageOption()
        {
            Confirm = null,
            Parameter = "pa",
            GridOption = new GridOption[] { new GridOption { AutoBind = true, Grid = GridView1, Pager = null, Query = btnQuery } }
        };
        protected override System.Data.DataTable QuerySourceData(int idx)
        {
            return dDB.QueryDataTable(@"
select B.ID,A.Name,A.IsTop,B.CimEnable,B.CimGroup,C.MS_SYS_DESC as NormalCode
,case when A.Type='E' then '地震' else '壓降' end as TypeName,case when A.FacArea='1' then '南廠' else '北廠' end as AreaName
from MappEV A inner join MappEV B on A.ID=B.MainID 
left join MappSetting C on B.NormalID=C.MS_SEQ_ID 
where A.ID=@ID", new { ID = iID });
        }
    }
}