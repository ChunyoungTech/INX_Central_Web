using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebApp._security;

namespace WebApp
{
    public partial class login_OLD : System.Web.UI.Page
    {
        pin.ExeResult oResult = new pin.ExeResult();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                TextBox1.Focus();
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            if (TextBox1.Text.Trim().Length > 0 && TextBox2.Text.Trim().Length > 0)
            {
                var user = pin.Login.GetUser(TextBox1.Text, TextBox2.Text);

                if (oResult.Success && user != null)
                {
                    var oUser = pin.Login.GetUserInfo(user);

                    Session["uid"] = oUser;
                    if (!string.IsNullOrEmpty(Request.QueryString["rtn"]))
                        Response.Redirect(Server.UrlDecode(Request.QueryString["rtn"]));
                    else
                    {
                        var m = pin.Login.GetUserMenu(oUser);
                        if (m != null && m.Count > 0)
                        {
                            //Response.Redirect("~/_app/home.aspx");
                            var p = m.First().Items.First();
                            Response.Redirect(string.Format("~/{0}/?app={1}", p.Dir, p.ID));
                        }
                        //foreach (var m in pin.Global.SysDir.List)
                        //{
                        //    //var x = from lsU in oUser.UserRole
                        //    //        join lsRP in pin.gObj.SysRoleProg on lsU.ID equals lsRP.RoleID
                        //    //        join lsP in pin.gObj.SysProg on lsRP.ProgID equals lsP.ID
                        //    //        where lsP.DirID == m.ID && lsP.Enabled == true
                        //    //        select new pin.UIMenuItem() { ID = lsP.ID, Name = lsP.Name, Dir = lsP.Folder, Seq = lsP.Seq };
                        //    var x = from lsU in oUser.Role
                        //            join lsX in pin.Global.SysRole.List.Where(p => p.Enabled) on lsU equals lsX.ID
                        //            join lsRP in pin.Global.SysRoleProg.List on lsU equals lsRP.RoleID
                        //            join lsP in pin.Global.SysProg.List on lsRP.ProgID equals lsP.ID
                        //            where lsP.DirID == m.ID && lsP.Enabled == true
                        //            select new pin.UIMenuItem() { ID = lsP.ID, Name = lsP.Name, Dir = lsP.Folder, Seq = lsP.Seq };
                        //    var z = x.GroupBy(o => o.ID).Select(o => o.FirstOrDefault()).OrderBy(p => p.Seq).FirstOrDefault();
                        //    if (z != null)
                        //        Response.Redirect(string.Format("~/{0}/?app={1}", z.Dir, z.ID.ToString()));
                        //}

                    }
                    oResult.Error("無系統權限");
                }
                else { oResult.Error("帳號或密碼輸入錯誤"); }
            }
            else { oResult.Error("帳號、密碼均不可空白"); }

            Label1.Text = oResult.Message;
        }
    }
}