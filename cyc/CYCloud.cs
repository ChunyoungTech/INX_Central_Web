using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

namespace CYCloud
{
    //public class DeptControl
    //{
    //    public static void DeptCreate(DropDownList ddlDept, cyc.Data.UserInfo oUser, bool isShowAll, bool isShowTop)
    //    {
    //        if (!isShowAll)
    //        {
    //            var dData = cyc.Global.SysDept.List.FirstOrDefault(p => p.ID == oUser.User.DeptLevel);
    //            if (dData != null)
    //            {
    //                ddlDept.Items.Add(new ListItem(dData.Name, dData.ID.ToString()));
    //                GetNextDept(ddlDept, dData.ID);
    //            }
    //        }
    //        else
    //        {
    //            if (isShowTop)
    //            {
    //                ddlDept.Items.Add(new ListItem("", "0"));
    //            }
    //            else if (cyc.Global.SysDept.List.Count(p => p.UpperID == 0) > 1)
    //            {
    //                ddlDept.Items.Add(new ListItem("全部", ""));
    //            }
    //            GetNextDept(ddlDept, 0);
    //        }
    //    }

    //    private static void GetNextDept(DropDownList ddlDept, int iID, string sPrefix = "")
    //    {
    //        sPrefix += iID == 0 ? "" : "　";
    //        foreach (var dept in cyc.Global.SysDept.List.Where(p => p.UpperID == iID))
    //        {
    //            ddlDept.Items.Add(new ListItem(sPrefix + dept.Name, dept.ID.ToString()));
    //            GetNextDept(ddlDept, dept.ID, sPrefix);
    //        }
    //    }

    //    public static void GetNextDept(int iID, ref List<int> oList)
    //    {
    //        foreach (var dept in cyc.Global.SysDept.List.Where(p => p.UpperID == iID))
    //        {
    //            oList.Add(dept.ID);
    //            GetNextDept(dept.ID, ref oList);
    //        }
    //    }

    //    //回傳 是否符合部門權限
    //    public static bool CheckDeptLimit(cyc.Data.UserInfo oUser, int iID)
    //    {
    //        var uDept = cyc.Global.SysDept.List.FirstOrDefault(p => p.ID == oUser.User.DeptLevel);
    //        if (uDept != null)
    //        {
    //            if (uDept.LevelNo == 1)
    //                return true;
    //            else
    //                return GetDeptLimit(uDept).Any(p => p.ID == iID);
    //        }
    //        return false;
    //    }

    //    public static IEnumerable<int> GetDeptRange(int iID)
    //    {
    //        var uDept = cyc.Global.SysDept.List.FirstOrDefault(p => p.ID == iID);
    //        if (uDept != null)
    //        {
    //            var dList = GetDeptLimit(uDept);
    //            if (dList != null && dList.Count() > 0)
    //                return dList.Select(p => p.ID);
    //        }
    //        return null;
    //    }

    //    //回傳 部門權限 SQL
    //    public static string GetDeptLimitSQL(cyc.Data.UserInfo oUser, string sColumn)
    //    {
    //        var uDept = cyc.Global.SysDept.List.FirstOrDefault(p => p.ID == oUser.User.DeptLevel);
    //        if (uDept != null)
    //        {
    //            if (uDept.LevelNo == 1)
    //                return "1=1";
    //            else
    //            {
    //                var dList = GetDeptLimit(uDept);
    //                if (dList != null && dList.Count() > 0)
    //                    return string.Format("{0} in ({1})", sColumn, string.Join(",", dList.Select(p => p.ID)));
    //            }
    //        }
    //        return "1=0";
    //    }
        
