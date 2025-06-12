using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp
{
    public partial class index : cyc.Page.BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var m = (from lsU in bUser.Role
                     join lsX in cyc.Global.SysRole.List.Where(p=>p.Enabled) on lsU equals lsX.ID
                     join lsR in cyc.Global.SysRoleProg.List on lsU equals lsR.RoleID
                     join lsP in cyc.Global.SysProg.List on lsR.ProgID equals lsP.ID
                     join lsD in cyc.Global.SysDir.List on lsP.DirID equals lsD.ID
                     where lsD.ID == cyc.Global.SysDir.List.FirstOrDefault().ID && lsP.Enabled == true
                     select new { ID = lsP.ID, Name = lsP.Name, Dir = lsP.Folder }).FirstOrDefault();
            if (m != null) { Response.Redirect("~/" + m.Dir + "/?app=" + m.ID.ToString()); }
        }
    }
}