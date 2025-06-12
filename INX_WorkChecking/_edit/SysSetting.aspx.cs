using cyc.Page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._edit
{
    public partial class SysSetting : BasePageSub
    {
        int iID = 0;
        protected override void OnLoad(EventArgs e)
        {
            iID = Convert.ToInt32(Request.QueryString["pa"]);
            base.OnLoad(e);
        }

        #region #繼承
        protected override SubPageOption SetPageOption() => new SubPageOption()
        {
            CheckOpen = "open.aspx;SysSetting.aspx",
            Confirm = btnConfirm,
            Parameter = "pa"
        };
        protected override void LoadData()
        {
            var oData = dDB.QueryOne<cyc.Data.SysSetting>("select * from SysSetting where ID=@ID", new { ID = iID });
            if (oData != null)
            {
                lblCode.Text = oData.Code;
                txtName.Text = oData.Name;
                hidType.Value = oData.Type;
                if (oData.Type != "pwd")
                    txtValue.Text = oData.Value;
                else
                {
                    txtValue2.Text = oData.Value;
                    txtValue2.Visible = true;
                    txtValue.Visible = false;
                }
                lblMemo.Text = oData.Memo ?? "";
            }
            else
                oResult.Error("查無資料");
        }
        protected override void SaveCheck()
        {
            //if (Page.IsValid)
            //{
            //    ValidList = new List<PageFormValid>();
            //    if (txtName.Text.Trim().Length == 0)
            //        ValidList.Add(new PageFormValid { Name = "參數名稱", Message = "不可空白" });
            //    if (txtValue.Text.Trim().Length == 0 && txtValue2.Text.Trim().Length == 0)
            //        ValidList.Add(new PageFormValid { Name = "設定值", Message = "不可空白" });
            //    else
            //    {
            //        if (hidType.Value == "int" && (!int.TryParse(txtValue.Text, out int iValue) || iValue <= 0))
            //            ValidList.Add(new PageFormValid { Name = "設定值", Message = "必須是正整數" });
            //    }
            //}
        }
        protected override void SaveData()
        {
            var oData = new cyc.Data.SysSetting
            {
                ID = iID,
                Code = lblCode.Text,
                Name = txtName.Text,
                Value = hidType.Value != "pwd" ? txtValue.Text : txtValue2.Text
            };
            dDB.Execute("update SysSetting set Name=@Name,Value=@Value where ID=@ID", oData);
            if (oResult.Success)
            {
                var data = cyc.Global.SysSetting.List.FirstOrDefault(p => p.Code == oData.Code);
                if (data != null) { data.Value = oData.Value; data.Name = oData.Name; }
                else { cyc.Global.SysSetting.Init(dDB, true); }

            }
        }
        #endregion
    }
}