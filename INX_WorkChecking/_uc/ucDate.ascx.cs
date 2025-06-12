using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._uc
{
    public partial class ucDate : System.Web.UI.UserControl
    {
        public delegate void DateChangedEventHandler(object sender, EventArgs e);
        public event DateChangedEventHandler DateChanged;

        public bool AutoPostBack
        {
            get { return txtDate.AutoPostBack; }
            set { txtDate.AutoPostBack = value; }
        }

        public bool Enabled
        {
            get { return txtDate.Visible; }
            set
            {
                txtDate.Visible = value;
                lblDate.Visible = !txtDate.Visible;
            }
        }

        public string Text
        {
            get { return txtDate.Text.Trim(); }
            set
            {
                txtDate.Text = cyc.Data.Shared.SetDate(cyc.Data.Shared.GetDate(value));// dDate.ToString(cyc.Data.Shared.DateFormat);
                lblDate.Text = txtDate.Text;// dDate.ToString(cyc.Data.Shared.DateFormat);
            }
        }

        public DateTime? Value
        {
            get { return cyc.Data.Shared.GetDate(txtDate.Text); }
            set { txtDate.Text = cyc.Data.Shared.SetDate(value); lblDate.Text = txtDate.Text; }
        }

        protected void txtDate_TextChanged(object sender, EventArgs e)
        {
            if (AutoPostBack) { DateChanged(this, e); }
        }
    }
}