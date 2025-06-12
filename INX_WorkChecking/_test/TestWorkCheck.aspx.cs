using Microsoft.SqlServer.Server;
using NPOI.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._test
{
    public partial class TestWorkCheck : cyc.Page.BasePage
    {
        static string ConnStr { get; } = cyc.Shared.SysQuery.GetConnectString("Vendor");
        static DateTime QryDate { get; } = new DateTime(2024, 3, 11);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                dteCreate.Value = DateTime.Today;
            }
            
        }

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            if (dteCreate.Value != null)
            {
                using (var oDB = new cyc.DB.SqlDapperConn(oResult, ConnStr))
                {
                    var oList = oDB.QueryList<VM_FAC>($"select * from VM_{ddlFactory.SelectedItem.Text} A inner join VM_DAILY_FAC B ON A.con_number = B.APPLY_PK where A.con_date=@QryDate", new { QryDate });
                    if (oList != null && oList.Count() > 0)
                    {
                        Random oRnd = new Random(DateTime.Now.Millisecond - DateTime.Now.Second);

                        VM_FAC oData = null;
                        int iCount = 0;
                        bool IsFound = false;

                        using (var xDB = new cyc.DB.SqlDapperConn(oResult, ""))
                        {
                            int qCnt = 0;
                            while (!IsFound && qCnt < 1000)
                            {
                                oData = (oList.ToList())[oRnd.Next(0, oList.Count() - 1)];

                                int q = xDB.QueryOne<int>("select count(1) from SysUser where Code=@Code", new { Code = oData.eng_NO });
                                if (q > 0)
                                {
                                    IsFound = true;
                                    iCount = oDB.QueryOne<int>($"select count(1) from VM_{ddlFactory.SelectedItem.Text} where con_date=@Date", new { Date = dteCreate.Value });
                                }
                                qCnt++;
                            }
                        }

                        if (oResult.Success && IsFound)
                        {
                            oData.con_number = $"{((DateTime)dteCreate.Value):yyyyMMdd}{ddlFactory.SelectedValue}{(iCount + 1):000}";
                            oData.APPLY_PK = oData.con_number;

                            oData.con_date = (DateTime)dteCreate.Value;
                            oData.BEGIN_TIME = oData.con_date.AddHours(9);
                            oData.END_TIME = oData.con_date.AddHours(18);

                            oDB.Execute($"insert into VM_{ddlFactory.SelectedItem.Text} ({string.Join(",", xStr.Split(','))}) values ({string.Join(",", xStr.Split(',').Select(p => $"@{p}"))})", oData);
                            if (oResult.Success)
                                oDB.Execute($"insert into VM_DAILY_FAC (APPLY_PK,PROJECT_NO,BEGIN_TIME,END_TIME,SAFE_NAME,eng_NO) values (@APPLY_PK,@PROJECT_NO,@BEGIN_TIME,@END_TIME,@SAFE_NAME,@eng_NO)", oData);

                            if (oResult.Success)
                                oResult.Message = oData.con_number;
                        }
                    }
                }

                txtCreate.Text = oResult.Message;
            }
        }

        protected void btnCheckin_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtConNumberIn.Text))
            {
                using (var oDB = new cyc.DB.SqlDapperConn(oResult))
                {
                    var oData = oDB.QueryOne<VM_FAC>("select * from View_VMT_FAC where con_number=@Number", new { Number = txtConNumberIn.Text });
                    if (oData != null)
                    {
                        var xLocation = oDB.QueryOne<cyc.Data.BaseObj>("select top 1 LOCATION as Name from AccessListMapping where VNTFAC=@Fac", new { Fac = oData.fac_name });
                        if (xLocation != null)
                        {
                            var qList = oDB.QueryList<string>("select distinct ID from AccessList2 where APPLY_PK=@Number and Direction=1", new { Number = oData.con_number });
                            if (qList != null)
                            {
                                int iStart = new Random(DateTime.Now.Millisecond - DateTime.Now.Second).Next(0, UserList.Count - 1);
                                int idx = iStart;
                                cyc.Data.BaseObj oUser = null;
                                do
                                {
                                    if (!qList.Any(p => p == UserList[idx % UserList.Count].Code))
                                    {
                                        oUser = UserList[idx];

                                        var xData = new AccessList { ID = oUser.Code, APPLY_PK = oData.con_number, Direction = true, EV_DATE = DateTime.Now, P_NAME = oUser.Name, LOCATION = xLocation.Name, SHORT_NAME = "???" };

                                        oDB.Execute("insert into AccessList2 (ID,EV_DATE,Direction,APPLY_PK,P_NAME,SHORT_NAME,LOCATION) values (@ID,@EV_DATE,@Direction,@APPLY_PK,@P_NAME,@SHORT_NAME,@LOCATION)", xData);
                                    }
                                    idx++;
                                }
                                while (idx % UserList.Count != iStart && oUser == null);
                            }
                        }
                    }
                }
                txtCheckin.Text = $"{oResult.Success}-{oResult.Message}";
            }
        }

        protected void btnCheckout_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtConNumberOut.Text))
            {
                using (var oDB = new cyc.DB.SqlDapperConn(oResult))
                {
                    var oData = oDB.QueryOne<VM_FAC>("select * from View_VMT_FAC where con_number=@Number", new { Number = txtConNumberOut.Text });
                    if (oData != null)
                    {
                        var qList = oDB.QueryList<AccessList>("select * from AccessList2 where APPLY_PK=@Number", new { Number = oData.con_number });
                        foreach (var qData in qList.Where(p => p.Direction && p.EV_DATE.AddMinutes(10) < DateTime.Now))
                        {
                            if (!qList.Any(p => p.ID == qData.ID && p.Direction == false && p.EV_DATE > qData.EV_DATE.AddMinutes(10)))
                            {
                                qData.Direction = false;
                                qData.EV_DATE = DateTime.Now;

                                oDB.Execute("insert into AccessList2 (ID,EV_DATE,Direction,APPLY_PK,P_NAME,SHORT_NAME,LOCATION) values (@ID,@EV_DATE,@Direction,@APPLY_PK,@P_NAME,@SHORT_NAME,@LOCATION)", qData);

                                break;
                            }
                        }
                    }
                }
                txtCheckout.Text = $"{oResult.Success}-{oResult.Message}";
            }
        }


        #region Data

        string xStr = "con_number,con_date,fac_name,fab_name,main_area,second_area,vendor_name,type1,con_conten,engineer,vendor_pe,peo_number";


        static List<cyc.Data.BaseObj> UserList = new List<cyc.Data.BaseObj>()
        {
            new cyc.Data.BaseObj{ Code = "A100100100", Name = "測試一" },
            new cyc.Data.BaseObj{ Code = "B200200200", Name = "測試二" },
            new cyc.Data.BaseObj{ Code = "C123123123", Name = "測試三" },
            new cyc.Data.BaseObj{ Code = "D222333444", Name = "測試四" },
            new cyc.Data.BaseObj{ Code = "E123456789", Name = "測試五" }
        };


        class VM_FAC
        {
            public string con_number { get; set; }
            public DateTime con_date { get; set; }
            public string fac_name { get; set; }
            public string fab_name { get; set; }
            public string main_area { get; set; }
            public string second_area { get; set; }
            public string vendor_name { get; set; }
            public string project_type { get; set; }
            public string type1 { get; set; }
            public string type2 { get; set; }
            public string type3 { get; set; }
            public string type4 { get; set; }
            public string type5 { get; set; }
            public string type6 { get; set; }
            public string type7 { get; set; }
            public string con_conten { get; set; }
            public string engineer { get; set; }
            public string vendor_pe { get; set; }
            public string peo_number { get; set; }
            public TimeSpan enter_time { get; set; }
            public TimeSpan leave_time { get; set; }
            public bool enter_confirm { get; set; }
            public bool leave_confirm { get; set; }
            public int confirm_number { get; set; }
            public string remarks { get; set; }
            public string APPLY_PK { get; set; }
            public string PROJECT_NO { get; set; }
            public DateTime BEGIN_TIME { get; set; }
            public DateTime END_TIME { get; set; }
            public string SAFE_NAME { get; set; }
            public string eng_NO { get; set; }
        }

        class AccessList
        {
            public string ID { get; set; }
            public DateTime EV_DATE { get; set; }
            public bool Direction { get; set; }
            public string APPLY_PK { get; set; }
            public string P_NAME { get; set; }
            public string SHORT_NAME { get; set; }
            public string LOCATION { get; set; }
        }

        #endregion
    }
}