using PayID.Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace PayID.Portal.Common
{
    public class Configuration
    {
        public static int PageSize = 10;
        public static List<Province> ListProvince = new List<Province>();
        public static List<ServiceType> ListService = new List<ServiceType>();
        public static List<Status> ListStatus = new List<Status>();
        public static List<Role> ListRole = new List<Role>();
        public static void Init()
        {
            PageSize = int.Parse("0" + System.Configuration.ConfigurationManager.AppSettings["PAGE_SIZE"].ToString());
            ListProvince = InitProvince();
            ListService = InitServiceType();
            ListStatus = InitStatus();
            ListRole = InitRoles();
        }

        public static List<Role> InitRoles()
        {
            List<Role> ListRole = new List<Role>();

            ListRole.Add(new Role{Id = "1", Name = "Admin" });
            ListRole.Add(new Role { Id = "2", Name = "Nghiệp vụ" });
            ListRole.Add(new Role { Id = "3", Name = "Kế toán - đối soát" });
            ListRole.Add(new Role { Id = "4", Name = "Chăm sóc Khách hàng" });
            ListRole.Add(new Role { Id = "5", Name = "Giao dịch viên" });
            ListRole.Add(new Role { Id= "6", Name = "Bưu tá"});

            return ListRole;
        }

        public static List<Status> InitStatus()
        {
            List<Status> ListStatus = new List<Status>();

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    string url = "api/Dictionary/GetStatus";

                    var response = client.GetAsync(url).Result;
                    var rs = response.Content.ReadAsAsync<dynamic>().Result;

                    if (rs.Code == "00")
                    {
                        dynamic oValue = rs.ListValue;

                        foreach (var item in oValue)
                        {
                            ListStatus.Add(new Status
                            {
                                Id = (item.Id ?? "").ToString(),
                                StatusCode = (item.StatusCode ?? "").ToString(),
                                StatusDescription = (item.StatusDescription ?? "").ToString()
                            });
                        }
                    }
                    else
                    {
                        ListStatus = null;
                    }
                }
            }
            catch
            {
                ListStatus = null;
            }

            return ListStatus;
        }

        public static List<Province> InitProvince()
        {
            List<Province> ListProvince = new List<Province>();

            try
            {
                using(var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    string url = "api/Dictionary/GetProvince";

                    var response = client.GetAsync(url).Result;
                    var rs = response.Content.ReadAsAsync<dynamic>().Result;

                    if(rs.Code == "00")
                    {
                        dynamic oValue = rs.ListValue;

                        foreach(var item in oValue)
                        {
                            ListProvince.Add(new Province{
                                Description = item.Description.ToString(),
                                ProvinceCode = item.ProvinceCode.ToString(),
                                ProvinceListCode = item.ProvinceListCode.ToString(),
                                ProvinceName = item.ProvinceName.ToString()
                            });
                        }
                    }
                    else
                    {
                        ListProvince = null;
                    }
                }
            }
            catch
            {
                ListProvince = null;
            }

            return ListProvince;
        }

        public static List<ServiceType> InitServiceType()
        {
            List<ServiceType> ListService = new List<ServiceType>();
            ListService.Add(new ServiceType
            {
                ServiceId = "1",
                ServiceName = "Chuyển phát nhanh"
            });

            ListService.Add(new ServiceType
            {
                ServiceId = "2",
                ServiceName = "Chuyển phát thường"
            });

            return ListService;
        }
    }
}