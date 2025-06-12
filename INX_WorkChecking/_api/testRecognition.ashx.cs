using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using cyc.Page;

namespace WebApp._api
{
    /// <summary>
    /// testRecognition 的摘要描述
    /// </summary>
    public class testRecognition : cyc.Page.BaseHandler
    {
        protected override void DoHandler(HttpContext context)
        {
            //using (var sr = new System.IO.StreamReader(context.Request.InputStream))
            //{
            //    var q = this.DeserializeObject<RequestData>(sr.ReadToEnd());
            //}

            System.Threading.Thread.Sleep(new Random((int)DateTime.Now.TimeOfDay.Ticks).Next(100, 500));

            context.Response.Write(this.SerializeObject(new CYCloud.IFP.RecognitionAuthResult()
            {
                Result = "Success",
                Code = "XX-000",
                //Message = "",
                Logs = CYCloud.IFP.RecognitionSimulation.List.Where(p => p.LogDateTime > DateTime.Now.AddSeconds(-30)).ToList()
            }));
        }

        protected override BaseHandlerOption SetBaseOption()
        {
            return new BaseHandlerOption() { NoCache = true, Session = false };
        }

        private class RequestData
        {
            public string DeviceName { get; set; }
            public DateTime StartDateTime { get; set; }
            public DateTime EndDateTime { get; set; }
        }
    }
}