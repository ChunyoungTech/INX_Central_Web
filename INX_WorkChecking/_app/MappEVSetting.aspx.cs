using cyc.Page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._app
{
    public partial class MappEVSetting : BasePageGrid
    {
        protected override System.Data.DataTable QuerySourceData(int idx)
        {
            return dDB.QueryDataTable(string.Format(@"
select A.ID,A.Code,A.Name,A.IsTop,A.CimEnable,A.CimEnable,A.CimLevel,A.CimGroup,B.MS_SYS_DESC as NormalCode
,case when A.Type='E' then '地震' else '壓降' end as TypeName,case when FacArea='1' then '南廠' else '北廠' end as AreaName
from MappEV A 
left join MappSetting B on A.NormalID=B.MS_SEQ_ID 
where 1=1 {0} {1} order by A.IsTop desc,A.Code"
, ddlTypeQ.SelectedValue.Length == 0 ? "" : "and A.Type=@Type"
, ddlAreaQ.SelectedValue.Length == 0 ? "" : "and A.FacArea=@FacArea")
                , new { Type = ddlTypeQ.SelectedValue, FacArea = ddlAreaQ.SelectedValue });
        }

        protected override GridPageOption SetPageSetting()
        {
            return new GridPageOption() { GridOption = new GridOption[] { new GridOption { AutoBind = true, Grid = GridView1, Pager = ucPager, Query = btnQuery } } };
        }
    }
}