    //    private static IEnumerable<cyc.Data.SysDept> GetDeptLimit(cyc.Data.SysDept uDept)
    //    {
    //        switch (uDept.LevelNo)
    //        {
    //            case 1:
    //                return cyc.Global.SysDept.List.Where(p => p.ID1 == uDept.ID);
    //            case 2:
    //                return cyc.Global.SysDept.List.Where(p => p.ID2 == uDept.ID);
    //            case 3:
    //                return cyc.Global.SysDept.List.Where(p => p.ID3 == uDept.ID);
    //            case 4:
    //                return cyc.Global.SysDept.List.Where(p => p.ID4 == uDept.ID);
    //            case 5:
    //                return cyc.Global.SysDept.List.Where(p => p.ID5 == uDept.ID);
    //            case 6:
    //                return cyc.Global.SysDept.List.Where(p => p.ID6 == uDept.ID);
    //            default:
    //                return null;
    //        }
    //    }
    //}

    #region 類別定義
    /// <summary>
    /// Tag設定
    /// </summary>
    public class TagData
    {
        public int ID { get; set; }
        public string Tag_Name { get; set; }
        public string Tag_Desc { get; set; }
        public string Unit { get; set; }
        public string Tag_Type { get; set; }
        public decimal? HiHi_Limit { get; set; }
        public decimal? Hi_Limit { get; set; }
        public decimal? Lo_Limit { get; set; }
        public decimal? LoLo_Limit { get; set; }
        public int User { get; set; }
        public DateTime DT { get; set; }
        public string opc_name { get; set; }
    }

    /// <summary>
    /// 部門使用Tag
    /// </summary>
    public class DeptTag
    {
        public int ID { get; set; }
        public int dept_id { get; set; }
        public int tag_data_id { get; set; }
        public string Tag_Name { get; set; }
        public string Tag_Desc { get; set; }
        public int User { get; set; }
        public DateTime DT { get; set; }
    }

    /// <summary>
    /// 部門使用Tag警報
    /// </summary>
    public class DeptAlarmTag : DeptTag
    {
        public decimal? HiHi { get; set; }
        public decimal? Hi { get; set; }
        public decimal? Lo { get; set; }
        public decimal? LoLo { get; set; }

        public bool? All_Enable { get; set; }
        public bool? HiHi_Enable { get; set; }
        public bool? Hi_Enable { get; set; }
        public bool? Lo_Enable { get; set; }
        public bool? LoLo_Enable { get; set; }
        public int? MAppGroupId { get; set; }
    }

    /// <summary>
    /// 報表設定
    /// </summary>
    public class ReportData
    {
        public int ID { get; set; }
        public int dept_id { get; set; }
        public string report_name { get; set; }
        public TimeSpan? auto_create_time { get; set; }
        public TimeSpan? auto_create_time2 { get; set; }
        public string save_pate { get; set; }
        public string report_desc { get; set; }
        public string upload_file_name { get; set; }
        public char stop_flag { get; set; }
        public int report_type { get; set; }
        public int User { get; set; }
        public DateTime DT { get; set; }
    }

    /// <summary>
    /// 報表Tag
    /// </summary>
    public class ReportTag
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int report_data_id { get; set; }
        public int tag_data_id { get; set; }
        public int sort { get; set; }
    }

    /// <summary>
    /// 報表Tag時間點
    /// </summary>
    public class ReportTime
    {
        public int ID { get; set; }
        public int report_data_id { get; set; }
        public TimeSpan value_time { get; set; }
        public int sort { get; set; }
    }

    /// <summary>
    /// 報表執行紀錄
    /// </summary>
    public class ReportExecLog
    {
        public int seq_id { get; set; }
        public int report_data_id { get; set; }
        public DateTime exec_time { get; set; }
        public string exec_status { get; set; }
        public string file_name { get; set; }
        public int dept_id { get; set; }
        public string report_name { get; set; }
    }

    /// <summary>
    /// 記錄值
    /// </summary>
    public class TagValue
    {
        public int ID { get; set; }
        public int tag_id { get; set; }
        public string tag_value { get; set; }
        public DateTime value_datetime { get; set; }
    }

    public class TagExtValue
    {
        public int tag_id { get; set; }
        public string tag_value { get; set; }
        public string tag_value_type { get; set; }
        public DateTime value_datetime { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RecognitionAuth
    {
        public int ID { get; set; }
        public string Fac { get; set; }
        public DateTime LogDateTime { get; set; }
        public string FRUserID { get; set; }
        public string FRUserName { get; set; }
    }
    #endregion
}
