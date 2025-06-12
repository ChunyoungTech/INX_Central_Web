using cyc.Page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp
{
    public partial class PatrolEdit : BasePageEdit
    {
        int iID = 0;
        protected override void OnLoad(EventArgs e)
        {
            iID = Convert.ToInt32(Request.QueryString["pa"]);
            base.OnLoad(e);
        }

        #region #繼承

        protected override EditPageOption SetEditOption()
        {
            return new EditPageOption() { Confirm = null, Session = true, Parent = true, Parameter = "pa", IsIntPa = true };
        }

        protected override void LoadData()
        {
            if (iID != 0)
            {
                var oData = dDB.QueryOne<PlanData>(@"select S.Name as Setting,B.Name as PatrolUser,C.Name as StartUser,A.TimeStart,A.TimeEnd,A.PlanDate
from PatrolPlan A inner join PatrolSetting S on A.SettingID=S.ID left join SysUser B on A.PatrolUser=B.ID left join SysUser C on A.StartUser=C.ID
where A.ID=@ID", new { ID = iID });
                if (oData == null)
                    oResult.Error("查無資料");
                else
                {
                    dteDate.Value = oData.PlanDate;
                    lblSetting.Text = oData.Setting;
                    lblPatrolUser.Text = oData.PatrolUser;
                    lblStartUser.Text = oData.StartUser;
                    lblTimeStart.Text = oData.TimeStart.ToString("yyyy-MM-dd HH:mm:ss");
                    if (oData.TimeEnd != null) lblTimeEnd.Text = ((DateTime)oData.TimeEnd).ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
        }
        //protected override void SaveCheck()
        //{
        //    List<string> sMsg = new List<string>();
        //    if (string.IsNullOrWhiteSpace(ddlSetting.SelectedValue)) sMsg.Add("[巡檢設定]不可空白");
        //    if (string.IsNullOrWhiteSpace(ddlUser.SelectedValue)) sMsg.Add("[巡檢人員]不可空白");
        //    if (sMsg.Count == 0 && dDB.QueryOne<cyc.Data.BaseObj>("select ID from PatrolPlan where SettingID=@Setting and PlanDate=@Date and ID<>@ID", new { ID = iID, Setting = ddlSetting.SelectedValue, Date = dteDate.Value }) != null)
        //        sMsg.Add("已存在相同[巡檢設定+巡檢日期]資料");

        //    if (sMsg.Count > 0) oResult.Error(string.Join(";", sMsg));
        //}
        protected override void SaveData()
        {
            //var oData = new CYCloud.Patrol.PatrolPlan()
            //{
            //    ID = iID,
            //    SettingID = Convert.ToInt32(ddlSetting.SelectedValue),
            //    PatrolUser = Convert.ToInt32(ddlUser.SelectedValue),
            //    PlanDate = (DateTime)dteDate.Value,
            //    StartUser = bUser.User.ID,
            //    TimeStart = DateTime.Now
            //};

            //oData.ID = dDB.Execute(cyc.DB.Shared.GetEditSQL("PatrolPlan", "PlanDate,SettingID,PatrolUser,StartUser,TimeStart;;ID", oData.ID == 0), oData);
        }

        #endregion

        class PlanData
        {
            public string Setting { get; set; }
            public string PatrolUser { get; set; }
            public string StartUser { get; set; }
            public DateTime PlanDate { get; set; }
            public DateTime TimeStart { get; set; }
            public DateTime? TimeEnd { get; set; }
        }
    }
}