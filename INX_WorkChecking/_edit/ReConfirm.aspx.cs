using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._edit
{
    public partial class ReConfirm : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!cyc.Login.CheckSession())
            {
                txtID.Focus();
            }
            else
            {
                txtID.Text = ((cyc.Data.UserInfo)Session["uid"]).User.Code;
                txtPWD.Focus();
            }
        }

        [System.Web.Services.WebMethod(EnableSession = true)]
        public static cyc.Data.ExeResult ReLoginCheck(RcvData oData)
        {
            cyc.Data.ExeResult oResult = new cyc.Data.ExeResult();
            if (string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["app"]) || !cyc.Shared.Check.IsInteger(HttpContext.Current.Request.QueryString["app"]))
            {
                oResult.Error("認證錯誤");
            }
            else
            {
                int iApp = Convert.ToInt32(HttpContext.Current.Request.QueryString["app"]);
                try
                {
                    cyc.Data.UserInfo oUser;
                    cyc.Data.SysUser user = cyc.Login.GetUser(oData.ID, oData.PWD);
                    if (user != null)
                    {
                        if (HttpContext.Current.Session["uid"] != null)
                        {
                            oUser = (cyc.Data.UserInfo)HttpContext.Current.Session["uid"];
                            if (oUser.User.ID != user.ID)
                                oResult.Error("認證錯誤");
                            else
                            {
                                oUser.Guid = Guid.NewGuid().ToString();
                                oResult.Message = oUser.Guid;
                            }
                        }
                        else
                        {
                            oUser = cyc.Login.GetUserInfo(user);
                            HttpContext.Current.Session["uid"] = oUser;
                            oUser.Guid = Guid.NewGuid().ToString();
                            oResult.Message = oUser.Guid;
                        }

                        if (oResult.Success && !cyc.Login.CheckUserProg(oUser, iApp))
                        {
                            oResult.Error("認證錯誤");
                        }
                    }
                    else
                    {
                        oResult.Error("認證錯誤");
                    }
                }
                catch (Exception ex)
                {
                    cyc.Log.WriteSysErrorLog(ex.Message + ":" + ex.StackTrace);
                    oResult.Error(ex.Message);
                }
            }
            return oResult;
        }

        public class RcvData
        {
            public string ID { get; set; }
            public string PWD { get; set; }
        }
    }
}