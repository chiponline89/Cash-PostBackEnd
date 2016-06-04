using MongoDB.Driver.Builders;
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
    public class RegistrationResponse
    {
        public string ErrorCode { get; set; }
        public string ErrorMsg { get; set; }

        public string MerchantCode { get; set; }
        public long MerchantId { get; set; }
    }
    public class Response
    {
        public string ErrorCode { get; set; }
        public string ErrorMsg { get; set; }
        public string Code { get; set; }
    }
    public class RegistrationController : ApiController
    {
        public RegistrationResponse GetAddChannel(
          long accountId,
          string channelName,
          string goodtype)
        {
            try
            {
                //Data.Entity.merchant mer = Data.CoreDataHelper.GetMerchant(accountId.ToString());
                dynamic _request = new ExpandoObject();
                _request.business_profile = accountId.ToString();
                _request.channel_name = channelName;
                _request.goods_type = goodtype;

                dynamic _response = new ExpandoObject();
                Process.MerchantProcess.channel_register(_request, out _response);
                return new RegistrationResponse
                {
                    ErrorCode = _response.response_code,
                    ErrorMsg = _response.response_message,
                    MerchantCode = _response.channel_code,
                    MerchantId = long.Parse(_response.channel_code)
                };
            }
            catch { }

            return new RegistrationResponse
            {
                ErrorCode = "96",
                ErrorMsg = "Lỗi hệ thống"
            };
        }

        public RegistrationResponse GetRegister(
            string email,
            string full_name,
            string mobile,
            string password,
            string system)
        {
            //Kết quả trả về
            RegistrationResponse _response = new RegistrationResponse();

            //Kiem tra da ton tai hay chua
            if (Process.MerchantProcess.IsExisted(email))
            {
                _response.ErrorCode = "01";
                _response.ErrorMsg = "Email đã được đăng ký";
                return _response;
            }

            string _code = Process.MerchantProcess.Register(
                email.ToString(),
                full_name.ToString(),
                mobile.ToString(),
                system);
            if (String.IsNullOrEmpty(_code))
            {
                _response.ErrorCode = "90";
                _response.ErrorMsg = "Khởi tạo không thành công";
                return _response;
            }

            Process.MerchantProcess.RegisterUser(_code, email, full_name, password, "MERCHANT_ADMIN");
            _response.ErrorCode = "00";
            _response.ErrorMsg = "Đăng ký thành công";
            _response.MerchantCode = _code;
            _response.MerchantId = long.Parse(_code);
            return _response;

        }

        public Response GetLogin(string email, string password)
        {
            Response _response = new Response();
            string _result = Process.MerchantProcess.IsAuthenticated(
                    email,
                    password
                );
            if (String.IsNullOrEmpty(_result))
            {
                _response.ErrorCode = "90";
                _response.ErrorMsg = "Thông tin xác thực người dùng không hợp lệ. Vui lòng kiểm tra lại email và mật khẩu!";
                _response.Code = "";
                return _response;
            }

            _response.ErrorCode = "00";
            _response.ErrorMsg = "Người dùng được xác thực";
            _response.Code = _result;

            return _response;
        }

        public JObject GetBusinessProfileByCode(string merchant_code)
        {
            try
            {
                dynamic business = Process.MerchantProcess.Data.GetDynamic("business_profile", Query.EQ("_id", merchant_code));
                return JsonConvert.DeserializeObject(business.ToString());
            }
            catch (Exception ex)
            { }
            return new JObject { };
        }
    }
}
