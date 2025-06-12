using cyc.Page;
using NPOI.SS.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.DynamicData;
using System.Web.UI;
using System.Web.UI.WebControls;
using Inx.Data;
using System.IO;
using NPOI.HPSF;

namespace WebApp._report
{
    public abstract class InxCentralReportPage : cyc.Page.BasePage
    {
        protected int QryYear = DateTime.Today.Year;
        protected ReportOption Option { get; set; }      
        protected override string DefaultConntionString() => cyc.DB.ConnString.Report;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Option = SetReportOption();

            if (bUser == null)
            {
                oResult.Success = false;
                if (IsPostBack)
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alert", "OpenLogin();", true);
                else
                    Response.Redirect("~/login.aspx?rtn=" + Server.UrlEncode(Request.RawUrl));
                return;
            }
            else
            {
                if (Option != null)
                {
                    if (Option.Query != null) { Option.Query.Click += Query_Click; }
                }

                if (!IsPostBack && oResult.Success == true)
                {
                    Option.Year.Text = QryYear.ToString();

                    if (Option.Factory != null)
                    {
                        Option.Factory.DataSource = dDB.QueryList<string>("select distinct FAC from ReportCategory where ReportID=@ReportID and IsSUM=0 and IsAVG=0", new { Option.ReportID });
                        Option.Factory.DataBind();
                        Option.Factory.Items.Insert(0, string.Empty);
                    }
                    if (Option.Level01 != null)
                    {
                        Option.Level01.DataSource = dDB.QueryList<string>("select distinct Level01 from ReportCategory where ReportID=@ReportID", new { Option.ReportID });
                        Option.Level01.DataBind();
                    }
                }
            }
        }

        protected void Query_Click(object sender, EventArgs e)
        {
            if (!oResult.Success) return;

            if (int.TryParse(Option.Year.Text.Trim(), out QryYear))
            {
                var xData = dDB.QueryMultiple($@"
select ID as CategoryID,FAC,Level01,Level02,Level03,DataType,AddSUM,IsSUM,AddAVG,IsAVG,ExtSUM,ExtAVG,TitleSUM,TitleAVG,AutoData,DataDecimal from ReportCategory
where ReportID=@ReportID 
{(Option.Factory == null || string.IsNullOrEmpty(Option.Factory.SelectedValue) ? string.Empty : "and FAC=@Fac")} 
{(Option.Level01 == null || string.IsNullOrEmpty(Option.Level01.SelectedValue) ? string.Empty : "and Level01=@Level01")} 
and @Year between ISNULL(YearS,2000) and ISNULL(YearE,2099) order by SeqNo,ID
;
select A.ID as CategoryID,B.[Month],B.ValueNum,B.ValueStr from ReportCategory A
inner join {Option.TableName} B on A.ID=B.CategoryID
where A.ReportID=@ReportID 
{(Option.Factory == null || string.IsNullOrEmpty(Option.Factory.SelectedValue) ? string.Empty : "and A.FAC=@Fac")} 
{(Option.Level01 == null || string.IsNullOrEmpty(Option.Level01.SelectedValue) ? string.Empty : "and A.Level01=@Level01")} 
and @Year between ISNULL(A.YearS,2000) and ISNULL(A.YearE,2099) and B.[Year]=@Year"
, new { Year = QryYear, Fac = CheckFilterFac(), Option.ReportID, Level01 = CheckFilterLevel() });

                string CheckFilterFac()
                {
                    if (Option.Factory == null || string.IsNullOrEmpty(Option.Factory.SelectedValue))
                        return string.Empty;
                    else
                        return Option.Factory.SelectedValue;
                }

                string CheckFilterLevel()
                {
                    if (Option.Level01 == null || string.IsNullOrEmpty(Option.Level01.SelectedValue))
                        return string.Empty;
                    else
                        return Option.Level01.SelectedValue;
                }

                if (xData != null)
                {
                    var cList = xData.Read<GridValue>().ToList();
                    var vList = xData.Read<TempValue>();

                    if (cList.Any() && vList.Any())
                    {
                        System.Threading.Tasks.Parallel.ForEach(cList, (cData) =>
                        {
                            var vData = vList.Where(p => p.CategoryID == cData.CategoryID && p.Month > 0 && p.Month < 13);
                            var sData = vList.Where(p => p.CategoryID == cData.CategoryID && p.Month > 20 && p.Month < 23);
                            if (cData.DataType == "i")
                            {
                                System.Threading.Tasks.Parallel.ForEach(vData, (v) =>
                                {
                                    cData.ValueNum[v.Month - 1] = v.ValueNum;
                                });
                                if (cData.AddSUM) cData.ValueSUM = cData.ExtSUM ? sData.Where(p => p.Month == 21).Sum(p => p.ValueNum) : cData.ValueNum.Sum();
                                if (cData.AddAVG) cData.ValueAVG = cData.ExtAVG ? sData.Where(p => p.Month == 22).Average(p => p.ValueNum) : cData.ValueNum.Average();
                            }
                            else if (cData.DataType == "s")
                            {
                                System.Threading.Tasks.Parallel.ForEach(vData, (v) =>
                                {
                                    cData.ValueStr[v.Month - 1] = v.ValueStr;
                                });
                            }
                        });
                    }

                    CreateReportData(cList);
                }
            }
            else
                oResult.Error("[年度]格式錯誤，必須是整數");

            if (!oResult.Success) ShowResult("");

            Option.Update.Visible = oResult.Success;
            if (oResult.Success)
                Session[Option.TableName] = Guid.NewGuid().ToString("N");
            else
                Session[Option.TableName] = null;

            Option.Auth.Value = Session[Option.TableName]?.ToString();
        }

