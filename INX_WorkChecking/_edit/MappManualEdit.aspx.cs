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
    public partial class MappManualEdit : BasePageSub
    {
        protected override SubPageOption SetPageOption() => new SubPageOption()
        {
            CheckOpen = "open.aspx;MappManualEdit.aspx",
            Confirm = btnConfirm,
            Parameter = "pa",
            SuccessMsg = "新增完成"
        };
        protected override void LoadData()
        {
            ViewState["ID"] = Request.QueryString["pa"];
            if (Request.QueryString["pa"] != "0")
            {
                using (cyc.DB.SqlDapperConn oDB = new cyc.DB.SqlDapperConn(null, cyc.DB.ConnString.MApp))
                {
                    var data = oDB.QueryOne<CYCloud.Mapp.Data.MappManual>("select * from MApp_Table where SEQ_ID=@ID", new { ID = Request.QueryString["pa"] });
                    if (data != null)
                    {
                        txtPlant.Text = data.MApp_Plant ?? "";
                        txtProvider.Text = data.MApp_Provider ?? "";
                        txtValue1.Text = data.MApp_Value1 ?? "";
                        txtValue2.Text = data.MApp_Value2 ?? "";
                        txtValue3.Text = data.MApp_Value3 ?? "";
                        txtSecond.Text = data.MApp_Sec.ToString();
                        lblAck_Flag.Text = data.MApp_Ack_Flag == 'N' ? "否" : "是";
                        lblDate.Text = (data.MApp_Date ?? "") + " " + (data.MApp_Time ?? "");
                        //lblTime.Text = data.MApp_Time ?? "";
                        txtType.Text = data.MApp_Type ?? "";
                    }
                    else
                        oResult.Error("查無資料");
                }
                btnConfirm.Visible = false;
            }
        }
        protected override void SaveCheck()
        {
            if (!cyc.Shared.Check.IsInteger(txtSecond.Text, true))
            {
                oResult.Error("[秒數]必須是大於0的整數");
            }
        }
        protected override void SaveData()
        {
            if (ViewState["ID"].ToString() == "0")
            {
                var data = new CYCloud.Mapp.Data.MappManual()
                {
                    MApp_Plant = txtPlant.Text.Trim(),
                    MApp_Provider = txtProvider.Text.Trim(),
                    MApp_Value1 = txtValue1.Text.Trim(),
                    MApp_Value2 = txtValue2.Text.Trim(),
                    MApp_Value3 = txtValue3.Text.Trim(),
                    MApp_Date = DateTime.Now.ToString("yyyy/MM/dd"),
                    MApp_Time = DateTime.Now.ToString("HH:mm:ss"),
                    MApp_Ack_Flag = 'N',
                    MApp_Sec = Convert.ToInt16(txtSecond.Text),
                    MApp_Type = txtType.Text.Trim(),
                    MApp_No = ""
                };
                using (cyc.DB.SqlDapperConn oDB = new cyc.DB.SqlDapperConn(null, cyc.DB.ConnString.MApp))
                {
                    try
                    {
                        oDB.Execute(@"
insert into MApp_Table (MApp_No,MApp_Type,MApp_Sec,MApp_Ack_Flag,MApp_Time,MApp_Date,MApp_Value3,MApp_Value2,MApp_Value1,MApp_Provider,MApp_Plant)
values (@MApp_No,@MApp_Type,@MApp_Sec,@MApp_Ack_Flag,@MApp_Time,@MApp_Date,@MApp_Value3,@MApp_Value2,@MApp_Value1,@MApp_Provider,@MApp_Plant)", data);
                    }
                    catch (Exception ex) { cyc.Log.WriteSysErrorLog("MApp手動發送:" + ex.Message, oResult); }
                }
            }
        }
    }
}