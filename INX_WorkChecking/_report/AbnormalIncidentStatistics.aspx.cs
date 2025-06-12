using cyc.Page;
using Inx.Data;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._report
{
    public partial class AbnormalIncidentStatistics : InxCentralReportPage
    {
        protected override ReportOption SetReportOption()
        {
            return new ReportOption
            {
                ReportID = 6,
                ReportName = "異常事故統計",
                TableName = "AbnormalIncidentStatistics",
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

                ltlContent01.Text = $"<tr><th style='width:5em;'>廠別</th><th>項次</th>{CreateTableHeader(QryYear, showSUM, showAVG, cList.First().TitleSUM, cList.First().TitleAVG)}</tr>";
                
                System.Threading.Tasks.Parallel.ForEach(cList.OrderBy(p => p.IsSUM || p.IsAVG).GroupBy(p => p.FAC), (cData) =>
                {
                    int idx = 0;
                    foreach (var c in cData)
                    {
                        if (c.IsSUM || c.IsAVG) CreateSummaryData(c, cList.Where(p => !(p.IsSUM || p.IsAVG) && p.Level02 == c.Level02));
                        //oStr.AppendLine($@"<tr data-key='{c.CategoryID}' data-y='{QryYear}'>{(idx == 0 ? $"<th rowspan='{cData.Count()}'>{cData.Key}</th>" : string.Empty)}<td>{c.Level02}</td>{(c.IsSUM || c.IsAVG ? GetReadOnlyRow(c) : GetNumberEditRow(c))}{GetAddSUM(c, showSUM)}{GetAddAVG(c, showAVG)}</tr>");
                        c.Html = $"<tr data-key='{c.CategoryID}' data-y='{QryYear}'>{(idx == 0 ? $"<th rowspan='{cData.Count()}'>{cData.Key}</th>" : string.Empty)}<td>{c.Level02}</td>{(c.IsSUM || c.IsAVG ? GetReadOnlyRow(c) : GetNumberEditRow(c))}{GetAddSUM(c, showSUM)}{GetAddAVG(c, showAVG)}</tr>";
                        idx++;
                    }
                });
                ltlContent02.Text = string.Join(string.Empty, cList.Select(p => p.Html));

                //System.Text.StringBuilder oStr = new System.Text.StringBuilder(string.Empty);
                //foreach (var cData in cList.OrderBy(p => p.IsSUM || p.IsAVG).GroupBy(p => p.FAC))
                //{
                //    int idx = 0;
                //    foreach (var c in cData)
                //    {
                //        if (!c.IsSUM && !c.IsAVG)
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