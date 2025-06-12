using cyc.Page;
using Inx.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._report
{
    public partial class IndirectMaterials : InxCentralReportPage
    {
        protected override ReportOption SetReportOption()
        {
            return new ReportOption
            {
                ReportID = 2,
                ReportName = "間材(扣除非廠務)",
                TableName = "IndirectMaterials",
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
                var xSUM = cList.FirstOrDefault(p => p.AddSUM);
                var xAVG = cList.FirstOrDefault(p => p.AddAVG);
                //bool showSUM = cList.Any(p => p.AddSUM);
                //bool showAVG = cList.Any(p => p.AddAVG);
                
                ltlContent01.Text = $"<tr><th style='width:5em;'>廠別</th><th>費用</th>{CreateTableHeader2(QryYear, xSUM, xAVG)}</tr>";

                System.Text.StringBuilder oStr = new System.Text.StringBuilder();
                foreach (var cData in cList.GroupBy(p => p.FAC).OrderBy(p => p.First().IsSUM || p.First().IsSUM))
                {
                    int idx = 0;
                    foreach (var c in cData)
                    {
                        if (c.IsSUM || c.IsAVG) CreateSummaryData(c, cList.Where(p => !(p.IsSUM || p.IsAVG)));
                        oStr.AppendLine($"<tr data-key='{c.CategoryID}' data-y='{QryYear}'>{(0 == idx++ ? $"<th rowspan='{cData.Count()}'>{cData.Key}</th>" : string.Empty)}<td>{c.Level01}</td>{(c.IsSUM || c.IsAVG ? GetReadOnlyRow(c) : GetNumberEditRow(c))}{GetAddSUM(c, xSUM != null)}{GetAddAVG(c, xAVG != null)}</tr>");
                    }
                }
                ltlContent02.Text = oStr.ToString();
            }
        }
    }
}