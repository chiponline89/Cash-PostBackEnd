using MongoDB.Driver;
using MongoDB.Driver.Builders;
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
    public class ProxyController : Controller
    {
        DictionaryService DICTIONARY_SERVICE = null;
        AccountService ACCOUNT_SERVICE = null;
        public ProxyController()
        {
            if (DICTIONARY_SERVICE == null)
            {
                DICTIONARY_SERVICE = new DictionaryService();
            }

            if (ACCOUNT_SERVICE == null)
            {
                ACCOUNT_SERVICE = new AccountService();
            }
        }

        // GET: Systems/Proxy
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Workflow_ListNextProcessUnit(string Service, string Type)
        {
            Service = Service.ToUpper();
            Type = Type.ToUpper();

            if (Session["profile"] == null)
            {
                var Account = ACCOUNT_SERVICE.GetAccountByUserName(User.Identity.Name);
                Session["profile"] = Account;
            }

            Account oAccount = (Account)Session["profile"];


            ///How to check unit level by unit_code
            ///- Check unit_code length: 2 -> BDT
            ///---4: BDTT
            ///---6: BCU/DTH

            ///1. Get Province Code By Unit
            string Province_Unit_Code = string.Empty;
            string Unit_Level_Code = string.Empty;
            string Next_Process_Level = string.Empty;//= "EMP";
            string UnitCode = oAccount.UnitCode;

            switch (oAccount.UnitCode.Length)
            {
                case 2:
                    Province_Unit_Code = UnitCode;
                    if (UnitCode == "00")
                    {
                        Unit_Level_Code = Param.TCT;
                    }
                    else
                    {
                        Unit_Level_Code = Param.BDT;
                    }
                    //Next_Process_Level = "BDTT";
                    break;
                case 4:
                    long lTotal = 0;
                    Province_Unit_Code = DICTIONARY_SERVICE.GetUnit(UnitCode, "", "", "", 0, ref lTotal)[0].ParentUnitCode;
                    Unit_Level_Code = Param.BDTT;
                    //Next_Process_Level = "BCU";
                    break;
                case 6:
                    long Total = 0;
                    var UnitProfile = DICTIONARY_SERVICE.GetPost(UnitCode, "", 0, "", "", "", "", 0, ref Total)[0];
                    Province_Unit_Code = UnitProfile.ProvinceCode;
                    Unit_Level_Code = (UnitProfile.PosTypeCode == 1 || UnitProfile.PosTypeCode == 2) ? Param.BCU : Param.DTH;
                    Next_Process_Level = Param.EMP; //End Step
                    break;
            }

            if (Next_Process_Level != Param.EMP)
            {

                #region Get Next_Process_Level
                var _lWorkFlow = DICTIONARY_SERVICE.GetWorkFlowService(Province_Unit_Code, Service, Type);

                if (_lWorkFlow == null || _lWorkFlow.Count == 0)
                {
                    _lWorkFlow = DICTIONARY_SERVICE.GetWorkFlowService("00", Service, Type);
                }

                WorkFlowService WorkFlow = new WorkFlowService();
                WorkFlow = _lWorkFlow[0];

                if (WorkFlow != null)
                {
                    //Step: "BDTT|TTG|BDT"
                    Next_Process_Level = Param.EMP;
                    string[] arrSteps = WorkFlow.Steps.Split('|');
                    for (int i = 0; i < arrSteps.Length; i++)
                    {
                        if (arrSteps[i] == Unit_Level_Code && i < arrSteps.Length - 1)
                        {
                            Next_Process_Level = arrSteps[i + 1].Trim();
                            break;
                        }
                    }

                }
                #endregion
            }

            #region Lấy danh sách đơn vị xử lý tiếp theo
            List<NextUnit> List_Next_Unit = new List<NextUnit>();
            //Lay danh sach next_process
            if (Next_Process_Level == Param.EMP)
            {
                var ListUser = ACCOUNT_SERVICE.GetAccountByUnitLink(UnitCode);
                foreach (var _u in ListUser)
                {
                    if (_u.UserName != User.Identity.Name && _u.Role == "1" && _u.Status == "1")
                        List_Next_Unit.Add(new NextUnit{
                            Id = _u.UnitName,
                            Name = _u.FullName,
                            UnitLink = _u.UnitLink
                        });
                }
            }
            else if (Next_Process_Level == Param.BDT)
            {
                long _Total = 0;
                var Province = DICTIONARY_SERVICE.GetUnit("", "00", "", "", 0, ref _Total);
                foreach (var _u in Province)
                {
                    if (oAccount.UnitCode == "00")
                    {
                        List_Next_Unit.Add(new NextUnit{
                                Id = _u.UnitCode,
                                Name = _u.UnitName,
                                UnitLink = Province_Unit_Code + "." + _u.UnitCode
                            });
                    }
                    else
                    {
                        List_Next_Unit.Add(new NextUnit{
                                Id = _u.UnitCode,
                                Name = _u.UnitName
                            });
                    }
                }
            }
            else if (Next_Process_Level == Param.BDTT)
            {
                long __Total = 0;
                var District = DICTIONARY_SERVICE.GetUnit("", UnitCode, "", "", 0, ref __Total);
                foreach (dynamic _u in District)
                {
                    List_Next_Unit.Add(new NextUnit{
                            Id = _u.UnitCode,
                            Name = _u.UnitName,
                            UnitLink = Province_Unit_Code + "." + _u.UnitCode
                        });
                }
            }
            else if (Next_Process_Level == Param.BCU)
            {
                List<Post> ListPos = new List<Post>();
                if (UnitCode.Length == 4)
                {
                    long bcTotal = 0;
                    ListPos = DICTIONARY_SERVICE.GetPost("", "", -1, Province_Unit_Code, "", "True", UnitCode, 0, ref bcTotal, "True", "True");
                }
                else
                {
                    long bcTotal = 0;
                    ListPos = DICTIONARY_SERVICE.GetPost("", "", -1, Province_Unit_Code, "", "True", "", 0, ref bcTotal, "", "True");

                }

                foreach (var _u in ListPos)
                {
                    List_Next_Unit.Add(new NextUnit{
                            Id = _u.PosCode,
                            Name = _u.PosName,
                            UnitLink = Province_Unit_Code + "." + _u.UnitCode + "." + _u.PosCode
                        });
                }
            }
            else
            {
                List<Post> _external = new List<Post>();
                long dfTotal = 0;
                _external = DICTIONARY_SERVICE.GetPost("", "", 0, "", "", "", Next_Process_Level, 0, ref dfTotal);

                foreach (var _u in _external)
                {
                    List_Next_Unit.Add(new NextUnit{
                            Id = _u.PosCode,
                            Name = _u.PosName,
                            UnitLink = Province_Unit_Code + "." + _u.UnitCode + "." + _u.PosCode
                        });
                }
            }
            #endregion

            return PartialView(List_Next_Unit);
            //return Json(List_Next_Unit, JsonRequestBehavior.AllowGet);
        }
    }
}
