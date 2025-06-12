using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._edit
{
    public partial class open : cyc.Page.BaseOpen //System.Web.UI.Page
    {
        //protected void Page_Load(object sender, EventArgs e)
        //{
        //    if (Session["uid"] == null)
        //    {
        //        Response.Redirect("~/loginOpen.aspx?rtn=" + Server.UrlEncode(Request.RawUrl));
        //    }
        //    else if (!string.IsNullOrEmpty(Request.QueryString["app"]) && !string.IsNullOrEmpty(Request.QueryString["sub"]) && int.TryParse(Request.QueryString["app"], out int iApp) && int.TryParse(Request.QueryString["sub"], out int iSub))
        //    {
        //        cyc.Data.UserInfo bUser = (cyc.Data.UserInfo)Session["uid"];

        //        var qList = from lsU in bUser.Role
        //                    join lsX in cyc.Global.SysRole.List.Where(p => p.Enabled) on lsU equals lsX.ID
        //                    join lsRS in cyc.Global.SysRoleProg.List on lsU equals lsRS.RoleID
        //                    join lsPS in cyc.Global.SysProgSub.List on lsRS.ProgID equals lsPS.UpperID
        //                    where lsPS.UpperID == iApp && lsPS.ID == iSub
        //                    select new { RoleID = lsU, isAll = lsRS.isAllSub, lsPS.Path };

        //        if (qList.Count() > 0)
        //        {
        //            var q = qList.FirstOrDefault(p => p.isAll);
        //            if (q != null) { Server.Transfer(q.Path); }

        //            var s = (from lsQ in qList
        //                     join lsS in cyc.Global.SysRoleProgSub.List.Where(p => p.ProgID == iApp && p.SubID == iSub) on lsQ.RoleID equals lsS.RoleID
        //                     select lsQ).FirstOrDefault();
        //            if (s != null) { Server.Transfer(s.Path); }
        //        }
        //    }

        //    Session["invalid"] = "參數錯誤";
        //    Response.Redirect("~/invalid.aspx");
        //}
    }
}