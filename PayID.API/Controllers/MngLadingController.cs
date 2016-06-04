using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MongoDB.Driver.Builders;
using MongoDB.Driver;
using MongoDB.Bson;
using PayID.DataHelper;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Dynamic;
using PayID.Common;
using System.Configuration;
using System.IO;

namespace PayID.API.Controllers
{
    public class MngLadingController : ApiController
    {
        [AcceptVerbs("POST")]
        public JObject LadingService(string function, JObject request)
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

                    case "change_services":
                        lading_update(_request, out _response);
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
        [AcceptVerbs("POST")]
        public JObject[] GetAllLading(JObject request)
        {
            string date_default = DateTime.Now.AddDays(-15).ToString("dd-MM-yyyy");
            string code, from_date, to_date, status, to_province_code, customer_code;

            code = (request["code"] != null) ? request["code"].ToString() : "";
            to_date = (request["to_date"] != null) ? request["to_date"].ToString() : "";
            from_date = (request["from_date"] != null) ? request["from_date"].ToString() : "";
            status = (request["status"] != null) ? request["status"].ToString() : "";
            to_province_code = (request["to_province_code"] != null) ? request["to_province_code"].ToString() : "";
            customer_code = (request["customer_code"] != null) ? request["customer_code"].ToString() : "";

            IMongoQuery query = Query.NE("_id", 0);
            //query = Query.And(
            //     query,
            //     Query.NE("Delete", 1)
            //     );

            if (!String.IsNullOrEmpty(code))
                query = Query.And(
                    query,
                    Query.EQ("Code", code)
                    );
            if (!String.IsNullOrEmpty(customer_code))
                query = Query.And(
                    query,
                    Query.EQ("CustomerCode", customer_code)
                    );
            if (!String.IsNullOrEmpty(to_province_code))
                query = Query.And(
                    query,
                    Query.EQ("ToProvinceCode", to_province_code)
                    );
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

            DynamicObj[] ObjLading = PayID.Portal.Areas.Lading.Configuration.Data.List("Lading", query);

            JObject[] List_Lading = new JObject[] { };
            dynamic lst = new PayID.DataHelper.DynamicObj();
            List_Lading = (from e in ObjLading select e).Select(c => JObject.Parse(JsonConvert.SerializeObject(c.ToExpando()))).ToArray();
            return List_Lading;
        }

