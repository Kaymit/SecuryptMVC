using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Securypt.Resources;

/// <summary>
/// 
/// </summary>
/// /// <author>
/// Michael O'Connell-Graf 17/11/2017 - 3/12/2017
/// </author>
namespace SecuryptMVC.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = MyStrings.HowStartH2;
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }
    }
}