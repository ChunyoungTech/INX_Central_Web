using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._query
{
    public partial class VendorFAC : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Request.QueryString["pa"]))
            {
                try
                {
                    HttpWebRequest oRequest = (HttpWebRequest)WebRequest.Create(new Uri(cyc.Shared.SysQuery.GetAppSettingValue("VendorLoginCheck")));
                    oRequest.Method = "POST";
                    oRequest.ContentType = "application/json";

                    using (var sw = new System.IO.StreamWriter(oRequest.GetRequestStream()))
                    {
                        sw.Write(Newtonsoft.Json.JsonConvert.SerializeObject(new { sData = Request.QueryString["pa"].ToString() }));
                        sw.Flush();
                        sw.Close();
                    }
                    //取回查詢結果
                    var oResponse = (HttpWebResponse)oRequest.GetResponse();
                    using (var oStr = new System.IO.StreamReader(oResponse.GetResponseStream()))
                    {
                        string str = oStr.ReadToEnd();
                        oStr.Close();
                        oResponse.Close();

                        var User = Newtonsoft.Json.JsonConvert.DeserializeObject<cyc.Data.SysUser>(str);
                        if (User != null)
                        {
                            var oUser = cyc.Login.GetUserInfo(User);
                            oUser.From = 1;
                            Session["uid"] = oUser;
                            string tp = "";
                            if (!string.IsNullOrEmpty(Request.QueryString["tp"])) { tp = Request.QueryString["tp"]; }
                            switch (tp)
                            {
                                case "1":
                                    Response.Redirect("../_app/?app=7");
                                    break;
                                case "2":
                                    Response.Redirect("../_app/?app=9");
                                    break;
                                default:
                                    Response.Redirect("../login.aspx");
                                    break;
                            }
                        }
                    }
                }
                catch (Exception ex) { cyc.Log.WriteSysErrorLog("VendorFAC:" + ex.Message); }
            }
            Response.Redirect("../login.aspx");
        }
    }
}