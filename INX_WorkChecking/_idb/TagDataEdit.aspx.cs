using cyc.Data;
using cyc.Page;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._idb
{
    public partial class TagDataEdit : BasePageSub
    {
        int iID = 0;
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (!IsPostBack)
            {
                ddlFac.DataSource = dDB.QueryList<BaseObj>(@"
select distinct C.SeqID as ID,C.FacName as Code from (
	select distinct ID3 from View_SysDeptLevel where ID1=@ID or ID2=@ID or ID3=@ID or ID4=@ID or ID5=@ID
)A inner join View_SysDeptLevel B on A.ID3=B.ID inner join IDBFacData C on B.Code=C.FacName", new { ID = bUser.User.DeptLevel });
                ddlFac.DataBind();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            iID = Convert.ToInt32(ViewState["ID"] ?? Request.QueryString["pa"]);
            base.OnLoad(e);
        }

        #region #繼承
        protected override SubPageOption SetPageOption() => new SubPageOption()
        {
            CheckOpen = "open.aspx;TagDataSetting.aspx",
            Confirm = btnConfirm,
            Parameter = "pa",
            AppID = new int[] { 30 },
            SubID = new int[] { 26 },
        };
        protected override void LoadData()
        {
            if (iID != 0)
            {
                var oData = dDB.QueryOne<TagData>("select A.*,B.IDBFacDataID as FacID,B.mesurement as [SysName] from TagData A inner join IDBSysMapping B on A.TagSys=B.SeqID where ID=@ID", new { ID = iID });
                if (oData != null)
                {
                    if (ddlFac.Items.Cast<ListItem>().FirstOrDefault(p => p.Value == oData.FacID.ToString()) != null)
                    {
                        ddlFac.SelectedValue = oData.FacID.ToString();
                        GetSysData();
                        ddlSys.SelectedValue = oData.TagSys.ToString();
                        txtName.Text = oData.Tag_Name;
                        txtDesc.Text = oData.Tag_Desc;
                        txtUnit.Text = oData.Unit;
                        ddlType.SelectedValue = oData.Tag_Type;
                        txtHiHiLimit.Text = oData.HiHi_Limit.ToString();
                        txtHiLimit.Text = oData.Hi_Limit.ToString();
                        txtLoLimit.Text = oData.Lo_Limit.ToString();
                        txtLoLoLimit.Text = oData.LoLo_Limit.ToString();
                    }
                    else
                        oResult.Error("查無資料");
                }
                else
                    oResult.Error("查無資料");
            }
            else
            {
                ddlFac.SelectedIndex = 0;
                GetSysData();
            }
        }
        protected override void SaveCheck()
        {
            string sMsg = "";
            if (string.IsNullOrWhiteSpace(txtName.Text))
                sMsg += "[點位名稱]不可空白;";
            else if (dDB.QueryOne<BaseObj>("select ID from TagData where Tag_Name=@Name and ID<>@ID", new { Name = txtName.Text, ID = iID }) != null)
                sMsg += "[點位名稱]已重複;";
            if (string.IsNullOrEmpty(ddlSys.SelectedValue))
                sMsg += "[系統別]不可空白";
            if (!string.IsNullOrWhiteSpace(txtHiHiLimit.Text) && !cyc.Shared.Check.IsNumeric(txtHiHiLimit.Text.Trim()))
                sMsg += "[HIHI警報值]必須是數值;";
            if (!string.IsNullOrWhiteSpace(txtHiLimit.Text) && !cyc.Shared.Check.IsNumeric(txtHiLimit.Text.Trim()))
                sMsg += "[HI警報值]必須是數值;";
            if (!string.IsNullOrWhiteSpace(txtLoLimit.Text) && !cyc.Shared.Check.IsNumeric(txtLoLimit.Text.Trim()))
                sMsg += "[LO警報值]必須是數值;";
            if (!string.IsNullOrWhiteSpace(txtLoLoLimit.Text) && !cyc.Shared.Check.IsNumeric(txtLoLoLimit.Text.Trim()))
                sMsg += "[LOLO警報值]必須是數值;";
            if (sMsg.Length > 0)
                oResult.Error(sMsg);
        }
        protected override void SaveData()
        {
            var oData = new TagData
            {
                ID = iID,
                Tag_Name = txtName.Text.Trim(),
                Tag_Desc = txtDesc.Text.Trim(),
                Unit = txtUnit.Text.Trim(),
                Tag_Type = ddlType.SelectedValue,
                TagSys = Convert.ToInt32(ddlSys.SelectedValue)
            };
            if (!string.IsNullOrWhiteSpace(txtHiHiLimit.Text)) oData.HiHi_Limit = Convert.ToDecimal(txtHiHiLimit.Text);
            if (!string.IsNullOrWhiteSpace(txtHiLimit.Text)) oData.Hi_Limit = Convert.ToDecimal(txtHiLimit.Text);
            if (!string.IsNullOrWhiteSpace(txtLoLimit.Text)) oData.Lo_Limit = Convert.ToDecimal(txtLoLimit.Text);
            if (!string.IsNullOrWhiteSpace(txtLoLoLimit.Text)) oData.LoLo_Limit = Convert.ToDecimal(txtLoLoLimit.Text);

            oData.ID = dDB.Execute(cyc.DB.Shared.GetEditSQL("TagData", "Tag_Name,Tag_Desc,Unit,Tag_Type,TagSys,HiHi_Limit,Hi_Limit,Lo_Limit,LoLo_Limit;;ID", iID == 0), oData, iID);
        }
        #endregion

        class TagData
        {
            public int ID { get; set; }
            public string Tag_Name { get; set; }
            public string Tag_Desc { get; set; }
            public string Tag_Type { get; set; }
            public string Unit { get; set; }
            public decimal? HiHi_Limit { get; set; }
            public decimal? Hi_Limit { get; set; }
            public decimal? Lo_Limit { get; set; }
            public decimal? LoLo_Limit { get; set; }
            public int TagSys { get; set; }
            public int FacID { get; set; }
            public string SysName { get; set; }
        }

        protected void ddlFac_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetSysData();
        }

        private void GetSysData()
        {
            ddlSys.DataSource = dDB.QueryList<BaseObj>("select SeqID as ID,mesurement as Code from IDBSysMapping where IDBFacDataID=@Fac", new { Fac = Convert.ToInt32(ddlFac.SelectedValue) });
            ddlSys.DataBind();
            ddlSys.Items.Insert(0, "");
        }
    }
}