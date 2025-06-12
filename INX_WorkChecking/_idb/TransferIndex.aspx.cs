using cyc.Data;
using cyc.Page;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace WebApp._idb
{
    public partial class TransferIndex : BasePageGrid
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (!IsPostBack)
            {
                if (bUser != null)
                {
                    ddlFactory.DataSource = dDB.QueryList<string>("select distinct ind_fac from IFMTransferIndex order by ind_fac");
                    ddlFactory.DataBind();
                    ddlFactory.Items.Insert(0, "");

                    LoadSysData();

                    //ddlSystem.DataSource = dDB.QueryList<string>("select distinct ind_System from IFMTransferIndex order by ind_System");
                    //ddlSystem.DataBind();
                    //ddlSystem.Items.Insert(0, "");
                }
            }
        }

        protected override void QueryCheck(int idx)
        {

        }

        protected override DataTable QuerySourceData(int idx)
        {
            bPara.Command = $@"select * from IFMTransferIndex A where 1=1
{(string.IsNullOrEmpty(ddlFactory.SelectedValue) ? string.Empty : "and A.ind_fac=@Fac")}
{(string.IsNullOrEmpty(ddlSystem.SelectedValue) ? string.Empty : "and A.ind_System=@Sys")}
{(string.IsNullOrWhiteSpace(txtTagName.Text) ? string.Empty : "and A.ind_tagname like @Tag")}";

            var oObj = new { Fac = ddlFactory.SelectedValue, Sys = ddlSystem.SelectedValue, Tag = $"%{txtTagName.Text.Trim()}%" };
            return dDB.QueryDataTable(bPara.Command, oObj);
        }

        protected override GridPageOption SetPageSetting()
        {
            return new GridPageOption() { GridOption = new GridOption[] { new GridOption { AutoBind = true, Grid = GridView1, Pager = ucPager, Query = btnQuery } } };
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            using (DataTable oDT = QuerySourceData(0))
            {
                if (oDT != null)
                {
                    Response.Clear();
                    Response.AddHeader("Content-Disposition", "attachment;filename=" + System.Web.HttpUtility.UrlEncode("TransferIndex維護") + ".csv");
                    Response.ContentType = "application/octet-stream";

                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(Response.OutputStream, System.Text.Encoding.UTF8))
                    {
                        string[] x = "SEQ_ID,ind_no,ind_source_note,scada_tagname,ind_tagname,ind_fac,ind_fab,ind_System,ind_unit,ind_eqp_group,ind_section,ind_Common,ind_level,ind_priority,ind_col_index,ind_row_index".Split(',');

                        sw.WriteLine(string.Join(",", x));
                        //sw.WriteLine("SEQ_ID,ind_no,ind_source_note,ind_tagname,ind_fac,ind_fab,ind_System,ind_unit,ind_eqp_group,ind_section,ind_Common,ind_level,ind_priority,ind_col_index,ind_row_index");

                        if (oDT.Rows.Count > 0)
                        {
                            DataView dv = oDT.DefaultView;
                            dv.Sort = GetSort(0);
                            for (int idx = 0; idx < dv.Count; idx++)
                            {
                                sw.WriteLine(string.Join(",", x.Select(p => dv[idx][p])));
                                //sw.WriteLine($"{dv[idx]["SEQ_ID"]},{dv[idx]["ind_no"]},{dv[idx]["ind_source_note"]},{dv[idx]["tf_ack_flag"]},{dv[idx]["tf_sn"]},{dv[idx]["created"]:yyyy-MM-dd HH:mm:ss}");
                            }
                        }

                        sw.Flush();
                        Response.Flush();
                        Response.End();
                    }
                }
            }
        }

        protected void ddlFactory_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadSysData();
        }

        private void LoadSysData()
        {
            ddlSystem.DataSource = dDB.QueryList<string>($"select distinct ind_System from IFMTransferIndex {(string.IsNullOrEmpty(ddlFactory.SelectedValue) ? string.Empty : "where ind_fac=@Fac")} order by ind_System", new { Fac = ddlFactory.SelectedValue });
            ddlSystem.DataBind();
            ddlSystem.Items.Insert(0, "");
        }
    }
}