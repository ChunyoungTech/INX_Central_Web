using cyc.Page;
using Microsoft.AspNet.SignalR.Hosting;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using CYCloud.WorkCheck;

namespace WebApp._app
{
    public partial class WorkCheckChart : BasePageGrid
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (!IsPostBack)
            {
                dteDateS.Text = DateTime.Today.AddMonths(-1).ToString("yyyy/MM/dd");
                dteDateE.Text = DateTime.Today.ToString("yyyy/MM/dd");
                if (bUser != null)
                {
                    ddlFAC.DataSource = cyc.UC.DeptControl.GetFacCode(bUser.User.DeptLevel).OrderBy(p => p);
                    ddlFAC.DataBind();

                    //加入總廠條件
                    var uDept = cyc.Global.SysDept.List.FirstOrDefault(p => p.ID == bUser.User.DeptLevel);
                    if (uDept != null && uDept.LevelNo == 1)
                        ddlFAC.Items.Insert(0, new ListItem("總廠", "ALL"));
                }
            }
        }

        protected override void QueryCheck(int idx)
        {
            if (dteDateS.Value == null || dteDateE.Value == null)
                oResult.Error("[施工日期]格式錯誤");
            else if (((DateTime)dteDateS.Value).AddYears(1) < (DateTime)dteDateE.Value)
                oResult.Error("[日期區間]不可超過1年");
        }

        protected override DataTable QuerySourceData(int idx)
        {
            if (oResult.Success)
            {
                var qList = GetDataList();
                if (oResult.Success)
                {
                    ltlVendor.Text = ddlFAC.SelectedValue == "ALL" ? "施工廠別" : "施工廠商";
                    if (qList == null || qList.Count == 0) 
                    {
                        ltlContent.Text = "<tr><td colspan='18'><div class='NoData'>查無符合條件資料</div></td></tr>";
                    }
                    else
                    {
                        ltlContent.Text = $@"{string.Join(string.Empty, qList.Select(p => $@"
<tr>
<td>{p.Vendor}</td>
<td class='td-right'><span data-v='{p.Vendor}' data-t='' class='work-type'>{p.Cnt01:N0}</span></td>
<td class='td-right'>{p.Cnt02:N0}</td>
<td class='td-right'>{p.AccCnt:N0}</td>
<td class='td-right'>{p.ChkCnt:N0}</td>
<td class='td-right'>{p.Rate01:P2}</td>
<td class='td-right'>{p.Rate02:P2}</td>
<td class='td-right'>{(p.T01 > 0 ? $"<span data-v='{p.Vendor}' data-t='T01' class='work-type'>{p.T01:N0}</span>" : string.Empty)}</td>
<td class='td-right'>{(p.T02 > 0 ? $"<span data-v='{p.Vendor}' data-t='T02' class='work-type'>{p.T02:N0}</span>" : string.Empty)}</td>
<td class='td-right'>{(p.T03 > 0 ? $"<span data-v='{p.Vendor}' data-t='T03' class='work-type'>{p.T03:N0}</span>" : string.Empty)}</td>
<td class='td-right'>{(p.T04 > 0 ? $"<span data-v='{p.Vendor}' data-t='T04' class='work-type'>{p.T04:N0}</span>" : string.Empty)}</td>
<td class='td-right'>{(p.T05 > 0 ? $"<span data-v='{p.Vendor}' data-t='T05' class='work-type'>{p.T05:N0}</span>" : string.Empty)}</td>
<td class='td-right'>{(p.T06 > 0 ? $"<span data-v='{p.Vendor}' data-t='T06' class='work-type'>{p.T06:N0}</span>" : string.Empty)}</td>
<td class='td-right'>{(p.T07 > 0 ? $"<span data-v='{p.Vendor}' data-t='T07' class='work-type'>{p.T07:N0}</span>" : string.Empty)}</td>
<td class='td-right'>{(p.T08 > 0 ? $"<span data-v='{p.Vendor}' data-t='T08' class='work-type'>{p.T08:N0}</span>" : string.Empty)}</td>
<td class='td-right'>{(p.T09 > 0 ? $"<span data-v='{p.Vendor}' data-t='T09' class='work-type'>{p.T09:N0}</span>" : string.Empty)}</td>
<td class='td-right'>{(p.T10 > 0 ? $"<span data-v='{p.Vendor}' data-t='T10' class='work-type'>{p.T10:N0}</span>" : string.Empty)}</td>
<td class='td-right'>{(p.T11 > 0 ? $"<span data-v='{p.Vendor}' data-t='T11' class='work-type'>{p.T11:N0}</span>" : string.Empty)}</td>
</tr>"))}
<tr>
<td colspan='7'>
</td><td class='td-right'>{qList.Sum(p => p.T01):N0}</td>
</td><td class='td-right'>{qList.Sum(p => p.T02):N0}</td>
</td><td class='td-right'>{qList.Sum(p => p.T03):N0}</td>
</td><td class='td-right'>{qList.Sum(p => p.T04):N0}</td>
</td><td class='td-right'>{qList.Sum(p => p.T05):N0}</td>
</td><td class='td-right'>{qList.Sum(p => p.T06):N0}</td>
</td><td class='td-right'>{qList.Sum(p => p.T07):N0}</td>
</td><td class='td-right'>{qList.Sum(p => p.T08):N0}</td>
</td><td class='td-right'>{qList.Sum(p => p.T09):N0}</td>
</td><td class='td-right'>{qList.Sum(p => p.T10):N0}</td>
</td><td class='td-right'>{qList.Sum(p => p.T11):N0}</td>
</tr>";
                    }

                    //GridView1.Columns[0].HeaderText = ddlFAC.SelectedValue == "ALL" ? "施工廠別" : "施工廠商";
                    //GridView1.DataSource = qList;
                    //GridView1.DataBind();

                    var pList = qList.Where(p => p.AccCnt > 0);
                    hidChartValue.Value = Newtonsoft.Json.JsonConvert.SerializeObject(new List<ChartData>()
                    {
                        new ChartData{ Name = "一般作業", Count = pList.Sum(p => p.T01) },
                        new ChartData{ Name = "動火作業", Count = pList.Sum(p => p.T02) },
                        new ChartData{ Name = "送電、活線作業或活線接近作業", Count = pList.Sum(p => p.T03) },
                        new ChartData{ Name = "高架作業", Count = pList.Sum(p => p.T04) },
                        new ChartData{ Name = "吊掛作業", Count = pList.Sum(p => p.T05) },
                        new ChartData{ Name = "局限空間作業", Count = pList.Sum(p => p.T06) },
                        new ChartData{ Name = "路面開挖作業", Count = pList.Sum(p => p.T07) },
                        new ChartData{ Name = "Inter-Lock by pass", Count = pList.Sum(p => p.T08) },
                        new ChartData{ Name = "安全防護系統中斷/隔離作業", Count = pList.Sum(p => p.T09) },
                        new ChartData{ Name = "危險管路拆卸鑽孔作業與化學品塗佈作業", Count = pList.Sum(p => p.T10) },
                        new ChartData{ Name = "開孔/防墬安全設施拆除作業", Count = pList.Sum(p => p.T11) }
                    });

                    hidDateS.Value = dteDateS.Text;
                    hidDateE.Value = dteDateE.Text;
                    hidFac.Value = ddlFAC.SelectedValue;
                }
            }
            if (!oResult.Success) hidChartValue.Value = "";

            return null;
        }

        protected override GridPageOption SetPageSetting()
        {
            return new GridPageOption();
            //return new GridPageOption() { GridOption = new GridOption[] { new GridOption { AutoBind = false, Grid = GridView1, Pager = null, InfluxDB = btnQuery } } };
        }

        class ChartData
        {
            public string Name { get; set; }
            public int Count { get; set; }
        }

        List<WorkCheckReport.QryData> GetDataList()
        {
            QueryCheck(0);
            if (oResult.Success)
            {
                var xList = WorkCheckReport.GetQueryList((DateTime)dteDateS.Value, (DateTime)dteDateE.Value, dDB, ddlFAC.SelectedValue);
                if (xList != null)
                    return WorkCheckReport.GetSummaryData(xList, ddlFAC.SelectedValue);
            }
            return null;
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            var qList = GetDataList();

            if (oResult.Success && qList != null)
            {
                byte[] oFile = WorkCheckReport.CreateExcelFile(qList, ddlFAC.SelectedValue);
                if (oFile != null)
                {
                    Response.Clear();
                    Response.AddHeader("Content-Disposition", "attachment;filename=" + System.Web.HttpUtility.UrlEncode($"{ddlFAC.SelectedItem.Text}施工統計表({dteDateS.Value:yyyyMMdd}~{dteDateE.Value:yyyyMMdd}).xlsx"));
                    Response.ContentType = "application/octet-stream";
                    Response.OutputStream.Write(oFile, 0, oFile.Length);
                    Response.OutputStream.Flush();
                    Response.OutputStream.Close();
                }
                Response.Flush();
                Response.End();
            }
        }

        protected void btnQuery_Click(object sender, EventArgs e)
        {
            QueryCheck(0);
            if (oResult.Success)
                QuerySourceData(0);
            else
                ShowResult("");
        }
    }
}