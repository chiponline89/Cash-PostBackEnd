using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
namespace PayID.Portal.Areas.Merchant.Controllers
{
    public class ProxyController : Controller
    {
        //
        // GET: /Merchant/Proxy/
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult ListProfile(string customer_code)
        {
            if (!string.IsNullOrEmpty(customer_code))
            {
                dynamic myObj = PayID.Portal.Configuration.Data_S24.Get("business_profile", Query.EQ("_id", customer_code));
                return Json(new
                {
                    customer_name = myObj.general_full_name,
                    customer_mobile = myObj.contact_phone_mobile,
                    address = myObj.contact_address_address,
                    province = myObj.contact_address_province,
                    error_code = "00",
                    error_message = "Lấy dữ liệu thành công."
                }, JsonRequestBehavior.AllowGet);
            }
            else
                return Json(new
                {
                    error_code = "01",
                    error_message = "Lấy dữ liệu không thành công."

                }, JsonRequestBehavior.AllowGet);

        }
        public JsonResult LookupProfile(string lockupValue)
        {
            if (!string.IsNullOrEmpty(lockupValue))
            {
                dynamic myObj = PayID.Portal.Configuration.Data_S24.Get("business_profile",
                    Query.Or(Query.EQ("_id", lockupValue), Query.EQ("contact_phone_mobile", lockupValue), Query.EQ("general_short_name", lockupValue), Query.EQ("general_short_name", lockupValue)));

                if (myObj!= null )
                    return Json(new
                    { 
                        active = myObj.active,
                        customer_name = myObj.general_full_name,
                        short_name = myObj.general_short_name,
                        customer_mobile = myObj.contact_phone_mobile,
                        address = myObj.contact_address_address,
                        email = myObj.general_email,
                        province = myObj.contact_address_province,
                        customer_code = myObj._id,
                        error_code = "00",
                        error_message = "Lấy dữ liệu thành công."
                    }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                error_code = "01",
                error_message = "Lấy dữ liệu không thành công."

            }, JsonRequestBehavior.AllowGet);
        }
    }
}