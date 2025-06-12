using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._sys
{
    public partial class SysLicense : cyc.Page.BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                System.Net.NetworkInformation.NetworkInterface[] nics = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
                foreach (var nic in nics)
                {
                    // 因為電腦中可能有很多的網卡(包含虛擬的網卡)，
                    // 我只需要 Ethernet 網卡的 MAC
                    if (nic.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Ethernet)
                    {
                        string mac = nic.GetPhysicalAddress().ToString();
                        ddlMacList.Items.Add(string.Format("{0}-{1}-{2}-{3}-{4}-{5}", mac.Substring(0, 2), mac.Substring(2, 2), mac.Substring(4, 2), mac.Substring(6, 2), mac.Substring(8, 2), mac.Substring(10, 2)));
                    }
                }
            }
        }

        protected void btnRun_Click(object sender, EventArgs e)
        {
            txtLicense.Text = "";
            string sKey = cyc.License.CreateLicense(ddlMacList.SelectedValue, txtQty.Text, txtPassword.Text, oResult);
            if (oResult.Success) { txtLicense.Text = sKey; }
            else { ShowResult(""); }
        }

        protected void btnReload_Click(object sender, EventArgs e)
        {
            if (!cyc.License.CheckLicense()) { oResult.Error("授權碼錯誤"); }

            ShowResult("[重新套用序號]完成");
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            if (txtLicense.Text.Trim().Length > 0)
                oResult = cyc.Shared.SysQuery.UpdateSysSetting("LicenseKey", txtLicense.Text);
            else
                oResult.Error("[序號]不可空白");

            ShowResult("[更新至SysSetting]完成");
        }
    }
}