using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WrtmWebApp.Startup))]
namespace WrtmWebApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
