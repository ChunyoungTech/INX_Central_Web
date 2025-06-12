using cyc.Page;
using CYCloud.WorkCheck;
using Microsoft.AspNet.SignalR.Hosting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace WebApp._test
{
    public partial class WorkCheckTest : BasePageGrid
    {
        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            if (bUser != null && bUser.From == 1) this.MasterPageFile = "~/_master/Vendor.Master";
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (!IsPostBack)
            {
                dteCreate.Value = DateTime.Today;
            }
        }

        protected override void QueryCheck(int idx)
        {
            if (dteCreate.Value == null)  oResult.Error("[工單日期]格式錯誤");
        }

        protected override DataTable QuerySourceData(int idx)
        {
            bPara.Command = string.Format(@"
with CTE as (
	select A.con_number,B.ID,B.EV_DATE,B.Direction
	from View_VMT_FAC A 
	inner join AccessList2 B on A.con_number=B.APPLY_PK
	inner join SysUser E on replace(SUBSTRING(A.engineer,0,5),'(','') = E.Name
	where A.fac_code=@FacCode and A.con_date=@Date
)

select A.con_number,A.con_date,A.fac_name,A.vendor_name,B.SEQ_ID,B.checkin_time,B.checkout_time
,isnull(C.CHECKIN,0)as checkin_count,isnull(C.CHECKOUT,0)as checkout_count
,case when B.checkin_time is null then 0 else 1 end as CheckOut
,(Select Count(distinct ID) From CTE Where con_number = A.con_number and Direction = 1 ) as 哨口進廠人數
,(Select Count(distinct ID) From CTE Where con_number = A.con_number and Direction = 0 ) as 哨口出廠人數
from View_VMT_FAC A left join WORK_CHECKIN B on A.con_number=B.con_number 
inner join SysUser E on replace(SUBSTRING(A.engineer,0,5),'(','') = E.Name
left join (
	select * from (
		select SEQ_ID,WORK_CHECKIN_ID,CHECK_TYPE from WORK_CHECKIN_LOG where WORK_CHECKIN_ID in (
			select B.SEQ_ID from View_VMT_FAC A left join WORK_CHECKIN B on A.con_number=B.con_number
			where A.fac_code=@FacCode and A.con_date=@Date
		)
	) X PIVOT (
	    COUNT(SEQ_ID)
	    FOR CHECK_TYPE IN ([CHECKIN], [CHECKOUT])
    ) p
) C on B.SEQ_ID=C.WORK_CHECKIN_ID where A.fac_code=@FacCode and A.con_date=@Date order by A.con_date,A.fac_name");

            bPara.Parameter.Clear();
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Date", dteCreate.Value));
            bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("FacCode", ddlFactory.SelectedItem.Text));
            return dDB.QueryDataTable(bPara);
        }

        protected override GridPageOption SetPageSetting()
        {
            return new GridPageOption() { CheckOpen = "", GridOption = new GridOption[] { new GridOption { AutoBind = true, Grid = GridView1, Pager = ucPager, Query = btnQuery } } };
        }

        protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "Checkin":
                    try
                    {
                        DoCheckin(e.CommandArgument.ToString());
                    }
                    catch (Exception ex) { oResult.Error(ex.Message); }
                    if (oResult.Success) BindGridView(0);
                    ShowResult("新增[報到]完成");
                    break;
                case "Checkout":
                    try
                    {
                        DoCheckout(e.CommandArgument.ToString());
                    }
                    catch (Exception ex) { oResult.Error(ex.Message); }
                    if (oResult.Success) BindGridView(0);
                    ShowResult("新增[報退]完成");
                    break;
                default:
                    break;
            }
        }

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            try
            {
                DoCreate();
                if (oResult.Success) BindGridView(0);
                ShowResult("新增[工單]完成");
            }
            catch (Exception ex) { oResult.Error(ex.Message); }
        }

        private void DoCreate()
        {
            if (dteCreate.Value != null)
            {
                using (var oDB = new cyc.DB.SqlDapperConn(oResult, cyc.Shared.SysQuery.GetConnectString("Vendor")))
                {
                    var oList = oDB.QueryList<VM_FAC>($"select * from VM_{ddlFactory.SelectedItem.Text} A inner join VM_DAILY_FAC B ON A.con_number = B.APPLY_PK where A.con_date=@Date", new { Date = new DateTime(2024, 3, 11) });
                    if (oList != null && oList.Count() > 0)
                    {
                        var uList = dDB.QueryList<cyc.Data.BaseObj>("select B.Code,B.Name from WorkCheckMappSetting A inner join SysUser B on A.DeptID=B.DeptID where A.IsEnabled=1 and B.Enabled=1");

                        Random oRnd = new Random(DateTime.Now.Second);

                        VM_FAC oData = null;
                        bool IsFound = false;

                        int qCnt = 0;
                        while (!IsFound && qCnt < oList.Count())
                        {
                            oData = oList.ToList()[oRnd.Next(0, oList.Count() - 1)];

                            if (uList.Any(p => p.Code == oData.eng_NO)) IsFound = true;

                            //int q = dDB.QueryOne<int>("select count(1) from SysUser where Code=@Code", new { Code = oData.eng_NO });
                            //if (q > 0)
                            //{
                            //    IsFound = true;
                            //    iCount = oDB.QueryOne<int>($"select count(1) from VM_{ddlFactory.SelectedItem.Text} where con_date=@Date", new { Date = dteCreate.Value });
                            //}
                            qCnt++;
                        }

                        if (oResult.Success && IsFound)
                        {
                            int iCount = oDB.QueryOne<int>($"select count(1) from VM_{ddlFactory.SelectedItem.Text} where con_date=@Date", new { Date = dteCreate.Value });

                            oData.con_number = $"{((DateTime)dteCreate.Value):yyyyMMdd}{ddlFactory.SelectedValue}{(iCount + 1):000}";
                            oData.APPLY_PK = oData.con_number;

                            oData.con_date = (DateTime)dteCreate.Value;
                            oData.BEGIN_TIME = oData.con_date.AddHours(9);
                            oData.END_TIME = oData.con_date.AddHours(18);

                            oDB.Execute($"insert into VM_{ddlFactory.SelectedItem.Text} ({string.Join(",", xStr.Split(','))}) values ({string.Join(",", xStr.Split(',').Select(p => $"@{p}"))})", oData);
                            if (oResult.Success)
                                oDB.Execute($"insert into VM_DAILY_FAC (APPLY_PK,PROJECT_NO,BEGIN_TIME,END_TIME,SAFE_NAME,eng_NO) values (@APPLY_PK,@PROJECT_NO,@BEGIN_TIME,@END_TIME,@SAFE_NAME,@eng_NO)", oData);

                            //if (oResult.Success)
                            //    oResult.Message = oData.con_number;
                        }
                    }
                }
            }
        }

        private void DoCheckin(string con_number)
        {
            var oData = dDB.QueryOne<VM_FAC>("select * from View_VMT_FAC where con_number=@Number", new { Number = con_number });
            if (oData != null)
            {
                var xLocation = dDB.QueryOne<cyc.Data.BaseObj>("select top 1 LOCATION as Name from AccessListMapping where VNTFAC=@Fac", new { Fac = oData.fac_name });
                if (xLocation != null)
                {
                    //var qList = dDB.QueryList<string>("select distinct ID from AccessList2 where APPLY_PK=@Number and Direction=1", new { Number = oData.con_number });
                    var qList = dDB.QueryList<string>("select distinct ID from AccessList2 where EV_DATE between @TimeS and @TimeE and Direction=1", new { TimeS = oData.con_date, TimeE = oData.con_date.AddDays(1) });
                    if (qList != null)
                    {
                        bool isOK = false;
                        do
                        {
                            string sID = GetRandomID();
                            if (!qList.Any(p => p == sID))
                            {
                                var xData = new AccessList { ID = sID, APPLY_PK = oData.con_number, Direction = true, EV_DATE = DateTime.Now, P_NAME = sID, LOCATION = xLocation.Name, SHORT_NAME = oData.vendor_name };
                                dDB.Execute("insert into AccessList2 (ID,EV_DATE,Direction,APPLY_PK,P_NAME,SHORT_NAME,LOCATION) values (@ID,@EV_DATE,@Direction,@APPLY_PK,@P_NAME,@SHORT_NAME,@LOCATION)", xData);
                                isOK = true;
                            }
                        }
                        while (!isOK);
                        //int iStart = new Random(DateTime.Now.Millisecond - DateTime.Now.Second).Next(0, UserList.Count - 1);
                        //int idx = iStart;
                        //cyc.Data.BaseObj oUser = null;
                        //do
                        //{
                        //    if (!qList.Any(p => p == UserList[idx % UserList.Count].Code))
                        //    {
                        //        oUser = UserList[idx];

                        //        var xData = new AccessList { ID = oUser.Code, APPLY_PK = oData.con_number, Direction = true, EV_DATE = DateTime.Now, P_NAME = oUser.Name, LOCATION = xLocation.Name, SHORT_NAME = "???" };

                        //        dDB.Execute("insert into AccessList2 (ID,EV_DATE,Direction,APPLY_PK,P_NAME,SHORT_NAME,LOCATION) values (@ID,@EV_DATE,@Direction,@APPLY_PK,@P_NAME,@SHORT_NAME,@LOCATION)", xData);
                        //    }
                        //    idx++;
                        //}
                        //while (idx % UserList.Count != iStart && oUser == null);
                    }
                }
            }
        }

        private void DoCheckout(string con_number)
        {
            var oData = dDB.QueryOne<VM_FAC>("select * from View_VMT_FAC where con_number=@Number", new { Number = con_number });
            if (oData != null)
            {
                var qList = dDB.QueryList<AccessList>("select * from AccessList2 where APPLY_PK=@Number", new { Number = oData.con_number });
                foreach (var qData in qList.Where(p => p.Direction && p.EV_DATE.AddMinutes(10) < DateTime.Now))
                {
                    if (!qList.Any(p => p.ID == qData.ID && p.Direction == false && p.EV_DATE > qData.EV_DATE.AddMinutes(10)))
                    {
                        qData.Direction = false;
                        qData.EV_DATE = DateTime.Now;

                        dDB.Execute("insert into AccessList2 (ID,EV_DATE,Direction,APPLY_PK,P_NAME,SHORT_NAME,LOCATION) values (@ID,@EV_DATE,@Direction,@APPLY_PK,@P_NAME,@SHORT_NAME,@LOCATION)", qData);

                        break;
                    }
                }
            }
        }

        private string GetRandomID()
        {
            Random oRnd = new Random(DateTime.Now.Second);
            var sPrefix = xPrefix.Split(' ');
            string sID = sPrefix[oRnd.Next(0, sPrefix.Length - 1)] + oRnd.Next(1, 2).ToString();
            for (int i = 0; i < 8; i++)
                sID += oRnd.Next(0, 9).ToString();
            return sID;
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

        string xPrefix = "A B C D E F G H I J K L M N O P Q R S T U V W X Y Z";

        #endregion
    }
}