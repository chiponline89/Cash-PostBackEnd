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
    public class ShipmentController : ApiController
    {
        [AcceptVerbs("POST")]
        public JObject Shipment(string function, JObject request)
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

        /// <summary>
        /// Hàm lấy ra tất cả danh sách vận đơn trong 1 tháng hoặc theo điều kiện.
        /// trungnd - 26/09/2014
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        public JObject[] ShipmentByReQuests(JObject request)
        {

            string date_default = DateTime.Now.AddDays(-15).ToString("dd/MM/yyyy");
            string from_date, to_date, status, busscode, current_assigned, FormType, province, district, pos, rptType;
            FormType = (request["FormType"] != null) ? request["FormType"].ToString() : " ";
            current_assigned = (request["current_assigned"] != null) ? request["current_assigned"].ToString() : " ";
            to_date = (request["to_date"] != null) ? request["to_date"].ToString() : "";
            from_date = (request["from_date"] != null) ? request["from_date"].ToString() : "";
            status = (request["status"] != null) ? request["status"].ToString() : "";
            province = (request["province"] != null) ? request["province"].ToString() : "";
            district = (request["district"] != null) ? request["district"].ToString() : "";
            pos = (request["pos"] != null) ? request["pos"].ToString() : "";
            rptType = (request["rptType"] != null) ? request["rptType"].ToString() : "";
            busscode = (request["busscode"] != null) ? request["busscode"].ToString() : "";
            IMongoQuery query = Query.NE("_id", ""); // NE : != 
            if (status == "0")
            {
                if (FormType == "FormName")
                {
                    query = Query.And(
                    query,
                    Query.Or(
                    Query.EQ("system_status", "C5"),
                    Query.EQ("system_status", "C6"),
                    Query.EQ("system_status", "C7")
                    )
                    );
                }
                else if (FormType == "FormCollect")
                {
                    query = Query.And(
                    query,
                    Query.Or(
                    Query.EQ("system_status", "C6"),
                    Query.EQ("system_status", "C7"),
                    Query.EQ("system_status", "C8"),
                    Query.EQ("system_status", "C9"),
                    Query.EQ("system_status", "C10"))

                    );
                }
                else if (FormType == "FormIssue")
                {
                    query = Query.And(
                        query,
                        Query.Or(Query.EQ("system_status", "C11"),
                        Query.EQ("system_status", "C12"),
                        Query.EQ("system_status", "C13"),
                        Query.EQ("system_status", "C14"),
                        Query.EQ("system_status", "C15"),
                        Query.EQ("system_status", "C16"),
                        Query.EQ("system_status", "C17"),
                        Query.EQ("system_status", "C18"))

                        );

                }
            }
            else
            {
                query = Query.And(
                  query,
                  Query.EQ("system_status", status));
            }


            if (!String.IsNullOrEmpty(current_assigned))
            {
                if (current_assigned == "00")
                {
                    if (!string.IsNullOrEmpty(province))
                    {
                        current_assigned = province;

                        if (!string.IsNullOrEmpty(district))
                        {
                            current_assigned = current_assigned + "." + district;
                        }

                        if (!string.IsNullOrEmpty(pos))
                        {
                            current_assigned = current_assigned + "." + pos;
                        }

                    }
                    else
                    {
                        current_assigned = "";
                    }
                }
                else
                {
                    if (current_assigned.Length == 2)
                    {
                        if (!string.IsNullOrEmpty(district))
                        {
                            current_assigned = current_assigned + "." + district;

                        }

                        if (!string.IsNullOrEmpty(pos))
                        {
                            current_assigned = current_assigned + "." + pos;
                        }
                    }
                    else if (current_assigned.Length >= 7)
                    {
                        if (!string.IsNullOrEmpty(pos))
                        {
                            if (!current_assigned.Contains(pos))
                                current_assigned = current_assigned + "." + pos;
                        }
                    }
                }
                MongoDB.Bson.BsonRegularExpression reg = new MongoDB.Bson.BsonRegularExpression("^" + current_assigned);

                query = Query.And(
                        query,
                        Query.Matches("current_assigned", reg)
                    );

            }
            if (!String.IsNullOrEmpty(from_date))
                query = Query.And(
                        query,
                        Query.GTE("system_time_key.date", int.Parse(DateTime.ParseExact(from_date, "dd/MM/yyyy", null).ToString("yyyyMMdd")))
                        );//GTE : >=
            else
                query = Query.And(
                       query,
                       Query.GTE("system_time_key.date", int.Parse(DateTime.ParseExact(date_default, "dd/MM/yyyy", null).ToString("yyyyMMdd")))
                       );

            if (!String.IsNullOrEmpty(to_date))
                query = Query.And(
                        query,
                        Query.LTE("system_time_key.date", int.Parse(DateTime.ParseExact(to_date, "dd/MM/yyyy", null).ToString("yyyyyMMdd")))
                        ); // LTE :  <=

            if (!String.IsNullOrEmpty(busscode))
                query = Query.And(
                    query,
                    Query.Or(
                    Query.EQ("customer.code", busscode),
                    Query.EQ("customer.name", busscode)
                    ));

            PayID.DataHelper.DynamicObj[] ObjShipment = PayID.API.Process.MerchantProcess.Data.List("shipment", query);
            dynamic _province = new DynamicObj();
            dynamic _province_assign = new DynamicObj();
            dynamic _district = new DynamicObj();
            dynamic _district_assign = new DynamicObj();
            dynamic _post_assign = new DynamicObj();
            dynamic _status = new DynamicObj();
            dynamic _traces = new DynamicObj();
            foreach (dynamic ite in ObjShipment)
            {
                if (ite.tracking_code != null)
                {
                    _traces = new DynamicObj();
                    _traces = PayID.Portal.Areas.Lading.Configuration.Data.Get("Lading", Query.EQ("Code", ite.tracking_code.ToString()));

                    if (_traces != null)
                    {
                        if (_traces.MainFee != null)
                        {
                            ite.MainFee = long.Parse(_traces.MainFee.ToString());
                        }
                        else
                        {
                            ite.MainFee = 0;
                        }
                        if (_traces.ServiceFee != null)
                        {
                            ite.ServiceFee = long.Parse(_traces.ServiceFee.ToString());
                        }
                        else
                        {
                            ite.ServiceFee = 0;
                        }
                        if (_traces.CodFee != null)
                        {
                            ite.CodFee = long.Parse(_traces.CodFee.ToString());
                        }
                        else
                        {
                            ite.CodFee = 0;
                        }
                        if (_traces.TotalFee != null)
                        {
                            ite.TotalFee = long.Parse(_traces.TotalFee.ToString());
                        }
                        else
                        {
                            ite.TotalFee = 0;
                        }
                        if (_traces.ReceiverMobile != null)
                        {
                            ite.ReceiverMobile = _traces.ReceiverMobile.ToString();
                        }
                        else
                        {
                            ite.ReceiverMobile = "";
                        }
                        if (_traces.ProductName != null)
                        {
                            ite.ProductName = _traces.ProductName.ToString();
                        }
                        else
                        {
                            ite.ProductName = "";
                        }
                        if (_traces.ProductDescription != null)
                        {
                            ite.ProductDescription = _traces.ProductDescription.ToString();
                        }
                        else
                        {
                            ite.ProductDescription = "";
                        }

                        ite.weight = long.Parse(_traces.Weight == null ? "0" : _traces.Weight.ToString());
                        if (_traces.CollectValue != null)
                        {
                            ite.collectvalue = long.Parse(_traces.CollectValue == null ? "0" : _traces.CollectValue.ToString());
                        }
                        else
                        {
                            if (_traces.ServiceCode != null && _traces.ServiceCode == "COD")
                            {
                                ite.collectvalue = long.Parse(_traces.Value == null ? "0" : _traces.Value.ToString());
                            }
                            else
                            {
                                ite.collectvalue = 0;
                            }
                        }
                    }
                    else
                    {
                        ite.MainFee = 0;
                        ite.ServiceFee = 0;
                        ite.CodFee = 0;
                        ite.TotalFee = 0;
                        if (ite.product.weight != null)
                        {
                            ite.weight = long.Parse(ite.product.weight == null ? "0" : ite.product.weight.ToString());
                        }
                        else
                        {
                            ite.weight = 0;
                        }
                        ite.collectvalue = 0;
                    }
                }
                else
                {
                    ite.MainFee = 0;
                    ite.ServiceFee = 0;
                    ite.CodFee = 0;
                    ite.TotalFee = 0;
                    if (ite.product.weight != null)
                    {
                        ite.weight = long.Parse(ite.product.weight == null ? "0" : ite.product.weight.ToString());
                    }
                    else
                    {
                        ite.weight = 0;
                    }
                    ite.collectvalue = 0;
                }
                if (ite.to_address != null)
                {
                    _province = PayID.Portal.Areas.Lading.Configuration.Data.Get("mbcProvince", Query.EQ("ProvinceCode", ite.to_address.province.ToString()));
                    _district = PayID.Portal.Areas.Lading.Configuration.Data.Get("mbcDistrict", Query.EQ("DistrictCode", ite.to_address.district.ToString()));
                    if (_province != null)
                    {
                        ite.to_province_name = _province.ProvinceName.ToString();
                    }
                    else
                    {
                        ite.to_province_name = "";
                    }
                    if (_district != null)
                    {
                        ite.to_district_name = _district.DistrictName.ToString();
                    }
                    else
                    {
                        ite.to_district_name = "";
                    }
                    ite.DeliveryProvince = ite.to_address.province == null ? "" : ite.to_address.province;
                    ite.DeliveryDistrict = ite.to_address.district == null ? "" : ite.to_address.district;
                    ite.DeliveryPo = ite.to_address.postcode_link == null ? "" : ite.to_address.postcode_link;

                    ite.to_address_name = ite.to_address.name.ToString();
                    ite.to_address_address = ite.to_address.address.ToString();
                    if (ite.to_address.phone != null)
                    {
                        ite.to_address_phone = ite.to_address.phone.ToString();
                    }
                    if (ite.to_address.mobile != null)
                    {
                        ite.to_address_mobile = ite.to_address.mobile.ToString();
                    }
                }

                if (ite.current_assigned != null)
                {
                    if (ite.current_assigned.ToString().Length <= 2)
                    {
                        ite.AssignProvince = ite.current_assigned.ToString();
                        _province_assign = PayID.Portal.Areas.Lading.Configuration.Data.Get("mbcProvince", Query.EQ("ProvinceCode", ite.current_assigned.ToString()));
                        if (_province_assign != null)
                        {
                            ite.AssignProvinceName = ite.current_assigned.ToString() + " - " + _province_assign.ProvinceName;
                            ite.AssignDistrictName = "";
                            ite.AssignPoName = "";
                        }
                        else
                        {
                            ite.AssignProvinceName = "";
                            ite.AssignDistrictName = "";
                            ite.AssignPoName = "";
                        }
                        ite.AssignDistrict = "";
                        ite.AssignPo = "";
                    }
                    else if (ite.current_assigned.ToString().Length <= 7 && ite.current_assigned.ToString().Length > 2)
                    {
                        ite.AssignProvince = ite.current_assigned.ToString().Substring(0, 2);
                        ite.AssignDistrict = ite.current_assigned.ToString().Substring(3, 4);
                        _province_assign = PayID.Portal.Areas.Lading.Configuration.Data.Get("mbcProvince", Query.EQ("ProvinceCode", ite.current_assigned.ToString().Substring(0, 2)));
                        if (_province_assign != null)
                        {
                            ite.AssignProvinceName = ite.current_assigned.ToString().Substring(0, 2) + " - " + _province_assign.ProvinceName;

                        }
                        else
                        {
                            ite.AssignProvinceName = "";

                        }
                        _district_assign = PayID.Portal.Areas.Lading.Configuration.Data.Get("mbcDistrict", Query.EQ("DistrictCode", ite.current_assigned.ToString().Substring(3, 4)));
                        if (_district_assign != null)
                        {
                            ite.AssignDistrictName = ite.current_assigned.ToString().Substring(3, 4) + " - " + _district_assign.DistrictName;
                        }
                        else
                        {
                            ite.AssignDistrictName = "";
                        }
                        ite.AssignPo = "";
                        ite.AssignPoName = "";
                    }
                    else
                    {
                        ite.AssignProvince = ite.current_assigned.ToString().Substring(0, 2);
                        ite.AssignDistrict = ite.current_assigned.ToString().Substring(3, 4);
                        ite.AssignPo = ite.current_assigned.ToString().Substring(8, 6);
                        _province_assign = PayID.Portal.Areas.Lading.Configuration.Data.Get("mbcProvince", Query.EQ("ProvinceCode", ite.current_assigned.ToString().Substring(0, 2)));
                        if (_province_assign != null)
                        {
                            ite.AssignProvinceName = ite.current_assigned.ToString().Substring(0, 2) + " - " + _province_assign.ProvinceName;

                        }
                        else
                        {
                            ite.AssignProvinceName = "";

                        }
                        _district_assign = PayID.Portal.Areas.Lading.Configuration.Data.Get("mbcDistrict", Query.EQ("DistrictCode", ite.current_assigned.ToString().Substring(3, 4)));
                        if (_district_assign != null)
                        {
                            ite.AssignDistrictName = ite.current_assigned.ToString().Substring(3, 4) + " - " + _district_assign.DistrictName;
                        }
                        else
                        {
                            ite.AssignDistrictName = "";
                        }
                        _post_assign = PayID.Portal.Areas.Lading.Configuration.Data.Get("mbcPos", Query.EQ("POSCode", ite.current_assigned.ToString().Substring(8, 6)));
                        if (_post_assign != null)
                        {
                            ite.AssignPoName = ite.current_assigned.ToString().Substring(8, 6) + "-" + _post_assign.POSName;
                        }
                        else
                        {
                            ite.AssignPoName = "";
                        }
                    }
                }
                else
                {
                    ite.AssignProvince = "";
                    ite.AssignDistrict = "";
                    ite.AssignPo = "";
                }

                if (ite.from_address.name != null)
                {
                    ite.sendername = ite.from_address.name.ToString();
                }
                if (ite.customer.code != null)
                {
                    ite.customercode = ite.customer.code.ToString();
                }
                if (ite.customer.name != null)
                {
                    ite.customername = ite.customer.name.ToString();
                }
                if (ite.product != null)
                {
                    ite.quantity = long.Parse(ite.product.quantity != null ? ite.product.quantity.ToString() : "0");
                    ite.value = long.Parse(ite.product.value.ToString());
                    ite.productdescription = ite.product.description.ToString();
                    ite.productname = ite.product.name.ToString();
                }

                if (ite.to_address != null)
                {
                    ite.to_address_name = ite.to_address.name.ToString();
                    ite.to_address_address = ite.to_address.address.ToString();
                    if (ite.to_address.phone != null)
                    {
                        ite.to_address_phone = ite.to_address.phone.ToString();
                    }
                    if (ite.to_address.mobile != null)
                    {
                        ite.to_address_mobile = ite.to_address.mobile.ToString();
                    }
                }
                if (ite.service != null)
                {
                    ite.service = ite.service.cashpost_service.ToString();
                }
                if (ite.system_status != null)
                {
                    _status = new DynamicObj();
                    _status = PayID.Portal.Areas.Lading.Configuration.Data.Get("Status", Query.EQ("StatusCode", ite.system_status.ToString()));

                    if (_status != null)
                        ite.system_status = _status.StatusDescription;

                }
            }
            JObject[] List_Shipment = new JObject[] { };
            List_Shipment = (from e in ObjShipment select e).Select(c => JObject.Parse(JsonConvert.SerializeObject(c.ToExpando()))).ToArray();

            return List_Shipment;


        }
        public JObject GetLogin(string email, string password)
        {
            string _json_response = String.Empty;
            dynamic _response = new ExpandoObject();
            DynamicObj _result = IsAuthenticated(
                    email,
                    password
                );
            if (_result == null)
            {
                _response.ErrorCode = "90";
                _response.ErrorMsg = "Thông tin xác thực người dùng không hợp lệ. Vui lòng kiểm tra lại email và mật khẩu!";
                _response.profile = null;
                // return _response;
                _json_response = JsonConvert.SerializeObject(_response);
                return JObject.Parse(_json_response);
            }

            _response.ErrorCode = "00";
            _response.ErrorMsg = "Người dùng được xác thực";
            _response.profile = JObject.Parse(JsonConvert.SerializeObject(_result.ToExpando()));

            _json_response = JsonConvert.SerializeObject(_response);
            return JObject.Parse(_json_response);
        }

        public static dynamic IsAuthenticated(string email, string password)
        {
            password = Security.CreatPassWordHash(password);
            dynamic _old = PayID.API.Process.MerchantProcess.Data.Get(
                "user",
                Query.And(
                    Query.EQ("_id", email),
                    Query.EQ("password", password)
                ));
            if (_old == null) return null;

            dynamic Func = new DynamicObj();
            dynamic UserFunc = new DynamicObj();
            
            string arrFunc = string.Empty;
            string arrUserFunc = string.Empty;
            string arrUserFuncCate = string.Empty;

            //đẩy màng phân quyền vào object dynamic
            if (_old.unit_type != null)
            {
                string unit_type = _old.unit_type.ToString();
                string userrole = _old.role;

                if (unit_type == "1" || unit_type == "2")
                {
                    UserFunc = PayID.API.Process.MerchantProcess.Data.Get(
           "GeneralUnitUserRole",
         Query.And(Query.EQ("role_code", userrole), Query.EQ("unit_type", unit_type)));
                    if (UserFunc == null)
                    {
                        UserFunc = new DynamicObj();
                        UserFunc = PayID.API.Process.MerchantProcess.Data.Get(
               "GeneralFuntionRole", Query.EQ("unit_code", unit_type));
                    }
                }
                else
                {
                    UserFunc = PayID.API.Process.MerchantProcess.Data.Get(
               "ProvinceUserRole", Query.And(
               Query.EQ("role_code", userrole), Query.EQ("unit_type", unit_type), Query.EQ("provincecode", _old.unit_link.ToString().Substring(0, 2))));
                    if (UserFunc == null)
                    {
                        UserFunc = PayID.API.Process.MerchantProcess.Data.Get(
                                           "GeneralUnitUserRole",
                                         Query.And(Query.EQ("role_code", userrole), Query.EQ("unit_type", unit_type)));
                        if (UserFunc == null)
                        {
                            UserFunc = new DynamicObj();
                            UserFunc = PayID.API.Process.MerchantProcess.Data.Get(
                   "GeneralFuntionRole", Query.EQ("unit_code", unit_type));
                        }
                    }
                }

                if (UserFunc != null)
                {
                    arrUserFunc = UserFunc.function;
                    arrUserFuncCate = UserFunc.module;
                }
                else
                {
                    arrUserFuncCate = "";
                    arrUserFunc = "";
                }
                _old.permission = string.IsNullOrEmpty(arrUserFunc) ? "" : arrUserFunc;
                _old.funcCategory = string.IsNullOrEmpty(arrUserFuncCate) ? "" : arrUserFuncCate;
            }
            return _old;
        }
    }
}
