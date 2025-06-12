using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Net;
using CYCloud.IFP;
using CYCloud.MappEV.Data;

namespace WebApp
{
    public class Global : System.Web.HttpApplication
    {
        static AutoSignal oSignal = null; // (暫不用)

        protected void Application_Start(object sender, EventArgs e)
        {
            //記錄系統啟動(重啟)時間
            //if (!cyc.Global.IsDevelop) 
                cyc.Log.WriteSysErrorLog(string.Format("Application_Start@{0}", DateTime.Now));

            //連線安全機制
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            //執行授權檢核
            cyc.License.CheckLicense();
            //啟動 signalR
            if (cyc.Shared.SysQuery.GetAppSettingValue("AutoSignal") == "1")
                oSignal = new AutoSignal();

            //自動簽到+簽退，已無同步辨識結果
            if (CYCloud.IFP.SyncRecognitionAuth.DoAutoCheckIn || CYCloud.IFP.SyncRecognitionAuth.DoAutoCheckOut)
            {
                string SyncRecognitionAuthInterval = cyc.Shared.SysQuery.GetAppSettingValue(CYCloud.IFP.SyncRecognitionAuth.JobKey);
                if (SyncRecognitionAuthInterval == "") { SyncRecognitionAuthInterval = "0/20 * * ? * * *"; }
                cyc.Auto.Manager.Create<CYCloud.IFP.SyncRecognitionAuth>(CYCloud.IFP.SyncRecognitionAuth.JobKey, CYCloud.IFP.SyncRecognitionAuth.JobName, SyncRecognitionAuthInterval);
            }

            //發送施工管理簽到退MAPP(八廠) 每小時 發送 前一小時資料
            if (cyc.Shared.SysQuery.GetAppSettingValue(CYCloud.WorkCheck.WorkCheckHourMapp.JobKey) == "1")
            {
                cyc.Auto.Manager.Create<CYCloud.WorkCheck.WorkCheckHourMapp>(CYCloud.WorkCheck.WorkCheckHourMapp.JobKey, CYCloud.WorkCheck.WorkCheckHourMapp.JobName, "2 2 0/1 ? * * *");
            }

            //MAPP發送
            if (cyc.Shared.SysQuery.GetAppSettingValue(CYCloud.Mapp.AutoMapp.JobKey) == "1")
            {
                cyc.Auto.Manager.Create<CYCloud.Mapp.AutoMapp>(CYCloud.Mapp.AutoMapp.JobKey, CYCloud.Mapp.AutoMapp.JobName, "0/5 * * ? * * *");
            }

            //地震壓降MAPP
            if (cyc.Shared.SysQuery.GetAppSettingValue(CYCloud.MappEV.MappEVCreate.JobKey) == "1")
            {
                cyc.Auto.Manager.Create<CYCloud.MappEV.MappEVCreate>(CYCloud.MappEV.MappEVCreate.JobKey, CYCloud.MappEV.MappEVCreate.JobName, "0/10 * * ? * * *");
            }

            //MAPP未解隔通知
            if (cyc.Shared.SysQuery.GetAppSettingValue(CYCloud.CYMappRemind.JobKey) == "1")
            {
                cyc.Auto.Manager.Create<CYCloud.CYMappRemind>(CYCloud.CYMappRemind.JobKey, CYCloud.CYMappRemind.JobName, "10 0/1 * ? * * *");
            }

            //警報報表自動發送
            if (cyc.Shared.SysQuery.GetAppSettingValue(CYCloud.WorkCheck.AutoWorkCheckReport.JobKey) == "1")
            {
                //cyc.Auto.Manager.Create<CYCloud.WorkCheck.AutoWorkCheck>("WorkReportAuth", "警報報表自動發送", "0 0/1 * ? * * *");
                //20231207改為 固定時間 18:02:05 執行一次，不要每分鐘查一次
                cyc.Auto.Manager.Create<CYCloud.WorkCheck.AutoWorkCheckReport>(CYCloud.WorkCheck.AutoWorkCheckReport.JobKey, CYCloud.WorkCheck.AutoWorkCheckReport.JobName, "5 2 18 ? * * *");
            }

            //自動匯入 LoaderDB 資料
            if (!string.IsNullOrWhiteSpace(cyc.Shared.SysQuery.GetAppSettingValue(idb.Job.LoaderDB.JobKey)))
            {
                cyc.Auto.Manager.Create<idb.Job.LoaderDB>(idb.Job.LoaderDB.JobKey, idb.Job.LoaderDB.JobName, cyc.Shared.SysQuery.GetAppSettingValue(idb.Job.LoaderDB.JobKey));
            }

            //自動匯入 InfluxDB 資料
            if (!string.IsNullOrWhiteSpace(cyc.Shared.SysQuery.GetAppSettingValue(idb.Job.InfluxDB.JobKey)))
            {
                cyc.Auto.Manager.Create<idb.Job.InfluxDB>(idb.Job.InfluxDB.JobKey, idb.Job.InfluxDB.JobName, cyc.Shared.SysQuery.GetAppSettingValue(idb.Job.InfluxDB.JobKey));
            }

            //定時連線本系統，避免所有Session Timeout
            if (cyc.Shared.SysQuery.GetAppSettingValue(cyc.Auto.KeepAlive.JobKey) != "")
            {
                cyc.Auto.Manager.Create<cyc.Auto.KeepAlive>(cyc.Auto.KeepAlive.JobKey, cyc.Auto.KeepAlive.JobName, "30 0/10 * ? * * *");
            }

            //所有自動作業排程都加入後 再執行
            cyc.Auto.Manager.Start();
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {
            ////關閉 模擬產生[辨識結果] (暫不用)
            //CYCloud.IFP.RecognitionSimulation.Stop();

            ////Application_End 委派事件
            //cyc.Global.DoApplicationEnd();  

            //MappEV 更新彙整資料、記錄LOG、更新最後記錄
            CYCloud.MappEV.MappHighSummary.Close();

            //關閉自動排程
            cyc.Auto.Manager.Stop();

            //清除 System Cache
            cyc.Global.Close();

            //記錄系統關閉(回收)時間
            //if (!cyc.Global.IsDevelop)
                cyc.Log.WriteSysErrorLog(string.Format("Application_End@{0}", DateTime.Now)); 
        }
    }
}