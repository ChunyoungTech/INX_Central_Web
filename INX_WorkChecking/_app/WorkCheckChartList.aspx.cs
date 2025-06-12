using cyc.Page;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._app
{
    public partial class WorkCheckChartList : BasePageSub
    {
        DateTime DateS = DateTime.Today;
        DateTime DateE = DateTime.Today;
        string Fac = string.Empty;
        string Vendor = string.Empty;

        protected override void OnLoad(EventArgs e)
        {
            if (!IsPostBack)
            {
                DateTime.TryParse(Request.QueryString["s"], out DateS);
                DateTime.TryParse(Request.QueryString["e"], out DateE);
                Fac = Request.QueryString["f"] ?? string.Empty;
                Vendor = Request.QueryString["v"] ?? string.Empty;

            }
            base.OnLoad(e);
        }
        protected override SubPageOption SetPageOption() => new SubPageOption() { CheckOpen = "", Confirm = null, Parameter = "s,e,f,v" };
        protected override void LoadData()
        {
            if (oResult.Success)
            {
                var qList = CYCloud.WorkCheck.WorkCheckReport.GetQueryList(DateS, DateE, dDB, Fac, Vendor, Request.QueryString["t"]);
                if (qList != null && qList.Any())
                {

                    ltlContent.Text = string.Join(string.Empty, qList.Select(p => $@"
<tr>
<td><span class='con_number'>{p.con_number}</span></td>
<td>{p.con_date:yyyy-MM-dd}</td>
<td>{p.Fac}</td>
<td>{p.Vendor}</td>
<td class='td-right'>{p.AccCnt:N0}</td>
<td class='td-right'>{p.ChkCnt:N0}</td>
<td class='td-center'>{(p.T01 > 0 ? "V" : string.Empty)}</td>
<td class='td-center'>{(p.T02 > 0 ? "V" : string.Empty)}</td>
<td class='td-center'>{(p.T03 > 0 ? "V" : string.Empty)}</td>
<td class='td-center'>{(p.T04 > 0 ? "V" : string.Empty)}</td>
<td class='td-center'>{(p.T05 > 0 ? "V" : string.Empty)}</td>
<td class='td-center'>{(p.T06 > 0 ? "V" : string.Empty)}</td>
<td class='td-center'>{(p.T07 > 0 ? "V" : string.Empty)}</td>
<td class='td-center'>{(p.T08 > 0 ? "V" : string.Empty)}</td>
<td class='td-center'>{(p.T09 > 0 ? "V" : string.Empty)}</td>
<td class='td-center'>{(p.T10 > 0 ? "V" : string.Empty)}</td>
<td class='td-center'>{(p.T11 > 0 ? "V" : string.Empty)}</td>
</tr>"));
                    //GridView1.DataSource = qList;
                    //GridView1.DataBind();
                }
                else
                    ltlContent.Text = "<tr><td colspan='17'><div class='NoData'>查無符合條件資料</div></td></tr>";
            }
        }
        protected override void SaveCheck()
        {

        }
        protected override void SaveData()
        {

        }

        //protected void GridView1_DataBound(object sender, EventArgs e)
        //{
        //    GridView1.HeaderRow.TableSection = TableRowSection.TableHeader;
        //}
    }
}