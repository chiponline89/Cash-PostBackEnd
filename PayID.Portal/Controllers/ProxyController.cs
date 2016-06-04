using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PayID.Portal.Controllers
{
    public class ProxyController : Controller
    {
        // GET: Proxy
        public ActionResult Index()
        {
            return View();
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

                dynamic _mee = PayID.Portal.Configuration.Data.Get("mbcPos", Query.EQ("POSCode",unitCode));
                mee.UnitCode = _mee.POSCode;
                mee.UnitName = _mee.POSName;
            }
            return Json(listUnitString(mee.UnitCode, mee.UnitName));
        }
        public string listUnitString(string unitCode, string unitName)
        {
            if (String.IsNullOrEmpty(unitCode)) return String.Empty;
            dynamic[] listSub = new dynamic[] { };
            if (unitCode.Length == 2)
            {
                listSub = PayID.Portal.Configuration.Data.List("mbcUnit", Query.EQ("ParentUnitCode", unitCode));
            }
            else if (unitCode.Length == 4)
            {
                IMongoQuery _qUnitCode = Query.EQ("UnitCode", unitCode);
                IMongoQuery _qStatus = Query.EQ("Status", true);
                
                var _listSub = PayID.Portal.Configuration.Data.List("mbcPos",Query.And(_qStatus,_qUnitCode) );
                List<dynamic> _list = new List<dynamic>();
                foreach (dynamic u in _listSub)
                {
                    dynamic _unit = new DataHelper.DynamicObj();
                    _unit.unitCode = u.POSCode;
                    _unit.unitName = u.POSName;
                    _list.Add(_unit);
                }
                listSub = _list.ToArray();
            }
            string result = "";
            if (listSub.Length == 0)
            {
                return String.Concat(
                   "<li><span ><i class='icon-leaf'></i> P</span> <a class='parentKey' keyVal='"
                   , unitCode
                   , "' href='#'>"
                   , unitName
                   , "</a></li>");
            }
            result = String.Concat(
                   "<li><span ><i class='icon-folder'></i> P</span> <a class='parentKey' keyVal='"
                   , unitCode
                   , "' href='#'>"
                   , unitName
                   , "</a><ul>");
            foreach (dynamic leaf in listSub) result = result + listUnitString(leaf.UnitCode, leaf.UnitName);
            result = result + "</ul></li>";
            return result;
        }
    }
}