using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace CYCloud
{
    public static class gObj
    {
        public static void InitAll()
        {
            //ReportSettings.Init();
            //MappSettings.Init();
            //IFP.SyncRecognitionAuth.Init();//起始資料，載入最近資料來比對，以免重新啟動漏掉
        }

        public static void ClearAll()
        {
            //ReportSettings.Clear();
            //MappSettings.Clear();
            //IFP.SyncRecognitionAuth.Clear();
        }

        //public static class ReportSettings
        //{
        //    public static List<ReportData> List;
        //    public static void Init()
        //    {
        //        Clear();
        //        List = pin.Global.gDB.QueryList<ReportData>("select * from ReportData").ToList();
        //    }
        //    public static void Clear()
        //    {
        //        if (List != null && List.Count > 0) { List.Clear(); }
        //    }
        //    public static void Update(ReportData oItem)
        //    {
        //        int index = List.FindIndex(p => p.ID == oItem.ID);
        //        if (index < 0)
        //            List.Add(oItem);
        //        else
        //        {
        //            List.RemoveAt(index);
        //            List.Insert(index, oItem);
        //        }
        //    }
        //    public static void Delete(int ID)
        //    {
        //        int index = List.FindIndex(p => p.ID == ID);
        //        if (index >= 0)
        //            List.RemoveAt(index);
        //    }
        //}

        //public static class MappSettings
        //{
        //    public static List<MappSetting> List;
        //    public static void Init()
        //    {
        //        Clear();
        //        List = pin.Global.gDB.QueryList<MappSetting>("select * from MappSetting").ToList();
        //    }
        //    public static void Clear()
        //    {
        //        if (List != null && List.Count > 0) { List.Clear(); }
        //    }
        //    public static void Update(MappSetting oItem)
        //    {
        //        if (List != null)
        //        {
        //            int index = List.FindIndex(p => p.MS_SEQ_ID == oItem.MS_SEQ_ID);
        //            if (index < 0)
        //                List.Add(oItem);
        //            else
        //            { List.RemoveAt(index); List.Insert(index, oItem); }
        //        }
        //    }
        //    public static void Delete(int ID)
        //    {
        //        if (List != null)
        //        {
        //            int index = List.FindIndex(p => p.MS_SEQ_ID == ID);
        //            if (index >= 0)
        //                List.RemoveAt(index);
        //        }
        //    }
        //}

        //public static class AutoSignal
        //{
        //    //發送訂閱
        //    public static event EventHandler<string> RaiseSendMappEvent;//發送 Mapp 訊息

        //    public static event EventHandler<string> RaiseSyncMappEvent;//同步 Mapp 內外網資料

        //    public static event EventHandler<string> RaiseSyncDataEvent;//同步 TagValues

        //    public static event EventHandler<string> RaiseTagDataChangeEvent;//發布 TagData 修改

        //    //發送訂閱訊息
        //    public static void DoSendMappPublish(string sMsg)
        //    {
        //        try
        //        { RaiseSendMappEvent?.Invoke(null, DateTime.Now.ToString() + "--" + sMsg); }
        //        catch (Exception ex) { DoErrorHandle(ex); }
        //    }

        //    public static void DoSyncMappPublish(string sMsg)
        //    {
        //        try
        //        { RaiseSyncMappEvent?.Invoke(null, DateTime.Now.ToString() + "--" + sMsg); }
        //        catch (Exception ex) { DoErrorHandle(ex); }
        //    }

        //    public static void DoSyncDataPublish(string sMsg)
        //    {
        //        try
        //        { RaiseSyncDataEvent?.Invoke(null, DateTime.Now.ToString() + "--" + sMsg); }
        //        catch (Exception ex) { DoErrorHandle(ex); }
        //    }

        //    public static void DoTagDataChangePublish(string sMsg)
        //    {
        //        try
        //        { RaiseTagDataChangeEvent?.Invoke(null, DateTime.Now.ToString() + "--" + sMsg); }
        //        catch (Exception ex) { DoErrorHandle(ex); }
        //    }

        //    private static void DoErrorHandle(Exception ex)
        //    {
        //        //Do Something
        //    }
        //}
    }

    //public static class SysInit
    //{
    //    private static pin.ExeResult oResult = new pin.ExeResult();

    //    public static void InitAll()
    //    {
    //        //InitSysUser();
    //        //InitSysDept();
    //        //InitSysDir();
    //        //InitSysProg();
    //        //InitSysRole();
    //        //InitSysRoleProg();
    //        //InitSysRoleUser();
    //        //InitSysSetting();
    //    }

    //    public static void ClearAll()
    //    {
    //        //if (pin.gObj.SysUser != null) { pin.gObj.SysUser.Clear(); }
    //        //if (pin.gObj.SysDept != null) { pin.gObj.SysDept.Clear(); }
    //        //if (pin.gObj.SysDir != null) { pin.gObj.SysDir.Clear(); }
    //        //if (pin.gObj.SysProg != null) { pin.gObj.SysProg.Clear(); }
    //        //if (pin.gObj.SysProgSub != null) { pin.gObj.SysProgSub.Clear(); }
    //        //if (pin.gObj.SysRole != null) { pin.gObj.SysRole.Clear(); }
    //        //if (pin.gObj.SysRoleProg != null) { pin.gObj.SysRoleProg.Clear(); }
    //        //if (pin.gObj.SysRoleProgSub != null) { pin.gObj.SysRoleProgSub.Clear(); }
    //        //if (pin.gObj.SysRoleUser != null) { pin.gObj.SysRoleUser.Clear(); }
    //        //if (pin.gObj.SysSetting != null) { pin.gObj.SysSetting.Clear(); }
    //    }

    //    //public static void InitSysUser()
    //    //{
    //    //    if (pin.gObj.SysUser != null) { pin.gObj.SysUser.Clear(); }
    //    //    pin.gObj.SysUser = pin.gObj.gDB.QueryList<pin.SysUser>(new pin.DB.SqlDBPara() { Command = "select * from SysUser", Result = oResult }).ToList();
    //    //}

    //    //public static void InitSysDept()
    //    //{
    //    //    if (pin.gObj.SysDept != null) { pin.gObj.SysDept.Clear(); }
    //    //    pin.gObj.SysDept = pin.gObj.gDB.QueryList<pin.SysDept>(new pin.DB.SqlDBPara() { Command = "select * from SysDept", Result = oResult }).ToList();
    //    //}

    //    //public static void InitSysDir()
    //    //{
    //    //    if (pin.gObj.SysDir != null) { pin.gObj.SysDir.Clear(); }
    //    //    pin.gObj.SysDir = pin.gObj.gDB.QueryList<pin.SysDir>(new pin.DB.SqlDBPara() { Command = "select * from SysDir order by Seq", Result = oResult }).ToList();
    //    //}

    //    //public static void InitSysProg()
    //    //{
    //    //    if (pin.gObj.SysProg != null) { pin.gObj.SysProg.Clear(); }
    //    //    pin.gObj.SysProg = pin.gObj.gDB.QueryList<pin.SysProg>(new pin.DB.SqlDBPara() { Command = "select A.*,B.Name as DirName from SysProg A inner join SysDir B on A.DirID=B.ID order by B.Seq,A.Seq", Result = oResult }).ToList();
    //    //    if (pin.gObj.SysProgSub != null) { pin.gObj.SysProgSub.Clear(); }
    //    //    pin.gObj.SysProgSub = pin.gObj.gDB.QueryList<pin.SysProgSub>(new pin.DB.SqlDBPara() { Command = "select * from SysProgSub", Result = oResult }).ToList();
    //    //}

    //    //public static void InitSysRole()
    //    //{
    //    //    if (pin.gObj.SysRole != null) { pin.gObj.SysRole.Clear(); }
    //    //    pin.gObj.SysRole = pin.gObj.gDB.QueryList<pin.SysRole>(new pin.DB.SqlDBPara() { Command = "select * from SysRole", Result = oResult }).ToList();
    //    //}

    //    //public static void InitSysRoleProg()
    //    //{
    //    //    if (pin.gObj.SysRoleProg != null) { pin.gObj.SysRoleProg.Clear(); }
    //    //    pin.gObj.SysRoleProg = pin.gObj.gDB.QueryList<pin.SysRoleProg>(new pin.DB.SqlDBPara() { Command = "select * from SysRoleProg", Result = oResult }).ToList();
    //    //    if (pin.gObj.SysRoleProgSub != null) { pin.gObj.SysRoleProgSub.Clear(); }
    //    //    pin.gObj.SysRoleProgSub = pin.gObj.gDB.QueryList<pin.SysRoleProgSub>(new pin.DB.SqlDBPara() { Command = "select * from SysRoleProgSub", Result = oResult }).ToList();
    //    //}

    //    //public static void InitSysRoleUser()
    //    //{
    //    //    if (pin.gObj.SysRoleUser != null) { pin.gObj.SysRoleUser.Clear(); }
    //    //    pin.gObj.SysRoleUser = pin.gObj.gDB.QueryList<pin.SysRoleUser>(new pin.DB.SqlDBPara() { Command = "select distinct * from SysRoleUser", Result = oResult }).ToList();
    //    //}

    //    //public static void InitSysSetting()
    //    //{
    //    //    if (pin.gObj.SysSetting != null) { pin.gObj.SysSetting.Clear(); }
    //    //    pin.gObj.SysSetting = pin.gObj.gDB.QueryList<pin.SysSetting>(new pin.DB.SqlDBPara() { Command = "select * from SysSetting", Result = oResult }).ToList();
    //    //}
    //}
}
