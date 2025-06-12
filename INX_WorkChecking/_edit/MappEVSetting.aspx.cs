using cyc.Page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._edit
{
    public partial class MappEVSetting : BasePageSub
    {
        int iID = 0;
        CYCloud.MappEV.Data.MappEVSetting oData = null;
        protected override void OnLoad(EventArgs e)
        {
            iID = Convert.ToInt32(Request.QueryString["pa"]);
            if (!IsPostBack)
            {
                var sList = dDB.QueryList<cyc.Data.BaseObj>("select A.MS_SEQ_ID as ID,A.MS_SYS_NAME+' ('+A.MS_SYS_DESC+')' as Name from MappSetting A");
                if (sList != null)
                {
                    ddlNormalID.DataSource = sList;
                    ddlNormalID.DataBind();

                    //ddlDisableID.DataSource = sList;
                    //ddlDisableID.DataBind();
                    //ddlDisableID.Items.Insert(0, new ListItem("", "0")); //20220921 隔離可不轉發
                }
                lblTemplate1.Text = string.Join("、", CYCloud.MappEV.Shared.TemplateTagE.Select(p => string.Format("{{{0}}}", p)));
                lblTemplate2.Text = string.Join("、", CYCloud.MappEV.Shared.TemplateTagP.Select(p => string.Format("{{{0}}}", p)));
            }
            base.OnLoad(e);
        }

        protected override SubPageOption SetPageOption() => new SubPageOption()
        {
            CheckOpen = "open.aspx;MappEVSetting.aspx",
            Confirm = btnConfirm,
            Parameter = "pa"
        };
        protected override void LoadData()
        {
            if (iID != 0)
            {
                oData = dDB.QueryOne<CYCloud.MappEV.Data.MappEVSetting>("select * from MappEV where ID=@ID", new { ID = iID });
                if (oData != null) 
                {
                    txtCode.Text = oData.Code;
                    txtName.Text = oData.Name;
                    ddlType.SelectedValue = oData.Type;
                    ddlArea.SelectedValue = oData.FacArea;
                    rblTop.SelectedValue = oData.IsTop ? "Y" : "N";
                    ddlNormalID.SelectedValue = oData.NormalID.ToString();
                    //ddlDisableID.SelectedValue = oData.DisableID.ToString();//20230320，隔離一律改至隔離設定
                    txtMappSubject.Text = oData.MappSubject;
                    var sContent = (oData.MappContent ?? "").Split(new string[] { "~@~" }, StringSplitOptions.None);
                    if (!oData.IsTop || sContent.Length == 1)
                        txtMappContent.Text = sContent[0];
                    else
                    {
                        txtMappContentH.Text = sContent[0];
                        txtMappContent.Text = sContent[1];
                        txtMappContentF.Text = sContent[2];
                    }
                    txtCimWebApi.Text = oData.CimWebApi ?? "";
                    ddlCimMethod.SelectedValue = oData.CimMethod ?? "";
                    txtCimParaData.Text = oData.CimParaData ?? "";
                    ddlCimEnable.SelectedValue = oData.CimEnable ? "1" : "0";
                    ddlCimLevel.SelectedValue = oData.CimLevel.ToString();
                    ddlCimGroup.SelectedValue = oData.CimGroup.ToString();
                }
                else
                    oResult.Error("查無資料");
            }
        }
        protected override void SaveCheck()
        {
            List<string> sList = new List<string>();
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtCode.Text))
                sList.Add("[廠別代號]及[設定名稱]均不可空白");

            if (oResult.Success)
            {
                oData = new CYCloud.MappEV.Data.MappEVSetting
                {
                    ID = iID,
                    Code = txtCode.Text.Trim(),
                    Name = txtName.Text.Trim(),
                    Type = ddlType.SelectedValue,
                    FacArea = ddlArea.SelectedValue,
                    IsTop = rblTop.SelectedValue == "Y",
                    NormalID = Convert.ToInt32(ddlNormalID.SelectedValue),
                    //DisableID = Convert.ToInt32(ddlDisableID.SelectedValue),//20230320，隔離一律改至隔離設定
                    DisableID = 0,
                    MappSubject = txtMappSubject.Text.Trim(),
                    MappContent = txtMappContent.Text.Trim(),
                    UpdateTime = DateTime.Now,
                    UpdateUser = bUser.User.ID,
                    CimEnable = ddlCimEnable.SelectedValue == "1",
                    CimWebApi = txtCimWebApi.Text,
                    CimMethod = ddlCimMethod.SelectedValue,
                    CimParaData = txtCimParaData.Text,
                    CimLevel = Convert.ToInt16(ddlCimLevel.SelectedValue),
                    CimGroup = Convert.ToInt16(ddlCimGroup.SelectedValue)
                };
                if (oData.IsTop)
                    oData.MappContent = string.Format("{0}~@~{1}~@~{2}", txtMappContentH.Text.Trim(), txtMappContent.Text.Trim(), txtMappContentF.Text.Trim());

                //20250206取消限制，單一廠區或高階彙整 允許發送多個群組
                //if (dDB.QueryOne<dynamic>("select ID,Code,Type from MappEV where Code=@Code and Type=@Type and FacArea=@FacArea and ID<>@ID", oData) != null)
                //    sList.Add("[廠別代號]+[分類]已存在相同資料");

                if (oData.CimEnable && (string.IsNullOrEmpty(oData.CimWebApi) || string.IsNullOrEmpty(oData.CimMethod)))
                    sList.Add("[CIM啟用]，則[CIM網址]及[CIM方法]均不可空白");

                if (oResult.Success && !CheckTemplate())
                    sList.Add("[標籤]使用錯誤，只能使用限定標籤");

                if (sList.Count > 0)
                    oResult.Error(string.Join(";", sList));
            }
        }
        protected override void SaveData()
        {
            if (oResult.Success && oData != null)
            {
                string sql = cyc.DB.Shared.GetEditSQL("MappEV", "Code,Name,Type,IsTop,NormalID,DisableID,MappSubject,UpdateUser,UpdateTime,FacArea,MappContent,CimWebApi,CimMethod,CimParaData,CimEnable,CimLevel,CimGroup;;ID", oData.ID == 0);
                oData.ID = dDB.Execute(sql, oData, oData.ID);
                if (oResult.Success) { CYCloud.Global.MappEV.Init(dDB, true); }
                //CYCloud.MappEV.Shared.Update(oData, dDB);
            }
        }

        private bool CheckTemplate()
        {
            string[] sTemp = oData.Type == "E" ? CYCloud.MappEV.Shared.TemplateTagE : CYCloud.MappEV.Shared.TemplateTagP;
            bool IsOK = CheckTemp(oData.MappSubject);
            if (IsOK) { IsOK = CheckTemp(oData.MappContent); }
            if (IsOK) { IsOK = CheckTemp(oData.CimParaData); }
            return IsOK;

            bool CheckTemp(string sStr)
            {
                if (!string.IsNullOrWhiteSpace(sStr))
                {
                    foreach (var s in sStr.Split('{').Skip(1))
                    {
                        string[] x = s.Split('}');
                        if (x.Length != 2) { return false; }
                        if (!sTemp.Contains(x[0])) { return false; }
                    }
                }
                return true;
            }
        }
    }
}