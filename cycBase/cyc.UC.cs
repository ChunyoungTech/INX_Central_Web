using cyc.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

namespace cyc.UC
{
    public abstract class ucPager : System.Web.UI.UserControl
    {
        public delegate void PageChangedEventHandler(object sender, PagerChangeArgs e);
        public event PageChangedEventHandler PageChanged;

        protected abstract DropDownList ddlPageSize { get; }
        protected abstract TextBox txtToGo { get; }
        protected abstract Label lblTotalPage { get; }
        protected abstract Label lblTotalCnt { get; }
        protected abstract Panel pnlVisible { get; }

        public string TargetID { get; set; }

        protected void Page_Init(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                if (Request.Cookies["pagesize"] != null && ddlPageSize.Items.FindByValue(Request.Cookies["pagesize"].Value) != null)
                {
                    ddlPageSize.SelectedValue = Request.Cookies["pagesize"].Value;
                }
                this.Refresh();
            }
        }

        public void Refresh()
        {
            GridView oGrid = (GridView)this.Parent.FindControl(TargetID);
            if (oGrid != null)
            {
                oGrid.PageSize = Convert.ToInt32(this.ddlPageSize.SelectedValue);
                this.txtToGo.Text = (oGrid.PageCount > 0 ? oGrid.PageIndex + 1 : 0).ToString();
                this.lblTotalPage.Text = oGrid.PageCount.ToString();
                this.pnlVisible.Visible = (oGrid.PageCount != 0);
            }
            else
                this.pnlVisible.Visible = false;
        }
        //顯示全部筆數
        public void showTotalCnt(int icnt)
        {
            this.lblTotalCnt.Text = icnt.ToString();
        }
        //觸發事件
        protected void RaisePagerEvent(char x)
        {
            int iPage = 1, iTotal = Convert.ToInt32(lblTotalPage.Text);
            if (iTotal == 0) { return; }

            int.TryParse(txtToGo.Text, out iPage);
            switch (x)
            {
                case 'F':
                    iPage = 1;
                    break;
                case 'P':
                    iPage -= 1;
                    break;
                case 'N':
                    iPage += 1;
                    break;
                case 'L':
                    iPage = Convert.ToInt32(lblTotalPage.Text);
                    break;
                default:
                    break;
            }

            if (iPage < 1) { iPage = 1; }
            if (iPage > iTotal) { iPage = iTotal; }
            txtToGo.Text = iPage.ToString();

            PagerChangeArgs args = new PagerChangeArgs()
            {
                CurrentPage = Convert.ToInt32(this.txtToGo.Text),
                PageSize = Convert.ToInt32(this.ddlPageSize.SelectedValue),
                TotalPages = Convert.ToInt32(this.lblTotalPage.Text)
            };
            PageChanged?.Invoke(this, args);
            this.Refresh();
        }
    }

    public class PagerChangeArgs : EventArgs
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
    }

    public class DeptControl
    {
        public static void DeptCreate(DropDownList ddlDept, cyc.Data.UserInfo oUser, bool isShowAll, bool isShowTop)
        {
            if (!isShowAll)
            {
                var dData = cyc.Global.SysDept.List.FirstOrDefault(p => p.ID == oUser.User.DeptLevel);
                if (dData != null)
                {
                    ddlDept.Items.Add(new ListItem(dData.Name, dData.ID.ToString()));
                    GetNextDept(ddlDept, dData.ID);
                }
            }
            else
            {
                if (isShowTop)
                {
                    ddlDept.Items.Add(new ListItem("", "0"));
                }
                else if (cyc.Global.SysDept.List.Count(p => p.UpperID == 0) > 1)
                {
                    ddlDept.Items.Add(new ListItem("全部", ""));
                }
                GetNextDept(ddlDept, 0);
            }
        }

        private static void GetNextDept(DropDownList ddlDept, int iID, string sPrefix = "")
        {
            sPrefix += iID == 0 ? "" : "　";
            foreach (var dept in cyc.Global.SysDept.List.Where(p => p.UpperID == iID))
            {
                ddlDept.Items.Add(new ListItem(sPrefix + dept.Name, dept.ID.ToString()));
                GetNextDept(ddlDept, dept.ID, sPrefix);
            }
        }

        public static void GetNextDept(int iID, ref List<int> oList)
        {
            foreach (var dept in cyc.Global.SysDept.List.Where(p => p.UpperID == iID))
            {
                oList.Add(dept.ID);
                GetNextDept(dept.ID, ref oList);
            }
        }

        //回傳 是否符合部門權限
        public static bool CheckDeptLimit(cyc.Data.UserInfo oUser, int iID)
        {
            var uDept = cyc.Global.SysDept.List.FirstOrDefault(p => p.ID == oUser.User.DeptLevel);
            if (uDept != null)
            {
                if (uDept.LevelNo == 1)
                    return true;
                else
                    return GetDeptLimit(uDept).Any(p => p.ID == iID);
            }
            return false;
        }

        public static IEnumerable<int> GetDeptRange(int iID)
        {
            var uDept = cyc.Global.SysDept.List.FirstOrDefault(p => p.ID == iID);
            if (uDept != null)
            {
                var dList = GetDeptLimit(uDept);
                if (dList != null && dList.Count() > 0)
                    return dList.Select(p => p.ID);
            }
            return null;
        }

        //回傳 部門權限 SQL
        public static string GetDeptLimitSQL(cyc.Data.UserInfo oUser, string sColumn)
        {
            var uDept = cyc.Global.SysDept.List.FirstOrDefault(p => p.ID == oUser.User.DeptLevel);
            if (uDept != null)
            {
                if (uDept.LevelNo == 1)
                    return "1=1";
                else
                {
                    var dList = GetDeptLimit(uDept);
                    if (dList != null && dList.Count() > 0)
                        return string.Format("{0} in ({1})", sColumn, string.Join(",", dList.Select(p => p.ID)));
                }
            }
            return "1=0";
        }

        private static IEnumerable<cyc.Data.SysDept> GetDeptLimit(cyc.Data.SysDept uDept)
        {
            switch (uDept.LevelNo)
            {
                case 1:
                    return cyc.Global.SysDept.List.Where(p => p.ID1 == uDept.ID);
                case 2:
                    return cyc.Global.SysDept.List.Where(p => p.ID2 == uDept.ID);
                case 3:
                    return cyc.Global.SysDept.List.Where(p => p.ID3 == uDept.ID);
                case 4:
                    return cyc.Global.SysDept.List.Where(p => p.ID4 == uDept.ID);
                case 5:
                    return cyc.Global.SysDept.List.Where(p => p.ID5 == uDept.ID);
                case 6:
                    return cyc.Global.SysDept.List.Where(p => p.ID6 == uDept.ID);
                default:
                    return null;
            }
        }

        public static List<string> GetFacCode(int iDept)
        {
            List<string> dList = new List<string>();
            var uDept = Global.SysDept.List.FirstOrDefault(p => p.ID == iDept);
            if (uDept != null)
            {
                switch (uDept.LevelNo)
                {
                    case 1:
                        dList.AddRange(Global.SysDept.List.Where(p => p.ID1 == uDept.ID && p.LevelNo == 3).Select(p => p.Code));
                        break;
                    case 2:
                        dList.AddRange(Global.SysDept.List.Where(p => p.ID2 == uDept.ID && p.LevelNo == 3).Select(p => p.Code));
                        break;
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                        var oDept = Global.SysDept.List.FirstOrDefault(p => p.ID == uDept.ID3);
                        if (oDept != null) dList.Add(oDept.Code);
                        break;
                    default:
                        break;
                }
            }
            return (from d in dList
                    join f in DeptToFac on d equals f.Code
                    select f.Name).Distinct().ToList();
        }

        private static readonly List<cyc.Data.BaseObj> DeptToFac = new List<BaseObj>()
        {
            new BaseObj {Code = "FAC01", Name = "FAC1" },
            new BaseObj {Code = "FAC02", Name = "FAC2" },
            new BaseObj {Code = "FAC03", Name = "FAC3" },
            new BaseObj {Code = "FAC04", Name = "FAC4" },
            new BaseObj {Code = "FAC05", Name = "FAC5" },
            new BaseObj {Code = "FAC06", Name = "FAC6" },
            new BaseObj {Code = "FAC07", Name = "FAC7" },
            new BaseObj {Code = "FAC08", Name = "FAC8" },
            new BaseObj {Code = "FACL", Name = "FACL" },
            //new BaseObj {Code = "FACL", Name = "科九" }
        };
    }

    //public class DeptControl
    //{
    //    public static void DeptCreate(DropDownList ddlDept, UserInfo oUser, bool isShowAll, bool isShowTop)
    //    {
    //        if (!isShowAll)
    //        {
    //            //if (oUser.UserRole.Any(p => p.LevelNo == 1))
    //            if ((from ls in oUser.Role
    //                 join lsR in cyc.Global.SysRole.List.Where(p => p.Enabled) on ls equals lsR.ID
    //                 where lsR.LevelNo == 1
    //                 select lsR).Any())
    //            {
    //                foreach (var dept in cyc.Global.SysDept.List.Where(p => p.UpperID == 0))
    //                {
    //                    ddlDept.Items.Add(new ListItem(dept.Name, dept.ID.ToString()));
    //                    GetNextDept(ddlDept, dept.ID);
    //                }
    //            }
    //            else
    //            {
    //                ddlDept.Items.Add(new ListItem(oUser.Dept.Name, oUser.Dept.ID.ToString()));
    //                GetNextDept(ddlDept, oUser.Dept.ID);
    //            }
    //        }
    //        else
    //        {
    //            if (isShowTop)
    //            {
    //                ddlDept.Items.Add(new ListItem("", "0"));
    //            }
    //            else if (cyc.Global.SysDept.List.Count(p => p.UpperID == 0) > 1)
    //            {
    //                ddlDept.Items.Add(new ListItem("全部", ""));
    //            }
    //            foreach (var dept in cyc.Global.SysDept.List.Where(p => p.UpperID == 0))
    //            {
    //                ddlDept.Items.Add(new ListItem(dept.Name, dept.ID.ToString()));
    //                GetNextDept(ddlDept, dept.ID);
    //            }
    //        }
    //    }

    //    private static void GetNextDept(DropDownList ddlDept, int iID, string sPrefix = "")
    //    {
    //        sPrefix += "　";
    //        foreach (var dept in cyc.Global.SysDept.List.Where(p => p.UpperID == iID))
    //        {
    //            ddlDept.Items.Add(new ListItem(sPrefix + dept.Name, dept.ID.ToString()));
    //            GetNextDept(ddlDept, dept.ID, sPrefix);
    //        }
    //    }

    //    public static void GetNextDept(int iID, ref List<int> oList)
    //    {
    //        foreach (var dept in cyc.Global.SysDept.List.Where(p => p.UpperID == iID))
    //        {
    //            oList.Add(dept.ID);
    //            GetNextDept(dept.ID, ref oList);
    //        }
    //    }

    //    private static bool GetNextDept(int iID, SysDept oDept)
    //    {
    //        var lsDept = cyc.Global.SysDept.List.Where(p => p.UpperID == oDept.ID);
    //        foreach (var cDept in lsDept)
    //        {
    //            if (cDept.ID == iID)
    //                return true;
    //            else
    //                return GetNextDept(iID, cDept);
    //        }
    //        return false;
    //    }

    //    public static bool CheckDeptLimite(UserInfo oUser, int iID)
    //    {
    //        //if (oUser.UserRole.Any(p => p.LevelNo == 1) || oUser.Dept.ID == iID)
    //        //    return true;

    //        if ((from ls in oUser.Role
    //             join lsR in cyc.Global.SysRole.List.Where(p => p.Enabled) on ls equals lsR.ID
    //             where lsR.LevelNo == 1
    //             select lsR).Any() || oUser.Dept.ID == iID)
    //            return true;

    //        return GetNextDept(iID, oUser.Dept);
    //    }

    //    public static string GetDeptLimitSQL(UserInfo oUser, string sColumn)
    //    {
    //        List<int> oList = new List<int>() { oUser.Dept.ID };
    //        GetNextDept(oUser.Dept.ID, ref oList);
    //        return string.Format("{0} in ({1})", sColumn, string.Join(",", oList.ToArray()));
    //    }
    //}
}
