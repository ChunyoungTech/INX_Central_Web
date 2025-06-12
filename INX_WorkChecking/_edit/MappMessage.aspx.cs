using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using cyc.Page;
using Dapper;
using System.IO;
using cyc;

namespace WebApp._edit
{
    public partial class MappMessage : cyc.Page.BasePageSub
    {
        int iID = 0;
        protected override void OnLoad(EventArgs e)
        {
            iID = Convert.ToInt32(Request.QueryString["pa"]);
            if (!IsPostBack)
            {
                ddlMS_SYS_NAME.DataSource = dDB.QueryDataTable(string.Format("select MS_SEQ_ID as ID,MS_SYS_NAME as Code,MS_SYS_NAME+' ('+MS_SYS_DESC+')' as Name from MappSetting where {0}", cyc.UC.DeptControl.GetDeptLimitSQL(bUser, "MS_SYS_DEPT")));
                ddlMS_SYS_NAME.DataBind();
                ddlMS_SYS_NAME.Items.Insert(0, new ListItem("", ""));
            }
            base.OnLoad(e);
        }

        protected override SubPageOption SetPageOption() => new SubPageOption()
        {
            CheckOpen = "open.aspx;MappMessage.aspx",
            Confirm = btnConfirm,
            Parameter = "pa",
            SuccessMsg = "成功加入發送排程"
        };
        protected override void LoadData()
        {
            if (iID != 0)
            {
                bPara.Command = @"
select MM_SEQ_ID,MS_SYS_NAME,MM_CONTENT_TYPE,MM_TEXT_CONTENT,MM_FILE_SHOW_NAME,MM_SENDED_FLAG,MM_SUBJECT,MM_TYPE,MM_TRANS_NAME
from MappMessage where MM_SEQ_ID=@ID
;
select * from MappSendLog where MM_SEQ_ID=@ID;";
                bPara.Parameter.Clear();
                bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("ID", iID));
                using (System.Data.DataSet oDS = dDB.QueryDataSet(bPara))
                {
                    if (oResult.Success)
                    {
                        if (oDS.Tables.Count == 2 && oDS.Tables[0].Rows.Count > 0)
                        {
                            System.Data.DataRow oRow = oDS.Tables[0].Rows[0];
                            ddlMS_SYS_NAME.SelectedValue = oRow["MS_SYS_NAME"].ToString().Trim();
                            ddlMM_CONTENT_TYPE.SelectedValue = oRow["MM_CONTENT_TYPE"].ToString().Trim();
                            txtMM_SUBJECT.Text = oRow["MM_SUBJECT"].ToString();
                            txtMM_TEXT_CONTENT.Text = oRow["MM_TEXT_CONTENT"].ToString();
                            ddlMM_SENDED_FLAG.SelectedValue = oRow["MM_SENDED_FLAG"].ToString();
                            ddlMM_TYPE.SelectedValue = oRow["MM_TYPE"].ToString();

                            txtMM_FILE_SHOW_NAME.Text = oRow["MM_FILE_SHOW_NAME"].ToString();

                            //手動才可編輯
                            if (oRow["MM_TYPE"].ToString() == "A")
                            {
                                txtMM_TEXT_CONTENT.Enabled = false;
                                ddlMS_SYS_NAME.Enabled = false;
                            }
                            //隔離轉發
                            lblTransName.Text = oRow["MM_TRANS_NAME"].ToString();

                            //記錄檔
                            if (oDS.Tables[1].Rows.Count > 0)
                            {
                                oRow = oDS.Tables[1].Rows[0];
                                lblML_IS_SUCCESS.Text = Convert.ToBoolean(oRow["ML_IS_SUCCESS"]) ? "是" : "否";
                                lblML_ERROR_CODE.Text = oRow["ML_ERROR_CODE"].ToString();
                                lblML_DESCRIPTION.Text = oRow["ML_DESCRIPTION"].ToString();
                                lblML_SEND_TIME.Text = Convert.ToDateTime(oRow["ML_SEND_TIME"]).ToString("yyyy/MM/dd HH:mm:ss");
                            }
                        }
                        else
                            oResult.Error("查無資料");
                    }
                }
            }
            else
            {
                btnConfirm.Visible = true;
                btnConfirm2.Visible = true;
                ddlMM_TYPE.SelectedValue = "M";
                //btnSend.Visible = false;
                fileMM_FILE_SHOW_NAME.Visible = true;
                txtMM_FILE_SHOW_NAME.Visible = false;
            }
        }
        protected override void SaveCheck()
        {
            List<string> sMsg = new List<string>();
            if (ddlMM_TYPE.SelectedValue == "A") { sMsg.Add("[發送類別]為自動，不可修改"); }

            if (ddlMS_SYS_NAME.SelectedValue == "") { sMsg.Add("[MAPP設定]不可空白"); }

            if (ddlMM_CONTENT_TYPE.SelectedValue == "2" || ddlMM_CONTENT_TYPE.SelectedValue == "3") //2:檔案
            {
                if (string.IsNullOrWhiteSpace(hidMM_FILE_SHOW_NAME.Value)) { sMsg.Add("[傳送檔案]不可空白"); }
            }
            else //1:文字
            {
                if (txtMM_TEXT_CONTENT.Text.Trim().Length == 0) { sMsg.Add("[訊息內容]不可空白"); }
            }
            if (sMsg.Count > 0) { oResult.Error(string.Join("\n", sMsg)); }
        }
        protected override void SaveData()
        {
            var oData = new CYCloud.Mapp.Data.MappMessage
            {
                MM_SEQ_ID = iID,
                MS_SYS_NAME = ddlMS_SYS_NAME.SelectedValue,
                MM_CONTENT_TYPE = Convert.ToInt32(ddlMM_CONTENT_TYPE.SelectedValue),
                MM_SUBJECT = txtMM_SUBJECT.Text.Trim(),
                MM_SENDED_FLAG = ddlMM_SENDED_FLAG.SelectedValue[0],
                MM_TYPE = ddlMM_TYPE.SelectedValue[0],
                MM_SendToOA = 'N',
                MM_SyncFromOA = 'N',
                UPDATE_TIME = DateTime.Now,
                UPDATE_USER = bUser.User.ID
            };

            if (oData.MM_CONTENT_TYPE == 2 || oData.MM_CONTENT_TYPE == 3) //2:檔案
            {
                var oUpload = CYCloud.MappFile.Upload.Get(hidMM_FILE_SHOW_NAME.Value);
                if (oUpload != null)
                {
                    oData.MM_FILE_SHOW_NAME = oUpload.Name;
                    oData.MM_ExtFileName = System.IO.Path.GetExtension(oUpload.Path);
                    oData.MM_MEDIA_CONTENT = File.ReadAllBytes(oUpload.Path);
                    CYCloud.MappFile.Upload.Remove(oUpload.Code);
                }
            }
            else //1:文字 or 4:聊天室
            {
                oData.MM_TEXT_CONTENT = txtMM_TEXT_CONTENT.Text.Trim();
            }

            string sColsAndKey = string.Format("MS_SYS_NAME,MM_CONTENT_TYPE,{0},MM_SUBJECT,MM_SENDED_FLAG,MM_TYPE,UPDATE_TIME,UPDATE_USER,MM_SendToOA,MM_SyncFromOA;;MM_SEQ_ID",
                oData.MM_CONTENT_TYPE == 2 || oData.MM_CONTENT_TYPE == 3 ? "MM_FILE_SHOW_NAME,MM_MEDIA_CONTENT,MM_ExtFileName" : "MM_TEXT_CONTENT");

            string sql = cyc.DB.Shared.GetEditSQL("MappMessage", sColsAndKey, oData.MM_SEQ_ID == 0);

            oData.MM_SEQ_ID = dDB.Execute(sql, oData, oData.MM_SEQ_ID);
        }

