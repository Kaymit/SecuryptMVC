using System.Web.Mvc;

namespace SecuryptMVC.Helpers {
    /// <summary>
    /// Based on: https://stackoverflow.com/questions/15047272/mvc4-localization-accessing-resx-from-view#30984175
    /// </summary>
    public static class HtmlExtensions {
        public static MvcHtmlString Translate(this HtmlHelper htmlHelper, string key) {
            var viewPath = ((System.Web.Mvc.RazorView)htmlHelper.ViewContext.View).ViewPath;
            var culture = System.Threading.Thread.CurrentThread.CurrentCulture;

            var httpContext = htmlHelper.ViewContext.HttpContext;
            var val = (string)httpContext.GetLocalResourceObject(viewPath, key, culture);

            return MvcHtmlString.Create(val);
        }
    }
}