using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using cyc.Page;
using System.Data.SqlClient;

namespace WebApp._edit
{
    public partial class SysDeptEdit : cyc.Page.BasePageSub
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
            CheckOpen = "open.aspx;SysDeptEdit.aspx",
            Confirm = btnConfirm,
            Parameter = "pa"
        };
        protected override void LoadData()
        {
            if (iID != 0)
            {
                var dept = cyc.Global.SysDept.List.FirstOrDefault(p => p.ID == iID);
                if (dept != null)
                {
                    this.txtName.Text = dept.Name;
                    this.txtCode.Text = dept.Code;
                    this.ucDept.DeptID = dept.UpperID;
                }
                else
                {
                    oResult.Error("查無資料");
                }
            }
        }
        protected override void SaveCheck()
        {
            if (iID != 0)
            {
                var dList = cyc.UC.DeptControl.GetDeptRange(iID);
                if (dList != null && dList.Any(p => p == ucDept.DeptID))
                    oResult.Error("[上級單位]不可指定自己或下屬單位");

                //var old = cyc.Global.SysDept.List.FirstOrDefault(p => p.ID == iID);
                //if (ucDept.DeptRange(old.ID).Any(p => p == ucDept.DeptID))
                //{
                //    oResult.Error("[上級單位]不可指定自己或下屬單位");
                //}
            }
        }
        protected override void SaveData()
        {
            var oData = dDB.QueryOne<cyc.Data.SysDept>("select * from SysDept where ID=@ID", new { ID = iID }) ?? new cyc.Data.SysDept();
            oData.Code = txtCode.Text.Trim();
            oData.Name = txtName.Text.Trim();
            oData.UpperID = ucDept.DeptID;
            //var oData = new cyc.Data.SysDept { ID = iID, Code = txtCode.Text.Trim(), Name = txtName.Text.Trim(), UpperID = ucDept.DeptID };
            oData.ID = dDB.Execute(cyc.DB.Shared.GetEditSQL("SysDept", "Code,Name,UpperID;;ID", oData.ID == 0), oData, oData.ID);

            if (oResult.Success)
            {
                cyc.Global.SysDept.Init(dDB, true);
            }
        }
        #endregion
    }
}