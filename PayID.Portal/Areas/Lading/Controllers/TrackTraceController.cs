using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Net;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Driver.Builders;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PayID.DataHelper;
namespace PayID.Portal.Areas.Lading.Controllers
{
    public class TrackTraceController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult LadingTrackTrace()
        {
            return View();
        }
        public ActionResult TrackTrace(string id)
        {
            DynamicObj[] lstJourney = PayID.Portal.Areas.Lading.Configuration.Data.List("LogChangeLading", Query.EQ("Code", id));
            ViewBag.lstJourney = lstJourney;
         
            if(lstJourney==null)
              ViewBag.ResultMessage = "Không tìm được thông tin tra cứu";
            return View();
        }
        public ActionResult ListJourney(string id)
        {
            DynamicObj[] lstJourney = PayID.Portal.Areas.Lading.Configuration.Data.List("LogJourney", Query.EQ("Code", id));
                dynamic _traces = new DynamicObj();
                foreach (dynamic Jour in lstJourney)
                {
                        _traces = new DynamicObj();
                        _traces = PayID.Portal.Areas.Lading.Configuration.Data.Get("Status", Query.EQ("StatusCode", Jour.Status.ToString()));
                        Jour.Status = _traces.StatusDescription.ToString(); 
                }
                JObject[] JourneyList = new JObject[] { };
            ViewBag.JourneyList = (from c in lstJourney select c).OrderByDescending(e => ((dynamic)e).DateCreate).ToArray();
            return View();
        }
        public ActionResult ListHistory(string p)
        {
            //string _code = "";
            //if(!string.IsNullOrEmpty(p))
            //{
            //    if(p.Length>13)
            //    {
            //        _code = p.Substring(0,13);
            //    }
            //    else
            //    {
            //        _code = p;
            //    }
            //}
            dynamic Journey = PayID.Portal.Areas.Lading.Configuration.Data.Get("LogChangeLading", Query.EQ("_id", p));
            return View(Journey);
        }
        public JsonResult LadingInfo(string id)
        {            
           dynamic myDynamic = Configuration.Data.Get("Lading", Query.EQ("Code", id));
           return Json(new{
               From = PayID.Portal.Areas.Metadata.Controllers.ProxyController.GetProvinceName(myDynamic.FromProvinceCode.ToString()),
               To = PayID.Portal.Areas.Metadata.Controllers.ProxyController.GetProvinceName(myDynamic.ToProvinceCode.ToString()),
               Weight = myDynamic.Weight
           });

        }

    }
}