using PayID.Portal.App_Start;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace PayID.Portal
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            PayID.Portal.Areas.ServiceRequest.Configuration.Init();
            PayID.Portal.Areas.Merchant.Configuration.Init();
            PayID.Portal.Areas.Metadata.Configuration.Init();
            PayID.Portal.Areas.Report.Configuration.Init();
            PayID.Portal.Areas.Systems.Configuration.Init();
            PayID.Portal.Areas.Transactions.Configuration.Init();            
            PayID.Portal.Areas.Lading.Configuration.Init();
            Configuration.Init();
            PayID.Portal.Areas.Merchant.Configuration.GetLocaltion();

            Common.Configuration.Init();
        }
    }
}
