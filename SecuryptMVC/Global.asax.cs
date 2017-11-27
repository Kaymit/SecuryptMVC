using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace SecuryptMVC {
    public class MvcApplication : HttpApplication {
        protected void Application_Start() {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        /// <summary>
        /// Changes the Resx file to the selected language chosen
        /// Author: Michael
        /// Date: 2017-11-25
        /// Based on: http://adamyan.blogspot.ca/2010/02/aspnet-mvc-2-localization-complete.html
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        protected void Application_AcquireRequestState(object sender, EventArgs e) {
            //It's important to check whether session object is ready
            if (HttpContext.Current.Session != null) {
                CultureInfo ci = (CultureInfo)this.Session["Culture"];
                string langName;
                //Checking first if there is no value in session 
                //and set default language 
                //this can happen for first user's request
                if (ci == null) {
                    //Sets default culture to english invariant
                    langName = "en";

                    //Try to get values from Accept lang HTTP header
                } else if (HttpContext.Current.Request.UserLanguages != null && HttpContext.Current.Request.UserLanguages.Length != 0) {
                    //Gets accepted 
                    langName = HttpContext.Current.Request.UserLanguages[0].Substring(0, 2);
                    ci = new CultureInfo(langName);
                    this.Session["Culture"] = ci;

                    //Finally setting culture for each request
                    Thread.CurrentThread.CurrentUICulture = ci;
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(ci.Name);
                }
            }

        }
        /*
        protected void Application_AcquireRequestState(object sender, EventArgs e)

     {

       //Create culture info object 

       CultureInfo ci = new CultureInfo("de");

     

       System.Threading.Thread.CurrentThread.CurrentUICulture = ci;

       System.Threading.Thread.CurrentThread.CurrentCulture = 

      CultureInfo.CreateSpecificCulture(ci.Name);

     }*/
    }
}