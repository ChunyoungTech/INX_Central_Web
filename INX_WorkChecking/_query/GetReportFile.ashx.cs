using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using cyc.Page;

namespace WebApp._query
{
    /// <summary>
    /// GetReportFile 的摘要描述
    /// </summary>
    public class GetReportFile : cyc.Page.BaseHandler
    {
        protected override void DoHandler(HttpContext context)
        {
            using (var dDB = new cyc.DB.SqlDapperConn(oResult))
            {
                dDB.Command = "select A.*,B.dept_id,B.report_name from ReportDataExecLog A inner join ReportData B on A.report_data_id=B.ID where A.seq_id=@ID";
                dDB.Object = new { ID = context.Request.QueryString["pa"] };
                var xData = dDB.QueryOne<CYCloud.ReportExecLog>("select A.*,B.dept_id,B.report_name from ReportDataExecLog A inner join ReportData B on A.report_data_id=B.ID where A.seq_id=@ID", new { ID = context.Request.QueryString["pa"] });

                if (oResult.Success && xData != null)
                {
                    if (cyc.UC.DeptControl.CheckDeptLimit(oUser, xData.dept_id))
                    {
                        string sPath = (cyc.Shared.SysQuery.GetAppSettingValue("ReportStorePath") + @"\" + xData.report_data_id.ToString() + @"\").Replace(@"\\", @"\");
                        string sFileName = xData.report_name + "_" + xData.exec_time.ToString("yyyyMMddHHmm");
                        string sFile = sPath + xData.file_name;
                        if (System.IO.File.Exists(sFile))
                        {
                            string sExt = new System.IO.FileInfo(sFile).Extension;
                            context.Response.ContentType = "application/octet-stream";
                            context.Response.AppendHeader("Content-Disposition", "attachment; filename=" + context.Server.UrlPathEncode(sFileName + sExt));
                            context.Response.TransmitFile(sPath + xData.file_name);
                        }
                        else
                            context.Response.Write("檔案不存在");
                    }
                    else
                        context.Response.Write("非部門權限資料");
                }
            }
        }

        protected override BaseHandlerOption SetBaseOption()
        {
            return new BaseHandlerOption() { Session = true, Parameter = "pa" };
        }
    }
}