using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MongoDB.Driver.Builders;
using MongoDB.Driver;
using MongoDB.Bson;
using PayID.Common;
using PayID.DataHelper;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Dynamic;

namespace PayID.API.Controllers
{
    public class LadingController : ApiController
    {
        [AcceptVerbs("POST")]
        public JObject Lading(string function, JObject request)
        {
            //Chuyển yêu cầu dạng JObject sang dynamic
            dynamic _request = new PayID.DataHelper.DynamicObj(
                MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(JsonConvert.SerializeObject(request)));

            dynamic _logRequest = _request;

            //Khởi tạo kết quả trả ra với mã lỗi mặc định
            dynamic _response = new ExpandoObject();
            _response.response_code = "96";
            _response.response_message = "Lỗi xử lý hệ thống";

            string _json_response = String.Empty;
            try
            {
                //Nếu trong yêu cầu không có tham số function
                if (String.IsNullOrEmpty(function))
                {
                    _response.response_code = "90";
                    _response.response_message = "Sai tham số kết nối";
                    _json_response = JsonConvert.SerializeObject(_response);
                    return JObject.Parse(_json_response);
                }
                switch (function)
                {
                    case "delete_lading":
                        break;
                    case "update_lading":
                        lading_update(_request, out _response);
                        break;
                    case "create_lading":
                        lading_create(_request, out _response);
                        break;
                    default:
                        _response.response_code = "91";
                        _response.response_message = "Yêu cầu không được hỗ trợ";
                        break;
                }
            }
            catch
            {
                _response.response_code = "09";
                _response.response_message = "Xử lý thất bại.";
                return _response;
            }
            _json_response = JsonConvert.SerializeObject(_response);
            return JObject.Parse(_json_response);
        }

