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
namespace PayID.Portal.Areas.Merchant
{
    public class Configuration
    {
        public static PayID.DataHelper.MongoHelper Data;
        public static List<dynamic> LIST_PROVINCE, LIST_DISTRICT, LIST_WARD, LIST_BUSINESSPROFILE, LIST_STATUS, LIST_POS;
        public static List<SelectListItem> List_Status, List_Item_Province, List_Item_District, List_Item_Ward;
        public static void Init()
        {
            Data = new DataHelper.MongoHelper(
                System.Configuration.ConfigurationManager.AppSettings["MERCHANT_DB_SERVER"],
                System.Configuration.ConfigurationManager.AppSettings["MERCHANT_DB_DATABASE"]
                );           
        }     

        public static void GetLocaltion()
        {
            #region Province
            try
            {
                LIST_PROVINCE = new List<dynamic>();
                LIST_PROVINCE.AddRange(PayID.Portal.Configuration.Data.List("mbcProvince", null));
                List<SelectListItem> items = new List<SelectListItem>();
                foreach (var p in LIST_PROVINCE)
                {
                    items.Add(new SelectListItem
                    {
                        Text = p.ProvinceName,
                        Value = p.ProvinceCode
                    }
                        );
                }

                List_Item_Province = items;
            }
            catch { }
            #endregion
            #region Status
            try
            {
                LIST_STATUS = new List<dynamic>();
                LIST_STATUS.AddRange(PayID.Portal.Configuration.Data.List("Status", null));
                List<SelectListItem> items = new List<SelectListItem>();
                foreach (var p in LIST_STATUS)
                {
                    items.Add(new SelectListItem
                    {
                        Text = p.StatusDescription,
                        Value = p.StatusCode
                    }
                        );
                }

                List_Status = items;
            }
            catch { }
            #endregion

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
            try
            {
                foreach (var p in LIST_STATUS)
                {
                    if (p.StatusCode == StatusCode)
                        return p.StatusDescription;
                }
            }
            catch {}
            return String.Empty;
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
                        Value = p.POSCode
                    }
                        );
                }

                return items;
            }
            catch { return null; }
        }
    }
}