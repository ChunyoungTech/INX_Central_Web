using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Security.Cryptography;
using cyc.Data;

namespace cyc
{
    public class Login : System.Web.SessionState.IReadOnlySessionState
    {
        public static bool CheckSession()
        {
            return (System.Web.HttpContext.Current.Session["uid"] != null);
        }

        public static List<UIMenuMain> GetUserMenu(UserInfo oUser)
        {
            return (from lsP in (from lsU in oUser.Role
                                 join lsR in cyc.Global.SysRole.List.Where(p => p.Enabled) on lsU equals lsR.ID
                                 join lsRP in cyc.Global.SysRoleProg.List on lsR.ID equals lsRP.RoleID
                                 join lsP in cyc.Global.SysProg.List.Where(p => p.Enabled) on lsRP.ProgID equals lsP.ID
                                 select lsP).Distinct().GroupBy(g => g.DirID)
                    join lsD in cyc.Global.SysDir.List.Where(p => p.Enabled) on lsP.Key equals lsD.ID
                    select new UIMenuMain { Name = lsD.Name, Seq = lsD.Seq, Items = lsP.Select(s => new UIMenuItem { ID = s.ID, Name = s.Name, Dir = s.Folder, Seq = s.Seq, Open = s.IsOpen }).OrderBy(o => o.Seq).ToList() }).OrderBy(o => o.Seq).ToList();
        }

        public static SysUser GetUser(string sID, string sPW)
        {
            using (var oDB = new cyc.DB.SqlDapperConn())
            {
                var oUser = oDB.QueryOne<SysUser>("select ID,Code,Name,DeptID,ISNULL(DeptLevel,DeptID)as DeptLevel,isManager from SysUser where Code=@Code and Password=@PW and Enabled=1", new { Code = sID.Trim(), PW = cyc.Login.CryptoPWD(sPW.Trim()) });
                //20250502 新增記錄Token
                if (oUser != null)
                {
                    oUser.LoginToken = Guid.NewGuid().ToString("N");
                    oDB.Execute("update SysUser set LoginToken=@LoginToken where ID=@ID", new { oUser.ID, oUser.LoginToken});
                }
                return oUser;
            }
        }

        public static UserInfo GetUserInfo(SysUser oUser)
        {
            return new UserInfo()
            {
                User = oUser,
                Dept = (SysDept)cyc.Global.SysDept.List.FirstOrDefault(p => p.ID == oUser.DeptID).Clone(),
                Role = GetUserRoles(oUser.ID)
            };
        }

        private static int[] GetUserRoles(int iUser)
        {
            var rList = cyc.Global.SysRole.List.Where(p => p.Enabled).Select(p => new { p.ID, p.IsDefault });
            return (from lsR in rList
                    join lsU in cyc.Global.SysRoleUser.List.Where(p => p.UserID == iUser) on lsR.ID equals lsU.RoleID
                    select lsR.ID).Concat(rList.Where(p => p.IsDefault).Select(p => p.ID)).Distinct().ToArray();
        }

        public static string CryptoPWD(string sPWD)
        {
            return Convert.ToBase64String(new SHA256CryptoServiceProvider().ComputeHash(Encoding.Default.GetBytes(sPWD)));
        }

        public static bool CheckUserProg(UserInfo oUser, int iApp)
        {
            var mProg = (from lsR in cyc.Global.SysRoleProg.List
                         join lsU in oUser.Role on lsR.RoleID equals lsU
                         where lsR.ProgID == iApp
                         select lsR).FirstOrDefault(p => p.isAllSub == true);
            return mProg != null;
        }

        public static SysProg GetNaviPage(UserInfo oUser, int iApp)
        {
            return (from lsU in oUser.Role
                    join lsR in cyc.Global.SysRoleProg.List on lsU equals lsR.RoleID
                    join lsP in cyc.Global.SysProg.List.Where(p => p.Enabled && p.ID == iApp) on lsR.ProgID equals lsP.ID
                    select lsP).FirstOrDefault();
        }

        public static SysUser GetAutoUser()
        {
            using (var oDB = new cyc.DB.SqlDapperConn())
            {
                var oUser = oDB.QueryOne<SysUser>("select ID,Code,Name,DeptID,ISNULL(DeptLevel,DeptID)as DeptLevel,isManager from SysUser where Code=@Code and Enabled=1", new { Code = "admin" });
                //20250502 新增記錄Token
                if (oUser != null)
                {
                    oUser.LoginToken = Guid.NewGuid().ToString("N");
                    oDB.Execute("update SysUser set LoginToken=@LoginToken where ID=@ID", new { oUser.ID, oUser.LoginToken });
                }
                return oUser;
            }
        }
    }
}
