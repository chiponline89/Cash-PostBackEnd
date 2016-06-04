using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace PayID.API
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            Process.MerchantProcess.Init();
            Portal.Areas.ServiceRequest.Configuration.Init();
            Portal.Areas.Merchant.Configuration.Init();
            Portal.Areas.Metadata.Configuration.Init();
            Portal.Areas.Systems.Configuration.Init();
            Portal.Areas.Lading.Configuration.Init();
            //BundleConfig.RegisterBundles(BundleTable.Bundles);

            Log.Start();
        }
    }
}
