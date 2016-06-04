using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Data.OleDb;
using System.IO;
using Newtonsoft.Json.Linq;
using MvcPaging;
using MongoDB.Driver.Builders;
using System.Web.Routing;
using System.Reflection;
using MongoDB.Driver;
using PayID.DataHelper;
using MongoDB.Bson;
using ExcelLibrary.SpreadSheet;
using PayID.Portal.Models;
using PayID.Portal.Common;
using PayID.Portal.Common.Service;

namespace PayID.Portal.Areas.Lading.Controllers
{
    public class ImportLadingController : Controller
    {
        LadingService LADING_SERVICE = null;

        public ImportLadingController()
        {
            if(LADING_SERVICE == null)
            {
                LADING_SERVICE = new LadingService();
            }
        }

        //
        // GET: /Lading/ImportLading/
        public static string FullPathResult = string.Empty;
        public const int defaultPageSize = 5;
        public List<dynamic> list_lading_approve = null;
        public static List<SelectListItem> List_File = new List<SelectListItem>();
        public static List<dynamic> LIST_OBJ_NAME = new List<dynamic>();
        public static List<dynamic> LIST_STORE = new List<dynamic>();
        public static List<dynamic> LIST_CUST = new List<dynamic>();

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Index()
        {
            if (Session["profile"] == null)
            {
                return RedirectToAction("LogOut", "Home", new { Area = "" });
            }
            else
            {
                return View();
            }
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

            }

        }
        [HttpPost]
        public ActionResult Index(string messsage)
        {
            string filename = "";
            string shortName = string.Empty;
            string fullpath = string.Empty;
            string filepath = string.Empty;
            string extension = string.Empty;
            int sStatusIns = 0;

            if (Request.Files["fileupload"] != null && Request.Files["fileupload"].ContentLength > 0)
            {
                extension = System.IO.Path.GetExtension(Request.Files["fileupload"].FileName);
                if (!Directory.Exists(Server.MapPath("~/UploadFiles")))
                    CreateDirectory(Server.MapPath("~/UploadFiles"));

                filename = string.Format("{0}/{1}", Server.MapPath("~/UploadFiles"), Request.Files["fileupload"].FileName);
                if (System.IO.File.Exists(filename))
                    System.IO.File.Delete(filename);

                shortName = Request.Files["fileupload"].FileName.ToString();
                Request.Files["fileupload"].SaveAs(filename);
            }
            if (!string.IsNullOrEmpty(filename))
            {
                int n = filename.Length;
                filepath = filename.Substring(12);
                string path = Path.GetDirectoryName(filename);
                fullpath = path + "\\" + shortName;
                FullPathResult = fullpath;
            }

            if (!string.IsNullOrEmpty(FullPathResult))
            {
                if (extension == ".xls" || extension == "xls")
                {

                    sStatusIns = InsLadingBillTmp(FullPathResult, extension, Request.Files["fileupload"].FileName);

                    if (sStatusIns == 0)
                    {
                        //try
                        //{
                        //    PropertyInfo isreadonly = typeof(System.Collections.Specialized.NameValueCollection).GetProperty(
                        //               "IsReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
                        //    // make collection editable
                        //    isreadonly.SetValue(this.Request.QueryString, false, null);
                        //    // remove
                        //    this.Request.QueryString.Remove("fileupload");
                        //}
                        //catch { }

                        return Redirect("~/Lading/ImportLading/?rs=0");
                    }
                    else
                    {
                        return Redirect("~/Lading/ImportLading/?rs=" + sStatusIns.ToString());
                    }
                }
                else
                {
                    return Redirect("~/Lading/ImportLading/?rs=5");
                }

            }

            //ViewData["ImpCustomer"] = null;

            //ViewData["SuccessMessage"] = "Tạo vận đơn không thành công. Xin kiểm tra lại file tạo vận đơn!";
            return RedirectToAction("index", "ImportLading");
        }
        #region CreateLadingbyExcel
        public dynamic GetStoreById(string Id)
        {
            IMongoQuery query = Query.EQ("_id", int.Parse(Id));
            dynamic dyna = PayID.Portal.Configuration.Data_S24.Get("profile_store", query);

            return dyna;
        }
        public dynamic GetCustomerByCode(string Id)
        {

            IMongoQuery query = Query.EQ("_id", Id);
            dynamic dyna = PayID.Portal.Configuration.Data_S24.Get("business_profile", query);

            return dyna;
        }
        public dynamic shipment(dynamic request)
        {
            string unit_code, unit_name, unit_link;
            dynamic from_address = request.from_address;
            request.system_status = "C5";

            if (!String.IsNullOrEmpty(from_address._id.ToString()))
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
                    if (store.PostCode != null && store.PostCode != "")
                    {
                        dynamic postCode = PayID.Portal.Areas.Metadata.Configuration.Data.Get("mbcPos", Query.EQ("_id", store.PostCode));
                        unit_code = store.PostCode;
                        unit_link = postCode.ProvinceCode + "." + postCode.UnitCode + "." + store.PostCode;
                        unit_name = postCode.POSName.Trim();
                        if (!string.IsNullOrEmpty(store.PostCode))
                            request.system_status = "C6";
                        request.is_assigned = 1;
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

                dynamic province = PayID.Portal.Areas.Metadata.Controllers.ProxyController.GetProvinceName(from_address.province.ToString());
                if (province == null) return JObject.Parse(@"{error_code:'11',error_message:'Invalid From Address Province Code'}");
                unit_code = from_address.province.ToString();
                unit_link = unit_code;
                unit_name = province;
            }

            string prefix = DateTime.Today.ToString("yy") + DateTime.Today.DayOfYear.ToString().PadLeft(3, '0');
            string _id = "10" + prefix + Configuration.Data.GetNextSquence("shipment_request_" + prefix).ToString().PadLeft(5, '0');
            request._id = _id;
            dynamic _comment = new PayID.DataHelper.DynamicObj();
            _comment.at = DateTime.Now.ToString("yyyyMMddHHmmss");
            _comment.by = "Cash@POST API";
            _comment.comment = "Khởi tạo";

            request.comments = new PayID.DataHelper.DynamicObj[]{
                    _comment
                };

            dynamic _assign = new PayID.DataHelper.DynamicObj();
            _assign.assign_by = "Cash@POST API";
            _assign.assign_at = DateTime.Now.ToString("yyyyMMddHHmmss");
            _assign.assign_to_id = unit_code;
            _assign.assign_to_full_name = unit_name;
            request.assigned_to = new PayID.DataHelper.DynamicObj[] { _assign };

            request.current_assigned = unit_link;
            request.created_by = "Cash@POST API";
            request.created_at = DateTime.Now.ToString("yyyyMMddHHmmss");
            request.current_assigned_name = unit_name;

            request.order_id = _id;
            Configuration.Data.Save("shipment", request);
            return JObject.Parse(@"{"
                + "error_code:'00'"
                + ",error_message:'Thành công'"
                + ",tracking_code: '" + request.tracking_code + "'"
                + ",request_code: '" + request._id + "'"
                + ",request_id: '" + request.request_id + "'"
                + ",order_id: '" + request.order_id + "'"
            + "}");
        }
        public bool CreateShippingPickupRequest(dynamic _request)
        {
            string prefix = DateTime.Today.ToString("yy") + DateTime.Today.DayOfYear.ToString().PadLeft(3, '0');

            dynamic myPickupObj = new PayID.DataHelper.DynamicObj();
            myPickupObj.tracking_code = _request._id;
            myPickupObj.refcode = _request._id;
            myPickupObj.from_address = new PayID.DataHelper.DynamicObj();
            myPickupObj.from_address._id = _request.FromAddressId;
            myPickupObj.from_address.name = _request.SenderName;
            myPickupObj.from_address.address = _request.SenderAddress;
            myPickupObj.from_address.district = _request.FromDistrictCode;
            myPickupObj.from_address.province = _request.FromProvinceCode;
            myPickupObj.from_address.ward = _request.FromWardCode;
            myPickupObj.from_address.email = _request.Email;
            myPickupObj.from_address.mobile = _request.SenderMobile;
            myPickupObj.from_address.postcode_link = _request.postcode_link;

            myPickupObj.to_address = new PayID.DataHelper.DynamicObj();
            myPickupObj.to_address._id = "";
            myPickupObj.to_address.name = _request.ReceiverName;
            myPickupObj.to_address.address = _request.ReceiverAddress;
            myPickupObj.to_address.district = _request.ToDistrictCode;
            myPickupObj.to_address.province = _request.ToProvinceCode;
            myPickupObj.to_address.ward = _request.ToWardCode;
            myPickupObj.to_address.email = _request.Email;
            myPickupObj.to_address.mobile = _request.SenderMobile;

            //myPickupObj.order_id = "";
            myPickupObj.request_id = "";
            if (_request.CollectValue != null)
                myPickupObj.collectvalue = _request.CollectValue;

            myPickupObj.product = new PayID.DataHelper.DynamicObj();
            myPickupObj.product.name = _request.ProductName;
            myPickupObj.product.description = _request.ProductDescription;
            myPickupObj.product.value = _request.Value;
            myPickupObj.product.collectvalue = _request.CollectValue;

            myPickupObj.service = new PayID.DataHelper.DynamicObj();
            myPickupObj.service.cashpost_service = 1;
            myPickupObj.service.shipping_main_service = _request.Type;
            myPickupObj.service.shipping_add_service = "";

            myPickupObj.customer = new PayID.DataHelper.DynamicObj();
            myPickupObj.customer.code = _request.CustomerCode;
            myPickupObj.customer.name = _request.SenderName;
            myPickupObj.customer.email = _request.SenderEmail;
            myPickupObj.customer.mobile = _request.SenderMobile;
            myPickupObj.parcel = new PayID.DataHelper.DynamicObj();
            myPickupObj.parcel.weight = long.Parse(_request.Weight == null ? "0" : _request.Weight.ToString());
            myPickupObj.parcel.Long = 0;
            myPickupObj.parcel.Height = 0;
            myPickupObj.parcel.Width = 0;
            shipment(myPickupObj);
            return true;

        }
        public JsonResult CreateLadingWithFiles()
        {
            Return rs = new Return();
            Return rslt = new Return();

            list_lading_approve = new List<dynamic>();
            string _arrImp = "";

            list_lading_approve = GET_LIST_LADING_TMP("", "", _arrImp);
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                foreach (dynamic item in list_lading_approve)
                {
                    string _value = string.Empty;
                    string _weight = string.Empty;
                    string _quantity = string.Empty;
                    string _Long = string.Empty;
                    string _Width = string.Empty;
                    string _Height = string.Empty;
                    string _collectValue = string.Empty;
                    try
                    {
                        if (item.Value != null)
                        {
                            _value = string.IsNullOrEmpty(item.Value.ToString()) ? "0" : item.Value.ToString();
                        }
                        else
                        {
                            _value = "0";
                        }
                    }
                    catch
                    {
                        _value = "0";
                    }
                    try
                    {
                        if (item.CollectValue != null)
                        {
                            _collectValue = string.IsNullOrEmpty(item.CollectValue.ToString()) ? "0" : item.CollectValue.ToString();
                        }
                        else
                        {
                            _collectValue = "0";
                        }
                    }
                    catch
                    {
                        _collectValue = "0";
                    }
                    try
                    {
                        if (item.Weight != null)
                        {
                            _weight = string.IsNullOrEmpty(item.Weight.ToString()) ? "0" : item.Weight.ToString();
                        }
                        else
                        {
                            _weight = "0";
                        }
                    }
                    catch
                    {
                        _weight = "0";
                    }

                    try
                    {
                        if (item.Quantity != null)
                        {
                            _quantity = string.IsNullOrEmpty(item.Quantity.ToString()) ? "0" : item.Quantity.ToString();
                        }
                        else
                        {
                            _quantity = "0";
                        }
                    }
                    catch
                    {
                        _quantity = "0";
                    }


                    string SenderName = string.IsNullOrEmpty(item.SenderName.ToString()) ? "" : item.SenderName;
                    string SenderAddress = string.IsNullOrEmpty(item.SenderAddress.ToString()) ? "" : item.SenderAddress;
                    string SenderMobile = string.IsNullOrEmpty(item.SenderMobile.ToString()) ? "" : item.SenderMobile;
                    string ReceiverName = string.IsNullOrEmpty(item.ReceiverName.ToString()) ? "" : item.ReceiverName;
                    string ReceiverAddress = string.IsNullOrEmpty(item.ReceiverAddress.ToString()) ? "" : item.ReceiverAddress;
                    string ReceiverMobile = string.IsNullOrEmpty(item.ReceiverMobile.ToString()) ? "" : item.ReceiverMobile;
                    string FromProvinceCode = string.IsNullOrEmpty(item.FromProvinceCode.ToString()) ? "" : item.FromProvinceCode;
                    string FromDistrictCode = string.IsNullOrEmpty(item.FromDistrictCode.ToString()) ? "" : item.FromDistrictCode;
                    string ToProvinceCode = string.IsNullOrEmpty(item.ToProvinceCode.ToString()) ? "" : item.ToProvinceCode;
                    string ToDistrictCode = string.IsNullOrEmpty(item.ToDistrictCode.ToString()) ? "" : item.ToDistrictCode;
                    string ToWardCode = string.IsNullOrEmpty(item.ToWardCode.ToString()) ? "" : item.ToWardCode;
                    string ProductName = string.IsNullOrEmpty(item.ProductName.ToString()) ? "" : item.ProductName;
                    string ProductDescription = string.IsNullOrEmpty(item.ProductDescription.ToString()) ? "" : item.ProductDescription;
                    string FromAddressId = string.IsNullOrEmpty(item.StoreCode.ToString()) ? "" : item.StoreCode.ToString();
                    string postcode_link = string.IsNullOrEmpty(item.PostCode.ToString()) ? "" : item.PostCode.ToString();
                    string CustomerCode = string.IsNullOrEmpty(item.CustomerCode.ToString()) ? "" : item.CustomerCode.ToString();
                    string UserCreate = string.IsNullOrEmpty(item.UserCreate.ToString()) ? "" : item.UserCreate.ToString();
                    string ServiceCode = string.IsNullOrEmpty(item.ServiceCode.ToString()) ? "" : item.ServiceCode.ToString();
                    string Type = string.IsNullOrEmpty(item.Type.ToString()) ? "" : item.Type.ToString();
                    string status = "C5";
                    if (!string.IsNullOrEmpty(FromAddressId))
                    {
                        dynamic store = PayID.Portal.Areas.Merchant.Configuration.Data.Get("profile_store",
                   Query.And(
                   Query.EQ("StoreCode", FromAddressId),
                   Query.EQ("UserId", CustomerCode)
                   )
                   );
                        if (store != null)
                        {

                            if (store.PostCode != null && store.PostCode != "")
                            {
                                if (!string.IsNullOrEmpty(store.PostCode))
                                    status = "C6";
                            }
                        }

                    }
                    var obj_request = new
                    {
                        Value = string.IsNullOrEmpty(_value.Replace(".", "").Replace(",", "")) ? 0 : long.Parse(_value.Replace(".", "").Replace(",", "")),
                        Weight = string.IsNullOrEmpty(_weight.Replace(".", "").Replace(",", "")) ? 0 : long.Parse(_weight.Replace(".", "").Replace(",", "")),
                        Quantity = string.IsNullOrEmpty(_quantity.Replace(".", "").Replace(",", "")) ? 0 : long.Parse(_quantity.Replace(".", "").Replace(",", "")),
                        CollectValue = string.IsNullOrEmpty(_collectValue.Replace(".", "").Replace(",", "")) ? 0 : long.Parse(_collectValue.Replace(".", "").Replace(",", "")),
                        SenderName = SenderName,
                        SenderAddress = SenderAddress,
                        SenderMobile = SenderMobile,
                        ReceiverName = ReceiverName,
                        ReceiverAddress = ReceiverAddress,
                        ReceiverMobile = ReceiverMobile,
                        CustomerCode = CustomerCode,
                        FromProvinceCode = FromProvinceCode,
                        FromDistrictCode = FromDistrictCode,
                        ToProvinceCode = ToProvinceCode,
                        ToDistrictCode = ToDistrictCode,
                        ToWardCode = ToWardCode,
                        Status = status,
                        ProductName = ProductName,
                        ProductDescription = ProductDescription,
                        Type = int.Parse(Type),
                        ServiceCode = "",
                        FromAddressId = FromAddressId,
                        postcode_link = postcode_link,
                        MainFee = 0,
                        ServiceFee = 0,
                        TotalFee = 0,
                        CodFee = 0,
                        UserCreate = UserCreate
                    };

                    var response = client.PostAsJsonAsync("api/Lading?function=create_lading", obj_request).Result;
                    rs = response.Content.ReadAsAsync<Return>().Result;

                    if (rs.response_code == "00")
                    {
                        //xoa ban ghi trên bảng LadingBillsTmp
                        var obj_requests = new
                        {
                            _id = item._id,
                            Status = item.Status,
                            Delete = 1,
                            ProductName = item.ProductName,
                            Value = item.Value,
                            Weight = item.Weight,
                            Quantity = item.Quantity,
                            ProductDescription = item.ProductDescription,
                            ReiceiverName = item.ReceiverName,
                            ReceiverAddress = item.ReceiverAddress,
                            ReceiverMobile = item.ReceiverMobile,
                            ToProvinceCode = item.ToProvinceCode
                        };
                        var responses = client.PostAsJsonAsync("api/LadingImport?function=update_lading", obj_requests).Result;
                        rslt = responses.Content.ReadAsAsync<Return>().Result;

                        dynamic myNewObj = new DynamicObj();
                        myNewObj = new PayID.DataHelper.DynamicObj(MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(JsonConvert.SerializeObject(obj_request)));
                        myNewObj._id = rs.Code;

                        try
                        {
                            bool myValue = CreateShippingPickupRequest(myNewObj);
                        }
                        catch
                        {

                        }
                        finally
                        { }
                    }

                }
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region create lading tmp
        public int InsLadingBillTmp(string FullPathResult, string extension, string filename)
        {
            if ((dynamic)Session["profile"] == null)
            {
                RedirectToAction("Login", "Home");
            }

            int obj = 0;
            string SourceConstr = string.Empty;
            string fromname = string.Empty;
            string frommobile = string.Empty;
            string fromaddress = string.Empty;
            string fromprovincecode = string.Empty;
            string fromDistrictCode = string.Empty;
            string StoreCode = string.Empty;
            string postCode = string.Empty;
            string _status = "C5";
            string _id = "";
            string _customercode = "";
            try
            {
                _id = Request.Form["dropStore"];
                _customercode = Request.Form["txtCustomer"];
            }
            catch { }

            //lay thong tin kho
            dynamic lstStore = new DynamicObj();
            if (!string.IsNullOrEmpty(_id))
            {
                try
                {
                    lstStore = GetStoreById(_id);

                    fromaddress = lstStore.Address;
                    fromDistrictCode = lstStore.DistrictCode.ToString();
                    fromprovincecode = lstStore.ProvinceCode.ToString();
                    StoreCode = lstStore.StoreCode.ToString();
                    postCode = string.IsNullOrEmpty(lstStore.PostCode) ? "" : lstStore.PostCode;
                }
                catch
                {
                    fromaddress = "";
                    fromDistrictCode = "";
                    fromprovincecode = "";
                    StoreCode = "";
                    postCode = "";
                }
            }

            //lay thong tin khách hàng
            dynamic lstCustomer = new DynamicObj();
            if (!string.IsNullOrEmpty(_customercode))
            {
                try
                {
                    lstCustomer = GetCustomerByCode(_customercode);

                    fromname = lstCustomer.general_full_name;
                    frommobile = lstCustomer.contact_phone_work;
                }
                catch
                {

                }
            }

            if (!string.IsNullOrEmpty(FullPathResult))
            {
                Workbook book = Workbook.Load(FullPathResult);
                Worksheet sheet = book.Worksheets[0];

                string weight = "0";
                string quantity = "0";

                string toname = string.Empty;
                string toaddress = string.Empty;
                string tomobile = string.Empty;
                string customercode = string.Empty;
                string value = "0";
                int type = 0;
                string errtype = "";
                string toprovincecode = string.Empty;
                string productname = string.Empty;
                string productdescription = string.Empty;
                string note = string.Empty;
                string orderId = string.Empty;
                string code = string.Empty;
                string Amount = string.Empty;
                //int type = 0;
                string size_pkg = string.Empty;
                string transportCode = string.Empty;

                string toDistrictCode = string.Empty;
                string toWardCode = string.Empty;
                string fileno = string.Empty;
                string size = string.Empty;

                for (int rowIndex = sheet.Cells.FirstRowIndex + 8;
                         rowIndex <= sheet.Cells.LastRowIndex; rowIndex++)
                {
                    weight = "0";
                    quantity = "0";
                    toname = string.Empty;
                    toaddress = string.Empty;
                    tomobile = string.Empty;
                    customercode = string.Empty;
                    value = "0";
                    type = 0;
                    errtype = "";
                    toprovincecode = string.Empty;
                    productname = string.Empty;
                    productdescription = string.Empty;
                    note = string.Empty;
                    orderId = string.Empty;
                    code = string.Empty;
                    Amount = string.Empty;
                    size_pkg = string.Empty;
                    transportCode = string.Empty;
                    toDistrictCode = string.Empty;
                    toWardCode = string.Empty;
                    fileno = string.Empty;
                    size = string.Empty;

                    Row row = sheet.Cells.GetRow(rowIndex);
                    try
                    {
                        if (row.LastColIndex > 0)
                        {
                            for (int colIndex = row.FirstColIndex + 1;
                               colIndex <= row.LastColIndex; colIndex++)
                            {
                                Cell cell = row.GetCell(colIndex);
                                switch (colIndex)
                                {
                                    case 1:
                                        errtype = cell.StringValue;
                                        break;
                                    case 2:
                                        productname = cell.StringValue;
                                        break;
                                    case 3:
                                        weight = cell.StringValue;
                                        break;
                                    case 5:
                                        value = cell.StringValue;
                                        break;
                                    case 6:
                                        toname = cell.StringValue;
                                        break;
                                    case 7:
                                        tomobile = cell.StringValue;
                                        break;
                                    case 8:
                                        toaddress = cell.StringValue;
                                        break;
                                    case 9:
                                        toprovincecode = string.IsNullOrEmpty(cell.StringValue) ? "" : cell.StringValue.Substring(0, 2);
                                        break;
                                    case 10:
                                        Amount = cell.StringValue;
                                        break;
                                }
                            }

                            if (string.IsNullOrEmpty(errtype))
                            {
                                return 9;
                            }

                            if (string.IsNullOrEmpty(productname.ToString()))
                            {
                                return 11;
                            }

                            if (string.IsNullOrEmpty(weight))
                            {
                                return 2;
                            }
                            else
                            {
                                long n;
                                bool isNumeric = long.TryParse(weight, out n);
                                if (!isNumeric)
                                {
                                    return 6;
                                }
                                if (n < 0)
                                {
                                    return 17;
                                }
                                if (n > 30000)
                                {
                                    return 18;
                                }
                            }


                            if (string.IsNullOrEmpty(value))
                            {
                                return 7;
                            }
                            else
                            {
                                long n;
                                bool isNumeric = long.TryParse(value, out n);
                                if (!isNumeric)
                                {
                                    return 8;
                                }

                                if (n < 0)
                                {
                                    return 16;
                                }
                            }

                            if (string.IsNullOrEmpty(toname))
                            {
                                return 1;
                            }

                            if (string.IsNullOrEmpty(tomobile))
                            {
                                return 22;
                            }
                            else
                            {
                                if (tomobile.Contains(','))
                                {
                                    string[] arrMobile = tomobile.Split(',');
                                    if (arrMobile != null && arrMobile.Length > 0)
                                    {
                                        foreach (string mb in arrMobile)
                                        {
                                            if (mb.Length < 10 || mb.Length > 11)
                                                return 23;

                                            long n;
                                            bool isNumeric = long.TryParse(mb, out n);
                                            if (!isNumeric)
                                            {
                                                return 24;
                                            }
                                            if (n < 0)
                                            {
                                                return 25;
                                            }

                                        }
                                    }
                                }


                            }

                            if (string.IsNullOrEmpty(toaddress.ToString()))
                            {
                                return 19;
                            }

                            if (string.IsNullOrEmpty(toprovincecode.ToString()))
                            {
                                return 12;
                            }

                            if (string.IsNullOrEmpty(Amount))
                            {
                                return 20;
                            }
                            else
                            {
                                long n;
                                bool isNumeric = long.TryParse(Amount, out n);
                                if (!isNumeric)
                                {
                                    return 27;
                                }
                                if (n < 0)
                                {
                                    return 21;
                                }

                            }

                        }
                        else
                        {
                            toname = "";
                        }
                    }
                    catch { return 3; }
                }


                // traverse rows by Index
                for (int rowIndex = sheet.Cells.FirstRowIndex + 8;
                       rowIndex <= sheet.Cells.LastRowIndex; rowIndex++)
                {
                    Row row = sheet.Cells.GetRow(rowIndex);
                    for (int colIndex = row.FirstColIndex + 1;
                       colIndex <= row.LastColIndex; colIndex++)
                    {
                        Cell cell = row.GetCell(colIndex);
                        switch (colIndex)
                        {
                            case 1:
                                type = int.Parse(cell.StringValue == null ? "" : cell.StringValue.Substring(0, 1));
                                break;
                            case 2:
                                productname = cell.StringValue;
                                break;
                            case 3:
                                weight = cell.StringValue;
                                break;
                            case 4:
                                size_pkg = cell.StringValue;
                                break;
                            case 5:
                                value = cell.StringValue;
                                break;
                            case 6:
                                toname = cell.StringValue;
                                break;
                            case 7:
                                tomobile = cell.StringValue;
                                break;
                            case 8:
                                toaddress = cell.StringValue;
                                break;
                            case 9:
                                toprovincecode = string.IsNullOrEmpty(cell.StringValue) ? "" : cell.StringValue.Substring(0, 2);
                                break;

                            case 10:
                                Amount = cell.StringValue;
                                break;
                        }
                    }
                    if (rowIndex <= sheet.Cells.LastRowIndex)
                    {
                        if (weight != "0")
                        {
                            if (!string.IsNullOrEmpty(postCode))
                            {
                                _status = "C6";
                            }
                            string[] arrPrName = null;
                            if (productname.Contains(","))
                            {
                                arrPrName = productname.Split(',');
                            }
                            var obj_request = new
                            {
                                Value = string.IsNullOrEmpty(value.Replace(".", "").Replace(",", "")) ? 0 : long.Parse(value.Replace(".", "").Replace(",", "")),
                                Weight = string.IsNullOrEmpty(weight.Replace(".", "").Replace(",", "")) ? 0 : long.Parse(weight.Replace(".", "").Replace(",", "")),
                                Quantity = string.IsNullOrEmpty(quantity.Replace(".", "").Replace(",", "")) ? 0 : long.Parse(quantity.Replace(".", "").Replace(",", "")),
                                CollectValue = string.IsNullOrEmpty(Amount.Replace(".", "").Replace(",", "")) ? 0 : long.Parse(Amount.Replace(".", "").Replace(",", "")),
                                SenderName = fromname,
                                SenderAddress = fromaddress,
                                SenderMobile = frommobile,
                                ReceiverName = toname,
                                ReceiverAddress = toaddress,
                                ReceiverMobile = tomobile,
                                CustomerCode = _customercode,
                                FromProvinceCode = fromprovincecode,
                                FromDistrictCode = fromDistrictCode,
                                ToProvinceCode = toprovincecode,
                                ToDistrictCode = "",
                                ToWardCode = "",
                                Status = _status,
                                ProductName = arrPrName == null ? productname : arrPrName[0].ToString(),
                                ProductDescription = arrPrName == null ? "" : arrPrName[1].ToString(),
                                Type = type,
                                FromAddressId = StoreCode,
                                postcode_link = postCode,
                                UserCreate = ((dynamic)Session["profile"]) != null ? ((dynamic)Session["profile"])._id : ""
                            };
                            Configuration.urlAPI = System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"].ToString();
                            Return rs = new Return();
                            var client = new HttpClient();
                            client.BaseAddress = new Uri(Configuration.urlAPI);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            try
                            {
                                var response = client.PostAsJsonAsync("api/LadingImport?function=create_lading_tmp", obj_request).Result;
                                rs = response.Content.ReadAsAsync<Return>().Result;

                            }
                            catch
                            {
                                obj = 3;
                            }
                            finally
                            { }

                        }

                    }
                }
                obj = 0;
            }
            else
            {
                obj = 4;
            }

            return obj;

        }
        public static List<SelectListItem> ListSelectFileName(IList<dynamic> list)
        {
            #region Province
            try
            {
                List<SelectListItem> items = new List<SelectListItem>();
                foreach (var p in list)
                {
                    items.Add(new SelectListItem
                    {
                        Text = p.FileNo,
                        Value = p.FileNo,

                    }
                        );
                }

                List_File = items;
            }
            catch { List_File = null; }
            #endregion
            return List_File;
        }
        public JsonResult DeleteLadingTmpFromList(int id)
        {
            Return rs = new Return();
            dynamic LadingItem = null;

            try
            {
                LadingItem = GetLadingBillById(id.ToString());
            }
            catch
            {
                LadingItem = null;
            }
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var obj_request = new
                {
                    _id = id,
                    Status = LadingItem.Status,
                    Delete = 1,
                    ProductName = LadingItem.ProductName,
                    Value = LadingItem.Value,
                    Weight = LadingItem.Weight,
                    Quantity = LadingItem.Quantity,
                    ProductDescription = LadingItem.ProductDescription,
                    ReiceiverName = LadingItem.ReceiverName,
                    ReceiverAddress = LadingItem.SenderAddress,
                    ReceiverMobile = LadingItem.ReceiverMobile,
                    ToProvinceCode = LadingItem.ToProvinceCode
                };
                var response = client.PostAsJsonAsync("api/LadingImport?function=update_lading", obj_request).Result;
                rs = response.Content.ReadAsAsync<Return>().Result;
            }
            return Json(rs, JsonRequestBehavior.AllowGet);
        }
        public dynamic GetLadingBillById(string id)
        {
            IMongoQuery query = Query.EQ("_id", Int64.Parse(id));
            dynamic dyna = Configuration.Data.Get("LadingBillsTmp", query);
            return dyna;
        }
        public ActionResult GetListLadingBillTmp(string receivername, string mobile, string file, int? page)
        {
            IList<dynamic> LIST = GET_LIST_LADING_TMP(string.IsNullOrEmpty(receivername) ? "" : receivername.Trim(), string.IsNullOrEmpty(mobile) ? "" : mobile.Trim(), file);

            #region
            if (LIST != null && LIST.Count > 0)
            {
                ViewBag.total_item = LIST.Count;
            }
            else
            {
                ViewBag.total_item = 0;
            }
            ViewBag.total_page = (LIST.Count + defaultPageSize - 1) / defaultPageSize;
            int currentPageIndex = page.HasValue ? page.Value : 1;
            LIST = LIST.ToPagedList(currentPageIndex, defaultPageSize);

            List<string> listStatus = new List<string>();
            List<string> listProvinceName = new List<string>();


            for (int i = 0; i < LIST.Count; i++)
            {
                ViewBag.ToProvinceName = PayID.Portal.Areas.Merchant.Configuration.GetNameProvinceByProvinceCode(LIST[i].ToProvinceCode);
                ViewBag.Status = PayID.Portal.Areas.Merchant.Configuration.GetStatusDescriptionByStatusCode(LIST[i].Status);
            }

            #endregion
            return View(LIST);
        }
        public List<dynamic> GET_LIST_LADING_TMP(string receivername, string mobile, string file)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var mylading = new
                {
                    receivername = receivername,
                    mobile = mobile,
                    fileno = file,
                    usercreated = ((dynamic)Session["profile"]) != null ? ((dynamic)Session["profile"])._id : ""
                };