        protected abstract ReportOption SetReportOption();

        protected abstract void CreateReportData(List<GridValue> cList);

        protected static string CreateTableHeader(int iYear, bool showExt1 = false, bool showExt2 = false, string strTitle1 = null, string strTitle2 = null)
        {
            int[] m = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            return $"{(string.Join(string.Empty, m.Select(p => $"<th>{iYear}/{p:00}</th>")))}{(showExt1 ? $"<th>{strTitle1 ?? "總計"}</th>" : string.Empty)}{(showExt2 ? $"<th>{strTitle2 ?? "平均"}</th>" : string.Empty)}";
        }

        protected static string CreateTableHeader2(int iYear, GridValue xSUM = null, GridValue xAVG = null)
        {
            int[] m = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            return $"{(string.Join(string.Empty, m.Select(p => $"<th>{iYear}/{p:00}</th>")))}{(xSUM != null ? $"<th>{xSUM.TitleSUM ?? "總計"}</th>" : string.Empty)}{(xAVG != null ? $"<th>{xAVG.TitleAVG ?? "平均"}</th>" : string.Empty)}";
        }

        protected static string GetNumberDecimal(decimal? v, int d = 0)
        {
            return v == null ? string.Empty : ((decimal)v).ToString($"N{d}");
        }

        protected static string GetNumberEditRow(GridValue c)
        {
            int iMonth = 1;
            return $"{string.Join(string.Empty, c.ValueNum.Select(p => $"<td><input type='text' class='txt-input-number' value='{GetNumberDecimal(p, c.DataDecimal)}' data-old='{GetNumberDecimal(p, c.DataDecimal)}' data-idx='{iMonth++}' /></td>"))}";
            //return $"{string.Join(string.Empty, c.ValueNum.Select(p => $"<td><input type='text' name='txt_C{c.CategoryID}_M{iMonth}' class='txt-input-number' value='{p}' data-old='{p}' data-idx='{iMonth++}' /></td>"))}";
        }

        protected static string GetReadOnlyRow(GridValue c)
        {
            return $"{string.Join(string.Empty, c.ValueNum.Select(p => $"<td><input type='text' class='txt-input-readonly' readonly value='{GetNumberDecimal(p, c.DataDecimal)}' /></td>"))}";
        }

        protected static string GetAutoDataRow(GridValue v, GridValue x)
        {
            if (x != null)
            {
                for (int idx = 0; idx < x.ValueNum.Length; idx++)
                    v.ValueNum[idx] = x.ValueNum[idx] + (idx == 0 ? 0 : v.ValueNum[idx - 1]);
            }
            return GetReadOnlyRow(v);
        }

        protected static string GetAddSUM(GridValue c, bool showSUM)
        {
            string s = GetNumberDecimal(c.ValueSUM, c.DataDecimal);
            return $"{(showSUM ? $"<td>{(c.AddSUM ? $"<input type='text' {(c.ExtSUM && !c.IsSUM ? $"class='txt-input-number' data-old='{s}' data-idx='21'" : "class='txt-input-readonly' readonly")} value='{s}' />" : string.Empty)}</td>" : string.Empty)}";
        }

        protected static string GetAddAVG(GridValue c, bool showAVG)
        {
            string s = GetNumberDecimal(c.ValueAVG, c.DataDecimal);
            return $"{(showAVG ? $"<td>{(c.AddAVG ? $"<input type='text' {(c.ExtAVG && !c.IsAVG ? $"class='txt-input-number' data-old='{s}' data-idx='22'" : "class='txt-input-readonly' readonly")} value='{s}' />" : string.Empty)}</td>" : string.Empty)}";
        }

        protected static void CreateSummaryData(GridValue c, IEnumerable<GridValue> cList)
        {
            if (c.IsSUM)
                for (int i = 0; i < 12; i++) { c.ValueNum[i] = cList.Select(p => p.ValueNum[i]).Sum(); }
            else
                for (int i = 0; i < 12; i++) { c.ValueNum[i] = cList.Select(p => p.ValueNum[i]).Average(); }

            c.ValueSUM = c.ValueNum.Sum();
            c.ValueAVG = c.ValueNum.Average();
        }

    }

    public class TempValue
    {
        public int CategoryID { get; set; }
        public int Month { get; set; }
        public decimal? ValueNum { get; set; }
        public string ValueStr { get; set; }
    }
}