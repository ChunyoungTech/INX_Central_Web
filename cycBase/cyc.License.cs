using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CyLicenseKey;

namespace cyc
{
    public static class License
    {
        public static bool IsValid { get; private set; } = false;
        public static int LicenseQty { get; private set; } = 0;
        public static DateTime? LastCheck { get; private set; } = null;

        public static bool CheckLicense()
        {
            System.Net.NetworkInformation.NetworkInterface[] nics = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            List<string> macList = new List<string>();
            foreach (var nic in nics)
            {
                // 因為電腦中可能有很多的網卡(包含虛擬的網卡)，
                // 我只需要 Ethernet 網卡的 MAC
                if (nic.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Ethernet)
                {
                    macList.Add(nic.GetPhysicalAddress().ToString());
                }
            }

            string sLicenseKey = cyc.Shared.SysQuery.GetSysSettingValue("LicenseKey");
            string sLicenseQty = cyc.Shared.SysQuery.GetAppSettingValue("LicenseQty");

            if (macList.Count > 0 && sLicenseKey.Length > 0 && int.TryParse(sLicenseQty, out int iQty))
            {
                ChunyoungKey oKey = new ChunyoungKey();
                foreach (var mac in macList)
                {
                    string sMac = string.Format("{0}-{1}-{2}-{3}-{4}-{5}", mac.Substring(0, 2), mac.Substring(2, 2), mac.Substring(4, 2), mac.Substring(6, 2), mac.Substring(8, 2), mac.Substring(10, 2));
                    if (oKey.checkLicenseNumber(sLicenseKey, sMac, iQty.ToString()))
                    {
                        IsValid = true;
                        LicenseQty = iQty;
                        return true;
                    }
                }
            }
            LastCheck = DateTime.Now;

            return IsValid;
        }

        public static string CreateLicense(string sMac, string sQty, string sPassword, cyc.Data.ExeResult oResult)
        {
            if (sMac.Length != 17) oResult.Error("MAC ADDRESS格式錯誤");
            if (!(int.TryParse(sQty, out int iQty) && iQty > 0)) oResult.Error("授權數量必須是大於0的整數");
            if (sPassword != "45I358@6" + DateTime.Today.ToString("yyyyMMdd")) oResult.Error("密碼錯誤");
            if (oResult.Success)
            {
                return new ChunyoungKey().getLicenseNumber(sMac, sQty);
            }
            return "";
        }
    }
}
