using cyc.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp._report
{
    /// <summary>
    /// InxCentralReportUpdate 的摘要描述
    /// </summary>
    public class InxCentralReportUpdate : cyc.Page.BaseHandler
    {
        protected override void DoHandler(HttpContext context)
        {
            if (oResult.Success)
            {
                RcvData rData = DeserializeObject<RcvData>(context.Request.Params[0]);

                if (oResult.Success && rData != null && !string.IsNullOrEmpty(rData.XT))
                {
                    if (string.IsNullOrEmpty(rData.XA) || context.Session[rData.XT]?.ToString() != rData.XA)
                        oResult.Error("認證錯誤");
                    else
                    {
                        if (rData.XL.Any())
                        {
                            UserInfo oUser = (UserInfo)context.Session["uid"];
                            using (cyc.DB.SqlDapperConn oDB = new cyc.DB.SqlDapperConn(oResult, cyc.DB.ConnString.Report))
                            {
                                foreach (var data in rData.XL.Where(p => p.C > 0 && p.Y >= 2000 && p.M > 0 && p.M < 23)) //M=21 => SUM   M=22 => AVG
                                {
                                    var qData = oDB.QueryOne<RptData>($@"
select A.ID as CategoryID,A.DataType,B.SEQ_ID,ISNULL(B.[Year],@Year)as [Year],ISNULL(B.[Month],@Month)as [Month],B.ValueNum,B.ValueStr
from ReportCategory A left join {rData.XT} B on A.ID=B.CategoryID and B.[Year]=@Year and B.[Month]=@Month
where A.ID=@CategoryID", new { CategoryID = data.C, Year = data.Y, Month = data.M });

                                    if (qData != null)
                                    {
                                        qData.UpdUser = oUser.User.ID;

                                        if (qData.DataType == "i")
                                        {
                                            if (!string.IsNullOrEmpty(data.V) && decimal.TryParse(data.V, out decimal d)) data.D = d;
                                            qData.ValueNum = data.D;
                                        }
                                        else
                                            qData.ValueStr = data.V;

                                        if (qData.SEQ_ID == 0)
                                            oDB.Execute($"insert into {rData.XT} (CategoryID,Year,Month,ValueNum,ValueStr,UpdUser,UpdTime) values (@CategoryID,@Year,@Month,@ValueNum,@ValueStr,@UpdUser,getdate())", qData);
                                        else
                                            oDB.Execute($"update {rData.XT} set {(qData.DataType == "i" ? "ValueNum=@ValueNum" : "ValueStr=@ValueStr")},UpdUser=@UpdUser,UpdTime=getdate() where SEQ_ID=@SEQ_ID", qData);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            context.Response.Write(SerializeObject(oResult));
        }

        protected override BaseHandlerOption SetBaseOption()
        {
            return new BaseHandlerOption { Session = true };
        }

        class RcvData
        {
            public string XA { get; set; }
            public List<Data> XL { get; set; }
            public string XT { get; set; }
        }

        class Data
        {
            public int C { get; set; }
            public int Y { get; set; }
            public int M { get; set; }
            public string V { get; set; }
            public decimal? D { get; set; }
        }

        class RptData
        {
            public int CategoryID { get; set; }
            public string DataType { get; set; }

            public int SEQ_ID { get; set; }
            public int Year { get; set; }
            public int Month { get; set; }
            public decimal? ValueNum { get; set; }
            public string ValueStr { get; set; }
            public int UpdUser { get; set; }
        }
    }
}