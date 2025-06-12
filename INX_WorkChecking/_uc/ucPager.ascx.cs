using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._uc
{
    public partial class ucPager : cyc.UC.ucPager
    {
        protected override DropDownList ddlPageSize { get { return ddlPageSizeN; } }

        protected override Label lblTotalCnt { get { return lblTotalCntQ; } }

        protected override Label lblTotalPage { get { return lblTotalPageN; } }

        protected override Panel pnlVisible { get { return Panel1; } }

        protected override TextBox txtToGo { get { return txtToGoN; } }

        protected void ImageButton1_Click(object sender, ImageClickEventArgs e)
        {
            RaisePagerEvent('F');
        }

        protected void ImageButton2_Click(object sender, ImageClickEventArgs e)
        {
            RaisePagerEvent('P');
        }

        protected void ImageButton3_Click(object sender, ImageClickEventArgs e)
        {
            RaisePagerEvent('N');
        }

        protected void ImageButton4_Click(object sender, ImageClickEventArgs e)
        {
            RaisePagerEvent('L');
        }

        protected void ImageButton5_Click(object sender, ImageClickEventArgs e)
        {
            RaisePagerEvent(' ');
        }

        protected void ddlPageSizeN_SelectedIndexChanged(object sender, EventArgs e)
        {
            Response.Cookies.Add(new HttpCookie("pagesize", ddlPageSizeN.SelectedValue));
            RaisePagerEvent('F');
        }
    }
}