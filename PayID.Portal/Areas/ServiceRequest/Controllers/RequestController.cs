using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PayID.DataHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Dynamic;
using System.Web.UI.WebControls;
using System.IO;
using System.Web.UI;
using System.Text;
using Microsoft.Reporting.WebForms;
using PayID.Common;
using PayID.Portal.Models;
using PayID.Portal.Common.Service;
namespace PayID.Portal.Areas.ServiceRequest.Controllers
{
    public class RequestController : Controller
    {

        AccountService ACCOUNT_SERVICE = null;
        ShipmentService SHIPMENT_SERVICE = null;

        public RequestController()
        {
            if(ACCOUNT_SERVICE == null)
            {
                ACCOUNT_SERVICE = new AccountService();
            }

            if(SHIPMENT_SERVICE == null)
            {
                SHIPMENT_SERVICE = new ShipmentService();
            }
        }

        [Authorize]
        public ActionResult Index()
        {
            if (Session["profile"] == null)
            {
                dynamic profile = Configuration.Data.Get("user", Query.EQ("_id", User.Identity.Name));
                Session["profile"] = profile;
            }
            return View();
        }

        [Authorize]
        public ActionResult CreateRequest()
        {
            if (Session["profile"] == null)
            {
                return RedirectToAction("LogOut", "Home", new { Area = "" });
            }
            else
            {
                Shipment oShipment = new Shipment();
                oShipment.sAmount = "0";
                oShipment.sQuantity = "0";
                oShipment.sWeight = "0";

                ViewBag.Title = "Tạo yêu cầu";
                List<SelectListItem> ListProvince = new List<SelectListItem>();
                List<SelectListItem> ListDistrict = new List<SelectListItem>();
                List<SelectListItem> ListWard = new List<SelectListItem>();
                List<SelectListItem> ListService = new List<SelectListItem>();
                List<SelectListItem> ListStore = new List<SelectListItem>();

                ListProvince.Add(new SelectListItem
                {
                    Text = "---Tỉnh, Thành phố---",
                    Value = ""
                });

                ListDistrict.Add(new SelectListItem
                {
                    Text = "---Quận, Huyện---",
                    Value = ""
                });

                ListWard.Add(new SelectListItem
                {
                    Text = "---Xã, Phường---",
                    Value = ""
                });

                ListStore.Add(new SelectListItem
                {
                    Text = "---Kho hàng---",
                    Value = ""
                });

                foreach (var item in Common.Configuration.ListProvince)
                {
                    ListProvince.Add(new SelectListItem
                    {
                        Text = item.ProvinceName,
                        Value = item.ProvinceCode
                    });
                }

                foreach (var item in Common.Configuration.ListService)
                {
                    ListService.Add(new SelectListItem
                    {
                        Text = item.ServiceName,
                        Value = item.ServiceId
                    });
                }

                ViewBag.ListProvince = ListProvince;
                ViewBag.ListDistrict = ListDistrict;
                ViewBag.ListWard = ListWard;
                ViewBag.ServiceType = ListService;
                ViewBag.ListStore = ListStore;

                return View(oShipment);
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult CreateRequest(Shipment oShipment)
        {
            JsonResult jResult = new JsonResult();
            if (Session["profile"] == null)
            {
                jResult = Json(new { Code = "-101", Mes = "Hãy đăng nhập lại trước khi tiếp tục thao tác." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                bool bResult = false;

                Account oAccount = (Account)Session["profile"];

                oShipment.Weight = long.Parse("0" + (oShipment.sWeight ?? "").Replace(",", "").Replace(".", ""));
                oShipment.Quantity = int.Parse("0" + (oShipment.sQuantity ?? "").Replace(",", "").Replace(".", ""));
                oShipment.Amount = long.Parse("0" + (oShipment.sAmount ?? "").Replace(",", "").Replace(".", ""));
                oShipment.UserCreate = oAccount.UserName;
                oShipment.UserCreateName = oAccount.FullName;
                oShipment.UnitCreate = oAccount.UnitCode;
                oShipment.UnitCreateName = oAccount.UnitName;
                oShipment.UnitLink = oAccount.UnitLink.Length == 2 && oAccount.UnitLink != "00" ? "00." + oAccount.UnitLink : oAccount.UnitLink;
                oShipment.UnitName = oAccount.UnitName;

                var sResult = SHIPMENT_SERVICE.UpdateShipment(oShipment, ref bResult);

                string Code = sResult.Split('|')[0].ToString();
                string Mes = sResult.Split('|')[1].ToString();

                jResult = Json(new { Code = Code, Mes = Mes }, JsonRequestBehavior.AllowGet);
            }

            return jResult;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult LastestRequestByCus(string CustomerCode)
        {
            long lTotal = 0;
            List<ShipmentAPI> ListShipment = new List<ShipmentAPI>();
            ListShipment = SHIPMENT_SERVICE.GetShipment(CustomerCode, "", "", "", "", "", "", 0, 0, Common.Param.C5, 1, 5,ref lTotal);
            return PartialView(ListShipment);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        [Authorize]
        public ActionResult List(string status)
        {
            if (Session["profile"] == null)
            {
                return RedirectToAction("LogOut", "Home", new { Area = "" });
            }
            else
            {
                List<SelectListItem> ListStatus = new List<SelectListItem>();
                ListStatus.Add(new SelectListItem
                {
                    Text = "---Trạng thái---",
                    Value = ""
                });

                foreach(var item in Common.Configuration.ListStatus)
                {
                    ListStatus.Add(new SelectListItem
                    {
                        Text = item.StatusDescription,
                        Value = item.StatusCode
                    });
                }

                ViewBag.ListStatus = ListStatus;

                ViewBag.status = status;
                ViewBag.Title = "Danh sách điều tin";
                return View();
            }

        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetOrder(string FromDate, string ToDate, string CustomerCode, string Status, string OrderCode, string TrackingCode, int PageIndex)
        {
            if(Session["profile"] == null)
            {
                var Account = ACCOUNT_SERVICE.GetAccountByUserName(User.Identity.Name);
                Session["profile"] = Account;
            }

            Account oAccount = (Account)Session["profile"];
            long lFromDate = 0;
            long lToDate = 0;
            long lTotal = 0;


            if (!string.IsNullOrEmpty(FromDate))
            {
                lFromDate = long.Parse("0" + FromDate.Split('/')[2].ToString().Trim() + FromDate.Split('/')[1].ToString().Trim() + FromDate.Split('/')[0].ToString().Trim());
            }

            if (!string.IsNullOrEmpty(ToDate))
            {
                lToDate = long.Parse("0" + ToDate.Split('/')[2].ToString().Trim() + ToDate.Split('/')[1].ToString().Trim() + ToDate.Split('/')[0].ToString().Trim());
            }

            string sUnitLink = oAccount.UnitLink;

            if(oAccount.UnitLink == "00")
            {
                sUnitLink = "";
            }
            //else if(oAccount.UnitLink.Length == 2)
            //{
            //    sUnitLink = "00.";
            //}

            var list = SHIPMENT_SERVICE.GetShipment(CustomerCode.Trim(), "", "", "", sUnitLink.Trim(), OrderCode.Trim(), TrackingCode.Trim(), lFromDate, lToDate, Status, PageIndex, Common.Configuration.PageSize, ref lTotal);

            ViewBag.PageIndex = PageIndex;
            ViewBag.PageSize = Common.Configuration.PageSize;
            ViewBag.ToTal = lTotal;

            return PartialView(list);
        }

        [AcceptVerbs("Get")]
        public JsonResult ListPostMan(string UnitCode)
        {
            JsonResult jResult = new JsonResult();

            List<Account> ListAccount = new List<Account>();
            ListAccount = ACCOUNT_SERVICE.GetAccountByUnitCode(UnitCode);

            List<SelectListItem> ListPostMan = new List<SelectListItem>();
            ListPostMan.Add(new SelectListItem
            {
                Text = "---Bưu tá Thu gom---",
                Value = ""
            });

            if (ListAccount != null)
            {
                foreach(var item in ListAccount)
                {
                    ListPostMan.Add(new SelectListItem
                    {
                        Text = item.FullName,
                        Value = item.UserName
                    });
                }
            }

            jResult = Json(new { data = ListPostMan }, JsonRequestBehavior.AllowGet);

            return jResult;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult Assign(string ShipmentId, string UnitCode, string UnitName, string PostMan, string PostManName, string Notes, string TypeAssign)
        {
            JsonResult jResult = new JsonResult();
            if (Session["profile"] == null)
            {
                jResult = Json(new { Code = "-101", Mes = "Hãy đăng nhập lại để tiếp tục thao tác tiếp." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                Account oAccount = (Account)Session["profile"];

                List<Assign> ListAssign = new List<Assign>();

                if (string.IsNullOrEmpty(TypeAssign))
                {
                    ListAssign.Add(new Assign
                    {
                        ShipmentId = ShipmentId,
                        Amnd_User = oAccount.UserName,
                        Note = Notes,
                        PostMan = PostMan,
                        PostManName = PostManName,
                        UnitCode = UnitCode,
                        UnitName = UnitName
                    });
                }
                else
                {
                    var arrShipmentId = ShipmentId.Split('|');

                    foreach(var item in arrShipmentId)
                    {
                        if(!string.IsNullOrEmpty(item))
                        {
                            ListAssign.Add(new Assign
                            {
                                ShipmentId = item.ToString().Trim(),
                                Amnd_User = oAccount.UserName,
                                Note = Notes,
                                PostMan = PostMan,
                                PostManName = PostManName,
                                UnitCode = UnitCode,
                                UnitName = UnitName
                            });
                        }
                    }
                }

                List<string> ListReturn = new List<string>();
                string sReturn = SHIPMENT_SERVICE.Assign(ListAssign, ref ListReturn);

                string Code = sReturn.Split('|')[0].ToString();
                string Mes = sReturn.Split('|')[1].ToString();

                jResult = Json(new { Code = Code, Mes = Mes }, JsonRequestBehavior.AllowGet);
            }

            return jResult;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ConfirmAssignCancel()
        {
            return PartialView();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult AssignCancel(string ShipmentId, string Notes, string TypeAssign)
        {
            JsonResult jResult = new JsonResult();
            if(Session["profile"] == null)
            {
                jResult = Json(new { Code = "-101", Mes = "Hãy đăng nhập lại để tiếp tục thao tác tiếp." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                Account oAccount = (Account)Session["profile"];

                List<Assign> ListAssign = new List<Assign>();

                if (string.IsNullOrEmpty(TypeAssign))
                {
                    ListAssign.Add(new Assign
                    {
                        ShipmentId = ShipmentId,
                        Amnd_User = oAccount.UserName,
                        Note = Notes
                    });
                }
                else
                {
                    var arrShipmentId = ShipmentId.Split('|');
                    foreach (var item in arrShipmentId)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            ListAssign.Add(new Assign
                            {
                                ShipmentId = item.ToString().Trim(),
                                Amnd_User = oAccount.UserName,
                                Note = Notes
                            });
                        }
                    }
                }

                List<string> ListReturn = new List<string>();
                string sReturn = SHIPMENT_SERVICE.AssignCancel(ListAssign, ref ListReturn);

                string Code = sReturn.Split('|')[0].ToString();
                string Mes = sReturn.Split('|')[1].ToString();

                jResult = Json(new { Code = Code, Mes = Mes }, JsonRequestBehavior.AllowGet);
            }

            return jResult;
        }

        public ActionResult Detail(string id)
        {
            if (String.IsNullOrEmpty(id)) return RedirectToAction("List", "Request");
            dynamic myObj = new DynamicObj();
            try
            {
                dynamic request = Configuration.Data.Get("shipment", Query.Or(Query.EQ("_id", id), Query.EQ("tracking_code", id)));
                dynamic statusObj = Configuration.Data.Get("Status", Query.EQ("StatusCode", request.system_status));
                ViewBag.StatusDescription = statusObj.StatusDescription;
                ViewBag.Local = ((dynamic)Session["profile"]).unit_name;
                return View(request);
            }
            catch
            {
                myObj._id = id;
                return RedirectToAction("List", "Request");
            }
        }
        public ActionResult NavigateRequest(string id)
        {
            if (String.IsNullOrEmpty(id)) return RedirectToAction("List", "Request");
            dynamic myObj = new DynamicObj();
            try
            {
                dynamic request = Configuration.Data.Get("shipment", Query.Or(Query.EQ("_id", id), Query.EQ("tracking_code", id)));
                dynamic statusObj = Configuration.Data.Get("Status", Query.EQ("StatusCode", request.system_status));
                ViewBag.StatusDescription = statusObj.StatusDescription;
                ViewBag.Local = ((dynamic)Session["profile"]).unit_name;
                return View(request);
            }
            catch
            {
                myObj._id = id;
                return RedirectToAction("List", "Request");
            }
        }

        [Authorize]
        public ActionResult ListRequest(string from_date, string to_date, string customer_code, string route, string tracking_code, string request_id, string status, int? page)
        {
            from_date = from_date.Substring(6, 4) + "-" + from_date.Substring(3, 2) + "-" + from_date.Substring(0, 2);
            to_date = to_date.Substring(6, 4) + "-" + to_date.Substring(3, 2) + "-" + to_date.Substring(0, 2);

            if (Session["profile"] == null)
            {
                dynamic profile = Configuration.Data.Get("user", Query.EQ("_id", User.Identity.Name));
                Session["profile"] = profile;
            }
            int p = (page == null) ? 1 : (int)page;
            long total = 0;
            List<dynamic> list = new List<dynamic>();
            string unit_link = ((dynamic)Session["profile"]).unit_link;
            if (unit_link == "00") unit_link = "";
            string sUnit_Link = unit_link;
            if (unit_link.Length == 2)
            {
                sUnit_Link = "00." + unit_link;
            }
            List<dynamic> lst = new List<dynamic>();
            lst = Configuration.Data.List("business_profile", Query.Or(Query.EQ("contact_address_province", sUnit_Link), Query.EQ("contact_address_province", unit_link))).ToList<dynamic>();

            MongoDB.Bson.BsonRegularExpression reg = new MongoDB.Bson.BsonRegularExpression("^" + sUnit_Link);
            MongoDB.Bson.BsonRegularExpression regs = new MongoDB.Bson.BsonRegularExpression("^" + unit_link);
            IMongoQuery _query = Query.NE("_id", "");//Query.Matches("current_assigned", reg);
            _query = Query.And(_query, Query.Or(Query.Matches("current_assigned", reg), Query.Matches("current_assigned", regs)));
            //if (unit_link == "10")
            //{
            //    _query = Query.And(
            //    Query.GTE("system_time_key.date", long.Parse(DateTime.Parse(from_date).ToString("yyyyMMdd"))),
            //    Query.LTE("system_time_key.date", long.Parse(DateTime.Parse(to_date).ToString("yyyyMMdd")))
            //    );
            //}
            //else
            //{
            _query = Query.And(
            _query,
            Query.GTE("system_time_key.date", long.Parse(DateTime.Parse(from_date).ToString("yyyyMMdd"))),
            Query.LTE("system_time_key.date", long.Parse(DateTime.Parse(to_date).ToString("yyyyMMdd")))
            );
            //}


            //if (lst != null && lst.Count > 0 && unit_link == "10")
            //{
            //    IMongoQuery _queryOr;
            //    //string sCustCode = "";
            //    var documentIds = new BsonValue[lst.Count];

            //    for (int j = 0; j < lst.Count; j++)
            //    {                   
            //            documentIds[j] = lst[j]._id;                     
            //    }

            //    _queryOr = Query.In("customer.code", documentIds);
            //    _query = Query.And(_query, _queryOr);
            //}
            //else 
            if (!String.IsNullOrEmpty(customer_code))
            {
                _query = Query.And(
                    _query,
                    Query.EQ("customer.code", customer_code));
            }

            if (!String.IsNullOrEmpty(route) && route != "0")
            {
                if (route == "1")
                {
                    _query = Query.And(
                        _query,
                        Query.EQ("from_address.province", reg.ToString().Replace("/", "").Replace("^", "")), Query.EQ("to_address.province", reg.ToString().Replace("/", "").Replace("^", "")));
                }
                else
                {
                    _query = Query.And(
                        _query,
                        Query.NE("to_address.province", reg.ToString().Replace("/", "").Replace("^", "")));
                }
            }

            if (!String.IsNullOrEmpty(status))
            {
                if (status == "C5")
                {
                    _query = Query.And(
                        _query,
                         Query.Or(Query.EQ("system_status", "C5"), Query.EQ("system_status", "C25")));
                }
                else
                {
                    _query = Query.And(
                        _query,
                        Query.EQ("system_status", status));
                }
            }

            if (!String.IsNullOrEmpty(request_id))
            {
                _query = Query.And(
                    _query,
                    Query.EQ("_id", request_id));

            }
            if (!String.IsNullOrEmpty(tracking_code))
            {
                _query = Query.EQ("tracking_code", tracking_code);
            }
            MongoDB.Driver.IMongoSortBy _sort = MongoDB.Driver.Builders.SortBy.Descending("created_at").Ascending("from_address.province").Ascending("from_address.district").Descending("_id");
            var _list = Configuration.Data.ListPagging("shipment", _query, _sort, 10, p, out total);
            ViewBag.total_page = (total + 9) / 10;
            ViewBag.total_item = total;
            total = 0;
            ViewBag.status = status;
            int currentPageIndex = page.HasValue ? page.Value : 1;
            ViewBag.Page = (currentPageIndex - 1) * 10;
            //ViewBag.StatusArray = new string[] {"C1", "C8"};
            return View(_list);
        }

        public ActionResult ListRequestPrint(string from_date, string to_date, string customer_code, string tracking_code, string request_id, string status)
        {
            if (Session["profile"] == null)
            {
                dynamic profile = Configuration.Data.Get("user", Query.EQ("_id", User.Identity.Name));
                Session["profile"] = profile;
            }
            from_date = from_date.Substring(6, 4) + "-" + from_date.Substring(3, 2) + "-" + from_date.Substring(0, 2);
            to_date = to_date.Substring(6, 4) + "-" + to_date.Substring(3, 2) + "-" + to_date.Substring(0, 2);

            string mystring = "Danh sách điều tin";
            ViewBag.status = status;
            if (status == "C1")
                mystring = "Danh sách tin mới tạo";
            if (status == "C6")
                mystring = "Danh sách tin đang xử lý";
            if (status == "C10")
                mystring = "Danh sách tin hẹn lại";

            ViewBag.ProvinceName = "Bưu điện " + PayID.Portal.Areas.Merchant.Configuration.GetNameProvinceByProvinceCode(string.Format("{0}", ((dynamic)Session["profile"]).unit_code.Substring(0, 2))) + "-" + mystring;
            List<dynamic> list = new List<dynamic>();
            string unit_link = ((dynamic)Session["profile"]).unit_link;
            if (unit_link == "00") unit_link = "";
            MongoDB.Bson.BsonRegularExpression reg = new MongoDB.Bson.BsonRegularExpression("^" + unit_link);
            IMongoQuery _query = Query.Matches("current_assigned", reg);
            _query = Query.And(
                _query,
                Query.GTE("system_time_key.date", long.Parse(DateTime.Parse(from_date).ToString("yyyyMMdd"))),
                Query.LTE("system_time_key.date", long.Parse(DateTime.Parse(to_date).ToString("yyyyMMdd")))
                );
            if (!String.IsNullOrEmpty(customer_code))
            {
                _query = Query.And(
                    _query,
                    Query.EQ("customer.code", customer_code));
            }

            if (!String.IsNullOrEmpty(tracking_code))
            {
                _query = Query.And(
                    _query,
                    Query.EQ("tracking_code", tracking_code));
            }

            if (!String.IsNullOrEmpty(status))
            {
                _query = Query.And(
                    _query,
                    Query.EQ("system_status", status));
            }

            if (!String.IsNullOrEmpty(request_id))
            {
                _query = Query.And(
                    _query,
                    Query.EQ("_id", request_id));
            }
            long total = 0;
            MongoDB.Driver.IMongoSortBy _sort = MongoDB.Driver.Builders.SortBy.Descending("created_at").Ascending("from_address.province").Ascending("from_address.district").Descending("_id");
            var _list = Configuration.Data.ListPagging("shipment", _query, _sort, 200, 1, out total);
            foreach (dynamic il in _list)
            {
                il.FromProvinceName = PayID.Portal.Areas.Merchant.Configuration.GetNameProvinceByProvinceCode(il.from_address.province);
                il.ToProvinceName = PayID.Portal.Areas.Merchant.Configuration.GetNameProvinceByProvinceCode(il.to_address.province);
            }
            return View(_list);
        }
        public ActionResult ExportShipment(string from_date, string to_date, string customer_code, string tracking_code, string request_id, string status)
        {
            long total = 0;
            List<dynamic> list = new List<dynamic>();
            string unit_link = ((dynamic)Session["profile"]).unit_link;
            if (unit_link == "00") unit_link = "";
            MongoDB.Bson.BsonRegularExpression reg = new MongoDB.Bson.BsonRegularExpression("^" + unit_link);
            IMongoQuery _query = Query.Matches("current_assigned", reg);
            _query = Query.And(
                _query,
                Query.GTE("system_time_key.date", long.Parse(DateTime.Parse(from_date).ToString("yyyyMMdd"))),
                Query.LTE("system_time_key.date", long.Parse(DateTime.Parse(to_date).ToString("yyyyMMdd")))
                );
            if (!String.IsNullOrEmpty(customer_code))
            {
                _query = Query.And(
                    _query,
                    Query.EQ("customer.code", customer_code));
            }

            if (!String.IsNullOrEmpty(tracking_code))
            {
                _query = Query.And(
                    _query,
                    Query.EQ("tracking_code", tracking_code));
            }

            if (!String.IsNullOrEmpty(status))
            {
                _query = Query.And(
                    _query,
                    Query.EQ("system_status", status));
            }

            if (!String.IsNullOrEmpty(request_id))
            {
                _query = Query.And(
                    _query,
                    Query.EQ("_id", request_id));
            }
            MongoDB.Driver.IMongoSortBy _sort = MongoDB.Driver.Builders.SortBy.Ascending("from_address.province").Ascending("from_address.district").Descending("_id");
            var _list = Configuration.Data.ListPagging("shipment", _query, _sort, 500, 1, out total);
            List<RequestToExport> listE = new List<RequestToExport>();
            foreach (dynamic e in _list)
            {
                listE.Add(new RequestToExport
                {
                    RequestCode = e._id,
                    FromAddress = e.from_address.address
                });
            }
            GridView gv = new GridView();
            gv.DataSource = listE;
            gv.DataBind();
            return new DownloadFileActionResult(gv, "Shipment.xls");
        }
        internal class RequestToExport
        {
            public string RequestCode { get; set; }
            public string FromAddress { get; set; }
        }
        public JsonResult Assign1(string ids, string notes, string unit_code, string unit_name, string pm)
        {
            var _ids = ids.Split('|');
            foreach (var _id in _ids)
            {
                if (String.IsNullOrEmpty(_id)) break;
                dynamic _request = Configuration.Data.Get("shipment", Query.EQ("_id", _id));
                List<PayID.DataHelper.DynamicObj> _comment = new List<PayID.DataHelper.DynamicObj>();
                _comment.AddRange(_request.comments);
                dynamic _new_comment = new PayID.DataHelper.DynamicObj();
                _new_comment.by = User.Identity.Name;
                _new_comment.at = DateTime.Now.ToString("yyyyMMddHHmmss");
                _new_comment.comment = "Phân cho " + unit_name + " xử lý<br/>" + notes;
                _new_comment.notes = notes;
                _comment.Add(_new_comment);
                _request.comments = _comment.ToArray();
                List<PayID.DataHelper.DynamicObj> _assigned = new List<PayID.DataHelper.DynamicObj>();
                _assigned.AddRange(_request.assigned_to);

                dynamic _new_assigned = new PayID.DataHelper.DynamicObj();
                _new_assigned.assign_by = User.Identity.Name;
                _new_assigned.assign_at = DateTime.Now.ToString("yyyyMMddHHmmss");
                _new_assigned.assign_to_id = unit_code;
                _new_assigned.assign_to_full_name = unit_name;
                _new_assigned.notes = notes;
                _assigned.Add(_new_assigned);
                _request.assigned_to = _assigned.ToArray();
                _request.current_assigned = unit_code;
                _request.current_postman = pm;
                _request.current_assigned_name = unit_name;
                _request.system_status = "C6";
                _request.is_assigned = 1;

                dynamic lading = new DynamicObj();

                lading = Configuration.Data.Get("Lading", Query.EQ("Code", _request.tracking_code == null ? "" : _request.tracking_code.ToString()));

                if (lading != null)
                {
                    lading.Status = "C6";
                    Configuration.Data.Save("Lading", lading);
                }
                Configuration.Data.Save("shipment", _request);
            }
            return Json(new { response_code = "00", response_message = "Phân hướng xử lý thành công" }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ReturnAssign(string id)
        {
            if (Session["profile"] == null)
            {
                dynamic profile = Configuration.Data.Get("user", Query.EQ("_id", User.Identity.Name));
                Session["profile"] = profile;
            }

            if (Session["profile"] == null)
                return Redirect("~/Home/SignOut");

            ViewBag.ID = id;
            return View();
        }
        public JsonResult CancelAssign(string id, string notes)
        {

            if (String.IsNullOrEmpty(id)) return Json(new { response_code = "01", response_message = "Hủy phân hướng không thành công" }, JsonRequestBehavior.AllowGet);
            dynamic _request = Configuration.Data.Get("shipment", Query.EQ("_id", id));
            int i = 0;
            foreach (dynamic item in _request.assigned_to)
            {
                i++;
                if (i == _request.assigned_to.Length)
                    item.notes = notes;
            }
            _request.current_assigned = _request.current_assigned.ToString().Substring(0, 2);
            dynamic _unit = Configuration.Data.Get("mbcUnit", Query.EQ("UnitCode", _request.current_assigned.ToString().Substring(0, 2)));
            _request.current_assigned_name = _unit.UnitName;
            _request.system_status = "C5";
            _request.assign_notes = notes;
            _request.is_assigned = 0;
            dynamic lading = new DynamicObj();
            lading = Configuration.Data.Get("Lading", Query.EQ("Code", _request.tracking_code == null ? "" : _request.tracking_code.ToString()));

            if (lading != null)
            {
                lading.Status = "C5";
                Configuration.Data.Save("Lading", lading);
                dynamic _jouey = PayID.Portal.Areas.Lading.Configuration.Data.Get("LogJourney", Query.And(Query.EQ("Code", _request.tracking_code == null ? "" : _request.tracking_code.ToString()), Query.EQ("Status", "C6")));
                if (_jouey != null)
                {
                    PayID.Portal.Areas.Lading.Configuration.Data.Delete("LogJourney", _jouey._id);
                }
            }
            Configuration.Data.Save("shipment", _request);

            return Json(new { response_code = "00", response_message = "Hủy phân hướng xử lý thành công" }, JsonRequestBehavior.AllowGet);
        }
        
        #region Confirm
        public JsonResult Confirm(string ids, string notes, string status, string confirm_date, string customer, string reason)
        {
            var _ids = ids.Split('|');
            foreach (var _id in _ids)
            {
                if (String.IsNullOrEmpty(_id)) break;
                dynamic _request = Configuration.Data.Get("shipment", Query.EQ("_id", _id));
                List<PayID.DataHelper.DynamicObj> _comment = new List<PayID.DataHelper.DynamicObj>();
                _comment.AddRange(_request.comments);
                dynamic _new_comment = new PayID.DataHelper.DynamicObj();
                _new_comment.by = User.Identity.Name;
                _new_comment.at = DateTime.Now.ToString("yyyyMMddHHmmss");
                _new_comment.comment = "Xác nhận trạng thái xử lý. " + notes;
                _new_comment.reason = reason;
                _comment.Add(_new_comment);
                _request.comments = _comment.ToArray();
                List<PayID.DataHelper.DynamicObj> _assigned = new List<PayID.DataHelper.DynamicObj>();
                _assigned.AddRange(_request.assigned_to);

                dynamic _new_assigned = new PayID.DataHelper.DynamicObj();
                _new_assigned.assign_by = User.Identity.Name;
                _new_assigned.assign_at = DateTime.Now.ToString("yyyyMMddHHmmss");
                _new_assigned.assign_to_id = User.Identity.Name;
                _new_assigned.assign_to_full_name = User.Identity.Name;
                _assigned.Add(_new_assigned);
                _request.assigned_to = _assigned.ToArray();
                _request.current_assigned = ((dynamic)Session["profile"]).unit_link.ToString() + "." + User.Identity.Name.ToString();
                _request.current_assigned_name = User.Identity.Name;
                _request.system_status = status;

                //Confirms
                List<PayID.DataHelper.DynamicObj> _confirms = new List<PayID.DataHelper.DynamicObj>();
                if (_request.confirms != null)
                    _confirms.AddRange(_request.confirms);

                dynamic _new_confirm = new PayID.DataHelper.DynamicObj();
                _new_confirm.by = User.Identity.Name;
                _new_confirm.at = DateTime.Now.ToString("yyyyMMddHHmmss");
                _new_confirm.comment = notes;
                _new_confirm.reason = reason;
                _new_confirm.date = confirm_date;
                _new_confirm.customer = customer;
                _confirms.Add(_new_confirm);
                _request.confirms = _confirms.ToArray();

                dynamic _log = new PayID.DataHelper.DynamicObj();
                dynamic lading = new DynamicObj();
                lading = Configuration.Data.Get("Lading", Query.EQ("Code", _request.tracking_code == null ? "" : _request.tracking_code.ToString()));
                if (lading != null)
                {
                    lading.Status = status;
                    Configuration.Data.Save("Lading", lading);
                }

                dynamic myObj = new DynamicObj();//
                if (_request.tracking_code != null)
                    myObj = Configuration.Data.Get("LogJourney", Query.And(Query.EQ("Code", _request.tracking_code.ToString()), Query.EQ("Status", status)));
                else
                    myObj = Configuration.Data.Get("LogJourney", Query.And(Query.EQ("Code", _id), Query.EQ("Status", status)));
                if (status == "C9")
                {
                    if (myObj == null)
                    {
                        _log = new PayID.DataHelper.DynamicObj();
                        _log._id = PayID.Portal.Areas.Lading.Configuration.Data.GetNextSquence("LogJourney");
                        _log.Code = _request.tracking_code;
                        _log.Status = status;
                        _log.UserId = _request.customer.code;
                        _log.Location = "Cast@Post";
                        if (!string.IsNullOrEmpty(reason))
                        {
                            _log.IsFail = 1;
                            _log.Note = reason;
                        }
                        else
                        {
                            _log.Note = notes;
                        }
                        _log.DateCreate = DateTime.Now;
                        PayID.Portal.Areas.Lading.Configuration.Data.Save("LogJourney", _log);
                    }
                    else
                    {

                        myObj.Status = "C9";
                        if (!string.IsNullOrEmpty(reason))
                        {
                            if (myObj.IsFail == null)
                            {
                                myObj.IsFail = 1;
                            }
                            else
                            {
                                myObj.IsFail = long.Parse(myObj.IsFail.ToString()) + 1;
                            }

                            if (myObj.IsFail == 2)
                            {
                                myObj.Note = "1." + myObj.Note.ToString() + "/" + myObj.IsFail.ToString() + "." + reason;
                            }
                            else if (myObj.IsFail == 3)
                            {
                                myObj.Note = myObj.Note.ToString() + "/" + myObj.IsFail.ToString() + "." + reason;
                            }
                            else
                            {
                                myObj.Note = reason;
                            }

                        }
                        else
                        {
                            myObj.Note = notes;
                        }
                        myObj.DateCreate = DateTime.Now;
                        PayID.Portal.Areas.Lading.Configuration.Data.Save("LogJourney", myObj);
                    }
                }
                else
                {
                    if (myObj == null)
                    {
                        _log = new PayID.DataHelper.DynamicObj();
                        _log._id = PayID.Portal.Areas.Lading.Configuration.Data.GetNextSquence("LogJourney");
                        _log.Code = _request.tracking_code;
                        _log.Status = status;
                        _log.UserId = _request.customer.code;
                        _log.Location = "Cast@Post";
                        if (!string.IsNullOrEmpty(reason))
                        {
                            _log.Note = reason;
                        }
                        else
                        {
                            _log.Note = notes;
                        }
                    }
                    _log.DateCreate = DateTime.Now;
                    PayID.Portal.Areas.Lading.Configuration.Data.Save("LogJourney", _log);
                }


                Configuration.Data.Save("shipment", _request);
            }
            return Json(new { response_code = "00", response_message = "Xác nhận thành công" }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region DetailPaymentCheck
        public Action DetailPaymentCheck(dynamic model)
        {
            return View(model);
        }
        #endregion
        #region CreatePaymentCheckRequest
        public JsonResult CreatePaymentCheckRequest(
            string transaction_code,
                string customer_code,
                string customer_name,
                string customer_mobile,
                string amount,
                string transaction_time,
                string payee_name,
                string payee_email,
                string notes
            )
        {
            dynamic _request = new PayID.DataHelper.DynamicObj();
            string prefix = DateTime.Today.ToString("yy") + DateTime.Today.DayOfYear.ToString().PadLeft(3, '0');
            string _id = "PC" + prefix + Configuration.Data.GetNextSquence("service_request_payment_check_" + prefix).ToString().PadLeft(5, '0');
            _request._id = _id;
            _request.customer_code = customer_code;
            _request.customer_name = customer_name;
            _request.customer_mobile = customer_mobile;
            _request.transaction_code = transaction_code;
            _request.notes = notes;
            _request.payee_name = payee_name;
            _request.payee_email = payee_email;
            _request.amount = amount;
            _request.transaction_time = transaction_time;
            _request.system_status = "ACTIVE";

            if (Configuration.Data.Save("payment_check_request", _request))
            {
                string unit_code = ((dynamic)Session["profile"]).unit_code;
                dynamic _service_request = new PayID.DataHelper.DynamicObj();
                _service_request._id = _id;
                _service_request.request_type = new PayID.DataHelper.DynamicObj();
                _service_request.request_type.type = "CHECK";
                _service_request.request_type.service = "PAYMENT";
                _service_request.request_type.title = "Tra soát giao dịch thanh toán";
                _service_request.customer = new PayID.DataHelper.DynamicObj();
                _service_request.customer.code = customer_code;
                _service_request.customer.full_name = customer_name;
                _service_request.customer.mobile = customer_mobile;
                _service_request.system_status = "C1";
                dynamic _comment = new PayID.DataHelper.DynamicObj();
                _comment.at = DateTime.Now.ToString("yyyyMMddHHmmss");
                _comment.by = User.Identity.Name;
                _comment.comment = "Tạo yêu cầu";

                _service_request.comments = new PayID.DataHelper.DynamicObj[]{
                    _comment
                };

                dynamic _assign = new PayID.DataHelper.DynamicObj();
                _assign.assigned_by = "CORE SYSTEM";
                _assign.assign_to_id = unit_code;
                _assign.assign_to_full_name = User.Identity.Name;
                _service_request.assigned_to = new PayID.DataHelper.DynamicObj[] { _assign };

                _service_request.current_assigned = unit_code;
                _service_request.created_by = "CORE SYSTEM";
                _service_request.created_at = DateTime.Now.ToString("yyyyMMddHHmmss");
                _service_request.current_assigned_name = User.Identity.Name;

                _service_request.request_content = new PayID.DataHelper.DynamicObj();
                _service_request.request_content.description = "Tra soát giao dịch " + transaction_code + ", lúc: " + transaction_time
                    + "<br/>" + "KH: " + customer_code + ", " + customer_name + ", " + customer_mobile
                    + "<br/>Người thanh toán: " + payee_name + ", " + payee_email;
                _service_request.request_content.notes = notes;
                if (Configuration.Data.Save("service_request", _service_request))
                {
                    return Json(new { response_code = "00", response_message = "Tạo yêu cầu thành công" }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { response_code = "01", response_message = "Tạo yêu cầu không thành công" }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region CreateShippingCancelRequest
        public JsonResult CreateShippingCancelRequest(string customer_code, string request)
        {
            dynamic profile = (dynamic)Session["profile"];
            dynamic _request = new DynamicObj(
              MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(request));
            string prefix = DateTime.Today.ToString("yy") + DateTime.Today.DayOfYear.ToString().PadLeft(3, '0');
            string _id = "SB" + prefix + Configuration.Data.GetNextSquence("service_request_shipping_cancel_" + prefix).ToString().PadLeft(5, '0');
            dynamic _pickupReq = Configuration.Data.Get("shipping_pickup_request", Query.EQ("_id", _request.lading_code));
            _request._id = _id;
            _request.receiver_name = _pickupReq.receiver_name;
            _request.receiver_mobile = _pickupReq.receiver_mobile;
            _request.receiver_address = _pickupReq.receiver_address;
            _request.receiver_province = _pickupReq.receiver_province;
            _request.receiver_province_name = _pickupReq.receiver_province_name;
            _request.receiver_district = _pickupReq.receiver_district;
            _request.receiver_district_name = _pickupReq.receiver_district_name;
            _request.receiver_ward = _pickupReq.receiver_ward;
            _request.receiver_ward_name = _pickupReq.receiver_ward_name;

            _request.address = _pickupReq.address;
            _request.province = _pickupReq.province;
            _request.province_name = _pickupReq.province_name;
            _request.district = _pickupReq.district;
            _request.district_name = _pickupReq.district_name;
            _request.ward = _pickupReq.ward;
            _request.ward_name = _pickupReq.ward_name;

            _request.system_status = "ACTIVE";

            if (Configuration.Data.Save("shipping_cancel_request", _request))
            {
                string unit_code = profile.unit_code;
                if (unit_code == "00") unit_code = _request.province;
                dynamic _service_request = new PayID.DataHelper.DynamicObj();
                _service_request._id = _id;
                _service_request.address = _request.address;
                _service_request.province = _request.province;
                _service_request.district = _request.district;
                _service_request.ward = _request.ward;
                _service_request.province_name = _request.province_name;
                _service_request.district_name = _request.district_name;
                _service_request.ward_name = _request.ward_name;

                _service_request.request_type = new PayID.DataHelper.DynamicObj();
                _service_request.request_type.type = "C2";
                _service_request.request_type.service = "SHIPPING";
                _service_request.request_type.title = "Hủy yêu cầu, vận đơn";
                _service_request.customer = new PayID.DataHelper.DynamicObj();
                _service_request.customer.code = _request.customer_code;
                _service_request.customer.full_name = _request.customer_name;
                _service_request.customer.mobile = _request.customer_mobile;
                _service_request.system_status = "C1";
                dynamic _comment = new PayID.DataHelper.DynamicObj();
                _comment.at = DateTime.Now.ToString("yyyyMMddHHmmss");
                _comment.by = User.Identity.Name;
                _comment.comment = "Tạo yêu cầu";

                _service_request.comments = new PayID.DataHelper.DynamicObj[]{
                    _comment
                };

                dynamic _assign = new PayID.DataHelper.DynamicObj();
                _assign.assign_by = User.Identity.Name;
                _assign.assign_at = DateTime.Now.ToString("yyyyMMddHHmmss");
                _assign.assign_to_id = unit_code;
                _assign.assign_to_full_name = _request.province_name;
                _service_request.assigned_to = new PayID.DataHelper.DynamicObj[] { _assign };

                _service_request.current_assigned = unit_code;
                _service_request.created_by = User.Identity.Name;
                _service_request.created_at = DateTime.Now.ToString("yyyyMMddHHmmss");
                _service_request.current_assigned_name = _request.province_name;

                _service_request.request_content = new PayID.DataHelper.DynamicObj();
                _service_request.request_content.description = "Người gửi: " + _request.customer_name + ", " + _request.customer_mobile +
                    ". Địa chỉ: " + _request.address + ", " + _request.district_name + ", " + _request.province_name
                    + ". Liên hệ: " + _request.contact_name + ", " + _request.contact_mobile + "<br/>"
                    + ((String.IsNullOrEmpty(_request.contact_date)) ? "" : "Ngày hẹn: " + _request.contact_date)
                    + ((String.IsNullOrEmpty(_request.contact_time)) ? "" : "- Giờ hẹn: " + _request.contact_time)
                    + "<br/>" + "Người nhận: " + _request.receiver_name + ", " + _request.receiver_mobile
                    + ". Địa chỉ: " + _request.receiver_address + ", " + _request.receiver_district_name + ", " + _request.receiver_province_name;
                _service_request.request_content.notes = _request.notes;
                if (Configuration.Data.Save("service_request", _service_request))
                {
                    return Json(new { response_code = "00", response_message = "Tạo yêu cầu thành công" }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { response_code = "01", response_message = "Tạo yêu cầu không thành công" }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        public JsonResult CreateSupportGeneralRequest(
           string customer_code,
           string customer_name,
           string customer_email,
           string customer_mobile,
           string customer_address,
           string notes
           )
        {
            dynamic _request = new PayID.DataHelper.DynamicObj();
            string prefix = DateTime.Today.ToString("yy") + DateTime.Today.DayOfYear.ToString().PadLeft(3, '0');
            string _id = "SG" + prefix + Configuration.Data.GetNextSquence("service_request_support_general_" + prefix).ToString().PadLeft(5, '0');
            _request._id = _id;
            _request.customer_code = customer_code;
            _request.customer_email = customer_email;
            _request.customer_name = customer_name;
            _request.customer_mobile = customer_mobile;
            _request.notes = notes;
            _request.customer_address = customer_address;
            _request.system_status = "ACTIVE";

            if (Configuration.Data.Save("support_general_request", _request))
            {
                string unit_code = ((dynamic)Session["profile"]).unit_code;
                dynamic _service_request = new PayID.DataHelper.DynamicObj();
                _service_request._id = _id;
                _service_request.request_type = new PayID.DataHelper.DynamicObj();
                _service_request.request_type.type = "GENERAL";
                _service_request.request_type.service = "SUPPORT";
                _service_request.request_type.title = "Hỗ trợ khách hàng";
                _service_request.customer = new PayID.DataHelper.DynamicObj();
                _service_request.customer.code = customer_code;
                _service_request.customer.full_name = customer_name;
                _service_request.customer.mobile = customer_mobile;
                _service_request.system_status = "C1";
                dynamic _comment = new PayID.DataHelper.DynamicObj();
                _comment.at = DateTime.Now.ToString("yyyyMMddHHmmss");
                _comment.by = User.Identity.Name;
                _comment.comment = "Tạo yêu cầu";

                _service_request.comments = new PayID.DataHelper.DynamicObj[]{
                    _comment
                };

                dynamic _assign = new PayID.DataHelper.DynamicObj();
                _assign.assigned_by = "CORE SYSTEM";
                _assign.assign_to_id = unit_code;
                _assign.assign_to_full_name = User.Identity.Name;
                _service_request.assigned_to = new PayID.DataHelper.DynamicObj[] { _assign };

                _service_request.current_assigned = unit_code;
                _service_request.created_by = "CORE SYSTEM";
                _service_request.created_at = DateTime.Now.ToString("yyyyMMddHHmmss");
                _service_request.current_assigned_name = User.Identity.Name;

                _service_request.request_content = new PayID.DataHelper.DynamicObj();
                _service_request.request_content.description = "KH: " + customer_code + ", " + customer_name + ", " + customer_mobile + ", " + customer_address;
                _service_request.request_content.notes = notes;
                if (Configuration.Data.Save("service_request", _service_request))
                {
                    return Json(new { response_code = "00", response_message = "Tạo yêu cầu thành công" }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { response_code = "01", response_message = "Tạo yêu cầu không thành công" }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPickupRequest(string lading_code)
        {
            dynamic _pickupReq = Configuration.Data.Get("shipping_pickup_request", Query.EQ("_id", lading_code));
            var returnObj = new
            {
                customer_code = _pickupReq.customer_code,
                customer_mobile = _pickupReq.customer_mobile,
                customer_name = _pickupReq.customer_name,
                receiver_name = _pickupReq.receiver_name,
                receiver_mobile = _pickupReq.receiver_mobile,
                receiver_address = _pickupReq.receiver_address,
                receiver_province = _pickupReq.receiver_province,
                receiver_province_name = _pickupReq.receiver_province_name,
                receiver_district = _pickupReq.receiver_district,
                receiver_district_name = _pickupReq.receiver_district_name,
                receiver_ward = _pickupReq.receiver_ward,
                receiver_ward_name = _pickupReq.receiver_ward_name,

                address = _pickupReq.address,
                province = _pickupReq.province,
                province_name = _pickupReq.province_name,
                district = _pickupReq.district,
                district_name = _pickupReq.district_name,
                ward = _pickupReq.ward,
                ward_name = _pickupReq.ward_name
            };
            return Json(returnObj, JsonRequestBehavior.AllowGet);
        }
        #region CreateShippingGetBackRequest
        public JsonResult CreateShippingGetBackRequest(string request)
        {
            dynamic profile = (dynamic)Session["profile"];
            dynamic _request = new DynamicObj(
              MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(request));
            string prefix = DateTime.Today.ToString("yy") + DateTime.Today.DayOfYear.ToString().PadLeft(3, '0');
            string _id = "SB" + prefix + Configuration.Data.GetNextSquence("service_request_shipping_getback_" + prefix).ToString().PadLeft(5, '0');
            dynamic _pickupReq = Configuration.Data.Get("shipping_pickup_request", Query.EQ("_id", _request.lading_code));
            _request._id = _id;
            _request.receiver_name = _pickupReq.receiver_name;
            _request.receiver_mobile = _pickupReq.receiver_mobile;
            _request.receiver_address = _pickupReq.receiver_address;
            _request.receiver_province = _pickupReq.receiver_province;
            _request.receiver_province_name = _pickupReq.receiver_province_name;
            _request.receiver_district = _pickupReq.receiver_district;
            _request.receiver_district_name = _pickupReq.receiver_district_name;
            _request.receiver_ward = _pickupReq.receiver_ward;
            _request.receiver_ward_name = _pickupReq.receiver_ward_name;

            _request.address = _pickupReq.address;
            _request.province = _pickupReq.province;
            _request.province_name = _pickupReq.province_name;
            _request.district = _pickupReq.district;
            _request.district_name = _pickupReq.district_name;
            _request.ward = _pickupReq.ward;
            _request.ward_name = _pickupReq.ward_name;

            _request.system_status = "ACTIVE";

            if (Configuration.Data.Save("shipping_getback_request", _request))
            {
                string unit_code = profile.unit_code;
                if (unit_code == "00") unit_code = _request.province;
                dynamic _service_request = new PayID.DataHelper.DynamicObj();
                _service_request._id = _id;
                _service_request.address = _request.address;
                _service_request.province = _request.province;
                _service_request.district = _request.district;
                _service_request.ward = _request.ward;
                _service_request.province_name = _request.province_name;
                _service_request.district_name = _request.district_name;
                _service_request.ward_name = _request.ward_name;

                _service_request.request_type = new PayID.DataHelper.DynamicObj();
                _service_request.request_type.type = "GETBACK";
                _service_request.request_type.service = "SHIPPING";
                _service_request.request_type.title = "Thu hồi, hoàn trả vận đơn";
                _service_request.customer = new PayID.DataHelper.DynamicObj();
                _service_request.customer.code = _request.customer_code;
                _service_request.customer.full_name = _request.customer_name;
                _service_request.customer.mobile = _request.customer_mobile;
                _service_request.system_status = "C1";
                dynamic _comment = new PayID.DataHelper.DynamicObj();
                _comment.at = DateTime.Now.ToString("yyyyMMddHHmmss");
                _comment.by = User.Identity.Name;
                _comment.comment = "Tạo yêu cầu";

                _service_request.comments = new PayID.DataHelper.DynamicObj[]{
                    _comment
                };

                dynamic _assign = new PayID.DataHelper.DynamicObj();
                _assign.assign_by = User.Identity.Name;
                _assign.assign_at = DateTime.Now.ToString("yyyyMMddHHmmss");
                _assign.assign_to_id = unit_code;
                _assign.assign_to_full_name = _request.province_name;
                _service_request.assigned_to = new PayID.DataHelper.DynamicObj[] { _assign };

                _service_request.current_assigned = unit_code;
                _service_request.created_by = User.Identity.Name;
                _service_request.created_at = DateTime.Now.ToString("yyyyMMddHHmmss");
                _service_request.current_assigned_name = _request.province_name;

                _service_request.request_content = new PayID.DataHelper.DynamicObj();
                _service_request.request_content.description = "Người gửi: " + _request.customer_name + ", " + _request.customer_mobile +
                    ". Địa chỉ: " + _request.address + ", " + _request.district_name + ", " + _request.province_name
                    + ". Liên hệ: " + _request.contact_name + ", " + _request.contact_mobile + "<br/>"
                    + ((String.IsNullOrEmpty(_request.contact_date)) ? "" : "Ngày hẹn: " + _request.contact_date)
                    + ((String.IsNullOrEmpty(_request.contact_time)) ? "" : "- Giờ hẹn: " + _request.contact_time)
                    + "<br/>" + "Người nhận: " + _request.receiver_name + ", " + _request.receiver_mobile
                    + ". Địa chỉ: " + _request.receiver_address + ", " + _request.receiver_district_name + ", " + _request.receiver_province_name;
                _service_request.request_content.notes = _request.notes;
                if (Configuration.Data.Save("service_request", _service_request))
                {
                    return Json(new { response_code = "00", response_message = "Tạo yêu cầu thành công" }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { response_code = "01", response_message = "Tạo yêu cầu không thành công" }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        // Hàm định danh khách hàng.
        public JsonResult ListProfile(string customer_code)
        {
            dynamic myObj = PayID.Portal.Configuration.Data.Get("business_profile", Query.EQ("_id", customer_code));
            return Json(new
            {
                customer_name = myObj.general_full_name,
                customer_mobile = myObj.contact_phone_mobile,
                address = myObj.contact_address_address,
                province = myObj.contact_address_province
            }, JsonRequestBehavior.AllowGet);
        }

        #region shipment
        public dynamic shipment(dynamic request)
        {
            string unit_code, unit_name, unit_link;
            dynamic from_address = request.from_address;
            request.system_status = "C5";

            if (!String.IsNullOrEmpty(from_address._id.ToString()))
            {
                dynamic store = PayID.Portal.Areas.Merchant.Configuration.Data.Get("profile_store",
                    Query.And(
                    Query.EQ("StoreCode", from_address._id.ToString()),
                    Query.EQ("UserId", request.customer.code.ToString())
                    )
                    );
                if (store != null)
                {
                    unit_code = store.ProvinceCode;
                    unit_link = store.ProvinceCode;
                    unit_name = PayID.Portal.Areas.Metadata.Controllers.ProxyController.GetProvinceName(unit_code).Trim();
                    if (store.PostCode != null && store.PostCode != "")
                    {
                        dynamic postCode = PayID.Portal.Areas.Metadata.Configuration.Data.Get("mbcPos", Query.EQ("_id", store.PostCode));
                        unit_code = store.PostCode;
                        unit_link = postCode.ProvinceCode + "." + postCode.UnitCode + "." + store.PostCode;
                        unit_name = postCode.POSName.Trim();
                        request.is_assigned = 1;
                        request.system_status = "C6";
                    }
                }
                else
                {
                    return JObject.Parse(@"{error_code:'10',error_message:'Invalid From Address ID'}");
                }
            }
            else
            {

                if (from_address.province == null)
                {
                    return JObject.Parse(@"{error_code:'11',error_message:'Invalid From Address Province Code'}");
                }

                dynamic province = PayID.Portal.Areas.Metadata.Controllers.ProxyController.GetProvince(from_address.province.ToString());
                if (province == null) return JObject.Parse(@"{error_code:'11',error_message:'Invalid From Address Province Code'}");
                if (from_address.postcode_link != null && !string.IsNullOrEmpty(from_address.postcode_link.ToString()))
                {
                    dynamic postCode = PayID.Portal.Areas.Metadata.Configuration.Data.Get("mbcPos", Query.EQ("_id", from_address.postcode_link.ToString()));
                    if (postCode != null)
                    {
                        unit_code = from_address.postcode_link;
                        unit_link = postCode.ProvinceCode + "." + postCode.UnitCode + "." + from_address.postcode_link;
                        unit_name = postCode.POSName.Trim();
                        request.is_assigned = 1;
                        request.system_status = "C6";
                    }
                    else
                    {
                        unit_code = province.ProvinceCode;
                        unit_link = unit_code;
                        unit_name = province.ProvinceName;
                    }
                }
                else
                {
                    unit_code = province.ProvinceCode;
                    unit_link = unit_code;
                    unit_name = province.ProvinceName;
                }
            }

            string prefix = DateTime.Today.ToString("yy") + DateTime.Today.DayOfYear.ToString().PadLeft(3, '0');
            string _id = "10" + prefix + Configuration.Data.GetNextSquence("shipment_request_" + prefix).ToString().PadLeft(5, '0');
            request._id = _id;
            request.order_id = _id;
            dynamic _comment = new JObject();
            _comment.at = DateTime.Now.ToString("yyyyMMddHHmmss");
            _comment.by = "Cash@POST API";
            _comment.comment = "Khởi tạo";

            request.comments = new JArray{
                    _comment
                };

            dynamic _assign = new JObject();
            _assign.assign_by = "Cash@POST API";
            _assign.assign_at = DateTime.Now.ToString("yyyyMMddHHmmss");
            _assign.assign_to_id = unit_code;
            _assign.assign_to_full_name = unit_name;
            request.assigned_to = new JArray { _assign };

            request.current_assigned = unit_link;
            request.refcode = _id;
            request.created_by = "Cash@Post";
            request.created_at = DateTime.Now.ToString("yyyyMMddHHmmss");
            request.current_assigned_name = unit_name;

            if (!String.IsNullOrEmpty(request.product.weight.ToString()))
                request.product.weight = long.Parse(request.product.weight.ToString().Replace(".", ""));
            if (!String.IsNullOrEmpty(request.product.value.ToString()))
                request.product.value = long.Parse(request.product.value.ToString().Replace(".", ""));
            if (!String.IsNullOrEmpty(request.product.quantity.ToString()))
                request.product.quantity = long.Parse(request.product.quantity.ToString().Replace(".", ""));

            Configuration.Data.SaveDynamic("shipment", request);
            return JObject.Parse(@"{"
                + "error_code:'00'"
                + ",error_message:'Thành công'"
                + ",tracking_code: '" + request.tracking_code + "'"
                + ",request_code: '" + request._id + "'"
                + ",request_id: '" + request.request_id + "'"
                + ",order_id: '" + request.order_id + "'"
            + "}");

        }
        #endregion
        #region save_shipment
        public dynamic save_shipment(dynamic request)
        {
            dynamic shipment = new PayID.DataHelper.DynamicObj(MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(request.ToString()));
            dynamic current = Configuration.Data.Get("shipment", Query.EQ("_id", shipment._id));

            string sUnitLink = ((dynamic)Session["profile"]).unit_link;

            dynamic ladingCurrent = new DynamicObj();
            try
            {
                ladingCurrent = Configuration.Data.Get("Lading", Query.EQ("Code", current.tracking_code));
            }
            catch
            {
                ladingCurrent = null;
            }
            Configuration.Data.Save("shipment_history", current);
            if (!string.IsNullOrEmpty(shipment.RefCode))
            {
                if (ladingCurrent != null)
                {
                    if (current.tracking_code != null && current.tracking_code.toString() != shipment.RefCode.ToString())
                    {
                        current.RefCode = current.tracking_code.ToString();
                        current.tracking_code = shipment.RefCode;
                    }
                    else
                    {
                        current.tracking_code = shipment.RefCode;
                    }
                    //current.system_status = "C25";
                    if (ladingCurrent.Code != null && ladingCurrent.Code.toString() != shipment.RefCode.ToString())
                    {
                        ladingCurrent.RefCode = ladingCurrent.Code.ToString();
                        ladingCurrent.Code = shipment.RefCode;
                    }
                    else
                    {
                        ladingCurrent.Code = shipment.RefCode;
                    }
                    //ladingCurrent.Status = "C25";
                    Configuration.Data.Save("Lading", ladingCurrent);
                    //dynamic _journey = Configuration.Data.Get("LogJourney", Query.And(Query.EQ("Code", current.tracking_code)
                    //                                , Query.EQ("Status", "C25")));
                    //if (_journey == null)
                    //{
                    //    dynamic objLogJourney = new DynamicObj();
                    //    objLogJourney._id = Configuration.Data.GetNextSquence("LogJourney");
                    //    objLogJourney.Code = current.tracking_code;
                    //    objLogJourney.Status = "C25";
                    //    objLogJourney.UserId = current.customer.code.ToString();
                    //    objLogJourney.Location = "Cast@Post";
                    //    objLogJourney.Note = "Mã vận đơn gốc: " + current.tracking_code;
                    //    objLogJourney.DateCreate = DateTime.Now;
                    //    Configuration.Data.Save("LogJourney", objLogJourney);
                    //}
                }
                else
                {
                    if (sUnitLink.Length >= 6)
                    {
                        current.tracking_code = shipment.RefCode;

                        using (var client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"].ToString());
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var obj_request = new
                            {
                                Value = Int64.Parse(shipment.product.value.ToString().Replace(".", "").Replace(",", "")),
                                Weight = Int64.Parse(shipment.product.weight.ToString().Replace(".", "").Replace(",", "")),
                                Quantity = Int64.Parse(shipment.product.quantity.ToString().Replace(".", "").Replace(",", "")),
                                SenderID = 0,
                                SenderName = shipment.from_address.name,
                                SenderAddress = shipment.from_address.address,
                                SenderMobile = shipment.from_address.phone,
                                ReceiverName = shipment.to_address.name,
                                ReceiverAddress = shipment.to_address.address,
                                ReceiverMobile = shipment.to_address.phone,
                                CustomerCode = current.customer.code,
                                FromProvinceCode = current.from_address.province,
                                FromDistrictCode = current.from_address.district,
                                ToProvinceCode = current.to_address.province,
                                ToDistrictCode = current.to_address.district,
                                ToWardCode = current.from_address.ward,
                                postcode_link = current.current_assigned.ToString().Substring(current.current_assigned.ToString().Length - 6, 6),
                                Status = current.system_status.ToString(),
                                ProductName = current.product.name,
                                ProductDescription = current.product.description,
                                Type = 0,
                                ServiceCode = "COD",
                                MainFee = 0,
                                ServiceFee = 0,
                                CodFee = 0,
                                TotalFee = 0,
                                Code = shipment.RefCode.ToString()
                            };
                            dynamic rs = new DynamicObj();
                            var response = client.PostAsJsonAsync("api/Lading?function=create_lading", obj_request).Result;
                            rs = response.Content.ReadAsAsync<dynamic>().Result;
                            if (rs.response_code == "00")
                            {
                                dynamic objLogJourney = new DynamicObj();
                                objLogJourney._id = Configuration.Data.GetNextSquence("LogJourney");
                                objLogJourney.Code = shipment.RefCode;
                                objLogJourney.Status = current.system_status.ToString();
                                objLogJourney.UserId = current.customer.code.ToString();
                                objLogJourney.Location = "Cast@Post";
                                objLogJourney.Note = "";
                                objLogJourney.DateCreate = DateTime.Now;
                                Configuration.Data.Save("LogJourney", objLogJourney);
                            }
                        }
                    }
                }

            }
            else
            {
                if (ladingCurrent == null)
                {
                    if (sUnitLink.Length >= 6)
                    {
                        using (var client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"].ToString());
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var obj_request = new
                            {
                                Value = Int64.Parse(shipment.product.value != null ? shipment.product.value.ToString().Replace(".", "").Replace(",", "") : "0"),
                                Weight = Int64.Parse(shipment.product.weight != null ? shipment.product.weight.ToString().Replace(".", "").Replace(",", "") : "0"),
                                Quantity = Int64.Parse(!string.IsNullOrEmpty(shipment.product.quantity) ? shipment.product.quantity.ToString().Replace(".", "").Replace(",", "") : "0"),
                                SenderID = 0,
                                SenderName = shipment.from_address.name,
                                SenderAddress = shipment.from_address.address,
                                SenderMobile = shipment.from_address.phone,
                                ReceiverName = shipment.to_address.name,
                                ReceiverAddress = shipment.to_address.address,
                                ReceiverMobile = shipment.to_address.phone,
                                CustomerCode = current.customer.code,
                                FromProvinceCode = current.from_address.province,
                                FromDistrictCode = current.from_address.district,
                                ToProvinceCode = current.to_address.province,
                                ToDistrictCode = current.to_address.district,
                                ToWardCode = current.from_address.ward,
                                postcode_link = current.current_assigned.ToString().Substring(current.current_assigned.ToString().Length - 6, 6),
                                Status = current.system_status.ToString(),
                                ProductName = current.product.name,
                                ProductDescription = current.product.description,
                                Type = 0,
                                ServiceCode = "COD",
                                MainFee = 0,
                                ServiceFee = 0,
                                CodFee = 0,
                                TotalFee = 0
                            };
                            dynamic rs = new DynamicObj();
                            var response = client.PostAsJsonAsync("api/Lading?function=create_lading", obj_request).Result;
                            rs = response.Content.ReadAsAsync<dynamic>().Result;
                            if (rs.response_code == "00")
                            {
                                current.tracking_code = rs.Code.ToString();
                                dynamic objLogJourney = new DynamicObj();
                                objLogJourney._id = Configuration.Data.GetNextSquence("LogJourney");
                                objLogJourney.Code = shipment.RefCode;
                                objLogJourney.Status = current.system_status.ToString();
                                objLogJourney.UserId = current.customer.code.ToString();
                                objLogJourney.Location = "Cast@Post";
                                objLogJourney.Note = "";
                                objLogJourney.DateCreate = DateTime.Now;
                                Configuration.Data.Save("LogJourney", objLogJourney);
                            }
                        }
                    }

                }
                else
                {
                    dynamic logChange = new DynamicObj();
                    long _value = long.Parse(shipment.product.value != null ? shipment.product.value.ToString().Replace(".", "").Replace(",", "") : "0");
                    long _weight = long.Parse(shipment.product.weight != null ? shipment.product.weight.ToString().Replace(".", "").Replace(",", "") : "0");
                    long _quantity = long.Parse(shipment.product.quantity != null && shipment.product.quantity != "" ? shipment.product.quantity.ToString().Replace(".", "").Replace(",", "") : "0");

                    dynamic _feeRslt = new DynamicObj();
                    try
                    {
                        var _res = GetFee(shipment.service.shipping_main_service, shipment.product.value != null ? shipment.product.value.ToString().Replace(".", "").Replace(",", "") : "0", shipment.from_address.province, shipment.to_address.province, shipment.product.weight != null ? shipment.product.weight.ToString().Replace(".", "").Replace(",", "") : "0");
                        _feeRslt.MainFee = string.IsNullOrEmpty(_res.Data.MainFee) ? "0" : _res.Data.MainFee;
                        _feeRslt.CodFee = string.IsNullOrEmpty(_res.Data.CodFee) ? "0" : _res.Data.CodFee;
                        _feeRslt.ServiceFee = string.IsNullOrEmpty(_res.Data.ServiceFee) ? "0" : _res.Data.ServiceFee;
                        _feeRslt.TotalFee = string.IsNullOrEmpty(_res.Data.TotalFee) ? "0" : _res.Data.TotalFee;

                        //ladingCurrent
                    }
                    catch
                    {
                        _feeRslt.MainFee = "0";
                        _feeRslt.CodFee = "0";
                        _feeRslt.ServiceFee = "0";
                        _feeRslt.TotalFee = "0";
                    }

                    var str = new
                    {
                        Value = long.Parse(string.Format("{0}", shipment.product.value.ToString().Replace(".", "").Replace(",", ""))),
                        Weight = long.Parse(string.Format("{0}", shipment.product.weight.ToString().Replace(".", "").Replace(",", ""))),
                        Quantity = long.Parse(string.Format("{0}", shipment.product.quantity != null && shipment.product.quantity != "" ? shipment.product.quantity.ToString().Replace(".", "").Replace(",", "") : "0")),
                        ProductName = shipment.product.name,
                        //ProductDescription = shipment.product.description,
                        ReceiverName = shipment.to_address.name,
                        ReceiverAddress = shipment.to_address.address,
                        ReceiverMobile = shipment.to_address.phone,
                        ToProvinceCode = shipment.to_address.province,
                        MainFee = _feeRslt.MainFee,
                        ServiceFee = _feeRslt.ServiceFee,
                        CodFee = _feeRslt.CodFee,
                        TotalFee = _feeRslt.TotalFee
                    };
                    var str_old = new
                    {
                        Value = long.Parse(string.Format("{0}", ladingCurrent.Value.ToString().Replace(".", "").Replace(",", ""))),
                        Weight = long.Parse(string.Format("{0}", ladingCurrent.Weight.ToString().Replace(".", "").Replace(",", ""))),
                        Quantity = long.Parse(string.Format("{0}", ladingCurrent.Quantity.ToString().Replace(".", "").Replace(",", ""))),
                        ProductName = ladingCurrent.ProductName,
                        //ProductDescription = ladingCurrent.ProductDescription,
                        ReceiverName = ladingCurrent.ReceiverName,
                        ReceiverAddress = ladingCurrent.ReceiverAddress,
                        ReceiverMobile = ladingCurrent.ReceiverMobile,
                        ToProvinceCode = ladingCurrent.ToProvinceCode,
                        MainFee = ladingCurrent.MainFee,
                        ServiceFee = ladingCurrent.ServiceFee,
                        CodFee = ladingCurrent.CodFee,
                        TotalFee = ladingCurrent.TotalFee
                    };
                    if (Security.CreatPassWordHash(str.ToString()) != Security.CreatPassWordHash(str_old.ToString()))
                    {
                        string sDate = DateTime.Now.ToString("yyyyMMdd");
                        logChange._id = ladingCurrent.Code + sDate + PayID.Portal.Areas.Lading.Configuration.Data.GetNextSquence("LogChangeLading");
                        logChange.Code = ladingCurrent.Code;
                        logChange.Value_old = long.Parse(string.Format("{0}", ladingCurrent.Value));
                        logChange.Weight_old = long.Parse(string.Format("{0}", ladingCurrent.Weight));
                        logChange.Quantity_old = long.Parse(string.Format("{0}", ladingCurrent.Quantity));
                        logChange.ProductName_old = ladingCurrent.ProductName;
                        //ProductDescription = ladingCurrent.ProductDescription,
                        logChange.ReceiverName_old = ladingCurrent.ReceiverName;
                        logChange.ReceiverAddress_old = ladingCurrent.ReceiverAddress;
                        logChange.ReceiverMobile_old = ladingCurrent.ReceiverMobile;
                        logChange.ToProvinceCode_old = ladingCurrent.ToProvinceCode;
                        logChange.MainFee_old = ladingCurrent.MainFee;
                        logChange.ServiceFee_old = ladingCurrent.ServiceFee;
                        logChange.CodFee_old = ladingCurrent.CodFee;
                        logChange.TotalFee_old = ladingCurrent.TotalFee;

                        logChange.Value = long.Parse(string.Format("{0}", shipment.product.value.ToString().Replace(".", "").Replace(",", "")));
                        logChange.Weight = long.Parse(string.Format("{0}", shipment.product.weight.ToString().Replace(".", "").Replace(",", "")));
                        logChange.Quantity = long.Parse(string.Format("{0}", shipment.product.quantity != null && shipment.product.quantity != "" ? shipment.product.quantity.ToString().Replace(".", "").Replace(",", "") : "0"));
                        logChange.ProductName = shipment.product.name;
                        //ProductDescription = ladingCurrent.ProductDescription,
                        logChange.ReceiverName = shipment.to_address.name;
                        logChange.ReceiverAddress = shipment.to_address.address;
                        logChange.ReceiverMobile = shipment.to_address.phone;
                        logChange.ToProvinceCode = shipment.to_address.province;
                        logChange.MainFee = _feeRslt.MainFee;
                        logChange.ServiceFee = _feeRslt.ServiceFee;
                        logChange.CodFee = _feeRslt.CodFee;
                        logChange.TotalFee = _feeRslt.TotalFee;

                        //Save LogChangeLading
                        Configuration.Data.Save("LogChangeLading", logChange);
                    }

                    ladingCurrent.Value = _value;// Int64.Parse(shipment.product.value != null ? shipment.product.value.ToString().Replace(".", "").Replace(",", "") : "0");
                    ladingCurrent.Weight = _weight;// Int64.Parse(shipment.product.weight != null ? shipment.product.weight.ToString().Replace(".", "").Replace(",", "") : "0");
                    ladingCurrent.Quantity = _quantity;// Int64.Parse(!string.IsNullOrEmpty(shipment.product.quantity) ? shipment.product.quantity.ToString().Replace(".", "").Replace(",", "") : "0");

                    ladingCurrent.SenderName = shipment.from_address.name;
                    ladingCurrent.SenderAddress = shipment.from_address.address;
                    ladingCurrent.SenderMobile = shipment.from_address.phone;
                    ladingCurrent.ReceiverName = shipment.to_address.name;
                    ladingCurrent.ReceiverAddress = shipment.to_address.address;
                    ladingCurrent.ReceiverMobile = shipment.to_address.phone;
                    ladingCurrent.CustomerCode = current.customer.code;
                    ladingCurrent.FromProvinceCode = shipment.from_address.province;
                    ladingCurrent.FromDistrictCode = shipment.from_address.district;
                    ladingCurrent.ToProvinceCode = shipment.to_address.province;
                    ladingCurrent.ToDistrictCode = shipment.to_address.district;
                    ladingCurrent.ToWardCode = shipment.from_address.ward;
                    if (current.postcode_link == null)
                    {
                        if (current.current_assigned.ToString().Length >= 14)
                        {
                            ladingCurrent.postcode_link = current.current_assigned.ToString().Substring(current.current_assigned.ToString().Length - 6, 6);
                        }
                        else
                        {
                            ladingCurrent.postcode_link = "";
                        }
                    }
                    ladingCurrent.Status = current.system_status.ToString();
                    ladingCurrent.ProductName = shipment.product.name;
                    ladingCurrent.ProductDescription = shipment.product.description;
                    ladingCurrent.Type = shipment.service.shipping_main_service;
                    ladingCurrent.MainFee = long.Parse(string.Format("{0}", _feeRslt.MainFee.ToString().Replace(".", "").Replace(",", "")));
                    ladingCurrent.ServiceFee = long.Parse(string.Format("{0}", _feeRslt.ServiceFee.ToString().Replace(".", "").Replace(",", "")));
                    ladingCurrent.CodFee = long.Parse(string.Format("{0}", _feeRslt.CodFee.ToString().Replace(".", "").Replace(",", "")));
                    ladingCurrent.TotalFee = long.Parse(string.Format("{0}", _feeRslt.TotalFee.ToString().Replace(".", "").Replace(",", "")));
                    //Save Lading
                    Configuration.Data.Save("Lading", ladingCurrent);
                }
            }
            current.from_address.name = shipment.from_address.name;
            current.from_address.address = shipment.from_address.address;
            current.from_address.ward = shipment.from_address.ward;
            current.from_address.district = shipment.from_address.district;
            current.from_address.province = shipment.from_address.province;
            current.from_address.phone = shipment.from_address.phone;
            current.from_address.postcode_link = shipment.from_address.postcode_link;

            current.to_address.name = shipment.to_address.name;
            current.to_address.address = shipment.to_address.address;
            current.to_address.ward = shipment.to_address.ward;
            current.to_address.district = shipment.to_address.district;
            current.to_address.province = shipment.to_address.province;
            current.to_address.phone = shipment.to_address.phone;

            if (!String.IsNullOrEmpty(shipment.product.weight.ToString()))
            {
                if (current.parcel != null && current.parcel.weight != null)
                {
                    current.parcel.weight = Int64.Parse(shipment.product.weight.ToString().Replace(".", "").Replace(",", ""));
                }
                else
                {
                    current.product.weight = Int64.Parse(shipment.product.weight.ToString().Replace(".", "").Replace(",", ""));
                }
            }
            if (!String.IsNullOrEmpty(shipment.product.value.ToString()))
                current.product.value = Int64.Parse(shipment.product.value.ToString().Replace(".", "").Replace(",", ""));
            if (!String.IsNullOrEmpty(shipment.product.quantity.ToString()))
                current.product.quantity = Int64.Parse(shipment.product.quantity.ToString().Replace(".", "").Replace(",", ""));

            current.product.name = shipment.product.name;
            current.product.code = shipment.product.code;
            //current.product.weight = shipment.product.weight;
            //current.product.quantity = shipment.product.quantity;
            current.product.type = shipment.product.type;
            current.product.description = shipment.product.description;
            //current.product.value = shipment.product.value;
            current.product.date = shipment.product.date;
            current.product.time = shipment.product.time;
            if (current.service == null)
            {
                current.service = shipment.service;
            }
            else
            {
                current.service.cashpost_service = shipment.service.cashpost_service;
                current.service.shipping_main_service = shipment.service.shipping_main_service;
                current.service.shipping_add_service = shipment.service.shipping_add_service;
            }


            Configuration.Data.Save("shipment", current);
            return JObject.Parse(@"{"
                + "error_code:'00'"
                + ",error_message:'Thành công'"
                + ",tracking_code: '" + shipment.tracking_code + "'"
                + ",request_code: '" + shipment._id + "'"
                + ",request_id: '" + shipment.request_id + "'"
                + ",order_id: '" + shipment.order_id + "'"
            + "}");
        }
        #endregion
        public JsonResult GetFee(string service_code, string value, string from_province_code, string to_province_code, string weight)
        {
            // Ki?m tra di?u ki?n tính cu?c from - to - weight.
            if (!String.IsNullOrEmpty(from_province_code) && !String.IsNullOrEmpty(to_province_code) && !String.IsNullOrEmpty(weight) && !String.IsNullOrEmpty(value))
            {
                dynamic dynamic_v = new DynamicObj();
                string v_value = value.Replace(".", "");
                string v_weight = weight.Replace(".", "");

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"].ToString());
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

                    var response = client.PostAsJsonAsync("api/Charge", obj_request).Result;
                    dynamic_v = response.Content.ReadAsAsync<JObject>().Result;
                }

                return Json(new
                {
                    MainFee = string.Format("{0:###,###,###}", (float)dynamic_v.MainFee),
                    CodFee = string.Format("{0:###,###,###}", (float)dynamic_v.CodFee),
                    ServiceFee = string.Format("{0:###,###,###}", (float)dynamic_v.ServiceFee),
                    TotalFee = string.Format("{0:###,###,###}", long.Parse(v_value) + (float)dynamic_v.MainFee)
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

        public JsonResult CreateShippingPickupRequest(string request)
        {
            dynamic _request = JObject.Parse(request);
            dynamic response = shipment(_request);
            #region "Comment"
            //dynamic profile = (dynamic)Session["profile"];
            //dynamic _request =new DynamicObj(MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(request));
            //string prefix = DateTime.Today.ToString("yy") + DateTime.Today.DayOfYear.ToString().PadLeft(3, '0');
            //string _id = "10" + prefix + Configuration.Data.GetNextSquence("service_request_shipping_pickup_" + prefix).ToString().PadLeft(5, '0');

            //_request.lading_code = "";
            //_request._id = _id;
            //_request.system_status = "ACTIVE";
            //_request.create_by = profile._id;
            //_request.create_by_full_name = profile.full_name;
            //if (Configuration.Data.Save("shipping_pickup_request", _request))
            //    //if (Configuration.Data.Save("shipping_pickup_request", _request))
            //{
            //    string unit_code = profile.unit_code;
            //    if (unit_code == "00") unit_code = _request.province;
            //    dynamic _service_request = new PayID.DataHelper.DynamicObj();
            //    _service_request._id = _id;
            //    _service_request.address = _request.address;
            //    _service_request.province = _request.province;
            //    _service_request.district = _request.district;
            //    _service_request.ward = _request.ward;
            //    _service_request.province_name = _request.province_name;
            //    _service_request.district_name = _request.district_name;
            //    _service_request.ward_name = _request.ward_name;

            //    _service_request.request_type =new PayID.DataHelper.DynamicObj();
            //    _service_request.request_type.type = "PICKUP";
            //    _service_request.request_type.service = "SHIPPING";
            //    _service_request.request_type.title = "Thu gom hàng hóa";
            //    _service_request.customer = new PayID.DataHelper.DynamicObj();
            //    _service_request.customer.code = _request.customer_code;
            //    _service_request.customer.full_name = _request.customer_name;
            //    _service_request.customer.mobile = _request.customer_mobile;
            //    _service_request.system_status = "C1";
            //    dynamic _comment =  new PayID.DataHelper.DynamicObj();
            //    _comment.at = DateTime.Now.ToString("yyyyMMddHHmmss");
            //    _comment.by = User.Identity.Name;
            //    _comment.comment = "Tạo yêu cầu";

            //    _service_request.comments = new PayID.DataHelper.DynamicObj[]{
            //        _comment
            //    };

            //    dynamic _assign = new  PayID.DataHelper.DynamicObj();
            //    _assign.assign_by = User.Identity.Name;
            //    _assign.assign_at = DateTime.Now.ToString("yyyyMMddHHmmss");
            //    _assign.assign_to_id = unit_code;
            //    _assign.assign_to_full_name = _request.province_name;
            //    _service_request.assigned_to = new PayID.DataHelper.DynamicObj[] { _assign };

            //    _service_request.current_assigned = unit_code;
            //    _service_request.created_by = User.Identity.Name;
            //    _service_request.created_at = DateTime.Now.ToString("yyyyMMddHHmmss");
            //    _service_request.current_assigned_name = _request.province_name;

            //    _service_request.request_content = new  PayID.DataHelper.DynamicObj();
            //    _service_request.request_content.description = "Người gửi: " + _request.customer_name + ", " + _request.customer_mobile +
            //        ". Địa chỉ: " + _request.address + ", " + _request.district_name + ", " + _request.province_name
            //        + ". Liên hệ: " + _request.contact_name + ", " + _request.contact_mobile + "<br/>"
            //        + ((String.IsNullOrEmpty(_request.contact_date)) ? "" : "Ngày hẹn: " + _request.contact_date)
            //        + ((String.IsNullOrEmpty(_request.contact_time)) ? "" : "- Giờ hẹn: " + _request.contact_time)
            //        + "<br/>" + "Người nhận: " + _request.receiver_name + ", " + _request.receiver_mobile
            //        + ". Địa chỉ: " + _request.receiver_address + ", " + _request.receiver_district_name + ", " + _request.receiver_province_name;
            //    _service_request.request_content.notes = _request.notes;
            //    if (Configuration.Data.Save("service_request", _service_request))
            //    {
            //        return Json(new { response_code = "00", response_message = "Tạo yêu cầu thành công" }, JsonRequestBehavior.AllowGet);
            //    }
            //}

            #endregion "Comment"
            return Json(new { response_code = response.error_code, response_message = response.error_message }, JsonRequestBehavior.AllowGet);
        }

        //public JsonResult CreateShippingPickupRequest(string request)
        //{
        //    dynamic _request = JObject.Parse(request);
        //    dynamic response = shipment(_request);
        //    return Json(new { response_code = response.error_code, response_message = response.error_message }, JsonRequestBehavior.AllowGet);
        //}

        #region SaveShippingPickupRequest
        public JsonResult SaveShippingPickupRequest(string request)
        {
            dynamic _request = JObject.Parse(request);
            dynamic response = save_shipment(_request);
            return Json(new { response_code = response.error_code, response_message = response.error_message }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region RequestExpInfo
        public ActionResult RequestExpInfo(string from_date, string to_date, string customer_code, string tracking_code, string request_id, string status)
        {
            from_date = from_date.Substring(6, 4) + "-" + from_date.Substring(3, 2) + "-" + from_date.Substring(0, 2);
            to_date = to_date.Substring(6, 4) + "-" + to_date.Substring(3, 2) + "-" + to_date.Substring(0, 2);

            LocalReport lr = new LocalReport();
            string path = Path.Combine(Server.MapPath("~/Areas/ServiceRequest/Templates"), "RequestExpTmp.rdlc");
            if (System.IO.File.Exists(path))
            {
                lr.ReportPath = path;
            }
            else
            {
                return View("Index");
            }
            List<Models.Request> lstLading = new List<Models.Request>();

            #region declare varialbles
            List<dynamic> list = new List<dynamic>();
            #endregion

            string unit_link = ((dynamic)Session["profile"]).unit_link;
            if (unit_link == "00") unit_link = "";
            MongoDB.Bson.BsonRegularExpression reg = new MongoDB.Bson.BsonRegularExpression("^" + unit_link);
            IMongoQuery _query = Query.Matches("current_assigned", reg);
            long _fromdt = long.Parse(DateTime.Parse(from_date).ToString("yyyyMMdd"));
            long _todt = long.Parse(DateTime.Parse(to_date).ToString("yyyyMMdd"));
            if (_fromdt == _todt)
            {
                _query = Query.And(
                    _query,
                            Query.GTE("system_time_key.date", _fromdt)
                    );
            }
            else
            {
                _query = Query.And(
                    _query,
                            Query.GTE("system_time_key.date", _fromdt)
                            , Query.LTE("system_time_key.date", _todt)
                    );
            }
            if (!String.IsNullOrEmpty(customer_code))
            {
                _query = Query.And(
                    _query,
                    Query.EQ("customer.code", customer_code));
            }

            if (!String.IsNullOrEmpty(tracking_code))
            {
                _query = Query.And(
                    _query,
                    Query.EQ("tracking_code", tracking_code));
            }

            if (!String.IsNullOrEmpty(status))
            {
                if (status == "C5")
                {
                    _query = Query.And(
                    _query,
                    Query.Or(Query.EQ("system_status", "C5"), Query.EQ("system_status", "C25")));
                }
                else
                {
                    _query = Query.And(
                    _query,
                    Query.EQ("system_status", status));
                }

            }

            if (!String.IsNullOrEmpty(request_id))
            {
                if (!request_id.Contains("#"))
                {
                    _query = Query.And(
                        _query,
                        Query.EQ("_id", request_id));
                }
                else
                {
                    string[] arrReqId = request_id.Split('#');
                    var documentIds = new BsonValue[arrReqId.Length];

                    for (int j = 0; j < arrReqId.Length; j++)
                    {
                        documentIds[j] = arrReqId[j];
                    }
                    _query = Query.And(
                        _query,
                        Query.In("_id", documentIds));
                }
            }

            List<dynamic> _list = Configuration.Data.List("shipment", _query).ToList<dynamic>();
            _list = (from c in _list orderby c._id descending select c).ToList<dynamic>();

            string _expTitle = "Danh sách yêu cầu chưa điều";
            foreach (dynamic l in _list)
            {
                Models.Request iLading = new Models.Request();

                string _fromname = string.Empty;

                if (!string.IsNullOrEmpty(l.from_address.name))
                {
                    _fromname = l.from_address.name;
                }
                try
                {
                    if (l.product.value != null && l.product.value > 0)
                    {
                        iLading.COD = "TRUE";
                    }
                    else
                    {
                        iLading.COD = "FALSE";
                    }
                }
                catch
                {
                    iLading.COD = "FALSE";
                }
                string contract = "";
                string MainKey = "";
                try
                {

                    try
                    {
                        if (!string.IsNullOrEmpty(l.customer.code))
                        {
                            dynamic custObj = Configuration.Data.Get("business_profile", Query.EQ("_id", l.customer.code));
                            contract = custObj.contract;
                            dynamic apiKey = Configuration.Data.Get("api_key", Query.EQ("business_profile", l.customer.code));
                            if (apiKey != null)
                            {
                                MainKey = apiKey.KeyCode;
                            }
                        }
                    }
                    catch
                    {
                        contract = "";
                    }
                    if (!string.IsNullOrEmpty(contract))
                    {
                        iLading.MoneyTypePay = "FALSE";
                        iLading.PayAtPos = "FALSE";
                    }
                    else
                    {
                        iLading.MoneyTypePay = "TRUE";
                        iLading.PayAtPos = "TRUE";
                    }
                }
                catch
                {
                    iLading.MoneyTypePay = "TRUE";
                    iLading.PayAtPos = "TRUE";
                }
                iLading.CollectPayer = "TRUE";
                iLading.DeliveriedPayer = "TRUE";
                string _code = l.tracking_code;
                string _refCode = l.RefCode;
                if (!string.IsNullOrEmpty(_code))
                {
                    if (!string.IsNullOrEmpty(_refCode))
                    {
                        iLading.Code = _refCode;
                    }
                    else
                    {
                        iLading.Code = _code;
                    }
                }
                else
                {
                    iLading.Code = "CZxxxxxxxxxVN";
                }
                if (!string.IsNullOrEmpty(MainKey))
                {
                    iLading.CustomerCode = MainKey;
                }
                else
                {
                    iLading.CustomerCode = l.customer.code.ToString();
                }
                iLading.GroupCustomerCode = "";
                iLading.SenderName = l.from_address.name;
                iLading.SenderAddr = l.from_address.address;
                iLading.SenderTel = l.from_address.phone;
                iLading.SenderEmail = l.from_address.email;
                string _senderPo = string.Empty;

                if (l.from_address.province.ToString().Length == 1)
                {
                    _senderPo = "0" + l.from_address.province.ToString();
                }
                else
                {
                    _senderPo = l.from_address.province.ToString();
                }

                if (!string.IsNullOrEmpty(_code) && _code.StartsWith("E"))
                {
                    iLading.TypePackage = "E";
                }
                else
                {
                    iLading.TypePackage = "AR";
                }

                if (!string.IsNullOrEmpty(_refCode))
                {
                    iLading.TypePackage = "R";
                }

                iLading.SenderPostCode = _senderPo;
                iLading.NotDeliveriedInstuct = "2";
                iLading.SenderTaxCode = "";
                iLading.SenderIdentification = "";
                iLading.ReceiverName = l.to_address.name;
                iLading.ReceiverAddr = l.to_address.address;
                iLading.ReceiverCountry = "";

                string _toProvince = l.to_address.province.ToString();
                try
                {
                    if (!string.IsNullOrEmpty(_toProvince) && _toProvince.Length < 2)
                    {
                        iLading.ReceiverProvince = "0" + _toProvince;
                    }
                    else if (!string.IsNullOrEmpty(_toProvince) && _toProvince.Length >= 2)
                    {
                        iLading.ReceiverProvince = _toProvince;
                    }
                    else
                    {
                        iLading.ReceiverProvince = "00";
                    }
                }
                catch
                {
                    iLading.ReceiverProvince = "00";
                }
                if (l.to_address.district != null)
                {
                    string _toDistrict = l.to_address.district.ToString();
                    try
                    {
                        if (!string.IsNullOrEmpty(_toDistrict) && _toDistrict.Length <= 1)
                        {
                            iLading.ReceiverDistrict = "0000";
                        }
                        else if (!string.IsNullOrEmpty(_toDistrict) && _toDistrict.Length == 2)
                        {
                            iLading.ReceiverDistrict = "00" + _toDistrict;
                        }
                        else if (!string.IsNullOrEmpty(_toDistrict) && _toDistrict.Length <= 3)
                        {
                            iLading.ReceiverDistrict = "0" + _toDistrict;
                        }
                        else if (!string.IsNullOrEmpty(_toDistrict) && _toDistrict.Length >= 4)
                        {
                            iLading.ReceiverDistrict = _toDistrict;
                        }
                        else
                        {
                            iLading.ReceiverDistrict = "0000";
                        }
                    }
                    catch
                    {
                        iLading.ReceiverDistrict = "0000";
                    }
                }


                string _toWard = l.to_address.ward.ToString();
                try
                {
                    if (!string.IsNullOrEmpty(_toWard) && _toWard.Length <= 1)
                    {
                        iLading.ReceiverCounty = "0000";
                    }
                    else if (!string.IsNullOrEmpty(_toWard) && _toWard.Length == 2)
                    {
                        iLading.ReceiverCounty = "00" + _toWard;
                    }
                    else if (!string.IsNullOrEmpty(_toWard) && _toWard.Length == 3)
                    {
                        iLading.ReceiverCounty = "0" + _toWard;
                    }
                    else if (!string.IsNullOrEmpty(_toWard) && _toWard.Length >= 4)
                    {
                        iLading.ReceiverCounty = _toWard;
                    }
                    else
                    {
                        iLading.ReceiverCounty = "0000";
                    }
                }
                catch
                {
                    iLading.ReceiverCounty = "0000";
                }
                if (l.to_address.phone != null)
                {
                    iLading.ReceiverMobile = l.to_address.phone;
                    if (iLading.ReceiverMobile.Length > 11)
                    {
                        iLading.ReceiverMobile = iLading.ReceiverMobile.Replace("<br/>", "-");
                        if (iLading.ReceiverMobile.Contains('-'))
                        {
                            string[] arrMobile = iLading.ReceiverMobile.Split('-');
                            iLading.ReceiverMobile = arrMobile[0];
                        }
                    }
                    iLading.ReceiverMobile = iLading.ReceiverMobile.Replace("-", "");
                }
                else
                {
                    iLading.ReceiverMobile = "";
                }
                iLading.DocumentCode = l.customer.code.ToString();
                try
                {
                    iLading.ReceiverEmail = l.to_address.email;
                }
                catch
                {
                    iLading.ReceiverEmail = "";
                }
                iLading.ContactName = "";
                try
                {
                    iLading.ReceiverPostCode = l.to_address.province.ToString();
                }
                catch
                {
                    iLading.ReceiverPostCode = "00";
                }
                try
                {
                    if (!string.IsNullOrEmpty(l.product.description.Trim()))
                    {
                        iLading.Content = l.product.description;
                    }
                    else
                    {
                        iLading.Content = "Chưa có";
                    }
                }
                catch
                {
                    iLading.Content = "Không có";
                }
                try
                {
                    if (l.parcel != null && l.parcel.weight != null)
                    {
                        iLading.Weight = decimal.Parse(l.parcel.weight.ToString());
                    }
                    else
                    {
                        if (l.product.weight != null)
                        {
                            iLading.Weight = decimal.Parse(l.product.weight.ToString());
                        }
                    }
                }
                catch
                {
                    iLading.Weight = 0;
                }

                try
                {
                    iLading.AmountCOD = decimal.Parse(l.product.value.ToString());
                }
                catch
                {
                    iLading.AmountCOD = 0;
                }
                try
                {
                    iLading.TaskNumber = l.order_id;
                }
                catch
                {
                    iLading.TaskNumber = "";
                }
                lstLading.Add(iLading);
                //}

            }

            ReportDataSource rd = new ReportDataSource("RequestInfo", lstLading);
            lr.DataSources.Add(rd);
            string reportType = "Excel";// id;
            string mimeType;
            string encoding;
            string fileNameExtension;

            if (!string.IsNullOrEmpty(status))
            {
                switch (status)
                {
                    case "C6":
                        _expTitle = "Danh sách yêu cầu đang xử lý";
                        break;
                    case "C10":
                        _expTitle = "Danh sách khách hàng hẹn lại";
                        break;
                    default:
                        _expTitle = "Danh sách yêu cầu chưa điều";
                        break;
                }
            }

            string deviceInfo =

            "<DeviceInfo>" +
            "  <OutputFormat>" + reportType + "</OutputFormat>" +
                //"  <PageWidth>9in</PageWidth>" +
                //"  <PageHeight>10in</PageHeight>" +
                //"  <MarginTop>0.5in</MarginTop>" +
                //"  <MarginLeft>1in</MarginLeft>" +
                //"  <MarginRight>0in</MarginRight>" +
                //"  <MarginBottom>0in</MarginBottom>" +
            "</DeviceInfo>";

            Warning[] warnings;
            string[] streams;
            byte[] renderedBytes;

            renderedBytes = lr.Render(
                reportType,
                deviceInfo,
                out mimeType,
                out encoding,
                out fileNameExtension,
                out streams,
                out warnings);


            return File(renderedBytes, mimeType, _expTitle + ".xls");
        }
        #endregion
        #region ExpInfoRequireNetPost
        public ActionResult ExpInfoRequireNetPost(string from_date, string to_date, string customer_code, string tracking_code, string request_id, string status)
        {
            from_date = from_date.Substring(6, 4) + "-" + from_date.Substring(3, 2) + "-" + from_date.Substring(0, 2);
            to_date = to_date.Substring(6, 4) + "-" + to_date.Substring(3, 2) + "-" + to_date.Substring(0, 2);

            LocalReport lr = new LocalReport();
            string path = Path.Combine(Server.MapPath("~/Areas/ServiceRequest/Templates"), "KHL.rdlc");
            if (System.IO.File.Exists(path))
            {
                lr.ReportPath = path;
            }
            else
            {
                return View("Index");
            }
            List<Models.RequestNE> lstLading = new List<Models.RequestNE>();

            #region declare varialbles
            List<dynamic> list = new List<dynamic>();
            #endregion

            string unit_link = ((dynamic)Session["profile"]).unit_link;
            if (unit_link == "00") unit_link = "";
            MongoDB.Bson.BsonRegularExpression reg = new MongoDB.Bson.BsonRegularExpression("^" + unit_link);
            IMongoQuery _query = Query.Matches("current_assigned", reg);
            long _fromdt = long.Parse(DateTime.Parse(from_date).ToString("yyyyMMdd"));
            long _todt = long.Parse(DateTime.Parse(to_date).ToString("yyyyMMdd"));
            if (_fromdt == _todt)
            {
                _query = Query.And(
                    _query,
                            Query.GTE("system_time_key.date", _fromdt)
                    );
            }
            else
            {
                _query = Query.And(
                    _query,
                            Query.GTE("system_time_key.date", _fromdt)
                            , Query.LTE("system_time_key.date", _todt)
                    );
            }
            if (!String.IsNullOrEmpty(customer_code))
            {
                _query = Query.And(
                    _query,
                    Query.EQ("customer.code", customer_code));
            }

            if (!String.IsNullOrEmpty(tracking_code))
            {
                _query = Query.And(
                    _query,
                    Query.EQ("tracking_code", tracking_code));
            }

            if (!String.IsNullOrEmpty(status))
            {
                if (status == "C5")
                {
                    _query = Query.And(
                    _query,
                    Query.Or(Query.EQ("system_status", "C5"), Query.EQ("system_status", "C25")));
                }
                else
                {
                    _query = Query.And(
                    _query,
                    Query.EQ("system_status", status));
                }

            }

            if (!String.IsNullOrEmpty(request_id))
            {
                if (!request_id.Contains("#"))
                {
                    _query = Query.And(
                        _query,
                        Query.EQ("_id", request_id));
                }
                else
                {
                    string[] arrReqId = request_id.Split('#');
                    var documentIds = new BsonValue[arrReqId.Length];

                    for (int j = 0; j < arrReqId.Length; j++)
                    {
                        documentIds[j] = arrReqId[j];
                    }
                    _query = Query.And(
                        _query,
                        Query.In("_id", documentIds));
                }
            }

            List<dynamic> _list = Configuration.Data.List("shipment", _query).ToList<dynamic>();
            _list = (from c in _list orderby c._id descending select c).ToList<dynamic>();

            string _expTitle = "Danh sách yêu cầu chưa điều";
            foreach (dynamic l in _list)
            {
                Models.RequestNE iLading = new Models.RequestNE();

                string _fromname = string.Empty;

                if (!string.IsNullOrEmpty(l.from_address.name))
                {
                    _fromname = l.from_address.name;
                }
                try
                {
                    if (l.product.value != null && long.Parse(l.product.value.ToString()) > 0)
                    {
                        iLading.COD = "1";
                    }
                    else
                    {
                        iLading.COD = "0";
                    }
                }
                catch
                {
                    iLading.COD = "";
                }
                string contract = "";
                string MainKey = "";
                try
                {

                    try
                    {
                        if (!string.IsNullOrEmpty(l.customer.code))
                        {
                            dynamic custObj = Configuration.Data.Get("business_profile", Query.EQ("_id", l.customer.code));
                            contract = custObj.contract;
                            dynamic apiKey = Configuration.Data.Get("api_key", Query.EQ("business_profile", l.customer.code));
                            if (apiKey != null)
                            {
                                MainKey = apiKey.KeyCode;
                            }
                        }
                    }
                    catch
                    {
                        contract = "";
                    }
                    if (!string.IsNullOrEmpty(contract))
                    {
                        iLading.MoneyTypePay = "FALSE";
                        iLading.PayAtPos = "FALSE";
                    }
                    else
                    {
                        iLading.MoneyTypePay = "TRUE";
                        iLading.PayAtPos = "TRUE";
                    }
                }
                catch
                {
                    iLading.MoneyTypePay = "TRUE";
                    iLading.PayAtPos = "TRUE";
                }
                iLading.CollectPayer = "TRUE";
                iLading.DeliveriedPayer = "TRUE";
                string _code = l.tracking_code;
                string _refCode = l.RefCode;
                if (!string.IsNullOrEmpty(_code))
                {
                    if (!string.IsNullOrEmpty(_refCode))
                    {
                        iLading.Code = _refCode;
                    }
                    else
                    {
                        iLading.Code = _code;
                    }
                }
                else
                {
                    iLading.Code = "CZxxxxxxxxxVN";
                }
                if (!string.IsNullOrEmpty(MainKey))
                {
                    iLading.CustomerCode = MainKey;
                }
                else
                {
                    iLading.CustomerCode = l.customer.code.ToString();
                }
                iLading.GroupCustomerCode = "";
                iLading.SenderName = l.from_address.name;
                iLading.SenderAddr = l.from_address.address;
                iLading.SenderTel = l.from_address.phone;
                iLading.SenderEmail = l.from_address.email;
                string _senderPo = string.Empty;

                if (l.from_address.province.ToString().Length == 1)
                {
                    _senderPo = "0" + l.from_address.province.ToString();
                }
                else
                {
                    _senderPo = l.from_address.province.ToString();
                }

                if (!string.IsNullOrEmpty(_code) && _code.StartsWith("E"))
                {
                    iLading.TypePackage = "E";
                }
                else
                {
                    iLading.TypePackage = "AR";
                }

                if (!string.IsNullOrEmpty(_refCode))
                {
                    iLading.TypePackage = "R";
                }

                iLading.SenderPostCode = _senderPo;
                iLading.NotDeliveriedInstuct = "2";
                iLading.SenderTaxCode = "";
                iLading.SenderIdentification = "";
                iLading.ReceiverName = l.to_address.name;
                iLading.ReceiverAddr = l.to_address.address;
                iLading.ReceiverCountry = "";

                string _toProvince = l.to_address.province.ToString();
                try
                {
                    //if (!string.IsNullOrEmpty(_toProvince) && _toProvince.Length < 2)
                    //{
                    //    iLading.ReceiverProvince = "0" + _toProvince;
                    //}
                    //else if (!string.IsNullOrEmpty(_toProvince) && _toProvince.Length >= 2)
                    //{
                    //    iLading.ReceiverProvince = _toProvince;
                    //}
                    //else
                    //{
                    //    iLading.ReceiverProvince = "00";
                    //}
                    dynamic province = Configuration.Data.Get("mbcProvince", Query.EQ("ProvinceCode", l.to_address.province.ToString()));
                    if (province != null)
                    {
                        iLading.ReceiverProvince = province.ProvinceName;
                    }
                    else
                    {
                        iLading.ReceiverProvince = "";
                    }
                }
                catch
                {
                    iLading.ReceiverProvince = "00";
                }

                string _toDistrict = l.to_address.district.ToString();
                try
                {
                    if (!string.IsNullOrEmpty(_toDistrict) && _toDistrict.Length <= 1)
                    {
                        iLading.ReceiverDistrict = "0000";
                    }
                    else if (!string.IsNullOrEmpty(_toDistrict) && _toDistrict.Length == 2)
                    {
                        iLading.ReceiverDistrict = "00" + _toDistrict;
                    }
                    else if (!string.IsNullOrEmpty(_toDistrict) && _toDistrict.Length <= 3)
                    {
                        iLading.ReceiverDistrict = "0" + _toDistrict;
                    }
                    else if (!string.IsNullOrEmpty(_toDistrict) && _toDistrict.Length >= 4)
                    {
                        iLading.ReceiverDistrict = _toDistrict;
                    }
                    else
                    {
                        iLading.ReceiverDistrict = "0000";
                    }
                }
                catch
                {
                    iLading.ReceiverDistrict = "0000";
                }
                string _toWard = l.to_address.ward.ToString();
                try
                {
                    if (!string.IsNullOrEmpty(_toWard) && _toWard.Length <= 1)
                    {
                        iLading.ReceiverCounty = "0000";
                    }
                    else if (!string.IsNullOrEmpty(_toWard) && _toWard.Length == 2)
                    {
                        iLading.ReceiverCounty = "00" + _toWard;
                    }
                    else if (!string.IsNullOrEmpty(_toWard) && _toWard.Length == 3)
                    {
                        iLading.ReceiverCounty = "0" + _toWard;
                    }
                    else if (!string.IsNullOrEmpty(_toWard) && _toWard.Length >= 4)
                    {
                        iLading.ReceiverCounty = _toWard;
                    }
                    else
                    {
                        iLading.ReceiverCounty = "0000";
                    }
                }
                catch
                {
                    iLading.ReceiverCounty = "0000";
                }
                if (l.to_address.phone != null)
                {
                    iLading.ReceiverMobile = l.to_address.phone;
                    if (iLading.ReceiverMobile.Length > 11)
                    {
                        iLading.ReceiverMobile = iLading.ReceiverMobile.Replace("<br/>", "-");
                        if (iLading.ReceiverMobile.Contains('-'))
                        {
                            string[] arrMobile = iLading.ReceiverMobile.Split('-');
                            iLading.ReceiverMobile = arrMobile[0];
                        }
                    }
                    iLading.ReceiverMobile = iLading.ReceiverMobile.Replace("-", "");
                }
                else
                {
                    iLading.ReceiverMobile = "";
                }
                iLading.DocumentCode = l.customer.code.ToString();
                try
                {
                    iLading.ReceiverEmail = l.to_address.email;
                }
                catch
                {
                    iLading.ReceiverEmail = "";
                }
                iLading.ContactName = "";
                try
                {
                    iLading.ReceiverPostCode = l.to_address.province.ToString();
                }
                catch
                {
                    iLading.ReceiverPostCode = "00";
                }
                try
                {
                    if (!string.IsNullOrEmpty(l.product.description.Trim()))
                    {
                        iLading.Content = l.product.description;
                    }
                    else
                    {
                        iLading.Content = "Chưa có";
                    }
                }
                catch
                {
                    iLading.Content = "Không có";
                }
                try
                {
                    if (l.parcel != null && l.parcel.weight != null)
                    {
                        iLading.Weight = decimal.Parse(l.parcel.weight.ToString());
                    }
                    else
                    {
                        if (l.product.weight != null)
                        {
                            iLading.Weight = decimal.Parse(l.product.weight.ToString());
                        }
                        else
                        {
                            iLading.Weight = 0;
                        }
                    }
                }
                catch
                {
                    iLading.Weight = 0;
                }

                try
                {
                    iLading.AmountCOD = decimal.Parse(l.product.value.ToString());
                }
                catch
                {
                    iLading.AmountCOD = 0;
                }
                try
                {
                    iLading.TaskNumber = l.order_id;
                }
                catch
                {
                    iLading.TaskNumber = "";
                }
                lstLading.Add(iLading);
                //}

            }

            ReportDataSource rd = new ReportDataSource("InfoNePost", lstLading);
            lr.DataSources.Add(rd);
            string reportType = "Excel";// id;
            string mimeType;
            string encoding;
            string fileNameExtension;

            if (!string.IsNullOrEmpty(status))
            {
                switch (status)
                {
                    case "C6":
                        _expTitle = "Danh sách yêu cầu đang xử lý";
                        break;
                    case "C10":
                        _expTitle = "Danh sách khách hàng hẹn lại";
                        break;
                    default:
                        _expTitle = "Danh sách yêu cầu chưa điều";
                        break;
                }
            }

            string deviceInfo =

            "<DeviceInfo>" +
            "  <OutputFormat>" + reportType + "</OutputFormat>" +
                //"  <PageWidth>9in</PageWidth>" +
                //"  <PageHeight>10in</PageHeight>" +
                //"  <MarginTop>0.5in</MarginTop>" +
                //"  <MarginLeft>1in</MarginLeft>" +
                //"  <MarginRight>0in</MarginRight>" +
                //"  <MarginBottom>0in</MarginBottom>" +
            "</DeviceInfo>";

            Warning[] warnings;
            string[] streams;
            byte[] renderedBytes;

            renderedBytes = lr.Render(
                reportType,
                deviceInfo,
                out mimeType,
                out encoding,
                out fileNameExtension,
                out streams,
                out warnings);


            return File(renderedBytes, mimeType, _expTitle + ".xls");
        }
        #endregion
        #region ExpInfoRequire
        public ActionResult ExpInfoRequire(string from_date, string to_date, string customer_code, string tracking_code, string request_id, string status)
        {
            //if (string.IsNullOrEmpty(from_date))

            from_date = from_date.Substring(6, 4) + "-" + from_date.Substring(3, 2) + "-" + from_date.Substring(0, 2);
            to_date = to_date.Substring(6, 4) + "-" + to_date.Substring(3, 2) + "-" + to_date.Substring(0, 2);

            LocalReport lr = new LocalReport();
            string path = Path.Combine(Server.MapPath("~/Areas/ServiceRequest/Templates"), "RequestExp.rdlc");
            if (System.IO.File.Exists(path))
            {
                lr.ReportPath = path;
            }
            else
            {
                return View("Index");
            }
            List<Models.RequireInfo> lstLading = new List<Models.RequireInfo>();

            #region declare varialbles
            List<dynamic> list = new List<dynamic>();
            #endregion

            string unit_link = ((dynamic)Session["profile"]).unit_link;
            if (unit_link == "00") unit_link = "";
            MongoDB.Bson.BsonRegularExpression reg = new MongoDB.Bson.BsonRegularExpression("^" + unit_link);
            IMongoQuery _query = Query.Matches("current_assigned", reg);
            long _fromdt = long.Parse(DateTime.Parse(from_date).ToString("yyyyMMdd"));
            long _todt = long.Parse(DateTime.Parse(to_date).ToString("yyyyMMdd"));
            if (_fromdt == _todt)
            {
                _query = Query.And(
                    _query,
                            Query.GTE("system_time_key.date", _fromdt)
                    );
            }
            else
            {
                _query = Query.And(
                    _query,
                            Query.GTE("system_time_key.date", _fromdt)
                            , Query.LTE("system_time_key.date", _todt)
                    );
            }
            if (!String.IsNullOrEmpty(customer_code))
            {
                _query = Query.And(
                    _query,
                    Query.EQ("customer.code", customer_code));
            }

            if (!String.IsNullOrEmpty(tracking_code))
            {
                _query = Query.And(
                    _query,
                    Query.EQ("tracking_code", tracking_code));
            }

            if (!String.IsNullOrEmpty(status))
            {
                if (status == "C5")
                {
                    _query = Query.And(
                    _query,
                    Query.Or(Query.EQ("system_status", "C5"), Query.EQ("system_status", "C25")));
                }
                else
                {
                    _query = Query.And(
                    _query,
                    Query.EQ("system_status", status));
                }

            }

            if (!String.IsNullOrEmpty(request_id))
            {
                if (!request_id.Contains("|"))
                {
                    _query = Query.And(
                        _query,
                        Query.EQ("_id", request_id));
                }
                else
                {
                    string[] arrReqId = request_id.Substring(0, request_id.Length - 1).Split('|');
                    var documentIds = new BsonValue[arrReqId.Length];

                    for (int j = 0; j < arrReqId.Length; j++)
                    {
                        documentIds[j] = arrReqId[j];
                    }
                    _query = Query.And(
                        _query,
                        Query.In("_id", documentIds));
                }
            }

            List<dynamic> _list = Configuration.Data.List("shipment", _query).ToList<dynamic>();
            _list = (from c in _list orderby c._id descending select c).ToList<dynamic>();

            string _expTitle = "Danh sách thu gom hàng TMDT";
            int i = 0;
            foreach (dynamic l in _list)
            {
                Models.RequireInfo iLading = new Models.RequireInfo();
                string MainKey = "";
                string contract = "";
                i++;
                try
                {
                    if (!string.IsNullOrEmpty(l.customer.code))
                    {
                        dynamic custObj = Configuration.Data.Get("business_profile", Query.EQ("_id", l.customer.code));
                        contract = custObj.general_full_name;
                        dynamic apiKey = Configuration.Data.Get("api_key", Query.EQ("business_profile", l.customer.code));
                        if (apiKey != null)
                        {
                            MainKey = apiKey.KeyCode + " - " + contract;
                        }
                    }
                }
                catch
                {
                    MainKey = "";
                }
                iLading.STT = decimal.Parse(i.ToString());
                string _code = l.tracking_code;
                string _refCode = "";
                if (l.RefCode != null)
                {
                    _refCode = l.RefCode;
                }
                if (!string.IsNullOrEmpty(_code))
                {
                    if (!string.IsNullOrEmpty(_refCode))
                    {
                        iLading.Code = _refCode;
                    }
                    else
                    {
                        iLading.Code = _code;
                    }
                    iLading.ProductCode = l.tracking_code;
                }
                else
                {
                    iLading.Code = "CZxxxxxxxxxVN";
                }

                if (!string.IsNullOrEmpty(MainKey))
                {
                    iLading.CustomerCode = MainKey;
                }
                else
                {
                    iLading.CustomerCode = l.customer.code.ToString() + " - " + contract;
                }

                if (l.to_address.phone != null)
                {
                    iLading.ReceiverMobile = l.to_address.phone;
                    if (iLading.ReceiverMobile.Length > 11)
                    {
                        iLading.ReceiverMobile = iLading.ReceiverMobile.Replace("<br/>", "-");
                        if (iLading.ReceiverMobile.Contains('-'))
                        {
                            string[] arrMobile = iLading.ReceiverMobile.Split('-');
                            iLading.ReceiverMobile = arrMobile[0];
                        }
                    }
                    iLading.ReceiverMobile = iLading.ReceiverMobile.Replace("-", "");
                }
                else
                {
                    iLading.ReceiverMobile = "";
                }

                if (l.to_address.address != null)
                {
                    iLading.ReceiverAddr = l.to_address.address;
                }
                else
                {
                    iLading.ReceiverAddr = "";
                }

                try
                {
                    if (!string.IsNullOrEmpty(l.product.description.Trim()))
                    {
                        iLading.Content = l.product.description;
                    }
                    else
                    {
                        iLading.Content = "Chưa có";
                    }
                }
                catch
                {
                    iLading.Content = "Không có";
                }
                try
                {
                    if (l.parcel != null && l.parcel.weight != null)
                    {
                        iLading.Weight = decimal.Parse(l.parcel.weight.ToString());
                    }
                    else
                    {
                        if (l.product.weight != null)
                        {
                            iLading.Weight = decimal.Parse(l.product.weight.ToString());
                        }
                    }
                }
                catch
                {
                    iLading.Weight = 0;
                }
                iLading.Amount = decimal.Parse(l.product.value.ToString());
                try
                {
                    if (l.service != null)
                    {
                        if (l.service.shipping_add_service != null && l.service.shipping_add_service == "COD")
                        {
                            iLading.AmountCOD = decimal.Parse(l.product.value.ToString());
                        }
                        else
                        {
                            iLading.AmountCOD = 0;
                        }
                    }
                }
                catch
                {
                    iLading.AmountCOD = 0;
                }
                iLading.Description = "";

                lstLading.Add(iLading);

            }

            ReportDataSource rd = new ReportDataSource("DsInfoExp", lstLading);
            lr.DataSources.Add(rd);
            string reportType = "Excel";// id;
            string mimeType;
            string encoding;
            string fileNameExtension;

            string deviceInfo =

            "<DeviceInfo>" +
            "  <OutputFormat>" + reportType + "</OutputFormat>" +
                //"  <PageWidth>9in</PageWidth>" +
                //"  <PageHeight>10in</PageHeight>" +
                //"  <MarginTop>0.5in</MarginTop>" +
                //"  <MarginLeft>1in</MarginLeft>" +
                //"  <MarginRight>0in</MarginRight>" +
                //"  <MarginBottom>0in</MarginBottom>" +
            "</DeviceInfo>";

            Warning[] warnings;
            string[] streams;
            byte[] renderedBytes;

            renderedBytes = lr.Render(
                reportType,
                deviceInfo,
                out mimeType,
                out encoding,
                out fileNameExtension,
                out streams,
                out warnings);


            return File(renderedBytes, mimeType, _expTitle + ".xls");
        }
        #endregion
        #region UpdateLadingWeight
        public JsonResult UpdateLadingWeight(string id, string weight)
        {

            dynamic _request = Configuration.Data.Get("shipment", Query.EQ("_id", id));
            dynamic _lading = new DynamicObj();

            if (_request != null)
            {
                _lading = Configuration.Data.Get("Lading", Query.EQ("Code", _request.tracking_code));

                if (!string.IsNullOrEmpty(weight))
                {
                    if (_request.parcel != null)
                    {
                        _request.parcel.weight = Int64.Parse(weight);
                    }

                    if (_request.product.weight != null)
                    {
                        _request.product.weight = Int64.Parse(weight);
                    }
                }
            }
            if (_lading != null)
            {
                //lưu log thay đổi thông tin khối lượng vận đơn
                try
                {
                    dynamic _logChange = new DynamicObj();
                    _logChange.Code = _lading.Code;
                    _logChange.ProductName = _lading.ProductName;
                    _logChange.ProductDescription = _lading.ProductDescription;
                    _logChange.Value = _lading.Value;
                    _logChange.Quantity = _lading.Quantity;
                    _logChange.ReceiverName = _lading.ReceiverName;
                    _logChange.ReceiverAddress = _lading.ReceiverAddress;
                    _logChange.ToProvinceCode = _lading.ToProvinceCode;
                    _logChange.FromProvinceCode = _lading.FromProvinceCode;
                    _logChange.CustomerCode = _lading.CustomerCode;
                    _logChange.Weight_old = _lading.Weight;
                    _logChange.Weight = Int64.Parse(weight);

                    bool sLog = Configuration.Data.Save("LogChangeLading", _logChange);
                }
                catch
                {

                }

                if (_lading.Weight != null)
                {
                    _lading.Weight = Int64.Parse(weight);
                }
            }

            if (Configuration.Data.Save("shipment", _request) && Configuration.Data.Save("Lading", _lading))
            {

                return Json(new { response_code = "00", response_message = "Khối lượng đã được điều chỉnh thành công" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { response_code = "01", response_message = "Chưa điều chỉnh được khối lượng" }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
    public class DownloadFileActionResult : ActionResult
    {

        public GridView ExcelGridView { get; set; }
        public string fileName { get; set; }


        public DownloadFileActionResult(GridView gv, string pFileName)
        {
            ExcelGridView = gv;
            fileName = pFileName;
        }


        public override void ExecuteResult(ControllerContext context)
        {

            //Create a response stream to create and write the Excel file
            HttpContext curContext = HttpContext.Current;
            curContext.Response.Clear();
            curContext.Response.AddHeader("content-disposition", "attachment;filename=" + fileName);
            curContext.Response.Charset = "UTF8";
            curContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            curContext.Response.ContentType = "application/vnd.ms-excel";

            //Convert the rendering of the gridview to a string representation 
            StringWriter sw = new StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);
            ExcelGridView.RenderControl(htw);

            //Open a memory stream that you can use to write back to the response
            byte[] byteArray = Encoding.UTF8.GetBytes(sw.ToString());
            MemoryStream s = new MemoryStream(byteArray);
            StreamReader sr = new StreamReader(s, Encoding.UTF8);

            //Write the stream back to the response
            curContext.Response.Write(sr.ReadToEnd());
            curContext.Response.End();

        }

    }
}

public class Response
{
    public string response_code { get; set; }
    public string response_message { get; set; }
}