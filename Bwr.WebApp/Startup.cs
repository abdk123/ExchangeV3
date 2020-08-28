using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Bwr.WebApp.Startup))]

namespace Bwr.WebApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
