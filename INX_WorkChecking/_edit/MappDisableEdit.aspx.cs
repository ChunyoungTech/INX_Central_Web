using cyc.Page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._edit
{
    public partial class MappDisableEdit : BasePageSub
    {
        int iID = 0;
        protected override void OnLoad(EventArgs e)
        {
            iID = Convert.ToInt32(Request.QueryString["pa"]);
            if (!IsPostBack)
            {
                for (int idx = 0; idx < 24; idx++)
                {
                    ddlTimeHour.Items.Add(idx.ToString("00"));
                    ddlTimeHour2.Items.Add(idx.ToString("00"));
                }
                for (int idx = 0; idx < 60; idx++)
                {
                    ddlTimeMinute.Items.Add(idx.ToString("00"));
                    ddlTimeMinute2.Items.Add(idx.ToString("00"));
                }
                DateTime tDate = DateTime.Now;
                dteDateS.Value = tDate.Date;
                dteDateE.Value = tDate.Date;
                ddlTimeHour.SelectedValue = "00";
                ddlTimeMinute.SelectedValue = "00";
                ddlTimeHour2.SelectedValue = "23";
                ddlTimeMinute2.SelectedValue = "59";

                ddlType.DataSource = CYCloud.Global.MappSettingType.List;
                ddlType.DataBind();
                ddlType.Items.Insert(0, new ListItem("", "0"));
                BindMappSetting();
            }
            base.OnLoad(e);
        }

        #region #繼承
        protected override SubPageOption SetPageOption() => new SubPageOption() { Confirm = btnConfirm, Parameter = "pa" };
        protected override void LoadData()
        {
            if (iID != 0)
            {
                var oData = dDB.QueryOne<CYCloud.Mapp.Data.MappDisable>(@"select B.* from MappDisable B where B.MD_SEQ_ID=@ID", new { ID = iID });
                if (oData == null)
                    oResult.Error("查無資料");
                else
                {
                    var oSetting = CYCloud.Global.MappSetting.List.FirstOrDefault(p => p.MS_SEQ_ID == oData.MS_SEQ_ID);
                    if (oSetting != null) { ddlType.SelectedValue = oSetting.MT_SEQ_ID.ToString(); }

                    ddlType.Enabled = false;
                    ddlSetting.SelectedValue = oData.MS_SEQ_ID.ToString();
                    ddlSetting.Enabled = false;

                    txtReason.Text = oData.MD_REASON;
                    dteDateS.Value = oData.MD_DATE_START;
                    ddlTimeHour.SelectedValue = oData.MD_DATE_START.ToString("HH");
                    ddlTimeMinute.SelectedValue = oData.MD_DATE_START.ToString("mm");
                    dteDateE.Value = oData.MD_DATE_END;
                    ddlTimeHour2.SelectedValue = oData.MD_DATE_END.ToString("HH");
                    ddlTimeMinute2.SelectedValue = oData.MD_DATE_END.ToString("mm");

                    txtMinites.Text = oData.MD_REMIND_MIN.ToString(); //逾時未解隔通知(每N分鐘)
                    ddlDisableRemind.SelectedValue = oData.MD_REMIND_SETTING.ToString(); //逾時未解隔通知設定(可空白)
                    ddlTransID.SelectedValue = oData.MD_TRANS_ID.ToString(); //隔離轉發設定(可空白)

                    if (oData.MD_STOP_TIME != null)
                        btnConfirm.Visible = false;
                }
            }
        }
        protected override void SaveCheck()
        {
            List<string> sMsg = new List<string>();
            if (string.IsNullOrWhiteSpace(txtReason.Text)) sMsg.Add("[隔離原因]不可空白");
            if (dteDateS.Value == null) sMsg.Add("[開始時間]不可空白且須為日期格式");
            if (dteDateE.Value == null) sMsg.Add("[結束時間]不可空白且須為日期格式");
            if (!cyc.Shared.Check.IsInteger(txtMinites.Text)) sMsg.Add("[未解隔離通知(分鐘)]不可空白且須為整數");
            if (sMsg.Count > 0) oResult.Error(string.Join(";",sMsg));
        }
        protected override void SaveData()
        {
            var oData = new CYCloud.Mapp.Data.MappDisable()
            {
                MS_SEQ_ID = Convert.ToInt32(ddlSetting.SelectedValue),
                MD_SEQ_ID = iID,
                MD_REASON = txtReason.Text.Trim(),
                MD_DATE_START = ((DateTime)dteDateS.Value).AddHours(Convert.ToInt32(ddlTimeHour.SelectedValue)).AddMinutes(Convert.ToInt32(ddlTimeMinute.SelectedValue)),
                MD_DATE_END = ((DateTime)dteDateE.Value).AddHours(Convert.ToInt32(ddlTimeHour2.SelectedValue)).AddMinutes(Convert.ToInt32(ddlTimeMinute2.SelectedValue)),
                MD_TRANS_ID = Convert.ToInt32(ddlTransID.SelectedValue),
                MD_REMIND_MIN = Convert.ToInt32(txtMinites.Text),
                MD_REMIND_SETTING = Convert.ToInt32(ddlDisableRemind.SelectedValue),
                UPDATE_USER = bUser.User.ID,
                UPDATE_TIME = DateTime.Now
            };

            using (cyc.DB.SqlDapperConn oDB = new cyc.DB.SqlDapperConn(oResult, null, true))
            {
                if (oData.MD_SEQ_ID != 0)//寫入LOG
                    oDB.Execute("insert into MappDisableLog select * from MappDisable where MD_SEQ_ID=@MD_SEQ_ID", new { oData.MD_SEQ_ID });
                if (oResult.Success)
                    oDB.Execute(cyc.DB.Shared.GetEditSQL("MappDisable", "MS_SEQ_ID,MD_REASON,MD_DATE_START,MD_DATE_END,UPDATE_USER,UPDATE_TIME,MD_TRANS_ID,MD_REMIND_MIN,MD_REMIND_SETTING;;MD_SEQ_ID", oData.MD_SEQ_ID == 0), oData);
                oDB.ResultTransaction();
            }
        }
        #endregion

        protected void ddlType_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindMappSetting(false);
        }
        void BindMappSetting(bool IsInit = true)
        {
            var allList = CYCloud.Global.MappSetting.List.Select(p => new { ID = p.MS_SEQ_ID, Name = string.Format("{0} ({1})", p.MS_SYS_NAME, p.MS_SYS_DESC), Dept = p.MS_SYS_DEPT, Type = p.MT_SEQ_ID, Default = p.MS_DEFAULT_REMIND });
            var depList = cyc.UC.DeptControl.GetDeptRange(bUser.User.DeptLevel);

            if (depList != null)
            {
                int iType = Convert.ToInt32(ddlType.SelectedValue);
                var usrList = from ls in allList
                              join lsD in cyc.UC.DeptControl.GetDeptRange(bUser.User.DeptLevel) on ls.Dept equals lsD
                              where iType == 0 || ls.Type == iType
                              select ls;

                if (IsInit)
                {
                    ddlDisableRemind.Items.Clear();
                    ddlDisableRemind.DataSource = allList;
                    ddlDisableRemind.DataBind();
                    ddlDisableRemind.Items.Insert(0, new ListItem("", "0"));

                    ddlTransID.Items.Clear();
                    ddlTransID.DataSource = usrList;
                    ddlTransID.DataBind();
                    ddlTransID.Items.Insert(0, new ListItem("", "0"));

                    var qData = allList.FirstOrDefault(p => p.Default);
                    if (qData != null) ddlDisableRemind.SelectedValue = qData.ID.ToString();
                }

                ddlSetting.Items.Clear();
                ddlSetting.DataSource = usrList;
                ddlSetting.DataBind();
            }
        }
    }
}