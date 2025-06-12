using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp
{
    public partial class loginInPage : cyc.Page.BaseLogin
    {
        protected override Option SetOption() => new Option { IsInPage = true, txtUserID = TextBox1, txtPassword = TextBox2, btnConfirm = btnConfirm, lblMessage = lblMessage };
    }
}