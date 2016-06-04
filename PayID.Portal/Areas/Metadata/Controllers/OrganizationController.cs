using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;
using MongoDB.Driver.Builders;
using MongoDB.Driver;
using System.Web.Mvc;
using MongoDB.Bson;
using PayID.DataHelper;
using Newtonsoft.Json;
using PayID.Portal.Common.Service;
using PayID.Portal.Models;
using PayID.Portal.Common;

namespace PayID.Portal.Areas.Metadata.Controllers
{
    public class OrganizationController : Controller
    {
        DictionaryService DICTIONARY_SERVICE = null;
        AccountService ACCOUNT_SERVICE = null;

        public OrganizationController()
        {
            if(DICTIONARY_SERVICE == null)
            {
                DICTIONARY_SERVICE = new DictionaryService();
            }

            if(ACCOUNT_SERVICE == null)
            {
                ACCOUNT_SERVICE = new AccountService();
            }
        }

        public static List<dynamic> LIST_UNIT;
        // GET: Metadata/Organization
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

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ListUsers(string UnitCode)
        {
            var ListUser = ACCOUNT_SERVICE.GetAccountByUnitCode(UnitCode);
            return PartialView(ListUser);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult CreateAccount(string _id = "")
        {
            Account oAccount = new Account();
            List<SelectListItem> ListRole = new List<SelectListItem>();
            ListRole.Add(new SelectListItem
            {
                Text = "---Quyền---",
                Value = ""
            });

            foreach(var item in Common.Configuration.ListRole)
            {
                ListRole.Add(new SelectListItem
                {
                    Text = item.Name,
                    Value = item.Id
                });
            }

            if(!string.IsNullOrEmpty(_id.Trim()))
            {
                oAccount = ACCOUNT_SERVICE.GetAccountByUserName(_id);
                oAccount.Id = 696969;
            }

            ViewBag.ListRole = ListRole;
            return PartialView(oAccount);
        }

        [ValidateAntiForgeryToken]
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult UpdateAccount(Account _Account)
        {
            JsonResult jResult = new JsonResult();
            try
            {
                if(Session["profile"] == null)
                {
                    jResult = Json(new { Code = "-101", Mes = "Đăng nhập trước khi tiếp tục thực hiện thao tác." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    Account oAccount = (Account)Session["profile"];

                    _Account.Amnd_User = oAccount.UserName;
                    _Account.UnitCreate = oAccount.UnitCode;
                    if(_Account.Id == 0)
                    {
                        _Account.Password = Security.CreatPassWordHash(_Account.Password);
                    }

                    string sReturn = ACCOUNT_SERVICE.UpdateAccount(_Account);

                    string Code = sReturn.Split('|')[0].ToString();
                    string Mes = sReturn.Split('|')[1].ToString();

                    jResult = Json(new { Code = Code, Mes = Mes }, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {
                jResult = Json(new { Code = "-99", Mes = "Có lỗi trong quá trình xử lý dữ liệu" }, JsonRequestBehavior.AllowGet);
            }

            return jResult;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult ChangeStatusAccount(string _id)
        {
            JsonResult jResult = new JsonResult();

            try
            {
                if (Session["profile"] == null)
                {
                    jResult = Json(new { Code = "-101", Mes = "Đăng nhập trước khi tiếp tục thực hiện thao tác." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    Account oAccount = new Account();
                    oAccount.UserName = _id;

                    string sReturn = ACCOUNT_SERVICE.ChangeStatusAccount(oAccount);

                    string Code = sReturn.Split('|')[0].ToString();
                    string Mes = sReturn.Split('|')[1].ToString();

                    jResult = Json(new { Code = Code, Mes = Mes }, JsonRequestBehavior.AllowGet);
                }
            }
            catch(Exception ex)
            {
                jResult = Json(new { Code = "-99", Mes = "Có lỗi trong quá trình xử lý dữ liệu" }, JsonRequestBehavior.AllowGet);
            }

            return jResult;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ListPo(string UnitCode, int PageIndex)
        {
            if(UnitCode.Length == 4)
            {
                long lTotal = 0;
                var listPost = DICTIONARY_SERVICE.GetPost("", "", 0, "", "", "", UnitCode, PageIndex, ref lTotal);

                ViewBag.UnitCode = UnitCode;
                ViewBag.PageIndex = PageIndex;
                ViewBag.PageSize = Common.Configuration.PageSize;
                ViewBag.ToTal = lTotal;

                return PartialView(listPost);
            }
            else
            {
                return RedirectToAction("ListUnit", "Organization", new { Area = "Metadata", UnitCode = UnitCode, PageIndex = PageIndex });
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ListUnit(string UnitCode, int PageIndex)
        {
            if(string.IsNullOrEmpty(UnitCode.Trim()))
            {
                UnitCode = "00";
            }

            long lTotal =0;

            var listUnit = DICTIONARY_SERVICE.GetUnit("", UnitCode, "", "", PageIndex, ref lTotal);

            ViewBag.UnitCode = UnitCode;
            ViewBag.PageIndex = PageIndex;
            ViewBag.PageSize = Common.Configuration.PageSize;
            ViewBag.ToTal = lTotal;

            return PartialView(listUnit);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult EditPos(string PosCode = "", string ProvinceCode = "")
        {
            Post oPost = new Post();

            List<SelectListItem> ListProvince = new List<SelectListItem>();
            List<SelectListItem> ListDistrict = new List<SelectListItem>();
            List<SelectListItem> ListWard = new List<SelectListItem>();

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

            foreach(var item in Common.Configuration.ListProvince)
            {
                ListProvince.Add(new SelectListItem
                {
                    Text = item.Description,
                    Value = item.ProvinceCode
                });
            }

           if(!string.IsNullOrEmpty(PosCode))
           {
               long lTotal =0;
               var _post = DICTIONARY_SERVICE.GetPost(PosCode, "", 0, "", "", "", "", 0, ref lTotal);

               if(_post != null && _post.Count > 0)
               {
                   oPost = _post[0];
                   oPost.Id = 696969;
               }
           }
           else
           {
               oPost.ProvinceCode = ProvinceCode;
           }


           if (!string.IsNullOrEmpty(oPost.ProvinceCode))
           {
               //Lấy danh sách Quận, Huyện dựa vào Provice
               var _listDistrict = DICTIONARY_SERVICE.GetDistrictByProvince(oPost.ProvinceCode);
               if (_listDistrict != null && _listDistrict.Count > 0)
               {
                   foreach (var item in _listDistrict)
                   {
                       ListDistrict.Add(new SelectListItem
                       {
                           Text = item.DistrictName,
                           Value = item.DistrictCode
                       });
                   }
               }

               if (!string.IsNullOrEmpty(oPost.CommuneCode))
               {
                   var _ward = DICTIONARY_SERVICE.GetWardByDistrict("", oPost.CommuneCode);

                   if (_ward != null && _ward.Count > 0)
                   {
                       oPost.DistrictCode = _ward[0].DistrictCode;

                       if (!string.IsNullOrEmpty(_ward[0].DistrictCode))
                       {
                           var _listWard = DICTIONARY_SERVICE.GetWardByDistrict(_ward[0].DistrictCode);
                           //Lấy danh sách Quận, Huyện dựa vào Provice
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
                       }
                   }
               }
           }

           ViewBag.ListProvince = ListProvince;
           ViewBag.ListDistrict = ListDistrict;
           ViewBag.ListWard = ListWard;

           return PartialView(oPost);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult UpdatePost(Post oPost)
        {
            JsonResult jResult = new JsonResult();

            try
            {
                string sResult = DICTIONARY_SERVICE.UpdatePos(oPost);

                string Code = sResult.Split('|')[0];
                string Mes = sResult.Split('|')[1];

                jResult = Json(new { Code = Code, Mes = Mes });
            }
            catch
            {
                jResult = Json(new { Code = "-99", Mes = "Có lỗi trong quá trình xử lý dữ liệu." }, JsonRequestBehavior.AllowGet);
            }

            return jResult;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult EditUnit(string UnitCode)
        {
            Unit oUnit = new Unit();

            List<SelectListItem> ListProvince = new List<SelectListItem>();
            List<SelectListItem> ListDistrict = new List<SelectListItem>();
            List<SelectListItem> ListWard = new List<SelectListItem>();

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

            foreach(var item in Common.Configuration.ListProvince)
            {
                ListProvince.Add(new SelectListItem
                {
                    Text = item.Description,
                    Value = item.ProvinceCode
                });
            }

            long lTotal =0;
            var _listUnit = DICTIONARY_SERVICE.GetUnit(UnitCode, "", "", "", 0, ref lTotal);
            if(_listUnit != null && _listUnit.Count > 0)
            {
                oUnit = _listUnit[0];

                if(oUnit.UnitCode.Length == 4)
                {
                    oUnit.DistrictCode = oUnit.UnitCode;

                    var District = DICTIONARY_SERVICE.GetDistrictByProvince("", oUnit.UnitCode);

                    if(District != null && District.Count > 0)
                    {
                        oUnit.ProvinceCode = Common.Configuration.ListProvince.FirstOrDefault(x => x.ProvinceCode == District[0].ProvinceCode).ProvinceCode;
                    }
                }
                else
                {
                    oUnit.ProvinceCode = oUnit.UnitCode;

                    if (!string.IsNullOrEmpty(oUnit.CommuneCode))
                    {
                        var Commune = DICTIONARY_SERVICE.GetWardByDistrict("", oUnit.CommuneCode);

                        if (Commune != null && Commune.Count > 0)
                        {
                            oUnit.DistrictCode = Commune[0].DistrictCode;
                        }
                    }
                }


                if(!string.IsNullOrEmpty(oUnit.DistrictCode))
                {
                    var _ListWard = DICTIONARY_SERVICE.GetWardByDistrict(oUnit.DistrictCode);

                    foreach (var item in _ListWard)
                    {
                        ListWard.Add(new SelectListItem
                        {
                            Text = item.WardName,
                            Value = item.WardCode
                        });
                    }
                }

                if(!string.IsNullOrEmpty(oUnit.ProvinceCode))
                {
                    var _District = DICTIONARY_SERVICE.GetDistrictByProvince(oUnit.ProvinceCode);

                    if(_District != null && _District.Count > 0)
                    {
                        foreach(var item in _District)
                        {
                            ListDistrict.Add(new SelectListItem
                            {
                                Text = item.DistrictName,
                                Value = item.DistrictCode
                            });
                        }
                    }
                }        
            }

            ViewBag.ListProvince = ListProvince;
            ViewBag.ListDistrict = ListDistrict;
            ViewBag.ListWard = ListWard;

            return PartialView(oUnit);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult UpdateUnit(Unit oUnit)
        {
            JsonResult jResult = new JsonResult();

            if (Session["profile"] == null)
            {
                jResult = Json(new { Code = "-101", Mes = "Đăng nhập trước khi tiếp tục thực hiện thao tác." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                string sResult = DICTIONARY_SERVICE.UpdateUnit(oUnit);

                string Code = sResult.Split('|')[0].ToString();
                string Mes = sResult.Split('|')[1].ToString();

                jResult = Json(new { Code = Code, Mes = Mes });
            }

            return jResult;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult ChangeStatusPos(string PostCode)
        {
            JsonResult jResult = new JsonResult();

            try
            {
                Post oPost = new Post();

                oPost.PosCode = PostCode;

                string sResult = DICTIONARY_SERVICE.ChangeStatusPos(oPost);
                string Code = sResult.Split('|')[0];
                string Mes = sResult.Split('|')[1];

                jResult = Json(new { Code = Code, Mes = Mes }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                jResult = Json(new { Code = "-99", Mes = "Có lỗi trong quá trình xử lý dữ liệu." }, JsonRequestBehavior.AllowGet);
            }

            return jResult;
        }

        public ActionResult ListUserRole(string unitCode)
        {
            DynamicObj[] UserRoleList = Configuration.Data.List("user_role", Query.EQ("unit_code", unitCode));
            ViewBag.user_role = UserRoleList;
            return View();
        }
        public ActionResult ListUnitInfo(string unitCode)
        {
            DynamicObj[] UnitInfoList = Configuration.Data.List("unit_info", Query.EQ("unit_code", unitCode));
            ViewBag.unit_info = UnitInfoList;
            return View();
        }
        public JsonResult SaveAccount(string _id, string full_name, string password, string role, string unit_code, string unit_link, string status, string address, string phone, string user_office, string _idEdit)
        {
            string v_unit_name,v_unit_type;
            string return_code = ""; string message_return = "";
            if (unit_link.EndsWith(".")) unit_link = unit_link.Substring(0, unit_link.Length - 1);
            if (unit_link.StartsWith("00") && unit_link.Length>3) unit_link = unit_link.Remove(0, 3);
            dynamic myObjPos = new DynamicObj(); dynamic myObjUnit = new DynamicObj();
            if (unit_code.Length >= 6)
            {
                myObjPos = Configuration.Data.Get("mbcPos", Query.EQ("_id", unit_code));
                v_unit_name = myObjPos._id + "-" + myObjPos.POSName;
                v_unit_type = "6";
            }
            else
            {
                myObjUnit = Configuration.Data.Get("mbcUnit", Query.EQ("UnitCode", unit_code));
                v_unit_name = myObjUnit.UnitName;
                v_unit_type = myObjUnit.UnitTypeCode;
            }
            dynamic myObj = new DynamicObj();
            try
            {
                if (!string.IsNullOrEmpty(_id) && !string.IsNullOrEmpty(unit_code))
                {
                    if (!string.IsNullOrEmpty(_idEdit))
                    {
                        myObj = Configuration.Data.Get("user", Query.EQ("_id", _idEdit));
                    }
                    else
                    {
                        myObj = Configuration.Data.Get("user", Query.EQ("_id", _id));

                        if(myObj!=null)
                        {
                            return_code = "02"; message_return = "Tài khoản đã tồn tại.";
                            return Json(new { result = return_code, message = message_return });
                        }
                        else
                        {
                            myObj = new DynamicObj();
                        }

                        myObj._id = _id;
                    }
                    
                    myObj.full_name = full_name;
                    myObj.role = role;
                    myObj.address = address;
                    myObj.phone = phone;
                    myObj.unit_code = unit_code;
                    myObj.unit_name = v_unit_name;
                    myObj.user_office = user_office;
                    myObj.unit_link = unit_link;
                    myObj.unit_type = v_unit_type;
                    myObj.status = int.Parse(status);
                    myObj.password = PayID.Common.Security.CreatPassWordHash(password);
                    PayID.Portal.Configuration.Data.Save("user", myObj);
                    return_code = "00"; message_return = "Cập nhật thành công."; 
                }
            }
            catch
            {
                return_code = "01"; message_return = "Cập nhật thất bại.";
            }
            return Json(new { result = return_code, message = message_return });
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ListTreeByUnit(string UnitCode, int Margin = 0)
        {
            long lTotal = 0;
            List<ListTree> ListTree = new List<ListTree>();
            if (UnitCode.Length == 2)
            {
                var listParent = DICTIONARY_SERVICE.GetUnit("", UnitCode, "", "", 0, ref lTotal);

                if (listParent != null && listParent.Count > 0)
                {
                    foreach (var item in listParent)
                    {
                        ListTree.Add(new ListTree
                        {
                            UnitCode = item.UnitCode,
                            UnitLink = UnitCode + "." + item.UnitCode,
                            UnitName = item.UnitCode + " - " + item.UnitName,
                            UnitParent = item.ParentUnitCode,
                            ProvinceCode = item.UnitCode.Length == 4 ? item.ParentUnitCode : item.UnitCode
                        });
                    }
                }
            }
            else if(UnitCode.Length == 4)
            {
                var Unit = DICTIONARY_SERVICE.GetUnit(UnitCode,"","","",0,ref lTotal);
                var listParent = DICTIONARY_SERVICE.GetPost("", "", 0, "", "", "", UnitCode, 0, ref lTotal);

                if (listParent != null && listParent.Count > 0)
                {
                    foreach (var item in listParent)
                    {
                        ListTree.Add(new ListTree
                        {
                            UnitCode = item.PosCode,
                            UnitLink = Unit[0].ParentUnitCode + "." + UnitCode + "." + item.PosCode,
                            UnitName = item.PosCode + " - " + item.PosName,
                            UnitParent = Unit[0].ParentUnitCode,
                            ProvinceCode = (UnitCode.Length == 4 ? Unit[0].ParentUnitCode : item.ProvinceCode)
                        });
                    }
                }
            }

            ViewBag.Margin = Margin + 10;

            return PartialView(ListTree);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ListTree()
        {
            if (Session["profile"] == null)
            {
                var profile = ACCOUNT_SERVICE.GetAccountByUserName(User.Identity.Name);
                Session["profile"] = profile;
            }

            Account oProfile = (Account)Session["profile"];
            long lTotal = 0;

            List<ListTree> ListTree = new List<ListTree>();

            if (oProfile.UnitCode.Length == 6)
            {
                var listPO = DICTIONARY_SERVICE.GetPost(oProfile.UnitCode, "", 0, "", "", "", "", 0, ref lTotal);

                if(listPO != null && listPO.Count > 0)
                {
                    foreach(var item in listPO)
                    {
                        ListTree.Add(new ListTree
                        {
                            UnitCode = item.PosCode,
                            UnitLink = item.UnitCode + "." + item.PosCode,
                            UnitName = item.PosCode + " - " + item.PosName,
                            UnitParent = item.UnitCode,
                            ProvinceCode = item.ProvinceCode
                        });
                    }
                }
                
            }
            else
            {
                var listUnit = DICTIONARY_SERVICE.GetUnit(oProfile.UnitCode, "", "", "", 0, ref lTotal);

                if (listUnit != null && listUnit.Count > 0)
                {
                    foreach (var item in listUnit)
                    {
                        ListTree.Add(new ListTree
                        {
                            UnitCode = item.UnitCode,
                            UnitLink = string.IsNullOrEmpty(item.ParentUnitCode) == true ? "00" : item.ParentUnitCode + "." + item.UnitCode,
                            UnitName = item.UnitCode + " - " + item.UnitName,
                            UnitParent = item.ParentUnitCode,
                            ProvinceCode = (item.UnitCode.Length == 4 ? item.ParentUnitCode : item.ProvinceCode)
                        });
                    }
                }
            }

            return PartialView(ListTree);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult LoadPosTree(string UnitCode, string UnitLink = "")
        {

            dynamic[] listSub = new dynamic[] { };
            string result = string.Empty;

            long lTotal = 0;
            if(UnitCode.Length == 2)
            {
                var listParent = DICTIONARY_SERVICE.GetUnit("", UnitCode, "", "", 0, ref lTotal);

                if (listParent != null && listParent.Count > 0)
                {
                    foreach (var leaf in listParent)
                    {
                        result = result + String.Concat(
                        "<li><span data-toggle='collapse'></span> <a id='po-" + leaf.UnitCode + "' onclick=Po.LoadTree('" + leaf.UnitCode + "') data-toggle='collapse' class='parentKey' keyVal='"
                       , leaf.UnitCode,
                       "' keyAddr='"
                       , "",
                       "' keyFullCode='"
                       , UnitLink + "." + leaf.UnitCode,
                        "' keyFullName='"
                       , leaf.UnitName,
                       "' href='#'>"
                       , leaf.UnitCode + '-' + leaf.UnitName
                       , "</a><ul id='tree-" + leaf.UnitCode + "'> </ul>");
                    }
                }
            }
            else if(UnitCode.Length == 4)
            {
                var listParent = DICTIONARY_SERVICE.GetPost("", "", 0, "", "", "", UnitCode, 0, ref lTotal);

                if (listParent != null && listParent.Count > 0)
                {
                    foreach (var leaf in listParent)
                    {
                        result = result + String.Concat(
                        "<li><span data-toggle='collapse'></span> <a id='po-" + leaf.PosCode + "'  data-toggle='collapse' class='parentKey' keyVal='"
                       , leaf.PosCode,
                       "' keyAddr='"
                       , "",
                       "' keyFullCode='"
                       , UnitLink + "." + leaf.PosCode,
                        "' keyFullName='"
                       , leaf.PosName,
                       "' href='#'>"
                       , leaf.PosCode + '-' + leaf.PosName
                       , "</a><ul id='tree-" + leaf.PosCode + "'> </ul></li>");
                    }
                }
            }

            return Json(new { Data = result }, JsonRequestBehavior.AllowGet);

        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult LoadTree()
        {
            if (Session["profile"] == null)
            {
                var profile = ACCOUNT_SERVICE.GetAccountByUserName(User.Identity.Name);
                Session["profile"] = profile;
            }

            Account oProfile = (Account)Session["profile"];

            string tree = string.Empty;
            string func = string.Empty;

            long lTotal = 0;
            if (oProfile.UnitCode.Length == 6)
            {
                var listPO = DICTIONARY_SERVICE.GetPost(oProfile.UnitCode, "", 0, "", "", "", "", 0, ref lTotal);

                tree = String.Concat(
                   "<li><span data-toggle='collapse'></span> <a id='po-" + listPO[0].PosCode + "' onclick=Po.LoadTree('" + listPO[0].PosCode + "') data-toggle='collapse' class='parentKey' keyVal='"
                  , listPO[0].PosCode,
                  "' keyAddr='"
                  , "",
                  "' keyFullCode='"
                  , "" + "." + listPO[0].PosCode,
                   "' keyFullName='"
                  , listPO[0].PosName,
                  "' href='#'>"
                  , listPO[0].PosCode + '-' + listPO[0].PosName
                  , "</a><ul id='tree-" + listPO[0].PosCode + "'> </ul></li>");
            }
            else
            {
                var mee = DICTIONARY_SERVICE.GetUnit(oProfile.UnitCode,"","","",0, ref lTotal);
                tree = listUnitString(String.Empty, String.Empty, (mee[0].UnitCode ?? "").ToString(), (mee[0].UnitName ?? "").ToString(), "", oProfile.UnitCode);
            }

            ViewBag.Tree = tree;
            return PartialView();
        }

        public string listUnitString(string parentCode, string parentName, string unitCode, string unitName, string address, string account)
        {
            if (String.IsNullOrEmpty(address)) address = "";
            if (!String.IsNullOrEmpty(parentName)) parentName += " > ";
            if (!String.IsNullOrEmpty(parentCode)) parentCode += ".";
            parentCode += unitCode.Trim();
            parentName += unitName.Trim();

            dynamic[] listSub = new dynamic[] { };
            string result = "";

            result = String.Concat(
                   "<li><span data-toggle='collapse'></span> <a id='po-" + unitCode + "' onclick=Po.LoadTree('" + unitCode + "') data-toggle='collapse' class='parentKey' keyVal='"
                   , unitCode,
                   "' keyAddr='"
                   , address,
                   "' keyFullCode='"
                   , parentCode,
                    "' keyFullName='"
                   , parentName,
                   "' href='#'>"
                   , unitCode + '-' + unitName
                   , "</a><ul id='tree-" + unitCode + "'>");
            result = result + "</ul></li>";
            return result;
            //}

        }

        public JsonResult UnitString(string unitCode)
        {
            var mee = GetUnitByCode(unitCode);
            var jsonValue = Json(listUnitString(String.Empty, String.Empty, mee.UnitCode, mee.UnitName, "", unitCode), JsonRequestBehavior.AllowGet);
            jsonValue.MaxJsonLength = Int32.MaxValue;
            return jsonValue;
        }
        private dynamic GetUnitByCode(string unitCode)
        {
            dynamic mee = new PayID.DataHelper.DynamicObj();
            if (unitCode.Length == 2 || unitCode.Length == 4)
            {
                mee = PayID.Portal.Configuration.Data.Get("mbcUnit", Query.EQ("UnitCode", unitCode));
            }
            return mee;
        }
        public JsonResult GetAccount(string unitcode)
        {
            DynamicObj[] UserList = PayID.Portal.Configuration.Data.List("user", Query.EQ("unit_code", unitcode));
            ViewBag.User_List = UserList;
            return Json(UserList, JsonRequestBehavior.AllowGet);
        }

        // Danh sách bưu cục
        public JsonResult GetPosByUnitCode(string unitcode)
        {           
            dynamic[] listSub = new dynamic[] { };
            IMongoQuery _qUnitCode = Query.EQ("UnitCode", unitcode);
           // IMongoQuery _qStatus = Query.EQ("Status", true);
            List<dynamic> _list = new List<dynamic>();
            var _listSub = PayID.Portal.Configuration.Data.List("mbcPos", Query.And(_qUnitCode, _qUnitCode));
            try
            {
               if(_listSub.Length>0)
               {
                  
                   foreach (dynamic u in _listSub)
                   {
                       _list.Add(new
                       {
                           _id = u._id,
                           POSName = u.POSName,
                           Address = u.Address,
                           Task = u.Task
                       });
                   }
               
               }
               else
               {
                   _list.Add(new
                   {
                       _id = "",
                       POSName = "",
                       Address = "",
                       Task = ""
                   });
               }
               return Json(_list, JsonRequestBehavior.AllowGet);                      
            }
            catch
            {
                _list.Add(new
                   {
                       _id = "",
                       POSName = "",
                       Address = "",
                       Task = ""
                   });
               
               return Json(_list, JsonRequestBehavior.AllowGet); 
            }
           
        }

        public JsonResult SaveUserRole(string unit_code, string _id, string role, string role_description)
        {
            bool sReturn = false;
            try
            {
                dynamic d_Object = new DynamicObj();
                if (!string.IsNullOrEmpty(_id))
                {
                    d_Object._id = long.Parse(_id);
                }
                d_Object._id = Configuration.Data.GetNextSquence("user_role");
                d_Object.role_name = role;
                d_Object.role_description = role_description;
                d_Object.unit_code = unit_code;
                sReturn = Configuration.Data.Save("user_role", d_Object);
            }
            catch
            { }
            return Json(sReturn);

        }
        public JsonResult GetPostCodeInfo(string post_code)
        {
            dynamic myObj = Configuration.Data.Get("mbcPos", Query.EQ("_id", post_code));
            if(myObj == null)
                return Json(new
                {
                    _id = "",
                    POSName = "",
                    Address = "",
                    Task = "",
                    IsDispatch = false

                }, JsonRequestBehavior.AllowGet);
            else
            return Json(new
            {
                _id = myObj._id,
                POSName = myObj.POSName,
                Address = myObj.Address,
                Task = myObj.Task,
                Status = myObj.Status,
                IsDispatch = myObj.IsDispatch
        
            }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveUnitInfo(string unit_code, string _id, string name, string phone, string path, string time, string status, string note)
        {
            bool sReturn = false;
            try
            {
                dynamic d_Object = new DynamicObj();
                if (!string.IsNullOrEmpty(_id))
                {
                    d_Object._id = long.Parse(_id);
                }
                d_Object._id = Configuration.Data.GetNextSquence("unit_info");
                d_Object.unit_code = unit_code;
                d_Object.name = name;
                d_Object.path = path;
                d_Object.phone = phone;
                d_Object.time = time;
                d_Object.status = status;
                d_Object.note = note;
                sReturn = Configuration.Data.Save("unit_info", d_Object);
            }
            catch
            { }
            return Json(sReturn);

        }
        public JsonResult SavePostCode(string _id, string name, string address, string task, string is_dispatch, string is_active)
        {
            bool sReturn = false;
            try
            {
                dynamic d_Object = new DynamicObj();
                if (!string.IsNullOrEmpty(_id))
                {
                    d_Object = PayID.Portal.Areas.Metadata.Configuration.Data.Get("mbcPos", Query.EQ("_id", _id));
                    d_Object._id = _id;
                    d_Object.POSName = name;
                    d_Object.Address = address;
                    d_Object.Task = task;
                    d_Object.IsDispatch = bool.Parse(is_dispatch);
                    d_Object.Status = bool.Parse(is_active);
                    sReturn = Configuration.Data.Save("mbcPos", d_Object);
                }
                
            }
            catch
            { }
            return Json(sReturn);
        }
        public JsonResult AddNewPostCode(string code, string name, string address, string unitcode)
        {
            bool sReturn = false;
            if (String.IsNullOrEmpty(code) || String.IsNullOrEmpty(name))
                return Json(false);
            try
            {
                string unit_link = ((dynamic)Session["profile"]).unit_link;
                dynamic d_Object = new DynamicObj();
                dynamic  _Object = PayID.Portal.Areas.Metadata.Configuration.Data.Get("mbcPos", Query.EQ("_id", code));
                if (_Object == null)     
                {
                    d_Object._id = code;
                    d_Object.POSName = name;
                    d_Object.Address = address;
                    d_Object.UnitCode = unitcode;

                    d_Object.ProvinceCode = unit_link.Substring(0,2);
                    d_Object.Status = true;
                    d_Object.IsDispatch = true;
                    sReturn = Configuration.Data.Save("mbcPos", d_Object);
                }

            }
            catch
            { }
            return Json(sReturn);

        }

        public JsonResult UserLock(string user_id)
        {
            bool _isOK = false;
            try
            {

                dynamic _user = Configuration.Data.Get("user",Query.EQ("_id",user_id));
                _user.status = (_user.status  + 1) % 2;
                _isOK = Configuration.Data.Save("user",_user);
            }
            catch
            { }
            return Json(_isOK);
        }
        public JsonResult UnitInfoRemove(string p)
        {
            bool _isOK = false;
            try
            {
                _isOK = Configuration.Data.Delete("unit_info", int.Parse(p));
            }
            catch
            { }
            return Json(_isOK);
        }
        public JsonResult RoleRemove(string p)
        {
            bool _isOK = false;
            try
            {
                _isOK = Configuration.Data.Delete("user_role", int.Parse(p));
            }
            catch
            { }
            return Json(_isOK);
        }
    }
}