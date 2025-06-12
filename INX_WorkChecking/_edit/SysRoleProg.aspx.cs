using cyc.Page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._edit
{
    public partial class SysRoleProg : BasePageSub
    {
        int iRole = 0;
        protected override void OnLoad(EventArgs e)
        {
            iRole = Convert.ToInt32(Request.QueryString["pa"]);
            base.OnLoad(e);
        }

        #region #繼承
        protected override SubPageOption SetPageOption() => new SubPageOption()
        {
            CheckOpen = "open.aspx;SysRoleProg.aspx",
            Confirm = btnConfirm,
            Parameter = "pa"
        };
        protected override void LoadData()
        {
            bindGrid();
        }
        protected override void SaveData()
        {
            List<cyc.Data.SysRoleProg> insDataP = new List<cyc.Data.SysRoleProg>(), delDataP = new List<cyc.Data.SysRoleProg>();
            List<cyc.Data.SysRoleProgSub> insDataS = new List<cyc.Data.SysRoleProgSub>(), delDataS = new List<cyc.Data.SysRoleProgSub>();

            var oldDataP = cyc.Global.SysRoleProg.List.Where(p => p.RoleID == iRole);
            var oldDataS = cyc.Global.SysRoleProgSub.List.Where(p => p.RoleID == iRole);
            List<cyc.Data.SysRoleProg> newDataP = new List<cyc.Data.SysRoleProg>();
            List<cyc.Data.SysRoleProgSub> newDataS = new List<cyc.Data.SysRoleProgSub>();

            foreach (GridViewRow gRow in GridView1.Rows)
            {
                if (gRow.RowType == DataControlRowType.DataRow)
                {
                    CheckBox chkID = (CheckBox)(gRow.FindControl("chkID"));
                    HiddenField hidID = (HiddenField)(gRow.FindControl("hidMainID"));
                    CheckBoxList chkList = (CheckBoxList)(gRow.FindControl("chkSubList"));
                    int pID = Convert.ToInt32(hidID.Value);

                    if (chkID.Checked)
                        newDataP.Add(new cyc.Data.SysRoleProg() { RoleID = iRole, ProgID = pID, isAllSub = true });
                    else if (chkList.SelectedValue.Length > 0)
                    {
                        newDataP.Add(new cyc.Data.SysRoleProg() { RoleID = iRole, ProgID = pID, isAllSub = false });
                        foreach (ListItem item in chkList.Items.Cast<ListItem>().Where(p => p.Selected))
                            newDataS.Add(new cyc.Data.SysRoleProgSub() { RoleID = iRole, ProgID = pID, SubID = Convert.ToInt32(item.Value) });
                    }
                }
            }

            foreach (var n in newDataP)
                if (!oldDataP.Any(p => p.ProgID == n.ProgID && p.isAllSub == n.isAllSub)) { insDataP.Add(n); }

            foreach (var o in oldDataP)
                if (!newDataP.Any(p => p.ProgID == o.ProgID && p.isAllSub == o.isAllSub)) { delDataP.Add(o); }

            foreach (var n in newDataS)
                if (!oldDataS.Any(p => p.ProgID == n.ProgID && p.SubID == n.SubID)) { insDataS.Add(n); }

            foreach (var o in oldDataS)
                if (!newDataS.Any(p => p.ProgID == o.ProgID && p.SubID == o.SubID)) { delDataS.Add(o); }

            if (insDataP.Any() || insDataS.Any() || delDataP.Any() || delDataS.Any())
            {
                using (cyc.DB.SqlDapperConn oDB = new cyc.DB.SqlDapperConn(oResult, null, true))
                {
                    if (delDataP.Any())
                        oDB.Execute("delete from SysRoleProg where RoleID=@RoleID and ProgID=@ProgID and isAllSub=@isAllSub", delDataP);

                    if (oResult.Success && insDataP.Any())
                        oDB.Execute("insert into SysRoleProg (RoleID,ProgID,isAllSub) values (@RoleID,@ProgID,@isAllSub)", insDataP);

                    if (oResult.Success && delDataS.Any())
                        oDB.Execute("delete from SysRoleProgSub where RoleID=@RoleID and ProgID=@ProgID and SubID=@SubID", delDataS);

                    if (oResult.Success && insDataS.Any())
                        oDB.Execute("insert into SysRoleProgSub (RoleID,ProgID,SubID) values (@RoleID,@ProgID,@SubID)", insDataS);

                    oDB.ResultTransaction();
                }
                if (oResult.Success) 
                {
                    cyc.Global.SysRoleProg.Init(dDB, true);
                    cyc.Global.SysRoleProgSub.Init(dDB, true);
                }
            }
        }
        #endregion

        IEnumerable<cyc.Data.SysRoleProgSub> subList;
        private void bindGrid()
        {
            subList = cyc.Global.SysRoleProgSub.List.Where(p => p.RoleID == iRole);
            GridView1.DataSource = cyc.Global.SysProg.List;
            GridView1.DataBind();
        }
        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                CheckBox chkAll = (CheckBox)(e.Row.FindControl("chkID"));
                HiddenField hidID = (HiddenField)(e.Row.FindControl("hidMainID"));
                CheckBoxList chkList = (CheckBoxList)(e.Row.FindControl("chkSubList"));
                if (chkAll != null && hidID != null && chkList != null && int.TryParse(hidID.Value, out int iProg))
                {
                    chkAll.Checked = cyc.Global.SysRoleProg.List.Any(p => p.RoleID == iRole && p.ProgID == iProg && p.isAllSub);

                    chkList.DataSource = cyc.Global.SysProgSub.List.Where(p => p.UpperID == iProg && p.isShow);
                    chkList.DataBind();

                    if (!chkAll.Checked && chkList.Items.Count > 0 && subList.Any())
                    {
                        foreach (var oItem in chkList.Items.Cast<ListItem>())
                            oItem.Selected = subList.Any(p => p.SubID == Convert.ToInt32(oItem.Value));

                        //foreach (var item in (from lsX in chkList.Items.Cast<ListItem>()
                        //                      join lsS in cyc.Global.SysRoleProgSub.List.Where(p => p.RoleID == iRole && p.ProgID == iProg) on lsX.Value equals lsS.SubID.ToString()
                        //                      select lsX))
                        //{
                        //    item.Selected = true;
                        //}
                    }
                }
            }
        }
    }
}