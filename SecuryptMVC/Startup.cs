using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SecuryptMVC.Startup))]
namespace SecuryptMVC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
