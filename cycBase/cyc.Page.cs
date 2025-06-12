using System;
using System.Web.UI;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using System.Data;
using System.Linq;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.HSSF.UserModel;
using cyc.UC;
using cyc.DB;
using cyc.Data;
using Quartz;

namespace cyc.Page
{
    #region < BasePage >
    /// <summary>
    /// 基礎頁面，含DB Connect
    /// </summary>
    public class BasePage : System.Web.UI.Page
    {
        protected ExeResult oResult = new ExeResult();
        protected UserInfo bUser = null;//使用者資訊
        private SqlDapperConn _dDB = null;
        protected SqlDBPara bPara = new SqlDBPara();
        protected SqlDapperConn dDB { get { if (_dDB == null) { _dDB = new SqlDapperConn(oResult, DefaultConntionString()); } return _dDB; } }

        protected virtual string DefaultConntionString() => cyc.DB.ConnString.Main;

        #region = PageEvent =
        protected override void OnInit(EventArgs e)
        {
            if (Session["uid"] != null) { bUser = (UserInfo)Session["uid"]; }
            bPara.Result = oResult;
            base.OnInit(e);
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }
        protected override void OnUnload(EventArgs e)
        {
            BasePageFunc.ClosePageConnect(_dDB);
            base.OnUnload(e);
        }
        protected override void OnError(EventArgs e)
        {
            BasePageFunc.ClosePageConnect(_dDB);
            base.OnError(e);
        }
        #endregion

        #region = MessageBox =
        protected void MsgBox(string sMsg, string sURL = "", bool bReload = false, bool bReturn = false)
        {
            sMsg = sMsg.Replace("'", "\'").Replace(Environment.NewLine, " \n");
            sMsg = string.Format("ClientAlert('{0}');", sMsg);
            if (sURL.Length > 0)
                sMsg += "document.location.href='" + sURL + "';";

            if (bReload && bReturn)
                sMsg += "parent.CloseAndReload(1,1);window.stop ? window.stop() : document.execCommand('Stop');";
            else
            {
                if (bReload)
                    sMsg += "parent.CloseAndReload(0,1);";
                if (bReturn)
                    sMsg += "parent.jQuery.fancybox.close();";
            }

            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alert", sMsg, true);
        }
        protected virtual void ShowResult(string sMsg, bool bReload = false, bool bReturn = false, string sUrl = "")
        {
            if (oResult.Success)
                MsgBox(sMsg, sUrl, bReload, bReturn);
            else
                MsgBox(oResult.Message);
        }
        #endregion
    }
    #endregion

    #region < BasePageGrid >
    /// <summary>
    /// 基礎 資料查詢清單 頁面(多清單)
    /// </summary>
    public abstract class BasePageGrid : BasePage, IPageGrid
    {
        protected GridPageOption PageOption;
        protected abstract DataTable QuerySourceData(int idx);//取得GridView資料來源
        protected abstract GridPageOption SetPageSetting();

        protected virtual ExportOption GetExportOption(int idx)
        {
            return null;
        }

        /// <summary>
        /// 檢查查詢條件，可複寫
        /// </summary>
        protected virtual void QueryCheck(int idx) { }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            PageOption = SetPageSetting();
            if (PageOption != null)
            {
                if (PageOption.CheckSession && Session["uid"] == null)
                {
                    oResult.Success = false;
                    if (IsPostBack)
                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alert", "OpenLogin();", true);
                    else
                        Response.Redirect("~/login.aspx?rtn=" + Server.UrlEncode(Request.RawUrl));
                    return;
                }

                if (!IsPostBack && PageOption.CheckOpen.Length > 0 && System.IO.Path.GetFileName(Request.PhysicalPath) != PageOption.CheckOpen)
                    Response.End();

                if (PageOption.GridOption != null)
                {
                    BasePageFunc.BindGridPageEvent(PageOption.GridOption, this);
                    //foreach (GridOption opt in PageOption.GridOption)
                    //{
                    //    if (opt.Grid != null) { opt.Grid.DataBound += Grid_DataBound; opt.Grid.Sorting += Grid_Sorting; }
                    //    if (opt.Pager != null) { opt.Pager.PageChanged += Pager_PageChanged; }
                    //    if (opt.Query != null) { opt.Query.Click += Query_Click; }
                    //    if (opt.Excel != null) { opt.Excel.Click += Excel_Click; }
                    //}
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                BasePageFunc.AutoBindGrid(PageOption.GridOption, BindGridView);
            }
        }

        #region Event
        void IPageGrid.Grid_DataBound(object sender, EventArgs e)
        {
            BasePageFunc.Grid_DataBound(PageOption.GridOption, (GridView)sender);
        }

        void IPageGrid.Grid_Sorting(object sender, GridViewSortEventArgs e)
        {
            BasePageFunc.Grid_Sorting(PageOption.GridOption, (GridView)sender, e, BindGridView);
        }

        void IPageGrid.Pager_PageChanged(object sender, PagerChangeArgs e)
        {
            BasePageFunc.Pager_PageChanged(PageOption.GridOption, (ucPager)sender, e, BindGridView);
        }

        void IPageGrid.Query_Click(object sender, EventArgs e)
        {
            BasePageFunc.Query_Click(PageOption.GridOption, (Button)sender, BindGridView);
        }

