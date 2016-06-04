using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PayID.DataHelper;
using PayID.Portal.Common;
using PayID.Portal.Common.Service;
using PayID.Portal.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace PayID.Portal.Controllers
{
    public class HomeController : Controller
    {
        AccountService ACCOUNT_SERVICE = null;
        public HomeController()
        {
            if (ACCOUNT_SERVICE == null)
            {
                ACCOUNT_SERVICE = new AccountService();
            }
        }

        [Authorize]
        public ActionResult Index()
        {
            if (Session["profile"] == null)
            {
                return RedirectToAction("LogOut", "Home", new { Area = "" });
            }
            else
            {
                return View();
            }

        }

        public bool _login(string user_id, string password, bool? rememberMe)
        {
            bool bResult = false;
            var oAccount = ACCOUNT_SERVICE.Login(user_id, Security.CreatPassWordHash(password), ref bResult);

            if (bResult == true)
            {
                if (rememberMe == true)
                {
                    Response.Cookies["UserName"].Value = user_id;
                    Response.Cookies["UserName"].Expires = DateTime.Now.AddDays(14);
                    Response.Cookies["Remember"].Value = "True";
                    Response.Cookies["Remember"].Expires = DateTime.Now.AddDays(14);
                }
                else
                {
                    Response.Cookies["UserName"].Value = null;
                    Response.Cookies["Remember"].Value = null;
                }

                Session["profile"] = oAccount;
                FormsAuthentication.SetAuthCookie(user_id, (bool)rememberMe);

                var panelIdCookie = new HttpCookie("panelIdCookie");
                panelIdCookie.Values.Add("userid", user_id.ToString(CultureInfo.InvariantCulture));
                panelIdCookie.Expires = DateTime.Now.AddDays(14);
                Response.Cookies.Add(panelIdCookie);
            }


            return bResult;
        }

        [AllowAnonymous]
        public ActionResult Login(string ReturnUrl)
        {
            if (String.IsNullOrEmpty(ReturnUrl)) ReturnUrl = Url.Content("/");
            ViewBag.redirect = ReturnUrl;
            ViewBag.login = true;
            if (Request.Cookies["Remember"] != null)
            {
                if (Request.Cookies["Remember"].Value.Trim() == "True")
                {
                    if (Request.Cookies["UserName"].Value != null)
                    {
                        ViewBag.UserName = Request.Cookies["UserName"].Value.Trim();
                        ViewBag.Remember = true;
                    }
                }
            }
            else
            {
                ViewBag.Remember = false;
            }
            ViewBag.login = true;
            return View();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult LogOut()
        {
            Session["profile"] = null;
            return RedirectToAction("Login", "Home", new { Area = "" });
        }

        [Authorize]
        public ActionResult ChangePass()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(string userid, string password, string redirect)
        {

            if (_login(userid, password, true))
            {
                if (String.IsNullOrEmpty(redirect))
                    return RedirectToAction("Index", "Home");
                else
                    return Redirect(redirect);
            }
            else
                ViewBag.login = false;

            return View();


        }

        [Authorize]
        public ActionResult SignOut()
        {
            FormsAuthentication.SignOut();
            return Redirect(FormsAuthentication.LoginUrl);
        }
        public JsonResult Quantity()
        {
            if (Session["profile"] == null)
            {
                dynamic profile = Configuration.Data.Get("user", Query.EQ("_id", User.Identity.Name));
                Session["profile"] = profile;
            }

            string year = DateTime.Now.Year.ToString();
            string month = DateTime.Now.Month.ToString("00");
            string day = DateTime.Now.Day.ToString("00");
            string date = year + month + day;

            long totalC1, totalC8, totalC5, totalC6 = 0;
            List<dynamic> list = new List<dynamic>();
            string unit_link = ((PayID.Portal.Models.Account)Session["profile"]).UnitLink;
            if (unit_link == "00") unit_link = "";
            string sUnit_Link = unit_link;
            if (unit_link.Length == 2)
            {
                sUnit_Link = "00." + unit_link;
            }
            List<dynamic> lst = new List<dynamic>();
            lst = Configuration.Data.List("business_profile", Query.EQ("contact_address_province", unit_link)).ToList<dynamic>();

            MongoDB.Bson.BsonRegularExpression reg = new MongoDB.Bson.BsonRegularExpression("^" + sUnit_Link);
            MongoDB.Bson.BsonRegularExpression regs = new MongoDB.Bson.BsonRegularExpression("^" + unit_link);
            IMongoQuery _query = Query.NE("_id", "");//Query.Matches("current_assigned", reg);
            _query = Query.And(_query, Query.Or(Query.Matches("current_assigned", reg), Query.Matches("current_assigned", regs)));
            //_query = Query.And(
            //    _query,
            //    Query.GTE("system_time_key.date", long.Parse(date)),
            //    Query.LTE("system_time_key.date", long.Parse(date))
            //    );

            //if (unit_link == "10")
            //{
            //    _query = Query.And(
            //    Query.GTE("system_time_key.date", long.Parse(date)),
            //    Query.LTE("system_time_key.date", long.Parse(date))
            //    );
            //}
            //else
            //{
            _query = Query.And(
            _query,
            Query.GTE("system_time_key.date", long.Parse(date)),
            Query.LTE("system_time_key.date", long.Parse(date))
            );
            //}

            //if (lst != null && lst.Count > 0 && unit_link == "10")
            //{
            //    IMongoQuery _queryOr;
            //    //string sCustCode = "";
            //    var documentIds = new BsonValue[lst.Count];

            //    for (int j = 0; j < lst.Count; j++)
            //    {
            //        documentIds[j] = lst[j]._id;
            //    }

            //    _queryOr = Query.In("customer.code", documentIds);
            //    _query = Query.And(_query, _queryOr);
            //}

            IMongoQuery _queryC1 = Query.And(
                    _query, Query.EQ("system_status", "C1"));

            IMongoQuery _queryC5 = Query.And(
                   _query, Query.Or(Query.EQ("system_status", "C5"), Query.EQ("system_status", "C25")));

            IMongoQuery _queryC6 = Query.And(
                   _query, Query.EQ("system_status", "C6"));

            IMongoQuery _queryC8 = Query.And(
                   _query, Query.EQ("system_status", "C8"));


            totalC1 = Configuration.Data.TotalbyStatus("shipment", _queryC1);
            totalC5 = Configuration.Data.TotalbyStatus("shipment", _queryC5);
            totalC8 = Configuration.Data.TotalbyStatus("shipment", _queryC8);
            totalC6 = Configuration.Data.TotalbyStatus("shipment", _queryC6);


            return Json(new
            {
                C1 = totalC1,
                C5 = totalC5,
                C8 = totalC8,
                C6 = totalC6
            }, JsonRequestBehavior.AllowGet);
        }
    }
    public class Response
    {
        public string ErrorCode { get; set; }
        public string ErrorMsg { get; set; }
        public string Code { get; set; }
    }
}