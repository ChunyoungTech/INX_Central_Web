using cyc.Data;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp._report
{
    /// <summary>
    /// FactoryExpenseAnalysis1 的摘要描述
    /// </summary>
    public class FactoryExpenseAnalysis1 : cyc.Page.BaseHandler
    {
        protected override void DoHandler(HttpContext context)
        {
            if (oResult.Success)
            {
                if (context.Session["AuthKey"] == null)
                    oResult.Error("認證錯誤");
                else
                {
                    RcvData rData = null;
                    try
                    {
                        rData = DeserializeObject<RcvData>(context.Request.Params[0]);
                    }
                    catch { oResult.Error("參數錯誤"); }

                    if (oResult.Success)
                    {
                        if (context.Session["AuthKey"].ToString() != rData.xAuth)
                            oResult.Error("認證錯誤");
                        else
                        {
                            if (rData.xList.Any())
                            {
                                UserInfo oUser = (UserInfo)context.Session["uid"];
                                using (cyc.DB.SqlDapperConn oDB = new cyc.DB.SqlDapperConn(oResult, cyc.DB.ConnString.Report))
                                {
                                    foreach (var x in rData.xList)
                                    {
                                        int[] ym = x.m.Split(',').Where(p => cyc.Shared.Check.IsInteger(p)).Select(p => Convert.ToInt32(p)).ToArray();
                                        if (ym.Length == 2 && ym[0] >= 2024 && ym[1] > 0 && ym[1] < 13)
                                        {
                                            if (!string.IsNullOrEmpty(x.v) && decimal.TryParse(x.v, out decimal d)) x.d = d;

                                            var qData = oDB.QueryOne<RptData>("select SEQ_ID,CategoryID,Year,Month,ValueNum from FactoryExpenseAnalysis where CategoryID=@Cate and Year=@Year and Month=@Month", new { Cate = x.c, Year = ym[0], Month = ym[1] });
                                            if (qData == null)
                                            {
                                                qData = new RptData { CategoryID = x.c, Year = ym[0], Month = ym[1], ValueNum = x.d, UpdUser = oUser.User.ID };
                                                oDB.Execute("insert into FactoryExpenseAnalysis (CategoryID,Year,Month,ValueNum,UpdUser,UpdTime) values (@CategoryID,@Year,@Month,@ValueNum,@UpdUser,getdate())", qData);
                                            }
                                            else if (qData.ValueNum != x.d)
                                            {
                                                qData.ValueNum = x.d;
                                                qData.UpdUser = oUser.User.ID;
                                                qData.UpdTime = DateTime.Now;
                                                oDB.Execute("update FactoryExpenseAnalysis set CategoryID=@CategoryID,Year=@Year,Month=@Month,ValueNum=@ValueNum,UpdUser=@UpdUser,UpdTime=getdate() where SEQ_ID=@SEQ_ID", qData);
                                            }
                                        }
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
            public string xAuth { get; set; }
            public List<Data> xList { get; set; }
        }

        class Data
        {
            public int c { get; set; }
            public string m { get; set; }
            public string v { get; set; }
            public decimal? d { get; set; }
        }

        class RptData
        {
            public int SEQ_ID { get; set; }
            public int CategoryID { get; set; }
            public int Year { get; set; }
            public int Month { get; set; }
            public decimal? ValueNum { get; set; }
            public DateTime UpdTime { get; set; }
            public int UpdUser { get; set; }
        }
    }
}