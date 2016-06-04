using PayID.Portal.Common.Log;
using PayID.Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace PayID.Portal.Common.Service
{
    public class LadingService
    {
        public List<Lading> GetLading(string LadingCode, string FromDate, string ToDate, string Status, string ProvinceCode, string CustomerCode, int PageIndex,ref long lTotal)
        {
            List<Lading> ListLading = new List<Lading>();

            
            if (PageIndex <= 0)
            {
                PageIndex = 1;
            }

            int PageSize = Common.Configuration.PageSize;

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    string url = "api/Lading/GetLading?LadingCode={0}&FromDate={1}&ToDate={2}&Status={3}&ProvinceCode={4}&CustomerCode={5}&PageIndex={6}&PageSize={7}";

                    var response = new HttpResponseMessage();
                    response = client.GetAsync(string.Format(url, LadingCode, FromDate, ToDate, Status, ProvinceCode, CustomerCode, PageIndex, PageSize)).Result;
                    var rs = response.Content.ReadAsAsync<dynamic>().Result;

                    if (rs.Code == "00")
                    {
                        dynamic oValue = rs.ListValue;
                        lTotal = long.Parse("0" + rs.Value.ToString());

                        foreach(var item in oValue)
                        {
                            Lading oLading = new Lading();

                            oLading.Code = (item.Code ?? "").ToString();
                            oLading.CodFee = long.Parse((item.CodFee ?? "0").ToString());
                            oLading.CustomerCode = (item.CustomerCode ?? "").ToString();
                            oLading.FromDistrictCode = (item.FromDistrictCode ?? "0").ToString();
                            oLading.FromProvinceCode = (item.FromProvinceCode ?? "0").ToString();
                            oLading._id = (item._id ?? "").ToString();
                            oLading.MainFee = long.Parse((item.MainFee ?? "0").ToString());
                            oLading.PartnerCode = (item.PartnerCode ?? "").ToString();
                            oLading.ProductDescription = (item.ProductDescription ?? "").ToString();
                            oLading.ProductName = (item.ProductName ?? "").ToString();
                            oLading.Quantity = int.Parse((item.Quantity ?? "0").ToString());
                            oLading.SenderAddress = (item.SenderAddress ?? "").ToString();
                            oLading.SenderMobile = (item.SenderMobile ?? "").ToString();
                            oLading.SenderName = (item.SenderName ?? "").ToString();
                            oLading.ServiceCode = (item.ServiceCode ?? "").ToString();
                            oLading.ServiceFee = long.Parse((item.ServiceFee ?? "0").ToString());
                            oLading.Status = (item.Status ?? "").ToString();
                            oLading.ToDistrictCode = (item.ToDistrictCode ?? "").ToString();
                            oLading.ToProvinceCode =(item.ToProvinceCode ?? "").ToString();
                            oLading.TotalFee = long.Parse((item.TotalFee ?? "0").ToString());
                            oLading.ToWardCode = (item.ToWardCode ?? "0").ToString();
                            oLading.Type = (item.Type ?? "").ToString();
                            oLading.Value = long.Parse((item.Value ?? "0").ToString());
                            oLading.Weight = long.Parse((item.Weight ?? "0").ToString());
                            oLading.DateCreated = DateTime.Parse(item.DateCreated.ToString());
                            oLading.ReceiverName = (item.ReceiverName ?? "").ToString();
                            oLading.ReceiverAddress = (item.ReceiverAddress ?? "").ToString();
                            oLading.ReceiverMobile = (item.ReceiverMobile ?? "").ToString();

                            ListLading.Add(oLading);
                        }
                    }
                    else
                    {
                        ListLading = null;
                    }
                }
            }
            catch(Exception ex)
            {
                LogAPP.LogToFile(LogFileType.EXCEPTION, "LadingService.GetLading: " + ex.Message);
                ListLading = null;
            }

            return ListLading;
        }

        public string LadingCancel(string LadingCode)
        {
            string sReturn = string.Empty;

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    string url = "api/Lading/LadingCancel?LadingCode={0}";

                    var response = new HttpResponseMessage();
                    response = client.PostAsync(string.Format(url, LadingCode),null).Result;
                    var rs = response.Content.ReadAsAsync<dynamic>().Result;

                    string Code = rs.Code.ToString().Trim();
                    string Mes = rs.Message.ToString().Trim();

                    sReturn = Code + "|" + Mes;
                }
            }
            catch(Exception ex)
            {
                LogAPP.LogToFile(LogFileType.EXCEPTION, "LadingService.LadingCancel: " + ex.Message);
                sReturn = "-99|Có lỗi trong quá trình xử lý dữ liệu.";
            }

            return sReturn;
        }

        public List<LogJourney> GetLogJourney(string LadingCode)
        {
            List<LogJourney> ListLogJourney = new List<LogJourney>();

            try
            {
                using(var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    string url = "api/Lading/GetLogJourney?LadingCode={0}";

                    var response = new HttpResponseMessage();
                    response = client.GetAsync(string.Format(url, LadingCode)).Result;

                    var rs = response.Content.ReadAsAsync<dynamic>().Result;

                    if(rs.Code == "00")
                    {
                        dynamic oValue = rs.ListValue;

                        foreach (var item in oValue)
                        {
                            ListLogJourney.Add(new LogJourney
                            {
                                Code = (item.Code ?? "").ToString(),
                                DateCreate = DateTime.Parse(item.DateCreate.ToString()),
                                Location = (item.Location ?? "").ToString(),
                                Note = (item.Note ?? "").ToString(),
                                Status = (item.Status ?? "").ToString(),
                                UserId = (item.UserId ?? "").ToString()
                            });
                        }
                    }
                    else
                    {
                        ListLogJourney = null;
                    }
                }
            }
            catch(Exception ex)
            {
                LogAPP.LogToFile(LogFileType.EXCEPTION, "LadingService.GetLogJourney: " + ex.Message);
                ListLogJourney = null;
            }

            return ListLogJourney;
        }

        public string LadingCreate(UpFileLading oUpload)
        {
            string sReturn = string.Empty;

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    string url = "api/Lading/LadingCreate";

                    var response = new HttpResponseMessage();
                    response = client.PostAsJsonAsync(url, oUpload).Result;

                    var rs = response.Content.ReadAsAsync<dynamic>().Result;

                    string Code = rs.Code.ToString();
                    string Mes = rs.Message.ToString();

                    sReturn = Code + "|" + Mes;

                }
            }
            catch(Exception ex)
            {
                LogAPP.LogToFile(LogFileType.EXCEPTION, "LadingService.LadingCreate: " + ex.Message);
                sReturn = "-99|Có lỗi trong quá trình xử lý dữ liệu.";
            }

            return sReturn;
        }

    }
}