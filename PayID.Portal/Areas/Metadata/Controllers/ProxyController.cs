using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PayID.Portal.Areas.Metadata.Controllers
{
    public class ProxyController : Controller
    {
        // GET: Metadata/Proxy
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetProvinces(string provincecode)
        {
            dynamic[] Pro = Configuration.Data.List("mbcProvince", Query.EQ("ProvinceCode", provincecode));

            return Json(
                (from e in Pro
                 select new
                 {
                     ProvinceCode = ((dynamic)e).ProvinceCode,
                     ProvinceName = ((dynamic)e).ProvinceName.Trim(),
                     ProvinceFullName = ((dynamic)e).Description.Trim()
                 }
                ).ToArray()
                , JsonRequestBehavior.AllowGet);
        }
        public JsonResult ListDistricts(string provincecode,string districtcode)
        {
            if (string.IsNullOrEmpty(districtcode))
            {
                return Json(
                    (from e in Configuration.LIST_DISTRICTS
                     where ((dynamic)e).ProvinceCode == provincecode
                     select new
                     {
                         ProvinceCode = ((dynamic)e).ProvinceCode,
                         DistrictCode = ((dynamic)e).DistrictCode,
                         DistrictName = ((dynamic)e).DistrictName.Trim(),
                         DistrictFullName = ((dynamic)e).Description.Trim(),
                     }
                     ).ToArray(),
                    JsonRequestBehavior.AllowGet);
            } else
            {
                return Json(
                    (from e in Configuration.LIST_DISTRICTS
                     where ((dynamic)e).DistrictCode == districtcode
                     select new
                     {
                         ProvinceCode = ((dynamic)e).ProvinceCode,
                         DistrictCode = ((dynamic)e).DistrictCode,
                         DistrictName = ((dynamic)e).DistrictName.Trim(),
                         DistrictFullName = ((dynamic)e).Description.Trim(),
                     }
                     ).ToArray(),
                    JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult ListPos(string provincecode,string poscode)
        {
            if (string.IsNullOrEmpty(poscode))
            {
                dynamic[] Pos = Configuration.Data.List("mbcPos", Query.EQ("UnitCode", provincecode));
                return Json(
                    (from e in Pos
                     where ((dynamic)e).UnitCode == provincecode
                     select new
                     {
                         ProvinceCode = ((dynamic)e).ProvinceCode.Trim(),
                         PosCode = ((dynamic)e)._id,
                         PosName = ((dynamic)e).POSName.Trim(),
                         Unitcode = ((dynamic)e).UnitCode.Trim()
                     }
                     ).ToArray().OrderBy(x => x.PosName),
                    JsonRequestBehavior.AllowGet);
            }
            else
            {
                dynamic[] Poss = Configuration.Data.List("mbcPos", Query.EQ("POSCode", poscode));
                return Json(
                    (from e in Poss
                     where ((dynamic)e).POSCode == poscode
                     select new
                     {
                         ProvinceCode = ((dynamic)e).ProvinceCode.Trim(),
                         POSCode = ((dynamic)e)._id,
                         PosName = ((dynamic)e).POSName.Trim(),
                         Unitcode = ((dynamic)e).UnitCode.Trim()
                     }
                     ).ToArray().OrderBy(x => x.PosName),
                    JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult ListProvinces()
        {
            return Json(
                (from e in Configuration.LIST_PROVINCES select 
                     new {
                         ProvinceCode = ((dynamic)e).ProvinceCode,
                         ProvinceName = ((dynamic)e).ProvinceName.Trim(),
                         ProvinceFullName = ((dynamic)e).Description.Trim()
                    }
                ).ToArray()
                , JsonRequestBehavior.AllowGet);
        }

        public static string GetProvinceName(string provinceCode)
        {
            return (from e in Configuration.LIST_PROVINCES where e.ProvinceCode == provinceCode select e.ProvinceName).SingleOrDefault();
        }

        public static dynamic GetProvince(string provinceCode)
        {
            return (from e in Configuration.LIST_PROVINCES where e.ProvinceCode == provinceCode select e).SingleOrDefault();
        }
        public static string GetDistrictName(string districtCode)
        {
            return (from e in Configuration.LIST_DISTRICTS where e.DistrictCode == districtCode select e.DistrictName).SingleOrDefault();
        }
   

        public JsonResult ListWards(string districtCode)
        {
            dynamic[] wards = Configuration.Data.List("mbcCommune", Query.EQ("DistrictCode", districtCode));
            return Json(
                (from e in wards
                 select new
                 {
                     DistrictCode = districtCode,
                     WardCode = ((dynamic)e).CommuneCode,
                     WardName = ((dynamic)e).CommuneName.Trim(),
                     WardFullName = ((dynamic)e).CommuneName.Trim()
                 }
                 ).ToArray(),
                JsonRequestBehavior.AllowGet);
        }

    }
}