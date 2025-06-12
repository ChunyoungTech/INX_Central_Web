using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace WebApp._security
{
    static public class LiseniceAuth
    {

        static string pubkey = "<RSAKeyValue><Modulus>vosF/ROYZ/2NJWlFCPIJPl7DcSGgFDaIAe10vv5R/rhma5btHnpQ9Ox5YnYI6Sngl75yRN09LaVa5jsTAiuuXIbLhq7dNXlWKV7qagZ2Qq/g7aa6I3KGWJec8eEjMYCSL4GeU5BCN8QKwn8GXrTsb1ff5hHKZy7T1kUKuW8OoFk=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
        static public LicInfo Validate()
        {
            string licPath = cyc.Shared.SysQuery.GetAppSettingValue("LicPath");
            LicInfo licInfo = new LicInfo
            {
                isValid = false,
                info = $"授權無效 {licPath}",
            };

            string[] paths = Directory.GetFiles(licPath, "*.lic");
            if (paths.Length == 0)
            {
                licInfo.info = $"查無授權檔 {licPath} ";
                return licInfo;
            }

            RasCryptorService _rsa = new RasCryptorService("");
            _rsa.GenerateKey();
            _rsa.PublicKey = pubkey;
            string info = Path.GetFileNameWithoutExtension(paths[0]);
            string sig = File.ReadAllText(paths[0]);
            var isValid = _rsa.VerifySignature(info, sig);

            if (isValid)
            {
                licInfo.isValid = isValid;
                licInfo.info = info;
                licInfo.validBefore = DateTime.Parse(info.Split(',')[0]).AddDays(1);
            }
            return licInfo;
        }

    }

    public class LicInfo
    {
        public bool isValid;
        public string info;
        public DateTime validBefore;
    }
}