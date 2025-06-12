using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace WebApp._test
{
    /// <summary>
    /// CimPost 的摘要描述
    /// </summary>
    public class CimPost : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            
            using (var reader = new StreamReader(context.Request.InputStream))
            {
                var json = reader.ReadToEnd();
            }

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