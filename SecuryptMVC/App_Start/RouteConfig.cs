using System.Web.Mvc;
using System.Web.Routing;

namespace SecuryptMVC
{
    /// <summary>
    /// Template MVC code
    /// </summary>
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            //add route for admin functionality
			routes.MapRoute(
				name: "Delete",
				url: "Admin/Delete/{id}",
				defaults: new { controller = "Admin", action = "Delete", id = UrlParameter.Optional }
			);
		}
    }
}
