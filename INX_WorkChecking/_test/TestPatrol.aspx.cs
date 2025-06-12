using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._test
{
    public partial class TestPatrol : cyc.Page.BasePageDB
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ddlSetting.DataSource = dDB.QueryDataTable("select ID,Name from PatrolSetting");
                ddlSetting.DataBind();
                ddlSetting.Items.Insert(0, "");
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            //if (!string.IsNullOrEmpty(TextBox1.Text))
            //{
            //    CYCloud.Global.AutoSignal.DoPatrolMessage(TextBox1.Text);
            //}
            if (ddlSetting.SelectedValue.Length > 0 && ddlPlace.SelectedValue.Length > 0)
            {
                var oData = new CYCloud.Patrol.PatrolEvent
                {
                    Setting = Convert.ToInt32(ddlSetting.SelectedValue),
                    Place = Convert.ToInt32(ddlPlace.SelectedValue),
                    DT = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                //if ()
                CYCloud.Global.AutoSignal.DoPatrolMessage(oData);
            }
        }

        protected void ddlPlace_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlEquip.Items.Clear();
            if (!string.IsNullOrEmpty(ddlPlace.SelectedValue))
            {
                ddlEquip.DataSource = dDB.QueryDataTable("select ID,Name from PatrolEquip where PlaceID=@ID", new { ID = Convert.ToInt32(ddlPlace.SelectedValue) });
                ddlEquip.DataBind();
                ddlEquip.Items.Insert(0, "");
            }
        }

        protected void ddlSetting_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlPlace.Items.Clear();
            if (!string.IsNullOrEmpty(ddlSetting.SelectedValue))
            {
                ddlPlace.DataSource = dDB.QueryDataTable("select ID,Name from PatrolPlace where SettingID=@ID", new { ID = Convert.ToInt32(ddlSetting.SelectedValue) });
                ddlPlace.DataBind();
                ddlPlace.Items.Insert(0, "");
                ddlPlace.SelectedIndex = 0;
                ddlPlace_SelectedIndexChanged(null, null);
            }
        }
    }
}