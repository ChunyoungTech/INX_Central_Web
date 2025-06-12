using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp._api
{
    /// <summary>
    /// UploadMappFile 的摘要描述
    /// </summary>
    public class UploadMappFile : cyc.Page.BaseHandler
    {
        protected override void DoHandler(HttpContext context)
        {
            if (oResult.Success)
            {
                HttpFileCollection files = context.Request.Files;
                if (files.Count > 0)
                {
                    var file = files[0];
                    if (file.ContentLength <= 10 * 1024 * 1024)
                    {
                        try
                        {
                            string sFolder = System.IO.Path.Combine(new string[] { context.Server.MapPath("~/_upload/MappFile"), DateTime.Now.ToString("yyyyMMddHHmmss") });
                            System.IO.Directory.CreateDirectory(sFolder);
                            string sPath = System.IO.Path.Combine(new string[] { sFolder, file.FileName });
                            file.SaveAs(sPath);
                            oResult.Message = CYCloud.MappFile.Upload.Add(sPath);
                            if (string.IsNullOrWhiteSpace(oResult.Message))
                                oResult.Error("上傳檔案發生錯誤");
                        }
                        catch (Exception ex) { cyc.Log.WriteSysErrorLog("MAPP上傳檔案發生錯誤：" + ex.Message, oResult); }
                    }
                    else
                    {
                        oResult.Error("檔案大小超過限制");
                    }
                }
            }
            context.Response.Write(SerializeObject(oResult));
        }

        protected override BaseHandlerOption SetBaseOption()
        {
            return new BaseHandlerOption { NoCache = true, Session = true };
        }
    }
}