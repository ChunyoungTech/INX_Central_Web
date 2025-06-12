using idb.Data;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._test
{
    public partial class TestInfluxDB : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                txtTime.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
            }
        }

        protected void btnRun_Click(object sender, EventArgs e)
        {
            if (DateTime.TryParse(txtTime.Text, out DateTime tTime))
            {
                idb.Job.Shared.TestInfluxDB(tTime);
            }
            else
            {
                lblMsg.Text = "時間格式錯誤，請輸入正確的日期時間格式。";
            }
        }

        protected void btnTagName_Click(object sender, EventArgs e)
        {
            List<idb.Data.FacData> facList = null;
            using (var oDB = new cyc.DB.SqlDapperConn())
            {
                facList = oDB.QueryList<idb.Data.FacData>(@"select * from IDBFacData").ToList();
            }
            if (facList != null && facList.Count > 0)
            {
                foreach (var fac in facList)
                {
                    var oOptions = new IDBOptions { Bucket = fac.BucketName, Token = fac.BucketToken };
                    using (var oService = new idb.InfluxDB.Service(oOptions))
                    {
                        var tList = oService.GetAllTagNames();
                        if (tList != null && tList.Count > 0)
                            TextBox1.Text += string.Join(",", tList);
                    }
                }
            }   
        }

        protected void btnMeasurement_Click(object sender, EventArgs e)
        {
            List<idb.Data.FacData> facList = null;
            using (var oDB = new cyc.DB.SqlDapperConn())
            {
                facList = oDB.QueryList<idb.Data.FacData>(@"select * from IDBFacData").ToList();
            }
            if (facList != null && facList.Count > 0)
            {
                foreach (var fac in facList)
                {
                    var oOptions = new IDBOptions { Bucket = fac.BucketName, Token = fac.BucketToken };
                    using (var oService = new idb.InfluxDB.Service(oOptions))
                    {
                        var tList = oService.GetAllMeasurement();
                        if (tList != null && tList.Count > 0)
                            txtMeasurement.Text += string.Join(",", tList);
                    }
                }
            }
        }
    }
}