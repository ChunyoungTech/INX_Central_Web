using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp
{
    public partial class TestIFP : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            //if (TextBox1.Text.Length > 0 && TextBox2.Text.Length > 0)
            //{
            //    for (int idx = 0; idx < 10; idx++)
            //    {
            //        CYCloud.IFP.AlertAuth.AlertToClient(TextBox1.Text, TextBox2.Text + idx);
            //        System.Threading.Thread.Sleep(10);
            //    }
            //}
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            ////foreach (var item in CYCloud.IFP.gObj.lstCompleted)
            ////{
            ////    ListBox1.Items.Add(item.Key);
            ////}
            //for (int i = 0; i < 2; i++)
            //{
            //    for (int idx = 0; idx < 10; idx++)
            //    {
            //        CYCloud.IFP.AlertAuth.AlertToClient("aaa", "QQQ" + idx);
            //        System.Threading.Thread.Sleep(10);
            //    }
            //    for (int idx = 0; idx < 10; idx++)
            //    {
            //        CYCloud.IFP.AlertAuth.AlertToClient("bbb", "GGG" + idx);
            //        System.Threading.Thread.Sleep(10);
            //    }
            //}
        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            //CYCloud.IFP.AlertAuth.ClearCompleted();
            //CYCloud.IFP.gObj.lstCompleted.Clear();
            //ListBox1.Items.Clear();
        }

        protected void Button4_Click(object sender, EventArgs e)
        {
            //if (TextBox3.Text.Length > 0 && TextBox4.Text.Length > 0)
            //{
            //    for (int idx = 0; idx < 10; idx++)
            //    {
            //        CYCloud.IFP.AlertAuth.AlertToClient(TextBox3.Text, TextBox4.Text + idx);
            //        System.Threading.Thread.Sleep(10);
            //    }
            //}
        }

        protected void Button5_Click(object sender, EventArgs e)
        {
            var list = CYCloud.IFP.AlertAuth.GetAllClient();
            ListBox1.Items.Clear();
            foreach (var item in list)
            {
                ListBox1.Items.Add(item.ID + ":" + item.Device);
            }
        }

        protected void Button6_Click(object sender, EventArgs e)
        {
            if (ListBox1.SelectedItem != null)
            {
                string[] str = ListBox1.SelectedValue.Split(':');

                //IHubContext hub = GlobalHost.ConnectionManager.GetHubContext<FillPortHub>();//辨識結果
                //hub.Clients.Client(str[0]).clientAlert(str[0], str[1], "");
                //hub.Clients.All.addMessage(str[0]);

                //Global.oSignal.HandleIFPAlert(null, new CYCloud.IFP.ClientAlert() { ID = str[0], Device = str[1], Key = "", Message = "" });

                ////CYCloud.IFP.AlertAuth.AlertToClient(str[1], str[0]);
            }
        }
    }
}