        //protected void ddlMS_SYS_NAME_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    var oData = CYCloud.Global.MappSetting.List.FirstOrDefault(p => p.MS_SYS_NAME == ddlMS_SYS_NAME.SelectedValue);
        //    if (oData != null)
        //    {
        //        foreach (ListItem oItem in ddlMM_CONTENT_TYPE.Items)
        //            oItem.Enabled = true;

        //        if (oData.MS_SEND_TYPE == 1)
        //        {
        //            ddlMM_CONTENT_TYPE.Items[2].Enabled = false;
        //        }
        //        else
        //        {
        //            ddlMM_CONTENT_TYPE.Items[0].Enabled = false;
        //            ddlMM_CONTENT_TYPE.Items[1].Enabled = false;
        //        }
        //    }
        //}

        //protected void btnSend_Click(object sender, EventArgs e)
        //{
        //    var oData = new CYCloud.MappMessage
        //    {
        //        MM_SEQ_ID = iID,
        //        MS_SYS_NAME = ddlMS_SYS_NAME.SelectedValue,
        //        MM_CONTENT_TYPE = Convert.ToInt32(ddlMM_CONTENT_TYPE.SelectedValue),
        //        MM_SUBJECT = txtMM_SUBJECT.Text.Trim(),
        //        MM_SENDED_FLAG = 'N',
        //        MM_TYPE = ddlMM_TYPE.SelectedValue[0],
        //        MM_SendToOA = 'N',
        //        MM_SyncFromOA = 'N',
        //        UPDATE_TIME = DateTime.Now,
        //        UPDATE_USER = bUser.User.ID
        //    };

        //    //SaveData();
        //    ShowResult(PageOption.SuccessMsg, true, true);
        //}
    }
}