using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp._query
{
    /// <summary>
    /// GetUserMenu 的摘要描述
    /// </summary>
    public class GetUserMenu : cyc.Page.BaseHandler
    {
        protected override void DoHandler(HttpContext context)
        {
            ////cyc.Data.UserInfo oUser = (cyc.Data.UserInfo)context.Session["uid"];
            //MasterCont oCont = new MasterCont() { UserName = oUser.User.Name, UserDept = oUser.Dept.Name, List = cyc.Login.GetUserMenu(oUser) };
            ////if (!string.IsNullOrEmpty(context.Request.QueryString["app"]) && cyc.Shared.Check.IsInteger(context.Request.QueryString["app"]))
            ////{
            ////    var prog = cyc.Global.SysProg.List.FirstOrDefault(p => p.ID == Convert.ToInt16(context.Request.QueryString["app"]));
            ////}
            //context.Response.Write(this.SerializeObject(oCont));

            context.Response.Write(this.SerializeObject(new MasterCont() { List = cyc.Login.GetUserMenu(oUser) }));
        }

        protected override BaseHandlerOption SetBaseOption()
        {
            return new BaseHandlerOption() { Session = true };
        }

        /// <summary>
        /// 主頁面載入回傳資訊
        /// </summary>
        private class MasterCont
        {
            //public string UserName { get; set; }
            //public string UserDept { get; set; }
            public List<cyc.Data.UIMenuMain> List { get; set; }
        }
    }
}