        #region Hàm lấy ra tất cả danh sách vận đơn trong 1 tháng hoặc theo điều kiện.
        /// <summary>
        /// Hàm lấy ra tất cả danh sách vận đơn trong 1 tháng hoặc theo điều kiện.
        /// trungnd - 26/09/2014
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        public JObject[] LadingByReQuests(JObject request)
        {
            string date_default = DateTime.Now.AddDays(-15).ToString("dd-MM-yyyy");
            string code, from_date, to_date, status, to_province_code, customer_code, unit_link;
            code = (request["code"] != null) ? request["code"].ToString() : "";
            to_date = (request["to_date"] != null) ? request["to_date"].ToString() : "";
            from_date = (request["from_date"] != null) ? request["from_date"].ToString() : "";
            status = (request["status"] != null) ? request["status"].ToString() : "";
            to_province_code = (request["to_province_code"] != null) ? request["to_province_code"].ToString() : "";
            customer_code = (request["customer_code"] != null) ? request["customer_code"].ToString() : "";
            unit_link = (request["unit_link"] != null) ? request["unit_link"].ToString() : "";
            IMongoQuery query = Query.NE("_id", 0);
            //query = Query.And(
            //       query,
            //       Query.NE("Status", "C2")
            //       );
            //query = Query.And(
            //     query,
            //     Query.NE("Delete", 1)
            //     );
            if (!String.IsNullOrEmpty(code))
            {
                query = Query.And(
                    query,
                    Query.EQ("Code", code)
                    );
            }
            else
            {
                if (!String.IsNullOrEmpty(customer_code))
                {
                    //query = Query.And(
                    //    query,
                    //    Query.EQ("CustomerCode", customer_code)
                    //    );
                    IMongoQuery _query = Query.Or(Query.EQ("_id", customer_code.Trim()), Query.EQ("general_email", customer_code.Trim()), Query.EQ("general_full_name", customer_code.Trim()), Query.EQ("contact_phone_mobile", customer_code.Trim()), Query.EQ("contact_phone_work", customer_code.Trim()));
                    dynamic[] lstCustomer = PayID.API.Process.MerchantProcess.Data.List("business_profile", _query);
                    if (lstCustomer != null)
                    {
                        string arrCustomer = "";
                        for (int i = 0; i <= lstCustomer.Length - 1; i++)
                        {
                            arrCustomer += lstCustomer[i]._id + ",";
                            //query = Query.And(query, Query.EQ("CustomerCode", lstCustomer[i].customercode));
                        }
                        arrCustomer = arrCustomer.Substring(0, arrCustomer.Length - 1);
                        query = Query.And(query, Query.In("CustomerCode", new BsonValue[] { arrCustomer }));

                    }
                    else
                    {
                        query = Query.And(
                            query,
                            Query.EQ("CustomerCode", customer_code)
                            );
                    }
                }

                if (!String.IsNullOrEmpty(to_province_code))
                {
                    if (!string.IsNullOrEmpty(unit_link))
                    {
                        if (unit_link.Length <= 2)
                        {
                            query = Query.And(
                       query,
                        Query.Or(Query.EQ("ToProvinceCode", int.Parse(to_province_code)), Query.EQ("ToProvinceCode", to_province_code), Query.EQ("FromProvinceCode", int.Parse(unit_link)), Query.EQ("FromProvinceCode", unit_link))
                       );

                        }
                        else
                        {
                            MongoDB.Bson.BsonRegularExpression reg = new MongoDB.Bson.BsonRegularExpression("^" + unit_link);
                            query = Query.And(
                       query,
                      Query.Or(Query.EQ("postcode_link", unit_link), Query.Matches("current_assigned", reg))
                       );
                        }

                        if (unit_link.Substring(0, 2) != "00")
                        {
                            IMongoQuery _query = Query.EQ("contact_address_province", unit_link.Substring(0, 2));
                            dynamic[] lstCustomer = PayID.API.Process.MerchantProcess.Data.List("business_profile", _query);
                            if (lstCustomer != null && lstCustomer.Length > 0)
                            {
                                string arrCustomer = "";
                                for (int i = 0; i <= lstCustomer.Length - 1; i++)
                                {
                                    arrCustomer += lstCustomer[i]._id + ",";
                                    //query = Query.And(query, Query.EQ("CustomerCode", lstCustomer[i].customercode));
                                }
                                arrCustomer = arrCustomer.Substring(0, arrCustomer.Length - 1);
                                query = Query.Or(Query.In("CustomerCode", new BsonValue[] { arrCustomer }), query);

                            }
                        }
                    }
                    else
                    {
                        query = Query.And(
                        query,
                        Query.EQ("ToProvinceCode", to_province_code)
                        );
                    }

                }
                else
                {
                    if (!string.IsNullOrEmpty(unit_link))
                    {
                        if (unit_link.Length <= 2)
                        {
                            query = Query.And(
                       query,
                        Query.Or(Query.EQ("ToProvinceCode", int.Parse(unit_link)), Query.EQ("FromProvinceCode", int.Parse(unit_link)), Query.EQ("ToProvinceCode", unit_link), Query.EQ("FromProvinceCode", unit_link))
                       );
                        }
                        else
                        {
                            query = Query.And(
                       query,
                       Query.EQ("postcode_link", unit_link)
                       );
                        }
                        if (unit_link.Substring(0, 2) != "00")
                        {
                            IMongoQuery _query = Query.EQ("contact_address_province", unit_link.Substring(0, 2));
                            dynamic[] lstCustomer = PayID.API.Process.MerchantProcess.Data.List("business_profile", _query);
                            if (lstCustomer != null)
                            {
                                string arrCustomer = "";
                                for (int i = 0; i <= lstCustomer.Length - 1; i++)
                                {
                                    arrCustomer += lstCustomer[i]._id + ",";
                                }
                                arrCustomer = arrCustomer.Substring(0, arrCustomer.Length - 1);
                                query = Query.Or(Query.In("CustomerCode", new BsonValue[] { arrCustomer }), query);

                            }
                        }
                    }
                }

                if (!String.IsNullOrEmpty(from_date))
                    query = Query.And(
                            query,
                            Query.GTE("DateCreated", DateTime.ParseExact(from_date, "dd-MM-yyyy", null))
                            );
                else
                    query = Query.And(
                           query,
                           Query.GTE("DateCreated", DateTime.ParseExact(date_default, "dd-MM-yyyy", null))
                           );

                if (!String.IsNullOrEmpty(to_date))
                    query = Query.And(
                            query,
                            Query.LTE("DateCreated", DateTime.ParseExact(to_date, "dd-MM-yyyy", null).AddDays(1))
                            );
                if (!String.IsNullOrEmpty(status))
                    query = Query.And(
                        query,
                        Query.EQ("Status", status)
                        );
            }
            PayID.DataHelper.DynamicObj[] ObjLading = PayID.Portal.Areas.Lading.Configuration.Data.List("Lading", query);

            JObject[] List_Lading = new JObject[] { };
            List_Lading = (from e in ObjLading select e).Select(c => JObject.Parse(JsonConvert.SerializeObject(c.ToExpando()))).ToArray();
            return List_Lading;
        }
        #endregion

