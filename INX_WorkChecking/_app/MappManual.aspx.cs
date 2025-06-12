using cyc.Page;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._app
{
    public partial class MappManual : cyc.Page.BasePageGrid
    {
        protected override void OnInit(EventArgs e)
        {
            if (!IsPostBack)
            {
                dteDateS.Text = DateTime.Today.ToString("yyyy/MM/dd");
                dteDateE.Text = DateTime.Today.ToString("yyyy/MM/dd");
            }
            base.OnInit(e);
        }

        protected override void QueryCheck(int idx)
        {
            if (dteDateS.Text.Trim().Length == 0 || !cyc.Shared.Check.IsDateTime(dteDateS.Text.Trim()) || dteDateE.Text.Trim().Length == 0 || !cyc.Shared.Check.IsDateTime(dteDateE.Text.Trim()))
            { oResult.Error("[發送日期]格式錯誤"); }
        }

        protected override DataTable QuerySourceData(int idx)
        {
            DateTime dDateS = DateTime.Today;
            DateTime dDateE = DateTime.Today;
            DateTime.TryParse(dteDateS.Text, out dDateS);
            DateTime.TryParse(dteDateE.Text, out dDateE);

            bPara.Command = @"
select SEQ_ID,MApp_Date,MApp_Time,MApp_Value1,MApp_Value2,MApp_Value3,MApp_Provider,MApp_Plant
,case MApp_Ack_Flag when 'N' then '否' else '是' end as Ack_Flag
from MApp_Table where MApp_Date between @DateS and @DateE order by MApp_Date desc,MApp_Time desc";

            bPara.Parameter.Clear();
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("DateS", dDateS.ToString("yyyy/MM/dd")));
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("DateE", dDateE.ToString("yyyy/MM/dd")));
            using (cyc.DB.SqlDapperConn oDB = new cyc.DB.SqlDapperConn(null, cyc.DB.ConnString.MApp))
            {
                return oDB.QueryDataTable(bPara);
            }
        }

        protected override GridPageOption SetPageSetting()
        {
            return new GridPageOption() { GridOption = new GridOption[] { new GridOption { AutoBind = true, Grid = GridView1, Pager = ucPager, Query = btnQuery } } };
        }
    }
}