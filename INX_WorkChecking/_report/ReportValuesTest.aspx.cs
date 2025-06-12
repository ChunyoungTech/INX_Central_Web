using cyc.Data;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.DynamicData;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._report
{
    public partial class ReportValuesTest : System.Web.UI.Page
    {
        ExeResult oResult = new ExeResult();
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            using (cyc.DB.SqlDapperConn oDB = new cyc.DB.SqlDapperConn(oResult, cyc.DB.ConnString.Report))
            {
                var oData = oDB.QueryOne<dynamic>("select top 1 * from FactoryExpenseAnalysis");
                if (oResult.Success && oData == null)
                {
                    var cList = oDB.QueryList<int>("select ID from ReportCategory where Report=1");
                    System.Data.DataTable oDT = oDB.QueryDataTable("select CategoryID,Year,Month,ValueNum,UpdTime,UpdUser from FactoryExpenseAnalysis where 1=0");
                    if (oResult.Success && cList != null && cList.Any())
                    {
                        for (int iYear = 2024; iYear < 2050; iYear++)
                        {
                            for (int iMonth = 1; iMonth < 13; iMonth++)
                            {
                                Random oRnd = new Random(DateTime.Now.Millisecond * iYear / iMonth);
                                foreach (int iCate in cList)
                                {
                                    oDT.Rows.Add(new object[] { iCate, iYear, iMonth, oRnd.Next(111, 999), DateTime.Now, 1 });
                                }
                            }
                        }

                        List<SqlBulkCopyColumnMapping> oMapping = new List<SqlBulkCopyColumnMapping>
                        {
                            new SqlBulkCopyColumnMapping("CategoryID", "CategoryID"),
                            new SqlBulkCopyColumnMapping("Year", "Year"),
                            new SqlBulkCopyColumnMapping("Month", "Month"),
                            new SqlBulkCopyColumnMapping("ValueNum", "ValueNum"),
                            new SqlBulkCopyColumnMapping("UpdTime", "UpdTime"),
                            new SqlBulkCopyColumnMapping("UpdUser", "UpdUser")
                        };
                        oDB.BulkCopy(oDT, "dbo.FactoryExpenseAnalysis", oMapping);
                    }
                }
            }
            Label1.Text = $"{oResult.Success}，{oResult.Message}";
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            //using (cyc.DB.SqlDapperConn oDB = new cyc.DB.SqlDapperConn(oResult, cyc.DB.ConnString.Report))
            //{
            //    oDB.Execute("truncate table FactoryExpenseAnalysis");
            //}
            //Label1.Text = $"{oResult.Success}，{oResult.Message}";
        }
    }
}