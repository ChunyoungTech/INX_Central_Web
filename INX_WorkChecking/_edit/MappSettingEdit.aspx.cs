using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using cyc.Page;
using Dapper;

namespace WebApp._edit
{
    public partial class MappSettingEdit : cyc.Page.BasePageSub
    {
        int iID = 0;
        protected override void OnLoad(EventArgs e)
        {
            iID = Convert.ToInt32(Request.QueryString["pa"]);
            if (!IsPostBack)
            {
                var tList = dDB.QueryList<cyc.Data.BaseObj>("select MT_SEQ_ID as ID,MT_TYPE_NAME as Name from MappSettingType order by MT_SORT_NUM");
                if (tList != null)
                {
                    ddlType.DataSource = tList;
                    ddlType.DataBind();
                }
            }
            base.OnLoad(e);
        }

        protected override SubPageOption SetPageOption() => new SubPageOption() { Confirm = btnConfirm, Parameter = "pa" };
        protected override void LoadData()
        {
            if (iID != 0)
            {
                var oData = dDB.QueryOne<CYCloud.Mapp.Data.MappSetting>(@"
select A.*,B.Name as UPDATE_USER_NAME
from MappSetting A left join SysUser B on A.UPDATE_USER=B.ID where A.MS_SEQ_ID=@ID", new { ID = iID });
                if (oData != null && cyc.UC.DeptControl.CheckDeptLimit(bUser, oData.MS_SYS_DEPT))
                {
                    txtName.Text = oData.MS_SYS_NAME;
                    txtDesc.Text = oData.MS_SYS_DESC;
                    txtAccount.Text = oData.MS_MAPP_ACCOUNT;
                    txtApiKey.Text = oData.MS_MAPP_API_KEY;
                    txtTeamSN.Text = oData.MS_MAPP_TEAM_SN.ToString();
                    rblStop.SelectedValue = (oData.MS_SYS_STOP ?? "N").ToString();
                    ucDept.DeptID = oData.MS_SYS_DEPT;
                    //ddlTransID.SelectedValue = oData.MS_TRANS_ID.ToString();
                    lblUpdateUser.Text = oData.UPDATE_USER_NAME;
                    lblUpdateTime.Text = oData.UPDATE_TIME.ToString("yyyy/MM/dd HH:mm:ss");
                    ddlType.SelectedValue = oData.MT_SEQ_ID.ToString();
                    rblDefaultRemind.SelectedValue = oData.MS_DEFAULT_REMIND ? "1" : "0"; //預設逾時未解隔通知
                    ddlSendType.SelectedValue = oData.MS_SEND_TYPE.ToString(); //發送類別 1:訊息 2:聊天室

                    txtName.Enabled = false;
                    btnDelete.Visible = true;
                }
                else
                    oResult.Error("查無資料");
            }
        }
        protected override void SaveCheck()
        {
            List<string> sMsg = new List<string>();
            if (txtName.Text.Trim().Length == 0 || txtDesc.Text.Trim().Length == 0 || txtAccount.Text.Trim().Length == 0 || txtApiKey.Text.Trim().Length == 0 || txtTeamSN.Text.Trim().Length == 0)
                sMsg.Add("[所有欄位]均不可空白");
            if (oResult.Success && dDB.QueryOne<string>("select MS_SYS_NAME from MappSetting where MS_SYS_NAME=@Name and MS_SEQ_ID<>@ID", new { Name = txtName.Text.Trim(), ID = iID }) != null)
                sMsg.Add("[設定名稱]重複");
            if (oResult.Success && !cyc.Shared.Check.IsInteger(txtTeamSN.Text))
                sMsg.Add("[團隊編號]必須是數字");

            if (oResult.Success && rblDefaultRemind.SelectedValue == "1") //預設逾時未解隔通知-只能有一筆
            { 
                if (dDB.QueryOne<string>("select MS_SYS_NAME from MappSetting where MS_DEFAULT_REMIND=1 and MS_SEQ_ID<>@ID", new { ID = iID }) != null)
                    sMsg.Add("[預設逾時未解隔通知設定]已被設定");
            }
            if (sMsg.Count > 0) { oResult.Error(string.Join(";", sMsg)); }
        }
        protected override void SaveData()
        {
            var oData = new CYCloud.Mapp.Data.MappSetting()
            {
                MS_SEQ_ID = iID,
                MS_SYS_NAME = txtName.Text.Trim(),
                MS_SYS_DESC = txtDesc.Text.Trim(),
                MS_MAPP_ACCOUNT = txtAccount.Text.Trim(),
                MS_MAPP_API_KEY = txtApiKey.Text.Trim(),
                MS_MAPP_TEAM_SN = Convert.ToInt32(txtTeamSN.Text),
                MS_SYS_STOP = rblStop.SelectedValue,
                UPDATE_USER = bUser.User.ID,
                UPDATE_TIME = DateTime.Now,
                MS_SYNC_TO_OA = 'N',
                MS_SYS_DEPT = ucDept.DeptID,
                MT_SEQ_ID = Convert.ToInt32(ddlType.SelectedValue),
                MS_DEFAULT_REMIND = rblDefaultRemind.SelectedValue == "1", //預設逾時未解隔通知
                MS_SEND_TYPE = Convert.ToInt16(ddlSendType.SelectedValue) //發送類別 1:訊息 2:聊天室
            };

            //20220920新增 寫入MappSettingLog
            if (oData.MS_SEQ_ID != 0)
            {
                dDB.Execute("insert into MappSettingLog select * from MappSetting where MS_SEQ_ID=@ID", new { ID = oData.MS_SEQ_ID });
            }

            string sColsAndKey = "MS_SYS_NAME,MS_SYS_DESC,MS_MAPP_ACCOUNT,MS_MAPP_API_KEY,MS_MAPP_TEAM_SN,UPDATE_TIME,UPDATE_USER,MS_SYS_STOP,MS_SYNC_TO_OA,MS_SYS_DEPT,MT_SEQ_ID,MS_DEFAULT_REMIND,MS_SEND_TYPE;;MS_SEQ_ID";
            oData.MS_SEQ_ID = dDB.Execute(cyc.DB.Shared.GetEditSQL("MappSetting", sColsAndKey, oData.MS_SEQ_ID == 0), oData, oData.MS_SEQ_ID);

            if (oResult.Success)
            {
                //CYCloud.gObj.MappSettings.Update(oData);
                CYCloud.Global.MappSetting.Init(dDB, true);
                CYCloud.ExecLog.WriteLog(new CYCloud.ExecLog.LogItem()
                {
                    ExecID = Convert.ToInt32(Request.QueryString["app"]),
                    ExecType = (iID != 0 ? "update" : "insert"),
                    ExecDesc = string.Format("MAPP發送設定 MS_SEQ_ID={0}", oData.MS_SEQ_ID),
                    UserID = bUser.User.ID
                }, dDB);
            }
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            hidKey.Value = BasePageFunc.ReCheckAuth(hidKey.Value, bUser.Guid, oResult);
            //CheckKey();
            if (!oResult.Success) { ShowResult("", false, false); return; }

            dDB.Execute("delete from MappSetting where MS_SEQ_ID=@ID", new { ID = iID });
            if (oResult.Success)
            {
                CYCloud.ExecLog.WriteLog(new CYCloud.ExecLog.LogItem() { ExecID = Convert.ToInt32(Request.QueryString["app"]), ExecType = "delete", ExecDesc = string.Format("刪除MAPP發送設定 MS_SEQ_ID={0}", iID), UserID = bUser.User.ID }, dDB);
                //CYCloud.gObj.MappSettings.Delete(iID);
                CYCloud.Global.MappSetting.Init(dDB, true);
            }
            ShowResult("刪除完成", true, true);
        }
    }
}