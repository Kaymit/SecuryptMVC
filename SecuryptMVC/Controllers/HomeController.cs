using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SecuryptMVC.Controllers {
    public class HomeController : BaseController {
        public ActionResult Index() {
            ViewBag.Message = Server.MapPath("~");

            return View();
        }

        public ActionResult About() {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact() {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        /// <summary>
        /// Based on: http://adamyan.blogspot.ca/2010/02/aspnet-mvc-2-localization-complete.html
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public ActionResult ChangeCulture(string lang, string returnUrl) {
            Session["Culture"] = new CultureInfo(lang);

            return Redirect(returnUrl);
        }

        /// <summary>
        /// Based on: http://afana.me/archive/2011/01/14/aspnet-mvc-internationalization.aspx/
        /// </summary>
        /// <param name="culture"></param>
        /// <returns></returns>
        public ActionResult c(string culture) {
            // Validate input
            culture = CultureHelper.GetImplementedCulture(culture);
            // Save culture in a cookie
            HttpCookie cookie = Request.Cookies["_culture"];
            if (cookie != null)
                cookie.Value = culture;   // update cookie value
            else {
                cookie = new HttpCookie("_culture");
                cookie.Value = culture;
                cookie.Expires = DateTime.Now.AddYears(1);
            }
            Response.Cookies.Add(cookie);
            return RedirectToAction("Index");
        }
    }
}