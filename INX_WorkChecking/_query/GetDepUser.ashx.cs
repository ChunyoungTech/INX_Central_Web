using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using cyc.Page;

namespace WebApp._query
{
    /// <summary>
    /// GetDepUser 的摘要描述
    /// </summary>
    public class GetDepUser : cyc.Page.BaseHandler
    {
        private void AddChildren(DepSelect oDept)
        {
            var dep = from ls in cyc.Global.SysDept.List where ls.UpperID == oDept.id select ls;
            foreach (cyc.Data.SysDept d in dep)
            {
                DepSelect oChild = new DepSelect() { id = d.ID, label = d.Name, children = new List<DepSelect>() };
                AddChildren(oChild);
                oDept.children.Add(oChild);
            }
        }

        protected override void DoHandler(HttpContext context)
        {

            cyc.Data.ExeResult oResult = new cyc.Data.ExeResult();
            //string sLimit = "FALSE";

            //if (!string.IsNullOrEmpty(context.Request.QueryString["Limit"]))
            //    sLimit = context.Request.QueryString["Limit"].ToString().ToUpper();

            //查詢全部單位資料
            if (string.IsNullOrEmpty(context.Request.QueryString["Dep"]) && string.IsNullOrEmpty(context.Request.QueryString["Emp"]))
            {
                List<DepSelect> oEntity = new List<DepSelect>();
                var dep = from ls in cyc.Global.SysDept.List where ls.UpperID == 0 select ls;
                foreach (cyc.Data.SysDept d in dep)
                {
                    DepSelect oDept = new DepSelect() { id = d.ID, label = d.Name, children = new List<DepSelect>() };
                    AddChildren(oDept);
                    oEntity.Add(oDept);
                }
                context.Response.Write(this.SerializeObject(oEntity));
            }
            else if (string.IsNullOrEmpty(context.Request.QueryString["Emp"]))//查詢單位內部所有人員
            {
                ////var oList = pin.gObj.gDB.InfluxDB<DepUser>(new cyc.DB.DapperDBPara() { Command = "select ID,Name from SysUser where DeptID=@Dep", Parameter = new { Dep = context.Request.QueryString["Dep"].ToString() } });
                ////context.Response.Write(this.SerializeObject(oList));
                //var users = pin.gObj.SysUser.Where(p => p.DeptID == Convert.ToInt32(context.Request.QueryString["Dep"]));
                //context.Response.Write(this.SerializeObject(users));
                using (cyc.DB.SqlDapperConn dDB = new cyc.DB.SqlDapperConn())
                {
                    context.Response.Write(this.SerializeObject(dDB.QueryList<DepUser>("select ID,Name from SysUser where DeptID=@Dep", new { Dep = context.Request.QueryString["Dep"] })));
                }
            }
            else//依員工班號或姓名查詢
            {
                ////var oItem = pin.gObj.gDB.InfluxDB<DepUser>(new cyc.DB.DapperDBPara() { Command = "select ID,Name from SysUser where ID=@ID or Name=@ID", Parameter = new { ID = context.Request.QueryString["Emp"].ToString() } }).FirstOrDefault();
                ////context.Response.Write(this.SerializeObject(oItem));
                //var user = pin.gObj.SysUser.FirstOrDefault(p => p.Name == context.Request.QueryString["Emp"].ToString());
                //context.Response.Write(this.SerializeObject(user));
                using (cyc.DB.SqlDapperConn dDB = new cyc.DB.SqlDapperConn())
                {
                    context.Response.Write(this.SerializeObject(dDB.QueryList<DepUser>("select ID,Name from SysUser where Name=@Name", new { Name = context.Request.QueryString["Emp"] })));
                }
            }
        }

        protected override BaseHandlerOption SetBaseOption()
        {
            return new BaseHandlerOption() { Session = true };
        }
    }

    public class DepSelect
    {
        public string label { get; set; }
        public int id { get; set; }
        public List<DepSelect> children { get; set; }
    }

    public class DepUser
    {
        public string Name { get; set; }
        public int ID { get; set; }
    }
}