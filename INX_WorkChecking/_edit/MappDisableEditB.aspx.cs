using cyc.Page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._edit
{
    public partial class MappDisableEditB : BasePageSub
    {
        protected override void OnLoad(EventArgs e)
        {
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

                ddlTypeQ.DataSource = CYCloud.Global.MappSettingType.List;
                ddlTypeQ.DataBind();
            }
            base.OnLoad(e);
        }
        protected IEnumerable<int> IDs = null;

        #region #繼承
        protected override SubPageOption SetPageOption() => new SubPageOption()
        {
            Confirm = btnConfirm,
            IsIntPa = false,
            SuccessMsg = "批次隔離設定完成"
        };
        protected override void LoadData()
        {
            BindGridView(true);
        }
        protected override void SaveCheck()
        {
            List<string> sMsg = new List<string>();
            IDs = hidID.Value.Split(',').Where(p => cyc.Shared.Check.IsInteger(p)).Select(p => Convert.ToInt32(p));
            if (IDs.Count() == 0) sMsg.Add("未選擇[MAPP設定]");
            if (string.IsNullOrWhiteSpace(txtReason.Text)) sMsg.Add("[隔離原因]不可空白");
            if (dteDateS.Value == null) sMsg.Add("[開始時間]不可空白且須為日期格式");
            if (dteDateE.Value == null) sMsg.Add("[結束時間]不可空白且須為日期格式");
            if (!string.IsNullOrWhiteSpace(txtMinites.Text) && !cyc.Shared.Check.IsInteger(txtMinites.Text)) sMsg.Add("[未解隔離通知(分鐘)]不可空白且須為整數");
            if (sMsg.Count > 0) oResult.Error(string.Join(";", sMsg));
        }
        protected override void SaveData()
        {
            if (oResult.Success && IDs.Count() > 0)
            {
                var IDx = dDB.QueryList<int>(string.Format("select MS_SEQ_ID from MappSetting where MS_SYS_STOP<>'Y' and {0}", cyc.UC.DeptControl.GetDeptLimitSQL(bUser, "MS_SYS_DEPT")));
                if (IDx != null)
                {
                    var sID = from lsS in IDs join lsX in IDx on lsS equals lsX select lsS;
                    if (sID.Count() > 0)
                    {
                        var oList = sID.Select(p => new CYCloud.Mapp.Data.MappDisable()
                        {
                            MS_SEQ_ID = p,
                            MD_DATE_START = ((DateTime)dteDateS.Value).AddHours(Convert.ToInt32(ddlTimeHour.SelectedValue)).AddMinutes(Convert.ToInt32(ddlTimeMinute.SelectedValue)),
                            MD_DATE_END = ((DateTime)dteDateE.Value).AddHours(Convert.ToInt32(ddlTimeHour2.SelectedValue)).AddMinutes(Convert.ToInt32(ddlTimeMinute2.SelectedValue)),
                            MD_REASON = txtReason.Text.Trim(),
                            MD_REMIND_MIN = Convert.ToInt32(txtMinites.Text),
                            MD_REMIND_SETTING = Convert.ToInt32(ddlDisableRemind.SelectedValue),
                            UPDATE_USER = bUser.User.ID,
                            UPDATE_TIME = DateTime.Now
                        });

                        using (cyc.DB.SqlDapperConn oDB = new cyc.DB.SqlDapperConn(oResult, null, true))
                        {
                            oDB.Execute(cyc.DB.Shared.GetEditSQL("MappDisable", "MS_SEQ_ID,MD_REASON,MD_DATE_START,MD_DATE_END,UPDATE_USER,UPDATE_TIME,MD_REMIND_MIN,MD_REMIND_SETTING;;MD_SEQ_ID", true), oList);
                            oDB.ResultTransaction();
                        }
                    }
                }
            }
        }
        #endregion

        protected void ddlTypeQ_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindGridView();
        }
        protected void ddlNameQ_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindGridView();
        }
        private void BindGridView(bool IsInit = false)
        {
            var aList = CYCloud.Global.MappSetting.List.Where(p => p.MS_SYS_STOP == "N").Select(p => new { p.MS_SEQ_ID, p.MS_SYS_NAME, p.MS_SYS_DEPT, p.MT_SEQ_ID, p.MS_DEFAULT_REMIND });
            if (IsInit)
            {
                ddlDisableRemind.Items.Clear();
                ddlDisableRemind.DataSource = aList;
                ddlDisableRemind.DataBind();
                ddlDisableRemind.Items.Insert(0, new ListItem("", "0"));

                var qData = aList.FirstOrDefault(p => p.MS_DEFAULT_REMIND);
                if (qData != null) ddlDisableRemind.SelectedValue = qData.MS_SEQ_ID.ToString();
            }

            var dList = cyc.UC.DeptControl.GetDeptRange(bUser.User.DeptLevel);
            if (dList != null)
            {
                var oList = from ls in aList
                            join lsD in cyc.UC.DeptControl.GetDeptRange(bUser.User.DeptLevel) on ls.MS_SYS_DEPT equals lsD
                            where (ddlTypeQ.SelectedValue.Length == 0 || ls.MT_SEQ_ID == Convert.ToInt32(ddlTypeQ.SelectedValue)) && (txtNameQ.Text.Trim().Length == 0 || ls.MS_SYS_NAME.Contains(txtNameQ.Text.Trim()))
                            select ls;
                GridView1.DataSource = oList;
                GridView1.DataBind();
            }
        }
        protected void btnQuery_Click(object sender, EventArgs e)
        {
            BindGridView();
        }
    }
}