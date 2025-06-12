using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._uc
{
    public partial class ucComboEnabled : System.Web.UI.UserControl
    {
        public bool Enabled { get { return ddlMain.Enabled; } set { ddlMain.Enabled = value; } }

        public string ValueField { set { ddlMain.DataValueField = value; ddlEnabled.DataValueField = value; } }
        public string TextField { set { ddlMain.DataTextField = value; } }
        public string ViewFiled { set { ddlEnabled.DataTextField = value; } }

        public object DataSource { set { ddlMain.DataSource = value; ddlEnabled.DataSource = value; } }

        public override void DataBind() { ddlMain.DataBind(); ddlEnabled.DataBind(); }

        public string SelectedValue
        {
            get { return ddlMain.SelectedValue; }
            set
            {
                if (ddlMain.Items.FindByValue(value) != null)
                {
                    ddlMain.SelectedValue = value;
                    ddlEnabled.SelectedValue = value;
                    lblEnabled.Text = (ddlEnabled.SelectedItem.Text.ToLower() == "false" ? "已停用" : "");
                }
            }
        }

    }
}