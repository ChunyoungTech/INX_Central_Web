using cyc.Page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Inx.Data;

namespace WebApp._edit
{
    public partial class ReportCategoryEdit : cyc.Page.BasePageSub
    {
        protected override string DefaultConntionString() => cyc.DB.ConnString.Report;
        int[] iID = { 0, 0};

        #region #繼承
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (oResult.Success && !IsPostBack)
            {
                ddlReportID.DataSource = dDB.QueryList<cyc.Data.BaseObj>("select ID,Name from ReportData");
                ddlReportID.DataBind();
            }
        }
        protected override void OnLoad(EventArgs e)
        {
            iID = (ViewState["iID"] ?? Request.QueryString["pa"]).ToString().Split(',').Where(p => cyc.Shared.Check.IsInteger(p)).Select(p => Convert.ToInt32(p)).ToArray();
            if (iID.Length < 2 || iID[1] <= 0 || ddlReportID.Items.FindByValue(iID[1].ToString()) == null) 
                oResult.Error("參數錯誤");
            base.OnLoad(e);
        }
        protected override SubPageOption SetPageOption() => new SubPageOption()
        {
            CheckOpen = "open.aspx;ReportCategory.aspx",
            Confirm = btnConfirm,
            Parameter = "pa",
            IsIntPa = false
        };
        protected override void LoadData()
        {
            if (oResult.Success) 
            {
                ddlReportID.SelectedValue = iID[1].ToString();
                if (iID[0] > 0)
                {
                    var oData = dDB.QueryOne<Inx.Data.ReportCategory>("select * from ReportCategory where ID=@ID and ReportID=@Report", new { ID = iID[0], Report = iID[1] });
                    if (oData != null)
                    {
                        ddlReportID.SelectedValue = oData.ReportID.ToString();

                        txtFAC.Text = oData.FAC;
                        txtLevel01.Text = oData.Level01;
                        txtLevel02.Text = oData.Level02;
                        txtLevel03.Text = oData.Level03;
                        txtSeqNo.Text = oData.SeqNo.ToString();
                        ddlDataType.SelectedValue = oData.DataType;
                        txtYearS.Text = oData.YearS.ToString();
                        txtYearE.Text = oData.YearE.ToString();

                        chkAddSUM.Checked = oData.AddSUM;
                        chkExtSUM.Checked = oData.ExtSUM;
                        chkIsSUM.Checked = oData.IsSUM;
                        txtTitleSUM.Text = oData.TitleSUM;

                        chkAddAVG.Checked = oData.AddAVG;
                        chkExtAVG.Checked = oData.ExtAVG;
                        chkIsAVG.Checked = oData.IsAVG;
                        txtTitleAVG.Text = oData.TitleAVG;
                    }
                    else
                        oResult.Error("查無資料");
                }
            }
        }
        protected override void SaveCheck()
        {
            string sMsg = "";
            if (txtFAC.Text.Trim().Length == 0)
                sMsg += "[廠別]不可空白;";
            if (txtLevel01.Text.Trim().Length == 0)
                sMsg += "[類別一]不可空白;";
            if (!int.TryParse(txtSeqNo.Text, out _))
                sMsg += "[排序]必須是數字;";
            if (!int.TryParse(txtYearS.Text, out int years) || years < 2000 || years > 2999)
                sMsg += "[起始年度]必須是數字;";
            if (!int.TryParse(txtYearE.Text, out int yeare) || yeare < 2000 || yeare > 2999)
                sMsg += "[結束年度]必須是數字，且大於2000、小於2999;";
            if (sMsg.Length > 0) oResult.Error(sMsg);
        }
        protected override void SaveData()
        {
            var oData = new Inx.Data.ReportCategory()
            {
                ID = iID[0],
                ReportID = Convert.ToInt32(ddlReportID.SelectedValue),
                FAC = txtFAC.Text,
                Level01 = txtLevel01.Text,
                Level02 = txtLevel02.Text,
                Level03 = txtLevel03.Text,
                SeqNo = Convert.ToInt32(txtSeqNo.Text),
                DataType = ddlDataType.SelectedValue,
                YearS = Convert.ToInt32(txtYearS.Text),
                YearE = Convert.ToInt32(txtYearE.Text),
                AddSUM = chkAddSUM.Checked,
                AddAVG = chkAddAVG.Checked,
                IsSUM = chkIsSUM.Checked,
                IsAVG = chkIsAVG.Checked,
                ExtSUM = chkExtSUM.Checked,
                ExtAVG = chkExtAVG.Checked,
                TitleSUM = txtTitleSUM.Text,
                TitleAVG = txtTitleAVG.Text,
            };
            oData.ID = dDB.Execute(cyc.DB.Shared.GetEditSQL("ReportCategory", "ReportID,FAC,Level01,Level02,Level03,SeqNo,DataType,YearS,YearE,AddSUM,AddAVG,IsSUM,IsAVG,ExtSUM,ExtAVG,TitleSUM,TitleAVG;;ID", oData.ID == 0), oData, oData.ID);
        }
        #endregion
    }
}