using Excess.RuntimeProject;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Excess.Web.Services
{
    public class NotificationHub : Hub
    {
    }

    public class HubNotifier : INotifier
    {
        IHubContext _ctx;
        string _connection; 
        public HubNotifier(string connection)
        {
            _connection = connection;
            _ctx = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
        }


        public void notify(Notification notification)
        {
            _ctx.Clients.Client(_connection).notify(notification);
        }
    }
}