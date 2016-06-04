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
    public class LadingImportController : ApiController
    {
        //
        // GET: /Lading/

        /// <summary>
        /// Hàm xử lý vận đơn(update, delete...)
        /// trungnd 25/09/2014
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        public JObject[] LadingByReQuests(JObject request)
        {
            string date_default = DateTime.Now.AddDays(30).ToString("dd-MM-yyyy");
            string receivername, mobile, fileno, usercreated;
            receivername = (request["receivername"] != null) ? request["receivername"].ToString() : "";
            mobile = (request["mobile"] != null) ? request["mobile"].ToString() : "";
            fileno = (request["fileno"] != null) ? request["fileno"].ToString() : "";
            usercreated = (request["usercreated"] != null) ? request["usercreated"].ToString() : "";
            IMongoQuery query = Query.NE("_id", 0);
            query = Query.And(query, Query.NE("Delete", 1), Query.EQ("system_time_key.date", long.Parse(DateTime.Now.ToString("yyyyMMdd"))));

            if (!String.IsNullOrEmpty(receivername))
                query = Query.And(
                    query,
                    Query.EQ("ReceiverName", receivername)
                    );
            if (!String.IsNullOrEmpty(mobile))
                query = Query.And(
                    query,
                    Query.EQ("ReceiverMobile", mobile)
                    );
            if (!String.IsNullOrEmpty(fileno))
                query = Query.And(
                    query,
                    Query.EQ("FileNo", fileno)
                    );
            if (!String.IsNullOrEmpty(usercreated))
                query = Query.And(
                    query,
                    Query.EQ("UserCreate", usercreated)
                    );

            DynamicObj[] ObjLading = PayID.API.Process.MerchantProcess.Data.List("LadingBillsTmp", query);

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
                        lading_delete_Tmp(_request, out _response);
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

                dynamic dyna = PayID.API.Process.MerchantProcess.Data.Get("LadingBillsTmp", query);

                //Get change values to table LadingBillsTmp
                dyna.Status = _request.Status;

                //Save changes to table LadingBillsTmp
                PayID.API.Process.MerchantProcess.Data.Save("LadingBillsTmp", dyna);
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
                dynamic dyna = PayID.API.Process.MerchantProcess.Data.Get("LadingBillsTmp", query);

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
                PayID.API.Process.MerchantProcess.Data.Save("LadingBillsTmp", dyna);

                _response.response_code = "00";
                _response.response_message = "Cập nhật thành công";
            }
            catch
            {
                _response.response_code = "90";
                _response.response_message = "Có lỗi trong quá trình xử lý. Vui lòng thử lại sau";
            }
        }
        private static void lading_delete_Tmp(dynamic _request, out dynamic _response)
        {
            _response = new ExpandoObject();
            try
            {
                IMongoQuery query = Query.EQ("_id", _request._id);
                dynamic dyna = PayID.API.Process.MerchantProcess.Data.Get("LadingBillsTmp", query);

                //Get change values to table LadingBillsTmp

                dyna.Delete = _request.Delete;
                PayID.API.Process.MerchantProcess.Data.Save("LadingBillsTmp", dyna);

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
                dynamic dynamicObj = new DynamicObj();
                _request._id = PayID.API.Process.MerchantProcess.Data.GetNextSquence("LadingBillsTmp");
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
                    PayID.API.Process.MerchantProcess.Data.Insert("LadingBillsTmp", _request);
                }
                catch
                {
                    _response.response_code = "01";
                    _response.response_message = "Có lỗi trong quá trình xử lý. Vui lòng thử lại sau";
                }
                finally
                {

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
