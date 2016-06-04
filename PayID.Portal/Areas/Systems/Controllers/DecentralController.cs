using MongoDB.Driver;
using MongoDB.Driver.Builders;
using PayID.DataHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PayID.Portal.Areas.Systems.Controllers
{
    public class DecentralController : Controller
    {
        //
        // GET: /Systems/Decentral/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetFunctionList(string unit_code)
        {
            DynamicObj[] UserList = PayID.Portal.Configuration.Data.List("FunctionCategory", Query.NE("_id", 0));
            dynamic func = new DynamicObj();
            if (!string.IsNullOrEmpty(unit_code))
            {
                if (((dynamic)Session["profile"]).unit_code == "00")
                {
                    func = PayID.Portal.Configuration.Data.Get("GeneralFuntionRole", Query.EQ("unit_code", unit_code));
                }
                else
                {
                    func = PayID.Portal.Configuration.Data.Get("ProvinceUnitRole", Query.And(Query.EQ("unit_code", ((dynamic)Session["profile"]).unit_code.ToString()), Query.EQ("unit_type", unit_code)));
                    if (func == null)
                    {
                        if (unit_code == "4")
                        {
                            func = PayID.Portal.Configuration.Data.Get("GeneralFuntionRole", Query.EQ("unit_code", "4"));
                        }
                        else
                        {
                            func = PayID.Portal.Configuration.Data.Get("GeneralFuntionRole", Query.EQ("unit_code", "6"));
                        }
                    }
                }
            }

            List<dynamic> listdetail = new List<dynamic>();
            List<dynamic> _detail = new List<dynamic>();
            DynamicObj[] lstFuncDetail = null;
            dynamic itemFunc = new DynamicObj();
            dynamic itemFuncDetail = new DynamicObj();
            string[] arrStr = null;
            string[] arrStrCate = null;
            foreach (dynamic ite in UserList)
            {
                itemFunc = new DynamicObj();
                itemFunc._id = ite._id;
                itemFunc.ModuleName = ite.ModuleName;
                itemFunc.ModuleCode = ite.ModuleCode;
                lstFuncDetail = PayID.Portal.Configuration.Data.List("FunctionDetail", Query.EQ("Module", ite.ModuleCode.ToString()));

                if (func == null)
                {
                    if (lstFuncDetail != null && lstFuncDetail.Length > 0)
                    {
                        itemFunc.detail = lstFuncDetail.ToList();
                    }
                }
                else
                {
                    //int i = 0;
                    if (lstFuncDetail != null && lstFuncDetail.Length > 0)
                    {
                        foreach (dynamic dyna in lstFuncDetail)
                        {
                            if (func.function != null)
                            {
                                if (func.module.ToString().Contains("|"))
                                {
                                    arrStr = func.function.ToString().Split('|');
                                }
                                else
                                {
                                    arrStr[0] = func.function.ToString();
                                }

                                foreach (string strValue in arrStr)
                                {
                                    if (strValue == dyna._id.ToString())
                                    {
                                        //i++;
                                        dyna.IsChecked = true;
                                    }
                                }
                            }

                        }
                    }

                    if (func.module != null)
                    {
                        if (func.module.ToString().Contains("|"))
                        {
                            arrStrCate = func.module.ToString().Split('|');
                        }
                        else
                        {
                            arrStrCate[0] = func.module.ToString();
                        }

                        foreach (string strCate in arrStrCate)
                        {
                            if (strCate == ite._id.ToString())
                            {
                                itemFunc.IsChecked = true;
                            }
                        }
                    }

                    itemFunc.detail = lstFuncDetail.ToList();
                }

                if (((dynamic)Session["profile"]).unit_code == "00")
                {
                    listdetail.Add(itemFunc);
                }
                else
                {
                    if (itemFunc.IsChecked == true)
                    {
                        listdetail.Add(itemFunc);
                    }
                }
            }

            return View(listdetail);
        }
        public ActionResult GetUserFunctionList(string unit_code, string unit_type)
        {
            DynamicObj[] UserList = PayID.Portal.Configuration.Data.List("FunctionCategory", Query.NE("_id", 0));
            dynamic func = new DynamicObj();
            if (((dynamic)Session["profile"]).unit_code == "00")
            {
                if (!string.IsNullOrEmpty(unit_code))
                {
                    func = PayID.Portal.Configuration.Data.Get("GeneralUnitUserRole", Query.And(Query.EQ("role_code", unit_code), Query.EQ("unit_type", unit_type)));
                    if (func == null)
                    {
                        func = PayID.Portal.Configuration.Data.Get("GeneralFuntionRole", Query.EQ("unit_code", unit_type));
                    }
                }
                //else
                //{
                //    func = PayID.Portal.Configuration.Data.Get("UserGroup", Query.NE("_id", 0));
                //}
            }
            else
            {
                func = PayID.Portal.Configuration.Data.Get("ProvinceUserRole", Query.And(Query.EQ("role_code", unit_code), Query.EQ("provincecode", ((dynamic)Session["profile"]).unit_code.ToString())));
                if (func == null)
                {
                    func = PayID.Portal.Configuration.Data.Get("ProvinceUnitRole", Query.And(Query.EQ("unit_code", ((dynamic)Session["profile"]).unit_code.ToString()), Query.EQ("unit_type", unit_type)));
                }
            }
            List<dynamic> listdetail = new List<dynamic>();
            List<dynamic> _detail = new List<dynamic>();
            DynamicObj[] lstFuncDetail = null;
            dynamic itemFunc = new DynamicObj();
            dynamic itemFuncDetail = new DynamicObj();
            string[] arrStr = null;
            string[] arrStrCate = null;

            foreach (dynamic ite in UserList)
            {
                itemFunc = new DynamicObj();
                itemFunc._id = ite._id;
                itemFunc.ModuleName = ite.ModuleName;
                itemFunc.ModuleCode = ite.ModuleCode;
                lstFuncDetail = PayID.Portal.Configuration.Data.List("FunctionDetail", Query.EQ("Module", ite.ModuleCode.ToString()));

                if (func == null)
                {
                    if (lstFuncDetail != null && lstFuncDetail.Length > 0)
                    {
                        itemFunc.detail = lstFuncDetail.ToList();
                    }
                }
                else
                {
                    //int i = 0;
                    if (lstFuncDetail != null && lstFuncDetail.Length > 0)
                    {
                        foreach (dynamic dyna in lstFuncDetail)
                        {
                            if (func.function != null)
                            {
                                if (func.module.ToString().Contains("|"))
                                {
                                    arrStr = func.function.ToString().Split('|');
                                }
                                else
                                {
                                    arrStr[0] = func.function.ToString();
                                }

                                foreach (string strValue in arrStr)
                                {
                                    if (strValue == dyna._id.ToString())
                                    {
                                        //i++;
                                        dyna.IsChecked = true;
                                    }
                                }
                            }

                        }
                    }

                    if (func.module != null)
                    {
                        if (func.module.ToString().Contains("|"))
                        {
                            arrStrCate = func.module.ToString().Split('|');
                        }
                        else
                        {
                            arrStrCate[0] = func.module.ToString();
                        }

                        foreach (string strCate in arrStrCate)
                        {
                            if (strCate == ite._id.ToString())
                            {
                                itemFunc.IsChecked = true;
                            }
                        }
                    }

                    itemFunc.detail = lstFuncDetail.ToList();
                }

                listdetail.Add(itemFunc);
            }

            return View(listdetail);
        }
        public JsonResult SaveUserRole(string unit_code, string id, string cate, string unit_type)
        {
            bool sReturn = false;
            try
            {
                dynamic func = new DynamicObj();
                if (!string.IsNullOrEmpty(unit_code))
                {
                    if (((dynamic)Session["profile"]).unit_code == "00")
                    {
                        func = PayID.Portal.Configuration.Data.Get("GeneralUnitUserRole", Query.And(Query.EQ("role_code", unit_code), Query.EQ("unit_type", unit_type)));
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(unit_type))
                        {
                            func = PayID.Portal.Configuration.Data.Get("ProvinceUserRole", Query.And(Query.EQ("role_code", unit_code), Query.EQ("unit_type", unit_type), Query.EQ("provincecode", ((dynamic)Session["profile"]).unit_code.ToString())));
                        }
                        else
                        {
                            return Json(new { response_code = "02", response_message = "Chưa chọn cấp đơn vị!" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                else
                {
                    return Json(new { response_code = "02", response_message = "Chưa chọn nhóm người dùng!" }, JsonRequestBehavior.AllowGet);
                }

                if (!string.IsNullOrEmpty(unit_code))
                {
                    if (!string.IsNullOrEmpty(id))
                    {
                        if (((dynamic)Session["profile"]).unit_code == "00")
                        {
                            if (func != null)
                            {
                                func.function = id.Substring(0, id.Length - 1);
                                func.module = cate.Substring(0, cate.Length - 1);
                                sReturn = Configuration.Data.Save("GeneralUnitUserRole", func);
                            }
                            else
                            {
                                func = new DynamicObj();
                                func._id = Configuration.Data.GetNextSquence("GeneralUnitUserRole");
                                switch (unit_code)
                                {
                                    case "1":
                                        func.role_name = "Admin";
                                        func.role_code = "1";
                                        break;
                                    case "2":
                                        func.role_name = "Nghiệp vụ";
                                        func.role_code = "2";
                                        break;
                                    case "3":
                                        func.role_name = "Kế toán - Đối soát";
                                        func.role_code = "3";
                                        break;
                                    case "4":
                                        func.role_name = "Chăm sóc khách hàng";
                                        func.role_code = "4";
                                        break;
                                    case "5":
                                        func.role_name = "Giao dịch viên";
                                        func.role_code = "5";
                                        break;
                                    case "6":
                                        func.role_name = "Bưu tá";
                                        func.role_code = "6";
                                        break;
                                }
                                func.unit_type = unit_type;
                                func.function = id.Substring(0, id.Length - 1);
                                func.module = cate.Substring(0, cate.Length - 1);
                                sReturn = Configuration.Data.Save("GeneralUnitUserRole", func);
                            }
                        }
                        else
                        {
                            if (func != null)
                            {
                                func.function = id.Substring(0, id.Length - 1);
                                func.module = cate.Substring(0, cate.Length - 1);
                                sReturn = Configuration.Data.Save("ProvinceUserRole", func);
                            }
                            else
                            {
                                func = new DynamicObj();
                                func._id = Configuration.Data.GetNextSquence("ProvinceUserRole");
                                switch (unit_code)
                                {
                                    case "1":
                                        func.role_name = "Admin";
                                        func.role_code = "1";
                                        break;
                                    case "2":
                                        func.role_name = "Nghiệp vụ";
                                        func.role_code = "2";
                                        break;
                                    case "3":
                                        func.role_name = "Kế toán - Đối soát";
                                        func.role_code = "3";
                                        break;
                                    case "4":
                                        func.role_name = "Chăm sóc khách hàng";
                                        func.role_code = "4";
                                        break;
                                    case "5":
                                        func.role_name = "Giao dịch viên";
                                        func.role_code = "5";
                                        break;
                                    case "6":
                                        func.role_name = "Bưu tá";
                                        func.role_code = "6";
                                        break;
                                }
                                func.provincecode = ((dynamic)Session["profile"]).unit_code.ToString();
                                func.unit_type = unit_type;
                                func.function = id.Substring(0, id.Length - 1);
                                func.module = cate.Substring(0, cate.Length - 1);
                                sReturn = Configuration.Data.Save("ProvinceUserRole", func);
                            }
                        }
                        return Json(new { response_code = "00", response_message = "Thông tin phân quyền đã lưu thành công!" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { response_code = "01", response_message = "Chưa chọn chức năng để phân quyền!" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { response_code = "02", response_message = "Chưa chọn nhóm người dùng!" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {
                return Json(new { response_code = "03", response_message = "Có lỗi xảy ra trong quá trình xử lý dữ liệu!" }, JsonRequestBehavior.AllowGet);
            }

        }
        public JsonResult SaveUnitInfo(string unit_code, string id, string cate)
        {
            bool sReturn = false;
            try
            {
                dynamic func = new DynamicObj();
                if (((dynamic)Session["profile"]).unit_code == "00")
                {
                    if (!string.IsNullOrEmpty(unit_code))
                    {
                        func = PayID.Portal.Configuration.Data.Get("GeneralFuntionRole", Query.EQ("unit_code", unit_code));
                    }
                }
                else
                {
                    if (((dynamic)Session["profile"]).unit_code.ToString().Length == 2)
                    {
                        func = PayID.Portal.Configuration.Data.Get("ProvinceUnitRole", Query.And(Query.EQ("unit_code", ((dynamic)Session["profile"]).unit_code.ToString()), Query.EQ("unit_type", unit_code)));
                    }
                }

                dynamic d_Object = new DynamicObj();
                if (!string.IsNullOrEmpty(unit_code))
                {
                    if (!string.IsNullOrEmpty(id))
                    {
                        if (unit_code == "1" || unit_code == "2")
                        {
                            if (func == null)
                            {
                                d_Object._id = Configuration.Data.GetNextSquence("GeneralFuntionRole");
                                d_Object.unit_code = unit_code;
                                d_Object.function = id.Substring(0, id.Length - 1);
                                d_Object.module = cate.Substring(0, cate.Length - 1);
                                sReturn = Configuration.Data.Save("GeneralFuntionRole", d_Object);
                            }
                            else
                            {
                                func.function = id.Substring(0, id.Length - 1);
                                func.module = cate.Substring(0, cate.Length - 1);
                                sReturn = Configuration.Data.Save("GeneralFuntionRole", func);
                            }

                            return Json(new { response_code = "00", response_message = "Thông tin phân quyền đã lưu thành công!" }, JsonRequestBehavior.AllowGet);
                        }
                        else if (unit_code == "4")
                        {
                            if (((dynamic)Session["profile"]).unit_code == "00")
                            {
                                if (func == null)
                                {
                                    d_Object._id = Configuration.Data.GetNextSquence("GeneralFuntionRole");
                                    d_Object.unit_code = unit_code;
                                    d_Object.function = id;
                                    d_Object.module = cate.Substring(0, cate.Length - 1);
                                    sReturn = Configuration.Data.Save("GeneralFuntionRole", d_Object);
                                }
                                else
                                {
                                    func.function = id;
                                    func.module = cate.Substring(0, cate.Length - 1);
                                    sReturn = Configuration.Data.Save("GeneralFuntionRole", func);
                                }
                            }
                            else
                            {
                                if (func == null)
                                {
                                    d_Object._id = Configuration.Data.GetNextSquence("ProvinceUnitRole");
                                    d_Object.unit_code = ((dynamic)Session["profile"]).unit_code.ToString();
                                    d_Object.unit_type = unit_code;
                                    d_Object.function = id;
                                    d_Object.module = cate.Substring(0, cate.Length - 1);
                                    sReturn = Configuration.Data.Save("ProvinceUnitRole", d_Object);
                                }
                                else
                                {
                                    func.function = id;
                                    func.module = cate.Substring(0, cate.Length - 1);
                                    sReturn = Configuration.Data.Save("ProvinceUnitRole", func);
                                }
                            }

                            return Json(new { response_code = "00", response_message = "Thông tin phân quyền đã lưu thành công!" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            if (((dynamic)Session["profile"]).unit_code == "00")
                            {
                                if (func == null)
                                {
                                    d_Object._id = Configuration.Data.GetNextSquence("GeneralFuntionRole");
                                    d_Object.unit_code = unit_code;
                                    d_Object.function = id;
                                    d_Object.module = cate.Substring(0, cate.Length - 1);
                                    sReturn = Configuration.Data.Save("GeneralFuntionRole", d_Object);
                                }
                                else
                                {
                                    func.function = id;
                                    func.module = cate.Substring(0, cate.Length - 1);
                                    sReturn = Configuration.Data.Save("GeneralFuntionRole", func);
                                }
                            }
                            else
                            {
                                if (func == null)
                                {
                                    d_Object._id = Configuration.Data.GetNextSquence("ProvinceUnitRole");
                                    d_Object.unit_code = ((dynamic)Session["profile"]).unit_code.ToString();
                                    d_Object.unit_type = unit_code;
                                    d_Object.function = id;
                                    d_Object.module = cate.Substring(0, cate.Length - 1);
                                    sReturn = Configuration.Data.Save("ProvinceUnitRole", d_Object);
                                }
                                else
                                {
                                    func.function = id;
                                    func.module = cate.Substring(0, cate.Length - 1);
                                    sReturn = Configuration.Data.Save("ProvinceUnitRole", func);
                                }
                            }
                            return Json(new { response_code = "00", response_message = "Thông tin phân quyền đã lưu thành công!" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(new { response_code = "01", response_message = "Chưa chọn chức năng để phân quyền!" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { response_code = "02", response_message = "Chưa chọn cấp đơn vị!" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {
                return Json(new { response_code = "03", response_message = "Có lỗi xảy ra trong quá trình xử lý dữ liệu!" }, JsonRequestBehavior.AllowGet);
            }
            //return Json(new { response_code = "04", response_message = "Cập nhật thông tin phân quyền chưa thành công!" }, JsonRequestBehavior.AllowGet);

        }
        public JsonResult GetPosByUnitCode(string unit_code)
        {
            dynamic[] listSub = new dynamic[] { };
            IMongoQuery _qUnitCode = Query.NE("UnitTypeCode", 0);//Query.Or(Query.EQ("UnitTypeCode", 1), Query.EQ("UnitTypeCode", 2), Query.EQ("UnitTypeCode", 4), Query.EQ("UnitTypeCode", 6));
            // IMongoQuery _qStatus = Query.EQ("Status", true);
            List<dynamic> _list = new List<dynamic>();

            if (!string.IsNullOrEmpty(((dynamic)Session["profile"]).unit_code.ToString()))
            {
                if (((dynamic)Session["profile"]).unit_code == "00")
                {
                    _qUnitCode = Query.And(_qUnitCode, Query.Or(Query.EQ("UnitTypeCode", 1), Query.EQ("UnitTypeCode", 2), Query.EQ("UnitTypeCode", 4), Query.EQ("UnitTypeCode", 6)));
                }
                else
                {
                    if (((dynamic)Session["profile"]).unit_code.ToString().Length == 2)
                    {
                        _qUnitCode = Query.And(_qUnitCode, Query.Or(Query.EQ("UnitTypeCode", 4), Query.EQ("UnitTypeCode", 6)));
                    }
                }
            }
            var _listSub = PayID.Portal.Configuration.Data.List("mbcUnitType", _qUnitCode);
            try
            {
                if (_listSub.Length > 0)
                {
                    foreach (dynamic u in _listSub)
                    {
                        _list.Add(new
                        {
                            _id = u.UnitTypeCode,
                            POSName = u.Description
                        });
                    }

                }
                else
                {
                    _list.Add(new
                    {
                        _id = "",
                        POSName = ""
                    });
                }
                return Json(_list, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                _list.Add(new
                {
                    _id = "",
                    POSName = ""
                });

                return Json(_list, JsonRequestBehavior.AllowGet);
            }

        }
        public JsonResult GetUserGroup(string unit_type)
        {
            dynamic[] listSub = new dynamic[] { };
            IMongoQuery _qUnitCode = Query.NE("_id", 0);

            if (!string.IsNullOrEmpty(unit_type))
            {
                if (((dynamic)Session["profile"]).unit_code == "00")
                {
                    if (unit_type == "1")
                    {
                        _qUnitCode = Query.And(_qUnitCode, Query.Or(Query.EQ("role_code", "2"), Query.EQ("role_code", "3"), Query.EQ("role_code", "4")));
                    }
                }               
            }

            List<dynamic> _list = new List<dynamic>();
            var _listSub = PayID.Portal.Configuration.Data.List("UserGroup", _qUnitCode);
            try
            {
                if (_listSub.Length > 0)
                {
                    foreach (dynamic u in _listSub)
                    {
                        _list.Add(new
                        {
                            _id = u._id,
                            POSName = u.role_name
                        });
                    }

                }
                else
                {
                    _list.Add(new
                    {
                        _id = "",
                        POSName = ""
                    });
                }
                return Json(_list, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                _list.Add(new
                {
                    _id = "",
                    POSName = ""
                });

                return Json(_list, JsonRequestBehavior.AllowGet);
            }

        }
    }
}