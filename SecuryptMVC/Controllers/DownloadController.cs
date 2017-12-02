using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using SecuryptMVC.Utility;
using SecuryptMVC.DAL;
using SecuryptMVC.Models;
using System.Threading.Tasks;


namespace SecuryptMVC.Controllers
{
    public class DownloadController : Controller
    {
        FileContext db = new FileContext();

        public async Task<ActionResult> Download(int id)
        {
            return View();
        }
    }
}