        #region Hầm cập nhật vận đơn
        private static void lading_update(dynamic _request, out dynamic _response)
        {
            _response = new ExpandoObject();
            try
            {
                IMongoQuery query = Query.EQ("_id", _request._id);
                dynamic dyna = PayID.Portal.Areas.Lading.Configuration.Data.Get("Lading", query);
                dynamic dyna_old = new DynamicObj();
                var str = new
                {
                    Value = long.Parse(string.Format("{0}", _request.Value)),
                    Weight = long.Parse(string.Format("{0}", _request.Weight)),
                    Quantity = long.Parse(string.Format("{0}", _request.Quantity)),
                    ProductName = _request.ProductName,
                    ProductDescription = _request.ProductDescription,
                    ReceiverName = _request.ReceiverName,
                    ReceiverAddress = _request.ReceiverAddress,
                    ReceiverMobile = _request.ReceiverMobile,
                    ToProvinceCode = _request.ToProvinceCode,
                    MainFee = _request.MainFee,
                    ServiceFee = _request.ServiceFee,
                    CodFee = _request.CodFee,
                    TotalFee = _request.TotalFee
                };
                var str_old = new
                {
                    Value = long.Parse(string.Format("{0}", _request.Value_old)),
                    Weight = long.Parse(string.Format("{0}", _request.Weight_old)),
                    Quantity = long.Parse(string.Format("{0}", _request.Quantity_old)),
                    ProductName = _request.ProductName_old,
                    ProductDescription = _request.ProductDescription_old,
                    ReceiverName = _request.ReceiverName_old,
                    ReceiverAddress = _request.ReceiverAddress_old,
                    ReceiverMobile = _request.ReceiverMobile_old,
                    ToProvinceCode = _request.ToProvinceCode_old,
                    MainFee = _request.MainFee_old,
                    ServiceFee = _request.ServiceFee_old,
                    CodFee = _request.CodFee_old,
                    TotalFee = _request.TotalFee_old
                };
                try
                {
                    if (Security.CreatPassWordHash(str.ToString()) != Security.CreatPassWordHash(str_old.ToString()))
                    {
                        dyna.Value = long.Parse(_request.Value);
                        dyna.Weight = long.Parse(_request.Weight);
                        dyna.Quantity = long.Parse(_request.Quantity);
                        dyna.ProductName = _request.ProductName;
                        dyna.ProductDescription = _request.ProductDescription;
                        dyna.ReceiverName = _request.ReceiverName;
                        dyna.ReceiverAddress = _request.ReceiverAddress;
                        dyna.ReceiverMobile = _request.ReceiverMobile;
                        dyna.ToProvinceCode = _request.ToProvinceCode;
                        dyna.MainFee = long.Parse(string.Format("{0}", _request.MainFee));
                        dyna.ServiceFee = long.Parse(string.Format("{0}", _request.ServiceFee));
                        dyna.CodFee = long.Parse(string.Format("{0}", _request.CodFee));
                        dyna.TotalFee = long.Parse(string.Format("{0}", _request.TotalFee));

                        string sDate = DateTime.Now.ToString("yyyyMMdd");
                        dyna_old._id = dyna.Code + sDate + PayID.Portal.Areas.Lading.Configuration.Data.GetNextSquence("LogChangeLading");

                        dyna_old.Code = dyna.Code;
                        dyna_old.Value = long.Parse(_request.Value);
                        dyna_old.Weight = long.Parse(_request.Weight);
                        dyna_old.Quantity = long.Parse(_request.Quantity);
                        dyna_old.ProductName = _request.ProductName;
                        dyna_old.ProductDescription = _request.ProductDescription;
                        dyna_old.ReceiverName = _request.ReceiverName;
                        dyna_old.ReceiverAddress = _request.ReceiverAddress;
                        dyna_old.ReceiverMobile = _request.ReceiverMobile;
                        dyna_old.ToProvinceCode = _request.ToProvinceCode;
                        dyna_old.MainFee = long.Parse(string.Format("{0}", _request.MainFee));
                        dyna_old.ServiceFee = long.Parse(string.Format("{0}", _request.ServiceFee));
                        dyna_old.CodFee = long.Parse(string.Format("{0}", _request.CodFee));
                        dyna_old.TotalFee = long.Parse(string.Format("{0}", _request.TotalFee));

                        dyna_old.Value_old = long.Parse(_request.Value_old);
                        dyna_old.Weight_old = long.Parse(_request.Weight_old);
                        dyna_old.Quantity_old = long.Parse(_request.Quantity_old);
                        dyna_old.ProductName_old = _request.ProductName_old;
                        dyna_old.ProductDescription_old = _request.ProductDescription_old;
                        dyna_old.ReceiverName_old = _request.ReceiverName_old;
                        dyna_old.ReceiverAddress_old = _request.ReceiverAddress_old;
                        dyna_old.ReceiverMobile_old = _request.ReceiverMobile_old;
                        dyna_old.ToProvinceCode_old = _request.ToProvinceCode_old;
                        dyna_old.MainFee_old = long.Parse(string.Format("{0}", _request.MainFee_old));
                        dyna_old.ServiceFee_old = long.Parse(string.Format("{0}", _request.ServiceFee_old));
                        dyna_old.CodFee_old = long.Parse(string.Format("{0}", _request.CodFee_old));
                        dyna_old.TotalFee_old = long.Parse(string.Format("{0}", _request.TotalFee_old));
                        PayID.Portal.Areas.Lading.Configuration.Data.Save("Lading", dyna);
                        PayID.Portal.Areas.Lading.Configuration.Data.Save("LogChangeLading", dyna_old);

                        dynamic _dynaShipment = new DynamicObj();
                        _dynaShipment = PayID.Portal.Areas.Lading.Configuration.Data.Get("shipment", Query.EQ("tracking_code", dyna.Code));
                        if (_dynaShipment != null)
                        {
                            if (_dynaShipment.product != null)
                            {
                                _dynaShipment.product.value = long.Parse(_request.Value);
                                _dynaShipment.product.weight = long.Parse(_request.Weight);
                                _dynaShipment.product.quantity = long.Parse(_request.Quantity);
                                _dynaShipment.product.name = _request.ProductName;
                                _dynaShipment.product.description = _request.ProductDescription;
                            }
                            if (_dynaShipment.parcel != null)
                            {
                                _dynaShipment.parcel.weight = long.Parse(_request.Weight);
                            }
                            _dynaShipment.to_address.name = _request.ReceiverName;
                            _dynaShipment.to_address.address = _request.ReceiverAddress;
                            _dynaShipment.to_address.phone = _request.ReceiverMobile;
                            _dynaShipment.to_address.province = _request.ToProvinceCode;
                            //Lưu thong tin thay doi khi sua van don
                            PayID.Portal.Areas.Lading.Configuration.Data.Save("shipment", _dynaShipment);
                        }

                    }
                }
                catch
                {
                }

                _response.response_code = "00";
                _response.response_message = "Cập nhật thành công";
            }
            catch
            {
                _response.response_code = "90";
                _response.response_message = "Có lỗi trong quá trình xử lý. Vui lòng thử lại sau";
            }
        }
        #endregion

