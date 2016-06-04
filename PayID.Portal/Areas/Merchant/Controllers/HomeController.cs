using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MongoDB.Driver.Builders;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Configuration;
using MongoDB.Driver;
using System.Dynamic;
using PayID.DataHelper;
using PayID.Portal.Common.Service;
namespace PayID.Portal.Areas.Merchant.Controllers
{
    public class HomeController : Controller
    {
        private int defaultPageSize = int.Parse(string.IsNullOrEmpty(ConfigurationManager.AppSettings["PAGE_SIZE"].ToString()) ? "10" : ConfigurationManager.AppSettings["PAGE_SIZE"].ToString());
        
        private CustomerService CUSTOMER_SERVICE = null;
        private DictionaryService DICTIONARY_SERVICE = null;
        public HomeController()
        {
            if (CUSTOMER_SERVICE == null)
            {
                CUSTOMER_SERVICE = new CustomerService();
            }

            if(DICTIONARY_SERVICE == null)
            {
                DICTIONARY_SERVICE = new DictionaryService();
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetDistrictByProvince(string ProvinceCode)
        {
            JsonResult jResult = new JsonResult();

            List<SelectListItem> ListDistrict = new List<SelectListItem>();

            var _listDistrict = DICTIONARY_SERVICE.GetDistrictByProvince(ProvinceCode);

            if(_listDistrict != null && _listDistrict.Count> 0)
            {
                foreach(var item in _listDistrict)
                {
                    ListDistrict.Add(new SelectListItem
                    {
                        Text = item.Description,
                        Value = item.DistrictCode
                    });
                }
            }

            jResult = Json(new { data = ListDistrict }, JsonRequestBehavior.AllowGet);

            return jResult;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetWardByDistrict(string DistrictCode)
        {
            JsonResult jResult = new JsonResult();

            List<SelectListItem> ListWard = new List<SelectListItem>();

            var _listWard = DICTIONARY_SERVICE.GetWardByDistrict(DistrictCode);

            if (_listWard != null && _listWard.Count > 0)
            {
                foreach (var item in _listWard)
                {
                    ListWard.Add(new SelectListItem
                    {
                        Text = item.WardName,
                        Value = item.WardCode
                    });
                }
            }

            jResult = Json(new { data = ListWard }, JsonRequestBehavior.AllowGet);

            return jResult;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetStoreByCustomer(string CustomerCode, long Id)
        {
            JsonResult jResult = new JsonResult();

            List<SelectListItem> ListStore = new List<SelectListItem>();

            var _listStore = CUSTOMER_SERVICE.GetStore(CustomerCode, Id, "", "", 0);

            jResult = Json(new { data = _listStore }, JsonRequestBehavior.AllowGet);

            return jResult;
        }

    
        [Authorize]
        public ActionResult Index(string employee_name, int? page)
        {
            if (Session["profile"] == null)
            {
                dynamic profile = Configuration.Data.Get("user", Query.EQ("_id", User.Identity.Name));
                Session["profile"] = profile;
            }
            ViewData["employee_name"] = employee_name;
            int currentPageIndex = page.HasValue ? page.Value : 1;
            ViewBag.Page = currentPageIndex;
            return View();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult SearchCustomer(string sSearch)
        {
            long lTotal = 0;
            var listCustomer = CUSTOMER_SERVICE.SearchBusinessProfile(sSearch.Trim(), 1, ref lTotal);
            return PartialView(listCustomer);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public ActionResult ProfileConfiguration(string id)
        {
            if (Session["profile"] == null)
            {
                dynamic profile = PayID.Portal.Areas.Metadata.Configuration.Data.Get("user", Query.EQ("_id", User.Identity.Name));
                Session["profile"] = profile;
            }
            string v_unitcode = ((dynamic)Session["profile"]).unit_code;
            v_unitcode = v_unitcode.Substring(0, 2);
            if (v_unitcode == "45")
                v_unitcode = "44";
            if (v_unitcode == "10" || v_unitcode == "11" || v_unitcode == "12" || v_unitcode == "13" || v_unitcode == "14" || v_unitcode == "15")
                v_unitcode = "10";
            if (v_unitcode == "70" || v_unitcode == "71" || v_unitcode == "72" || v_unitcode == "73" || v_unitcode == "74" || v_unitcode == "75" || v_unitcode == "75")
                v_unitcode = "70";
            ViewBag.v_unitcode = v_unitcode;
            ViewBag.profile = Configuration.Data.Get("business_profile", Query.EQ("_id", id));
            ViewBag.user_login = User.Identity.Name;
            ViewBag.customer_code = id;

            //return View();
            //if(Session["profile"] == null) return Redirect("/");
            //string unit_code = ((dynamic)Session["profile"]).unit_code;

            if (v_unitcode == "00")
                return View(Configuration.Data.List("mbcUnit", Query.EQ("ParentUnitCode", v_unitcode)));
            else
                return View(Configuration.Data.List("mbcUnit", Query.EQ("UnitCode", v_unitcode)));

        }

        public ActionResult ListBank(string id)
        {
            DynamicObj[] lstBank = PayID.Portal.Areas.Merchant.Configuration.Data.List("profile_bank", Query.EQ("UserId", id));
            ViewBag.BankList = lstBank;
            return View();
        }
        public ActionResult ListStore(string id)
        {
            DynamicObj[] lstStore = PayID.Portal.Areas.Merchant.Configuration.Data.List("profile_store", Query.EQ("UserId", id));
            ViewBag.StoreList = lstStore;
            return View();
        }


        public ActionResult StoreManagement()
        {
            if (Session["profile"] == null)
            {
                dynamic profile = PayID.Portal.Areas.Metadata.Configuration.Data.Get("user", Query.EQ("_id", User.Identity.Name));
                Session["profile"] = profile;
            }
            string myunit_code = ((dynamic)Session["profile"]).unit_code;
            myunit_code = myunit_code.Substring(0, 2);
            if (myunit_code == "45")
                myunit_code = "44";
            if (myunit_code == "10" || myunit_code == "11" || myunit_code == "12" || myunit_code == "13" || myunit_code == "14" || myunit_code == "15")
                myunit_code = "10";
            if (myunit_code == "70" || myunit_code == "71" || myunit_code == "72" || myunit_code == "73" || myunit_code == "74" || myunit_code == "75" || myunit_code == "75")
                myunit_code = "70";


            DynamicObj[] lstStore = PayID.Portal.Areas.Merchant.Configuration.Data.List("profile_store", Query.EQ("ProvinceCode", myunit_code));
            DynamicObj[] lstCustomer = PayID.Portal.Areas.Merchant.Configuration.Data.List("business_profile", null);
            foreach (dynamic store in lstStore)
            {
                store.ProvinceName = Areas.Metadata.Controllers.ProxyController.GetProvinceName(store.ProvinceCode);
                store.DistrictName = Areas.Metadata.Controllers.ProxyController.GetDistrictName(store.DistrictCode);
            }
            ViewBag.unit = myunit_code;
            ViewBag.StoreList = lstStore;
            ViewBag.CustomerList = lstCustomer;
            ViewData["dropTinhNhan"] = PayID.Portal.Areas.Merchant.Configuration.List_Item_Province;

            if (myunit_code == "00")
                return View(Configuration.Data.List("mbcUnit", Query.EQ("ParentUnitCode", myunit_code)));
            else
                return View(Configuration.Data.List("mbcUnit", Query.EQ("UnitCode", myunit_code)));
        }
        public ActionResult ListStoreByProvince(string province_code)
        {
            DynamicObj[] lstStore = PayID.Portal.Areas.Merchant.Configuration.Data.List("profile_store", Query.EQ("ProvinceCode", province_code));
            foreach (dynamic store in lstStore)
            {
                store.ProvinceName = Areas.Metadata.Controllers.ProxyController.GetProvinceName(store.ProvinceCode);
                store.DistrictName = Areas.Metadata.Controllers.ProxyController.GetDistrictName(store.DistrictCode);
            }
            ViewBag.StoreList = lstStore;
            return View();
        }


        [HttpPost]
        public JsonResult SetActiveProfile(string id, string active)
        {
            string myreturn = "";
            dynamic myObj = new DynamicObj();
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    myObj = PayID.Portal.Areas.Merchant.Configuration.Data.Get("business_profile", Query.EQ("_id", id));
                }
                else
                {
                    myObj = null;
                }

                if (myObj != null)
                {
                    if (!string.IsNullOrEmpty(active) && active == "1")
                    {
                        myObj.active = true;
                    }
                    else
                    {
                        myObj.active = false;
                    }
                    Configuration.Data.Save("business_profile", myObj);
                    myreturn = "00";
                }
                else
                {
                    myreturn = "01";
                }
            }
            catch
            {
                myreturn = "01";
            }
            return Json(new { result = myreturn });
        }

        public ActionResult ListProfile(string email, string code, string type, string shortname, int? page, string pagesize)
        {
            IMongoQuery query = Query.NE("_id", "");
            //List<dynamic> LIST = new List<dynamic>();         
            try
            {
                if (!string.IsNullOrEmpty(email))
                {
                    query = Query.And(query, Query.EQ("general_email", email));
                }
                if (!string.IsNullOrEmpty(code))
                {
                    query = Query.And(query, Query.EQ("_id", code));
                }
                if (!string.IsNullOrEmpty(shortname))
                {
                    query = Query.And(query, Query.EQ("general_short_name", shortname));
                }
                if (!string.IsNullOrEmpty(type))
                {
                    query = Query.And(query, Query.EQ("general_account_type", type));
                }

                if ((dynamic)Session["profile"] != null && ((dynamic)Session["profile"]).unit_link != null)
                {
                    if (((dynamic)Session["profile"]).unit_code != "00")
                    {
                        //if (((dynamic)Session["profile"]).unit_link.Length <= 2)
                        //{
                        query = Query.And(query, Query.EQ("contact_address_province", ((dynamic)Session["profile"]).unit_link.ToString().Substring(0, 2)));
                        //}
                        //else if (((dynamic)Session["profile"]).unit_link.Length > 2 && ((dynamic)Session["profile"]).unit_link.Length < 8)
                        //{
                        //    query = Query.And(query, Query.EQ("contact_address_province", ((dynamic)Session["profile"]).unit_link.ToString().Substring(0, 2)), Query.EQ("contact_address_district", ((dynamic)Session["profile"]).unit_link.ToString().Substring(3, 4)));
                        //}
                    }
                }

                MongoDB.Driver.IMongoSortBy _sort = MongoDB.Driver.Builders.SortBy.Ascending("general_full_name");
                long total = 0;
                int currentPageIndex = page.HasValue ? page.Value : 1;
                int pageSize = string.IsNullOrEmpty(pagesize) ? defaultPageSize : int.Parse(pagesize);
                if (pageSize == 0)
                    pageSize = defaultPageSize;
                var LIST = Configuration.Data.ListPagging("business_profile", query, _sort, pageSize, currentPageIndex, out total);
                ViewBag.listProfiles = LIST;
                ViewBag.Page = (currentPageIndex - 1) * pageSize;
                ViewBag.total_page = (total + pageSize - 1) / pageSize;
            }
            catch
            {
                //myreturn = "01";
            }
            return View(ViewBag.listProfiles);
        }


        [HttpPost]
        public JsonResult InsertStore(string customer_code, string province_code, string province_name, string district_code, string district_name, string address)
        {
            bool sReturn = false;
            try
            {
                dynamic dyna = new DynamicObj();
                dyna._id = PayID.Portal.Areas.Merchant.Configuration.Data.GetNextSquence("profile_store");
                dyna.ProvinceCode = province_code;
                dyna.DistrictCode = district_code;
                dyna.Address = address;
                dyna.Default = 0;
                dyna.UserId = customer_code;
                dyna.Description = province_name + " " + district_name + " " + address;

                sReturn = Configuration.Data.Save("profile_store", dyna);
            }
            catch
            { }
            return Json(sReturn);
        }
        public JsonResult StoreSave(string customer_code, string id, string province_code, string province_name, string district_code, string district_name, string address)
        {
            bool sReturn = false;
            try
            {
                dynamic dyna = new DynamicObj();
                dyna._id = (id == "0") ? dyna._id = PayID.Portal.Areas.Merchant.Configuration.Data.GetNextSquence("profile_store") : long.Parse(id);
                dyna.ProvinceCode = province_code;
                dyna.DistrictCode = district_code;
                dyna.Address = address;
                dyna.Default = 0;
                dyna.UserId = customer_code;

                dyna.Description = province_name + " " + district_name + " " + address;

                sReturn = Configuration.Data.Save("profile_store", dyna);
            }
            catch
            { }
            return Json(sReturn);

        }
        //Hàm thiết lập tài khoản ngân hàng
        [HttpPost]
        public JsonResult save_bank(string id, string customer_code, string bank_code, string bank_name, string bank_branch, string bank_user, string account_number)
        {
            string return_code = ""; string message_return = "";
            dynamic myObj = new DynamicObj();
            try
            {
                if (!string.IsNullOrEmpty(id))
                {

                    myObj = PayID.Portal.Areas.Merchant.Configuration.Data.Get("profile_bank", Query.EQ("_id", long.Parse(id)));
                }
                else
                {
                    myObj._id = PayID.Portal.Areas.Merchant.Configuration.Data.GetNextSquence("profile_bank");
                }
                myObj.UserId = customer_code;
                myObj.bank_code = bank_code;
                myObj.bank_name = bank_name;
                myObj.bank_branch = bank_branch;
                myObj.bank_user = bank_user;
                myObj.account_number = account_number;

                Configuration.Data.Save("profile_bank", myObj);
                return_code = "00"; message_return = "Cập nhật thành công.";

            }
            catch
            {
                return_code = "01"; message_return = "Cập nhật thất bại.";
            }
            return Json(new { result = return_code, message = message_return });
        }
        //---
        [HttpPost]
        public JsonResult save_store_mng(string customer_code, string _id, string store_name,
            string store_manager_name, string store_manager_mobile, string store_manager_email,
            string store_to_address, string province_code, string district_code, string postcode)
        {
            string return_code = ""; string message_return = "";
            dynamic myObj = new DynamicObj();
            try
            {
                if (!string.IsNullOrEmpty(_id))
                {
                    myObj = PayID.Portal.Areas.Merchant.Configuration.Data.Get("profile_store", Query.EQ("_id", long.Parse(_id)));
                }
                else
                {
                    myObj._id = PayID.Portal.Areas.Merchant.Configuration.Data.GetNextSquence("profile_store");
                    myObj.StoreCode = customer_code + PayID.Portal.Configuration.Data.GetNextSquence("profile_store" + customer_code).ToString("000");
                }
                // myObj._id = (String.IsNullOrEmpty(_id))? PayID.Portal.Areas.Merchant.Configuration.Data.GetNextSquence("profile_store"): long.Parse(_id);
                myObj.UserId = customer_code;
                myObj.StoreName = store_name;
                myObj.Address = store_to_address;
                myObj.ManagerName = store_manager_name;
                myObj.ManagerMobile = store_manager_mobile;
                myObj.ManagerEmail = store_manager_email;
                myObj.ProvinceCode = province_code;
                myObj.DistrictCode = district_code;
                myObj.PostCode = postcode;
                Configuration.Data.Save("profile_store", myObj);
                return_code = "00"; message_return = "Cập nhật thành công.";

            }
            catch
            {
                return_code = "01"; message_return = "Cập nhật thất bại.";
            }
            return Json(new { result = return_code, message = message_return });
        }
        //---
        [HttpPost]
        public JsonResult save_store(string customer_code, string _id, string store_name,
            string store_manager_name, string store_manager_mobile, string store_manager_email,
            string store_to_address, string province_code, string district_code, string postcode)
        {
            string return_code = ""; string message_return = "";
            dynamic myObj = new DynamicObj();
            try
            {
                if (!string.IsNullOrEmpty(_id))
                {
                    myObj = PayID.Portal.Areas.Merchant.Configuration.Data.Get("profile_store", Query.EQ("_id", long.Parse(_id)));
                }
                else
                {
                    myObj._id = PayID.Portal.Areas.Merchant.Configuration.Data.GetNextSquence("profile_store");
                    myObj.StoreCode = customer_code + PayID.Portal.Configuration.Data.GetNextSquence("profile_store" + customer_code).ToString("000");
                }
                myObj.UserId = customer_code;
                myObj.StoreName = store_name;
                myObj.Address = store_to_address;
                myObj.ManagerName = store_manager_name;
                myObj.ManagerMobile = store_manager_mobile;
                myObj.ManagerEmail = store_manager_email;
                myObj.ProvinceCode = province_code;
                myObj.DistrictCode = district_code;
                myObj.PostCode = postcode;
                Configuration.Data.Save("profile_store", myObj);
                return_code = "00"; message_return = "Cập nhật thành công.";
            }
            catch
            {
                return_code = "01"; message_return = "Cập nhật thất bại.";
            }
            return Json(new { result = return_code, message = message_return });
        }
        //Lưu thông tin thanh toán
        [HttpPost]
        public JsonResult Save_Payment(string customer_code, string _id, string payment_company_name, string payment_account_name, string payment_account_mst, string payment_mobile, string payment_to_address, string payment_drop_province, string payment_drop_district, string payment_customer_type)
        {
            string return_code = ""; string message_return = "";
            dynamic myObj = new DynamicObj();
            try
            {
                if (!string.IsNullOrEmpty(_id))
                {
                    myObj = PayID.Portal.Areas.Merchant.Configuration.Data.Get("profile_payment", Query.EQ("_id", long.Parse(_id)));
                }
                else
                {
                    myObj._id = PayID.Portal.Areas.Merchant.Configuration.Data.GetNextSquence("profile_payment");
                }
                myObj.UserId = customer_code;
                myObj.PaymentCompanyName = payment_company_name;
                myObj.PaymentAccountName = payment_account_name;
                myObj.PaymentAccountName = payment_account_mst;
                myObj.PaymentMobile = payment_mobile;
                myObj.PaymentAddress = payment_to_address;
                myObj.PaymentProvinceCode = payment_drop_province;
                myObj.PaymentDistrictCode = payment_drop_district;
                myObj.PaymentCustomerType = int.Parse(payment_customer_type);
                Configuration.Data.Save("profile_payment", myObj);
                return_code = "00"; message_return = "Cập nhật thành công.";
            }
            catch
            {
                return_code = "01"; message_return = "Cập nhật thất bại.";
            }
            return Json(new { result = return_code, message = message_return });
        }
        //Hàm cấu hình phí vận chuyển và hãng vận chuyển
        //[HttpPost]
        //public JsonResult InsertBussinessContract(string cuocPhiVC, string phiCOD, string hangVC, string dichVu)
        //{
        //    string ketqua = "1";

        //    var bussinessContract = new modelbusinesscontract();
        //    bussinessContract._id = "PA" + Session["CustomerCode"].ToString();
        //    bussinessContract.business_code = Session["CustomerCode"].ToString();
        //    bussinessContract.cod_method = phiCOD;
        //    bussinessContract.fee_method = cuocPhiVC;
        //    bussinessContract.reference_code = "S24";
        //    bussinessContract.service_code = dichVu;
        //    bussinessContract.system_historical_notes = "";
        //    bussinessContract.system_last_updated_by = "S24";
        //    bussinessContract.system_last_updated_time = DateTime.Now;
        //    bussinessContract.transport_code = hangVC;

        //    HttpClient client = new HttpClient();
        //    client.BaseAddress = new Uri(MongoHelper.UriStringOther);
        //    var response = client.PostAsJsonAsync("api/BussinessContract", bussinessContract).Result;

        //    return Json(new { result = ketqua });
        //}

        public JsonResult GetDistrictJson(string IdTinh)
        {
            var listDistrict = PayID.Portal.Areas.Metadata.Configuration.GetDistrictByProvinceCode(IdTinh);
            return Json(new SelectList(listDistrict, "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        //>>>>>????
        //static List<DichVu> getAllPeople()
        //{
        //    HttpClient client = new HttpClient();
        //    client.BaseAddress = new Uri(MongoHelper.UriStringOther);
        //    client.DefaultRequestHeaders.Accept.Clear();
        //    client.DefaultRequestHeaders.Accept.Add(
        //        new MediaTypeWithQualityHeaderValue("application/json"));
        //    var url = "api/Servicess";
        //    HttpResponseMessage response = client.GetAsync(url).Result;
        //    return response.Content.ReadAsAsync<List<DichVu>>().Result;
        //}
        //public JsonResult GetInfoService()
        //{
        //    List<DichVu> lstCreateLading = getAllPeople();
        //    return Json(lstCreateLading, JsonRequestBehavior.AllowGet);
        //}
        public JsonResult GetPayment(string customer_code)
        {
            dynamic myObj = PayID.Portal.Areas.Merchant.Configuration.Data.Get("profile_payment", Query.EQ("UserId", customer_code));
            if (myObj != null)
            {
                return Json(new
                {
                    payment_company_name = myObj.PaymentCompanyName,
                    payment_account_name = myObj.PaymentAccountName,
                    payment_account_mobile = myObj.PaymentMobile,
                    payment_address = myObj.PaymentAddress,
                    payment_province_code = myObj.PaymentProvinceCode,
                    payment_district_code = myObj.PaymentDistrictCode,
                    payment_customer_type = myObj.PaymentCustomerType
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new
                {
                    error_code = "01",
                    error_message = "Lấy dữ liệu thất bại"
                }, JsonRequestBehavior.AllowGet);
            }

        }
        //Hàm thiết lập tài khoản
        [HttpPost]
        public JsonResult save_account(string customer_type, string customer_code, string company_name, string shortname, string name, string email, string phone, string birthday, string sex, string tax, string contract, string address, string provincecode, string districtcode, string payment_type)
        {
            string myreturn = "";
            try
            {
                dynamic myObj = PayID.Portal.Areas.Merchant.Configuration.Data.Get("business_profile", Query.EQ("_id", customer_code));
                if (myObj == null)
                {
                    myObj = new DynamicObj();
                    myObj._id = PayID.Portal.Areas.Merchant.Configuration.Data.GetNextSquence("business_profile");
                }
                myObj.general_account_type = customer_type;
                myObj._id = customer_code;
                myObj.general_full_name = company_name;
                myObj.contact_name = name;
                myObj.general_email = email;
                myObj.contact_phone_work = phone;
                myObj.business_tax = tax;
                myObj.contract = contract;
                myObj.contact_address_address = address;
                myObj.contact_address_province = provincecode;
                myObj.contact_address_district = districtcode;
                myObj.general_short_name = shortname;
                myObj.payment_type = payment_type == "1" ? true : false;
                Configuration.Data.Save("business_profile", myObj);
                myreturn = "00";
            }
            catch
            {
                myreturn = "01";
            }
            return Json(new { result = myreturn });
        }
        public JsonResult GetAccount(string customer_code)
        {
            dynamic myObj = Configuration.Data.Get("business_profile", Query.EQ("_id", customer_code));
            if (myObj != null)
            {
                return Json(new
                {
                    type = myObj.general_account_type,
                    company_name = myObj.general_full_name,
                    shortname = myObj.general_short_name,
                    name = myObj.contact_name,
                    email = myObj.general_email,
                    phone = myObj.contact_phone_work,
                    birthday = myObj.birthday,
                    sex = myObj.sex,
                    tax = myObj.business_tax,
                    contract = myObj.contract,
                    address = myObj.contact_address_address,
                    provincecode = myObj.contact_address_province,
                    districtcode = myObj.contact_address_district,
                    paymenttype = myObj.payment_type
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new
                {
                    error_code = "01",
                    error_message = "Lấy dữ liệu thất bại"
                }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetWardsJson(string DistrictId)
        {
            var listWard = PayID.Portal.Areas.Metadata.Configuration.GetWardByDistrictId(DistrictId);
            return Json(new SelectList(listWard, "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPosJson(string DistrictId)
        {
            var listPos = PayID.Portal.Areas.Metadata.Configuration.GetPosByDistrictCoce(DistrictId);
            return Json(new SelectList(listPos, "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        public JsonResult StoreDelete(string p)
        {
            bool _isOK = false;
            try
            {
                _isOK = Configuration.Data.Delete("profile_store", int.Parse(p));
            }
            catch
            { }
            return Json(_isOK);
        }
        public JsonResult StoreDeleteMng(string p)
        {
            bool _isOK = false;
            try
            {
                _isOK = Configuration.Data.Delete("profile_store", int.Parse(p));
            }
            catch
            { }
            return Json(_isOK);
        }
        public JsonResult BankDelete(string p)
        {
            bool _isOK = false;
            try
            {
                _isOK = Configuration.Data.Delete("profile_bank", int.Parse(p));
            }
            catch
            { }
            return Json(_isOK);
        }
        public JsonResult ChangePassWord(string account, string old_password, string new_password)
        {
            try
            {
                dynamic profile = PayID.Portal.Areas.Metadata.Configuration.Data.Get("user", Query.EQ("_id", account));
                if (_checkPassWord(account, old_password))
                {
                    profile.password = PayID.Common.Security.CreatPassWordHash(new_password);
                    PayID.Portal.Areas.Metadata.Configuration.Data.Save("user", profile);
                }
                else
                    return Json(new
                    {
                        error_code = "01",
                        error_message = "Sai tên đăng nhập hoặc mật khẩu."
                    }, JsonRequestBehavior.AllowGet);
                return Json(new
                {
                    error_code = "00",
                    error_message = "Đổi mật khẩu thành công."
                }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new
                {
                    error_code = "",
                    error_message = "Lỗi truy vấn dữ liệu."
                }, JsonRequestBehavior.AllowGet);
            }

        }
        public bool _checkPassWord(string account, string old_password)
        {
            IMongoQuery _qId = Query.EQ("_id", account);
            IMongoQuery _qPassword = Query.EQ("password", PayID.Common.Security.CreatPassWordHash(old_password));
            dynamic profile = PayID.Portal.Areas.Metadata.Configuration.Data.Get("user", Query.And(_qId, _qPassword));

            if (profile == null) return false;
            return true;
        }
    }
}