using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Dapper;

namespace WebApp._edit
{
    public partial class ChangePWD : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!cyc.Login.CheckSession())
            {
                Response.Redirect("ReConfirm.aspx");
            }
        }

        [System.Web.Services.WebMethod(EnableSession = true)]
        public static cyc.Data.ExeResult Change(RcvData oData)
        {
            cyc.Data.ExeResult oResult = new cyc.Data.ExeResult();
            try
            {
                cyc.Data.UserInfo oUser = (cyc.Data.UserInfo)HttpContext.Current.Session["uid"];

                var user = cyc.Login.GetUser(oUser.User.Code, oData.O);

                if (user != null)
                {
                    using (cyc.DB.SqlDapperConn oDB = new cyc.DB.SqlDapperConn())
                    {
                        oDB.Execute("update SysUser set Password=@Password where ID=@ID", new { ID = oUser.User.ID, Password = cyc.Login.CryptoPWD(oData.N.Trim()) });
                    }
                }
                else
                    oResult.Error("認證錯誤");
            }
            catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex.Message + ":" + ex.StackTrace, oResult); }
            return oResult;
        }

        public class RcvData
        {
            public string O { get; set; }
            public string N { get; set; }
            public string N2 { get; set; }
        }
    }
}