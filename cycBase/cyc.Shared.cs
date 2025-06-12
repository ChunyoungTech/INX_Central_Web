using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using Dapper;
using System.Security.Cryptography;
using cyc.Data;

namespace cyc.Shared
{
    /// <summary>
    /// 系統參數查詢取值功能
    /// </summary>
    public static class SysQuery
    {
        public static string GetSysSettingValue(string code)
        {
            return cyc.Global.SysSetting.List.FirstOrDefault(p => p.Code == code)?.Value ?? string.Empty;
            //if (cyc.Global.SysSetting.List.Any(p => p.Code == code))
            //    return cyc.Global.SysSetting.List.FirstOrDefault(p => p.Code == code).Value;
            //return "";
        }

        public static string GetAppSettingValue(string name)
        {
            return System.Web.Configuration.WebConfigurationManager.AppSettings[name]?.ToString();
            //string x = string.Empty;
            //try { x = System.Web.Configuration.WebConfigurationManager.AppSettings[name]?.ToString(); }
            //catch { }
            //return x;
        }

        public static string GetConnectString(string name)
        {
            return System.Configuration.ConfigurationManager.ConnectionStrings[name]?.ToString();
            //string x = string.Empty;
            //try { x = System.Configuration.ConfigurationManager.ConnectionStrings[name]?.ToString(); }
            //catch { }
            //return x;
        }