        void IPageGrid.Excel_Click(object sender, EventArgs e)
        {
            BasePageFunc.Excel_Click(PageOption.GridOption, (Button)sender, CreateExcel);
        }
        ///// <summary>
        ///// GridView生成後觸發
        ///// </summary>
        //private void Grid_DataBound(object sender, EventArgs e)
        //{
        //    BasePageFunc.Grid_DataBound(PageOption.GridOption, (GridView)sender);
        //}
        ///// <summary>
        ///// GridView排序觸發
        ///// </summary>
        //private void Grid_Sorting(object sender, GridViewSortEventArgs e)
        //{
        //    BasePageFunc.Grid_Sorting(PageOption.GridOption, (GridView)sender, e, BindGridView);
        //}
        ///// <summary>
        ///// Pager換頁事件觸發
        ///// </summary>
        //private void Pager_PageChanged(object sender, PagerChangeArgs e)
        //{
        //    BasePageFunc.Pager_PageChanged(PageOption.GridOption, (ucPager)sender, e, BindGridView);
        //}
        ///// <summary>
        ///// 查詢按鍵用
        ///// </summary>
        //private void Query_Click(object sender, EventArgs e)
        //{
        //    BasePageFunc.Query_Click(PageOption.GridOption, (Button)sender, BindGridView);
        //}
        ///// <summary>
        ///// 匯出EXCEL用
        ///// </summary>
        //private void Excel_Click(object sender, EventArgs e)
        //{
        //    BasePageFunc.Excel_Click(PageOption.GridOption, (Button)sender, CreateExcel);
        //}
        #endregion

        /// <summary>
        /// 取得及設定排序欄位
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="column">排序欄位</param>
        /// <returns></returns>
        protected string GetSort(int idx, string column = null)
        {
            return BasePageFunc.GridGetSort(ViewState, idx, column);
        }

