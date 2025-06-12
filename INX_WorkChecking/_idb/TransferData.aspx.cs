using cyc.Data;
using cyc.Page;
using Microsoft.AspNet.SignalR.Hosting;
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
    public partial class TransferData : BasePageGrid
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (!IsPostBack)
            {
                dteDateS.Text = DateTime.Today.AddDays(-7).ToString("yyyy/MM/dd");
                dteDateE.Text = DateTime.Today.ToString("yyyy/MM/dd");
                if (bUser != null)
                {

                }
            }
        }

        protected override void QueryCheck(int idx)
        {
            if (dteDateS.Value == null || dteDateE.Value == null)
                oResult.Error("[日期區間]不可空白且須為日期格式");
            else if (((DateTime)dteDateS.Value).AddDays(90) < (DateTime)dteDateE.Value)
                oResult.Error("[日期區間]不可超過90天");
        }

        protected override DataTable QuerySourceData(int idx)
        {
            DateTime DateS = (DateTime)dteDateS.Value;
            DateTime DateE = ((DateTime)dteDateE.Value).AddDays(1).AddSeconds(-1);

            return dDB.QueryDataTable($@"
select * from IFMTransferData where [created] between @DateS and @DateE
{(string.IsNullOrWhiteSpace(txtTagName.Text) ? string.Empty : "and tf_tagname like @Tag")}", new { DateS, DateE, Tag = $"%{txtTagName.Text}%" });
        }

        protected override GridPageOption SetPageSetting()
        {
            return new GridPageOption() { GridOption = new GridOption[] { new GridOption { AutoBind = true, Grid = GridView1, Pager = ucPager, Query = btnQuery } } };
        }

        class UploadData
        {
            public int SeqID { get; set; }
            public int TagID { get; set; }
            public bool Enable { get; set; }
            public DateTime? LastTime { get; set; }
            public bool IsRed { get; set; }
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            using (DataTable oDT = QuerySourceData(0))
            {
                if (oDT != null)
                {
                    Response.Clear();
                    Response.AddHeader("Content-Disposition", "attachment;filename=" + System.Web.HttpUtility.UrlEncode("TransferData歷史查詢") + ".csv");
                    Response.ContentType = "application/octet-stream";

                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(Response.OutputStream, System.Text.Encoding.UTF8))
                    {
                        sw.WriteLine("資料型態,資料點名稱,數值,ACK_FLAG,SN,產生時間");

                        if (oDT.Rows.Count > 0)
                        {
                            DataView dv = oDT.DefaultView;
                            dv.Sort = GetSort(0);
                            for (int idx = 0; idx < dv.Count; idx++)
                            {
                                sw.WriteLine($"{dv[idx]["tf_data_source"]},{dv[idx]["tf_tagname"]},{dv[idx]["tf_value"]},{dv[idx]["tf_ack_flag"]},{dv[idx]["tf_sn"]},{dv[idx]["created"]:yyyy-MM-dd HH:mm:ss}");
                            }
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