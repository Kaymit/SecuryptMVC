using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Securypt.Resources;

namespace SecuryptMVC.Controllers {
    public class HomeController : Controller {
        public ActionResult Index() {
            //ViewBag.Message = Server.MapPath("~");

            return View();
        }

        public ActionResult About() {
            ViewBag.Message = MyStrings.HowStartH2;
            return View();
        }

        public ActionResult Contact() {
            return View();
        }
    }
}