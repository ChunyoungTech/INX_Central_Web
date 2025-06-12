using cyc.Page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._edit
{
    public partial class MappDisableStop : cyc.Page.BasePageSub
    {
        int iID = 0;
        protected override void OnLoad(EventArgs e)
        {
            iID = Convert.ToInt32(Request.QueryString["pa"]);
            base.OnLoad(e);
        }

        #region #繼承
        protected override SubPageOption SetPageOption() => new SubPageOption()
        {
            Confirm = btnConfirm,
            Parameter = "pa",
            SuccessMsg = "解隔離完成"
        };
        protected override void LoadData()
        {
            if (iID != 0)
            {
                var oData = dDB.QueryOne<CYCloud.Mapp.Data.MappDisable>(@"
select B.*,C.MS_SYS_NAME,A.Name as MD_STOP_USER_NAME from MappDisable B 
inner join MappSetting C on B.MS_SEQ_ID=C.MS_SEQ_ID
left join SysUser A on B.MD_STOP_USER=A.ID where B.MD_SEQ_ID=@ID", new { ID = iID });
                if (oData == null)
                    oResult.Error("查無資料");
                else
                {
                    lblSetting.Text = oData.MS_SYS_NAME;
                    txtReason.Text = oData.MD_REASON;
                    txtDateS.Text = oData.MD_DATE_START.ToString("yyyy/MM/dd HH:mm");
                    txtDateE.Text = oData.MD_DATE_END.ToString("yyyy/MM/dd HH:mm");
                    lblStopUser.Text = oData.MD_STOP_USER_NAME;
                    if (oData.MD_STOP_TIME != null)
                        btnConfirm.Visible = false;
                    else
                        lblStopUser.Text = bUser.User.Name;
                }
            }
        }
        protected override void SaveData()
        {
            var oData = new CYCloud.Mapp.Data.MappDisable()
            {
                MD_SEQ_ID = iID,
                MD_STOP_USER = bUser.User.ID,
                MD_STOP_TIME = DateTime.Now
            };
            dDB.Execute("update MappDisable set MD_STOP_USER=@MD_STOP_USER,MD_STOP_TIME=@MD_STOP_TIME where MD_SEQ_ID=@MD_SEQ_ID", oData);
        }
        #endregion
    }
}