using Owin;
using Microsoft.Owin;
[assembly: OwinStartup(typeof(WingkouWeb.Hubs.Startup))]
namespace WingkouWeb.Hubs
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}