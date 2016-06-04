using MongoDB.Driver.Builders;
using Newtonsoft.Json;
using PayID.Common;
using PayID.DataHelper;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace PayID.API.Process
{
    public class MerchantProcess
    {
        public static PayID.DataHelper.MongoHelper Data;
        public static void Init()
        {
            Data = new DataHelper.MongoHelper(
                System.Configuration.ConfigurationManager.AppSettings["MERCHANT_DB_SERVER"],
                System.Configuration.ConfigurationManager.AppSettings["MERCHANT_DB_DATABASE"]
                );
        }

        public static bool IsExisted(string username)
        {
            return Data.Count("merchant_user", Query.EQ("_id", username)) > 0;
        }
        public static void user_is_authenticated(dynamic _request, out dynamic _response)
        {
            _response = new ExpandoObject();
            string _result = Login(
                    _request.email,
                    _request.password
                );
            if (String.IsNullOrEmpty(_result))
            {
                _response.response_code = "90";
                _response.response_message = "Thông tin xác thực người dùng không hợp lệ. Vui lòng kiểm tra lại email và mật khẩu!";
                _response.profile_code = "";
                return;
            }

            _response.response_code = "00";
            _response.response_message = "Người dùng được xác thực";
            _response.profile_code = _result;
        }
        public static void change_password(dynamic _request, out dynamic _response)
        {
            _response = new ExpandoObject();
            bool _result = ChangePassword(
                _request.email,
                _request.old_password,
                _request.new_password);

            if (!_result)
            {
                _response.response_code = "90";
                _response.response_message = "Sai mật khẩu cũ hoặc tài khoản không tồn tại. Vui lòng thử lại sau";
                return;
            }

            _response.response_code = "00";
            _response.response_message = "Đổi mật khẩu thành công";
            return;

        }
        public static void user_register(dynamic _request, out dynamic _response)
        {
            _response = new ExpandoObject();
            bool _result = RegisterUser(
                    _request.business_profile.ToString(),
                    _request.email,
                    _request.full_name,
                    _request.password,
                    _request.default_role
                );
            if (!_result)
            {
                _response.response_code = "90";
                _response.response_message = "Có lỗi trong quá trình xử lý. Vui lòng thử lại sau";
                return;
            }

            _response.response_code = "00";
            _response.response_message = "Đăng ký thành công";
        }
        public static void register(dynamic _request, out dynamic _response)
        {
            //Kết quả trả về
            _response = new ExpandoObject();

            //Kiem tra da ton tai hay chua
            if (IsExisted(_request.email))
            {
                _response.response_code = "01";
                _response.response_message = "Email đã được đăng ký";
                return;
            }

            string _code = Register(
                _request.email.ToString(),
                _request.full_name.ToString(),
                _request.mobile.ToString(),
                _request.system);
            if (String.IsNullOrEmpty(_code))
            {
                _response.response_code = "90";
                _response.response_message = "Khởi tạo không thành công";
                return;
            }

            RegisterUser(_code, _request.email, _request.full_name, _request.password, "MERCHANT_ADMIN");

            _response.response_code = "00";
            _response.response_message = "Khởi tạo thành công";
            _response.profile_code = _code;
        }
        public static string IsAuthenticated(string email, string password)
        {
            password = Security.CreatPassWordHash(email + password);
            dynamic _old = Data.Get(
                "merchant_user",
                Query.And(
                    Query.EQ("_id", email),
                    Query.EQ("password", password)
                ));
            if (_old == null) return String.Empty;
            return _old.AccountID.ToString();
        }
        public static bool RegisterUser(string business_profile, string email, string full_name, string password, string default_role)
        {
            DateTime now = DateTime.Now.ToUniversalTime();
            dynamic _user = new ExpandoObject();

            _user._id = email;
            _user.UserEmail = email;
            _user.FullName = full_name;
            _user.AccountID = long.Parse(business_profile);
            _user.Roles = default_role;
            _user.Status = "ACTIVED";
            _user.password = Security.CreatPassWordHash(email + password);
            _user.user_create = "SYSTEM";
            _user.system_created_time = now;
            _user.system_last_updated_time = now;
            _user.system_last_updated_by = "UPDATE BY SYSTEM";
            string notes_login = now.ToString() + " - CREATE A NEW USER LOGIN BY SYSTEM";
            notes_login = notes_login + Environment.NewLine;
            _user.system_historical_notes = notes_login;

            return Data.InsertDynamic("merchant_user", _user);
        }
        public static string Register(string email, string full_name, string mobile, string system)
        {
            //Lay thoi gian hien tai
            DateTime date = DateTime.Now;//.ToUniversalTime();

            //Lay sequence de tao ma merchant
            long iSeq = Data.GetNextSquence("business_profile_" + date.ToString("yyMM") + "1");
            dynamic _profile = new ExpandoObject();

            //Ma merchant
            string business_code = date.ToString("yy") + date.DayOfYear.ToString().PadLeft(3, '0') + iSeq.ToString().PadLeft(3, '0');

            //Khoi tao object profile
            _profile._id = business_code;
            _profile.general_email = email;
            _profile.general_system = system;
            _profile.general_account_type = "B";
            _profile.general_full_name = full_name;
            _profile.system_is_active = false;
            _profile.business_tax = String.Empty;
            _profile.business_website = String.Empty;
            _profile.contact_name = String.Empty;
            _profile.contact_address_address = String.Empty;
            _profile.contact_address_district = String.Empty;
            _profile.contact_address_province = String.Empty;
            _profile.contact_phone_mobile = mobile;
            _profile.contact_phone_work = String.Empty;
            _profile.contact_phone_fax = String.Empty;
            _profile.legacy_no = String.Empty;
            _profile.legacy_issued_by = String.Empty;
            _profile.legacy_issued_date = DateTime.MinValue;
            _profile.settlement_channel = String.Empty;
            _profile.settlement_scheme_process_window = 0;
            _profile.settlement_scheme_settle_window = 0;
            _profile.settlement_scheme_transaction_expire_window = 0;

            _profile.waitting_amount = 0;
            _profile.paid_amount = 0;

            _profile.system_last_updated_time = date;
            _profile.system_last_updated_by = "CREATE BY SYSTEM";

            string notes = date.ToString() + " - CREATE A NEW PROFILE BY SYSTEM";
            notes = notes + Environment.NewLine;
            _profile.system_historical_notes = notes;

            //Luu vao trong csdl
            Data.InsertDynamic("business_profile", _profile);
            //Dang ky channel
            RegisterChannel(business_code, "DEFAULT CHANNEL", "PHYSICAL");

            return business_code;
        }
        public static string RegisterChannel(string business_profile, string channel_name, string good_type)
        {
            //Lay sequence cua channel
            DateTime date = DateTime.Now.ToUniversalTime();
            long iSeq = Data.GetNextSquence("business_profile_" + date.ToString("yyMM") + "1");
            //Ma channel
            string code = business_profile + iSeq.ToString().PadLeft(2, '0');
            //DateTime date = DateTime.Now;//.ToUniversalTime();

            dynamic _channel = new ExpandoObject();
            _channel._id = code;
            _channel.general_title = channel_name;
            _channel.system_is_active = true;
            _channel.business_code = business_profile;
            _channel.api_key = Guid.NewGuid().ToString();
            _channel.api_ip_address = String.Empty;
            _channel.api_url_request = String.Empty;
            _channel.api_url_response = String.Empty;
            _channel.goods_type = good_type;
            _channel.api_currency = "VND";
            _channel.system_last_updated_time = date;
            _channel.system_last_updated_by = "CREATE BY SYSTEM";
            string notes_channel = date.ToString() + " - CREATE A NEW CHANNEL BY SYSTEM";
            notes_channel = notes_channel + Environment.NewLine;
            _channel.system_historical_notes = notes_channel;

            //luu vao trong db
            Data.InsertDynamic("business_channel", _channel);
            return code;
        }
        public static dynamic Login(string username, string password)
        {
            password = Security.CreatPassWordHash(username + password);
            dynamic _old = Data.Get(
                "customer_user",
                Query.And(
                    Query.EQ("_id", username),
                    Query.EQ("password", password)
                ));
            if (_old == null) return String.Empty;
            return _old.account_id.ToString();
        }

        public bool ResetPassword(string email, string new_password)
        {
            new_password = Security.CreatPassWordHash(email + new_password);
            dynamic _old = Data.GetDynamic(
                "merchant_user",
                Query.EQ("_id", email)
                );

            _old.password = new_password;
            return Data.Save(
                "merchant_user",
                _old
                );
        }
        public bool ChangePassword(string email, string old_password, string new_password)
        {
            old_password = Security.CreatPassWordHash(email + old_password);
            new_password = Security.CreatPassWordHash(email + new_password);

            dynamic _old = Data.GetDynamic(
                "merchant_user",
                Query.EQ("_id", email)
                );
            if (_old.password.Equal(old_password))
            {
                _old.password = new_password;
                return Data.Save(
                    "merchant_user",
                    _old
                    );
            }
            return false;
        }

        public static void make_transaction(dynamic request, out dynamic response)
        {
            //Kết quả trả về
            response = new ExpandoObject();
            //Khởi tạo billing
            dynamic billing_info = new DynamicObj();

            #region "các tham số mặc định của giao dịch"
            //mã kênh giao dịch
            string channel = request.channel.ToString();
            string business_profile = channel.Substring(0, channel.Length - 2);
            //long lbusiness_profile = long.Parse(business_profile);
            //long lchannel = long.Parse(channel);

            ////kiem tra tinh hop le cua channel
            //dynamic channelObj = Data.Get("business_channel", Query.EQ("_id", channel));

            ////neu khong hop le thi tra ve ket qua loi
            //if (channelObj == null)
            //{
            //    response.response_code = "01";
            //    response.response_message = "Mã kênh giao dịch không hợp lệ";
            //    return;
            //}

            //mã đơn hàng
            string order_id = request.order_id.ToString();
            //tổng tiền
            long amount = Convert.ToInt64(request.amount);
            //loại tiền tệ (không bắt buộc)
            string currency = (request.currency == null) ? "VND" : request.currency.ToString();
            //địa chỉ ip mua hàng (không bắt buộc)
            string ip = (request.ip == null) ? String.Empty : request.ip.ToString();
            //chuỗi bảo mật - chưa áp dụng
            string token = request.token.ToString();

            long shipping_fee = 0; //tính toán sau

            #endregion "các tham số mặc định của giao dịch"
            DateTime _transactionTime = DateTime.Now;
            long iSeqCode = Data.GetNextSquence("billing_" + _transactionTime.ToString("yy") + _transactionTime.DayOfYear.ToString().PadLeft(3, '0')
                + _transactionTime.ToString("HH"));

            //Mã billing của hệ thống
            string code = _transactionTime.ToString("yy") + _transactionTime.DayOfYear.ToString().PadLeft(3, '0')
                + _transactionTime.ToString("HH") + iSeqCode.ToString().PadLeft(6, '0');

            billing_info.is_partial = false;
            if (request.is_partial != null)
                billing_info.is_partial = request.is_partial;
            billing_info.business_code = business_profile;
            billing_info._id = code;
            billing_info.billing_code = order_id;
            billing_info.amount = amount;
            billing_info.currency = currency;
            billing_info.system_status = "ACTIVED";

            //Khai bao mang object customer

            dynamic objCustomer = new DynamicObj();

            if (request.full_name!=null)
            {
                objCustomer.full_name = request.full_name.ToString();
            }
            else
            {
                objCustomer.full_name = null;
            }

            if (request.address!=null)
            {
                objCustomer.address = request.address.ToString();
            }
            else
            {
                objCustomer.address = null;
            }

            if (request.email!=null)
            {
                objCustomer.email = request.email.ToString();
            }
            else
            {
                objCustomer.email = null;
            }

            if (request.mobile!=null)
            {
                objCustomer.mobile = request.mobile.ToString();
            }
            else
            {
                objCustomer.mobile = null;
            }

            billing_info.expired_datetime = _transactionTime.AddHours(48);
            billing_info.channel_code = channel;
            if (objCustomer != null)
            {
                billing_info.customer = objCustomer;
            }

            billing_info.shipping_option = (request.shipping_option == null) ? String.Empty : request.shipping_option;

            #region "các tham số về chuyển phát"
            if (request.shipping_address != null)
            {
                billing_info.shipping_address = request.shipping_address;
            }
            shipping_fee = 0;
            if (request.shipping_fee != null)
                shipping_fee = request.shipping_fee;
            #endregion "các tham số về chuyển phát"

            #region "danh sach san pham"
            dynamic objProduct = new DynamicObj();
            if (request.description != null)
            {
                objProduct.description = request.description.ToString();
            }

            if(request.amount!=null)
            {
                objProduct.amount = request.amount;
            }

            if (request.quantity != null)
            {
                objProduct.quantity = request.quantity;
            }

            if (objProduct != null)
            {
                billing_info.products = objProduct;
            }

            #endregion "danh sach san pham"
            billing_info.shipping_fee = shipping_fee;
            
            #region "khoi tao giao dich"
            dynamic time = new DynamicObj();

            billing_info.system_last_updated_time = _transactionTime;
            billing_info.system_last_updated_by = "- SYSTEM - ";
            string notes = _transactionTime.ToString() + " - CREATE BY SYSTEM";
            notes = notes + Environment.NewLine;
            billing_info.system_historical_notes = notes;

            if (!Data.Insert("billing_info", billing_info))
            {
                response.response_code = "02";
                response.response_message = "Lỗi khởi tạo giao dịch";
                return;
            }
            #endregion "khoi tao giao dich"

            #region insert transactions
            bool _result = false;
            try
            {
                dynamic paymentDoc = new DynamicObj();
                paymentDoc.CreatedDate = _transactionTime;
                paymentDoc.ChannelId = long.Parse(channel);
                paymentDoc.ChannelTransaction = order_id;
                paymentDoc.AccountId = long.Parse(business_profile);
                paymentDoc.SystemTransactionRef = code;
                paymentDoc.OriginalAmount = amount;
                paymentDoc.OriginalCurrency = currency;
                paymentDoc.CustomerEmail = billing_info.customer.email;
                paymentDoc.CustomerFullName = billing_info.customer.full_name;
                paymentDoc.CustomerMobile =
                    (billing_info.customer.mobile == null) ? String.Empty : billing_info.customer.mobile
                    ;
                if (request.products != null)
                {
                    paymentDoc.items = request.products;
                }
                paymentDoc.CustomerIP = ip;
                paymentDoc.Status = "NEW";
                _result = Data.Insert("PaymentTransaction", paymentDoc);
            }
            catch { }

            #endregion
            if (_result)
            {
                response.response_code = "00";
                response.response_message = "Giao dịch thành công";
                response.pay_code = code;
                response.shipping_fee = shipping_fee;
            }
            else
            {
                response.response_code = "03";
                response.response_message = "Giao dịch chưa thực hiện thành công";
                response.pay_code = code;
                response.shipping_fee = shipping_fee;
            }
        }
        internal static void channel_register(dynamic _request, out dynamic _response)
        {
            _response = new ExpandoObject();
            string _code = RegisterChannel(
                _request.business_profile.ToString(),
                _request.channel_name.ToString(),
                _request.goods_type.ToString()
                );
            _response.response_code = "00";
            _response.response_message = "Đăng ký thành công";
            _response.channel_code = _code;
        }
    }
}