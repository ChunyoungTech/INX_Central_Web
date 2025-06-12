using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp
{
    // 發送 SignalR 通知 物件 (暫不用)
    public class AutoSignal
    {
        //IHubContext hub1;
        //IHubContext hub2;
        //IHubContext hub3;
        IHubContext hub4;
        IHubContext hub5;
        //IHubContext hub6;

        public AutoSignal()
        {
            //    CYCloud.gObj.AutoSignal.RaiseSyncDataEvent += HandleSyncDataEvent;//同步 TagValues
            //    CYCloud.gObj.AutoSignal.RaiseSyncMappEvent += HandleSyncMappEvent;//同步 Mapp 內外網資料
            //    CYCloud.IFP.AlertAuth.RaiseIFPAlertEvent += HandleIFPAlert;//發送 辨識結果
            //CYCloud.gObj.AutoSignal.RaiseTagDataChangeEvent += HandleTagDataChangeEvent;//同步 TagData

            //    hub1 = GlobalHost.ConnectionManager.GetHubContext<SyncMappHub>();//Mapp內外網資料
            //    hub2 = GlobalHost.ConnectionManager.GetHubContext<SyncDataHub>();//TagValues
            //    hub3 = GlobalHost.ConnectionManager.GetHubContext<FillPortHub>();//辨識結果
            hub4 = GlobalHost.ConnectionManager.GetHubContext<TagDataHub>();//同步 TagData

            CYCloud.Global.AutoSignal.RaiseSendMappEvent += HandleSendMappEvent;
            //hub5 = GlobalHost.ConnectionManager.GetHubContext<SendMappHub>();
        }

        public void HandleSendMappEvent(object sender, string e)
        {
            if (hub5 == null) { hub5 = GlobalHost.ConnectionManager.GetHubContext<SendMappHub>(); }
            if (hub5 != null) { hub5.Clients.All.addMessage(e); }
        }

        //public void HandleTagDataChangeEvent(object sender, string e)
        //{
        //    if (hub4 == null) { hub4 = GlobalHost.ConnectionManager.GetHubContext<TagDataHub>(); }
        //    if (hub4 != null) { hub4.Clients.All.addMessage(e); }
        //}

        //public void HandleSyncMappEvent(object sender, string e)
        //{
        //    if (hub1 == null) { hub1 = GlobalHost.ConnectionManager.GetHubContext<SyncMappHub>(); }
        //    if (hub1 != null) { hub1.Clients.All.addMessage(e); }
        //}

        //public void HandleSyncDataEvent(object sender, string e)
        //{
        //    if (hub2 == null) { hub2 = GlobalHost.ConnectionManager.GetHubContext<SyncDataHub>(); }
        //    if (hub2 != null) { hub2.Clients.All.addMessage(e); }
        //}

        //public void HandleIFPAlert(object sender, CYCloud.IFP.ClientAlert oAlert)
        //{
        //    if (hub3 == null) { hub3 = GlobalHost.ConnectionManager.GetHubContext<FillPortHub>(); }
        //    if (hub3 != null)
        //    {
        //        while (oAlert.Count < 10 && !oAlert.OK)
        //        {
        //            oAlert.Count++;
        //            hub3.Clients.Client(oAlert.ClientID).clientAlert(oAlert);
        //            System.Threading.Thread.Sleep(1000);
        //        }
        //    }
        //}
    }
}