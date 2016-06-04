using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PayID.DataHelper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace PayID.API.Controllers
{
    // - : C1,
    // --> : Mới tạo
    // - : C2,
    // --> : Hủy
    // - : C3,
    // --> : Đã thanh toán
    // - : C4,
    // --> : Chờ thanh toán
    // - : C5,
    // --> : Chấp nhận
    // - : C6,
    // --> : Đang lấy hàng
    // - : C7,
    // --> : Lấy hàng thành công
    // - : C8,
    // --> : Hủy lấy hàng
    // - : C9,
    // --> : Lấy hàng thất bại
    // - : C10,
    // --> : Khách hàng hẹn lại
    // - : C11,
    // --> : Đang vận chuyển
    // - : C12,
    // --> : Chờ phát
    // - : C13,
    // --> : Giao thành công
    // - : C14,
    // --> : Giao thất bại
    // - : C15,
    // --> : Chuyển hoàn
    // - : C16,
    // --> : Đã thu tiền
    // - : C17,
    // --> : Đã trả tiền
    // - : C18,
    // --> : Phát hoàn
    // - : C19,
    // --> : Chuyển tiếp
    // - : C20,
    // --> : Rút bưu gửi
    // - : C21,
    // --> : Vô thừa nhận
    // - : C22,
    // --> : Đang phát
    // - : C23,
    // --> : Phát tại địa chỉ
    // - : C24,
    // --> : Phát tại quầy
    // - : C27,
    // --> : Phát hoàn thành công
    // - : C25,
    // --> : Đang thu hồi
    // - : C26,
    // --> : Thu hồi thành công
    // http://localhost:24799/api/ServiceRequest?function=innerShipment&api_key=545397418bb45a4589065bb4
    public class ServiceRequestController : ApiController
    {
        [AcceptVerbs("POST")]
        public JObject Post(string function, string api_key, JObject data)
        {
            //Chuyển yêu cầu dạng JObject sang dynamic
            //dynamic _request = new PayID.DataHelper.DynamicObj(
            //    MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(JsonConvert.SerializeObject(data)));
            if (api_key == null)
            {
                return JObject.Parse(@"{error_code:'99',error_message:'Invalid API Key'}");
            }
            dynamic _api_key = PayID.Portal.Areas.Systems.Configuration.Data.Get("api_key", Query.EQ("_id", new ObjectId(api_key)));
            if (_api_key == null)
                return JObject.Parse(@"{error_code:'99',error_message:'Invalid API Key'}");

            dynamic business_profile = PayID.Portal.Areas.Merchant.Configuration.Data.Get("business_profile", Query.EQ("_id", _api_key.business_profile.ToString()));
            if (business_profile.active == null || !business_profile.active)
                return JObject.Parse(@"{error_code:'98',error_message:'Merchant Account has been locked!'}");

            //BsonDocument _data = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(data.ToString());
            JObject _data = JObject.Parse(data.ToString());
            dynamic _request = _data;

            // code from api_key
            _request.apicode = _api_key.code.ToString() + " api";
            //dynamic _req = new DynamicObj();
            if (_request.tracking_code == null || string.IsNullOrEmpty(_request.tracking_code.ToString()))
            {
                return JObject.Parse(@"{"
                      + "error_code:'09'"
                      + ",error_message:'Xử lý thất bại. Mã vận đơn hoặc mã đơn hàng bị rỗng! '"
                      + ",tracking_code: '" + _request.tracking_code + "'"
                      + ",request_code: '" + _request._id + "'"
                      + ",request_id: '" + _request.request_id + "'"
                      + ",order_id: '" + _request.order_id + "'"
                  + "}");
            }

            dynamic _requestToLog = data;
            _requestToLog.type = "REQUEST";
            _requestToLog.module = "SERVICE_REQUEST";
            _requestToLog.function = function;
            _requestToLog.api_key = api_key;
            _requestToLog._id = Guid.NewGuid().ToString();

            Log.Logs.Add(_requestToLog);

            _request.is_mapping = _api_key.is_mapping;
            dynamic _logRequest = _request;

            //Khởi tạo kết quả trả ra với mã lỗi mặc định
            dynamic _response = new ExpandoObject();
            _response.error_code = "96";
            _response.error_message = "Lỗi xử lý hệ thống";

            string _json_response = String.Empty;
            try
            {
                //Nếu trong yêu cầu không có tham số function
                if (String.IsNullOrEmpty(function))
                {
                    _response.error_code = "90";
                    _response.error_message = "Sai tham số kết nối";
                    _json_response = JsonConvert.SerializeObject(_response);

                    dynamic _responseToLog = JObject.Parse(_json_response);
                    _responseToLog.request_id = _requestToLog._id;
                    _responseToLog._id = Guid.NewGuid().ToString();
                    _responseToLog.type = "RESP";
                    _responseToLog.module = "SERVICE_REQUEST";
                    _responseToLog.function = function;
                    _responseToLog.api_key = api_key;
                    Log.Logs.Add(_responseToLog);

                    return JObject.Parse(_json_response);
                }
                switch (function)
                {
                    case "shipment":
                        _request.customer = JObject.Parse(@"{"
                + "code:'" + business_profile._id + "',"
                + "email:'" + business_profile.general_email + "',"
                + "full_name:'" + business_profile.general_full_name + "'"
                + "}");
                        if (_request.from_address._id == null | String.IsNullOrEmpty(_request.from_address._id.ToString()))
                            if (_api_key.default_store != null && !String.IsNullOrEmpty(_api_key.default_store))
                                _request.from_address._id = _api_key.default_store;
                        _response = shipment(_request);
                        break;
                    case "tracking":
                        _response = tracking(_request);
                        break;
                    case "return":
                        _response = return_shipment(_request);
                        break;
                    case "address":
                        _response = address(_request);
                        break;
                    case "info":
                        _response = info(_request);
                        break;
                    default:
                        _response.error_code = "91";
                        _response.error_message = "Yêu cầu không được hỗ trợ";
                        break;
                }
            }
            catch (Exception ex)
            {
                _response.error_code = "09";
                _response.error_message = "Xử lý thất bại." + ex.Message;
            }
            _json_response = JsonConvert.SerializeObject(_response);

            dynamic _responseToLogFinal = JObject.Parse(_json_response);
            _responseToLogFinal.request_id = _requestToLog._id;
            _responseToLogFinal._id = Guid.NewGuid().ToString();
            _responseToLogFinal.type = "RESP";
            _responseToLogFinal.module = "SERVICE_REQUEST";
            _responseToLogFinal.function = function;
            _responseToLogFinal.api_key = api_key;
            Log.Logs.Add(_responseToLogFinal);

            return JObject.Parse(_json_response);
        }

        private dynamic address(dynamic _request)
        {
            dynamic _response = new JObject();

            dynamic myObj = new DynamicObj();
            try
            {
                myObj._id = PayID.Portal.Areas.Merchant.Configuration.Data.GetNextSquence("profile_store");

                string _cust = _request.customer_code;
                string _addr = _request.store_to_address;
                string _store_name = _request.store_name;
                string _mn_name = _request.store_manager_name;
                string _mn_mobile = _request.store_manager_mobile;
                string _mn_email = _request.store_manager_email;
                string _mn_province = _request.province_code;
                string _mn_district = _request.district_code;
                string _mn_postcode = _request.postcode;

                myObj.StoreCode = _cust + PayID.Portal.Areas.Merchant.Configuration.Data.GetNextSquence("profile_store" + _cust).ToString("000");
                myObj.UserId = _cust;
                myObj.StoreName = _store_name;
                myObj.Address = _addr;
                myObj.ManagerName = _mn_name;
                myObj.ManagerMobile = _mn_mobile;
                myObj.ManagerEmail = _mn_email;
                myObj.ProvinceCode = _mn_province;
                myObj.DistrictCode = _mn_district;
                myObj.PostCode = _mn_postcode;
                myObj.Default = 1;
                PayID.Portal.Areas.ServiceRequest.Configuration.Data.Save("profile_store", myObj);

                _response.error_code = "00";
                _response.error_message = "Success";
                _response.store_code = myObj.StoreCode;

            }
            catch (Exception ex)
            {
                _response.error_code = "09";
                _response.error_message = "Xử lý thất bại. " + ex.Message.ToString();
                _response.store_code = "";
            }
            return _response;
        }
        private dynamic return_shipment(dynamic _request)
        {
            dynamic _response = new JObject();
            dynamic _shipment = new PayID.DataHelper.DynamicObj();
            _response.error_code = "01";
            _response.error_message = "Thông tin không hợp lệ";
            if (_request.tracking_code != null)
            {
                try
                {
                    _shipment = PayID.Portal.Areas.ServiceRequest.Configuration.Data.Get("shipment", Query.Or(Query.EQ("tracking_code", _request.tracking_code.ToString()), Query.EQ("RefCode", _request.tracking_code.ToString())));
                }
                catch
                {
                    _shipment = null;
                }
            }

            if (_shipment == null && _request.order_id != null)
            {
                try
                {
                    _shipment = PayID.Portal.Areas.ServiceRequest.Configuration.Data.Get("shipment", Query.EQ("order_id", _request.order_id.ToString()));
                }
                catch
                {
                    _shipment = null;
                }
            }

            if (_shipment == null)
                return _response;
            string new_status = "C15";
            if (_shipment.system_status == "C1" || _shipment.system_status == "C5" || _shipment.system_status == "C6") new_status = "C8";
            else
            {
                _response.error_code = "13";
                _response.error_message = "Đã thu gom";
                return _response;
            }
            PayID.Portal.Areas.ServiceRequest.Configuration.Data.UpdateObject("shipment", Query.EQ("tracking_code", _request.tracking_code.ToString()), Update.Set("system_status", new_status));
            try
            {
                //save change to log Journey
                dynamic _jouey = PayID.Portal.Areas.Lading.Configuration.Data.Get("LogJourney", Query.And(Query.EQ("Code", _request.Code), Query.EQ("Status", "C1")));
                if (_jouey == null)
                {
                    dynamic objLogJourney = new DynamicObj();
                    objLogJourney._id = PayID.Portal.Areas.Lading.Configuration.Data.GetNextSquence("LogJourney");
                    objLogJourney.Code = _request.Code;
                    objLogJourney.Status = new_status;
                    objLogJourney.UserId = _request.CustomerCode.ToString();
                    objLogJourney.Location = "Cast@Post";
                    objLogJourney.Note = _request.apicode;
                    objLogJourney.DateCreate = DateTime.Now;
                    PayID.Portal.Areas.Lading.Configuration.Data.Save("LogJourney", objLogJourney);
                }
            }
            catch
            {

            }
            _shipment.error_code = "00";
            _shipment.error_message = "Thông tin hợp lệ";
            return JObject.Parse(
                _shipment.ToBsonDocument().ToString());
        }
        private dynamic info(dynamic _request)
        {
            dynamic _response = new JObject();
            dynamic _shipment = new PayID.DataHelper.DynamicObj();
            _response.error_code = "01";
            _response.error_message = "Thông tin không hợp lệ";
            if (_request._id != null)
            {
                _shipment = PayID.Portal.Areas.ServiceRequest.Configuration.Data.Get("shipment", Query.EQ("_id", _request._id.ToString()));
            }
            if (_request.tracking_code != null)
            {
                try
                {
                    _shipment = PayID.Portal.Areas.ServiceRequest.Configuration.Data.Get("shipment", Query.Or(Query.EQ("tracking_code", _request.tracking_code.ToString()), Query.EQ("RefCode", _request.tracking_code.ToString())));
                }
                catch
                {
                    _shipment = null;
                }
            }
            if (_request.order_code != null)
            {
                _shipment = PayID.Portal.Areas.ServiceRequest.Configuration.Data.Get("shipment", Query.EQ("order_code", _request.order_code.ToString()));
            }
            if (_request.request_code != null)
            {
                _shipment = PayID.Portal.Areas.ServiceRequest.Configuration.Data.Get("shipment", Query.EQ("request_id", _request.request_code.ToString()));
            }
            if (_shipment == null)
                return _response;
            _shipment.error_code = "00";
            _shipment.error_message = "Thông tin hợp lệ";
            return JObject.Parse(
                _shipment.ToBsonDocument().ToString())
                ;

        }
        private dynamic tracking(dynamic _request)
        {
            dynamic _response = new JObject();
            bool has_detail = false;
            dynamic _shipment = new PayID.DataHelper.DynamicObj();
            dynamic _info = new PayID.DataHelper.DynamicObj();
            dynamic[] _traces = new PayID.DataHelper.DynamicObj[] { };
            if (_request.tracking_code != null)
            {
                try
                {
                    _shipment = PayID.Portal.Areas.ServiceRequest.Configuration.Data.Get("shipment", Query.Or(Query.EQ("tracking_code", _request.tracking_code.ToString()), Query.EQ("RefCode", _request.tracking_code.ToString())));
                }
                catch
                {
                    _shipment = null;
                }
            }
            if (_shipment == null && _request.order_code != null)
            {
                _shipment = PayID.Portal.Areas.ServiceRequest.Configuration.Data.Get("shipment", Query.EQ("order_code", _request.order_code.ToString()));
            }
            if (_shipment == null && _request.request_code != null)
            {
                _shipment = PayID.Portal.Areas.ServiceRequest.Configuration.Data.Get("shipment", Query.EQ("request_id", _request.request_code.ToString()));
            }
            if (_request.has_detail != null)
            {
                if (!string.IsNullOrEmpty(_request.has_detail.ToString()) && _request.has_detail.ToString() == "Y")
                {
                    has_detail = true;
                }
            }


            if (_shipment == null)
                return JObject.Parse(@"{error_code:'03',error_message:'Invalide Tracking or Request Code'}");

            _info = PayID.Portal.Areas.ServiceRequest.Configuration.Data.Get("lading_info", Query.EQ("_id", _shipment.tracking_code.ToString()));
            _traces = PayID.Portal.Areas.ServiceRequest.Configuration.Data.List("shipment_trace", Query.EQ("tracking_code", _shipment.tracking_code.ToString()));
            if (_traces != null && _traces.Length > 0)
            {
                _traces = (from e in _traces select e).OrderBy(t => (dynamic)t._id).ToArray();
            }
            _response.error_code = "00";
            _response.error_message = "Success";
            _response.tracking_code = _shipment.tracking_code;
            _response.request_code = _shipment._id;
            _response.request_id = _shipment.request_id;
            _response.order_id = _shipment.order_id;
            _response.tracking_request_id = _shipment.customer.code + DateTime.Now.ToString("yyyyMMddHHmmss");
            _response.status = _shipment.system_status.ToLower();
            if (_shipment.parcel != null)
                _response.parcel = JObject.Parse(_shipment.parcel.ToBsonDocument().ToString());
            else
                _response.parcel = null;

            if (_info == null)
            {
                _info = new PayID.DataHelper.DynamicObj();
                _info.freight = 0;
                _info.cod_freight = 0;
            }

            _response.fee = JObject.Parse(@"{"
                + "main_fee: " + long.Parse("0" + _info.freight.ToString())
                + ",service_fee: 0"
                + ",cod_fee: " + long.Parse("0" + _info.cod_freight.ToString())
                + "}");
            List<JObject> _tracking_details = new List<JObject>();
            foreach (dynamic _trace in _traces)
            {
                dynamic _t = new DataHelper.DynamicObj();
                _t.datetime = _trace.trace_date.ToString("yyyyMMddHHmmss");
                _t.message = _trace.description;
                _t.status = _trace.pos;
                _t.code = _trace.code;
                _tracking_details.Add(JObject.Parse(_t.ToBsonDocument().ToString()));
                _response.status = _trace.code;
            }
            if (has_detail)
                _response.tracking_details = new JArray(_tracking_details.ToArray());
            return _response;
        }
        //Push data to return status tracking code - Hoang Anh
        [Route("api/ServiceRequest/trackingsystem")]
        [AcceptVerbs("GET")]
        public JObject trackingsystem(string api_key, string orderlist)
        {
            JObject[] List_Lading = new JObject[] { };
            if (api_key == null)
            {
                return JObject.Parse(@"{error_code:'99',error_message:'Invalid API Key'}"); ;
            }
            dynamic _api_key = PayID.Portal.Areas.Systems.Configuration.Data.Get("api_key", Query.EQ("_id", new ObjectId(api_key)));
            if (_api_key == null)
            {
                return JObject.Parse(@"{error_code:'99',error_message:'Invalid API Key'}"); ;
            }

            string _json_response = String.Empty;
            dynamic _response = new ExpandoObject();
            _response.response_code = "96";
            _response.response_message = "Lỗi xử lý hệ thống";

            dynamic _shipment = new PayID.DataHelper.DynamicObj();
            dynamic _info = new PayID.DataHelper.DynamicObj();
            dynamic[] _traces = new PayID.DataHelper.DynamicObj[] { };
            string dateSpec = ConfigurationSettings.AppSettings.Get("step");
            try
            {
                DateTime date = DateTime.Now;
                int expTime = 0;

                if (!string.IsNullOrEmpty(dateSpec))
                {
                    date = DateTime.Now.AddDays(int.Parse(dateSpec));
                }

                expTime = int.Parse(date.ToString("yyyyMMdd"));
                string arrStatus = string.Empty;
                try
                {
                    if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings.Get("status")))
                    {
                        arrStatus = ConfigurationSettings.AppSettings.Get("status");
                    }
                    else
                    {
                        arrStatus = "";
                    }
                }
                catch
                {
                    arrStatus = "";
                }

                string[] authorize_Status = null;
                string[] orderlst = null;
                if (!string.IsNullOrEmpty(orderlist))
                {
                    if (arrStatus.Trim().Contains(';'))
                    {
                        orderlst = orderlist.Trim().Split(';');
                    }
                    else
                    {
                        orderlst[0] = orderlist;
                    }
                }
                else
                {
                    _response.response_code = "13";
                    _response.response_message = "Không tìm thấy dữ liệu thu gom phù hợp";
                    _json_response = JsonConvert.SerializeObject(_response);

                    return JObject.Parse(_json_response);
                }
                if (!string.IsNullOrEmpty(arrStatus))
                {
                    if (arrStatus.Trim().Contains(';'))
                    {
                        authorize_Status = arrStatus.Trim().Split(';');
                    }
                    else
                    {
                        authorize_Status[0] = arrStatus;
                    }
                }

                //IMongoQuery query = Query.EQ("system_time_key.date", expTime);
                IMongoQuery query = Query.NE("_id", "");
                IMongoQuery queryLog = Query.EQ("system_time_key.date", expTime);
                query = Query.And(query, Query.EQ("partnercode", api_key));

                int time = int.Parse(date.ToString("HHmmss"));
                //if(time>=120000)
                //{
                //    query = Query.And(query, Query.GTE("system_time_key.time", 120000));
                //    queryLog = Query.And(queryLog, Query.GTE("system_time_key.time", 120000));
                //}

                IMongoQuery _queryOr = Query.NE("_id", 0);
                if (authorize_Status != null && authorize_Status.Length > 0)
                {
                    var documentIds = new BsonValue[authorize_Status.Length];

                    for (int j = 0; j < authorize_Status.Length; j++)
                    {
                        documentIds[j] = authorize_Status[j];
                    }

                    _queryOr = Query.In("Status", documentIds);
                    query = Query.And(query, Query.In("system_status", documentIds));
                }

                if (orderlst != null && orderlst.Length > 0)
                {
                    var documentorders = new BsonValue[orderlst.Length];

                    for (int k = 0; k < orderlst.Length; k++)
                    {
                        documentorders[k] = orderlst[k];
                    }
                    query = Query.And(query, Query.In("order_id", documentorders));
                }
                DynamicObj[] dyna = null;
                List<DynamicObj> _listResult = new List<DynamicObj>();
                dynamic[] _listJourney = null;
                dynamic _ite = new DynamicObj();
                if (PayID.Portal.Areas.ServiceRequest.Configuration.Data.List("shipment", query) != null && PayID.Portal.Areas.ServiceRequest.Configuration.Data.List("shipment", query).Length > 0)
                {
                    dyna = new DynamicObj[PayID.Portal.Areas.ServiceRequest.Configuration.Data.List("shipment", query).Length];

                    dyna = PayID.Portal.Areas.ServiceRequest.Configuration.Data.List("shipment", query);

                    foreach (dynamic c in dyna)
                    {
                        _listJourney = PayID.Portal.Areas.ServiceRequest.Configuration.Data.List("LogJourney", Query.And(Query.EQ("Code", c.tracking_code == null ? "" : c.tracking_code.ToString()), queryLog, _queryOr));
                        if (_listJourney != null && _listJourney.Length > 0)
                        {
                            foreach (dynamic _dynaJourney in _listJourney)
                            {
                                _ite = new DynamicObj();
                                _ite.CustomerCode = c.customer.code;
                                if (c.tracking_code != null)
                                {
                                    _ite.Code = c.tracking_code;
                                }
                                _ite.DateCreated = DateTime.ParseExact(c.created_at.ToString(), "yyyyMMddHHmmss", null);
                                if (c.quantity != null)
                                {
                                    _ite.Quantity = c.quantity;
                                }
                                _ite.Value = long.Parse(c.product.value.ToString());
                                _ite.Status = _dynaJourney.Status;
                                _ite.Note = _dynaJourney.Note;
                                _ite.OrderId = c.order_id;
                                _ite.Weight = c.parcel.weight;
                                _ite.Height = 0;
                                _ite.Long = 0;
                                _ite.Width = 0;
                                _ite.ServiceCode = c.service.cashpost_service;

                                _listResult.Add(_ite);
                            }
                        }
                        else
                        {
                            _ite = new DynamicObj();
                            _ite.CustomerCode = c.customer.code;
                            if (c.tracking_code != null)
                            {
                                _ite.Code = c.tracking_code;
                            }
                            if (c.quantity != null)
                            {
                                _ite.Quantity = c.quantity;
                            }
                            _ite.Weight = c.parcel.weight;
                            _ite.Height = 0;
                            _ite.Long = 0;
                            _ite.Width = 0;
                            _ite.DateCreated = DateTime.ParseExact(c.created_at.ToString(), "yyyyMMddHHmmss", null);
                            _ite.Value = long.Parse(c.product.value.ToString());
                            _ite.Status = c.system_status;
                            _ite.OrderId = c.order_id;
                            _ite.ServiceCode = c.service.cashpost_service;
                            _listResult.Add(_ite);
                        }
                    }

                    PayID.DataHelper.DynamicObj[] ObjLading = _listResult.ToArray();

                    List_Lading = (from e in ObjLading select e).Select(c => JObject.Parse(JsonConvert.SerializeObject(c.ToExpando()))).ToArray();

                    if (List_Lading != null && List_Lading.Length > 0)
                    {
                        _response.response_code = "00";
                        _response.response_message = "Truy vấn thành công";
                        _response.ListLading = List_Lading;

                        _json_response = JsonConvert.SerializeObject(_response);

                    }
                    else
                    {
                        _response.response_code = "13";
                        _response.response_message = "Không tìm thấy dữ liệu thu gom phù hợp";

                        _json_response = JsonConvert.SerializeObject(_response);
                    }

                    return JObject.Parse(_json_response);
                }

                _response.response_code = "13";
                _response.response_message = "Không tìm thấy dữ liệu thu gom phù hợp";
                _json_response = JsonConvert.SerializeObject(_response);

                return JObject.Parse(_json_response);
            }
            catch (Exception ex)
            {
                return JObject.Parse(@"{error_code:'09',error_message:'Xử lý thất bại. Lỗi phát sinh từ hàm xử lý!'}"); ;
            }

        }
        #region shipment
        private dynamic shipment(dynamic request)
        {
            string unit_code, unit_name, unit_link;
            try
            {
                dynamic from_address = request.from_address;

                if (!String.IsNullOrEmpty(request.tracking_code.ToString()) || !String.IsNullOrEmpty(request.order_id.ToString()))
                {
                    if (from_address._id != null && !String.IsNullOrEmpty(from_address._id.ToString()))
                    {
                        dynamic store = PayID.Portal.Areas.Merchant.Configuration.Data.Get("profile_store",
                            Query.And(
                            Query.EQ("StoreCode", from_address._id.ToString()),
                            Query.EQ("UserId", request.customer.code.ToString())
                            )
                            );
                        if (store != null)
                        {
                            unit_code = store.ProvinceCode;
                            unit_link = store.ProvinceCode;
                            unit_name = PayID.Portal.Areas.Metadata.Controllers.ProxyController.GetProvinceName(unit_code).Trim();
                            if (store.PostCode != null)
                            {
                                dynamic postCode = PayID.Portal.Areas.Metadata.Configuration.Data.Get("mbcPos", Query.EQ("_id", store.PostCode));
                                unit_code = store.PostCode;
                                unit_link = postCode.ProvinceCode + "." + postCode.UnitCode + "." + store.PostCode;
                                unit_name = postCode.POSName.Trim();
                                request.from_address.address = store.Address;
                                request.from_address.name = store.StoreName;
                            }
                        }
                        else
                        {
                            return JObject.Parse(@"{error_code:'10',error_message:'Invalid From Address ID'}");
                        }
                    }
                    else
                    {
                        if (from_address.province == null)
                        {
                            return JObject.Parse(@"{error_code:'11',error_message:'Invalid From Address Province Code'}");
                        }
                        if ((bool)request.is_mapping)
                        {
                            from_address.province = PayID.Portal.Areas.Merchant.Configuration.Data.Get("partner_mapping_province",
                                Query.And(Query.EQ("Id", long.Parse(from_address.province.ToString())),
                                Query.EQ("Partner", long.Parse(request.customer.code.ToString()))
                                )).ProvinceCode;

                            request.to_address.province = PayID.Portal.Areas.Merchant.Configuration.Data.Get("partner_mapping_province",
                             Query.And(Query.EQ("Id", long.Parse(request.to_address.province.ToString())),
                             Query.EQ("Partner", long.Parse(request.customer.code.ToString()))
                             )).ProvinceCode;
                        }
                        dynamic province = PayID.Portal.Areas.Metadata.Controllers.ProxyController.GetProvinceName(from_address.province.ToString());
                        if (province == null) return JObject.Parse(@"{error_code:'11',error_message:'Invalid From Address Province Code'}");
                        request.from_address.province = from_address.province;
                        unit_code = from_address.province.ToString();
                        unit_link = unit_code;
                        unit_name = province;
                    }

                    string prefix = DateTime.Today.ToString("yy") + DateTime.Today.DayOfYear.ToString().PadLeft(3, '0');
                    string _id = "10" + prefix + PayID.Portal.Areas.ServiceRequest.Configuration.Data.GetNextSquence("shipment_request_" + prefix).ToString().PadLeft(5, '0');
                    request._id = _id;
                    MetadataController md = new MetadataController();

                    //Check Code exist
                    //if (String.IsNullOrEmpty(request.tracking_code.ToString()))
                    //request.tracking_code = md.CreateAutoGenCode(request.customer.code.ToString(),
                    //        int.Parse("0" + request.service.cashpost_service.ToString()), request.from_address.province.ToString());
                    dynamic _comment = new JObject();
                    _comment.at = DateTime.Now.ToString("yyyyMMddHHmmss");
                    _comment.by = "Cash@POST API";
                    _comment.comment = "Khởi tạo";

                    request.comments = new JArray{
                    _comment
                };
                    request.tracking_code = request.tracking_code.ToString().Trim();
                    dynamic _assign = new JObject();
                    _assign.assign_by = "Cash@POST API";
                    _assign.assign_at = DateTime.Now.ToString("yyyyMMddHHmmss");
                    _assign.assign_to_id = unit_code;
                    _assign.assign_to_full_name = unit_name;
                    request.assigned_to = new JArray { _assign };

                    request.current_assigned = unit_link;
                    request.created_by = "Cash@POST API";
                    request.created_at = DateTime.Now.ToString("yyyyMMddHHmmss");
                    request.current_assigned_name = unit_name;                    
                    request.refcode = request.tracking_code.ToString().Trim();
                    request.system_status = (String.IsNullOrEmpty(request.tracking_code.ToString())) ? "C1" : "C5";
                    PayID.Portal.Areas.ServiceRequest.Configuration.Data.SaveDynamic("shipment", request);

                    //ProxyController.InsertShipmentOracle(request);

                    #region "Tao van don"
                    if (!String.IsNullOrEmpty(request.tracking_code.ToString()))
                    {
                        dynamic obj_request = new DynamicObj();
                        obj_request.Code = request.tracking_code.ToString().Trim();
                        try
                        {
                            obj_request.Value = long.Parse(request.product.value.ToString());
                            obj_request.Weight = long.Parse("0" + request.parcel.weight.ToString());
                            obj_request.Quantity = 1;
                            obj_request.SenderName = request.from_address.name.ToString();
                            obj_request.SenderAddress = request.from_address.address.ToString();
                            obj_request.SenderMobile = request.from_address.phone.ToString();
                            obj_request.ReceiverName = request.to_address.name.ToString();
                            obj_request.ReceiverAddress = request.to_address.address.ToString();
                            obj_request.ReceiverMobile = request.to_address.phone.ToString();
                            obj_request.CustomerCode = request.customer.code.ToString();
                            obj_request.FromProvinceCode = int.Parse("0" + request.from_address.province.ToString());
                            obj_request.FromDistrictCode = int.Parse("0" + request.from_address.district.ToString());
                            obj_request.ToProvinceCode = int.Parse("0" + request.to_address.province.ToString());
                            obj_request.ToDistrictCode = int.Parse("0" + request.to_address.district.ToString());
                            obj_request.ToWardCode = int.Parse("0" + request.to_address.ward.ToString());
                            obj_request.Status = "C1";
                            obj_request.ProductName = request.product.name.ToString();
                            obj_request.ProductDescription = request.product.description.ToString();
                            obj_request.ServiceCode = (request.shipping_main_service == null) ? "" : int.Parse("0" + request.shipping_main_service.ToString());
                            obj_request.Type = (request.service.shipping_add_service == null) ? "" : int.Parse("0" + request.service.shipping_add_service.ToString());

                        }
                        catch { }
                        lading_create(obj_request);
                    }
                    #endregion
                    return JObject.Parse(@"{"
                      + "error_code:'00'"
                      + ",error_message:'Thành công'"
                      + ",tracking_code: '" + request.tracking_code + "'"
                      + ",request_code: '" + request._id + "'"
                      + ",request_id: '" + request.request_id + "'"
                      + ",order_id: '" + request.order_id + "'"
                  + "}");
                }
                return JObject.Parse(@"{"
                      + "error_code:'09'"
                      + ",error_message:'Xử lý thất bại. Mã vận đơn bị rỗng! '"
                      + ",tracking_code: '" + request.tracking_code + "'"
                      + ",request_code: '" + request._id + "'"
                      + ",request_id: '" + request.request_id + "'"
                      + ",order_id: '" + request.order_id + "'"
                  + "}");
            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("duplicate key error index"))
                {
                    return JObject.Parse(@"{"
                                  + "error_code:'12'"
                                  + ",error_message:'Xử lý thất bại. Mã vận đơn hoặc mã đơn hàng bị trùng! '"
                                  + ",tracking_code: '" + request.tracking_code + "'"
                                  + ",request_code: '" + request._id + "'"
                                  + ",request_id: '" + request.request_id + "'"
                                  + ",order_id: '" + request.order_id + "'"
                              + "}");
                }
                return JObject.Parse(@"{"
                      + "error_code:'09'"
                      + ",error_message:'Xử lý thất bại '" + ex.ToString()
                      + ",tracking_code: '" + request.tracking_code + "'"
                      + ",request_code: '" + request._id + "'"
                      + ",request_id: '" + request.request_id + "'"
                      + ",order_id: '" + request.order_id + "'"
                  + "}");
            }
        }
        #endregion

        #region InnerShipment

        [Route("api/ServiceRequest/InnerShipment")]
        [AcceptVerbs("POST")]
        public JObject InnerShipment(dynamic request, string api_key)
        {
            if (api_key == null)
            {
                return JObject.Parse(@"{error_code:'99',error_message:'Invalid API Key'}"); ;
            }
            dynamic _api_key = PayID.Portal.Areas.Systems.Configuration.Data.Get("api_key", Query.EQ("_id", new ObjectId(api_key)));
            if (_api_key == null)
            {
                return JObject.Parse(@"{error_code:'99',error_message:'Invalid API Key'}"); ;
            }
            //request.is_mapping = _api_key.is_mapping;
            string unit_code, unit_name, unit_link;
            try
            {
                dynamic from_address = request.from_address;

                //if (!String.IsNullOrEmpty(request.tracking_code.ToString()))
                //{
                if (from_address._id != null && !String.IsNullOrEmpty(from_address._id.ToString()))
                {
                    dynamic store = PayID.Portal.Areas.Merchant.Configuration.Data.Get("profile_store",
                        Query.And(
                        Query.EQ("StoreCode", from_address._id.ToString()),
                        Query.EQ("UserId", request.customer.code.ToString())
                        )
                        );
                    if (store != null)
                    {
                        unit_code = store.ProvinceCode;
                        unit_link = store.ProvinceCode;
                        unit_name = PayID.Portal.Areas.Metadata.Controllers.ProxyController.GetProvinceName(unit_code).Trim();
                        if (store.PostCode != null)
                        {
                            dynamic postCode = PayID.Portal.Areas.Metadata.Configuration.Data.Get("mbcPos", Query.EQ("_id", store.PostCode));
                            unit_code = store.PostCode;
                            unit_link = postCode.ProvinceCode + "." + postCode.UnitCode + "." + store.PostCode;
                            unit_name = postCode.POSName.Trim();
                            request.from_address.address = store.Address;
                            request.from_address.name = store.StoreName;
                        }
                    }
                    else
                    {
                        return JObject.Parse(@"{error_code:'10',error_message:'Invalid From Address ID'}");
                    }
                }
                else
                {
                    if (from_address.province == null)
                    {
                        return JObject.Parse(@"{error_code:'11',error_message:'Invalid From Address Province Code'}");
                    }
                    //if ((bool)request.is_mapping)
                    //{
                    //    from_address.province = PayID.Portal.Areas.Merchant.Configuration.Data.Get("partner_mapping_province",
                    //        Query.And(Query.EQ("Id", long.Parse(from_address.province.ToString())),
                    //        Query.EQ("Partner", long.Parse(request.customer.code.ToString()))
                    //        )).ProvinceCode;

                    //    request.to_address.province = PayID.Portal.Areas.Merchant.Configuration.Data.Get("partner_mapping_province",
                    //     Query.And(Query.EQ("Id", long.Parse(request.to_address.province.ToString())),
                    //     Query.EQ("Partner", long.Parse(request.customer.code.ToString()))
                    //     )).ProvinceCode;
                    //}
                    dynamic province = PayID.Portal.Areas.Metadata.Controllers.ProxyController.GetProvinceName(from_address.province.ToString());
                    if (province == null) return JObject.Parse(@"{error_code:'11',error_message:'Invalid From Address Province Code'}");
                    request.from_address.province = from_address.province;
                    unit_code = from_address.province.ToString();
                    unit_link = unit_code;
                    unit_name = province;
                }

                string prefix = DateTime.Today.ToString("yy") + DateTime.Today.DayOfYear.ToString().PadLeft(3, '0');
                string _id = "10" + prefix + PayID.Portal.Areas.ServiceRequest.Configuration.Data.GetNextSquence("shipment_request_" + prefix).ToString().PadLeft(5, '0');
                request._id = _id;

                # region Sinh mã vận đơn
                //MetadataController md = new MetadataController();
                //Check Code exist
                //if (String.IsNullOrEmpty(request.tracking_code.ToString()))
                //request.tracking_code = md.CreateAutoGenCode(request.customer.code.ToString(),
                //        int.Parse("0" + request.service.cashpost_service.ToString()), request.from_address.province.ToString());
                #endregion

                dynamic _comment = new JObject();
                _comment.at = DateTime.Now.ToString("yyyyMMddHHmmss");
                _comment.by = request.apicode;
                _comment.comment = "Khởi tạo";

                request.comments = new JArray{
                    _comment
                };

                dynamic _assign = new JObject();
                _assign.assign_by = request.apicode;
                _assign.assign_at = DateTime.Now.ToString("yyyyMMddHHmmss");
                _assign.assign_to_id = unit_code;
                _assign.assign_to_full_name = unit_name;
                request.assigned_to = new JArray { _assign };
                request.partnercode = api_key;
                request.current_assigned = unit_link;
                request.created_by = request.apicode;
                request.refcode = _id;
                request.created_at = DateTime.Now.ToString("yyyyMMddHHmmss");
                request.current_assigned_name = unit_name;
                request.system_status = "C5";

                PayID.Portal.Areas.ServiceRequest.Configuration.Data.SaveDynamic("shipment", request);

                #region "Tao van don"
                if (request.tracking_code != null && !String.IsNullOrEmpty(request.tracking_code.ToString()))
                {
                    dynamic obj_request = new DynamicObj();
                    obj_request.Code = request.tracking_code.ToString();
                    try
                    {
                        obj_request.Value = long.Parse(request.product.value.ToString());
                        obj_request.Weight = long.Parse("0" + request.parcel.weight.ToString());
                        obj_request.Quantity = 1;
                        obj_request.SenderName = request.from_address.name.ToString();
                        obj_request.SenderAddress = request.from_address.address.ToString();
                        obj_request.SenderMobile = request.from_address.phone.ToString();
                        obj_request.ReceiverName = request.to_address.name.ToString();
                        obj_request.ReceiverAddress = request.to_address.address.ToString();
                        obj_request.ReceiverMobile = request.to_address.phone.ToString();
                        obj_request.CustomerCode = request.customer.code.ToString();
                        obj_request.FromProvinceCode = int.Parse("0" + request.from_address.province.ToString());
                        obj_request.FromDistrictCode = int.Parse("0" + request.from_address.district.ToString());
                        obj_request.ToProvinceCode = int.Parse("0" + request.to_address.province.ToString());
                        obj_request.ToDistrictCode = int.Parse("0" + request.to_address.district.ToString());
                        obj_request.ToWardCode = int.Parse("0" + request.to_address.ward.ToString());
                        obj_request.Status = "C5";
                        obj_request.PartnerCode = api_key;
                        obj_request.ProductName = request.product.name.ToString();
                        obj_request.ProductDescription = request.product.description.ToString();
                        obj_request.ServiceCode = (request.shipping_main_service == null) ? "" : int.Parse("0" + request.shipping_main_service.ToString());
                        obj_request.Type = (request.service.shipping_add_service == null) ? "" : int.Parse("0" + request.service.shipping_add_service.ToString());
                        obj_request.apicode = request.apicode;
                        obj_request.OrderId = request.order_id;
                    }
                    catch { }
                    lading_create(obj_request);
                }
                #endregion
                return JObject.Parse(@"{"
                  + "error_code:'00'"
                  + ",error_message:'Thành công'"
                  + ",tracking_code: '" + request.tracking_code + "'"
                  + ",request_code: '" + request._id + "'"
                  + ",request_id: '" + request.request_id + "'"
                  + ",order_id: '" + request.order_id + "'"
              + "}");

            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("11000"))
                {
                    return JObject.Parse(@"{"
                                  + "error_code:'12'"
                                  + ",error_message:'Mã đơn hàng bị trùng! '"
                                  + ",request_code: '" + request._id + "'"
                                  + ",request_id: '" + request.request_id + "'"
                                  + ",order_id: '" + request.order_id + "'"
                              + "}");
                }
                return JObject.Parse(@"{"
                      + "error_code:'09'"
                      + ",error_message:'Xử lý thất bại '" + ex.ToString()
                      + ",request_code: '" + request._id + "'"
                      + ",request_id: '" + request.request_id + "'"
                      + ",order_id: '" + request.order_id + "'"
                  + "}");
            }
        }
        #endregion
        //Ham tao van don tu shipment
        private static void lading_create(dynamic _request)
        {
            //ICalculateRate cal = new CalculateRate();          
            //ProxyController px = new ProxyController();
            dynamic dynamicObj = new DynamicObj();
            _request._id = PayID.Portal.Areas.Lading.Configuration.Data.GetNextSquence("LogCreateLading");
            _request.Weight = 0;
            _request.MainFee = 0;
            _request.ServiceFee = 0;
            _request.CodFee = 0;
            _request.TotalFee = 0;
            _request.DateCreated = DateTime.Now;

            #region tinh phi theo ham cua VNP
            //try
            //{
            //dynamicObj = px.processChargesInfo(int.Parse(_request.Weight.ToString()), double.Parse(_request.Value.ToString()), _request.ServiceCode.ToString(), _request.FromProvinceCode.ToString(), _request.ToProvinceCode.ToString());
            //dynamicObj = dynamicObj.Output[0];
            //_request.MainFee =  long.Parse(dynamicObj.MainCharges.ToString());
            //_request.ServiceFee = long.Parse(dynamicObj.PlusServiceCharges.ToString());
            //_request.CodFee =long.Parse(dynamicObj.CodCharges.ToString());
            //_request.TotalFee =  long.Parse(dynamicObj.CodCharges.ToString()) + long.Parse(_request.Value.ToString());
            //_request.DateCreated = DateTime.Now;
            //}
            //catch
            //{
            //    Charges[] mangphi = cal.CalutaltorFee(string.IsNullOrEmpty(_request.ServiceCode.ToString()) ? "" : _request.ServiceCode.ToString(), long.Parse(_request.Value.ToString()), _request.FromProvinceCode.ToString(), "VN", _request.ToProvinceCode.ToString(), "VN", long.Parse(_request.Weight.ToString()), 0);
            //    _request.MainFee = long.Parse(mangphi[0].MainFee.ToString());
            //    _request.ServiceFee = long.Parse(mangphi[0].ServiceFee.ToString());
            //    _request.CodFee = long.Parse(mangphi[0].CodFee.ToString());
            //    _request.TotalFee = long.Parse(mangphi[0].CodFee.ToString()) + long.Parse(_request.Value.ToString());
            //_request.Status = "C5";
            //    _request.DateCreated = DateTime.Now;
            //}
            #endregion
            try
            {
                // InsertLogLading(_request);
                PayID.Portal.Areas.Lading.Configuration.Data.Insert("LogCreateLading", _request);
                dynamic _jouey = PayID.Portal.Areas.Lading.Configuration.Data.Get("LogJourney", Query.And(Query.EQ("Code", _request.Code), Query.EQ("Status", "C5")));
                if (_jouey == null)
                {
                    dynamic objLogJourney = new DynamicObj();
                    objLogJourney._id = PayID.Portal.Areas.Lading.Configuration.Data.GetNextSquence("LogJourney");
                    objLogJourney.Code = _request.Code;
                    objLogJourney.Status = "C5";
                    objLogJourney.UserId = _request.CustomerCode.ToString();
                    objLogJourney.Location = "Cast@Post";
                    objLogJourney.Note = _request.apicode;
                    objLogJourney.DateCreate = DateTime.Now;
                    PayID.Portal.Areas.Lading.Configuration.Data.Save("LogJourney", objLogJourney);
                }
            }
            catch
            {
            }
            finally
            {
                PayID.Portal.Areas.Lading.Configuration.Data.Save("Lading", _request);
            }

        }
    }
}
