using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PayID.DataHelper;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace PayID.API.Controllers
{
    public class SystemController : ApiController
    {
        static string template_folder = String.Empty;
        static Dictionary<dynamic, dynamic> template_dictionary = new Dictionary<dynamic, dynamic>();

        public static PayID.DataHelper.MongoHelper Data = null;

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
                    case "send_notification":
                        send_notification(_request.system.ToString(), _request.module.ToString(),
                            _request.template.ToString(), _request.type.ToString(),
                            _request.receiver.ToString(),
                            _request.content.ToString());
                        _response.response_code = "00";
                        _response.response_message = "Thành công";
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
        private void send_notification(string system, string module, string template, string type, string receiver, string content)
        {
            try
            {
                init();
                dynamic email = new DynamicObj();
                email.system = system;
                email.module = module;
                email.template = template;
                email.status = "NEW";
                email.receiver = receiver;
                email.sender = "SYSTEM";
                email.type = type.ToUpper();

                dynamic _template_id = system + "." + module + "." + template;

                var _template = template_dictionary[_template_id];
                if (_template == null)
                {
                    email.status = "ERROR";
                    email.error_code = "01";
                    email.error_message = "INVALID TEMPLATE";
                }
                else
                {
                    email.content = String.Format(_template.content, content.Split('|'));
                }

                Data.Insert("message", email);
            }
            catch { }
        }

        private void init()
        {
            if (Data == null)
            {
                Data = new MongoHelper(
                    System.Configuration.ConfigurationManager.AppSettings["CORE_DB_SERVER"],
                    System.Configuration.ConfigurationManager.AppSettings["CORE_DB_DATABASE"]
                    );
                template_folder = System.Configuration.ConfigurationManager.AppSettings["Templates_Folder"];

                loadTemplates();
            }
        }

        private void loadTemplates()
        {
            var _list_template = Data.List("template", null);
            foreach (dynamic _temp in _list_template)
            {
                if (_temp.style == "FILE")
                {
                    string _content = loadContentFromFile(_temp.content);
                    if (String.IsNullOrEmpty(_content))
                        _temp.status = "ERROR";
                    _temp.content = _content;
                }
                template_dictionary.Add(_temp._id.system + "." + _temp._id.module + "." + _temp._id.code, _temp);
            }
        }

        private string loadContentFromFile(string _file)
        {
            string _content = String.Empty;
            _file = template_folder + _file;
            if (System.IO.File.Exists(_file))
            {
                using (StreamReader reader = new StreamReader(_file))
                {
                    _content = reader.ReadToEnd();
                }
            }
            return _content;
        }
    }
}