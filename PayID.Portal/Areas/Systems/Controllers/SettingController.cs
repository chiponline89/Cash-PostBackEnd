using MongoDB.Driver.Builders;
using PayID.DataHelper;
using PayID.Portal.Common;
using PayID.Portal.Common.Service;
using PayID.Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PayID.Portal.Areas.Systems.Controllers
{
    public class SettingController : Controller
    {

        DictionaryService DICTIONARY_SERVICE = null;
        AccountService ACCOUNT_SERVICE = null;

        public SettingController()
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

        // GET: Systems/Setting
        public ActionResult Index()
        {
            if (Session["profile"] == null)
            {
                dynamic profile = Configuration.Data.Get("user", Query.EQ("_id", User.Identity.Name));
                Session["profile"] = profile;
            }
            return View();
        }
        public ActionResult WorkflowUnit()
        {
            if (Session["profile"] == null) return Redirect("/");
            string unit_code = ((dynamic)Session["profile"]).unit_code;
            dynamic model = Configuration.Data.Get("mbcUnit", Query.EQ("UnitCode", unit_code));
            model.workflow_unit_levels = Configuration.Data.List("wf_unit_level", Query.EQ("unit_code", unit_code));
            return View(model);
        }

        public JsonResult ListDistrictUnit()
        {
            dynamic[] _list = Configuration.Data.List("mbcUnit", Query.EQ("ParentUnitCode", ((dynamic)Session["profile"]).unit_code.ToString()));
            var _listReturn = (from e in _list
                               select new
                               {
                                   UnitCode = e.UnitCode,
                                   UnitName = e.UnitName
                               }
                ).ToArray();
            return Json(_listReturn, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ListPOUnit(string unit_code)
        {
            dynamic[] _list = Configuration.Data.List("mbcPos",
                        Query.And(
                        Query.EQ("UnitCode", unit_code),
                        Query.EQ("IsDispatch", true))
                        );
            var _listReturn = (from e in _list
                               select new
                               {
                                   UnitCode = e.POSCode,
                                   UnitName = e.POSName
                               }
                ).ToArray();
            return Json(_listReturn, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ListUnit(string level)
        {
            dynamic[] list_unit = new dynamic[] { };
            string unit_link = ((dynamic)Session["profile"]).unit_link;
            switch (level)
            {
                case "TCT":
                    list_unit = Configuration.Data.List("mbcUnit",
                        Query.And(
                        Query.EQ("ParentUnitCode", ((dynamic)Session["profile"]).unit_code),
                        Query.NE("IsWorkflowUnit", true))
                        );
                    break;
                case "BDTT":
                    list_unit = Configuration.Data.List("mbcUnit",
                        Query.And(
                        Query.EQ("ParentUnitCode", ((dynamic)Session["profile"]).unit_code),
                        Query.NE("IsWorkflowUnit", true))
                        );
                    break;
                case "BCU":
                    List<dynamic> _list = new List<dynamic>();
                    dynamic[] _list_po = Configuration.Data.List("mbcPos",
                        Query.And(
                        Query.EQ("ProvinceCode", ((dynamic)Session["profile"]).unit_code),
                        Query.EQ("IsDispatch", true),
                        Query.NE("IsWorkflowUnit", true))
                        );
                    foreach (dynamic po in _list_po)
                    {
                        dynamic newPO = new PayID.DataHelper.DynamicObj();
                        newPO.UnitCode = po._id;
                        newPO.UnitName = po.POSName;
                        _list.Add(newPO);
                    }
                    list_unit = _list.ToArray();
                    break;
                default:
                    List<dynamic> _listOther = new List<dynamic>();
                    dynamic[] _list_other = Configuration.Data.List("mbcPos",
                        Query.And(
                        Query.EQ("UnitCode", level),
                        Query.EQ("ProvinceCode", unit_link.Substring(0, 2)),
                        Query.EQ("IsDispatch", true),
                        Query.EQ("IsWorkflowUnit", true))
                        );
                    foreach (dynamic po in _list_other)
                    {
                        dynamic newPO = new PayID.DataHelper.DynamicObj();
                        newPO.UnitCode = po._id;
                        newPO.UnitName = po.POSName;
                        _listOther.Add(newPO);
                    }
                    list_unit = _listOther.ToArray();
                    break;
            }
            List<dynamic> _returnList = new List<dynamic>();
            foreach (dynamic dy in list_unit)
            {
                _returnList.Add(
                    new
                    {
                        UnitCode = dy.UnitCode,
                        UnitName = dy.UnitName
                    }
                    );
            }
            return Json(_returnList, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Workflow()
        {
            if (Session["profile"] == null)
            {
                return RedirectToAction("LogOut", "Home", new { Area = "" });
            }
            else
            {
                Models.Account oAccount = (Models.Account)Session["profile"];

                if(oAccount.UnitCode == "00")
                {
                    List<SelectListItem> ListUnit = new List<SelectListItem>();

                    ListUnit.Add(new SelectListItem
                    {
                        Text = "---Áp dụng toàn quốc---",
                        Value = "00"
                    });

                    long lTotal = 0;
                    var Units = DICTIONARY_SERVICE.GetUnit("", "00", "", "", 0, ref lTotal);

                    if(Units != null && Units.Count > 0)
                    {
                        foreach(var item in Units)
                        {
                            ListUnit.Add(new SelectListItem
                            {
                                Text = item.UnitName,
                                Value = item.UnitCode
                            });
                        }
                    }

                    ViewBag.ListUnit = ListUnit;
                }
                
                return View();
            }

            //if (Session["profile"] == null) return Redirect("/");
            //string unit_code = ((dynamic)Session["profile"]).unit_code;
            //ViewBag.external_unit = Configuration.Data.List("wf_unit_level", Query.EQ("unit_code", unit_code));
            //if (unit_code == "00")
            //    return View(Configuration.Data.List("mbcUnit", Query.EQ("ParentUnitCode", unit_code)));
            //else
            //    return View(Configuration.Data.List("mbcUnit", Query.EQ("UnitCode", unit_code)));
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ListWorkflow(string unitCode)
        {
            if (Session["profile"] == null)
            {
                var profile = ACCOUNT_SERVICE.GetAccountByUserName(User.Identity.Name);
                Session["profile"] = profile;
            }

            Account oAccount = (Account)Session["profile"];

            if(oAccount.UnitCode != "00")
            {
                unitCode = oAccount.UnitCode;
            }
            
            var ListWorkFlow = DICTIONARY_SERVICE.GetWorkFlowService(unitCode,"","");

            return PartialView(ListWorkFlow);
        }

        //[ValidateAntiForgeryToken]
        [ValidateJsonAntiForgeryTokenAttribute]
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult UpdateWorkFlowService(string UnitCode, string RequestService, string RequestType, string Steps, string StepsName)
        {
            JsonResult jResult = new JsonResult();
            if (Session["profile"] == null)
            {
                jResult = Json(new { Code = "-101", Mes = "Đăng nhập trước khi tiếp tục thực hiện thao tác." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                Account oAccount = (Account)Session["profile"];

                WorkFlowService oWorkFlowService = new WorkFlowService();

                if (oAccount.UnitCode != "00")
                {
                    oWorkFlowService.UnitCode = oAccount.UnitCode;
                }
                else
                {
                    oWorkFlowService.UnitCode = UnitCode;
                }

                oWorkFlowService.RequestService = RequestService;
                oWorkFlowService.RequestType = RequestType;
                oWorkFlowService.Steps = Steps;
                oWorkFlowService.StepName = StepsName;

                string sResult = DICTIONARY_SERVICE.UpdateWorkFlowService(oWorkFlowService);

                string Code = sResult.Split('|')[0].ToString();
                string Mes = sResult.Split('|')[1].ToString();

                jResult = Json(new { Code = Code, Mes = Mes }, JsonRequestBehavior.AllowGet);
            }

            return jResult;
        }

        public JsonResult SaveUnit(string code, string name, string parent)
        {
            string unit_code = ((dynamic)Session["profile"]).unit_code;

            dynamic unit = new PayID.DataHelper.DynamicObj();
            unit._id = code;
            unit.POSCode = code;
            unit.POSName = name;
            unit.UnitCode = parent;
            unit.ProvinceCode = unit_code;
            unit.IsDispatch = true;
            unit.Status = true;
            unit.IsWorkflowUnit = true;
            bool isOK = Configuration.Data.Save("mbcPos", unit);
            if (isOK)
                return Json(new { response_code = "00", response_message = "Cập nhật đơn vị thành công" }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { response_code = "01", response_message = "Cập nhật đơn vị không thành công" }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult RemoveUnit(string code)
        {
            string unit_code = ((dynamic)Session["profile"]).unit_code;

            bool isOK = Configuration.Data.Delete("mbcPos", code);
            if (isOK)
                return Json(new { response_code = "00", response_message = "Xóa đơn vị thành công" }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { response_code = "01", response_message = "Xóa đơn vị không thành công" }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveUnitLevel(string code, string name, string parent)
        {
            string unit_code = ((dynamic)Session["profile"]).unit_code;
            dynamic unitLevel = new PayID.DataHelper.DynamicObj();
            unitLevel.code = code;
            unitLevel.name = name;
            unitLevel.parent = parent;
            unitLevel.unit_code = unit_code;
            unitLevel._id = unit_code + "." + code;
            bool isOK = Configuration.Data.Save("wf_unit_level", unitLevel);

            dynamic unit = new PayID.DataHelper.DynamicObj();
            unit.UnitCode = code;
            unit.UnitName = name;
            unit.ParentUnitCode = unit_code;
            unit._id = unit_code + "." + code;
            isOK = isOK & Configuration.Data.Save("mbcUnit", unit);
            if (isOK)
                return Json(new { response_code = "00", response_message = "Cập nhật cấp đơn vị thành công" }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { response_code = "01", response_message = "Cập nhật cấp đơn vị không thành công" }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult RemoveUnitLevel(string code)
        {
            string unit_code = ((dynamic)Session["profile"]).unit_code;
            string _id = unit_code + "." + code;
            bool isOK = Configuration.Data.Delete("wf_unit_level", _id);
            isOK = isOK && Configuration.Data.Delete("mbcUnit", _id);
            if (isOK)
                return Json(new { response_code = "00", response_message = "Xóa thành công" }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { response_code = "01", response_message = "Xóa không thành công" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveWF(
            string unit_code,
            string request_service,
            string request_type,
            string wf,
            string wf_titles
            )
        {
            bool isOK = false;
            dynamic workflow = new PayID.DataHelper.DynamicObj();
            if (unit_code != "00")
            {
                workflow._id = String.Join("_", request_service.ToLower(), request_type.ToLower(), unit_code);
                workflow.unit_code = unit_code;
                workflow.request_service = request_service;
                workflow.request_type = request_type;
                workflow.steps = wf.Replace(",", "|");
                workflow.step_names = wf_titles.Replace(",", " -> ");
                isOK = Configuration.Data.Save("workflow_service", workflow);
            }
            else
            {
                workflow._id = String.Join("_", request_service.ToLower(), request_type.ToLower(), unit_code);
                workflow.unit_code = unit_code;
                workflow.request_service = request_service;
                workflow.request_type = request_type;
                workflow.steps = wf.Replace(",", "|");
                workflow.step_names = wf_titles.Replace(",", " -> ");
                isOK = Configuration.Data.Save("workflow_service", workflow);

                DynamicObj[] lstUnit = Configuration.Data.List("mbcUnit", Query.EQ("ParentUnitCode", unit_code));
                if(lstUnit!=null && lstUnit.Length>0)
                {
                    foreach(dynamic ite in lstUnit.ToList<dynamic>())
                    {
                        workflow = new PayID.DataHelper.DynamicObj();
                        workflow._id = String.Join("_", request_service.ToLower(), request_type.ToLower(), ite.UnitCode);
                        workflow.unit_code = ite.UnitCode;
                        workflow.request_service = request_service;
                        workflow.request_type = request_type;
                        workflow.steps = wf.Replace(",", "|");
                        workflow.step_names = wf_titles.Replace(",", " -> ");
                        Configuration.Data.Save("workflow_service", workflow);
                    }
                }

            }
            if (isOK)
                return Json(new { response_code = "00", response_message = "Cập nhật Luồng xử lý thành công" }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { response_code = "01", response_message = "Cập nhật Luồng xử lý không thành công" }, JsonRequestBehavior.AllowGet);
        }
    }
}