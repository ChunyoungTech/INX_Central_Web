using cyc.Page;
using Inx.Data;
using Microsoft.AspNet.SignalR.Hosting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace WebApp._report
{
    public partial class FactoryExpenseAnalysis : InxCentralReportPage
    {
        protected override ReportOption SetReportOption()
        {
            return new ReportOption
            {
                ReportID = 1,
                ReportName = "廠務費用分析",
                TableName = "FactoryExpenseAnalysis",
                Query = btnQuery,
                Update = btnUpdate,
                Year = txtYear,
                Factory = ddlFAC,
                Level01 = null,
                Auth = hidAuth
            };
        }

        protected override void CreateReportData(List<GridValue> cList)
        {
            ltlContent01.Text = $"<tr><th style='width:5em;'>廠別</th><th>主類別</th><th>次類別</th>{CreateTableHeader(QryYear)}</tr>";

            //System.Threading.Tasks.Parallel.ForEach(cList.GroupBy(p => p.FAC), (vList) => 
            //{
            //    int idx = 0;
            //    foreach (var v in vList)
            //        v.Html = $"<tr data-key='{v.CategoryID}' data-y='{QryYear}'>{(0 == idx++ ? $"<th rowspan='{vList.Count()}'>{vList.Key}</th>" : string.Empty)}<th>{v.Level01}</th><th>{v.Level02}</th>{GetNumberEditRow(v)}</tr>";
            //});
            //ltlContent02.Text = string.Join(string.Empty, cList.Select(p => p.Html));

            System.Text.StringBuilder oStr = new System.Text.StringBuilder();
            foreach (var vList in cList.GroupBy(p => p.FAC))
            {
                int idx = 0;
                foreach (var v in vList)
                    oStr.AppendLine($"<tr data-key='{v.CategoryID}' data-y='{QryYear}'>{(0 == idx++ ? $"<th rowspan='{vList.Count()}'>{vList.Key}</th>" : string.Empty)}<th>{v.Level01}</th><th>{v.Level02}</th>{GetNumberEditRow(v)}</tr>");
            }
            ltlContent02.Text = oStr.ToString();
        }

        //protected void btnExport_Click(object sender, EventArgs e) { }

        //        protected void btnQuery_Click(object sender, EventArgs e)
        //        {
        //            if (int.TryParse(txtYear.Text.Trim(), out QryYear))
        //            {
        //                var xData = dDB.QueryMultiple($@"
        //select ID as CategoryID,FAC,Level01,Level02,DataType,@Year as [Year] from ReportCategory
        //where Report=@ReportID {(string.IsNullOrEmpty(ddlFAC.SelectedValue) ? string.Empty : "and FAC=@Fac")} and @Year between YearS and ISNULL(YearE,2099) order by SeqNo,ID
        //;
        //select A.ID as CategoryID,B.[Month],B.[ValueNum] from ReportCategory A
        //inner join {Option.TableName} B on A.ID=B.CategoryID
        //where A.Report=@ReportID {(string.IsNullOrEmpty(ddlFAC.SelectedValue) ? string.Empty : "and A.FAC=@Fac")} and @Year between A.YearS and ISNULL(A.YearE,2099) and B.[Year]=@Year", new { Year = QryYear, Fac = ddlFAC.SelectedValue, Option.ReportID });

        //                if (xData != null)
        //                {
        //                    var cList = xData.Read<GridValue>().ToList();
        //                    var vList = xData.Read<TempValue>();

        //                    if (cList.Any() && vList.Any())
        //                    {
        //                        System.Threading.Tasks.Parallel.ForEach(cList, (cData) => {
        //                            var vData = vList.Where(p => p.CategoryID == cData.CategoryID && p.Month > 0 && p.Month < 13);
        //                            System.Threading.Tasks.Parallel.ForEach(vData, (v) => {
        //                                cData.Values[v.Month - 1] = v.ValueNum;
        //                            });
        //                        });
        //                    }

        //                    //GridView1.DataSource = cList;
        //                    //GridView1.DataBind();

        //                    //for (int idx = 1; idx < 13; idx++)
        //                    //    GridView1.HeaderRow.Cells[idx + 2].Text = $"{QryYear}/{idx:00}";

        //                    ltlContent01.Text = $"<tr><th style='width:5em;'>廠別</th><th>主類別</th><th>次類別</th><th>{QryYear}/01</th><th>{QryYear}/02</th><th>{QryYear}/03</th><th>{QryYear}/04</th><th>{QryYear}/05</th><th>{QryYear}/06</th><th>{QryYear}/07</th><th>{QryYear}/08</th><th>{QryYear}/09</th><th>{QryYear}/10</th><th>{QryYear}/11</th><th>{QryYear}/12</th></tr>";

        //                    System.Text.StringBuilder oStr = new System.Text.StringBuilder(string.Empty);
        //                    foreach (var cData in cList.GroupBy(p => p.FAC))
        //                    {
        //                        int idx = 0;
        //                        foreach (var c in cData)
        //                        {
        //                            int iMonth = 1;
        //                            oStr.AppendLine($@"
        //<tr data-key='{c.CategoryID}'>{(idx == 0 ? $"<th rowspan='{cData.Count()}'>{cData.Key}</th>" : string.Empty)}<td>{c.Level01}</td><td>{c.Level02}</td>
        //{string.Join(string.Empty, c.Values.Select(p => $"<td><input type='text' name='txt_C{c.CategoryID}_M{iMonth}' class='txt-input-number' value='{p}' data-old='{p}' data-idx='{QryYear},{iMonth++}' /></td>"))}
        //</tr>");
        //                            idx++;
        //                        }
        //                    }
        //                    ltlContent02.Text = oStr.ToString();

        //                    //SetGridRowSpan(GridView1, 0);
        //                }
        //            }
        //            else
        //                oResult.Error("[年度]格式錯誤，必須是整數");

        //            if (!oResult.Success) ShowResult("");

        //            btnUpdate.Visible = oResult.Success;
        //            if (oResult.Success)
        //                Session["AuthKey"] = Guid.NewGuid().ToString("N");
        //            else
        //                Session["AuthKey"] = null;

        //            hidAuth.Value = Session["AuthKey"]?.ToString();
        //            //GridView1.Visible = oResult.Success;
        //        }

        //private void SetGridRowSpan(GridView oGrid, int iCell)
        //{
        //    string TmpStr = string.Empty;
        //    int TmpIndex = 0, TmpCount = 0;
        //    foreach (GridViewRow Row in oGrid.Rows)
        //    {
        //        if (Row.RowType == DataControlRowType.DataRow)
        //        {
        //            if (Row.Cells[iCell].Text != TmpStr)
        //            {
        //                TmpStr = Row.Cells[iCell].Text;
        //                if (TmpCount > 0) { RowSpan(); }
        //                TmpIndex = Row.RowIndex;
        //            }
        //            TmpCount++;
        //        }
        //    }
        //    if (TmpCount > 0) { RowSpan(); }

        //    void RowSpan()
        //    {
        //        oGrid.Rows[TmpIndex].Cells[iCell].Attributes.Add("rowspan", TmpCount.ToString());
        //        oGrid.Rows[TmpIndex].Cells[iCell].CssClass = "td-row-span";
        //        for (int idx = 1; idx < TmpCount; idx++) { oGrid.Rows[TmpIndex + idx].Cells[iCell].Visible = false; }
        //        TmpCount = 0;
        //    }
        //}
    }
}