using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Dapper;

namespace WebApp._app
{
    public partial class CryptoPW : System.Web.UI.Page
    {
        protected void Button1_Click(object sender, EventArgs e)
        {
            using (pin.DB.SqlDBConnLite bDB = new pin.DB.SqlDBConnLite())
            {
                var xList = bDB.oConn.Query<dynamic>("select ID,Password from SysUser");
                foreach (var xData in xList)
                {
                    bDB.oConn.Execute("update SysUser set Password=@Password where ID=@ID",
                        new { ID = xData.ID, Password = pin.Comm.Login.CryptoPWD(xData.Password) });
                }
            }
        }
    }
}