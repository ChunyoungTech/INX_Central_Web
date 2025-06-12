using cyc.Page;
using Inx.Data;
using NPOI.SS.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._report
{
    public partial class CostReductionManagement : InxCentralReportPage
    {
        protected override ReportOption SetReportOption()
        {
            return new ReportOption
            {
                ReportID = 4,
                ReportName = "Cost Down 管控_1",
                TableName = "CostReductionManagement",
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
                            var xList = gData.GroupBy(p => new { p.Level02, p.Level03 }).OrderBy(p => p.First().IsSUM).Select(p => new
                            {
                                p.Key.Level02,
                                p.Key.Level03,
                                DataList = p.ToList(),
                            });

                            System.Text.StringBuilder oStr = new System.Text.StringBuilder(string.Empty);
                            foreach (var x in xList)
                            {
                                //bool showSUM = x.DataList.Any(p => p.AddSUM);
                                //bool showAVG = x.DataList.Any(p => p.AddAVG);

                                oStr.Append($@"
<div class='div-category-type'><span>{x.Level02}</span> <span>{x.Level03}</span></div>
<div class='fix-table'>
<table class='MainGridView'>
<thead>
<tr><th style='width:5em;'>廠別</th>{CreateTableHeader(QryYear)}</tr>
</thead>
<tbody>
{string.Join(string.Empty, x.DataList.Select(p => $"<tr data-key='{p.CategoryID}' data-y='{QryYear}'><th>{p.FAC}</th>{GetRowData(p)}</tr>"))}
</tbody>
</table>
</div>");

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
                                        return GetAutoDataRow(v, gData.FirstOrDefault(p => p.FAC == v.FAC && p.Level01 == v.Level01 && p.Level02 == v.Level02 && string.IsNullOrEmpty(p.AutoData)));
                                    }
                                }
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

//        protected override void OnInit(EventArgs e)
//        {
//            base.OnInit(e);
//            if (!IsPostBack)
//            {
//                txtYear.Text = QryYear.ToString();

//                var xData = dDB.QueryMultiple(@"
//select distinct FAC from ReportCategory where Report=@ReportID;
//select distinct Level01 from ReportCategory where Report=@ReportID;
//select distinct Level02 from ReportCategory where Report=@ReportID", new { ReportID });

//                if (xData != null)
//                {
//                    ddlFAC.DataSource = xData.Read<string>();
//                    ddlFAC.DataBind();
//                    ddlFAC.Items.Insert(0, string.Empty);

//                    ddlLevel01.DataSource = xData.Read<string>();
//                    ddlLevel01.DataBind();

//                    ddlLevel02.DataSource = xData.Read<string>();
//                    ddlLevel02.DataBind();
//                }
//            }
//        }

//        protected override void QueryCheck(int idx)
//        {
//            if (!int.TryParse(txtYear.Text, out int _))
//            { oResult.Error("[年度]格式錯誤，必須是整數"); }
//        }

//        protected override DataTable QuerySourceData(int idx)
//        {
//            return null;
//        }

//        protected override GridPageOption SetPageSetting()
//        {
//            return new GridPageOption() { GridOption = new GridOption[] { new GridOption { AutoBind = false, Grid = null, Pager = null, InfluxDB = null, Refresh = lbRefresh } } };
//        }

//        protected void btnExport_Click(object sender, EventArgs e) { }

//        protected void btnQuery_Click(object sender, EventArgs e)
//        {
//            if (int.TryParse(txtYear.Text.Trim(), out QryYear))
//            {
//                var xData = dDB.QueryMultiple($@"
//select ID as CategoryID,FAC,Level01,Level02,Level03,DataType,@Year as [Year] from ReportCategory
//where Report=@ReportID and Level01=@Level01 {(string.IsNullOrEmpty(ddlFAC.SelectedValue) ? string.Empty : "and FAC=@Fac")} and @Year between YearS and ISNULL(YearE,2099) order by SeqNo,ID
//;
//select A.ID as CategoryID,B.[Month],B.[ValueNum],B.[ValueStr] from ReportCategory A
//inner join CostReductionManagement B on A.ID=B.CategoryID
//where A.Report=@ReportID and A.Level01=@Level01 {(string.IsNullOrEmpty(ddlFAC.SelectedValue) ? string.Empty : "and A.FAC=@Fac")} and @Year between A.YearS and ISNULL(A.YearE,2099) and B.[Year]=@Year"
//, new { Year = QryYear, Fac = ddlFAC.SelectedValue, ReportID, Level01 = ddlLevel01.SelectedValue });

//                if (xData != null)
//                {
//                    var cList = xData.Read<GridValue>().ToList();
//                    var vList = xData.Read<TempValue>();

//                    if (cList.Any())
//                    {
//                        var gList = cList.GroupBy(p => p.DataType);

//                        if (gList.Count() == 2 && gList.Count(p => p.Key == "i") == 1)
//                        {
//                            foreach (var gData in gList)
//                            {
//                                if (gData.Key == "i")
//                                {
//                                    System.Threading.Tasks.Parallel.ForEach(gData, (cData) =>
//                                    {
//                                        var vData = vList.Where(p => p.CategoryID == cData.CategoryID && p.Month > 0 && p.Month < 13);
//                                        System.Threading.Tasks.Parallel.ForEach(vData, (v) => { cData.Values[v.Month - 1] = v.ValueNum; });
//                                        //foreach (var v in vData)
//                                        //{
//                                        //    switch (v.Month)
//                                        //    {
//                                        //        case 1:
//                                        //            cData.Month01 = v.ValueNum; break;
//                                        //        case 2:
//                                        //            cData.Month02 = v.ValueNum; break;
//                                        //        case 3:
//                                        //            cData.Month03 = v.ValueNum; break;
//                                        //        case 4:
//                                        //            cData.Month04 = v.ValueNum; break;
//                                        //        case 5:
//                                        //            cData.Month05 = v.ValueNum; break;
//                                        //        case 6:
//                                        //            cData.Month06 = v.ValueNum; break;
//                                        //        case 7:
//                                        //            cData.Month07 = v.ValueNum; break;
//                                        //        case 8:
//                                        //            cData.Month08 = v.ValueNum; break;
//                                        //        case 9:
//                                        //            cData.Month09 = v.ValueNum; break;
//                                        //        case 10:
//                                        //            cData.Month10 = v.ValueNum; break;
//                                        //        case 11:
//                                        //            cData.Month11 = v.ValueNum; break;
//                                        //        case 12:
//                                        //            cData.Month12 = v.ValueNum; break;
//                                        //        default:
//                                        //            break;
//                                        //    }
//                                        //}
//                                    });

//                                    var xList = gData.GroupBy(p => new { p.Level02, p.Level03 }).Select(p => new
//                                    {
//                                        p.Key.Level02,
//                                        p.Key.Level03,
//                                        DataList = p.ToList()
//                                    });

//                                    System.Text.StringBuilder oStr = new System.Text.StringBuilder(string.Empty);
//                                    foreach (var x in xList)
//                                    {
//                                        oStr.Append($@"
//<div class='div-category-type'><span>{x.Level02}</span> <span>{x.Level03}</span></div>
//<div class='fix-table'>
//<table class='MainGridView'>
//<thead>
//<tr><th style='width:5em;'>廠別</th><th>{QryYear}/01</th><th>{QryYear}/02</th><th>{QryYear}/03</th><th>{QryYear}/04</th><th>{QryYear}/05</th><th>{QryYear}/06</th><th>{QryYear}/07</th><th>{QryYear}/08</th><th>{QryYear}/09</th><th>{QryYear}/10</th><th>{QryYear}/11</th><th>{QryYear}/12</th></tr>
//</thead>
//<tbody>
//{(string.Join(string.Empty, x.DataList.Select(p => $"<tr data-key='{p.CategoryID}'><th>{p.FAC}</th>{GetRowData(p)}</tr>")))}
//</tbody>
//</table>
//</div>");

//                                        string GetRowData(GridValue v)
//                                        {
//                                            int iMonth = 1;
//                                            return string.Join(string.Empty, v.Values.Select(p => $"<td><input type='text' class='txt-input-number' value='{p}' data-old='{p}' data-idx='{iMonth++}' /></td>"));
//                                        }
//                                    }
//                                    ltlContent01.Text = oStr.ToString();

//                                    //DataList1.DataSource = xList;
//                                    //DataList1.DataBind();

//                                    //foreach (DataListItem oItem in DataList1.Items)
//                                    //{
//                                    //    GridView oGrid = (GridView)oItem.FindControl("GridView1");
//                                    //    if (oGrid != null)
//                                    //        for (int idx = 1; idx < 13; idx++)
//                                    //            oGrid.HeaderRow.Cells[idx].Text = $"{QryYear}/{idx:00}";
//                                    //}
//                                    ////GridView1.DataSource = gData;
//                                    ////GridView1.DataBind();

//                                    ////for (int idx = 1; idx < 13; idx++)
//                                    ////    GridView1.HeaderRow.Cells[idx + 2].Text = $"{QryYear}/{idx:00}";
//                                }
//                                else
//                                {
//                                    var xList = new List<GridValueS>();
//                                    foreach (var cData in gData)
//                                    {
//                                        var tList = new List<GridValueS>();
//                                        for (int idx = 1; idx < 13; idx++)
//                                            tList.Add(new GridValueS { CategoryID = cData.CategoryID, FAC = cData.FAC, Year = QryYear, Month = idx, Level02 = cData.Level02 });

//                                        System.Threading.Tasks.Parallel.ForEach(tList, (tData) => {
//                                            var vData = vList.FirstOrDefault(p => p.CategoryID == tData.CategoryID && p.Month == tData.Month);
//                                            if (vData != null) tData.ValueStr = vData.ValueStr;
//                                        });

//                                        xList.AddRange(tList);
//                                    }

//                                    GridView2.DataSource = xList;
//                                    GridView2.DataBind();

//                                    SetGridRowSpan(GridView2, 0);
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//            else
//                oResult.Error("[年度]格式錯誤，必須是整數");

//            if (!oResult.Success) ShowResult("");

//            //GridView1.Visible = oResult.Success;
//            //DataList1.Visible = oResult.Success;
//            GridView2.Visible = oResult.Success;
//        }

//        private void SetGridRowSpan(GridView oGrid, int iCell)
//        {
//            string TmpStr = string.Empty;
//            int TmpIndex = 0, TmpCount = 0;
//            foreach (GridViewRow Row in oGrid.Rows)
//            {
//                if (Row.RowType == DataControlRowType.DataRow)
//                {
//                    if (Row.Cells[iCell].Text != TmpStr)
//                    {
//                        TmpStr = Row.Cells[iCell].Text;
//                        if (TmpCount > 0)
//                        {
//                            oGrid.Rows[TmpIndex].Cells[iCell].Attributes.Add("rowspan", TmpCount.ToString());
//                            for (int idx = 1; idx < TmpCount; idx++)
//                            {
//                                oGrid.Rows[TmpIndex + idx].Cells[iCell].Visible = false;
//                            }
//                            TmpCount = 0;
//                        }
//                        TmpIndex = Row.RowIndex;
//                    }
//                    TmpCount++;
//                }
//            }
//            if (TmpCount > 0)
//            {
//                oGrid.Rows[TmpIndex].Cells[iCell].Attributes.Add("rowspan", TmpCount.ToString());
//                for (int idx = 1; idx < TmpCount; idx++)
//                {
//                    oGrid.Rows[TmpIndex + idx].Cells[iCell].Visible = false;
//                }
//            }
//        }

//        protected void btnUpdate_Click(object sender, EventArgs e)
//        {

//        }

//        class TempValue
//        {
//            public int CategoryID { get; set; }
//            public int Month { get; set; }
//            public decimal? ValueNum { get; set; }
//            public string ValueStr { get; set; } = string.Empty;
//        }

//        class GridValue
//        {
//            public int CategoryID { get; set; }
//            public string FAC { get; set; }
//            public string Level01 { get; set; }
//            public string Level02 { get; set; }
//            public string Level03 { get; set; }
//            //public int SeqNo { get; set; }
//            public string DataType { get; set; }
//            public int Year { get; set; }
//            public decimal?[] Values { get; set; } = new decimal?[12];
//        }

//        class GridValueS
//        {
//            public int CategoryID { get; set; }
//            public string FAC { get; set; }
//            public string Level02 { get; set; }
//            public int Year { get; set; }
//            public int Month { get; set; }
//            public string ValueStr { get; set; }
//        }
    }
}