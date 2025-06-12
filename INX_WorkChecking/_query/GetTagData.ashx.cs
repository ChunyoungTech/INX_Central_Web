using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using pin.Page;
using Dapper;

namespace WebApp._query
{
    /// <summary>
    /// GetTagData 的摘要描述
    /// </summary>
    public class GetTagData : pin.Page.BaseHandler
    {
        protected override void DoHandler(HttpContext context)
        {
            rtnObj rData = new rtnObj();
            var x = pin.gObj.gDB.oConn.Query<dynamic>("select * from TagData where Tag_Name=@Name", new { Name = context.Request.QueryString["tag"] }).FirstOrDefault();
            if (x == null)
                rData.Error("查無符合條件資料");
            else
            {
                rData.ID = x.ID;
                rData.Tag_Desc = x.Tag_Desc;
                rData.HIHI = x.HiHi_Limit;
                rData.HI = x.Hi_Limit;
                rData.LO = x.Lo_Limit;
                rData.LOLO = x.LoLo_Limit;
                rData.OpcName = x.opc_name;
            }
            context.Response.Write(this.SerializeObject(rData));
        }

        protected override BaseHandlerOption SetBaseOption()
        {
            return new BaseHandlerOption() { Session = true, Parameter = "tag" };
        }

        class rtnObj : pin.ExeResult
        {
            public int ID { get; set; }
            public string Tag_Desc { get; set; }
            public decimal? HIHI { get; set; }
            public decimal? HI { get; set; }
            public decimal? LO { get; set; }
            public decimal? LOLO { get; set; }
            public string OpcName { get; set; }

        }
    }
}