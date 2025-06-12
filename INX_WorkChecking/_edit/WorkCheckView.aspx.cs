using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using cyc.Page;

namespace WebApp._edit
{
    public partial class WorkCheckView : cyc.Page.BasePageSub
    {
        protected override SubPageOption SetPageOption() => new SubPageOption()
        {
            CheckOpen = "",
            Confirm = btnConfirm,
            Parameter = "pa",
            IsIntPa = false
        };

        protected override void LoadData()
        {
            bPara.Command = "select A.SEQ_ID,A.Remark,B.* from WORK_CHECKIN A inner join View_VMT_FAC B on A.con_number=B.con_number where A.con_number=@Number";
            bPara.Parameter.Clear();
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Number", Request.QueryString["pa"]));
            using (var oDT = dDB.QueryDataTable(bPara))
            {
                if (oResult.Success)
                {
                    if (oDT.Rows.Count > 0)
                    {
                        var row = oDT.Rows[0];
                        labTime.Text = Convert.ToDateTime(row["BEGIN_TIME"]).ToString("HH:mm") + " - " + Convert.ToDateTime(row["END_TIME"]).ToString("HH:mm");
                        lblDate.Text = Convert.ToDateTime(row["con_date"]).ToString("yyyy/MM/dd");
                        labPROJECT.Text = row["PROJECT_NO"].ToString();
                        lblNumber.Text = row["con_number"].ToString();
                        lblFAC.Text = row["fac_name"].ToString();
                        lblFAB.Text = row["fab_name"].ToString();
                        lblMainArea.Text = row["main_area"].ToString();
                        lblSecondArea.Text = row["second_area"].ToString();
                        lblVendor.Text = row["vendor_name"].ToString();
                        lblType1.Text = row["type1"].ToString();
                        lblType2.Text = row["type2"].ToString();
                        lblType3.Text = row["type3"].ToString();
                        lblType4.Text = row["type4"].ToString();
                        lblType5.Text = row["type5"].ToString();
                        lblType6.Text = row["type6"].ToString();
                        lblType7.Text = row["type7"].ToString();
                        lblContent.Text = row["con_conten"].ToString();
                        lblEngineer.Text = row["engineer"].ToString();
                        lblVendorPE.Text = row["vendor_pe"].ToString();
                        labSAFE_NAME.Text = row["SAFE_NAME"].ToString();

                        hidID.Value = row["SEQ_ID"].ToString();
                        txtRemark.Text = row["Remark"].ToString();
                    }
                    else
                        oResult.Error("查無資料");
                }
            }
        }

        protected override void SaveData()
        {

        }
    }
}