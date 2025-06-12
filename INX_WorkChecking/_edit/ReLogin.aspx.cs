using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._edit
{
    public partial class ReLogin : System.Web.UI.Page
    {
        [System.Web.Services.WebMethod(EnableSession = true)]
        public static pin.ExeResult ReLoginCheck(RcvData oData)
        {
            pin.ExeResult oResult = new pin.ExeResult();
            try
            {
                var user = pin.Comm.Login.GetUser(oData.ID, oData.PWD);
                if (user != null)
                {
                    if (HttpContext.Current.Session["uid"] != null)
                    {
                        var oUser = (pin.UserInfo)HttpContext.Current.Session["uid"];
                        oUser.Guid = Guid.NewGuid().ToString();
                        oResult.Message = oUser.Guid;
                    }
                    else
                    {
                        var oUser = pin.Comm.Login.GetUserInfo(user);
                        HttpContext.Current.Session["uid"] = oUser;
                        oUser.Guid = Guid.NewGuid().ToString();
                        oResult.Message = oUser.Guid;
                    }
                }
                else
                {
                    oResult.Error("認證錯誤");
                }
            }
            catch (Exception ex) { oResult.Error(ex.Message); }
            return oResult;
        }

        public class RcvData
        {
            public string ID { get; set; }
            public string PWD { get; set; }
        }
    }
}