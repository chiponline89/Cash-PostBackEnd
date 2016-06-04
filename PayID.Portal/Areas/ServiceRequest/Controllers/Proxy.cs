using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Newtonsoft.Json.Linq;
using PayID.DataHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PayID.Portal.Areas.ServiceRequest.Controllers
{
    public class ProxyController : Controller
    {
        public JsonResult ListStore(string user_id)
        {
            dynamic[] lstStore = PayID.Portal.Areas.Merchant.Configuration.Data.List("profile_store", Query.EQ("UserId", user_id));
            List<dynamic> list = new List<dynamic>();
            foreach (dynamic s in lstStore)
            {
                list.Add(new 
                    {
                     _id= s._id,
                     StoreCode = s.StoreCode,
                     StoreName = s.StoreName,
                     Address = s.Address,
                     ManagerName = s.ManagerName,
                     ManagerMobile = s.ManagerMobile,
                     District = s.DistrictCode,
                     PostCode = s.PostCode,
                     DistrictName = PayID.Portal.Areas.Metadata.Controllers.ProxyController.GetDistrictName(s.DistrictCode),
                     Province = s.ProvinceCode,
                     ProvinceName = PayID.Portal.Areas.Metadata.Controllers.ProxyController.GetProvinceName(s.ProvinceCode)
                    }
                    );
            }
            return Json(list.ToArray(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult UnitString(string unitCode)
        {
            dynamic mee = new PayID.DataHelper.DynamicObj();
            if (unitCode.Length == 2 || unitCode.Length == 4)
            {
                mee = PayID.Portal.Configuration.Data.Get("mbcUnit", Query.EQ("UnitCode", unitCode));
            }
            else
            {
                dynamic _mee = PayID.Portal.Configuration.Data.Get("mbcPos", Query.EQ("POSCode", unitCode));
                mee.UnitCode = _mee.POSCode;
                mee.UnitName = _mee.POSName;
            }
            return Json(listUnitString("", "", mee.UnitCode, mee.UnitName), JsonRequestBehavior.AllowGet);
        }
        public string listUnitString(string parentCode, string parentName, string unitCode, string unitName)
        {
            if (!String.IsNullOrEmpty(parentName)) parentName += " > ";
            if (!String.IsNullOrEmpty(parentCode)) parentCode += ".";

            dynamic[] listSub = new dynamic[] { };
            parentCode += unitCode;
            parentName += unitName;
            if (unitCode.Length == 2)
            {
                listSub = PayID.Portal.Configuration.Data.List("mbcUnit", Query.EQ("ParentUnitCode", unitCode));
            }
            else if (unitCode.Length == 4)
            {
                IMongoQuery _qUnitCode = Query.EQ("UnitCode", unitCode);
                IMongoQuery _qStatus = Query.EQ("Status", true);
                IMongoQuery _qBCU = Query.LTE("POSTypeCode", 2);

                var _listSub = PayID.Portal.Configuration.Data.List("mbcPos", Query.And(_qStatus, _qUnitCode, _qBCU));
                List<dynamic> _list = new List<dynamic>();
                foreach (dynamic u in _listSub)
                {
                    dynamic _unit = new DataHelper.DynamicObj();
                    _unit.UnitCode = u.POSCode;
                    _unit.UnitName = u.POSName;
                    _list.Add(_unit);
                }
                listSub = _list.ToArray();
            }
            string result = "";
            if (listSub.Length == 0)
            {
                return String.Concat(
                   "<li><a class='parentKey' keyVal='"
                   , parentCode
                   , "' keyText = '" + parentName + "' unitText = '" + unitName + "' href='#'>"
                   , unitName
                   , "</a></li>");
            }
            result = String.Concat(
                   "<li class='dropdown-submenu'><a class='parentKey' keyVal='"
                   , parentCode
                   , "' keyText = '" + parentName + "' unitText = '" + unitName + "' href='#'>"
                   , unitName
                   , "</a><ul class='dropdown-menu'>");
            foreach (dynamic leaf in listSub) result = result + listUnitString(parentCode, parentName, leaf.UnitCode, leaf.UnitName);
            result = result + "</ul></li>";

            return result;
        }
    }
}