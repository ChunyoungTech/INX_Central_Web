using cyc.Data;
using cyc.Page;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace WebApp._idb
{
    public partial class IDBHistorianUpload : BasePageGrid
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (!IsPostBack)
            {
                if (bUser != null)
                {
                    var oList = dDB.QueryList<BaseObj>(@"
select distinct C.SeqID as ID,C.FacName as Code from (
	select distinct ID3 from View_SysDeptLevel where ID1=@ID or ID2=@ID or ID3=@ID or ID4=@ID or ID5=@ID
)A inner join View_SysDeptLevel B on A.ID3=B.ID inner join IDBFacData C on B.Code=C.FacName", new { ID = bUser.User.DeptLevel });

                    ddlFactory.DataSource = oList;
                    ddlFactory.DataBind();
                    ddlFactory.Items.Insert(0, "");

                    //lstFactory.DataSource = oList;
                    //lstFactory.DataBind();

                    LoadSysData();
                }
            }
        }

        protected override void QueryCheck(int idx)
        {
        }

        protected override DataTable QuerySourceData(int idx)
        {
            bPara.Command = $@"
select ISNULL(D.SeqID,0)as SeqID,A.ID as TagID,A.Tag_Name as TagName,A.Tag_Desc as TagDesc,C.FacName,B.mesurement as SysName,ISNULL(D.Enable,0)as [Enabled],D.LastTime
,case when ISNULL(D.LastTime,'2020/1/1')<@Time then 1 else 0 end as IsRed
from TagData A
inner join IDBSysMapping B on A.TagSys=B.SeqID
inner join IDBFacData C on B.IDBFacDataID=C.SeqID
left join IDBHistorianUpload D on A.ID=D.TagID
where C.SeqID in @FacList
{(string.IsNullOrEmpty(ddlFactory.SelectedValue) ? string.Empty : "and C.SeqID=@Fac")}
{(string.IsNullOrEmpty(ddlSystem.SelectedValue) ? string.Empty : "and B.mesurement=@Sys")}
{(string.IsNullOrWhiteSpace(txtTagName.Text) ? string.Empty : "and A.Tag_Name like @Tag")}
{(chkOnly.Checked ? "and (ISNULL(D.LastTime,'2020/1/1')<@Time and ISNULL(D.Enable,0)=1)" : string.Empty)}";

            var oObj = new { Time = DateTime.Now.AddHours(-1), Fac = ddlFactory.SelectedValue, Sys = ddlSystem.SelectedValue, Tag = $"%{txtTagName.Text.Trim()}%", FacList = ddlFactory.Items.Cast<ListItem>().Where(p => !string.IsNullOrEmpty(p.Value)).Select(p => p.Value) };
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

                //lstSystem.DataSource = oList;
                //lstSystem.DataBind();
            }
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            foreach(GridViewRow row in GridView1.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    if (row.FindControl("lblSeqID") is Label lblSeqID && row.FindControl("lblTagID") is Label lblTagID && row.FindControl("chkEnabled") is CheckBox chkEnabled)
                    {
                        var uData = new UploadData { SeqID = Convert.ToInt32(lblSeqID.Text), TagID = Convert.ToInt32(lblTagID.Text), Enable = chkEnabled.Checked };

                        if (uData.SeqID == 0)
                        {
                            uData.SeqID = dDB.Execute("insert into IDBHistorianUpload (TagID,Enable) values (@TagID,@Enable)", uData, 0);
                            if (dDB.Result.Success) lblSeqID.Text = uData.SeqID.ToString();
                        }
                        else
                        {
                            dDB.Execute("update IDBHistorianUpload set Enable=@Enable where SeqID=@SeqID and Enable<>@Enable", uData);
                        }
                    }
                }
            }
            BindGridView(0);
        }

        class UploadData
        {
            public int SeqID { get; set; }
            public int TagID { get; set; }
            public bool Enable { get; set; }
            public DateTime? LastTime { get; set; }
            public bool IsRed { get; set; }
        }

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //if (new Random(DateTime.Now.Millisecond).Next(0, 10) > 5)
                //    e.Row.BackColor = System.Drawing.Color.LightPink;

                DataRowView row = (DataRowView)e.Row.DataItem;
                if (Convert.ToBoolean(row["IsRed"]) && Convert.ToBoolean(row["Enabled"]))
                    e.Row.BackColor = System.Drawing.Color.LightPink;
            }
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            using (DataTable oDT = QuerySourceData(0))
            {
                if (oDT != null) 
                {
                    Response.Clear();
                    Response.AddHeader("Content-Disposition", "attachment;filename=" + System.Web.HttpUtility.UrlEncode("ImfluxDB上傳點位設定") + ".csv");
                    Response.ContentType = "application/octet-stream";

                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(Response.OutputStream, System.Text.Encoding.UTF8))
                    {
                        sw.WriteLine("廠別,系統別,點位名稱,點位描述,資料時間,上傳");

                        if (oDT.Rows.Count > 0)
                        {
                            DataView dv = oDT.DefaultView;
                            dv.Sort = GetSort(0);
                            for (int idx = 0; idx < dv.Count; idx++)
                            {
                                sw.WriteLine($"{dv[idx]["FacName"]},{dv[idx]["SysName"]},{dv[idx]["TagName"]},{dv[idx]["TagDesc"].ToString().Replace(",", "，")},{dv[idx]["LastTime"]},{dv[idx]["Enabled"]}");
                            }
                        }

                        sw.Flush();
                        Response.Flush();
                        Response.End();
                    }
                }
            }
        }

        //protected void lstFactory_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    var IDs = lstFactory.Items.Cast<ListItem>().Where(p => p.Selected).Select(p => p.Value).Where(p => cyc.Shared.Check.IsInteger(p)).Select(p => Convert.ToInt32(p));

        //}
    }
}