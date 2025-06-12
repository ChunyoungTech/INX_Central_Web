using cyc.Page;
using Inx.Data;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._report
{
    public partial class ObsoleteInventoryClearance : InxCentralReportPage
    {
        protected override ReportOption SetReportOption()
        {
            return new ReportOption
            {
                ReportID = 7,
                ReportName = "備堪品去化",
                TableName = "ObsoleteInventoryClearance",
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
                var xList = cList.GroupBy(p => new { p.Level02, p.Level03 }).OrderBy(p => p.First().IsSUM).Select(p => new
                {
                    p.Key.Level02,
                    p.Key.Level03,
                    DataList = p.ToList()
                });

                System.Text.StringBuilder oStr = new System.Text.StringBuilder("");
                foreach (var x in xList)
                {
                    oStr.Append($@"
<div class='div-category-type'><span>{x.Level02}</span> <span>{x.Level03}</span></div>
<div class='fix-table'>
<table class='MainGridView'>
<thead>
<tr><th style='width:5em;'>廠別</th>{CreateTableHeader(QryYear)}</tr>
</thead>
<tbody>
{(string.Join(string.Empty, x.DataList.Select(p => $"<tr data-key='{p.CategoryID}' data-y='{QryYear}'><th>{p.FAC}</th>{GetRowData(p)}</tr>")))}
</tbody>
</table></div>");

                    string GetRowData(GridValue v)
                    {
                        if (string.IsNullOrEmpty(v.AutoData))
                        {
                            if (v.IsSUM)
                            {
                                CreateSummaryData(v, cList.Where(p => !(p.IsSUM || p.IsAVG) && p.Level01 == v.Level01 && p.Level02 == v.Level02));
                                return GetReadOnlyRow(v);
                            }
                            return GetNumberEditRow(v);
                        }
                        else //if (v.AutoData == "AS")
                        {
                            return GetAutoDataRow(v, cList.FirstOrDefault(p => p.FAC == v.FAC && p.Level01 == v.Level01 && p.Level02 == v.Level02 && string.IsNullOrEmpty(p.AutoData)));
                        }
                    }
                }
                ltlContent01.Text = oStr.ToString();
            }
        }
    }
}