using cyc.Page;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Dapper;

namespace WebApp._edit
{
    public partial class SysUserEdit : cyc.Page.BasePageSub
    {
        static string sDefault = cyc.Login.CryptoPWD(cyc.Shared.SysQuery.GetSysSettingValue("DefaultPWD"));
        int iID = 0;
        protected override void OnLoad(EventArgs e)
        {
            iID = Convert.ToInt32(Request.QueryString["pa"]);
            base.OnLoad(e);
        }

        #region #繼承
        protected override SubPageOption SetPageOption() => new SubPageOption()
        {
            CheckOpen = "open.aspx;SysUserEdit.aspx",
            Confirm = btnConfirm,
            Parameter = "pa"
        };
        protected override void LoadData()
        {
            if (iID != 0)
            {
                var user = dDB.QueryOne<cyc.Data.SysUser>("select ID,Code,Name,DeptID,Enabled,isManager,ISNULL(DeptLevel,DeptID)as DeptLevel from SysUser where ID=@ID", new { ID = iID });
                if (oResult.Success && user != null)
                {
                    txtName.Text = user.Name;
                    txtCode.Text = user.Code;
                    btnDefault.Visible = true;
                    ucDept.DeptID = user.DeptID;
                    chkEnabled.Checked = user.Enabled;
                    chkManager.Checked = user.isManager;
                    ucDeptLevel.DeptID = user.DeptLevel;
                }
                else
                    oResult.Error("查無資料");
            }
        }
        protected override void SaveData()
        {
            var oUser = dDB.QueryOne<cyc.Data.SysUser>("select * from SysUser where ID=@ID", new { ID = iID });
            if (iID == 0) { oUser = new cyc.Data.SysUser(); }

            oUser.Code = txtCode.Text.Trim();
            oUser.Name = txtName.Text.Trim();
            oUser.DeptID = ucDept.DeptID;
            oUser.Enabled = chkEnabled.Checked;
            oUser.isManager = chkManager.Checked;
            oUser.DeptLevel = ucDeptLevel.DeptID;
            if (iID == 0) oUser.Password = sDefault;

            dDB.Execute(cyc.DB.Shared.GetEditSQL("SysUser", "Code,Name,DeptID,Password,Enabled,isManager,DeptLevel;;ID", iID == 0), oUser);
        }
        #endregion

        protected void btnDefault_Click(object sender, EventArgs e)
        {
            //if (this.hidID.Value != "0")
            //{
            //    try
            //    {
            //        bDB.oConn.Execute("update SysUser set Password=@PW where ID=@ID",new { ID = Convert.ToInt32(hidID.Value), PW = sDefault });
            //    }
            //    catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex.Message + ":" + ex.StackTrace, oResult); }
            //    ShowResult("回復成功", false, false);
            //}
            if (iID != 0)
            {
                dDB.Execute("update SysUser set Password=@PW where ID=@ID", new { ID = iID, PW = sDefault });
                ShowResult("回復成功", false, false);
            }
        }
    }
}