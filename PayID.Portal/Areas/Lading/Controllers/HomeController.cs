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
using PayID.Portal.Common;
using PayID.Portal.Common.Service;
namespace PayID.Portal.Areas.Lading.Controllers
{
    public class HomeController : Controller
    {
        private const int defaultPageSize = 10;
        LadingService LADING_SERVICE = null;
        DictionaryService DICTIONARY_SERVICE = null;

        public HomeController()
        {
            if(LADING_SERVICE == null)
            {
                LADING_SERVICE = new LadingService();
            }

            if(DICTIONARY_SERVICE == null)
            {
                DICTIONARY_SERVICE = new DictionaryService();
            }
        }

        [HttpPost]
        public List<dynamic> GET_LIST_LADING(string v_code, string v_from_date, string v_to_date, string v_status, string v_to_province_code,string v_customer_code)
        {
            v_from_date = v_from_date.Substring(6, 4) + "-" + v_from_date.Substring(3, 2) + "-" + v_from_date.Substring(0, 2);
            v_to_date = v_to_date.Substring(6, 4) + "-" + v_to_date.Substring(3, 2) + "-" + v_to_date.Substring(0, 2);
            string unit_link = ((dynamic)Session["profile"]).unit_link;
            if (unit_link == "00") unit_link = "";
           if(unit_link.Length>2)
           {
               unit_link = unit_link.Substring(unit_link.Length-6,6);
           }
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
                var mylading = new
                {
                    code = v_code.Trim().ToUpper(),
                    from_date = String.IsNullOrEmpty(v_from_date) ? "" : DateTime.Parse(v_from_date).ToString("dd-MM-yyyy"),
                    to_date = String.IsNullOrEmpty(v_to_date) ? "" : DateTime.Parse(v_to_date).ToString("dd-MM-yyyy"),
                    status = v_status,
                    to_province_code = v_to_province_code,
                    customer_code = v_customer_code,
                    unit_link = unit_link
                };
                List<dynamic> list_lading = new List<dynamic>();
                try
                {
                    var response = client.PostAsJsonAsync("api/Lading", mylading).Result;
                    list_lading = response.Content.ReadAsAsync<IEnumerable<dynamic>>().Result.ToList();
                    list_lading = (from c in list_lading orderby c.DateCreated descending select c).ToList();
                }
                catch { }
                return list_lading;
            }
        }
        public ActionResult PrintLadingBill(string id)
        {
            if (Session["profile"] == null)
            {
                return RedirectToAction("LogOut", "Home", new { Area = "" });
            }
            else
            {
                long lTotal = 0;
                var ListLading = LADING_SERVICE.GetLading(id, "", "", "", "", "", 1, ref lTotal);
                Models.Lading oPRINT_LADING = new Models.Lading();

                if (ListLading != null && ListLading.Count > 0)
                {
                    oPRINT_LADING = ListLading[0];

                    if (!string.IsNullOrEmpty(oPRINT_LADING.FromProvinceCode))
                    {
                        var district = DICTIONARY_SERVICE.GetDistrictByProvince("", oPRINT_LADING.FromDistrictCode);
                        if(district!= null && district.Count > 0)
                        {
                            oPRINT_LADING.FromDistrictName = district[0].DistrictName;
                        }
                    }

                    if (!string.IsNullOrEmpty(oPRINT_LADING.ToDistrictCode))
                    {
                        var district = DICTIONARY_SERVICE.GetDistrictByProvince("", oPRINT_LADING.ToDistrictCode);
                        if (district != null && district.Count > 0)
                        {
                            oPRINT_LADING.ToDistrictName = district[0].DistrictName;
                        }
                    }

                    if (!string.IsNullOrEmpty(oPRINT_LADING.FromProvinceCode))
                    {
                        var province = Common.Configuration.ListProvince.FirstOrDefault(x => x.ProvinceCode == oPRINT_LADING.FromProvinceCode);
                        if(province != null)
                        {
                            oPRINT_LADING.FromProvinceName = province.Description;
                        }
                    }

                    if (!string.IsNullOrEmpty(oPRINT_LADING.ToProvinceCode))
                    {
                        var province = Common.Configuration.ListProvince.FirstOrDefault(x => x.ProvinceCode == oPRINT_LADING.ToProvinceCode);
                        if (province != null)
                        {
                            oPRINT_LADING.ToProvinceName = province.Description;
                        }
                    }

                }

                return View(oPRINT_LADING);
            }
        }


        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ListLading(string LadingCode, string FromDate, string ToDate, string Status, string ProvinceCode,string CustomerCode, int PageIndex)
        {
            long lTotal = 0;
            var listLading = LADING_SERVICE.GetLading(LadingCode.Trim(), FromDate, ToDate, Status, ProvinceCode, CustomerCode.Trim(), PageIndex, ref lTotal);

            ViewBag.PageIndex = PageIndex;
            ViewBag.PageSize = Common.Configuration.PageSize;
            ViewBag.ToTal = lTotal;

            return PartialView(listLading);
        }
        public ActionResult Index(string employee_name)
        {
            if (Session["profile"] == null)
            {
                return RedirectToAction("LogOut", "Home", new { Area = "" });
            }
            else
            {
                List<SelectListItem> ListProvince = new List<SelectListItem>();
                ListProvince.Add(new SelectListItem
                {
                    Text = "---Tỉnh, Thành phố---",
                    Value = ""
                });

                List<SelectListItem> ListStatus = new List<SelectListItem>();
                ListStatus.Add(new SelectListItem
                {
                    Text = "---Trạng thái---", 
                    Value = ""
                });

                foreach(var item in Common.Configuration.ListProvince)
                {
                    ListProvince.Add(new SelectListItem
                    {
                        Text = item.Description,
                        Value = item.ProvinceCode
                    });
                }

                foreach(var item in Common.Configuration.ListStatus)
                {
                    ListStatus.Add(new SelectListItem
                    {
                        Text = item.StatusDescription,
                        Value = item.StatusCode
                    });
                }

                ViewBag.ListStatus = ListStatus;
                ViewBag.ListProvince = ListProvince;
                ViewBag.Title = "Quản lý vận đơn";
                return View();
            }

        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult LadingCancel(string LadingCode)
        {
            JsonResult jResult = new JsonResult();

            try
            {
                if (Session["profile"] == null)
                {
                    jResult = Json(new { Code = "-101", Mes = "Đăng nhập lại trước khi tiếp tục thao tác." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string jReturn = LADING_SERVICE.LadingCancel(LadingCode);

                    string Code = jReturn.Split('|')[0].ToString().Trim();
                    string Mes = jReturn.Split('|')[1].ToString().Trim();

                    jResult = Json(new { Code = Code.ToString(), Mes = Mes.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }
            catch(Exception ex)
            {
                jResult = Json(new { Code = "-105", Mes = "Có lỗi trong quá trình xử lý dữ liệu" }, JsonRequestBehavior.AllowGet);
            }

            return jResult;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult LadingEdit(string Code)
        {
            if (Session["profile"] == null)
            {
                return RedirectToAction("LogOut", "Home", new { Area = "" });
            }
            else
            {
                long lTotal = 0;
                var ListLading = LADING_SERVICE.GetLading(Code, "", "", "", "", "", 1, ref lTotal);
                List<SelectListItem> ListProvince = new List<SelectListItem>();

                ListProvince.Add(new SelectListItem
                {
                    Text = "---Tỉnh, Thành phố---",
                    Value = ""
                });

                foreach (var item in Common.Configuration.ListProvince)
                {
                    ListProvince.Add(new SelectListItem
                    {
                        Text = item.Description,
                        Value = item.ProvinceCode
                    });
                }

                ViewBag.ListProvince = ListProvince;
                return PartialView(ListLading[0]);
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult LadingDetail(string LadingCode)
        {
            if (Session["profile"] == null)
            {
                return RedirectToAction("LogOut", "Home", new { Area = "" });
            }
            else
            {
                long lTotal = 0;
                var ListLading = LADING_SERVICE.GetLading(LadingCode, "", "", "", "", "", 1, ref lTotal);

                Models.Lading oLading = ListLading[0];

                List<SelectListItem> ListProvince = new List<SelectListItem>();
                List<SelectListItem> ListFromDistrict = new List<SelectListItem>();
                List<SelectListItem> ListToDistrict = new List<SelectListItem>();
                List<SelectListItem> ListFromWard = new List<SelectListItem>();
                List<SelectListItem> ListToWard = new List<SelectListItem>();
                List<SelectListItem> ListServiceType = new List<SelectListItem>();

                ListProvince.Add(new SelectListItem
                {
                    Text = "---Tỉnh, Thành phố---",
                    Value = ""
                });

                foreach (var item in Common.Configuration.ListProvince)
                {
                    ListProvince.Add(new SelectListItem
                    {
                        Text = item.Description,
                        Value = item.ProvinceCode
                    });
                }

                ListFromDistrict.Add(new SelectListItem
                {
                    Text = "---Quận, Huyện---",
                    Value = ""
                });

                if (!string.IsNullOrEmpty(oLading.FromProvinceCode))
                {
                    var ListDistrict = DICTIONARY_SERVICE.GetDistrictByProvince(oLading.FromProvinceCode);

                    if (ListDistrict != null && ListDistrict.Count > 0)
                    {
                        foreach (var item in ListDistrict)
                        {
                            ListFromDistrict.Add(new SelectListItem
                            {
                                Text = item.Description,
                                Value = item.DistrictCode
                            });
                        }
                    }
                }

                ListToDistrict.Add(new SelectListItem
                {
                    Text = "---Quận, Huyện---",
                    Value = ""
                });

                if (!string.IsNullOrEmpty(oLading.ToProvinceCode))
                {
                    var ListDistrict = DICTIONARY_SERVICE.GetDistrictByProvince(oLading.ToProvinceCode);

                    if (ListDistrict != null && ListDistrict.Count > 0)
                    {
                        foreach (var item in ListDistrict)
                        {
                            ListToDistrict.Add(new SelectListItem
                            {
                                Text = item.Description,
                                Value = item.DistrictCode
                            });
                        }
                    }
                }

                ListFromWard.Add(new SelectListItem
                {
                    Text = "---Xã, Phường---",
                    Value = ""
                });

                if (!string.IsNullOrEmpty(oLading.FromDistrictCode))
                {
                    var ListWard = DICTIONARY_SERVICE.GetWardByDistrict(oLading.FromDistrictCode);

                    if (ListWard != null && ListWard.Count > 0)
                    {
                        foreach (var item in ListWard)
                        {
                            ListFromWard.Add(new SelectListItem
                            {
                                Text = item.WardName,
                                Value = item.WardCode
                            });
                        }
                    }
                }

                ListToWard.Add(new SelectListItem
                {
                    Text = "---Xã, Phường---",
                    Value = ""
                });

                if (!string.IsNullOrEmpty(oLading.ToDistrictCode))
                {
                    var ListWard = DICTIONARY_SERVICE.GetWardByDistrict(oLading.ToDistrictCode);

                    if (ListWard != null)
                    {
                        foreach (var item in ListWard)
                        {
                            ListToWard.Add(new SelectListItem
                            {
                                Text = item.WardName,
                                Value = item.WardCode
                            });
                        }
                    }
                }

                foreach (var item in Common.Configuration.ListService)
                {
                    ListServiceType.Add(new SelectListItem
                    {
                        Text = item.ServiceName,
                        Value = item.ServiceId
                    });
                }

                ViewBag.ListProvince = ListProvince;
                ViewBag.ListFromDistrict = ListFromDistrict;
                ViewBag.ListToDistrict = ListToDistrict;
                ViewBag.ListFromWard = ListFromWard;
                ViewBag.ListToWard = ListToWard;
                ViewBag.ListServiceType = ListServiceType;


                return View(oLading);
            }
        }

        public JsonResult UpdateServiceLadingbill(string id, string type)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            string ngayHT = DateTime.Now.ToString();

            string newps = "id=" + id + "&&" + "type=" + type;
            var url = "api/UpdateServiceFastSlow?" + newps;
            HttpResponseMessage response = client.GetAsync(url).Result;
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult LadingEdit(string code)
        //{
        //    dynamic myDynamic = new PayID.DataHelper.DynamicObj();
        //    myDynamic = Configuration.Data.Get("Lading", Query.EQ("Code", code));
        //    ViewBag.stateList = new SelectList(PayID.Portal.Areas.Merchant.Configuration.List_Item_Province, "Value", "Text", myDynamic.ToProvinceCode);
        //    return View(myDynamic);
        //}        
        public JsonResult LadingAccept(string code)
        {
            bool v_return = false; bool sReturn = false;
            dynamic myObj = Configuration.Data.Get("Lading", Query.EQ("Code", code));
            try
            {
                if (myObj != null)
                {
                    myObj.Status = "C5";
                    v_return = Configuration.Data.Save("Lading", myObj);
                }
                dynamic _jouey = Configuration.Data.Get("LogJourney", Query.And(Query.EQ("Code", code), Query.EQ("Status", "C5")));
                  if (_jouey == null)
                  {
                      dynamic dyna = new PayID.DataHelper.DynamicObj();
                      dyna._id = Configuration.Data.GetNextSquence("LogJourney");
                      dyna.Code = code;
                      dyna.Status = "C5";
                      dyna.UserId = Session["CustomerCode"];
                      dyna.Location = "Cash@Post";
                      dyna.Note = "Chấp nhận";
                      dyna.DateCreate = DateTime.Now;
                      sReturn = Configuration.Data.Save("LogJourney", dyna);
                  }
            }
            catch
            {
            }
            return Json(v_return);
        }
        public JsonResult LadingBillDelete(string code)
        {
            bool v_return = false;
            dynamic myObj = Configuration.Data.Get("Lading", Query.EQ("Code", code));
            if (myObj != null)
            {
                myObj.Delete = 1;
                v_return = Configuration.Data.Save("Lading", myObj);
            }

            return Json(v_return, JsonRequestBehavior.AllowGet);

        }
        //public JsonResult LadingCancel(string code)
        //{
        //    bool v_return = false;
        //    bool v_cancel_lading = false;
        //    bool v_cancel_request = false;

        //    dynamic myObj = Configuration.Data.Get("Lading", Query.EQ("Code", code));
        //    dynamic reqObj = Configuration.Data.Get("shipment", Query.EQ("tracking_code", code));

        //    if (myObj != null)
        //    {
        //        if (myObj.Status != null && myObj.Status == "C1")
        //        {
        //            myObj.Status = "C2";
        //        }
        //        else 
        //        {
        //            myObj.Status = "C8";
        //        }
        //        v_cancel_lading = Configuration.Data.Save("Lading", myObj);
        //    }

        //    if (reqObj != null)
        //    {
        //        if (reqObj.system_status != null && reqObj.system_status == "C1")
        //        {
        //            reqObj.system_status = "C2";
        //        }
        //        else
        //        {
        //            reqObj.system_status = "C8";
        //        }

        //        v_cancel_request = Configuration.Data.Save("shipment", reqObj);
        //    }

        //    if (v_cancel_request == true && v_cancel_request==true)
        //    {
        //        v_return = true;
        //    }

        //    return Json(v_return, JsonRequestBehavior.AllowGet);
        //}
        public JsonResult GetFee(string service_code, string value, string from_province_code, string to_province_code, string weight)
        {
            dynamic dynamic_v = new PayID.DataHelper.DynamicObj();
            // Kiểm tra điều kiện tính cước from - to - weight.
            if (!String.IsNullOrEmpty(from_province_code) && !String.IsNullOrEmpty(to_province_code) && !String.IsNullOrEmpty(weight))
            {
                string v_value = value.Replace(".", "").Replace(",", "");
                string v_weight = weight.Replace(".", "").Replace(",", "");

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var obj_request = new
                    {
                        ServiceCode = service_code,
                        FromProvinceCode = from_province_code,
                        ToProvinceCode = to_province_code,
                        Value = value.Replace(".", "").Replace(",", ""),
                        Weight = weight.Replace(".", "").Replace(",", ""),
                    };

                    var response = client.PostAsJsonAsync("api/Charge/QueryPostage", obj_request).Result;
                    dynamic_v = response.Content.ReadAsAsync<JObject>().Result;
                  
                }
                long mainfee = dynamic_v.MainFee;
                long codfee = dynamic_v.CodFee;
                long servicefee = dynamic_v.ServiceFee;
                long totalfee = long.Parse(v_value) + mainfee;

                return Json(new
                {
                    MainFee = string.Format("{0:###,###,###}", mainfee),
                    CodFee = string.Format("{0:###,###,###}", codfee),
                    ServiceFee = string.Format("{0:###,###,###}", servicefee),
                    TotalFee = string.Format("{0:###,###,###}", totalfee)
                }, JsonRequestBehavior.AllowGet);
            }
            else

                return Json(new
                {
                    MainFee = 0,
                    CodFee = 0,
                    ServiceFee = 0,
                    TotalFee = 0
                }, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult SaveLading(string tenSP, string giaTri, string khoiLuong, string soLuong, string moTaSP, string ten, string ten_old, string buuCucNhan, string soDienThoai, string soDienThoai_old, string diaChi, string diaChi_old, int id, string tenSP_old, string giaTri_old, string khoiLuong_old, string soLuong_old, string moTaSP_old,  string buuCucNhan_old,  string MainFee, string MainFee_old, string CodFee, string CodFee_old, string ServiceFee, string ServiceFee_old, string TotalFee, string TotalFee_old)
        {
            Return rs = new Return();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var pValue = (!String.IsNullOrEmpty(giaTri)) ? giaTri.Replace(",", "") : "0";
                var pValue_old = (!String.IsNullOrEmpty(giaTri_old)) ? giaTri_old.Replace(",", "") : "0";
                var pWeight = (!String.IsNullOrEmpty(khoiLuong)) ? khoiLuong.Replace(",", "") : "0";
                var pWeight_old = (!String.IsNullOrEmpty(khoiLuong_old)) ? khoiLuong_old.Replace(",", "") : "0";
                var pQuantity = (!String.IsNullOrEmpty(soLuong)) ? soLuong.Replace(",", "") : "0";
                var pQuantity_old = (!String.IsNullOrEmpty(soLuong_old)) ? soLuong_old.Replace(",", "") : "0";
                var pMainFee = (!String.IsNullOrEmpty(MainFee)) ? MainFee.Replace(",", "") : "0";
                var pMainFee_old = (!String.IsNullOrEmpty(MainFee_old)) ? MainFee_old.Replace(",", "") : "0";
                var pCodFee = (!String.IsNullOrEmpty(CodFee)) ? CodFee.Replace(",", "") : "0";
                var pCodFee_old = (!String.IsNullOrEmpty(CodFee_old)) ? CodFee_old.Replace(",", "") : "0";
                var pServiceFee = (!String.IsNullOrEmpty(ServiceFee)) ? ServiceFee.Replace(",", "") : "0";
                var pServiceFee_old = (!String.IsNullOrEmpty(ServiceFee_old)) ? ServiceFee_old.Replace(",", "") : "0";
                var pTotalFee = long.Parse(pValue) + long.Parse(pMainFee);
                var pTotalFee_old = (!String.IsNullOrEmpty(TotalFee_old)) ? TotalFee_old.Replace(",", "") : "0";

                var obj_request = new
                {
                    _id = id,
                    ProductName = tenSP,
                    ProductName_old = tenSP_old,

                    Value = pValue,
                    Value_old = pValue_old,

                    Weight = pWeight,
                    Weight_old = pWeight_old,

                    Quantity = pQuantity,
                    Quantity_old = pQuantity_old,

                    ProductDescription = moTaSP,
                    ProductDescription_old = moTaSP_old,

                    ReceiverName = ten,
                    ReceiverAddress = diaChi,
                    ReceiverMobile = soDienThoai,

                    ReceiverName_old = ten_old,
                    ReceiverAddress_old = diaChi_old,
                    ReceiverMobile_old = soDienThoai_old,

                    ToProvinceCode = buuCucNhan,
                    ToProvinceCode_old = buuCucNhan_old,

                    MainFee = pMainFee,
                    MainFee_old = pMainFee_old,
                    ServiceFee = pServiceFee,
                    ServiceFee_old = pServiceFee_old,
                    CodFee = pCodFee,
                    CodFee_old = pCodFee_old,
                    TotalFee = pTotalFee,
                    TotalFee_old = pTotalFee_old
                };

                var response = client.PostAsJsonAsync("api/Lading/Lading?function=update_lading", obj_request).Result;

                rs = response.Content.ReadAsAsync<Return>().Result;
            }
            return Json(rs, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SetDate()
        {
            DateTime now = DateTime.Now;
            TimeSpan timespan1 = new TimeSpan(2, 12, 0, 0);
            TimeSpan timespan2 = new TimeSpan(12, 12, 0, 0);
            TimeSpan quakhu = timespan1 + timespan2;
            DateTime past = now - quakhu - quakhu;
            var obj_request = new
              {
                  v_from_date = past.ToString("yyyy-MM-dd"),
                  v_to_date = now.ToString("yyyy-MM-dd")
              };
            return Json(obj_request, JsonRequestBehavior.AllowGet);
        }      
        public ActionResult LadingTrackTrace(string code)
        {
            var ListLogJourney = LADING_SERVICE.GetLogJourney(code);
            return PartialView(ListLogJourney);
        }      

        public ActionResult GetService(string code)
        {
            List<dynamic> LIST = GET_LIST_LADING(code, "", "", "", "","");
            return View(LIST);
        }
        public JsonResult ListService(string code)
        {
            List<dynamic> LIST = GET_LIST_LADING(code, "", "", "", "","");
            return Json(LIST, JsonRequestBehavior.AllowGet);
        }
    }
}
