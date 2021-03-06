﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace WingkouWeb.Hubs
{
    public class ProcessingHub : Hub
    {
        static IHubContext HubContext = GlobalHost.ConnectionManager.GetHubContext<ProcessingHub>();

        public static void UpdateProgress(string connId, float percentage, string message)
        {
            HubContext.Clients.Client(connId).updateProgress(percentage, message);
        }

        public static void ReceiveData(string connId, string data, int len, int tot)
        {
            HubContext.Clients.Client(connId).receiveData(data, len, tot);
        }
    }
}