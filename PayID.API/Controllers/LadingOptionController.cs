using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MongoDB.Driver.Builders;
using MongoDB.Driver;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Dynamic;
using System.IO;
using PayID.DataHelper;

namespace PayID.API.Controllers
{
    public class LadingOptionController : ApiController
    {
        //
        // GET: /Lading/

        /// <summary>
        /// Hàm xử lý vận đơn(update, delete...)
        /// trungnd 25/09/2014
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        public JObject Lading(string option, JObject request)
        {
            //Chuyển yêu cầu dạng JObject sang dynamic
            dynamic _request = new DynamicObj(
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
                if (String.IsNullOrEmpty(option))
                {
                    _response.response_code = "90";
                    _response.response_message = "Sai tham số kết nối";

                    _json_response = JsonConvert.SerializeObject(_response);
                    return JObject.Parse(_json_response);
                }

                lading_create(option, _request, out _response);

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
        private static void lading_create(string collection_name, dynamic _request, out dynamic _response)
        {
            _response = new ExpandoObject();
            try
            {
                MetadataController md = new MetadataController();


                // tao ma van don
                _request.Code = md.CreateAutoGenCode(_request.CustomerCode.ToString(), _request.Type, _request.FromProvinceCode.ToString());
                _request._id = PayID.Portal.Areas.Lading.Configuration.Data.GetNextSquence(collection_name);
                _request.DateCreated = DateTime.Now;

                PayID.Portal.Areas.Lading.Configuration.Data.Insert(collection_name, _request);
                // Trả về JOBJECT
                _response.Code = _request.Code;
                _response.response_code = "00";
                _response.response_message = "Cập nhật thành công";
            }
            catch
            {
                _response.response_code = "90";
                _response.response_message = "Có lỗi trong quá trình xử lý. Vui lòng thử lại sau";
            }

            // Hàm này phải xử lý lại, chưa ổn. 02/11/2014
        }
        [AcceptVerbs("POST")]
        public JObject[] LadingByReQuests(JObject request)
        {
            string date_default = DateTime.Now.AddDays(30).ToString("dd-MM-yyyy");
            string code, from_date, to_date, status, to_province_code, fileno,usercreate;
            int del=-1;
            code = (request["code"] != null) ? request["code"].ToString() : "";
            to_date = (request["to_date"] != null) ? request["to_date"].ToString() : "";
            from_date = (request["from_date"] != null) ? request["from_date"].ToString() : "";
            status = (request["status"] != null) ? request["status"].ToString() : "";
            to_province_code = (request["to_province_code"] != null) ? request["to_province_code"].ToString() : "";
            fileno = (request["fileno"] != null) ? request["fileno"].ToString() : "";
            usercreate = (request["usercreate"] != null) ? request["usercreate"].ToString() : "";
            del = int.Parse((request["del"] != null) ? request["del"].ToString() : "-1");
            IMongoQuery query = Query.NE("_id", 0);
            if (!String.IsNullOrEmpty(code))
                query = Query.And(
                    query,
                    Query.EQ("Code", code)
                    );
            if (!String.IsNullOrEmpty(to_province_code))
                query = Query.And(
                    query,
                    Query.EQ("ToProvinceCode", to_province_code)
                    );
            if (!String.IsNullOrEmpty(fileno))
                query = Query.And(
                    query,
                    Query.EQ("FileNo", fileno)
                    );
            if (string.IsNullOrEmpty(status))
            {
                query = Query.And(
                    query,
                    Query.EQ("Status", status)
                    );
            }
            if (del>=0)
            {
                query = Query.And(
                    query,
                    Query.EQ("Delete", del)
                    );
            }
            if (!string.IsNullOrEmpty(usercreate))
            {
                query = Query.And(
                    query,
                    Query.EQ("UserCreate", usercreate)
                    );
            }
            DynamicObj[] ObjLading = PayID.Portal.Areas.Lading.Configuration.Data.List("LadingBillsTmp", query);

            JObject[] List_Lading = new JObject[] { };
            List_Lading = (from e in ObjLading select e).Select(c => JObject.Parse(JsonConvert.SerializeObject(c.ToExpando()))).ToArray();
            return List_Lading;
        }
        [AcceptVerbs("POST")]
        public JObject LadingTmp(string function, JObject request)
        {
            dynamic _request = new DynamicObj(
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
                        lading_update_Tmp(_request, out _response);
                        break;
                    case "create_lading_tmp":
                        lading_create_tmp(_request, out _response);
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
        public static void lading_update_Tmp_by_Item(dynamic _request)
        {

            try
            {
                IMongoQuery query1 = Query.EQ("Code", _request.Code);
                IMongoQuery query2 = Query.EQ("Delete", 0);
                IMongoQuery query3 = Query.EQ("Status", "C1");
                IMongoQuery query = Query.And(query1, query2, query3);

                dynamic dyna = PayID.Portal.Areas.Lading.Configuration.Data.Get("LadingBillsTmp", query);

                //Get change values to table LadingBillsTmp
                dyna.Status = _request.Status;

                //Save changes to table LadingBillsTmp
                PayID.Portal.Areas.Lading.Configuration.Data.Save("LadingBillsTmp", dyna);
            }
            catch
            {

            }
        }
        private static void lading_update_Tmp(dynamic _request, out dynamic _response)
        {
            _response = new ExpandoObject();
            try
            {
                IMongoQuery query = Query.EQ("_id", _request._id);
                dynamic dyna = PayID.Portal.Areas.Lading.Configuration.Data.Get("LadingBillsTmp", query);

                //Get change values to table LadingBillsTmp
                dyna.Status = _request.Status;
                dyna.Delete = _request.Delete;
                dyna.Value = _request.Value;
                dyna.Weight = _request.Weight;
                dyna.Quantity = int.Parse(string.Format("{0}", _request.Quantity));
                dyna.ProductName = _request.ProductName;
                dyna.ProductDescription = _request.ProductDescription;
                dyna.ToPostCode = _request.ToPostCode;
                dyna.ReceiverName = _request.ReceiverName;
                dyna.ReceiverAddress = _request.ReceiverAddress;
                dyna.ReceiverMobile = _request.ReceiverMobile;
                dyna.ToProvinceCode = _request.ToProvinceCode;
                //Save changes to table LadingBillsTmp
                PayID.Portal.Areas.Lading.Configuration.Data.Save("LadingBillsTmp", dyna);

                _response.response_code = "00";
                _response.response_message = "Cập nhật thành công";
            }
            catch
            {
                _response.response_code = "90";
                _response.response_message = "Có lỗi trong quá trình xử lý. Vui lòng thử lại sau";
            }
        }
        #region Hàm tạo vận đơn
        private static void lading_create_tmp(dynamic _request, out dynamic _response)
        {
            _response = new ExpandoObject();
            try
            {
                ProxyController px = new ProxyController();
                dynamic dynamicObj = new DynamicObj();
                _request._id = PayID.Portal.Areas.Lading.Configuration.Data.GetNextSquence("LogCreateLading");
                if (string.IsNullOrEmpty(_request.Weight.ToString()))
                    _request.Weight = 10;

                _request.MainFee = 0;
                _request.ServiceFee = 0;
                _request.CodFee = 0;
                _request.TotalFee = 0;
                _request.DateCreated = DateTime.Now;

                // Trả về JOBJECT
                _response.Code = _request.Code;
                _response.MainFee = 0;
                _response.ServiceFee = 0;
                _response.CodFee = 0;
                _response.TotalFee = 0;

                try
                {
                    // InsertLogLading(_request);
                    PayID.Portal.Areas.Lading.Configuration.Data.Insert("LogCreateLading", _request);
                    // Insert LogJourney
                    dynamic _jouey = PayID.Portal.Areas.Lading.Configuration.Data.Get("LogJourney", Query.And(Query.EQ("Code", _request.Code), Query.EQ("Status", "C5")));
                    if (_jouey == null)
                    {
                        dynamic objLogJourney = new DynamicObj();
                        objLogJourney._id = PayID.Portal.Areas.Lading.Configuration.Data.GetNextSquence("LogJourney");
                        objLogJourney.Code = _request.Code;
                        objLogJourney.Status = "C5";
                        objLogJourney.UserId = _request.CustomerCode.ToString();
                        objLogJourney.Location = "Cast@Post";
                        objLogJourney.Note = "Excel";
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
                    Response _res = new Response();
                    lading_update_Tmp_by_Item(_request);
                }
                _response.response_code = "00";
                _response.response_message = "Cập nhật thành công";
            }
            catch (Exception ex)
            {
                _response.response_code = "90";
                _response.response_message = "Có lỗi trong quá trình kết nối, hàm xử lý nội bộ.";
            }

        }

        #endregion
    }
}
