using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using cyc.Page;

namespace WebApp._alarm
{
    public partial class TagAlarmStatus : BasePageGrid
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                ddlDeptQ.DeptID = bUser.Dept.ID;
        }

        protected override DataTable QuerySourceData(int idx)
        {
            System.Text.StringBuilder str = new System.Text.StringBuilder(@"
                    select A.*,B.Tag_Name,B.HiHi_Limit,B.Hi_Limit,B.Lo_Limit,B.LoLo_Limit,B.u_user,B.u_date,
                    A.HIHI_Enable ,A.HI_Enable ,A.LO_Enable ,A.LOLO_Enable ,A.ALL_Enable,
                    B.HIHI_Enable as thihi ,B.HI_Enable as thi, B.LO_Enable as tlo ,B.LOLO_Enable as tlolo ,B.ALL_Enable as tall ,
                    B.quality
                    from DeptTagAlarmStatus A 
                    inner join TagData B on A.tag_data_id=B.ID
                    where A.dept_id=@Dept");
            if (txtNameQ.Text.Trim().Length > 0) { str.Append(" and B.Tag_Name like '%'+@Name+'%'"); }
            if (chkType.SelectedValue.Length > 0)
            {
                str.Append(" and (");
                foreach (ListItem item in chkType.Items.Cast<ListItem>())
                {
                    if (item.Selected)
                    {
                        switch (item.Value)
                        {
                            case "1":
                                str.Append("(B.ALL_Enable<>A.ALL_Enable) or ");
                                break;
                            case "2":
                                str.Append("(B.HIHI_Enable<>A.HIHI_Enable) or ");
                                break;
                            case "3":
                                str.Append("(B.HI_Enable<>A.HI_Enable) or ");
                                break;
                            case "4":
                                str.Append("(B.LO_Enable<>A.LO_Enable) or ");
                                break;
                            case "5":
                                str.Append("(B.LOLO_Enable<>A.LOLO_Enable) or ");
                                break;
                            case "6":
                                str.Append("(A.HIHI<>B.HiHi_Limit) or ");
                                break;
                            case "7":
                                str.Append("(A.HI<>B.Hi_Limit) or ");
                                break;
                            case "8":
                                str.Append("(A.LO<>B.Lo_Limit) or ");
                                break;
                            case "9":
                                str.Append("(A.LOLO<>B.LoLo_Limit) or ");
                                break;
                            case "10":
                                str.Append("(B.quality<>192) or ");
                                break;
                        }
                    }
                }
                str.Remove(str.Length - 3, 3).Append(")");
            }
            bPara.Command = str.ToString();
            bPara.Parameter.Clear();
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Name", txtNameQ.Text.Trim()));
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Dept", ddlDeptQ.DeptID));
            return dDB.QueryDataTable(bPara);
        }

        protected override GridPageOption SetPageSetting()
        {
            return new GridPageOption() { GridOption = new GridOption[] { new GridOption { AutoBind = false, Grid = GridView1, Pager = ucPager, Query = btnQuery } } };
        }

        protected void GridView1_DataBound(object sender, EventArgs e)
        {
            //System.Threading.Tasks.Parallel.ForEach(GridView1.Rows.OfType<GridViewRow>().Where(p => p.RowType == DataControlRowType.DataRow), (GridViewRow gRow) =>
            //{
            //    for (int idx = 1; idx < 6; idx++)
            //    {
            //        TableCell oCell = gRow.Cells[idx];
            //        WebApp._uc.ucYesNo oYesNo = (WebApp._uc.ucYesNo)oCell.FindControl("yn00" + idx.ToString());
            //        if (oYesNo != null && oYesNo.Value == false)
            //        {
            //            oCell.BackColor = System.Drawing.Color.Yellow;
            //        }
            //    }
            //});
            //foreach (GridViewRow gRow in GridView1.Rows)
            //{
            //    for (int idx = 1; idx < 6; idx++)
            //    {
            //        TableCell oCell = gRow.Cells[idx];
            //        WebApp._uc.ucYesNo oYesNo = (WebApp._uc.ucYesNo)oCell.FindControl("yn00" + idx.ToString());
            //        if (oYesNo != null && oYesNo.Value == true)
            //        {
            //            oCell.BackColor = System.Drawing.Color.Yellow;
            //        }
            //    }
            //}
        }

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            //	1全部禁能	2HIHI禁能	3HI禁能	4LO禁能	5LOLO禁能	6HIHI設定值	7HI設定值	8LO設定值	9LOLO設定值
            DataRowView drv = (DataRowView)e.Row.DataItem;

            if ((e.Row.RowType == DataControlRowType.DataRow) || (e.Row.RowType == DataControlRowType.Footer))
            {
                if (drv != null)
                {
                    //judge color
                    if (drv["ALL_Enable"].ToString() != drv["tall"].ToString()) e.Row.Cells[1].BackColor = System.Drawing.Color.Yellow;
                    if (drv["HIHI_Enable"].ToString() != drv["thihi"].ToString()) e.Row.Cells[2].BackColor = System.Drawing.Color.Yellow;
                    if (drv["HI_Enable"].ToString() != drv["thi"].ToString()) e.Row.Cells[3].BackColor = System.Drawing.Color.Yellow;
                    if (drv["LO_Enable"].ToString() != drv["tlo"].ToString()) e.Row.Cells[4].BackColor = System.Drawing.Color.Yellow;
                    if (drv["LOLO_Enable"].ToString() != drv["tlolo"].ToString()) e.Row.Cells[5].BackColor = System.Drawing.Color.Yellow;


                    float.TryParse(drv["HiHi"].ToString(), out var opchihi);
                    float.TryParse(drv["HiHi_Limit"].ToString(), out var hihi);
                    if (drv["HIHI_Enable"].ToString() == "True" && hihi != opchihi)
                        e.Row.Cells[6].BackColor = System.Drawing.Color.Yellow;

                    float.TryParse(drv["Hi"].ToString(), out var opchi);
                    float.TryParse(drv["Hi_Limit"].ToString(), out var hi);
                    if (drv["HI_Enable"].ToString() == "True" && hi != opchi)
                        e.Row.Cells[7].BackColor = System.Drawing.Color.Yellow;

                    float.TryParse(drv["Lo"].ToString(), out var opclo);
                    float.TryParse(drv["Lo_Limit"].ToString(), out var lo);
                    if (drv["LO_Enable"].ToString() == "True" && lo != opclo)
                        e.Row.Cells[8].BackColor = System.Drawing.Color.Yellow;

                    float.TryParse(drv["LoLo"].ToString(), out var opclolo);
                    float.TryParse(drv["LoLo_Limit"].ToString(), out var lolo);
                    if (drv["LOLO_Enable"].ToString() == "True" && lolo != opclolo)
                        e.Row.Cells[9].BackColor = System.Drawing.Color.Yellow;

                    if (drv["quality"].ToString() != "192") e.Row.Cells[11].BackColor = System.Drawing.Color.LightPink;

                    //change text
                    e.Row.Cells[1].Text = drv["tall"].ToString() == "True" ? "否" : "是";
                    e.Row.Cells[2].Text = drv["thihi"].ToString() == "True" ? "否" : "是";
                    e.Row.Cells[3].Text = drv["thi"].ToString() == "True" ? "否" : "是";
                    e.Row.Cells[4].Text = drv["tlo"].ToString() == "True" ? "否" : "是";
                    e.Row.Cells[5].Text = drv["tlolo"].ToString() == "True" ? "否" : "是";
                }
            }
        }
    }
}