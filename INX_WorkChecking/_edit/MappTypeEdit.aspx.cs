using cyc.Page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._edit
{
    public partial class MappTypeEdit : BasePageSub
    {
        int iID = 0;
        protected override void OnLoad(EventArgs e)
        {
            iID = Convert.ToInt32(Request.QueryString["pa"]);
            base.OnLoad(e);
        }

        protected override SubPageOption SetPageOption() => new SubPageOption() { Confirm = btnConfirm, Parameter = "pa" };
        protected override void LoadData()
        {
            if (iID != 0)
            {
                var oData = dDB.QueryOne<CYCloud.Mapp.Data.MappSettingType>(@"select A.* from MappSettingType A where A.MT_SEQ_ID=@ID", new { ID = iID });
                if (oData != null)
                {
                    txtName.Text = oData.MT_TYPE_NAME;
                    txtSort.Text = oData.MT_SORT_NUM.ToString();
                }
                else
                    oResult.Error("查無資料");
            }
        }
        protected override void SaveCheck()
        {
            List<string> sMsg = new List<string>();
            if (string.IsNullOrWhiteSpace(txtName.Text))
                sMsg.Add("[分類名稱]不可空白");
            if (oResult.Success && dDB.QueryOne<string>("select MT_TYPE_NAME from MappSettingType where MT_TYPE_NAME=@Name and MT_SEQ_ID<>@ID", new { Name = txtName.Text.Trim(), ID = iID }) != null)
                sMsg.Add("[分類名稱]重複");
            if (oResult.Success && !cyc.Shared.Check.IsInteger(txtSort.Text))
                sMsg.Add("[分類排序]必須是數字");

            if (sMsg.Count > 0) { oResult.Error(string.Join(";", sMsg)); }
        }
        protected override void SaveData()
        {
            var oData = new CYCloud.Mapp.Data.MappSettingType()
            {
                MT_SEQ_ID = iID,
                MT_TYPE_NAME = txtName.Text.Trim(),
                MT_SORT_NUM = Convert.ToInt32(txtSort.Text),
                UPDATE_USER = bUser.User.ID,
                UPDATE_TIME = DateTime.Now
            };

            string sColsAndKey = "MT_TYPE_NAME,MT_SORT_NUM,UPDATE_USER,UPDATE_TIME;;MT_SEQ_ID";
            oData.MT_SEQ_ID = dDB.Execute(cyc.DB.Shared.GetEditSQL("MappSettingType", sColsAndKey, oData.MT_SEQ_ID == 0), oData, oData.MT_SEQ_ID);

            if (oResult.Success)
                CYCloud.Global.MappSettingType.Init(dDB, true);
        }
    }
}