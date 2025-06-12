using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp
{
    public partial class invalid : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["invalid"] != null)
            {
                lblMessage.Text = Session["invalid"].ToString();
                Session["invalid"] = null;
            }
        }
    }
}