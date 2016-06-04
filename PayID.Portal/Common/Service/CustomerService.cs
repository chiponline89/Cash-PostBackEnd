using PayID.Portal.Common.Log;
using PayID.Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace PayID.Portal.Common.Service
{
    public class CustomerService
    {
        public List<BusinessProfile> SearchBusinessProfile(string sSearchText, int pageIndex, ref long lTotal)
        {
            List<BusinessProfile> BusinessProfiles = new List<BusinessProfile>();

            try
            {
                lTotal = 0;

                if (pageIndex <= 0)
                {
                    pageIndex = 1;
                }

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    string url = "api/Merchant/SearchBusinessProfile?sSearch={0}&pageIndex={1}";

                    var response = new HttpResponseMessage();

                    response = client.GetAsync(string.Format(url, sSearchText, pageIndex)).Result;

                    var rs = response.Content.ReadAsAsync<dynamic>().Result;

                    if (rs.Code == "00")
                    {
                        dynamic oValue = rs.ListValue;
                        lTotal = long.Parse("0" + rs.Value.ToString());

                        foreach (var item in oValue)
                        {
                            BusinessProfile oBusinessProfile = new BusinessProfile();

                            BusinessProfiles.Add(new BusinessProfile
                            {
                                Address = (item.Address ?? "").ToString(),
                                BusinessTax = (item.BusinessTax ?? "").ToString(),
                                BusinessWebsite = (item.BusinessWebsite ?? "").ToString(),
                                ContactName = (item.ContactName ?? "").ToString(),
                                ContactPhoneMobile = (item.ContactPhoneMobile ?? "").ToString(),
                                ContactPhoneWork = (item.ContactPhoneWork ?? "").ToString(),
                                DistrictId = int.Parse((item.DistrictId ?? "0").ToString()),
                                GeneralAccountType = (item.GeneralAccountType ?? "").ToString(),
                                GeneralEmail = (item.GeneralEmail ?? "").ToString(),
                                GeneralFullName = (item.GeneralFullName ?? "").ToString(),
                                GeneralShortName = (item.GeneralShortName ?? "").ToString(),
                                GeneralSystem = (item.GeneralSystem ?? "").ToString(),
                                Id = long.Parse((item.Id ?? "0").ToString()),
                                ProvinceId = int.Parse((item.ProvinceId ?? "0").ToString()),
                                WardId = int.Parse((item.WardId ?? "0").ToString())
                            });
                        }
                    }
                    else
                    {
                        BusinessProfiles = null;
                    }

                }
            }
            catch( Exception ex)
            {
                LogAPP.LogToFile(LogFileType.EXCEPTION, "CustomerService.SearchBusinessProfile: " + ex.Message);
                BusinessProfiles = null;
            }

            return BusinessProfiles;
        }

        public List<BusinessProfile> GetBusinessProfile(string Id, string Email, string ShortName, int PageIndex)
        {
            List<BusinessProfile> BusinessProfiles = new List<BusinessProfile>();

            try
            {
                long lTotal = 0;

                if (PageIndex <= 0)
                {
                    PageIndex = 1;
                }

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    string url = "api/Merchant/GetBusinessProfile?CustomerId={0}&Email={1}&ShortName={2}&PageIndex={3}&PageSize={4}";

                    var response = new HttpResponseMessage();

                    response = client.GetAsync(string.Format(url, Id, Email, ShortName, PageIndex, Configuration.PageSize)).Result;

                    var rs = response.Content.ReadAsAsync<dynamic>().Result;

                    if (rs.Code == "00")
                    {
                        dynamic oValue = rs.ListValue;
                        lTotal = int.Parse("0" + rs.Value.ToString());

                        foreach (var item in oValue)
                        {
                            BusinessProfile oBusinessProfile = new BusinessProfile();

                            BusinessProfiles.Add(new BusinessProfile
                            {
                                Address = (item.Address ?? "").ToString(),
                                BusinessTax = (item.BusinessTax ?? "").ToString(),
                                BusinessWebsite = (item.BusinessWebsite ?? "").ToString(),
                                ContactName = (item.ContactName ?? "").ToString(),
                                ContactPhoneMobile = (item.ContactPhoneMobile ?? "").ToString(),
                                ContactPhoneWork = (item.ContactPhoneWork ?? "").ToString(),
                                DistrictId = int.Parse((item.DistrictId ?? "0").ToString()),
                                GeneralAccountType = (item.GeneralAccountType ?? "").ToString(),
                                GeneralEmail = (item.GeneralEmail ?? "").ToString(),
                                GeneralFullName = (item.GeneralFullName ?? "").ToString(),
                                GeneralShortName = (item.GeneralShortName ?? "").ToString(),
                                GeneralSystem = (item.GeneralSystem ?? "").ToString(),
                                Id = long.Parse((item.Id ?? "0").ToString()),
                                ProvinceId = int.Parse((item.ProvinceId ?? "0").ToString()),
                                WardId = int.Parse((item.WardId ?? "0").ToString())
                            });
                        }
                    }
                    else
                    {
                        BusinessProfiles = null;
                    }

                }
            }
            catch(Exception ex)
            {
                LogAPP.LogToFile(LogFileType.EXCEPTION, "CustomerService.GetBusinessProfile: " + ex.Message);
                BusinessProfiles = null;
            }

            return BusinessProfiles;
        }

        public List<Store> GetStore(string CustomerCode, long Id, string StoreCode, string StoreName, int PageIndex)
        {
            List<Store> Stores = new List<Store>();

            try
            {
                long lTotal = 0;

                if (PageIndex <= 0)
                {
                    PageIndex = 1;
                }

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    string url = "api/Merchant/GetStore/{0}?CustomerCode={1}&StoreCode={2}&StoreName={3}&PageIndex={4}&PageSize={5}";

                    var response = new HttpResponseMessage();

                    response = client.GetAsync(string.Format(url, Id, CustomerCode, StoreCode, StoreName, PageIndex, Configuration.PageSize)).Result;

                    var rs = response.Content.ReadAsAsync<dynamic>().Result;

                    if (rs.Code == "00")
                    {
                        dynamic oValue = rs.ListValue;
                        lTotal = int.Parse("0" + rs.Value.ToString());

                        foreach (var item in oValue)
                        {
                            Store oStore = new Store();

                            oStore.Address = item.Address.ToString();
                            oStore.CustomerCode = item.CustomerCode.ToString();
                            oStore.Default = int.Parse("0" + (item.Default ?? "").ToString());
                            oStore.DistrictCode = item.DistrictCode.ToString();
                            oStore.Id = long.Parse("0" + (item.Id.ToString()));
                            oStore.ManagerEmail = item.ManagerEmail.ToString();
                            oStore.ManagerMobile = item.ManagerMobile.ToString();
                            oStore.ManagerName = item.ManagerName.ToString();
                            oStore.PostCode = item.PostCode.ToString();
                            oStore.ProvinceCode = item.ProvinceCode.ToString();
                            oStore.StoreCode = item.StoreCode.ToString();
                            oStore.StoreName = item.StoreName.ToString();
                            Stores.Add(oStore);
                        }
                    }
                    else
                    {
                        Stores = null;
                    }

                }
            }
            catch(Exception ex)
            {
                LogAPP.LogToFile(LogFileType.EXCEPTION, "CustomerService.GetStore: " + ex.Message);
                Stores = null;
            }

            return Stores;
        }
    }
}