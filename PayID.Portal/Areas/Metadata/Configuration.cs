using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;
using MongoDB.Driver.Builders;
using MongoDB.Driver;
using System.Web.Mvc;
using PayID.DataHelper;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime;
namespace PayID.Portal.Areas.Metadata
{
    public class Configuration
    {
        public static PayID.DataHelper.MongoHelper Data;
        public static dynamic[] LIST_PROVINCES, LIST_DISTRICTS;
        public static List<dynamic> LIST_PROVINCE, LIST_DISTRICT,  LIST_WARD, LIST_BUSINESSPROFILE, LIST_STATUS, LIST_POS;
        public static List<SelectListItem> List_Status, List_Item_Province, List_Item_District, List_Item_Ward;
        public static void Init()
        {
            Data = new DataHelper.MongoHelper
                (
                System.Configuration.ConfigurationManager.AppSettings["METADATA_DB_SERVER"],
                System.Configuration.ConfigurationManager.AppSettings["METADATA_DB_DATABASE"]
                );
            if (LIST_DISTRICTS == null || LIST_DISTRICTS.Length == 0)
            {
                LIST_DISTRICTS = Data.List("mbcDistrict", null);
            }
            if (LIST_PROVINCES == null || LIST_PROVINCES.Length == 0)
            {
                LIST_PROVINCES = Data.List("mbcProvince", null);
            }
        }

        #region BusinessProfile


        public static List<dynamic> GetBusinessProfileByCustomerCode(string CustomerCode)
        {
            try
            {
                IMongoQuery _qCustomerCode = Query.EQ("_id", CustomerCode);
                LIST_BUSINESSPROFILE = new List<dynamic>();
                LIST_BUSINESSPROFILE.AddRange(PayID.Portal.Configuration.Data.List("business_profile", _qCustomerCode));
                return LIST_BUSINESSPROFILE;
            }
            catch { return null; }
        }
        #endregion
        public static string GetNameProvinceByProvinceCode(string ProvinceCode)
        {
            try
            {
                foreach (var p in LIST_PROVINCE)
                {
                    if (p.ProvinceCode == ProvinceCode)
                        return p.ProvinceName;

                }
            }
            catch { }
            return String.Empty;
        }
        public static string GetStatusDescriptionByStatusCode(string StatusCode)
        {
            string StatusDescription = "";
            try
            {
                foreach (var p in LIST_STATUS)
                {
                    if (p.StatusCode == StatusCode)
                        StatusDescription = p.StatusDescription;
                }
                return StatusDescription;
            }
            catch { return null; }
        }
        public static List<SelectListItem> GetDistrictByProvinceCode(string ProvinceCode)
        {
            try
            {
                IMongoQuery _qProvinceCode = Query.EQ("ProvinceCode", ProvinceCode);
                LIST_DISTRICT = new List<dynamic>();
                LIST_DISTRICT.AddRange(PayID.Portal.Configuration.Data.List("mbcDistrict", _qProvinceCode));

                List<SelectListItem> items = new List<SelectListItem>();
                foreach (var p in LIST_DISTRICT)
                {
                    items.Add(new SelectListItem
                    {
                        Text = p.DistrictName,
                        Value = p.DistrictCode.ToString()
                    }
                        );
                }

                return items;
            }
            catch { return null; }
        }
        public static List<SelectListItem> GetWardByDistrictId(string DistrictCode)
        {
            try
            {
                IMongoQuery _qDistrictId = Query.EQ("DistrictCode", DistrictCode);
                LIST_WARD = new List<dynamic>();
                LIST_WARD.AddRange(PayID.Portal.Configuration.Data.List("mbcCommune", _qDistrictId));

                List<SelectListItem> items = new List<SelectListItem>();
                foreach (var p in LIST_WARD)
                {
                    items.Add(new SelectListItem
                    {
                        Text = p.CommuneName,
                        Value = p.CommuneCode
                    }
                        );
                }

                return items;
            }
            catch { return null; }
        }
        public static List<SelectListItem> GetPosByDistrictCoce(string DistrictCode)
        {
            try
            {
                IMongoQuery _qDistrictCode = Query.EQ("UnitCode", DistrictCode);
                LIST_POS = new List<dynamic>();
                LIST_POS.AddRange(PayID.Portal.Configuration.Data.List("mbcPos", _qDistrictCode));

                List<SelectListItem> items = new List<SelectListItem>();
                foreach (var p in LIST_POS)
                {
                    items.Add(new SelectListItem
                    {
                        Text = p.POSName,
                        Value = p._id
                    }
                        );
                }

                return items;
            }
            catch { return null; }
        }
    }
}