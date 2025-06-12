using cyc.Data;
using cyc.Page;
using Microsoft.AspNet.SignalR.Hosting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace WebApp._idb
{
    public partial class TagData : BasePageGrid
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (!IsPostBack)
            {
                if (bUser != null)
                {
                    ddlFactory.DataSource = dDB.QueryList<BaseObj>(@"
select distinct C.SeqID as ID,C.FacName as Code from (
	select distinct ID3 from View_SysDeptLevel where ID1=@ID or ID2=@ID or ID3=@ID or ID4=@ID or ID5=@ID
)A inner join View_SysDeptLevel B on A.ID3=B.ID inner join IDBFacData C on B.Code=C.FacName", new { ID = bUser.User.DeptLevel });
                    ddlFactory.DataBind();
                    ddlFactory.Items.Insert(0, "");

                    LoadSysData();
                }
            }
        }

        protected override void QueryCheck(int idx)
        {
        }

        protected override DataTable QuerySourceData(int idx)
        {
            bPara.Command = $@"select ID,Tag_Name,Tag_Desc,Unit,Tag_Type,C.FacName,B.mesurement as SysName,HiHi_Limit,Hi_Limit,Lo_Limit,LoLo_Limit from TagData A
inner join IDBSysMapping B on A.TagSys=B.SeqID inner join IDBFacData C on B.IDBFacDataID=C.SeqID where C.SeqID in @FacList
{(string.IsNullOrEmpty(ddlFactory.SelectedValue) ? string.Empty : "and C.SeqID=@Fac")}
{(string.IsNullOrEmpty(ddlSystem.SelectedValue) ? string.Empty : "and B.mesurement=@Sys")}
{(string.IsNullOrWhiteSpace(txtTagName.Text) ? string.Empty : "and A.Tag_Name like @Tag")}";

            var oObj = new { Fac = ddlFactory.SelectedValue, Sys = ddlSystem.SelectedValue, Tag = $"%{txtTagName.Text.Trim()}%", FacList = ddlFactory.Items.Cast<ListItem>().Where(p => !string.IsNullOrEmpty(p.Value)).Select(p => p.Value) };
            return dDB.QueryDataTable(bPara.Command, oObj);
        }

        protected override GridPageOption SetPageSetting()
        {
            return new GridPageOption() { GridOption = new GridOption[] { new GridOption { AutoBind = true, Grid = GridView1, Pager = ucPager, Query = btnQuery } } };
        }

        protected void ddlFactory_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadSysData();
        }

        private void LoadSysData()
        {
            IEnumerable<string> oList = null;
            if (string.IsNullOrEmpty(ddlFactory.SelectedValue))
                oList = dDB.QueryList<string>("select distinct mesurement from IDBSysMapping where IDBFacDataID in @ID order by mesurement", new { ID = ddlFactory.Items.Cast<ListItem>().Where(p => !string.IsNullOrEmpty(p.Value)).Select(p => Convert.ToInt32(p.Value)) });
            else
                oList = dDB.QueryList<string>("select distinct mesurement from IDBSysMapping where IDBFacDataID=@ID order by mesurement", new { ID = Convert.ToInt32(ddlFactory.SelectedValue) });

            if (oList != null)
            {
                ddlSystem.DataSource = oList;
                ddlSystem.DataBind();
                ddlSystem.Items.Insert(0, "");
            }
        }
    }
}