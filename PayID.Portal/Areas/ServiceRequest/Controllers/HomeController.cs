using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PayID.Portal.Areas.ServiceRequest.Controllers
{
    public class HomeController : Controller
    {
        // GET: ServiceRequest/Home
        [Authorize]
        public ActionResult Index()
        {
            return RedirectToAction("Index","Request");
        }
    }
}