using cyc.Page;
using Inx.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._report
{
    public partial class KPIConsumptionAndEfficiency : InxCentralReportPage
    {
        protected override ReportOption SetReportOption()
        {
            return new ReportOption
            {
                ReportID = 3,
                ReportName = "KPI耗量與效率",
                TableName = "KPIConsumptionAndEfficiency",
                Query = btnQuery,
                Update = btnUpdate,
                Year = txtYear,
                Factory = ddlFAC,
                Level01 = ddlLevel01,
                Auth = hidAuth
            };
        }

        protected override void CreateReportData(List<GridValue> cList)
        {
            if (cList.Any())
            {
                var gList = cList.GroupBy(p => p.DataType);

                if (gList.Count() == 2 && gList.Count(p => p.Key == "i") == 1)
                {
                    foreach (var gData in gList)
                    {
                        if (gData.Key == "i")
                        {
                            var xList = gData.GroupBy(p => new { p.Level02, p.Level03 }).Select(p => new
                            {
                                p.Key.Level02,
                                p.Key.Level03,
                                DataList = p.ToList()
                            });

                            System.Text.StringBuilder oStr = new System.Text.StringBuilder($"<h3 class='report-level01'>{ddlLevel01.SelectedItem.Text}</h3>");
                            foreach (var x in xList)
                            {
                                bool showSUM = x.DataList.Any(p => p.AddSUM);
                                bool showAVG = x.DataList.Any(p => p.AddAVG);

                                //此報表暫時將 [合計值] 轉為 [累計年平均值]， 呈現順序 先[年平均值]，後[累計年平均值]
                                oStr.Append($@"
<div class='div-category-type'><span>{x.Level02}</span> <span>{x.Level03}</span></div>
<div class='fix-table'>
<table class='MainGridView'>
<thead>
<tr><th style='width:5em;'>廠別</th>{CreateTableHeader(QryYear, showAVG, showSUM, x.DataList.First().TitleAVG, x.DataList.First().TitleSUM)}</tr>
</thead>
<tbody>
{(string.Join(string.Empty, x.DataList.Select(p => $"<tr data-key='{p.CategoryID}' data-y='{QryYear}'><th>{p.FAC}</th>{GetNumberEditRow(p)}{GetAddAVG(p, showAVG)}{GetAddSUM(p, showSUM)}</tr>")))}
</tbody>
</table>
</div>");
                            }
                            ltlContent01.Text = oStr.ToString();
                        }
                        else
                        {
                            var xList = gData.GroupBy(p => new { p.Level02, p.Level03 }).Select(p => new
                            {
                                p.Key.Level02,
                                p.Key.Level03,
                                DataList = p.ToList()
                            });

                            System.Text.StringBuilder oStr = new System.Text.StringBuilder(string.Empty);
                            foreach (var x in xList)
                            {
                                oStr.Append($@"
<div class='div-category-type'><span>{x.Level02}</span> <span>{x.Level03}</span></div>
<div class='fix-table2'>
<table class='MainGridView Grid100'>
<thead>
<tr><th style='width:5em;'>年度月份</th><th style='width:5em;'>廠區</th><th>未達標說明 (請在600字內完成敘述說明，禁止使用 ' 符號，以免造成上傳錯誤)</th></tr>
</thead>
<tbody>
{GetRowData(x.DataList)}
</tbody>
</table>
</div>");
                                string GetRowData(List<GridValue> list)
                                {
                                    int iRow = list.GroupBy(p => p.FAC).Count();
                                    System.Text.StringBuilder str = new System.Text.StringBuilder();
                                    for (int iMonth = 1; iMonth < 13; iMonth++)
                                    {
                                        int idx = 0;
                                        str.Append(string.Join("", list.Select(p => $@"<tr data-key='{p.CategoryID}' data-y='{QryYear}'>{(idx++ == 0 ? $"<th rowspan='{iRow}'>{QryYear}/{(iMonth):00}</th>" : string.Empty)}<th>{p.FAC}</th><td><input type='text' class='txt-input-string' value='{p.ValueStr[iMonth - 1]}' data-old='{p.ValueStr[iMonth - 1]}' data-idx='{iMonth}' /></td></tr>")));
                                        //str.Append(string.Join("", list.Select(p => $@"<tr data-key='{p.CategoryID}'><td>{QryYear}/{(iMonth):00}</td><td>{p.FAC}</td><td><input type='text' class='txt-input-string' value='{p.ValueStr[iMonth - 1]}' data-old='{p.ValueStr[iMonth - 1]}' data-idx='{iMonth}' /></td></tr>")));
                                    }
                                    return str.ToString();
                                }
                            }
                            ltlContent02.Text = oStr.ToString();
                        }
                    }
                }
            }
        }      
    }
}