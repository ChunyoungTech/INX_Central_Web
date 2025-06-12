using cyc.Page;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using System.Xml.Linq;

namespace WebApp._edit
{
    public partial class WorkCheckMappSetting : BasePageSub
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
                var oData = dDB.QueryOne<MappSetting>("select * from WorkCheckMappSetting where ID=@ID", new { ID = iID });
                if (oData != null)
                {
                    ucDept.DeptID = oData.DeptID;
                    ddlMappSetting.SelectedValue = oData.MappName;
                    ddlEnabled.SelectedValue = oData.IsEnabled ? "1" : "0";
                }
                else
                    oResult.Error("查無資料");
            }
        }
        protected override void SaveCheck()
        {
            var oData = dDB.QueryOne<MappSetting>("select top 1 * from WorkCheckMappSetting where DeptID=@Dept and MappName=@Name and ID<>@ID", new { ID = iID, Dept = ucDept.DeptID, Name = ddlMappSetting.SelectedValue });
            if (oResult.Success && oData != null)
                oResult.Error("已存在相同[發送部門]+[MAPP設定]");
        }
        protected override void SaveData()
        {
            var oData = new MappSetting
            {
                ID = iID,
                DeptID = ucDept.DeptID,
                MappName = ddlMappSetting.SelectedValue,
                IsEnabled = ddlEnabled.SelectedValue == "1"
            };

            oData.ID = dDB.Execute(cyc.DB.Shared.GetEditSQL("WorkCheckMappSetting", "DeptID,MappName,IsEnabled;;ID", iID == 0), oData, oData.ID);
        }

        class MappSetting
        {
            public int ID { get; set; }
            public int DeptID { get; set; }
            public string MappName { get; set; }
            public bool IsEnabled { get; set; }
        }
    }
}