using cyc.Page;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._edit
{
    public partial class WorkCheckMappFacReport : BasePageSub
    {
        int iID = 0;

        protected override void OnLoad(EventArgs e)
        {
            iID = Convert.ToInt32(Request.QueryString["pa"]);
            if (!IsPostBack)
            {
                var sList = dDB.QueryList<cyc.Data.BaseObj>("select A.MS_SYS_NAME as Code,A.MS_SYS_NAME+' ('+A.MS_SYS_DESC+')' as Name from MappSetting A");
                if (sList != null)
                {
                    ddlMappSetting.DataSource = sList;
                    ddlMappSetting.DataBind();
                }
                var fList = dDB.QueryList<string>("select FAC from AccessListMapping group by FAC order by FAC");
                if (fList != null)
                {
                    ddlFacCode.DataSource = fList;
                    ddlFacCode.DataBind();
                    ddlFacCode.Items.Add(new ListItem("總廠", "ALL"));
                }
            }
            base.OnLoad(e);
        }

        protected override SubPageOption SetPageOption() => new SubPageOption()
        {
            CheckOpen = "open.aspx;WorkCheckMappSetting.aspx",
            Confirm = btnConfirm,
            Parameter = "pa"
        };
        protected override void LoadData()
        {
            if (iID != 0)
            {
                var oData = dDB.QueryOne<MappSetting>("select * from WorkCheckMappFacReport where ID=@ID", new { ID = iID });
                if (oData != null)
                {
                    ddlFacCode.SelectedValue = oData.FacCode;
                    ddlMappSetting.SelectedValue = oData.MappName;
                    ddlEnabled.SelectedValue = oData.IsEnabled ? "1" : "0";
                }
                else
                    oResult.Error("查無資料");
            }
        }
        protected override void SaveCheck()
        {
            var oData = dDB.QueryOne<MappSetting>("select top 1 * from WorkCheckMappFacReport where FacCode=@Code and MappName=@Name and ID<>@ID", new { ID = iID, Code = ddlFacCode.SelectedValue, Name = ddlMappSetting.SelectedValue });
            if (oResult.Success && oData != null)
                oResult.Error("已存在相同[廠別]+[MAPP設定]");
        }
        protected override void SaveData()
        {
            var oData = new MappSetting
            {
                ID = iID,
                FacCode = ddlFacCode.SelectedValue,
                MappName = ddlMappSetting.SelectedValue,
                IsEnabled = ddlEnabled.SelectedValue == "1"
            };

            oData.ID = dDB.Execute(cyc.DB.Shared.GetEditSQL("WorkCheckMappFacReport", "FacCode,MappName,IsEnabled;;ID", iID == 0), oData, oData.ID);
        }

        class MappSetting
        {
            public int ID { get; set; }
            public string FacCode { get; set; }
            public string MappName { get; set; }
            public bool IsEnabled { get; set; }
        }
    }
}