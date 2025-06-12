using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApplication1._master
{
    public partial class Main : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && Session["uid"] != null)
            {
                var oUser = (cyc.Data.UserInfo)Session["uid"];
                ltlUser.Text = $"<p>使用者：{oUser.User.Name}</p><p>單位：{oUser.Dept.Name}</p>";

                var oList = cyc.Login.GetUserMenu(oUser);
                if (oList != null)
                {
                    ltlMenu.Text = string.Join("", oList.Select(p => $"<h1>{p.Name}</h1><ul>{string.Join("", p.Items.Select(d => $"<li data-v='{d.ID}'><a href='../{d.Dir}/?app={d.ID}' {(d.Open ? "target='_blank'" : string.Empty)}>{d.Name}</a></li>"))}</ul>"));
                }
            }
        }
    }
}