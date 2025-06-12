using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._uc
{
    public partial class ucYesNo : System.Web.UI.UserControl
    {
        public string Yes { get; set; } = "是";
        public string No { get; set; } = "否";

        public bool Value
        {
            //get { return Convert.ToBoolean(this.hidYesNo.Value); }
            set
            {
                hidYesNo.Value = value.ToString();
                lblYesNo.Text = value ? Yes : No;
            }
        }
        public int ValueInt
        {
            set
            {
                Value = value > 0;
            }
        }
    }
}