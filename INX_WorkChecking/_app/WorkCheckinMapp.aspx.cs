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
    public partial class WorkCheckinMapp : BasePageGrid
    {
        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            if (bUser != null && bUser.From == 1) this.MasterPageFile = "~/_master/Vendor.Master";
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (!IsPostBack)
            {
                dteDateS.Text = DateTime.Today.ToString("yyyy/MM/dd");
                dteDateE.Text = DateTime.Today.ToString("yyyy/MM/dd");
                if (bUser != null)
                {
                    ddlFAC.DataSource = cyc.UC.DeptControl.GetFacCode(bUser.User.DeptLevel).OrderBy(p => p);
                    ddlFAC.DataBind();
                    //bPara.Command = string.Format("select FAC from AccessLimitFac {0} order by FAC", (!bUser.User.isManager && bUser.Dept.Code.Length > 0) ? "where DEPT=@DEPT" : "");
                    //bPara.Parameter.Clear();
                    //bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("DEPT", bUser.Dept.Code));
                    //using (DataTable oDT = dDB.QueryDataTable(bPara))
                    //{
                    //    if (oResult.Success)
                    //    {
                    //        ddlFAC.DataSource = oDT;
                    //        ddlFAC.DataBind();
                    //    }
                    //}
                    ////if (!bUser.User.isManager && bUser.Dept.Code.Length > 0)
                    ////{
                    ////    ddlFAC.SelectedValue = bUser.Dept.Code;
                    ////    ddlFAC.Enabled = false;
                    ////}
                }
            }
        }

        protected override void QueryCheck(int idx)
        {
            if (dteDateS.Value == null || dteDateE.Value == null)
                oResult.Error("[施工日期]格式錯誤");
            else if (((DateTime)dteDateS.Value).AddDays(60) < (DateTime)dteDateE.Value)
                oResult.Error("[施工日期]區間，請勿超過60天");
        }

        protected override DataTable QuerySourceData(int idx)
        {
            DateTime dDateS = Convert.ToDateTime(dteDateS.Text);
            DateTime dDateE = Convert.ToDateTime(dteDateE.Text);

            bPara.Command = string.Format(@"
select A.con_date,B.EV_DATE,B.SHORT_NAME,B.APPLY_PK,B.ID,B.P_NAME,B.MAPP_ERROR_Type,C.Name as ERROR_CODE
from View_VMT_FAC A
inner join WORK_CHECKIN_MAPP B on A.con_number=B.APPLY_PK
left join WORK_CHECKIN_MAPP_TYPE C on B.MAPP_ERROR_Type=C.Code
where A.fac_code=@FacCode and A.con_date between @DateS and @DateE {0} {1}"
, string.IsNullOrWhiteSpace(txtNumber.Text) ? string.Empty : "and (A.con_number like '%'+@Number+'%' or B.SHORT_NAME like '%'+@Number+'%')"
, string.IsNullOrWhiteSpace(txtName.Text) ? string.Empty : "and (B.P_NAME like '%'+@Name+'%' or B.ID like '%'+@Name+'%')"
);
            bPara.Parameter.Clear();
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("DateS", dDateS));
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("DateE", dDateE));
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("FacCode", ddlFAC.SelectedValue));
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Number", txtNumber.Text.Trim()));
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Name", txtName.Text.Trim()));
            return dDB.QueryDataTable(bPara);
        }

        protected override GridPageOption SetPageSetting()
        {
            return new GridPageOption() { GridOption = new GridOption[] { new GridOption { AutoBind = true, Grid = GridView1, Pager = ucPager, Query = btnQuery, Excel = btnExport } } };
        }

        protected override ExportOption GetExportOption(int idx)
        {
            return new ExportOption()
            {
                Mapping = new string[] { "違規時間", "廠商名稱", "工單號碼", "違規人員", "姓名", "違規事由" },
                Column = new string[] { "EV_DATE", "SHORT_NAME", "APPLY_PK", "ID", "P_NAME", "ERROR_CODE" },
                ColType = new string[] { "dt", "s", "s", "s", "s", "s" },
                FileName = "廠商違規紀錄"
            };
        }
    }
}