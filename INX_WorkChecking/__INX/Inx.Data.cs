using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace Inx.Data
{
    public class ReportCategory
    {
        public int ID { get; set; }
        public int ReportID { get; set; }
        public string Level01 { get; set; }
        public string Level02 { get; set; }
        public string Level03 { get; set; }
        public string FAC { get; set; }
        public int SeqNo { get; set; }
        public string DataType { get; set; }
        public int YearS { get; set; }
        public int YearE { get; set; }
        public bool AddSUM { get; set; }
        public bool AddAVG { get; set; }
        public bool IsSUM { get; set; }
        public bool IsAVG { get; set; }
        public bool ExtSUM { get; set; }
        public bool ExtAVG { get; set; }
        public string TitleSUM { get; set; }
        public string TitleAVG { get; set; }
        public int DataDecimal { get; set; }//小數位數
    }

    public class GridValue
    {
        public int CategoryID { get; set; }
        public string FAC { get; set; }
        public string Level01 { get; set; }
        public string Level02 { get; set; }
        public string Level03 { get; set; }
        public string DataType { get; set; }
        //public int Year { get; set; }
        public decimal?[] ValueNum { get; set; } = new decimal?[12];
        public string[] ValueStr { get; set; } = new string[12];

        public bool AddSUM { get; set; }//此列加入[合計值]欄位(自動計算，不可編輯)
        public bool ExtSUM { get; set; }//此列加入[合計值]欄位(可編輯) [Month]=21
        public bool IsSUM { get; set; }//此列資料為[合計值](不可編輯)
        public string TitleSUM { get; set; }//[合計值]表頭


        public bool AddAVG { get; set; }//此列加入[平均值]欄位(自動計算，不可編輯)
        public bool ExtAVG { get; set; }//此列加入[平均值]欄位(可編輯) [Month]=22
        public bool IsAVG { get; set; }//此列資料為[平均值](不可編輯)
        public string TitleAVG { get; set; }//[平均值]表頭

        public string AutoData { get; set; }//
        public int DataDecimal { get; set; }//小數位數

        public decimal? ValueSUM { get; set; }
        public decimal? ValueAVG { get; set; }

        public string Html { get; set; }
    }

    public class ReportOption
    {
        public int ReportID { get; set; }
        public string ReportName { get; set; }
        public string TableName { get; set; }
        public Button Query { get; set; }
        public Button Update { get; set; }
        public Button Export { get; set; }
        public TextBox Year { get; set; }
        public DropDownList Factory { get; set; }
        public DropDownList Level01 { get; set; }
        public HiddenField Auth { get; set; }
    }
}