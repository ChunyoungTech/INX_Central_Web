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
    public partial class AlarmStatistics : InxCentralReportPage
    {
        protected override ReportOption SetReportOption()
        {
            return new ReportOption
            {
                ReportID = 10,
                ReportName = "警報統計",
                TableName = "AlarmStatistics",
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
            if (cList.Any())
            {
                var xList = cList.GroupBy(p => new { p.Level01, p.Level02 }).Select(p => new
                {
                    p.Key.Level01,
                    p.Key.Level02,
                    DataList = p.OrderBy(q => q.IsAVG || q.IsSUM).ToList()
                });

                System.Text.StringBuilder oStr = new System.Text.StringBuilder("");
                foreach (var x in xList)
                {
                    bool showSUM = x.DataList.Any(p => p.AddSUM);
                    bool showAVG = x.DataList.Any(p => p.AddAVG);
                    oStr.Append($@"
<div class='div-category-type'><span>{x.Level01}</span> <span>{x.Level02}</span></div>
<div class='fix-table'>
<table class='MainGridView'>
<thead>
<tr><th style='width:5em;'>廠別</th>{CreateTableHeader(QryYear, showSUM, showAVG)}</tr>
</thead>
<tbody>{GetRowHtml(x.DataList)}</tbody>
</table></div>");

                    string GetRowHtml(List<GridValue> vList)
                    {
                        System.Threading.Tasks.Parallel.ForEach(vList, (v) => 
                        {
                            if (v.IsSUM || v.IsAVG) CreateSummaryData(v, x.DataList.Where(p => !(p.IsSUM || p.IsAVG) && p.Level01 == v.Level01 && p.Level02 == v.Level02));
                            v.Html = $"{(v.IsSUM || v.IsAVG ? GetReadOnlyRow(v) : GetNumberEditRow(v))}{GetAddSUM(v, showSUM)}{GetAddAVG(v, showAVG)}";
                        });
                        return string.Join(string.Empty, vList.Select(p => $"<tr data-key='{p.CategoryID}' data-y='{QryYear}'><th>{p.FAC}</th>{p.Html}</tr>"));
                    }
                }
                ltlContent01.Text = oStr.ToString();
            }
        }
    }
}