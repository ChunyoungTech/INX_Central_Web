using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using cyc.Page;
using System.Data.SqlClient;
using System.Data;

namespace WebApp._edit
{
    public partial class SysRoleEdit : cyc.Page.BasePageSub
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
            CheckOpen = "open.aspx;SysRoleEdit.aspx",
            Confirm = btnConfirm,
            Parameter = "pa"
        };
        protected override void LoadData()
        {
            if (iID != 0)
            {
                var oRole = cyc.Global.SysRole.List.FirstOrDefault(p => p.ID == iID);
                if (oRole != null)
                {
                    txtName.Text = oRole.Name;
                    ddlLevelNo.SelectedValue = oRole.LevelNo.ToString();
                    chkDefault.Checked = oRole.IsDefault;
                    chkEnabled.Checked = oRole.Enabled;
                }
                else
                    oResult.Error("查無資料");
            }
        }
        protected override void SaveData()
        {
            var oRole = new cyc.Data.SysRole()
            {
                ID = iID,
                Name = txtName.Text.Trim(),
                LevelNo = Convert.ToInt32(ddlLevelNo.SelectedValue),
                Enabled = chkEnabled.Checked,
                IsDefault = chkDefault.Checked,
                User = bUser.User.ID
            };

            if (iID == 0)
            {
                oRole.ID = dDB.Execute("insert into SysRole (Name,LevelNo,Enabled,isDefault,c_user) values (@Name,@LevelNo,@Enabled,@IsDefault,@User)", oRole, 0);
                //if (oResult.Success && oRole.ID > 0) { cyc.Global.SysRole.List.Add(oRole); }
            }
            else
            {
                dDB.Execute("update SysRole set Name=@Name,LevelNo=@LevelNo,Enabled=@Enabled,isDefault=@IsDefault,u_user=@User,u_date=getdate() where id=@ID", oRole);
                //if (oResult.Success)
                //{
                //    var oData = cyc.Global.SysRole.List.FirstOrDefault(p => p.ID == iID);
                //    if (oData != null) { pin.Global.CopyObjectValues<cyc.Data.SysRole>(oRole, oData); }
                //}
            }
            if (oResult.Success) { cyc.Global.SysRole.Init(dDB, true); }

        }
        #endregion
    }
}