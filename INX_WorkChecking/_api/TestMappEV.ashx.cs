using cyc.Page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CYCloud.MappEV.Data;

namespace WebApp._api
{
    /// <summary>
    /// TestMappEV 的摘要描述
    /// </summary>
    public class TestMappEV : BaseHandler
    {
        protected override void DoHandler(HttpContext context)
        {
            MappEVInput oInput = this.DeserializeObject<MappEVInput>(context.Request.Params[0]);
            if (oResult.Success && oInput != null)
            { 
                using (var oDB = new cyc.DB.SqlDapperConn(oResult))
                {
                    oDB.Execute("insert into MappEVInput (FacName,Type,Value1,Value2,Value3,Value4,Value5,InputSource) values (@FacName,@Type,@Value1,@Value2,@Value3,@Value4,@Value5,@InputSource)", oInput);
                }
            }
            context.Response.Write(this.SerializeObject(oResult));
        }

        protected override BaseHandlerOption SetBaseOption()
        {
            return new BaseHandlerOption() { NoCache = true, Session = false };
        }
    }
}