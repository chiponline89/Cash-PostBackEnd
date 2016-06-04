using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace PayID.API.Controllers
{
    public class MerchantController : ApiController
    {

        [AcceptVerbs("POST")]
        public JObject Post(string function, JObject data)
        {

            dynamic _request = data;
            dynamic _logRequest = _request;
            dynamic _logResponse;
            _logRequest.log_time = DateTime.Now;
            _logRequest.api_function = function;
            _logRequest.log_type = "request";
            //Logging.loggingList.Add(_logRequest);

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
                    _logResponse = _response;
                    _logResponse.log_type = "response";
                    _logRequest.log_time = DateTime.Now;
                    _logRequest.api_function = function;
                    //Logging.loggingList.Add(_logResponse);
                    _json_response = JsonConvert.SerializeObject(_response);
                    return JObject.Parse(_json_response);
                }

                //lấy giá trị tham số function
                function = function.ToLower();
                //các function xử lý
                switch (function)
                {
                    case "merchant_registration":
                        Process.MerchantProcess.register(_request, out _response);
                        break;
                    case "channel_register":
                        Process.MerchantProcess.channel_register(_request, out _response);
                        break;
                    case "user_registration":
                        Process.MerchantProcess.user_register(_request, out _response);
                        break;
                    case "user_is_authenticated":
                        Process.MerchantProcess.user_is_authenticated(_request, out _response);
                        break;
                    case "user_change_password":
                        Process.MerchantProcess.change_password(_request, out _response);
                        break;
                    case "make_transaction":
                        Process.MerchantProcess.make_transaction(_request, out _response);
                        break;
                    default:
                        _response.response_code = "91";
                        _response.response_message = "Yêu cầu không được hỗ trợ";
                        break;
                }
            }
            catch (Exception ex)
            {
                _response = new ExpandoObject();
                _response.response_code = "96";
                _response.response_message = ex.Message;// "Lỗi xử lý hệ thống";
            }

            _logResponse = _response;
            _logRequest.log_time = DateTime.Now;
            _logRequest.api_function = function;
            _logResponse.log_type = "response";
            //Logging.loggingList.Add(_logResponse);
            _json_response = JsonConvert.SerializeObject(_response);
            return JObject.Parse(_json_response);
        }
    }
}
