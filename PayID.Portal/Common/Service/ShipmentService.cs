using PayID.Portal.Common.Log;
using PayID.Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace PayID.Portal.Common.Service
{
    public class ShipmentService
    {
        public string UpdateShipment(Shipment _Shipment, ref bool bResult)
        {
            string sReturn = "-99|Có lỗi trong quá trình xử lý dữ liệu";

            try
            {
                ShipmentAPI oShipmentAPI = new ShipmentAPI();

                oShipmentAPI.Id = _Shipment.Id ?? "";


                oShipmentAPI.FromAddress = new Address();
                oShipmentAPI.FromAddress.CustomerCode = _Shipment.CustomerCode;
                oShipmentAPI.FromAddress.CustomerName = _Shipment.CustomerName;
                oShipmentAPI.FromAddress.CustomerMobile = _Shipment.CustomerMobile;
                oShipmentAPI.FromAddress.StoreCode = _Shipment.StoreId;
                oShipmentAPI.FromAddress.StoreName = _Shipment.StoreName;
                oShipmentAPI.FromAddress.ProvinceId = _Shipment.PickUpProvince;
                oShipmentAPI.FromAddress.DistrictId = _Shipment.PickUpDistrict;
                oShipmentAPI.FromAddress.WardId = _Shipment.PickUpWard;
                oShipmentAPI.FromAddress.Street = _Shipment.PickUpStreet;
                oShipmentAPI.FromAddress.FullAddress = _Shipment.PickUpAddress;
                oShipmentAPI.FromAddress.PostCode = _Shipment.PostCode;
                oShipmentAPI.FromAddress.PostCodeLink = _Shipment.PostCodeLink;
                oShipmentAPI.FromAddress.ContactName = _Shipment.ContactName;
                oShipmentAPI.FromAddress.ContactMobile = _Shipment.ContactMobile;
                oShipmentAPI.FromAddress.ContactEmail = _Shipment.CustomerEmail;
                oShipmentAPI.FromAddress.ProvinceName = _Shipment.PickUpProvinceName;
                oShipmentAPI.FromAddress.DistrictName = _Shipment.PickUpDistrictName;
                oShipmentAPI.FromAddress.WardName = _Shipment.PickUpWardName;


                oShipmentAPI.ToAddress = new Address();
                oShipmentAPI.ToAddress.ContactName = _Shipment.ReceiverName;
                oShipmentAPI.ToAddress.ProvinceId = _Shipment.ReceiverProvince;
                oShipmentAPI.ToAddress.DistrictId = _Shipment.ReceiverDistrict;
                oShipmentAPI.ToAddress.WardId = _Shipment.ReceiverWard;
                oShipmentAPI.ToAddress.Street = _Shipment.ReceiverStreet;
                oShipmentAPI.ToAddress.FullAddress = _Shipment.ReceiverAddress;
                oShipmentAPI.ToAddress.ContactMobile = _Shipment.ReceiverMobile;
                oShipmentAPI.ToAddress.ContactEmail = "";


                oShipmentAPI.Product = new Product();
                oShipmentAPI.Product.Value = _Shipment.Amount;
                oShipmentAPI.Product.Weight = _Shipment.Weight;
                oShipmentAPI.Product.Quantity = _Shipment.Quantity;
                oShipmentAPI.Product.Name = _Shipment.ProductName;
                oShipmentAPI.Product.Description = _Shipment.Description;


                oShipmentAPI.Service = new Models.Service();
                oShipmentAPI.Service.CashPostService = _Shipment.ServiceType;

                oShipmentAPI.Status = Param.C5;
                oShipmentAPI.UnitCreateCode = _Shipment.UnitCreate;
                oShipmentAPI.UnitCreateName = _Shipment.UnitCreateName;
                oShipmentAPI.UserCreateId = _Shipment.UserCreate;
                oShipmentAPI.UserCreateName = _Shipment.UserCreateName;
                oShipmentAPI.CurrentAssigned = _Shipment.UnitLink;
                oShipmentAPI.CurrentAssignedName = _Shipment.UnitName;


                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    string url = string.Empty;

                    if (!string.IsNullOrEmpty(_Shipment.Id))
                    {
                        url = "api/Shipment/EditShipment";
                    }
                    else
                    {
                        url = "api/Shipment/CreateShipment";
                    }

                    var response = new HttpResponseMessage();
                    response = client.PostAsJsonAsync(url, oShipmentAPI).Result;
                    var rs = response.Content.ReadAsAsync<dynamic>().Result;


                    if (rs.Code == "00")
                    {
                        sReturn = rs.Value.ToString() + "|" + rs.Message.ToString();
                    }
                    else
                    {
                        sReturn = rs.Code.ToString() + "|" + rs.Message.ToSTring();
                    }
                }
            }
            catch (Exception ex)
            {
                LogAPP.LogToFile(LogFileType.EXCEPTION, "ShipmentService.CreateShipment: " + ex.Message);
                sReturn = "-99|Có lỗi trong quá trình xử lý dữ liệu";
            }

            return sReturn;
        }

        public List<ShipmentAPI> GetShipment(string CustomerCode, string ProvinceId, string DistrictId, string UnitCreate, string CurrentAssigned, string Id, string TrackingCode, long FromDate, long ToDate, string Status, int PageIndex, int PageSize, ref long lTotal)
        {
            List<ShipmentAPI> ListShipment = new List<ShipmentAPI>();

            lTotal = 0;

            if (PageIndex <= 0)
            {
                PageIndex = 1;
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    string url = "api/Shipment/GetShipment?CustomerCode={0}&ProvinceId={1}&DistrictId={2}&UnitCreate={3}&CurrentAssigned={4}&ShipmentId={5}&TrackingCode={6}&FromDate={7}&ToDate={8}&Status={9}&PageIndex={10}&PageSize={11}";

                    var response = new HttpResponseMessage();
                    response = client.GetAsync(string.Format(url, CustomerCode, ProvinceId, DistrictId, UnitCreate, CurrentAssigned, Id, TrackingCode, FromDate, ToDate, Status, PageIndex, PageSize)).Result;
                    var rs = response.Content.ReadAsAsync<dynamic>().Result;

                    if (rs.Code == "00")
                    {
                        dynamic oValue = rs.ListValue;
                        lTotal = long.Parse("0" + rs.Value.ToString());

                        foreach (var item in oValue)
                        {
                            ShipmentAPI oShipment = new ShipmentAPI();

                            oShipment.Id = (item.Id ?? "").ToString();
                            oShipment.TrackingCode = (item.TrackingCode ?? "").ToString();
                            oShipment.RefCode = (item.RefCode ?? "").ToString();

                            if (item.FromAddress != null)
                            {
                                dynamic oFromAddress = item.FromAddress;
                                oShipment.FromAddress = new Address();
                                oShipment.FromAddress.CustomerCode = (oFromAddress.CustomerCode ?? "").ToString();
                                oShipment.FromAddress.CustomerName = (oFromAddress.CustomerName ?? "").ToString();
                                oShipment.FromAddress.CustomerMobile = (oFromAddress.CustomerMobile ?? "").ToString();
                                oShipment.FromAddress.StoreCode = (oFromAddress.StoreCode ?? "").ToString();
                                oShipment.FromAddress.StoreName = (oFromAddress.StoreName ?? "").ToString();
                                oShipment.FromAddress.ProvinceId = int.Parse("0" + (oFromAddress.ProvinceId ?? "0").ToString());
                                oShipment.FromAddress.DistrictId = int.Parse("0" + (oFromAddress.DistrictId ?? "0").ToString());
                                oShipment.FromAddress.WardId = int.Parse("0" + (oFromAddress.WardId ?? "0").ToString());
                                oShipment.FromAddress.Street = (oFromAddress.Street ?? "").ToString();
                                oShipment.FromAddress.FullAddress = (oFromAddress.FullAddress ?? "").ToString();
                                oShipment.FromAddress.PostCode = (oFromAddress.PostCode ?? "").ToString();
                                oShipment.FromAddress.PostCodeLink = (oFromAddress.PostCodeLink ?? "").ToString();
                                oShipment.FromAddress.ContactName = (oFromAddress.ContactName ?? "").ToString();
                                oShipment.FromAddress.ContactMobile = (oFromAddress.ContactMobile ?? "").ToString();
                                oShipment.FromAddress.ContactEmail = (oFromAddress.ContactEmail ?? "").ToString();
                            }

                            if (item.ToAddress != null)
                            {
                                dynamic oToAddress = item.ToAddress;
                                oShipment.ToAddress = new Address();
                                oShipment.ToAddress.ContactName = (oToAddress.ContactName ?? "").ToString();
                                oShipment.ToAddress.ProvinceId = int.Parse("0" + (oToAddress.ProvinceId ?? "0").ToString());
                                oShipment.ToAddress.DistrictId = int.Parse("0" + (oToAddress.DistrictId ?? "0").ToString());
                                oShipment.ToAddress.WardId = int.Parse("0" + (oToAddress.WardId ?? "0").ToString());
                                oShipment.ToAddress.Street = (oToAddress.Street ?? "").ToString();
                                oShipment.ToAddress.FullAddress = (oToAddress.FullAddress ?? "").ToString();
                                oShipment.ToAddress.ContactMobile = (oToAddress.ContactMobile ?? "").ToString();
                                oShipment.ToAddress.ContactEmail = (oToAddress.ContactEmail ?? "").ToString();
                            }

                            if (item.Product != null)
                            {
                                dynamic oProduct = item.Product;
                                oShipment.Product = new Product();
                                oShipment.Product.Value = long.Parse("0" + (oProduct.Value ?? "0").ToString());
                                oShipment.Product.Weight = long.Parse("0" + (oProduct.Weight ?? "0").ToString());
                                oShipment.Product.Quantity = int.Parse("0" + (oProduct.Quantity ?? "0").ToString());
                                oShipment.Product.Name = (oProduct.Name ?? "").ToString();
                                oShipment.Product.Description = (oProduct.Description ?? "").ToString();
                            }

                            if (item.Service != null)
                            {
                                dynamic oService = item.Service;
                                oShipment.Service = new Models.Service();
                                oShipment.Service.CashPostService = int.Parse("0" + (oService.CashPostService ?? "0").ToString());
                                oShipment.Service.ShippingMainService = int.Parse("0" + (oService.CashPostService ?? "0").ToString());
                                oShipment.Service.ShippingAddService = (oService.CashPostService ?? "").ToString();
                            }

                            oShipment.Status = (item.Status ?? "").ToString();
                            oShipment.UnitCreateCode = (item.UnitCreateCode ?? "").ToString();
                            oShipment.UnitCreateName = (item.UnitCreateName ?? "").ToString();
                            oShipment.UserCreateId = (item.UserCreateId ?? "").ToString();
                            oShipment.UserCreateName = (item.UserCreateName ?? "").ToString();
                            oShipment.LastUpdateTime = long.Parse("0" + (item.LastUpdateTime ?? "0").ToString());
                            oShipment.CreateTime = long.Parse("0" + (item.CreateTime ?? "0").ToString());
                            oShipment.CurrentAssigned = (item.CurrentAssigned ?? "").ToString();
                            oShipment.CurrentAssignedName = (item.CurrentAssignedName ?? "").ToString();
                            oShipment.CurrentPostman = (item.CurrentPostman ?? "").ToString();
                            oShipment.CurrentPostmanName = (item.CurrentPostmanName ?? "").ToString();


                            ListShipment.Add(oShipment);
                        }
                    }
                    else
                    {
                        ListShipment = null;
                    }
                }
            }
            catch (Exception ex)
            {
                LogAPP.LogToFile(LogFileType.EXCEPTION, "ShipmentService.GetShipment: " + ex.Message);
                ListShipment = null;
            }

            return ListShipment;
        }

        public string Assign(List<Assign> ListAssign, ref List<string> ListReturn)
        {
            string sReturn = string.Empty;
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    string url = "api/Shipment/Assign";

                    var response = new HttpResponseMessage();
                    response = client.PostAsJsonAsync(url, ListAssign).Result;
                    var rs = response.Content.ReadAsAsync<dynamic>().Result;

                    if(rs.Code == "01")
                    {
                        dynamic oValue = rs.ListValue;
                        if(oValue != null)
                        {
                            ListReturn = new List<string>();
                            foreach(var item in oValue)
                            {
                                ListReturn.Add(item.ToString().Trim());
                            }
                        }
                    }

                    sReturn = rs.Code + "|" + rs.Message;
                }
            }
            catch (Exception ex)
            {
                LogAPP.LogToFile(LogFileType.EXCEPTION, "ShipmentService.Assign: " + ex.Message);
                sReturn = "-99|Có lỗi trong quá trình xử lý dữ liệu";
            }

            return sReturn;
        }

        public string AssignCancel(List<Assign> ListAssign, ref List<string> ListReturn)
        {
            string sReturn = string.Empty;
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    string url = "api/Shipment/CancelAssign";

                    var response = new HttpResponseMessage();
                    response = client.PostAsJsonAsync(url, ListAssign).Result;
                    var rs = response.Content.ReadAsAsync<dynamic>().Result;

                    if (rs.Code == "01")
                    {
                        dynamic oValue = rs.ListValue;
                        if (oValue != null)
                        {
                            ListReturn = new List<string>();
                            foreach (var item in oValue)
                            {
                                ListReturn.Add(item.ToString().Trim());
                            }
                        }
                    }

                    sReturn = rs.Code + "|" + rs.Message;
                }
            }
            catch (Exception ex)
            {
                LogAPP.LogToFile(LogFileType.EXCEPTION, "ShipmentService.AssignCancel: " + ex.Message);
                sReturn = "-99|Có lỗi trong quá trình xử lý dữ liệu";
            }

            return sReturn;
        }
    }
}