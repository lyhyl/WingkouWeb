using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace WingkouWeb.Hubs
{
    public class ProcessingHub : Hub
    {
        //從其他類別呼叫需用以下方法取得UploaderHub Instance
        static IHubContext HubContext = GlobalHost.ConnectionManager.GetHubContext<ProcessingHub>();

        public static void UpdateProgress(string connId, string name, float percentage, string progress, string message = null)
        {
            HubContext.Clients.Client(connId).updateProgress(name, percentage, progress, message);
        }
    }
}