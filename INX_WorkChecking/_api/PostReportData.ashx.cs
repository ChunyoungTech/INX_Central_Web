using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp._api
{
    /// <summary>
    /// PostReportData 的摘要描述
    /// </summary>
    public class PostReportData : cyc.Page.BaseHandler
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
        }
    }
}