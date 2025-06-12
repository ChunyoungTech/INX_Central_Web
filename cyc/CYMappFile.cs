using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CYCloud.MappFile
{
    public static class Upload
    {
        private static object oLock { get; set; } = new object();
        private static List<UploadFile> List { get; set; } = new List<UploadFile>();

        public static string Add(string sPath)
        {
            lock (oLock)
            {
                DateTime tTime = DateTime.Now;
                foreach (var xUpload in List.Where(p => p.UploadTime < tTime.AddMinutes(-3)))
                {
                    try
                    {
                        System.IO.File.Delete(xUpload.Path);
                    }
                    catch (Exception ex) { cyc.Log.WriteSysErrorLog("刪除MAPP上傳檔案發生錯誤：" + ex.Message); }
                }

                List.RemoveAll(p => p.UploadTime < tTime.AddMinutes(-3));

                if (System.IO.File.Exists(sPath))
                {
                    var oFile = new System.IO.FileInfo(sPath);
                    var oUpload = new UploadFile() { Code = Guid.NewGuid().ToString("N"), Name = oFile.Name, Path = sPath, UploadTime = tTime };
                    List.Add(oUpload);
                    return oUpload.Code;
                }
            }
            return "";
        }
        public static void Remove(string sCode)
        {
            lock (oLock)
            {
                var xUpload = List.FirstOrDefault(p => p.Code == sCode);
                if (xUpload != null)
                {
                    try
                    {
                        System.IO.FileInfo oFile = new System.IO.FileInfo(xUpload.Path);
                        if (oFile != null)
                        {
                            System.IO.Directory.Delete(oFile.Directory.FullName, true);
                        }
                        //System.IO.File.Delete(xUpload.Path);
                    }
                    catch (Exception ex) { cyc.Log.WriteSysErrorLog("刪除MAPP上傳檔案發生錯誤：" + ex.Message); }
                    List.Remove(xUpload);
                }
            }
        }
        public static UploadFile Get(string sCode)
        {
            return List.FirstOrDefault(p => p.Code == sCode);
        }
    }

    public class UploadFile
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public DateTime UploadTime { get; set; }
    }
}