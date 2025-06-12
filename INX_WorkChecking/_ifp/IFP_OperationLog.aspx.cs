using pin.Page;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Dapper;

namespace WebApp._ifp
{
    public partial class IFP_OperationLog : BasePageGridMulti
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (!IsPostBack)
            {
                dteDateE.Text = DateTime.Today.ToString("yyyy/MM/dd");
                dteDateS.Text = DateTime.Today.AddDays(-6).ToString("yyyy/MM/dd");
                ddlMaterial.DataSource = bDB.QueryList<CYCloud.IFP.BaseData>("select PortNo as ID,Name from IFP_Material where " + CYCloud.DeptControl.GetDeptLimitSQL(bUser, "TypeID"));
                ddlMaterial.DataBind();
                ddlMaterial.Items.Insert(0, "");
            }
        }

        protected override DataTable QuerySourceData(int idx)
        {
            if (!pin.Comm.Check.IsDateTime(dteDateS.Text)) { DateTime.Today.AddDays(-6).ToString("yyyy/MM/dd"); }
            if (!pin.Comm.Check.IsDateTime(dteDateE.Text)) { DateTime.Today.ToString("yyyy/MM/dd"); }
            DateTime DateS = Convert.ToDateTime(dteDateS.Text);
            DateTime DateE = DateE = Convert.ToDateTime(dteDateE.Text).AddDays(1).AddMilliseconds(-1);

            bPara.Command = @"
select A.*,B.[Name] as PortName,C.[Name] as UserName from IFP_OperationLog A
left join IFP_Material B on A.Material_Port=B.PortNo
left join SysUser C on A.User_ID=C.ID
where A.Operation_Time between @DateS and @DateE";
            if (ddlMaterial.SelectedValue.Length > 0) { bPara.Command += " and A.Material_Port=@Port"; }
            bPara.Command += " order by A.Operation_Time";
            bPara.Parameter.Clear();
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("DateS", DateS));
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("DateE", DateE));
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Port", ddlMaterial.SelectedValue));
            return bDB.QueryDataTable(bPara);
        }

        protected override GridPageSetting SetPageSetting()
        {
            return new GridPageSetting() { Option = new GridOption[] { new GridOption { AutoBind = true, Grid = GridView1, Pager = ucPager, Query = btnQuery, Refresh = lbRefresh } } };
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            using (DataTable oDT = QuerySourceData(0))
            {
                if (oResult.Success)
                {
                    //using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                    //{
                    //    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(ms, System.Text.Encoding.UTF8))
                    //    {
                    //        sw.WriteLine("充填口名稱,操作時間,操作內容,使用者名稱,圖控操作人員");

                    //        foreach (DataRow row in oDT.Rows)
                    //        {
                    //            sw.WriteLine(string.Join(",", new List<string>
                    //            {
                    //                row["PortName"].ToString(),
                    //                Convert.ToDateTime(row["Operation_Time"]).ToString("yyyy/MM/dd HH:mm:ss"),
                    //                row["Operation_Log"].ToString().Replace(",", "，"),
                    //                row["UserName"].ToString(),
                    //                row["SCADA_USER"].ToString()
                    //            }));
                    //        }

                    //        sw.Flush();

                    //        Response.Clear();
                    //        Response.AddHeader("Content-Disposition", "attachment;filename=" + System.Web.HttpUtility.UrlEncode("填充操作記錄") + ".csv");
                    //        Response.ContentType = "application/octet-stream";
                    //        Response.BinaryWrite(ms.GetBuffer());
                    //        //Response.Output.Write(ms.GetBuffer(), 0, ms.GetBuffer().Length);
                    //        ms.Flush();
                    //        Response.Flush();
                    //        Response.End();
                    //    }
                    //}

                    Response.Clear();
                    Response.AddHeader("Content-Disposition", "attachment;filename=" + System.Web.HttpUtility.UrlEncode("填充操作記錄") + ".csv");
                    Response.ContentType = "application/octet-stream";

                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(Response.OutputStream, System.Text.Encoding.UTF8))
                    {
                        sw.WriteLine("充填口名稱,操作時間,操作內容,使用者名稱,圖控操作人員");

                        foreach (DataRow row in oDT.Rows)
                        {
                            sw.WriteLine(string.Join(",", new List<string>
                                {
                                    row["PortName"].ToString(),
                                    Convert.ToDateTime(row["Operation_Time"]).ToString("yyyy/MM/dd HH:mm:ss"),
                                    row["Operation_Log"].ToString().Replace(",", "，"),
                                    row["UserName"].ToString(),
                                    row["SCADA_USER"].ToString()
                                }));
                        }
                        sw.Flush();

                        Response.Flush();
                        Response.End();
                    }
                }
            }
        }
    }
}