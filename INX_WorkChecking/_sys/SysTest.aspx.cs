using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._sys
{
    public partial class SysTest : cyc.Page.BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            int[] total = { 0, 0, 0 };
            int icnt = 0;
            if (int.TryParse(TextBox1.Text, out icnt))
            {
                for (int idx = 0; idx < icnt; idx++)
                {
                    //switch (DateTime.Now.Millisecond % 3)
                    switch (idx % 3)
                    {
                        case 0:
                            System.Threading.Tasks.Task.Run(() => Test01());
                            total[0]++;
                            break;
                        case 1:
                            System.Threading.Tasks.Task.Run(() => Test02());
                            total[1]++;
                            break;
                        default:
                            System.Threading.Tasks.Task.Run(() => Test03());
                            total[2]++;
                            break;
                    }
                }
                Label1.Text = string.Format("Test01:{0}，Test02:{1}，Test03:{2}", total[0], total[1], total[2]);
            }
        }

        private void Test01()
        {
            System.Threading.Thread.Sleep(new Random().Next(1, 9));
            try
            {
                var x = cyc.Global.SysDept.List.Where(p => p.ID % 2 == 1);
            }
            catch (Exception ex) { cyc.Log.WriteSysErrorLog("Test01:" + ex.Message); }
        }

        private void Test02()
        {
            System.Threading.Thread.Sleep(new Random().Next(1, 9));
            try
            {
                var x = cyc.Global.SysDept.List.Where(p => p.ID % 2 == 0);
            }
            catch (Exception ex) { cyc.Log.WriteSysErrorLog("Test02:" + ex.Message); }
        }

        private void Test03()
        {
            System.Threading.Thread.Sleep(new Random().Next(1, 9));
            try
            {
                //cyc.Global.SysDept.Init(pin.Global.gDB);
            }
            catch (Exception ex) { cyc.Log.WriteSysErrorLog("Test03:" + ex.Message); }
        }

        protected void btnCache_Click(object sender, EventArgs e)
        {
            //pin.Global.Close();
        }
    }
}