        public static ExeResult UpdateSysSetting(string code, string value)
        {
            ExeResult oResult = new ExeResult();
            try
            {
                using (DB.SqlDapperConn oDB = new DB.SqlDapperConn(oResult))
                {
                    oDB.Execute("update SysSetting set Value=@Value where Code=@Code", new { Code = code, Value = value });
                    var data = Global.SysSetting.List.FirstOrDefault(p => p.Code == code);
                    if (data != null) { data.Value = value; } else { Global.SysSetting.List.Add(new SysSetting { Code = code, Value = value }); }
                }
            }
            catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex.Message + ":" + ex.StackTrace, oResult); }
            return oResult;
        }
    }

    /// <summary>
    /// 系統公用檢核功能
    /// </summary>
    public static class Check
    {
        public static bool IsNumeric(object exp)
        {
            double retNum;
            return Double.TryParse(Convert.ToString(exp), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
        }

        public static bool IsInteger(object exp, bool isOverZero = false)
        {
            int retNum;
            if (int.TryParse(exp.ToString(), out retNum))
                return !isOverZero || (isOverZero && retNum > 0);
            else
                return false;
        }

        public static bool IsDateTime(object exp)
        {
            DateTime dt;
            return DateTime.TryParse(exp.ToString(), out dt);
        }

        public static bool IsOverZero(object exp, bool bEqu = false)
        {
            if (IsNumeric(exp))
            {
                if (!bEqu)
                    return (Convert.ToDouble(exp) > 0);
                else
                    return (Convert.ToDouble(exp) >= 0);
            }
            return false;
        }

        public static bool IsNumBetween(object exp, double max, double min = 0)
        {
            if (IsNumeric(exp)) { return (Convert.ToDouble(exp) >= min && Convert.ToDouble(exp) <= max); }
            return false;
        }

        public static bool IsTWIDNoComplex(string code)
        {
            var d = false;
            if (code.Length == 10)
            {
                code = code.ToUpper();
                if (code[0] >= 0x41 && code[0] <= 0x5A)
                {
                    var a = new[] { 10, 11, 12, 13, 14, 15, 16, 17, 34, 18, 19, 20, 21, 22, 35, 23, 24, 25, 26, 27, 28, 29, 32, 30, 31, 33 };
                    var b = new int[11];
                    b[1] = a[(code[0]) - 65] % 10;
                    var c = b[0] = a[(code[0]) - 65] / 10;
                    for (var i = 1; i <= 9; i++)
                    {
                        b[i + 1] = code[i] - 48;
                        c += b[i] * (10 - i);
                    }
                    if (((c % 10) + b[10]) % 10 == 0)
                    {
                        d = true;
                    }
                }
            }
            return d;
        }

        public static bool IsTWIDNoSimple(string code)
        {
            var d = false;
            if (code.Length == 10)
            {
                code = code.ToUpper();
                if (code[0] >= 0x41 && code[0] <= 0x5A && (code[1] == '1' || code[1] == '2'))
                {
                    d = true;
                    for (int i = 2; i < 10; i++)
                    {
                        if (!(code[i] >= 0x30 && code[i] <= 0x39))
                        {
                            d = false;
                            break;
                        }
                    }
                }
            }
            return d;
        }
    }

    /// <summary>
    /// 系統公用轉換DBNull值
    /// </summary>
    /// 
    public static class NullPara
    {
        public static object DateTimeNP(string sDate)
        {
            if (sDate.Length > 0 && Check.IsDateTime(sDate))
                return Convert.ToDateTime(sDate);
            else
                return System.DBNull.Value;
        }

        public static object IntegerNP(string sInt)
        {
            if (sInt.Length > 0 && Check.IsInteger(sInt))
                return Convert.ToInt32(sInt);
            else
                return System.DBNull.Value;
        }

        public static object DoubleNP(string sDouble)
        {
            if (sDouble.Length > 0 && Check.IsNumeric(sDouble))
                return Convert.ToDouble(sDouble);
            else
                return System.DBNull.Value;
        }

        public static object StringNP(string str)
        {
            if (str.Length > 0)
                return str;
            else
                return System.DBNull.Value;
        }
    }

    /// <summary>
    /// 系統公用轉換 object Null
    /// </summary>
    public static class NullValue
    {
        public static object DateTimeNP(string sDate)
        {
            if (sDate.Length > 0 && Check.IsDateTime(sDate))
                return Convert.ToDateTime(sDate);
            else
                return null;
        }

        public static object IntegerNP(string sInt)
        {
            if (sInt.Length > 0 && Check.IsInteger(sInt))
                return Convert.ToInt32(sInt);
            else
                return null;
        }

        public static object DoubleNP(string sDouble)
        {
            if (sDouble.Length > 0 && Check.IsNumeric(sDouble))
                return Convert.ToDouble(sDouble);
            else
                return null;
        }

        public static object DecimalNP(string sDecimal)
        {
            if (sDecimal.Length > 0 && Check.IsNumeric(sDecimal))
                return Convert.ToDecimal(sDecimal);
            else
                return null;
        }

        public static string StringNP(string str)
        {
            if (str.Length > 0)
                return str;
            else
                return null;
        }
    }

    public class NPOI
    {
        public static IWorkbook GetWorkbook(ref System.Web.UI.WebControls.FileUpload oFileUpload)
        {
            IWorkbook workbook = null;
            if (oFileUpload.HasFile)
            {
                string sFileExt = System.IO.Path.GetExtension(oFileUpload.FileName).ToLower();
                if (sFileExt == ".xls")
                    workbook = new HSSFWorkbook(oFileUpload.FileContent);
                else if (sFileExt == ".xlsx")
                    workbook = new XSSFWorkbook(oFileUpload.FileContent);
            }
            return workbook;
        }

        public static IWorkbook GetWorkbook(string sFile)
        {
            IWorkbook workbook = null;
            if (System.IO.File.Exists(sFile))
            {
                string sFileExt = System.IO.Path.GetExtension(sFile).ToLower();
                using (System.IO.FileStream fs = System.IO.File.Open(sFile, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                {
                    switch (sFileExt)
                    {
                        case ".xlsx":
                        case ".xlsm":
                        case ".xltx":
                        case ".xltm":
                            workbook = new XSSFWorkbook(fs);
                            break;
                        case ".xls":
                        case ".xlt":
                            workbook = new HSSFWorkbook(fs);
                            break;
                        default:
                            break;
                    }
                    //if (sFileExt == ".xls")
                    //    workbook = new HSSFWorkbook(fs);
                    //else if (sFileExt == ".xlsx")
                    //    workbook = new XSSFWorkbook(fs);

                    fs.Close();
                }
            }
            return workbook;
        }

        public static string GetCellValue(ICell cell)
        {
            string str = string.Empty;
            switch (cell.CellType)
            {
                case CellType.Numeric:  // 數值格式
                    if (DateUtil.IsCellDateFormatted(cell))
                    {   // 日期格式
                        str = cell.DateCellValue.ToString();
                    }
                    else
                    {   // 數值格式
                        str = cell.NumericCellValue.ToString();
                    }
                    break;
                case CellType.String:   // 字串格式
                    str = cell.StringCellValue;
                    break;
                //case CellType.Formula:  // 公式格式
                //    var formulaValue = formulaEvaluator.Evaluate(cell);
                //    if (formulaValue.CellType == CellType.String) str = formulaValue.StringValue.ToString();          // 執行公式後的值為字串型態
                //    else if (formulaValue.CellType == CellType.Numeric) str = formulaValue.NumberValue.ToString();    // 執行公式後的值為數字型態
                //    break;
                default:
                    break;
            }
            return str;
        }

        public static bool CheckColumnMap(IRow row, string[] strColumns)
        {
            bool bCheck = false;
            if (row.Cells.Count >= strColumns.Length)
            {
                bCheck = true;
                for (int idx = 0; idx < strColumns.Length; idx++)
                {
                    if (row.Cells[idx].StringCellValue.Trim() != strColumns[idx])
                    {
                        bCheck = false;
                        break;
                    }
                }
            }
            return bCheck;
        }
    }
}
