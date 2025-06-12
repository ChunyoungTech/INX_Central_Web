using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace WebApp
{
    public class PatrolHub : Hub
    {

    }

    public class SyncDataHub : Hub // (暫不用)
    {

    }
    
    public class SyncMappHub : Hub // (暫不用)
    {

    }

    public class FillPortHub : Hub // (暫不用)
    {
        //void ClientAlert(string ID, string msg, string key)
        //{
        //    Clients.Client(ID).clientAlert(msg, key);
        //}

        ////Client 註冊 Device
        //public string RegisterClient(string name)
        //{
        //    CYCloud.IFP.AlertAuth.AddClientDevice(Context.ConnectionId, name);
        //    return Context.ConnectionId + "新增:" + name;
        //}

        ////Client 取消註冊 Device
        //public string UnRegisterClient(string name)
        //{
        //    CYCloud.IFP.AlertAuth.DelClientDevice(Context.ConnectionId, name);
        //    return Context.ConnectionId + "取消:" + name;
        //}

        ////接收 Client 收到訊息的回傳
        //public void ReturnConfirm(string Key)
        //{
        //    CYCloud.IFP.AlertAuth.AlertConfirm(Key);
        //}

        //public override Task OnConnected()
        //{
        //    Clients.All.addMessage(Context.ConnectionId + ":connected");
        //    return base.OnConnected();
        //}

        //public override Task OnDisconnected(bool stopCalled)
        //{
        //    CYCloud.IFP.AlertAuth.RemoveClient(Context.ConnectionId);
        //    Clients.All.addMessage(Context.ConnectionId + ":disconnected");
        //    return base.OnDisconnected(stopCalled);
        //}
    }

    public class TagDataHub : Hub
    {

    }

    public class SendMappHub : Hub
    {

    }
}