        private static void lading_update(dynamic _request, out dynamic _response)
        {
            _response = new ExpandoObject();
            try
            {
                ProxyController px = new ProxyController();
                IMongoQuery query = Query.EQ("_id", Int64.Parse(_request._id));
                dynamic dyna = PayID.Portal.Areas.Lading.Configuration.Data.Get("Lading", query);
                dynamic dyna_old = new DynamicObj();
                dynamic dynamicObj = new DynamicObj();
                long _totalfee = 0;
                try
                {
                    dynamicObj = px.processChargesInfo(int.Parse(_request.Weight.ToString()), double.Parse(_request.Value.ToString()), _request.Type.ToString(), _request.FromProvinceCode.ToString(), _request.ToProvinceCode.ToString());

                    dynamicObj = dynamicObj.Output[0];
                    _totalfee = long.Parse(string.IsNullOrEmpty(dynamicObj.CodCharges.ToString()) ? "0" : dynamicObj.CodCharges.ToString()) + long.Parse(_request.Value.ToString());
                }
                catch(Exception ex)
                {
                    writelog(ex.ToString());
                    dynamicObj.MainCharges = 0;
                    dynamicObj.PlusServiceCharges = 0;
                    dynamicObj.CodCharges = 0;
                }
                var str = new
                {
                    MainFee = dynamicObj.MainCharges.ToString(),
                    ServiceFee = dynamicObj.PlusServiceCharges.ToString(),
                    CodFee = dynamicObj.CodCharges.ToString(),
                    TotalFee = _totalfee.ToString(),
                    Type= _request.Type
                };
                var str_old = new
                {                     
                    MainFee = _request.MainFee_old,
                    ServiceFee = _request.ServiceFee_old,
                    CodFee = _request.CodFee_old,
                    TotalFee = _request.TotalFee_old,
                    Type= _request.Type_old
                };
                try
                {
                    if (Security.CreatPassWordHash(str.ToString()) != Security.CreatPassWordHash(str_old.ToString()))
                    {

                        dyna.MainFee = long.Parse(string.Format("{0}", dynamicObj.MainCharges.ToString()));
                        dyna.ServiceFee = long.Parse(string.Format("{0}", dynamicObj.PlusServiceCharges.ToString()));
                        dyna.CodFee = long.Parse(string.Format("{0}", dynamicObj.CodCharges.ToString()));
                        dyna.TotalFee = long.Parse(string.Format("{0}", _totalfee.ToString()));
                        dyna.Type = _request.Type;

                        string sDate = DateTime.Now.ToString("yyyyMMdd");
                        dyna_old._id = dyna.Code + sDate + PayID.Portal.Areas.Lading.Configuration.Data.GetNextSquence("LogChangeLading");
                        dyna_old.Code = dyna.Code;
                        dyna_old.MainFee = long.Parse(string.Format("{0}", dynamicObj.MainCharges.ToString()));
                        dyna_old.ServiceFee = long.Parse(string.Format("{0}", dynamicObj.PlusServiceCharges.ToString()));
                        dyna_old.CodFee = long.Parse(string.Format("{0}", dynamicObj.CodCharges.ToString()));
                        dyna_old.TotalFee = long.Parse(string.Format("{0}", _totalfee.ToString()));
                        dyna_old.Type = _request.Type;
                        
                        dyna_old.MainFee_old = long.Parse(string.Format("{0}", _request.MainFee_old));
                        dyna_old.ServiceFee_old = long.Parse(string.Format("{0}", _request.ServiceFee_old));
                        dyna_old.CodFee_old = long.Parse(string.Format("{0}", _request.CodFee_old));
                        dyna_old.TotalFee_old = long.Parse(string.Format("{0}", _request.TotalFee_old));
                        dyna_old.Type = _request.Type_old;

                        PayID.Portal.Areas.Lading.Configuration.Data.Save("Lading", dyna);
                        PayID.Portal.Areas.Lading.Configuration.Data.Save("LogChangeLading", dyna_old);
                        _response.response_code = "00";
                        _response.response_message = "Cập nhật thành công";
                    }
                    else
                    {
                        _response.response_code = "01";
                        _response.response_message = "Dữ liệu không đổi. Vui lòng chọn lại dịch vụ khác";
                    }                    
                }
                catch(Exception ex)
                {
                    writelog(ex.ToString());
                    _response.response_code = "90";
                    _response.response_message = "Có lỗi trong quá trình xử lý. Vui lòng thử lại sau";
                }                
            }
            catch(Exception ex)
            {
                writelog(ex.ToString());
                _response.response_code = "90";
                _response.response_message = "Có lỗi trong quá trình xử lý. Vui lòng thử lại sau";
            }
        }

        public static void writelog(string strLogText)
        {
            //create folder
            string FolderPath = ConfigurationSettings.AppSettings["log"].ToString();
            if (!Directory.Exists(FolderPath))
            {
                CreateDirectory(FolderPath);
            }
            string FolderPathDate = FolderPath + @"\log" + string.Format("{0:ddMMyyyy}", DateTime.Now);
            CreateDirectory(FolderPathDate);
            // Create a writer and open the file:
            string path = "";
            StreamWriter log;
            if (Directory.Exists(FolderPath))
                path = FolderPathDate + @"\LogBatchFile" + string.Format("{0:ddMMyyyy}", DateTime.Now) + ".txt";
            else
                path = @"C:\LogCfm" + string.Format("{0:ddMMyyyy}", DateTime.Now) + ".txt";
            if (!File.Exists(path))
            {
                log = new StreamWriter(path);
            }
            else
            {
                log = File.AppendText(path);
            }

            // Write to the file:
            //log.WriteLine(DateTime.Now);
            log.WriteLine(strLogText);
            //log.WriteLine("==============================================================================================");
            log.WriteLine();

            // Close the stream:
            log.Close();
        }
        public static void CreateDirectory(string DirectoryName)
        {
            try
            {
                // Determine whether the directory exists.

                if (Directory.Exists(DirectoryName))
                {

                    return;

                }
                // Create directory

                DirectoryInfo di = Directory.CreateDirectory(DirectoryName);
            }

            catch (Exception exp)
            {
                writelog(exp.ToString());
            }

        }
    }
}
