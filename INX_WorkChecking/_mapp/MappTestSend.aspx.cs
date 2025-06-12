using CYCloud.Mapp.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._mapp
{
    public partial class MappTestSend : cyc.Page.BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ddlSetting.DataSource = CYCloud.Global.MappSetting.List.Select(p => new { ID = p.MS_SEQ_ID, Name = $"{p.MS_SYS_NAME}({p.MS_SYS_DESC})" });
                ddlSetting.DataBind();

                txtMappApi01.Text = cyc.Shared.SysQuery.GetSysSettingValue("MappTeamService");
                txtMappApi02.Text = cyc.Shared.SysQuery.GetSysSettingValue("MappIMService");
            }
        }

        protected void btnSend_Click(object sender, EventArgs e)
        {
            if (int.TryParse(ddlSetting.SelectedValue, out int iID) && !string.IsNullOrEmpty(txtSubject.Text) && !string.IsNullOrEmpty(txtContent.Text)
                && !string.IsNullOrEmpty(txtMappApi01.Text) && !string.IsNullOrEmpty(txtMappApi02.Text))
            {
                var oSetting = CYCloud.Global.MappSetting.List.FirstOrDefault(p => p.MS_SEQ_ID == iID);
                if (oSetting != null)
                {
                    byte[] bArray;

                    if (oSetting.MS_SEND_TYPE == 1)
                    {
                        bArray = Encoding.UTF8.GetBytes(string.Format("account={0}&api_key={1}&team_sn={2}&content_type={3}&text_content={4}&media_content={5}&file_show_name={6}&subject={7}",
                            Uri.EscapeDataString(oSetting.MS_MAPP_ACCOUNT),
                            Uri.EscapeDataString(oSetting.MS_MAPP_API_KEY),
                            oSetting.MS_MAPP_TEAM_SN,
                            1,
                            Uri.EscapeDataString(txtContent.Text),
                            Uri.EscapeDataString(string.Empty),
                            Uri.EscapeDataString(string.Empty),
                            Uri.EscapeDataString(txtSubject.Text)));
                    }
                    else
                    {
                        bArray = Encoding.UTF8.GetBytes(string.Format("account={0}&api_key={1}&chat_sn={2}&content_type={3}&msg_content={4}&file_show_name={5}",
                            Uri.EscapeDataString(oSetting.MS_MAPP_ACCOUNT),
                            Uri.EscapeDataString(oSetting.MS_MAPP_API_KEY),
                            oSetting.MS_MAPP_TEAM_SN,
                            1,
                            Uri.EscapeDataString(txtContent.Text),
                            Uri.EscapeDataString(string.Empty)));
                    }

                    try
                    {
                        string uri = oSetting.MS_SEND_TYPE == 1 ? $"{txtMappApi01.Text}?ask=postMessage" : $"{txtMappApi02.Text}?ask=sendChatMessage";

                        HttpWebRequest oRequest = (HttpWebRequest)WebRequest.Create(uri);
                        oRequest.Method = "POST";
                        oRequest.ContentType = "application/x-www-form-urlencoded";
                        oRequest.ContentLength = bArray.Length;
                        using (Stream oStream = oRequest.GetRequestStream())
                        {
                            oStream.Write(bArray, 0, bArray.Length);
                        }

                        using (HttpWebResponse oResponse = (HttpWebResponse)oRequest.GetResponse())
                        {
                            using (StreamReader sr = new StreamReader(oResponse.GetResponseStream()))
                            {
                                try
                                {
                                    var oReturn = Newtonsoft.Json.JsonConvert.DeserializeObject<MappMessageResponse>(sr.ReadToEnd());
                                    if (oReturn != null)
                                    {
                                        if (!oReturn.IsSuccess)
                                            oResult.Error($"{oReturn.Description}({oReturn.ErrorCode})");
                                    }
                                }
                                catch (Exception exx) { oResult.Error($"解析API回傳發生錯誤({exx.Message})"); }
                                sr.Close();
                            }
                            oResponse.Close();
                        }
                    }
                    catch (Exception ex) { oResult.Error($"MApp發送發生錯誤：{ex.Message}"); }
                }
                else
                    oResult.Error("查無[發送群組]");
            }
            else
                oResult.Error("所有欄位不可空白");

            lblMessage.Text = $"{(oResult.Success ? "OK" : "Error: ")} {oResult.Message}";
        }
    }
}