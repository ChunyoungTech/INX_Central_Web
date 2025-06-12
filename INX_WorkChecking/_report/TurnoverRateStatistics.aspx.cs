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
    public partial class TurnoverRateStatistics : InxCentralReportPage
    {
        protected override ReportOption SetReportOption()
        {
            return new ReportOption
            {
                ReportID = 8,
                ReportName = "離職率統計",
                TableName = "TurnoverRateStatistics",
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
                System.Text.StringBuilder oStr = new System.Text.StringBuilder(string.Empty);

                bool showSUM = cList.Any(p => p.AddSUM);
                bool showAVG = cList.Any(p => p.AddAVG);

                ltlContent01.Text = $"<tr><th style='width:5em;'>廠別</th><th></th>{CreateTableHeader(QryYear, showSUM, showAVG)}</tr>";
                foreach (var cData in cList.GroupBy(p => p.FAC).OrderBy(p => p.First().IsSUM))
                {
                    int idx = 0;
                    foreach (var c in cData)
                    {
                        if (c.IsSUM || c.IsAVG) CreateSummaryData(c, cList.Where(p => !(p.IsSUM || p.IsAVG)));
                        oStr.AppendLine($"<tr data-key='{c.CategoryID}' data-y='{QryYear}'>{(idx == 0 ? $"<th rowspan='{cData.Count()}'>{cData.Key}</th>" : string.Empty)}<td>{c.Level02}</td>{(c.IsSUM || c.IsAVG ? GetReadOnlyRow(c) : GetNumberEditRow(c))}{GetAddSUM(c, showSUM)}{GetAddAVG(c, showAVG)}</tr>");
                        //if (!c.IsSUM)
                        //{
                        //    oStr.AppendLine($"<tr data-key='{c.CategoryID}' data-y='{QryYear}'>{(idx == 0 ? $"<th rowspan='{cData.Count()}'>{cData.Key}</th>" : string.Empty)}<td>{c.Level02}</td>{GetNumberEditRow(c)}{GetAddSUM(c, showSUM)}{GetAddAVG(c, showAVG)}</tr>");
                        //}
                        //else
                        //{
                        //    CreateSummaryData(c, cList.Where(p => !(p.IsSUM || p.IsAVG)));
                        //    oStr.AppendLine($"<tr data-key='{c.CategoryID}' data-y='{QryYear}'>{(idx == 0 ? $"<th rowspan='{cData.Count()}'>{cData.Key}</th>" : string.Empty)}<td>{c.Level02}</td>{GetReadOnlyRow(c)}{GetAddSUM(c, showSUM)}{GetAddAVG(c, showAVG)}</tr>");
                        //}
                        idx++;
                    }
                }
                ltlContent02.Text = oStr.ToString();
            }
        }       
    }
}