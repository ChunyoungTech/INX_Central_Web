using cyc.Page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._edit
{
    public partial class ReportSettingEdit : cyc.Page.BasePageSub
    {
        protected override string DefaultConntionString() => cyc.DB.ConnString.Report;
        int iID = 0;

        #region #繼承
        protected override void OnLoad(EventArgs e)
        {
            iID = Convert.ToInt32(ViewState["iID"] ?? Request.QueryString["pa"]);
            base.OnLoad(e);
        }
        protected override SubPageOption SetPageOption() => new SubPageOption()
        {
            CheckOpen = "open.aspx;ReportSetting.aspx",
            Confirm = btnConfirm,
            Parameter = "pa"
        };
        protected override void LoadData()
        {
            if (iID > 0)
            {
                var oData = dDB.QueryOne<ReportData>("select * from ReportSetting where ID=@ID", new { ID = iID });
                if (oData != null)
                {
                    txtCode.Text = oData.Code;
                    txtName.Text = oData.Name;
                    chkEnabled.Checked = oData.IsEnabled;
                }
                else
                    oResult.Error("查無資料");
            }
        }
        protected override void SaveCheck()
        {
            string sMsg = "";
            if (txtCode.Text.Trim().Length == 0)
                sMsg += "[報表代號]不可空白;";
            if (txtName.Text.Trim().Length == 0)
                sMsg += "[報表名稱]不可空白;";

            if (sMsg.Length > 0) oResult.Error(sMsg);
        }
        protected override void SaveData()
        {
            var oData = new ReportData()
            {
                ID =iID,
                Code= txtCode.Text.Trim(),
                Name = txtName.Text.Trim(),
                IsEnabled = chkEnabled.Checked
            };
            oData.ID = dDB.Execute(cyc.DB.Shared.GetEditSQL("ReportData", "Code,Name,IsEnabled;;ID", oData.ID == 0), oData, oData.ID);
        }
        #endregion

        class ReportData : cyc.Data.BaseObj
        {
            public bool IsEnabled { get; set; }
        }
    }
}