        /// <summary>
        /// 綁定 GridView 
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="sSort"></param>
        protected virtual void BindGridView(int idx = 0, string sSort = null)
        {
            QueryCheck(idx);
            if (oResult.Success)
            {
                try
                {
                    BasePageFunc.PageBindGrid(PageOption.GridOption[idx], idx, GetSort(idx, sSort), QuerySourceData);
                }
                catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex, oResult); }
            }
            if (!oResult.Success) { MsgBox(oResult.Message); }
        }

        /// <summary>
        /// 產生 Excel 檔案
        /// </summary>
        /// <param name="idx"></param>
        protected void CreateExcel(int idx)
        {
            BasePageFunc.CreateExcel(Response, idx, GetSort(idx), GetExportOption(idx), QuerySourceData);
        }
    }
    #endregion

    #region < BasePageSub >
    public abstract class BasePageSub : BasePage, IPageGrid
    {
        protected SubPageOption PageOption { get; set; }
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            PageOption = SetPageOption();

            if (PageOption.Session && Session["uid"] == null)
            {
                oResult.Success = false;
                if (IsPostBack)
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alert", "OpenLogin();", true);
                else
                    Response.Redirect("~/loginOpen.aspx?rtn=" + Server.UrlEncode(Request.RawUrl));
                return;
            }

            if (!IsPostBack && !string.IsNullOrEmpty(PageOption.CheckOpen) && !PageOption.CheckOpen.Split(';').Contains(System.IO.Path.GetFileName(Request.PhysicalPath)))
                oResult.Error("參數錯誤");

            if (oResult.Success)
                BasePageFunc.CheckSubPageValid(PageOption, this, oResult);

            if (!oResult.Success)
            { 
                Session["invalid"] = oResult.Message;
                Response.Redirect("~/invalid.aspx");
            }

            if (PageOption.Confirm != null) { PageOption.Confirm.Click += Confirm_Click; }
            if (PageOption.GridOption != null)
            {
                BasePageFunc.BindGridPageEvent(PageOption.GridOption, this);
                //foreach (var opt in PageOption.GridOption)
                //{
                //    if (opt.Grid != null) { opt.Grid.DataBound += Grid_DataBound; opt.Grid.Sorting += Grid_Sorting; }
                //    if (opt.Pager != null) { opt.Pager.PageChanged += Pager_PageChanged; }
                //    if (opt.Query != null) { opt.Query.Click += Query_Click; }
                //    if (opt.Excel != null) { opt.Excel.Click += Excel_Click; }
                //}
            }
        }
        protected override void OnLoad(EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                if (oResult.Success) { LoadCheck(); }
                if (oResult.Success) { LoadData(); }
                if (!oResult.Success) { Session["invalid"] = oResult.Message; Response.Redirect("~/invalid.aspx"); }

                if (PageOption.GridOption != null) { BasePageFunc.AutoBindGrid(PageOption.GridOption, BindGridView); }
            }
            base.OnLoad(e);
        }

        #region -事件-
        private void Confirm_Click(object sender, EventArgs e)
        {
            //檢查資料正確性
            if (!Page.IsValid) { oResult.Success = false; return; }
            SaveCheck();
            //儲存資料
            if (oResult.Success) { SaveData(); }

            //ShowResult(PageOption.SuccessMsg, true, true);
            ShowResult(PageOption.SuccessMsg, true, PageOption.CloseWhenSuccess);
        }
        //private void Grid_DataBound(object sender, EventArgs e)
        //{
        //    BasePageFunc.Grid_DataBound(PageOption.GridOption, (GridView)sender);
        //}
        //private void Grid_Sorting(object sender, GridViewSortEventArgs e)
        //{
        //    BasePageFunc.Grid_Sorting(PageOption.GridOption, (GridView)sender, e, BindGridView);
        //}
        //private void Pager_PageChanged(object sender, PagerChangeArgs e)
        //{
        //    BasePageFunc.Pager_PageChanged(PageOption.GridOption, (ucPager)sender, e, BindGridView);
        //}
        //private void Query_Click(object sender, EventArgs e)
        //{
        //    BasePageFunc.Query_Click(PageOption.GridOption, (Button)sender, BindGridView);
        //}
        //private void Excel_Click(object sender, EventArgs e)
        //{
        //    BasePageFunc.Excel_Click(PageOption.GridOption, (Button)sender, CreateExcel);
        //}
        void IPageGrid.Grid_DataBound(object sender, EventArgs e)
        {
            BasePageFunc.Grid_DataBound(PageOption.GridOption, (GridView)sender);
        }
        void IPageGrid.Grid_Sorting(object sender, GridViewSortEventArgs e)
        {
            BasePageFunc.Grid_Sorting(PageOption.GridOption, (GridView)sender, e, BindGridView);
        }
        void IPageGrid.Pager_PageChanged(object sender, PagerChangeArgs e)
        {
            BasePageFunc.Pager_PageChanged(PageOption.GridOption, (ucPager)sender, e, BindGridView);
        }
        void IPageGrid.Query_Click(object sender, EventArgs e)
        {
            BasePageFunc.Query_Click(PageOption.GridOption, (Button)sender, BindGridView);
        }
        void IPageGrid.Excel_Click(object sender, EventArgs e)
        {
            BasePageFunc.Excel_Click(PageOption.GridOption, (Button)sender, CreateExcel);
        }
        #endregion

        #region --Function--
        protected abstract SubPageOption SetPageOption();
        protected virtual ExportOption GetExportOption(int idx) { return null; }
        protected virtual void LoadData() { }
        protected virtual void SaveData() { }
        protected virtual void LoadCheck() { }
        protected virtual void SaveCheck() { }
        protected virtual DataTable QuerySourceData(int idx) { return null; }
        protected virtual void QueryCheck(int idx) { }
        protected string GetSort(int idx, string column = null)
        {
            return BasePageFunc.GridGetSort(ViewState, idx, column);
        }
        protected void BindGridView(int idx = 0, string sSort = null)
        {
            QueryCheck(idx);
            if (oResult.Success)
            {
                try
                {
                    BasePageFunc.PageBindGrid(PageOption.GridOption[idx], idx, GetSort(idx, sSort), QuerySourceData);
                }
                catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex, oResult); }
            }
            if (!oResult.Success) { MsgBox(oResult.Message); }
        }
        protected void CreateExcel(int idx)
        {
            BasePageFunc.CreateExcel(Response, idx, GetSort(idx), GetExportOption(idx), QuerySourceData);
        }

        private bool CheckAppSub()
        {
            bool IsOK = true;
            if (PageOption.AppID != null && PageOption.SubID != null && PageOption.AppID.Length > 0 && PageOption.SubID.Length > 0 && int.TryParse(Request.QueryString["app"], out int iApp) && int.TryParse(Request.QueryString["sub"], out int iSub))
                IsOK = PageOption.AppID.Contains(iApp) && PageOption.SubID.Contains(iSub);
            return IsOK;
        }
        #endregion
    }
    #endregion

    #region < 共用Function >
    /// <summary>
    /// BasePage 共用Function
    /// </summary>
    public class BasePageFunc
    {
        public delegate void delBindGridView(int idx, string sSort = null);
        public delegate void delCreateExcel(int idx);
        public delegate DataTable delGetDataSource(int idx);

        internal static void Query_Click(GridOption[] Options, Button sender, delBindGridView bindGrid)
        {
            var opt = Options.FirstOrDefault(p => p.Query != null && p.Query.ID == sender.ID);
            bindGrid(Array.IndexOf(Options, opt));
        }
        internal static void Grid_Sorting(GridOption[] Options, GridView sender, GridViewSortEventArgs e, delBindGridView bindGrid)
        {
            var opt = Options.FirstOrDefault(p => p.Grid != null && p.Grid.ID == sender.ID);
            bindGrid(Array.IndexOf(Options, opt), e.SortExpression);
        }
        internal static void Grid_DataBound(GridOption[] Options, GridView sender)
        {
            var opt = Options.FirstOrDefault(p => p.Pager != null && p.Pager.TargetID == sender.ID);
            opt?.Pager.Refresh();
            sender.HeaderRow.TableSection = TableRowSection.TableHeader;
        }
        internal static void Pager_PageChanged(GridOption[] Options, ucPager sender, PagerChangeArgs e, delBindGridView bindGrid)
        {
            var opt = Options.FirstOrDefault(p => p.Pager != null && p.Pager.ID == sender.ID && p.Grid != null);
            if (opt != null)
            {
                opt.Grid.PageIndex = e.CurrentPage - 1;
                opt.Grid.PageSize = e.PageSize;
                bindGrid(Array.IndexOf(Options, opt));
            }
        }
        internal static void Excel_Click(GridOption[] Options, Button sender, delCreateExcel createExcel)
        {
            var opt = Options.FirstOrDefault(p => p.Excel != null && p.Excel.ID == sender.ID);
            if (opt != null) { createExcel(Array.IndexOf(Options, opt)); }
        }

        internal static void BindGridPageEvent(GridOption[] oOptins, IPageGrid Page)
        {
            System.Threading.Tasks.Parallel.ForEach(oOptins, (opt) => {
                if (opt.Grid != null)
                {
                    opt.Grid.DataBound += Page.Grid_DataBound;
                    opt.Grid.Sorting += Page.Grid_Sorting;
                    if (opt.Pager != null) { opt.Pager.PageChanged += Page.Pager_PageChanged; }
                }
                if (opt.Query != null) { opt.Query.Click += Page.Query_Click; }
                if (opt.Excel != null) { opt.Excel.Click += Page.Excel_Click; }
            });
        }
        internal static void AutoBindGrid(GridOption[] Options, delBindGridView bindGrid)
        {
            if (Options != null)
            {
                foreach (GridOption opt in Options)
                    if (opt.AutoBind && opt.Grid != null)
                        bindGrid(Array.IndexOf(Options, opt));
            }
        }
        internal static void PageBindGrid(GridOption oOpt, int idx, string sSort, delGetDataSource getData)
        {
            try
            {
                using (DataTable oDT = getData(idx))
                {
                    if (oDT != null)
                    {
                        oDT.DefaultView.Sort = sSort;
                        oOpt.Grid.DataSource = oDT;
                        oOpt.Grid.DataBind();
                        oOpt.Pager?.showTotalCnt(oDT.Rows.Count);
                    }
                }
            }
            catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex.Message + ":" + ex.StackTrace); }
        }
        internal static string GridGetSort(StateBag state, int idx, string column)
        {
            string[] sDir = { "ASC", "DESC" };
            string sDirection = $"SortDir{idx}";
            string sExpression = $"SortExp{idx}";

            if (state[sExpression] == null && column == null)
                return "";
            else if (column != null)
            {
                if (state[sExpression] != null && state[sExpression].ToString() == column)
                    state[sDirection] = (state[sDirection] == null || state[sDirection].ToString() == sDir[1]) ? sDir[0] : sDir[1];
                else
                    state[sExpression] = column;
            }
            if (state[sDirection] == null) { state[sDirection] = sDir[0]; }
            return string.Format("{0} {1}", state[sExpression], state[sDirection]);
        }
        internal static void CreateExcel(System.Web.HttpResponse response, int idx, string sSort, ExportOption export, delGetDataSource getData)
        {
            try
            {
                using (DataTable oDT = getData(idx))
                {
                    if (oDT != null)
                    {
                        oDT.DefaultView.Sort = sSort;
                        using (ExportExcel excel = new ExportExcel(export, oDT.DefaultView))
                        {
                            if (excel.Workbook != null)
                            {
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    excel.Workbook.Write(ms);
                                    response.Clear();
                                    response.AddHeader("Content-Disposition", "attachment;filename=" + System.Web.HttpUtility.UrlEncode((excel.Option.FileName == "" ? "匯出清冊" : excel.Option.FileName) + ".xls"));
                                    response.ContentType = "application/octet-stream";
                                    response.OutputStream.Write(ms.GetBuffer(), 0, ms.GetBuffer().Length);
                                    response.OutputStream.Flush();
                                    response.OutputStream.Close();
                                    ms.Close();
                                }
                                response.Flush();
                                response.End();
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex.Message + ":" + ex.StackTrace); }
        }


        internal static void CheckSubPageValid(SubPageOption oOption, System.Web.UI.Page oPage, ExeResult oResult)
        {
            if (oResult.Success && (oOption.Session || !string.IsNullOrEmpty(oOption.Parameter) || oOption.Parent || oOption.Opener))
            {
                if (oResult.Success && oOption.Session)
                {
                    if (!cyc.Login.CheckSession()) oResult.Error("未登入或逾時登出");
                }
                if (oResult.Success && oOption.Parent)
                {
                    if (oPage.Request.UrlReferrer != null)
                    {
                        if (oPage.Request.Url.Host != oPage.Request.UrlReferrer.Host) { oResult.Error("來源頁有誤"); }
                    }
                    else { oResult.Error("來源頁有誤"); }
                }
                if (oResult.Success && oOption.Opener)
                {
                    if (oPage.PreviousPage != null && oPage.PreviousPage is System.Web.UI.Page)
                    {
                        if (oPage.Request.Url.Host != oPage.PreviousPage.Request.Url.Host) { oResult.Error("來源頁有誤"); }
                    }
                    else { oResult.Error("來源頁有誤"); }
                }
                if (oResult.Success && !string.IsNullOrEmpty(oOption.Parameter))
                {
                    string[] sPara = oOption.Parameter.Split(',');
                    foreach (string pa in sPara)
                        if (string.IsNullOrEmpty(oPage.Request.QueryString[pa]))
                        { oResult.Error("參數錯誤"); break; }

                    if (oResult.Success && oOption.IsIntPa && !string.IsNullOrEmpty(oPage.Request.QueryString["pa"]) && !cyc.Shared.Check.IsInteger(oPage.Request.QueryString["pa"]))
                        oResult.Error("參數錯誤");
                }
            }
        }
        public static string ReCheckAuth(string sKey, string sGuid, ExeResult oResult)
        {
            if (sKey.Length == 0 || sKey != sGuid) { oResult.Error("認證失敗"); }
            return "";
        }
        internal static void ClosePageConnect(SqlDapperConn dDB)
        {
            if (dDB != null) dDB.Dispose();//關閉頁面連線
        }
    }
    #endregion

    #region < EXCEL匯出執行 >
    /// <summary>
    /// DataView 匯出 Excel
    /// </summary>
    public class ExportExcel : IDisposable
    {
        public ExportOption Option { get; set; }
        public HSSFWorkbook Workbook { get; set; }
        public ExportExcel(ExportOption opt, DataView oDV)
        {
            Option = opt;
            if (Option != null && Option.ColType.Length > 0 && Option.Column.Length > 0 && Option.Mapping.Length > 0 && Option.ColType.Length == Option.Column.Length && Option.ColType.Length == Option.Mapping.Length)
            {
                Workbook = new HSSFWorkbook();
                ISheet sheet1 = Workbook.CreateSheet(Option.FileName == "" ? "sheet1" : Option.FileName);

                int idxRow = 0;
                IRow header = sheet1.CreateRow(idxRow);
                for (int idx = 0; idx < Option.Mapping.Length; idx++)
                    header.CreateCell(idx).SetCellValue(Option.Mapping[idx]);
                idxRow++;

                for (int idx = 0; idx < oDV.Count; idx++)
                {
                    IRow datarow = sheet1.CreateRow(idxRow);
                    for (int idx2 = 0; idx2 < Option.Mapping.Length; idx2++)
                    {
                        switch (Option.ColType[idx2])
                        {
                            case "s"://文字
                                datarow.CreateCell(idx2).SetCellValue(oDV[idx][Option.Column[idx2]].ToString());
                                break;
                            case "i"://數字
                                datarow.CreateCell(idx2).SetCellValue(Convert.ToInt32(oDV[idx][Option.Column[idx2]]));
                                break;
                            case "t"://日期
                                if (oDV[idx][Option.Column[idx2]].ToString() != "")
                                    datarow.CreateCell(idx2).SetCellValue(Convert.ToDateTime(oDV[idx][Option.Column[idx2]]).ToString("yyyy/MM/dd"));
                                break;
                            case "p"://百分比
                                datarow.CreateCell(idx2).SetCellValue(string.Format("{0:P2}", Convert.ToDouble(oDV[idx][Option.Column[idx2]])));
                                break;
                            case "y"://是否
                                if (Convert.ToBoolean(oDV[idx][Option.Column[idx2]]))
                                    datarow.CreateCell(idx2).SetCellValue("是");
                                break;
                            default://文字
                                datarow.CreateCell(idx2).SetCellValue(oDV[idx][Option.Column[idx2]].ToString());
                                break;
                        }
                    }
                    idxRow++;
                }
                //for (int idx = 0; idx < Option.Mapping.Length; idx++)
                //    sheet1.AutoSizeColumn(idx, true);
            }
        }
        public void Dispose()
        {
            if (Workbook != null) { Workbook = null; }
        }
    }
    #endregion

    #region < 類別定義 >
    public interface IPageGrid
    {
        void Grid_DataBound(object sender, EventArgs e);
        void Grid_Sorting(object sender, GridViewSortEventArgs e);
        void Pager_PageChanged(object sender, cyc.UC.PagerChangeArgs e);
        void Query_Click(object sender, EventArgs e);
        void Excel_Click(object sender, EventArgs e);
    }

    public class ExportOption
    {
        public string[] Mapping { get; set; }
        public string[] Column { get; set; }
        public string[] ColType { get; set; }
        public string FileName { get; set; }
    }

    /// <summary>
    /// GridView 頁面 設定
    /// </summary>
    public class GridPageOption
    {
        public bool CheckSession { get; set; }
        public bool CheckGuid { get; set; }
        public string CheckOpen { get; set; }
        public GridOption[] GridOption { get; set; }
        public GridPageOption() { CheckSession = true; CheckGuid = false; CheckOpen = "index.aspx"; }
    }

    /// <summary>
    /// GridView相對應控制項
    /// </summary>
    public class GridOption
    {
        public GridView Grid { get; set; }
        public Button Excel { get; set; }
        public ucPager Pager { get; set; }
        public Button Query { get; set; }
        //public LinkButton Refresh { get; set; }
        public bool AutoBind { get; set; }
        public GridOption() { AutoBind = false; }
    }

    public class SubPageOption
    {
        public Button Confirm { get; set; }//[確認]按鍵
        public Button Delete { get; set; }//[刪除]按鍵
        public string SuccessMsg { get; set; } = "更新完成";//執行成功訊息
        public bool Session { get; set; } = true;//檢查Session
        public bool Opener { get; set; } = false;//檢查Opener
        public bool Parent { get; set; } = true;//檢查上層視窗
        public string CheckOpen { get; set; } = "open.aspx"; //是否檢查直接輸入網址進入
        public string Parameter { get; set; } = string.Empty; //檢查必要參數
        public bool IsIntPa { get; set; } = true;//'pa'參數是否為int，預設=是
        public GridOption[] GridOption { get; set; }
        public bool CloseWhenSuccess { get; set; } = true;//執行成功後自動關閉

        public int[] SubID { get; set; }
        public int[] AppID { get; set; }
    }
    #endregion

    #region < OLD BasePageEdit 停用 >
    /// <summary>
    /// 基礎 編輯資料 頁面
    /// </summary>
    //public abstract class BasePageEdit : BasePage
    //{
    //    protected abstract EditPageOption PageOption { get; }

    //    //protected EditPageOption PageOption;
    //    //protected abstract EditPageOption SetEditOption();

    //    /// <summary>
    //    /// 設定更新完成訊息
    //    /// </summary>
    //    protected string SuccessMessage { get; set; }

    //    /// <summary>
    //    /// 設定紀錄檔資訊
    //    /// </summary>
    //    protected string LogMessage { get; set; }

    //    /// <summary>
    //    /// 將資料載入頁面
    //    /// </summary>
    //    protected abstract void LoadData();
    //    /// <summary>
    //    /// 儲存輸入資料
    //    /// </summary>
    //    protected abstract void SaveData();
    //    /// <summary>
    //    /// 資料載入前檢核
    //    /// </summary>
    //    protected virtual void LoadCheck() { }
    //    /// <summary>
    //    /// 資料儲存前檢核
    //    /// </summary>
    //    protected virtual void SaveCheck() { }

    //    protected override void OnInit(EventArgs e)
    //    {
    //        base.OnInit(e);

    //        if (!IsPostBack && PageOption.CheckOpen.Length > 0 && System.IO.Path.GetFileName(Request.PhysicalPath) != PageOption.CheckOpen)
    //            oResult.Error("參數錯誤");

    //        if (oResult.Success) { BasePageFunc.CheckEditValid(PageOption, this, oResult); }

    //        if (!oResult.Success) { Session["invalid"] = oResult.Message; Response.Redirect("~/invalid.aspx"); }

    //        if (PageOption.Confirm != null) { PageOption.Confirm.Click += Confirm_Click; }
    //    }

    //    protected override void OnLoad(EventArgs e)
    //    {
    //        if (!Page.IsPostBack)
    //        {
    //            if (oResult.Success) { this.LoadCheck(); }
    //            if (oResult.Success) { this.LoadData(); }
    //            if (!oResult.Success) { Session["invalid"] = oResult.Message; Response.Redirect("~/invalid.aspx"); }
    //        }
    //        base.OnLoad(e);
    //    }

    //    /// <summary>
    //    /// [確定]按鍵用
    //    /// </summary>
    //    private void Confirm_Click(object sender, EventArgs e)
    //    {
    //        //檢查資料正確性
    //        if (!Page.IsValid) { oResult.Success = false; return; }
    //        this.SaveCheck();
    //        //儲存資料
    //        if (oResult.Success) { this.SaveData(); }

    //        this.ShowResult((SuccessMessage == null ? PageOption.SuccessMsg : SuccessMessage), true, true);
    //    }
    //}

    /// <summary>
    /// 資料編輯頁面 設定選項
    /// </summary>
    //public class EditPageOption
    //{
    //    public Button Confirm { get; set; }//[確認]按鍵
    //    public Button Delete { get; set; }//[刪除]按鍵
    //    public string SuccessMsg { get; set; }//執行成功訊息
    //    public bool Session { get; set; }//檢查Session
    //    public bool Opener { get; set; }//檢查Opener
    //    public bool Parent { get; set; }//檢查上層視窗
    //    public string CheckOpen { get; set; }//是否檢查直接輸入網址進入
    //    public bool Guid { get; set; }//
    //    public string Parameter { get; set; }//檢查必要參數
    //    public bool IsIntPa { get; set; }//'pa'參數是否為int，預設=是
    //    public bool CloseWhenSuccess { get; set; }//作業完成後自動關閉
    //    public bool ReCheckAuth { get; set; }//是否檢查重新登入
    //    public bool IsWriteExecLog { get; set; }//是否寫入操作紀錄
    //    public EditPageOption() { SuccessMsg = "更新完成"; Session = false; Opener = false; Parent = false; Guid = false; Parameter = ""; IsIntPa = true; CloseWhenSuccess = true; CheckOpen = "open.aspx"; }
    //}
    #endregion

    #region < 其它頁面 >
    /// <summary>
    /// 基礎導向頁面
    /// </summary>
    public class BaseNavi : System.Web.UI.Page
    {
        protected override void OnInit(EventArgs e)
        {
            if (!string.IsNullOrEmpty(Request.QueryString["App"]) && int.TryParse(Request.QueryString["App"], out int iApp))
            {
                if (Session["uid"] != null)
                {
                    UserInfo oUser = (UserInfo)Session["uid"];

                    var m = (from lsP in Global.SysProg.List.Where(p => p.ID == iApp && p.Enabled)
                             join lsRP in Global.SysRoleProg.List on lsP.ID equals lsRP.ProgID
                             join lsR in Global.SysRole.List.Where(p => p.Enabled) on lsRP.RoleID equals lsR.ID
                             join lsU in oUser.Role on lsR.ID equals lsU
                             select lsP).FirstOrDefault();

                    if (m != null) { Server.Transfer(string.Format("~/{0}/{1}", m.Folder, m.Path)); }
                }
                else
                {
                    Response.Redirect("~/login.aspx?rtn=" + Server.UrlEncode(Request.RawUrl)); return;
                }
            }
            base.OnInit(e);
        }
    }

    /// <summary>
    /// 基礎導向子頁面
    /// </summary>
    public class BaseOpen : System.Web.UI.Page
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (Session["uid"] == null)
            {
                Response.Redirect("~/loginOpen.aspx?rtn=" + Server.UrlEncode(Request.RawUrl));
            }
            else if (!string.IsNullOrEmpty(Request.QueryString["app"]) && !string.IsNullOrEmpty(Request.QueryString["sub"]) && int.TryParse(Request.QueryString["app"], out int iApp) && int.TryParse(Request.QueryString["sub"], out int iSub))
            {
                cyc.Data.UserInfo bUser = (cyc.Data.UserInfo)Session["uid"];

                var qList = from lsU in bUser.Role
                            join lsX in cyc.Global.SysRole.List.Where(p => p.Enabled) on lsU equals lsX.ID
                            join lsRS in cyc.Global.SysRoleProg.List on lsU equals lsRS.RoleID
                            join lsPS in cyc.Global.SysProgSub.List on lsRS.ProgID equals lsPS.UpperID
                            where lsPS.UpperID == iApp && lsPS.ID == iSub
                            select new { RoleID = lsU, isAll = lsRS.isAllSub, lsPS.Path };

                if (qList.Count() > 0)
                {
                    var q = qList.FirstOrDefault(p => p.isAll);
                    if (q != null) { Server.Transfer(q.Path); }

                    var s = (from lsQ in qList
                             join lsS in cyc.Global.SysRoleProgSub.List.Where(p => p.ProgID == iApp && p.SubID == iSub) on lsQ.RoleID equals lsS.RoleID
                             select lsQ).FirstOrDefault();
                    if (s != null) { Server.Transfer(s.Path); }
                }
            }

            Session["invalid"] = "參數錯誤";
            Response.Redirect("~/invalid.aspx");
        }
    }

    /// <summary>
    /// 基礎處理常式
    /// </summary>
    public abstract class BaseHandler : System.Web.IHttpHandler, System.Web.SessionState.IReadOnlySessionState
    {
        protected BaseHandlerOption oOption;
        protected abstract void DoHandler(System.Web.HttpContext context);
        protected abstract BaseHandlerOption SetBaseOption();
        protected ExeResult oResult = new ExeResult();
        protected UserInfo oUser;
        SqlDapperConn _dDB;
        public SqlDapperConn dDB { get { if (_dDB == null) { _dDB = new SqlDapperConn(oResult); } return _dDB; } }

        /// <summary>
        /// 處理常式 主流程
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(System.Web.HttpContext context)
        {
            //取得 設定項目
            oOption = SetBaseOption();
            if (oOption == null) { oOption = new BaseHandlerOption(); }

            //檢核
            this.CheckValid(context);
            if (oResult.Success)
            {
                //檢核成功才繼續
                context.Response.ContentType = "text/plain";
                if (oOption.NoCache)
                {
                    context.Response.CacheControl = "no-cache";
                    context.Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
                }
                try { DoHandler(context); }
                finally { _dDB?.Dispose(); }
            }
            context.Response.End();
        }
        public bool IsReusable { get { return false; } }

        /// <summary>
        /// 根據 設定項目 檢核
        /// </summary>
        /// <param name="context"></param>
        private void CheckValid(System.Web.HttpContext context)
        {
            if (oOption.Session)
            {
                if (context.Session["uid"] == null)
                    oResult.Error("未登入");
                else
                    oUser = (UserInfo)context.Session["uid"];

                if (oResult.Success && oOption.Guid)
                {
                    if (string.IsNullOrEmpty(context.Request.QueryString["Guid"]))
                        oResult.Error("參數錯誤");
                    else if (context.Request.QueryString["Guid"] != oUser.Guid)
                        oResult.Error("參數錯誤");
                }
            }
            if (oResult.Success && oOption.Parameter.Length > 0)
            {
                if (string.IsNullOrEmpty(context.Request.QueryString[oOption.Parameter]))
                    oResult.Error("參數錯誤");
            }
        }

        /// <summary>
        /// 將傳入JSON字串 轉換成 指定物件類別
        /// </summary>
        /// <typeparam name="T">物件類別</typeparam>
        /// <param name="obj">物件</param>
        /// <param name="str">JSON字串</param>
        protected T DeserializeObject<T>(string str)
        {
            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(str);
            }
            catch { oResult.Error("格式錯誤"); }
            return default(T);
        }

        /// <summary>
        /// 將傳入物件 轉換成 JSON字串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns>回傳JSON字串</returns>
        protected string SerializeObject<T>(T obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// 處理常式 設定選項
        /// </summary>
        public class BaseHandlerOption
        {
            public bool Session { get; set; }//是否檢查Session
            public bool Guid { get; set; }//是否檢查
            public string Parameter { get; set; }//檢查必要參數
            public bool NoCache { get; set; }//設定Client NoCache
            public BaseHandlerOption() { Session = false; Guid = false; Parameter = ""; NoCache = true; }
        }
    }

    public abstract class BaseLogin : BasePage
    {
        Option _oOpt = null;

        protected Option oOpt { get { if (_oOpt == null) { _oOpt = SetOption(); } return _oOpt; } }

        protected abstract Option SetOption();

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (oOpt.btnConfirm != null) { oOpt.btnConfirm.Click += Confirm_Click; }
        }

        protected override void OnLoad(EventArgs e)
        {
            if (!IsPostBack)
            {
                if (cyc.Shared.SysQuery.GetAppSettingValue("AutoLogin") == "1")
                {
                    var user = cyc.Login.GetAutoUser();

                    if (oResult.Success && user != null)
                    {
                        var oUser = cyc.Login.GetUserInfo(user);
                        Session["uid"] = oUser;

                        if (oOpt.IsInPage)
                            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alert", "CloseParent();", true);
                        else if (!string.IsNullOrEmpty(Request.QueryString["rtn"]))
                            Response.Redirect(Server.UrlDecode(Request.QueryString["rtn"]));
                        else
                        {
                            var m = cyc.Login.GetUserMenu(oUser);
                            if (m != null && m.Count > 0)
                            {
                                var p = m.First().Items.First();
                                Response.Redirect(string.Format("~/{0}/?app={1}", p.Dir, p.ID));
                            }
                            oResult.Error("無系統權限");
                        }
                    }
                    else { oResult.Error("帳號或密碼輸入錯誤"); }
                }
            }
            base.OnLoad(e);
            oOpt.txtUserID?.Focus();
        }

        private void Confirm_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(oOpt.txtUserID.Text) && !string.IsNullOrWhiteSpace(oOpt.txtPassword.Text))
            {
                var user = cyc.Login.GetUser(oOpt.txtUserID.Text, oOpt.txtPassword.Text);

                if (oResult.Success && user != null)
                {
                    var oUser = cyc.Login.GetUserInfo(user);
                    Session["uid"] = oUser;

                    if (oOpt.IsInPage)
                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alert", "CloseParent();", true);
                    else if (!string.IsNullOrEmpty(Request.QueryString["rtn"]))
                        Response.Redirect(Server.UrlDecode(Request.QueryString["rtn"]));
                    else
                    {
                        var m = cyc.Login.GetUserMenu(oUser);
                        if (m != null && m.Count > 0)
                        {
                            var p = m.First().Items.First();
                            Response.Redirect(string.Format("~/{0}/?app={1}", p.Dir, p.ID));
                        }
                        oResult.Error("無系統權限");
                    }                  
                }
                else { oResult.Error("帳號或密碼輸入錯誤"); }
            }
            else { oResult.Error("帳號、密碼均不可空白"); }

            oOpt.lblMessage.Text = oResult.Message;
        }

        public class Option
        {
            public TextBox txtUserID { get; set; }
            public TextBox txtPassword { get; set; }
            public Label lblMessage { get; set; }
            public Button btnConfirm { get; set; }
            public bool IsInPage { get; set; }
        }
    }
    #endregion

    //public class KeepAlive : cyc.Auto.AutoJob
    //{
    //    public static readonly string JobKey = "KeepAliveURL";
    //    public static readonly string JobName = "KeepAlive";
    //    protected override void Run()
    //    {
    //        if (cyc.Auto.Manager.GetExclusive(JobKey))
    //        {
    //            try
    //            {
    //                var sURL = cyc.Shared.SysQuery.GetAppSettingValue(JobKey);
    //                if (!string.IsNullOrWhiteSpace(sURL))
    //                {
    //                    System.Net.HttpWebRequest oRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(sURL);
    //                    using (System.Net.HttpWebResponse oResponse = (System.Net.HttpWebResponse)oRequest.GetResponse())
    //                    {
    //                        oResponse.Close();
    //                    }
    //                }
    //            }
    //            catch (Exception ex) { oResult.Error(ex.Message); }
    //            finally { cyc.Auto.Manager.CloseExclusive(JobKey, oResult); }
    //        }
    //        //var sURL = cyc.Shared.SysQuery.GetAppSettingValue("KeepAliveURL");
    //        //if (!string.IsNullOrWhiteSpace(sURL))
    //        //{
    //        //    try
    //        //    {
    //        //        System.Net.HttpWebRequest oRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(sURL);
    //        //        using (System.Net.HttpWebResponse oResponse = (System.Net.HttpWebResponse)oRequest.GetResponse())
    //        //        {
    //        //            oResponse.Close();
    //        //        }
    //        //    }
    //        //    catch { }
    //        //    //catch (Exception ex) { var x = ex.Message; }
    //        //}
    //    }
    //}
}
