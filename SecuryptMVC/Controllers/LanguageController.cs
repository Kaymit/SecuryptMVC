using System;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace SecuryptMVC.Controllers
{
    public class LanguageController : Controller
    {

        /// <summary>
        /// Author: Michael
        /// Date: 2017-12-04
        /// Based on: https://www.youtube.com/watch?v=oGeAYd3idBc
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        public ActionResult ChangeCulture(string lang) {

            if (lang != null ) {
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(lang);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);
            }

            HttpCookie cookie = new HttpCookie("Language");
            cookie.Value = lang;
            Response.Cookies.Add(cookie);
            return RedirectToAction("Index", "Home");
        }
    }
}