                List<JObject> list_obj = new List<JObject>();
                List<dynamic> lstLading = new List<dynamic>();
                //list_lading_approve = new List<Lading>();
                try
                {
                    var response = client.PostAsJsonAsync("api/LadingImport", mylading).Result;
                    list_obj = response.Content.ReadAsAsync<IEnumerable<JObject>>().Result.ToList();

                    foreach (JObject item in list_obj)
                    {
                        dynamic itemLading = new DynamicObj();
                        try
                        {
                            itemLading._id = int.Parse(item["_id"].ToString());

                            itemLading.CodFee = 0;
                            itemLading.Type = item["Type"].ToString();
                            itemLading.CustomerCode = item["CustomerCode"].ToString();
                            itemLading.DateCreated = DateTime.Parse(item["DateCreated"].ToString());
                            //itemLading.Delete = int.Parse(item["Delete"].ToString());
                            //itemLading.FileCode = string.IsNullOrEmpty(item["FileCode"].ToString()) ? "" : item["FileCode"].ToString();
                            //itemLading.FileNo = string.IsNullOrEmpty(item["FileNo"].ToString()) ? "" : item["FileNo"].ToString();
                            itemLading.FromDistrictCode = string.IsNullOrEmpty(item["FromDistrictCode"].ToString()) ? "" : item["FromDistrictCode"].ToString();
                            itemLading.FromProvinceCode = string.IsNullOrEmpty(item["FromProvinceCode"].ToString()) ? "" : item["FromProvinceCode"].ToString();
                            itemLading.MainFee = 0;

                            if (item["CollectValue"] != null)
                            {
                                itemLading.CollectValue = item["CollectValue"].ToString();
                            }
                            else
                            {
                                itemLading.CollectValue = "0";
                            }

                            //itemLading.PackagingType = item["PackagingType"].ToString();
                            //itemLading.PickupMethod = item["PickupMethod"].ToString();
                            itemLading.ProductDescription = string.IsNullOrEmpty(item["ProductDescription"].ToString()) ? "" : item["ProductDescription"].ToString();
                            itemLading.ProductName = string.IsNullOrEmpty(item["ProductName"].ToString()) ? "" : item["ProductName"].ToString();
                            itemLading.Quantity = int.Parse(item["Quantity"].ToString());
                            itemLading.ReceiverAddress = string.IsNullOrEmpty(item["ReceiverAddress"].ToString()) ? "" : item["ReceiverAddress"].ToString();
                            itemLading.ReceiverMobile = string.IsNullOrEmpty(item["ReceiverMobile"].ToString()) ? "" : item["ReceiverMobile"].ToString();
                            itemLading.ReceiverName = string.IsNullOrEmpty(item["ReceiverName"].ToString()) ? "" : item["ReceiverName"].ToString();
                            itemLading.SenderAddress = string.IsNullOrEmpty(item["SenderAddress"].ToString()) ? "" : item["SenderAddress"].ToString();
                            itemLading.SenderMobile = string.IsNullOrEmpty(item["SenderMobile"].ToString()) ? "" : item["SenderMobile"].ToString();
                            itemLading.SenderName = string.IsNullOrEmpty(item["SenderName"].ToString()) ? "" : item["SenderName"].ToString();
                            itemLading.ServiceCode = "";// string.IsNullOrEmpty(item["ServiceCode"].ToString()) ? "" : item["ServiceCode"].ToString();
                            itemLading.ServiceFee = 0;
                            itemLading.Status = item["Status"].ToString();
                            //itemLading.system_created_time = DateTime.Parse(item["system_created_time"].ToString());
                            itemLading.ToDistrictCode = string.IsNullOrEmpty(item["ToDistrictCode"].ToString()) ? "" : item["ToDistrictCode"].ToString();
                            itemLading.ToProvinceCode = string.IsNullOrEmpty(item["ToProvinceCode"].ToString()) ? "" : item["ToProvinceCode"].ToString();
                            itemLading.TotalFee = 0;
                            itemLading.ToWardCode = string.IsNullOrEmpty(item["ToWardCode"].ToString()) ? "" : item["ToWardCode"].ToString();
                            itemLading.StoreCode = string.IsNullOrEmpty(item["FromAddressId"].ToString()) ? "" : item["FromAddressId"].ToString();
                            itemLading.PostCode = string.IsNullOrEmpty(item["postcode_link"].ToString()) ? "" : item["postcode_link"].ToString();
                            itemLading.Value = long.Parse(item["Value"].ToString());
                            itemLading.Weight = int.Parse(item["Weight"].ToString());
                            //itemLading.Unit_link = string.IsNullOrEmpty(item["Unit_link"].ToString()) ? "" : item["Unit_link"].ToString();
                            itemLading.UserCreate = string.IsNullOrEmpty(item["UserCreate"].ToString()) ? "" : item["UserCreate"].ToString();

                            lstLading.Add(itemLading);
                        }
                        catch
                        {
                            lstLading = null;
                        }

                    }
                    if (!string.IsNullOrEmpty(file))
                    {
                        if (lstLading.Count > 0 && lstLading != null)
                            lstLading = (from c in lstLading
                                         where c.CustomerCode == Session["CustomerCode"].ToString()
                                         && c.Status == "C1"
                                         && c.Delete == 0
                                         && c.FileNo == file
                                         orderby c.DateCreated descending
                                         select c).ToList();
                    }
                    else
                    {
                        if (lstLading.Count > 0 && lstLading != null)
                            lstLading = (from c in lstLading
                                         where c.CustomerCode == Session["CustomerCode"].ToString()
                                         && c.Status == "C1"
                                         && c.Delete == 0
                                         && c.FileNo != ""
                                         orderby c.DateCreated descending
                                         select c).ToList();
                    }
                }
                catch { }
                return lstLading;
            }
        }
        public JsonResult CancelLadingWithFiles(string filename)
        {
            Return rs = new Return();
            list_lading_approve = new List<dynamic>();
            string file_name = "";
            if (!string.IsNullOrEmpty(filename))
            {
                file_name = filename;
            }
            list_lading_approve = GET_LIST_LADING_TMP("", "", file_name);
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                foreach (dynamic item in list_lading_approve)
                {
                    var obj_request = new
                    {
                        _id = item._id,
                        Status = item.Status,
                        Delete = 1,
                        ProductName = item.ProductName,
                        Value = item.Value,
                        Weight = item.Weight,
                        Quantity = item.Quantity,
                        ProductDescription = item.ProductDescription,
                        ReiceiverName = item.ReceiverName,
                        ReceiverAddress = item.ReceiverAddress,
                        ReceiverMobile = item.ReceiverMobile,
                        ToProvinceCode = item.ToProvinceCode
                    };
                    var response = client.PostAsJsonAsync("api/LadingImport?function=update_lading", obj_request).Result;
                    rs = response.Content.ReadAsAsync<Return>().Result;
                }
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        public static List<SelectListItem> GetStoreByUserId(string UserId)
        {
            try
            {
                IMongoQuery _qStore = Query.EQ("UserId", UserId);
                LIST_STORE = new List<dynamic>();
                LIST_STORE.AddRange(PayID.Portal.Configuration.Data_S24.List("profile_store", _qStore));

                List<SelectListItem> items = new List<SelectListItem>();
                foreach (var p in LIST_STORE)
                {
                    items.Add(new SelectListItem
                    {
                        Text = p.StoreName,
                        Value = p._id.ToString()
                    }
                        );
                }

                return items;
            }
            catch { return null; }
        }
        public List<SelectListItem> GetCustomer()
        {
            try
            {
                IMongoQuery _qStore = Query.NE("_id", "");

                if ((dynamic)Session["profile"] != null && ((dynamic)Session["profile"]).unit_link != null)
                {
                    if (((dynamic)Session["profile"]).unit_code != "00")
                    {
                        _qStore = Query.And(_qStore, Query.EQ("contact_address_province", ((dynamic)Session["profile"]).unit_link.ToString().Substring(0, 2)));
                    }
                }
                LIST_CUST = new List<dynamic>();
                LIST_CUST.AddRange(PayID.Portal.Configuration.Data_S24.List("business_profile", Query.And(Query.EQ("active", true), _qStore)));

                List<SelectListItem> items = new List<SelectListItem>();
                foreach (var p in LIST_CUST)
                {
                    items.Add(new SelectListItem
                    {

                        Text = p.general_full_name,
                        Value = p._id.ToString()

                    }
                        );
                }

                return items;
            }
            catch { return null; }
        }
        public JsonResult GetStore(string UserId)
        {
            if (!string.IsNullOrEmpty(UserId))
            {
                var listStore = GetStoreByUserId(UserId);
                return Json(new SelectList(listStore, "Value", "Text"), JsonRequestBehavior.AllowGet);
            }
            return null;
        }
        public JsonResult GetBusiness()
        {

            var listBusi = GetCustomer();
            return Json(new SelectList(listBusi, "Value", "Text"), JsonRequestBehavior.AllowGet);

        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult CreateLading(string lading, string fileName, string customerCode)
        {
            JsonResult jResult = new JsonResult();

            try
            {
                if(Session["profile"] == null)
                {
                    jResult = Json(new { Code = "-101", Mes = "Đăng nhập lại trước khi thực hiện thao tác." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    Account oProfile = (Account)Session["profile"];

                    List<Models.Lading> ListLading = new List<Models.Lading>();
                    dynamic _request = JObject.Parse(lading);

                    foreach(var item in _request.Lading)
                    {
                        Models.Lading oLading = new Models.Lading();

                        oLading.Id = long.Parse("0" + (item.Id ?? "0").ToString());
                        oLading.Weight = long.Parse("0" + (item.Weight ?? "0").ToString());
                        oLading.Quantity = int.Parse("0" + (item.Quantity ?? "0").ToString());
                        oLading.CustomerCode = (item.CustomerCode ?? "").ToString();
                        oLading.StoreCode = (item.StoreCode ?? "").ToString();
                        oLading.SenderName = (item.SenderName ?? "").ToString();
                        oLading.SenderAddress = (item.SenderAddress ?? "").ToString();
                        oLading.SenderMobile = (item.SenderMobile ?? "").ToString();
                        oLading.ToProvinceCode = (item.ToProvinceCode ?? "").ToString();
                        oLading.ToDistrictCode = (item.ToDistrictCode ?? "").ToString();
                        oLading.ToWardCode = (item.ToWardCode ?? "").ToString();
                        oLading.ReceiverName = (item.Receiver_Name ?? "").ToString();
                        oLading.ReceiverAddress = (item.Receiver_Street ?? "").ToString();
                        oLading.ReceiverMobile = (item.Receiver_Mobile ?? "").ToString();
                        oLading.FromProvinceCode = (item.ReceiverProvinceId ?? "").ToString();
                        oLading.FromDistrictCode = (item.ReceiverDistrictId ?? "").ToString();
                        oLading.ToWardCode = (item.ReceiverWardId ?? "").ToString();
                        oLading.ProductName = (item.ProductName ?? "").ToString();
                        oLading.ProductDescription = (item.ProductDescription ?? "").ToString();
                        oLading.ServiceCode = (item.ServiceCode ?? "").ToString();
                        oLading.Type = (item.Type ?? "").ToString();
                        oLading.Value = long.Parse("0" + (item.Value ?? "0").ToString());
                        oLading.CollectValue = long.Parse("0" + (item.CollectValue ?? "0").ToString());
                        oLading.FileNo = (item.FileName ?? "").ToString();
                        oLading.Height = (item.Height ?? "").ToString();
                        oLading.Channel = (item.Channel ?? "").ToString();
                        oLading.Check = Help.ToBoolean((item.Check ?? "false").ToString());
                        oLading.IsConsorShip = Help.ToBoolean((item.IsConsorShip ?? "false").ToString());
                        oLading.UnitCreate = oProfile.UnitCode;
                        oLading.UserCreate = oProfile.UserName;

                        ListLading.Add(oLading);
                    }

                    FileUp oFile = new FileUp();
                    oFile.CustomerCode = customerCode;
                    oFile.FileName = fileName;
                    oFile.UnitCode = oProfile.UnitCode;
                    oFile.UnitUpName = oProfile.UnitName;
                    oFile.UserUp = oProfile.UserName;

                    UpFileLading oUpLoad = new UpFileLading();

                    oUpLoad.ListLading = ListLading;
                    oUpLoad.FileUp = oFile;

                    string sReturn = LADING_SERVICE.LadingCreate(oUpLoad);

                    string Code = sReturn.Split('|')[0].ToString();
                    string Mes = sReturn.Split('|')[1].ToString();

                    jResult = Json(new { Code = Code, Mes = Mes }, JsonRequestBehavior.AllowGet);
                }
            }
            catch(Exception ex)
            {
                jResult = Json(new { Code = "-99", Mes = "Có lỗi trong quá trình xử lý dữ liệu." }, JsonRequestBehavior.AllowGet);
            }

            return jResult;
        }

        public bool CheckStructureFile(string fullpath, string extention, HttpPostedFileBase file, ref string Mes)
        {
            bool bResult = false;
            try
            {
                Workbook book = Workbook.Load(fullpath);
                Worksheet sheet = book.Worksheets[0];

                for (int rowIndex = sheet.Cells.FirstRowIndex + 1; rowIndex <= sheet.Cells.LastRowIndex; rowIndex++)
                {
                    Row row = sheet.Cells.GetRow(rowIndex);

                    if (row.LastColIndex > 0 && rowIndex == 6)
                    {
                        Shipment oShipment = new Shipment();

                        for (int colIndex = row.FirstColIndex; colIndex <= row.LastColIndex; colIndex++)
                        {
                            Cell cell = row.GetCell(colIndex);


                            if (colIndex == 0)
                            {
                                if (cell.StringValue.ToString().Trim().ToUpper() != "STT".ToUpper())
                                {
                                    Mes = "Cột '[1]-STT' không đúng.";
                                    return false;
                                }
                            }
                            //else if (colIndex == 1)
                            //{
                            //    if (cell.StringValue.ToString().Trim().ToUpper() != "Mã đơn hàng".ToUpper())
                            //    {
                            //        Mes = "Cột '[2]-Mã đơn hàng' không đúng.";
                            //        return false;
                            //    }
                            //}
                            else if (colIndex == 1)
                            {
                                if (cell.StringValue.ToString().Trim().ToUpper() != "Loại dịch vụ".ToUpper())
                                {
                                    Mes = "Cột '[2]-Loại dịch vụ' không đúng.";
                                    return false;
                                }
                            }
                            else if (colIndex == 2)
                            {
                                if (cell.StringValue.ToString().Trim().ToUpper() != "Nội dung hàng hoá".ToUpper())
                                {
                                    Mes = "Cột '[3]-Nội dung hàng hoá' không đúng.";
                                    return false;
                                }
                            }
                            else if (colIndex == 3)
                            {
                                if (cell.StringValue.ToString().Trim().ToUpper() != "Khối lượng(Gram)".ToUpper())
                                {
                                    Mes = "Cột '[4]-Khối lượng(Gram)' không đúng.";
                                    return false;
                                }
                            }
                            else if(colIndex == 4)
                            {
                                if(cell.StringValue.ToString().Trim().ToUpper() != "Kích thước (Dài x rộng x cao) (cm)".ToUpper())
                                {
                                    Mes = "Cột '[5]-Kích thước (Dài x rộng x cao) (cm)' không đúng.";
                                    return false;
                                }
                            }
                            else if (colIndex == 5)
                            {
                                if (cell.StringValue.ToString().Trim().ToUpper() != "Tổng giá trị tiền hàng(VNĐ)".ToUpper())
                                {
                                    Mes = "Cột '[6]-Tổng giá trị tiền hàng(VNĐ)' không đúng.";
                                    return false;
                                }
                            }
                            else if (colIndex == 6)
                            {
                                if (cell.StringValue.ToString().Trim().ToUpper() != "Họ và Tên Người nhận".ToUpper())
                                {
                                    Mes = "Cột '[7]-Người nhận' không đúng.";
                                    return false;
                                }
                            }
                            else if (colIndex == 7)
                            {
                                if (cell.StringValue.ToString().Trim().ToUpper() != "Điện thoại Người nhận".ToUpper())
                                {
                                    Mes = "Cột '[8]-Điện thoại Người nhận' không đúng.";
                                    return false;
                                }
                            }
                            else if (colIndex == 8)
                            {
                                if (cell.StringValue.ToString().Trim().ToUpper() != "Địa chỉ Người nhận".ToUpper())
                                {
                                    Mes = "Cột '[9]-Địa chỉ Người nhận' không đúng.";
                                    return false;
                                }
                            }
                            else if (colIndex == 9)
                            {
                                if (cell.StringValue.ToString().Trim().ToUpper() != "Tỉnh thành".ToUpper())
                                {
                                    Mes = "Cột '[10]-Tỉnh thành' không đúng.";
                                    return false;
                                }
                            }
                            else if (colIndex == 10)
                            {
                                if (cell.StringValue.ToString().Trim().ToUpper() != "Tổng tiền thu hộ(VNĐ)".ToUpper())
                                {
                                    Mes = "Cột '[11]-Tổng tiền thu hộ(VNĐ)' không đúng.";
                                    return false;
                                }
                            }
                            else if (colIndex == 11)
                            {
                                if (cell.StringValue.ToString().Trim().ToUpper() != "Ghi chú".ToUpper())
                                {
                                    Mes = "Cột '[12]-Ghi chú' không đúng.";
                                    return false;
                                }
                            }
                        }

                        bResult = true;
                    }
                }
            }
            catch
            {
                Mes = "Có lỗi trong quá trình kiểm tra cấu trúc Bảng.";
                bResult = false;
            }

            return bResult;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult Uploads()
        {
            JsonResult result = new JsonResult();
            try
            {
                if (Session["profile"] != null)
                {
                    Account oProfile = (Account)Session["profile"];

                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        HttpPostedFileBase file = Request.Files[i];

                        if (file != null)
                        {
                            string extension = string.Empty;
                            string filename = string.Empty;
                            string shortName = string.Empty;

                            extension = System.IO.Path.GetExtension(file.FileName);
                            if (!Directory.Exists(Server.MapPath("~/UploadFiles")))
                                CreateDirectory(Server.MapPath("~/UploadFiles"));

                            filename = string.Format("{0}/{1}", Server.MapPath("~/UploadFiles"), file.FileName);

                            if (System.IO.File.Exists(filename))
                                System.IO.File.Delete(filename);

                            shortName = file.FileName.ToString();
                            file.SaveAs(filename);

                            if (extension == ".xls" || extension == "xls" || extension == "xlsx" || extension == ".xlsx")
                            {
                                //CASHPOST_0100_20160421_Lading_001
                                string fileNameNoExtension = System.IO.Path.GetFileNameWithoutExtension(file.FileName);
                                string[] arrFormatFileName = fileNameNoExtension.Split('_');
                                if (arrFormatFileName.Count() == 5) //Check định dạng File
                                {
                                    if (arrFormatFileName[0].ToUpper() == "CASHPOST".ToUpper() &&
                                        arrFormatFileName[1].ToUpper() == oProfile.UnitCode.ToUpper() &&
                                        arrFormatFileName[2].ToUpper() == DateTime.Now.ToString("yyyyMMdd").ToUpper() &&
                                        arrFormatFileName[3].ToUpper() == "Lading".ToUpper() &&
                                        !arrFormatFileName[4].ToUpper().Contains("."))
                                    {
                                        string Mes = string.Empty;
                                        if (CheckStructureFile(Path.GetDirectoryName(filename + "\\" + shortName), extension, file, ref Mes))
                                        {
                                            int iTotalError = 0;
                                            string rMes = string.Empty;

                                            var lstItem = InitLading(Path.GetDirectoryName(filename + "\\" + shortName), extension, file, ref iTotalError, ref rMes);

                                            if (lstItem != null && lstItem.Count > 0)
                                            {
                                                result = Json(new { Mes = (iTotalError > 0 ? "01|Đọc thành công: " + lstItem.Count().ToString() + " dòng; Lỗi: " + iTotalError + " dòng (" + rMes + ")" : "00|Đọc dữ liệu thành công (" + lstItem.Count().ToString() + " dòng).") + "|" + fileNameNoExtension, Value = lstItem }, JsonRequestBehavior.AllowGet);
                                            }
                                        }
                                        else
                                        {
                                            if (string.IsNullOrEmpty(Mes))
                                            {
                                                result = Json(new { Mes = "05|Cấu trúc File Import không đúng, vui lòng Download file mẫu bên dưới." }, JsonRequestBehavior.AllowGet);
                                            }
                                            else
                                            {
                                                result = Json(new { Mes = "05|" + Mes }, JsonRequestBehavior.AllowGet);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        result = Json(new { Mes = "05|Tên File không đúng định dạng (CASHPOST_MãĐơnVị_yyyyMMdd_Lading_***)." }, JsonRequestBehavior.AllowGet);
                                    }

                                }
                                else
                                {
                                    result = Json(new { Mes = "05|Tên File không đúng định dạng (CASHPOST_MãĐơnVị_yyyyMMdd_Lading_***)." }, JsonRequestBehavior.AllowGet);
                                }


                            }
                            else
                            {
                                result = Json(new { Mes = "04|File không đúng định dạng." }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
                else
                {
                    result = Json(new { Mes = "06|Hãy đăng nhập lại trước khi tiếp tục thao tác." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {
                result = Json(new { Mes = "03|Có lỗi trong quá trình xử lý File." }, JsonRequestBehavior.AllowGet);
            }

            return result;
        }

        public List<Models.Lading> InitLading(string fullpath, string extention, HttpPostedFileBase file, ref int TotalError, ref string Mes)
        {
            string result = string.Empty;
            List<Models.Lading> lstLading = new List<Models.Lading>();
            Mes = string.Empty;
            try
            {
                Workbook book = Workbook.Load(fullpath);
                Worksheet sheet = book.Worksheets[0];

                for (int rowIndex = sheet.Cells.FirstRowIndex + 1;
                      rowIndex <= sheet.Cells.LastRowIndex; rowIndex++)
                {
                    Row row = sheet.Cells.GetRow(rowIndex);

                    int iCol=0;
                    try
                    {
                        if (row.LastColIndex > 0)
                        {

                            Models.Lading oLading = new Models.Lading();
                            for (int colIndex = row.FirstColIndex;
                               colIndex <= row.LastColIndex; colIndex++)
                            {
                                Cell cell = row.GetCell(colIndex);
                                iCol = colIndex;
                                if (rowIndex >= 7)
                                {
                                    switch (colIndex)
                                    {
                                        case 0:
                                            oLading._id = cell.StringValue.ToString().Replace(",", "").Replace(".", "").Trim();
                                            break;
                                        case 1:
                                            oLading.Type = GetServiceType(cell.StringValue).ToString();
                                            break;
                                        case 2:
                                            oLading.ProductName = cell.StringValue.ToString();
                                            break;
                                        case 3:
                                            oLading.Weight = long.Parse("0" + cell.StringValue.ToString().Replace(",", "").Replace(".", "").Trim());
                                            break;
                                        case 4:
                                            oLading.Height = cell.StringValue.ToString();
                                            break;
                                        case 5:
                                            oLading.Value = long.Parse("0" + cell.StringValue.ToString().Replace(",", "").Replace(".", "").Trim()); ;
                                            break;
                                        case 6:
                                            oLading.ReceiverName = cell.StringValue.ToString();
                                            break;
                                        case 7:
                                            oLading.ReceiverMobile = cell.StringValue.ToString();
                                            break;
                                        case 8:
                                            oLading.ReceiverAddress = cell.StringValue.ToString();
                                            break;
                                        case 9:
                                            oLading.ToProvinceCode = cell.StringValue.ToString().Split('-')[0].ToString();
                                            oLading.ToProvinceName = cell.StringValue.ToString().Split('-')[1].ToString();
                                            break;
                                        case 10:
                                            oLading.CollectValue += long.Parse("0" + cell.StringValue.ToString().Replace(",", "").Replace(".", "").Trim());
                                            if(oLading.CollectValue > 0)
                                            {
                                                oLading.ServiceCode = "COD";
                                            }
                                            break;
                                        case 11:
                                            oLading.ProductDescription = cell.StringValue;
                                            break;
                                    }
                                }
                            }


                            if (oLading != null && !string.IsNullOrEmpty(oLading._id))
                            {
                                lstLading.Add(oLading);
                                oLading = null;
                            }
                        }
                    }
                    catch {
                        TotalError++;
                        Mes += "[Dòng:" + (rowIndex - 6) + ";Cột:" + (iCol + 1) + "];";
                    }
                }
            }
            catch
            {
                lstLading = null;
            }

            return lstLading;
        }

        public int GetServiceType(string Input)
        {
            int iServiceType = 1;
            try
            {
                if (Input.Contains("0-Chuyển phát nhanh"))
                {
                    iServiceType = 0;
                }
                else if (Input.Contains("1-Chuyển phát hỏa tốc"))
                {
                    iServiceType = 1;
                }
                else if (Input.Contains("2-Chuyển phát đặc biệt"))
                {
                    iServiceType = 2;
                }
            }
            catch
            {
                iServiceType = 0;
            }
            return iServiceType;
        }
    }
}
public class Return
{
    public string Code { get; set; }
    public string response_code { get; set; }
    public string response_message { get; set; }
}