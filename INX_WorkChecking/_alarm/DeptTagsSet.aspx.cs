using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using cyc.Page;

namespace WebApp._alarm
{
    public partial class DeptTagsSet : BasePageGrid
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                ddlDeptQ.DeptID = bUser.Dept.ID;
        }

        protected override DataTable QuerySourceData(int idx)
        {
            //bPara.Command = @"select * from DeptTagAlarmStatus A 
            //                  inner join TagData B on A.tag_data_id=B.ID
            //                  left join MappSetting M on M.MS_SEQ_ID=A.MAppGroupId";
            ////bPara.Command += ddlDeptQ.GetQuerySQL("A.dept_id", "where");
            //bPara.Command += " where A.dept_id=@Dept";
            //if (ddlTypeQ.SelectedValue.Length > 0) { bPara.Command += " and B.Tag_Type=@Type"; }
            //if (txtNameQ.Text.Trim().Length > 0) { bPara.Command += " and B.Tag_Name like '%'+@Name+'%'"; }
            //bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Type", ddlTypeQ.SelectedValue));
            //bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Name", txtNameQ.Text.Trim()));
            //bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Dept", ddlDeptQ.DeptID));
            //return dDB.QueryDataTable(bPara);
            return dDB.QueryDataTable(string.Format(@"select * from DeptTagAlarmStatus A 
                              inner join TagData B on A.tag_data_id=B.ID
                              left join MappSetting M on M.MS_SEQ_ID=A.MAppGroupId where A.dept_id=@Dept {0} {1}",
                              ddlTypeQ.SelectedValue.Length > 0 ? "and B.Tag_Type=@Type" : string.Empty,
                              txtNameQ.Text.Trim().Length > 0 ? "and B.Tag_Name like '%'+@Name+'%'" : string.Empty),
                              new { Type = ddlTypeQ.SelectedValue, Name = txtNameQ.Text.Trim(), Dept = ddlDeptQ.DeptID });
        }

        protected override GridPageOption SetPageSetting()
        {
            return new GridPageOption() { GridOption = new GridOption[] { new GridOption { AutoBind = true, Grid = GridView1, Pager = ucPager, Query = btnQuery, Excel = btnExport } } };
        }

        protected override ExportOption GetExportOption(int idx)
        {
            return new ExportOption()
            {
                Mapping = new string[] { "資料點名稱" },
                Column = new string[] { "Tag_Name" },
                ColType = new string[] { "s" },
                FileName = "部門與資料點對應"
            };
        }

        [WebMethod(EnableSession = true)]
        public static bool DeleteItem(string id, int App)
        {
            //try
            //{
            //    bPara.Command = @"delete DeptTagAlarmStatus where id=@id";
            //    bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("id", id));

            //    bDB.QueryDT(bPara);
            //}
            //catch (Exception ex)
            //{
            //    cyc.Log.WriteSysErrorLog($"刪除部門警報參數點失敗： {ex.StackTrace}");
            //    return false;
            //}

            try
            {
                using (cyc.DB.SqlDapperConn oDB = new cyc.DB.SqlDapperConn())
                {
                    cyc.DB.SqlDBPara bPara = new cyc.DB.SqlDBPara
                    {
                        Command = @"delete DeptTagAlarmStatus where id=@id"
                    };
                    bPara.Parameter.Clear();
                    bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("id", id));
                    oDB.QueryDataTable(bPara);
                    var oUser = (cyc.Data.UserInfo)HttpContext.Current.Session["uid"];
                    CYCloud.ExecLog.WriteLog(new CYCloud.ExecLog.LogItem() { ExecID = App, ExecType = "update", ExecDesc = string.Format("刪除部門警報參數點", id), UserID = oUser.User.ID }, oDB);
                }

            }
            catch (Exception e)
            {
                cyc.Log.WriteSysErrorLog($"刪除部門警報參數點失敗： {e.StackTrace}");
                return false;
            }
            return true;
        }

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            //	1全部禁能	2HIHI禁能	3HI禁能	4LO禁能	5LOLO禁能	6HIHI設定值	7HI設定值	8LO設定值	9LOLO設定值
            DataRowView drv = (DataRowView)e.Row.DataItem;

            if ((e.Row.RowType == DataControlRowType.DataRow) || (e.Row.RowType == DataControlRowType.Footer))
            {
                if (drv != null)
                {
                    //change text
                    e.Row.Cells[6].Text = drv["ALL_Enable"].ToString()  == "True" ? "是" : "否";
                    e.Row.Cells[7].Text = drv["HIHI_Enable"].ToString() == "True" ? "是" : "否";
                    e.Row.Cells[8].Text = drv["HI_Enable"].ToString()   == "True" ? "是" : "否";
                    e.Row.Cells[9].Text = drv["LO_Enable"].ToString()   == "True" ? "是" : "否";
                    e.Row.Cells[10].Text = drv["LOLO_Enable"].ToString() == "True" ? "是" : "否";
                }
            }
        }
    }
}