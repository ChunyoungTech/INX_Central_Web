using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._uc
{
    public partial class ucDept : System.Web.UI.UserControl
    {
        bool _isNoInclude = false;
        bool _isShowTop = false;

        public delegate void DeptChangedEventHandler(object sender, EventArgs e);
        public event DeptChangedEventHandler DeptChanged;

        public int DeptID
        {
            get { return Convert.ToInt32(ddlDept.SelectedValue); }
            set
            {
                var dept = ddlDept.Items.FindByValue(value.ToString());
                if (dept != null) { ddlDept.SelectedValue = value.ToString(); }
            }
        }

        public bool AutoPostBack
        {
            get { return ddlDept.AutoPostBack; }
            set { ddlDept.AutoPostBack = value;chkInclude.AutoPostBack = true; }
        }

        public bool isShowAll { get; set; }

        public bool isShowTop { get { return _isShowTop; } set { _isShowTop = value; } }

        public bool isNoInclude { get { return _isNoInclude; } set { _isNoInclude = value; } }

        public bool isInclude { get { return chkInclude.Checked; } }

        protected void Page_Init(object sender, EventArgs e)
        {
            chkInclude.Visible = !isNoInclude;

            if (!IsPostBack && Session["uid"] != null)
            {
                Reset();
            }
        }

        public void Reset()
        {
            ddlDept.Items.Clear();
            cyc.UC.DeptControl.DeptCreate(ddlDept, (cyc.Data.UserInfo)Session["uid"], isShowAll, isShowTop);
        }

        public List<int> DeptRange(int iID = 0)
        {
            if (iID == 0 && ddlDept.SelectedValue.Length != 0) { iID = Convert.ToInt32(ddlDept.SelectedValue); }
            List<int> lstDept = new List<int>();
            if (ddlDept.SelectedValue.Length != 0)
            {
                lstDept.Add(iID);
                if (chkInclude.Checked) { cyc.UC.DeptControl.GetNextDept(iID, ref lstDept); }
            }
            return lstDept;
        }

        public string GetQuerySQL(string sColumn, string sPrefix = "")
        {
            return (ddlDept.SelectedValue.Length != 0 ? string.Format(" {0} {1} in ({2})", sPrefix, sColumn, string.Join(",", DeptRange().ToArray())) : "");
        }

        protected void ddlDept_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlDept.AutoPostBack) { DeptChanged(this, e); }
        }

        protected void chkInclude_CheckedChanged(object sender, EventArgs e)
        {
            if (chkInclude.AutoPostBack) { DeptChanged(this, e); }
        }
    }
}