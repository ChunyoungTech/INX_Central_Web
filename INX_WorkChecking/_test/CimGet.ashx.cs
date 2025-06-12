using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp._test
{
    /// <summary>
    /// CimGet 的摘要描述
    /// </summary>
    public class CimGet : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.Write("Hello World");
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}