using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp
{
    public partial class loginOpen : cyc.Page.BaseLogin //System.Web.UI.Page
    {
        protected override Option SetOption() => new Option { txtUserID = TextBox1, txtPassword = TextBox2, btnConfirm = btnConfirm, lblMessage = lblMessage };
    }
}