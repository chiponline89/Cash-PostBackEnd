using PayID.Portal.Common.Log;
using PayID.Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace PayID.Portal.Common.Service
{
    public class DictionaryService
    {
        public List<District> GetDistrictByProvince(string ProvinceCode, string DistrictCode = "")
        {
            List<District> ListDistrict = new List<District>();

            try
            {
                using(var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    string url = "api/Dictionary/GetDistrictByProvince?ProvinceCode={0}&DistrictCode={1}";

                    var response = new HttpResponseMessage();
                    response = client.GetAsync(string.Format(url, ProvinceCode, DistrictCode)).Result;
                    var rs = response.Content.ReadAsAsync<dynamic>().Result;

                    if(rs.Code == "00")
                    {
                        dynamic oValue = rs.ListValue;

                        foreach(var item in oValue)
                        {
                            ListDistrict.Add(new District
                            {
                                Description = item.Description.ToString(),
                                DistrictCode = item.DistrictCode.ToString(),
                                DistrictName = item.DistrictName.ToString(),
                                ProvinceCode = item.ProvinceCode.ToString()
                            });
                        }
                    }
                    else
                    {
                        ListDistrict = null;
                    }
                }
            }
            catch(Exception ex)
            {
                LogAPP.LogToFile(LogFileType.EXCEPTION, "DictionaryService.GetDistrictByProvince: " + ex.Message);
                ListDistrict = null;
            }
            return ListDistrict;
        }

        public List<Ward> GetWardByDistrict (string DistrictCode, string WardCode = "")
        {
            List<Ward> ListWard = new List<Ward>();

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    string url = "api/Dictionary/GetWardByDistrict?DistrictCode={0}&WardCode={1}";

                    var response = new HttpResponseMessage();
                    response = client.GetAsync(string.Format(url, DistrictCode, WardCode)).Result;
                    var rs = response.Content.ReadAsAsync<dynamic>().Result;

                    if (rs.Code == "00")
                    {
                        dynamic oValue = rs.ListValue;

                        foreach (var item in oValue)
                        {
                            ListWard.Add(new Ward
                            {
                                DistrictCode = item.DistrictCode.ToString(),
                                WardCode = item.WardCode.ToString(),
                                WardName = item.WardName.ToString()
                            });
                        }
                    }
                    else
                    {
                        ListWard = null;
                    }
                }
            }
            catch(Exception ex)
            {
                LogAPP.LogToFile(LogFileType.EXCEPTION, "DictionaryService.GetWardByDistrict: " + ex.Message);
                ListWard = null;
            }
            return ListWard;
        }

        public List<Unit> GetUnit(string UnitCode, string ParentUnitCode, string CommuneCode, string UnitTypeCode, int PageIndex, ref long Total)
        {
            List<Unit> ListUnit = new List<Unit>();

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    string url = "api/Dictionary/GetUnit?UnitCode={0}&ParentUnitCode={1}&CommuneCode={2}&UnitTypeCode={3}&PageIndex={4}&PageSize={5}";

                    var response = new HttpResponseMessage();
                    response = client.GetAsync(string.Format(url,UnitCode,ParentUnitCode,CommuneCode,UnitTypeCode,PageIndex,Configuration.PageSize)).Result;
                    var rs = response.Content.ReadAsAsync<dynamic>().Result;

                    if (rs.Code == "00")
                    {
                        dynamic oValue = rs.ListValue;
                        Total = long.Parse("0" + rs.Value.ToString());

                        foreach (var item in oValue)
                        {
                            ListUnit.Add(new Unit
                            {
                                UnitCode = (item.UnitCode ?? "").ToString(),
                                UnitName = (item.UnitName ?? "").ToString(),
                                ParentUnitCode = (item.ParentUnitCode ?? "").ToString(),
                                CommuneCode = (item.CommuneCode ?? "").ToString(),
                                UnitTypeCode = (item.UnitTypeCode ?? "").ToString()
                            });
                        }
                    }
                    else
                    {
                        ListUnit = null;
                    }
                }
            }
            catch(Exception ex)
            {
                LogAPP.LogToFile(LogFileType.EXCEPTION, "DictionaryService.GetUnit: " + ex.Message);
                ListUnit = null;
            }

            return ListUnit;
        }

        public List<Post> GetPost(string PosCode, string CommuneCode, int PosTypeCode, string ProvinceCode, string PosLevelCode, string Status, string UnitCode, int PageIndex, ref long Total, string IsWorkflowUnit = "", string Dispatch = "")
        {
            List<Post> ListPost = new List<Post>();

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    string url = "api/Dictionary/GetPost?PosCode={0}&CommuneCode={1}&PosTypeCode={2}&ProvinceCode={3}&PosLevelCode={4}&Status={5}&UnitCode={6}&PageIndex={7}&PageSize={8}&IsWorkflowUnit={9}&Dispatch={10}";

                    var response = new HttpResponseMessage();
                    response = client.GetAsync(string.Format(url, PosCode, CommuneCode, PosTypeCode, ProvinceCode, PosLevelCode, Status, UnitCode, PageIndex, Configuration.PageSize, IsWorkflowUnit, Dispatch)).Result;
                    var rs = response.Content.ReadAsAsync<dynamic>().Result;

                    if (rs.Code == "00")
                    {
                        dynamic oValue = rs.ListValue;
                        Total = long.Parse("0" + rs.Value.ToString());

                        foreach (var item in oValue)
                        {
                            Post oPost = new Post();

                            oPost.PosCode = (item.PosCode ?? "").ToString();
                            oPost.PosName = (item.PosName ?? "").ToString();
                            oPost.Address = (item.Address ?? "").ToString();
                            oPost.CommuneCode = (item.CommuneCode ?? "").ToString();
                            oPost.Tel = (item.Tel ?? "").ToString();
                            oPost.Fax = (item.Fax ?? "").ToString();
                            oPost.PosTypeCode = int.Parse("0" + (item.PosTypeCode ?? "0").ToString());
                            oPost.ProvinceCode = (item.ProvinceCode ?? "").ToString();
                            oPost.PosLevelCode = (item.PosLevelCode ?? "").ToString();
                            oPost.AddressCode = (item.AddressCode ?? "").ToString();
                            oPost.Status = (item.Status ?? "").ToString();
                            oPost.UnitCode = (item.UnitCode ?? "").ToString();

                            ListPost.Add(oPost);

                        }
                    }
                    else
                    {
                        ListPost = null;
                    }
                }
            }
            catch(Exception ex)
            {
                LogAPP.LogToFile(LogFileType.EXCEPTION, "DictionaryService.GetPost: " + ex.Message);
                ListPost = null;
            }

            return ListPost;
        }

        public List<WorkFlowService> GetWorkFlowService(string UnitCode, string RequestService, string RequestType)
        {
            List<WorkFlowService> ListWorkFlowService = new List<WorkFlowService>();

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    string url = "api/Dictionary/GetWorkFlowService?UnitCode={0}&RequestService={1}&RequestType={2}";

                    var response = new HttpResponseMessage();
                    response = client.GetAsync(string.Format(url, UnitCode, RequestService, RequestType)).Result;
                    var rs = response.Content.ReadAsAsync<dynamic>().Result;

                    if (rs.Code == "00")
                    {
                        dynamic oValue = rs.ListValue;

                        foreach (var item in oValue)
                        {
                            WorkFlowService oWorkFlowService = new WorkFlowService();

                            oWorkFlowService.Id = (item.Id ?? "").ToString();
                            oWorkFlowService.UnitCode = (item.UnitCode ?? "").ToString();
                            oWorkFlowService.RequestService = (item.RequestService ?? "").ToString();
                            oWorkFlowService.RequestType = (item.RequestType ?? "").ToString();
                            oWorkFlowService.Steps = (item.Steps ?? "").ToString();
                            oWorkFlowService.StepName = (item.StepName ?? "").ToString();

                            ListWorkFlowService.Add(oWorkFlowService);
                        }
                    }
                    else
                    {
                        ListWorkFlowService = null;
                    }
                }
            }
            catch(Exception ex)
            {
                LogAPP.LogToFile(LogFileType.EXCEPTION, "DictionaryService.GetWorkFlowService: " + ex.Message);
                ListWorkFlowService = null;
            }
            return ListWorkFlowService;
        }

        public string UpdateUnit(Unit oUnit)
        {
            string sReturn = string.Empty;

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    string url = "api/Dictionary/UpdateUnit";

                    var response = new HttpResponseMessage();
                    response = client.PostAsJsonAsync(url, oUnit).Result;
                    var rs = response.Content.ReadAsAsync<dynamic>().Result;

                    string Code = rs.Code.ToString();
                    string Mes = rs.Message.ToString();

                    sReturn = Code + "|" + Mes;
                }
            }
            catch(Exception ex)
            {
                LogAPP.LogToFile(LogFileType.EXCEPTION, "DictionaryService.UpdateUnit: " + ex.Message);
                sReturn = "-99|Có lỗi trong quá trình xử lý dữ liệu.";
            }

            return sReturn;
        }

        public string UpdatePos(Post oPost)
        {
            string sReturn = string.Empty;

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    string url = "api/Dictionary/UpdatePos";

                    var response = new HttpResponseMessage();
                    response = client.PostAsJsonAsync(url, oPost).Result;
                    var rs = response.Content.ReadAsAsync<dynamic>().Result;

                    string Code = rs.Code.ToString();
                    string Mes = rs.Message.ToString();

                    sReturn = Code + "|" + Mes;
                }
            }
            catch (Exception ex)
            {
                LogAPP.LogToFile(LogFileType.EXCEPTION, "DictionaryService.UpdatePos: " + ex.Message);
                sReturn = "-99|Có lỗi trong quá trình xử lý dữ liệu.";
            }

            return sReturn;
        }

        public string ChangeStatusPos(Post oPos)
        {
            string sReturn = string.Empty;

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    string url = "api/Dictionary/ChangeStatusPos";

                    var response = new HttpResponseMessage();
                    response = client.PostAsJsonAsync(url, oPos).Result;
                    var rs = response.Content.ReadAsAsync<dynamic>().Result;

                    string Code = rs.Code.ToString();
                    string Mes = rs.Message.ToString();

                    sReturn = Code + "|" + Mes;
                }
            }
            catch(Exception ex)
            {
                LogAPP.LogToFile(LogFileType.EXCEPTION, "DictionaryService.ChangeStatusPos: " + ex.Message);
                sReturn = "-99|Có lỗi trong quá trình xử lý dữ liệu.";
            }

            return sReturn;
        }

        public string UpdateWorkFlowService(WorkFlowService oWorkFlowService)
        {
            string sReturn = string.Empty;

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    string url = "api/Dictionary/UpdateWorkFlowService";

                    var response = new HttpResponseMessage();
                    response = client.PostAsJsonAsync(url, oWorkFlowService).Result;
                    var rs = response.Content.ReadAsAsync<dynamic>().Result;

                    string Code = rs.Code.ToString();
                    string Mes = rs.Message.ToString();

                    sReturn = Code + "|" + Mes;
                }
            }
            catch(Exception ex)
            {
                LogAPP.LogToFile(LogFileType.EXCEPTION, "DictionaryService.UpdateWorkFlowService: " + ex.Message);
                sReturn = "-99|Có lỗi trong quá trình xử lý dữ liệu.";
            }

            return sReturn;
        }
    }
}