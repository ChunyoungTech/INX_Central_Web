using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using cyc.Page;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.HSSF.UserModel;

namespace WebApp._api
{
    /// <summary>
    /// testEXCEL 的摘要描述
    /// </summary>
    public class testEXCEL : cyc.Page.BaseHandler
    {
        protected override void DoHandler(HttpContext context)
        {
            HSSFWorkbook Workbook = new HSSFWorkbook();
            ISheet sheet1 = Workbook.CreateSheet("sheet1");

            if (context.Request.Params.Count > 0)
            {
                System.Collections.Specialized.NameValueCollection paras = context.Request.Params;
                int idx = 0;
                foreach (string key in paras.Keys)
                {
                    IRow row = sheet1.CreateRow(idx);
                    row.CreateCell(0).SetCellValue(key);
                    row.CreateCell(1).SetCellValue(context.Request.Params[key]);
                    idx++;
                }
            }

            using (MemoryStream ms = new MemoryStream())
            {
                Workbook.Write(ms);
                context.Response.Clear();
                context.Response.AddHeader("Content-Disposition", "attachment;filename=" + System.Web.HttpUtility.UrlEncode(string.Format("測試EXCEL{0}.xls", DateTime.Now.ToString("yyyyMMddHHmmss"))));
                context.Response.ContentType = "application/octet-stream";
                context.Response.OutputStream.Write(ms.GetBuffer(), 0, ms.GetBuffer().Length);
                context.Response.OutputStream.Flush();
                context.Response.OutputStream.Close();
            }
            context.Response.Flush();
        }

        protected override BaseHandlerOption SetBaseOption()
        {
            return new BaseHandlerOption() { Session = false };
        }
    }
}