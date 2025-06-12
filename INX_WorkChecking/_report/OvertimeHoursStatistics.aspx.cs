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
    public partial class OvertimeHoursStatistics : InxCentralReportPage
    {
        protected override ReportOption SetReportOption()
        {
            return new ReportOption
            {
                ReportID = 9,
                ReportName = "加班時數統計",
                TableName = "OvertimeHoursStatistics",
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
                bool showSUM = cList.Any(p => p.AddSUM);
                bool showAVG = cList.Any(p => p.AddAVG);

                ltlContent01.Text = $"<tr><th style='width:5em;'>廠別</th><th></th>{CreateTableHeader(QryYear, showSUM, showAVG)}</tr>";

                System.Threading.Tasks.Parallel.ForEach(cList.GroupBy(p => p.FAC), (vList) =>
                {
                    int idx = 0;
                    foreach (var v in vList)
                    {
                        if (v.IsSUM || v.IsAVG) CreateSummaryData(v, cList.Where(p => !(p.IsSUM || p.IsAVG) && p.Level02 == v.Level02));
                        v.Html = $"<tr data-key='{v.CategoryID}' data-y='{QryYear}'>{(idx == 0 ? $"<th rowspan='{vList.Count()}'>{vList.Key}</th>" : string.Empty)}<td>{v.Level02}</td>{(v.IsSUM || v.IsAVG ? GetReadOnlyRow(v) : GetNumberEditRow(v))}{GetAddSUM(v, showSUM)}{GetAddAVG(v, showAVG)}</tr>";
                        idx++;
                    }
                });
                ltlContent02.Text = string.Join(string.Empty, cList.Select(p => p.Html));

                System.Text.StringBuilder oStr = new System.Text.StringBuilder(string.Empty);
                //foreach (var cData in cList.GroupBy(p => p.FAC))
                //{
                //    int idx = 0;
                //    foreach (var c in cData)
                //    {
                //        if (!(c.IsSUM || c.IsAVG))
                //        {
                //            oStr.AppendLine($"<tr data-key='{c.CategoryID}' data-y='{QryYear}'>{(idx == 0 ? $"<th rowspan='{cData.Count()}'>{cData.Key}</th>" : string.Empty)}<td>{c.Level02}</td>{GetNumberEditRow(c)}{GetAddSUM(c, showSUM)}{GetAddAVG(c, showAVG)}</tr>");
                //        }
                //        else
                //        {
                //            CreateSummaryData(c, cList.Where(p => !(p.IsSUM || p.IsAVG) && p.Level02 == c.Level02));
                //            oStr.AppendLine($"<tr data-key='{c.CategoryID}' data-y='{QryYear}'>{(idx == 0 ? $"<th rowspan='{cData.Count()}'>{cData.Key}</th>" : string.Empty)}<td>{c.Level02}</td>{GetReadOnlyRow(c)}{GetAddSUM(c, showSUM)}{GetAddAVG(c, showAVG)}</tr>");
                //        }
                //        idx++;
                //    }
                //}
                //ltlContent02.Text = oStr.ToString();
            }
        }
    }
}