using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp._query
{
    /// <summary>
    /// EditRoleUser 的摘要描述
    /// </summary>
    public class EditRoleUser : cyc.Page.BaseHandler
    {
        protected override void DoHandler(HttpContext context)
        {
            if (!string.IsNullOrEmpty(context.Request.Params[0]))
            {
                try
                {
                    RoleData oData = Newtonsoft.Json.JsonConvert.DeserializeObject<RoleData>(context.Request.Params[0]);
                    List<cyc.Data.SysRoleUser> insData = new List<cyc.Data.SysRoleUser>(), delData = new List<cyc.Data.SysRoleUser>();

                    using (var oDB = new cyc.DB.SqlDapperConn(oResult, null, true))
                    {
                        var oDataO = oDB.QueryList<cyc.Data.SysRoleUser>("select * from SysRoleUser where RoleID=@RoleID", new { RoleID = oData.ID });
                        //var oDataO = cyc.Global.SysRoleUser.List.Where(p => p.RoleID == oData.ID);
                        //比對新舊資料
                        foreach (var n in oData.Users)
                        {
                            var o = oDataO.FirstOrDefault(p => p.UserID == n.UserID);
                            if (o == null)//新資料有，舊資料沒有
                                insData.Add(n);
                        }
                        foreach (var o in oDataO)
                        {
                            var n = oData.Users.FirstOrDefault(p => p.UserID == o.UserID);
                            if (n == null)//舊資料有，新資料沒有
                                delData.Add(o);
                        }

                        if (insData.Count > 0 || delData.Count > 0)
                        {
                            if (insData.Count > 0)
                                oDB.Execute("insert into SysRoleUser(RoleID, UserID) values(@RoleID, @UserID)", insData);
                            if (oResult.Success && delData.Count > 0)
                                oDB.Execute("delete from SysRoleUser where RoleID=@RoleID and UserID=@UserID", delData);

                            if (oResult.Success) cyc.Global.SysRoleUser.Init(oDB, true);

                            oDB.ResultTransaction();
                        }
                    }

                    //using (SqlDBConn oDB = new SqlDBConn(ConnString.Main, true))
                    //{
                    //    if (insData.Count > 0 || delData.Count > 0)
                    //    {
                    //        if (insData.Count > 0)
                    //            oDB.Execute(new SqlDBPara() { Command = "insert into SysRoleUser(RoleID, UserID) values(@RoleID, @UserID)", Object = insData, Result = oResult });
                    //        if (oResult.Success && delData.Count > 0)
                    //            oDB.Execute(new SqlDBPara() { Command = "delete from SysRoleUser where RoleID=@RoleID and UserID=@UserID", Object = delData, Result = oResult });

                    //        oDB.Result(oResult);

                    //        if (oResult.Success)
                    //            CYCloud.SysInit.InitSysRoleUser();
                    //    }
                    //}
                }
                catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex.Message + ":" + ex.StackTrace, oResult); }
            }
            else
                oResult.Error("參數錯誤");

            context.Response.Write(this.SerializeObject(oResult));
        }

        protected override BaseHandlerOption SetBaseOption()
        {
            return new BaseHandlerOption() { Session = true };
        }

        #region 類別定義
        private class RoleData
        {
            public int ID { get; set; }
            public List<cyc.Data.SysRoleUser> Users { get; set; }
        }

        private class UserData
        {
            public int RoleID { get; set; }
            public string UserID { get; set; }
        }
        #endregion
    }
}