        #region Hàm tạo vận đơn
        private static void lading_create(dynamic _request, out dynamic _response)
        {
            _response = new ExpandoObject();
            ICalculateRate cal = new CalculateRate();

            // tao ma van don
            MetadataController md = new MetadataController();
            //_request.Code = md.CreateAutoGenCode(_request.CustomerCode.ToString(), _request.Type, _request.FromProvinceCode.ToString());
            try
            {
                ProxyController px = new ProxyController();
                dynamic dynamicObj = new DynamicObj();
                _request._id = PayID.Portal.Areas.Lading.Configuration.Data.GetNextSquence("LogCreateLading");
                if (string.IsNullOrEmpty(_request.Weight.ToString()))
                    _request.Weight = 50;
                try
                {
                    // Tính phí BCCP
                    //dynamicObj = px.processChargesInfo(int.Parse(_request.Weight.ToString()), double.Parse(_request.Value.ToString()), _request.ServiceCode.ToString(), _request.FromProvinceCode.ToString(), _request.ToProvinceCode.ToString());

                    //dynamicObj = dynamicObj.Output[0];
                    //_request.MainFee = long.Parse(dynamicObj.MainCharges.ToString());
                    //_request.ServiceFee = long.Parse(dynamicObj.PlusServiceCharges.ToString());
                    //_request.CodFee = long.Parse(dynamicObj.CodCharges.ToString());
                    //_request.TotalFee = long.Parse(dynamicObj.CodCharges.ToString()) + long.Parse(_request.Value.ToString());
                    _request.DateCreated = DateTime.Now;
                    if (_request.Code == null)
                    {
                        _request.Code = md.CreateAutoGenCode(_request.CustomerCode.ToString(), _request.Type, _request.FromProvinceCode.ToString());
                    }
                    // Trả về JOBJECT
                    _response.Code = _request.Code;
                    _response.MainFee = long.Parse(_request.MainFee.ToString());
                    _response.ServiceFee = long.Parse(_request.ServiceFee.ToString());
                    _response.CodFee = long.Parse(_request.CodFee.ToString());
                    _response.TotalFee = long.Parse(_request.TotalFee.ToString()) + long.Parse(_request.Value.ToString());
                    _response.response_code = "00";
                    _response.response_message = "Cập nhật thành công";
                }
                catch
                {
                    //Charges[] mangphi = cal.CalutaltorFee(string.IsNullOrEmpty(_request.ServiceCode.ToString()) ? "" : _request.ServiceCode.ToString(), long.Parse(_request.Value.ToString()), _request.FromProvinceCode.ToString(), "VN", _request.ToProvinceCode.ToString(), "VN", long.Parse(_request.Weight.ToString()), 0);
                    //_request.MainFee = long.Parse(mangphi[0].MainFee.ToString());
                    //_request.ServiceFee = long.Parse(mangphi[0].ServiceFee.ToString());
                    //_request.CodFee = long.Parse(mangphi[0].CodFee.ToString());
                    //_request.TotalFee = long.Parse(mangphi[0].CodFee.ToString()) + long.Parse(_request.Value.ToString());
                    //_request.DateCreated = DateTime.Now;
                    //_request.Code = md.CreateAutoGenCode(_request.CustomerCode.ToString(), _request.Type, _request.FromProvinceCode.ToString());

                    //// Trả về JOBJECT
                    //_response.Code = _request.Code;
                    //_response.MainFee = long.Parse(mangphi[0].MainFee.ToString());
                    //_response.ServiceFee = long.Parse(mangphi[0].ServiceFee.ToString());
                    //_response.CodFee = long.Parse(mangphi[0].CodFee.ToString());
                    //_response.TotalFee = long.Parse(mangphi[0].CodFee.ToString()) + long.Parse(_request.Value.ToString());
                }

                try
                {
                    // InsertLogLading(_request);
                    PayID.Portal.Areas.Lading.Configuration.Data.Insert("LogCreateLading", _request);
                    dynamic _jouey = PayID.Portal.Areas.Lading.Configuration.Data.Get("LogJourney", Query.And(Query.EQ("Code", _request.Code), Query.EQ("Status", _request.Status.ToString())));
                    if (_jouey == null)
                    {
                        dynamic objLogJourney = new DynamicObj();
                        objLogJourney._id = PayID.Portal.Areas.Lading.Configuration.Data.GetNextSquence("LogJourney");
                        objLogJourney.Code = _request.Code;
                        objLogJourney.Status = _request.Status;
                        objLogJourney.UserId = _request.CustomerCode.ToString();
                        objLogJourney.Location = "Cast@Post";
                        if (_request.Status == "C1")
                        {
                            objLogJourney.Note = "Yêu cầu mới";
                        }
                        else
                        {
                            //dynamic _status = PayID.Portal.Areas.Lading.Configuration.Data.Get("Status", Query.EQ("StatusCode", _request.Status.ToString()));
                            objLogJourney.Note = "";
                        }
                        objLogJourney.DateCreate = DateTime.Now;
                        PayID.Portal.Areas.Lading.Configuration.Data.Insert("LogJourney", objLogJourney);
                    }
                }
                catch
                {
                    _response.response_code = "01";
                    _response.response_message = "Có lỗi trong quá trình xử lý. Vui lòng thử lại sau";
                }
                finally
                {
                    // InsertLading(_request);
                    PayID.Portal.Areas.Lading.Configuration.Data.Insert("Lading", _request);
                }
            }
            catch
            {
                _response.response_code = "90";
                _response.response_message = "Có lỗi trong quá trình kết nối, hàm xử lý nội bộ.";
            }

            // Hàm này phải xử lý lại, chưa ổn. 02/10/2014
        }
        #endregion
        private void InsertLogLading(dynamic insertLogLading)
        {
            PayID.Portal.Areas.Lading.Configuration.Data.Insert("LogCreateLading", insertLogLading);
        }
        private void InsertLading(dynamic insertLading)
        {
            PayID.Portal.Areas.Lading.Configuration.Data.Insert("Lading", insertLading);
        }
    }
}
