using System.Web;
using System.Web.Mvc;

namespace SecuryptMVC
{
    /// <summary>
    /// Template MVC code
    /// </summary>
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
