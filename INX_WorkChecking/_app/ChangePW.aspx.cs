using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using cyc.Page;
using Dapper;
using System.Security.Cryptography;
using System.Text;

namespace WebApp._app
{
    public partial class ChangePW : cyc.Page.BasePage
    {
        protected void SaveCheck()
        {
            if (txtPWOLD.Text.Trim().Length == 0 || txtPWNEW.Text.Trim().Length == 0 || txtPWNEW2.Text.Trim().Length == 0)
            {
                oResult.Error("所有欄位均不可空白");
            }
            else if (txtPWNEW.Text.Trim() != txtPWNEW2.Text.Trim())
            {
                oResult.Error("[新密碼]與[確認新密碼]必須相同");
            }
            else
            {
                var xData = dDB.QueryList<dynamic>("select ID from SysUser where ID=@ID and Password=@Password", 
                    new { ID = bUser.User.ID, Password = cyc.Login.CryptoPWD(txtPWOLD.Text.Trim()) });
                if (xData == null || xData.Count() == 0)
                {
                    oResult.Error("[原密碼]輸入錯誤");
                }
            }
        }

        protected void SaveData()
        {
            dDB.Execute("update SysUser set Password=@Password where ID=@ID",
                new { ID = bUser.User.ID, Password = cyc.Login.CryptoPWD(txtPWNEW.Text.Trim()) });
        }

        protected void btnConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                SaveCheck();
                if (oResult.Success)
                    SaveData();
            }
            catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex.Message + ":" + ex.StackTrace, oResult); }

            ShowResult("修改成功", false, false);

            if (oResult.Success)
            {
                txtPWOLD.Text = "";
                txtPWNEW.Text = "";
                txtPWNEW2.Text = "";
            }
        }
    }
}