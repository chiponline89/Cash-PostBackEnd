using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Security;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;

using MongoDB.Driver.Builders;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using MongoDB.Driver;
using System.Dynamic;
using PayID.DataHelper;
using Microsoft.Reporting.WebForms;
using System.IO;
using System.Configuration;
using PayID.Portal.Areas.Report.Models;


namespace PayID.Portal.Areas.Report.Controllers
{
    public class HomeController : Controller
    {
        private static string _status = string.Empty;
        private static string _busscode = string.Empty;
        private static string _fromdate = string.Empty;
        private static string _todate = string.Empty;
        private static string _srvCode = string.Empty;
        private static string _province = string.Empty;
        private static string _district = string.Empty;
        private static string _pos = string.Empty;
        private static string _current_assigned = string.Empty;

        dynamic[] lstStatus;
        //
        // GET: /Report/Home/
        [Authorize]
        public ActionResult Index()
        {
            if (Session["profile"] == null)
            {
                dynamic profile = Configuration.Data.Get("user", Query.EQ("_id", User.Identity.Name));
                Session["profile"] = profile;
            }
            ViewBag.Local = ((dynamic)Session["profile"]).unit_name;
            string _title = "";
            try
            {
                _title = Request.QueryString["opt"].ToString();
            }
            catch
            {

            }
            if (!string.IsNullOrEmpty(_title))
            {
                if (_title == "vd" || _title == "vd#")
                {
                    ViewBag.Title = "Báo cáo chi tiết vận đơn";
                }
                else if (_title == "tkvdtt" || _title == "tkvdtt#")
                {
                    ViewBag.Title = "Thống kê vận đơn theo tên khách hàng";
                }
                else if (_title == "tkvdtg" || _title == "tkvdtg#")
                {
                    ViewBag.Title = "Thống kê vận đơn theo các đơn hàng điều tin thu gom";
                }
                else if (_title == "tkvdph" || _title == "tkvdph#")
                {
                    ViewBag.Title = "Thống kê vận đơn theo các đơn hàng đã được phát hành";
                }
                else if (_title == "thvd" || _title == "thvd#")
                {
                    ViewBag.Title = "Báo cáo tổng hợp vận đơn";
                }
                else if (_title == "thu" || _title == "thu#")
                {
                    ViewBag.Title = "Báo cáo chi tiết giao dịch thanh toán";
                }
                else if (_title == "thuth" || _title == "thuth#")
                {
                    ViewBag.Title = "Báo cáo tổng hợp giao dịch thanh toán";
                }
                else if (_title == "dh" || _title == "dh#")
                {
                    ViewBag.Title = "Báo cáo chi tiết quản lý đơn hàng";
                }
                else if (_title == "dhth" || _title == "dhth#")
                {
                    ViewBag.Title = "Báo cáo tổng hợp quản lý đơn hàng";
                }
            }

            return View();
        }

        [HttpPost]

        public ActionResult ListFormName(string status, string todt, string fromdt, string province, string district, string pos, string current_assigned, string busscode, int? page)
        {
            //dynamic profile = ((dynamic)Session["profile"]);

            current_assigned = ((dynamic)Session["profile"]).unit_link;
            _status = status;
            _busscode = busscode;
            _province = province;
            _district = district;
            _pos = pos;
            _current_assigned = current_assigned;
            if (!string.IsNullOrEmpty(todt))
            {
                todt = todt.Substring(3, 2) + "/" + todt.Substring(0, 2) + "/" + todt.Substring(6, 4);
                _todate = todt;
            }
            if (!string.IsNullOrEmpty(fromdt))
            {
                fromdt = fromdt.Substring(3, 2) + "/" + fromdt.Substring(0, 2) + "/" + fromdt.Substring(6, 4);
                _fromdate = fromdt;
            }
            int p = (page == null) ? 1 : (int)page;
            int pg_size = 10;
            try
            {
                pg_size = int.Parse(ConfigurationManager.AppSettings["PAGE_SIZE"].ToString());
            }
            catch
            {
                pg_size = 10;
            }
            string sFullName = string.Empty;
            dynamic dyna = new DynamicObj();

            if (Session["profile"] != null)
            {
                dyna = (dynamic)Session["profile"];
            }

            if (dyna.full_name != null)
            {
                sFullName = dyna.full_name;
            }

            ViewBag.FullName = sFullName;


            string sCustomerName = string.Empty;
            string sCustomerCode = string.Empty;
            if (!string.IsNullOrEmpty(busscode))
            {
                dynamic dynaBusi = new DynamicObj();
                dynaBusi = PayID.Portal.Configuration.Data_S24.Get("business_profile", Query.Or(
                    Query.EQ("_id", busscode),
                    Query.EQ("general_full_name", busscode)
                    )
                );
                if (dynaBusi != null)
                {
                    sCustomerName = dynaBusi.general_full_name;
                    sCustomerCode = dynaBusi._id;
                }
                ViewBag.CustomerName = "-" + sCustomerName;
                ViewBag.CustomerCode = sCustomerCode;
            }
            else
            {
                ViewBag.CustomerName = " ";
                ViewBag.CustomerCode = "Tất Cả";
            }

            List<dynamic> list_shipment = new List<dynamic>();
            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var myshipment = new
                    {

                        current_assigned = _current_assigned,
                        from_date = String.IsNullOrEmpty(fromdt) ? "" : DateTime.Parse(fromdt).ToString("dd/MM/yyyy"),
                        to_date = String.IsNullOrEmpty(todt) ? "" : DateTime.Parse(todt).ToString("dd/MM/yyyy"),
                        status = _status,
                        busscode = _busscode,
                        district = _district,
                        pos = _pos,
                        province = _province,
                        FormType = "FormName"
                    };
                    try
                    {

                        var response = client.PostAsJsonAsync("api/Shipment", myshipment).Result;
                        list_shipment = response.Content.ReadAsAsync<IEnumerable<dynamic>>().Result.ToList();
                        //list_shipment = (from c in list_shipment orderby c.CREATEDDATE descending select c).ToList();
                        ViewBag.total_page = (list_shipment.Count + pg_size - 1) / pg_size;
                        ViewBag.total_item = list_shipment.Count;
                        int currentPageIndex = page.HasValue ? page.Value : 1;
                        ViewBag.Page = (currentPageIndex - 1) * pg_size;
                    }
                    catch { }
                }
                catch
                {
                    //list_shipment = null;
                }
            }

            long sum_CodFee = 0;
            long sum_collectvalue = 0;

            foreach (dynamic l in list_shipment)
            {

                if (l.CodFee == null) l.CodFee = "0";
                if (l.CodFee == null) l.CodFee = "0";
                sum_CodFee += long.Parse(string.Format("{0}", l.CodFee));
                sum_collectvalue += long.Parse(string.Format("{0}", l.collectvalue));

            }
            ViewBag.sum_CodFee = sum_CodFee;
            ViewBag.sum_collectvalue = sum_collectvalue;
            int skip = (p - 1) * pg_size;
            list_shipment = (from c in list_shipment orderby c.system_last_updated_time descending select c).Skip(skip).Take(pg_size).ToList<dynamic>();

            return View(list_shipment);

        }

        //Thống kê thu gom
        public ActionResult ListFormCollect(string status, string todt, string fromdt, string province, string district, string pos, string current_assigned, string busscode, int? page)
        {
            current_assigned = ((dynamic)Session["profile"]).unit_link;
            _status = status;
            _busscode = busscode;
            _province = province;
            _district = district;
            _pos = pos;
            _current_assigned = current_assigned;

            if (!string.IsNullOrEmpty(todt))
            {
                todt = todt.Substring(3, 2) + "/" + todt.Substring(0, 2) + "/" + todt.Substring(6, 4);
                _todate = todt;
            }
            if (!string.IsNullOrEmpty(fromdt))
            {
                fromdt = fromdt.Substring(3, 2) + "/" + fromdt.Substring(0, 2) + "/" + fromdt.Substring(6, 4);
                _fromdate = fromdt;
            }
            string sCustomerName = string.Empty;
            string sCustomerCode = string.Empty;
            if (!string.IsNullOrEmpty(busscode))
            {
                dynamic dynaBusi = new DynamicObj();
                dynaBusi = PayID.Portal.Configuration.Data_S24.Get("business_profile", Query.Or(
                    Query.EQ("_id", busscode),
                    Query.EQ("general_full_name", busscode)
                    )
                );
                if (dynaBusi != null)
                {
                    sCustomerName = dynaBusi.general_full_name;
                    sCustomerCode = dynaBusi._id;
                }
                ViewBag.CustomerName = "-" + sCustomerName;
                ViewBag.CustomerCode = sCustomerCode;
            }
            else
            {
                ViewBag.CustomerName = " ";
                ViewBag.CustomerCode = "Tất Cả";
            }
            int p = (page == null) ? 1 : (int)page;
            int pg_size = 10;
            try
            {
                pg_size = int.Parse(ConfigurationManager.AppSettings["PAGE_SIZE"].ToString());
            }
            catch
            {
                pg_size = 10;
            }
            List<dynamic> list_shipment = new List<dynamic>();
            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var myshipment = new
                    {
                        current_assigned = ((dynamic)Session["profile"]) != null ? ((dynamic)Session["profile"]).unit_link : "",
                        from_date = String.IsNullOrEmpty(fromdt) ? "" : DateTime.Parse(fromdt).ToString("dd/MM/yyyy"),
                        to_date = String.IsNullOrEmpty(todt) ? "" : DateTime.Parse(todt).ToString("dd/MM/yyyy"),
                        status = _status,
                        busscode = _busscode,
                        province = _province,
                        district = _district,
                        pos = _pos,
                        FormType = "FormCollect"
                    };
                    try
                    {

                        var response = client.PostAsJsonAsync("api/Shipment", myshipment).Result;
                        list_shipment = response.Content.ReadAsAsync<IEnumerable<dynamic>>().Result.ToList();
                        //list_shipment = (from c in list_shipment orderby c.CREATEDDATE descending select c).ToList();
                        //long count_tracking_code = list_shipment.Count(x => x.tracking_code);//Sản lượng
                        //long count_order_id = list_shipment.Count(x => x.order_id);//Sản lượng
                        long lSumValue = list_shipment.Sum(x => x.value);
                        long lSumCollectValue = list_shipment.Sum(x => x.collectvalue);
                        long lSumquantity = list_shipment.Sum(x => x.quantity);

                        //ViewBag.count_tracking_code = count_tracking_code;
                        //ViewBag.count_order_id = count_order_id;
                        ViewBag.SumQuantity = lSumquantity;
                        ViewBag.SumValue = lSumValue;
                        ViewBag.SumCollectValue = lSumCollectValue;
                        ViewBag.total_page = (list_shipment.Count + pg_size - 1) / pg_size;
                        ViewBag.total_item = list_shipment.Count;
                        int currentPageIndex = page.HasValue ? page.Value : 1;
                        ViewBag.Page = (currentPageIndex - 1) * pg_size;
                    }
                    catch { }
                }
                catch
                {
                    //list_shipment = null;
                }
            }

            int skip = (p - 1) * pg_size;
            list_shipment = (from c in list_shipment orderby c.system_last_updated_time descending select c).Skip(skip).Take(pg_size).ToList<dynamic>();

            // ViewBag.listshipment = list_shipment;
            return View(list_shipment);

        }
        public ActionResult ListFormIssue(string status, string todt, string fromdt, string province, string district, string pos, string busscode, string current_assigned, int? page)
        {
            current_assigned = ((dynamic)Session["profile"]).unit_link;
            _status = status;
            _busscode = busscode;
            _province = province;
            _district = district;
            _pos = pos;
            _current_assigned = current_assigned;
            if (!string.IsNullOrEmpty(todt))
            {
                todt = todt.Substring(3, 2) + "/" + todt.Substring(0, 2) + "/" + todt.Substring(6, 4);
                _todate = todt;
            }
            if (!string.IsNullOrEmpty(fromdt))
            {
                fromdt = fromdt.Substring(3, 2) + "/" + fromdt.Substring(0, 2) + "/" + fromdt.Substring(6, 4);
                _fromdate = fromdt;
            }

            int p = (page == null) ? 1 : (int)page;
            int pg_size = 10;
            try
            {
                pg_size = int.Parse(ConfigurationManager.AppSettings["PAGE_SIZE"].ToString());
            }
            catch
            {
                pg_size = 10;
            }
            string sCustomerName = string.Empty;
            string sCustomerCode = string.Empty;
            if (!string.IsNullOrEmpty(busscode))
            {
                dynamic dynaBusi = new DynamicObj();
                dynaBusi = PayID.Portal.Configuration.Data_S24.Get("business_profile", Query.Or(
                    Query.EQ("_id", busscode),
                    Query.EQ("general_full_name", busscode)
                    )
                );
                if (dynaBusi != null)
                {
                    sCustomerName = dynaBusi.general_full_name;
                    sCustomerCode = dynaBusi._id;
                }
                ViewBag.CustomerName = "-" + sCustomerName;
                ViewBag.CustomerCode = sCustomerCode;
            }
            else
            {
                ViewBag.CustomerName = " ";
                ViewBag.CustomerCode = "Tất Cả";
            }
            List<dynamic> list_shipment = new List<dynamic>();
            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var myshipment = new
                    {
                        current_assigned = ((dynamic)Session["profile"]) != null ? ((dynamic)Session["profile"]).unit_link : "",
                        from_date = String.IsNullOrEmpty(fromdt) ? "" : DateTime.Parse(fromdt).ToString("dd/MM/yyyy"),
                        to_date = String.IsNullOrEmpty(todt) ? "" : DateTime.Parse(todt).ToString("dd/MM/yyyy"),
                        status = _status,
                        busscode = _busscode,
                        district = _district,
                        pos = _pos,
                        province = province,
                        FormType = "FormIssue"
                    };
                    try
                    {
                        var response = client.PostAsJsonAsync("api/Shipment", myshipment).Result;
                        list_shipment = response.Content.ReadAsAsync<IEnumerable<dynamic>>().Result.ToList();
                        //list_shipment = (from c in list_shipment orderby c.CREATEDDATE descending select c).ToList();
                        long lSumValue = list_shipment.Sum(x => x.value);
                        long lSumCollectValue = list_shipment.Sum(x => x.collectvalue);
                        long lSumServiceFee = list_shipment.Sum(x => x.ServiceFee);
                        long lSumCodFee = list_shipment.Sum(x => x.CodFee);
                        long lSumFeeVC = list_shipment.Sum(x => x.MainFee);
                        long lSumTotalFee = list_shipment.Sum(x => x.TotalFee);
                        long lSumWeight = list_shipment.Sum(x => x.weight);
                        ViewBag.SumValue = lSumValue;
                        ViewBag.SumCollectValue = lSumCollectValue;
                        ViewBag.SumFeeVC = lSumFeeVC;
                        ViewBag.SumAddFee = lSumServiceFee;
                        ViewBag.SumCodFee = lSumCodFee;
                        ViewBag.SumFee = lSumTotalFee;
                        ViewBag.SumWeight = lSumWeight;
                        ViewBag.total_page = (list_shipment.Count + pg_size - 1) / pg_size;
                        ViewBag.total_item = list_shipment.Count;
                        int currentPageIndex = page.HasValue ? page.Value : 1;
                        ViewBag.Page = (currentPageIndex - 1) * pg_size;
                    }
                    catch { }
                }
                catch
                {
                    //list_shipment = null;
                }
            }

            int skip = (p - 1) * pg_size;
            list_shipment = (from c in list_shipment orderby c.system_last_updated_time descending select c).Skip(skip).Take(pg_size).ToList<dynamic>();
            // ViewBag.listshipment = list_shipment;
            return View(list_shipment);

        }
        public ActionResult ListFormNameTH(string status, string todt, string fromdt, string province, string district, string pos, string current_assigned, string busscode, int? page)
        {
            current_assigned = ((dynamic)Session["profile"]).unit_link;
            _status = status;
            _busscode = busscode;
            _province = province;
            _district = district;
            _pos = pos;
            _current_assigned = current_assigned;

            if (!string.IsNullOrEmpty(todt))
            {
                todt = todt.Substring(3, 2) + "/" + todt.Substring(0, 2) + "/" + todt.Substring(6, 4);
                _todate = todt;
            }
            if (!string.IsNullOrEmpty(fromdt))
            {
                fromdt = fromdt.Substring(3, 2) + "/" + fromdt.Substring(0, 2) + "/" + fromdt.Substring(6, 4);
                _fromdate = fromdt;
            }

            int p = (page == null) ? 1 : (int)page;
            int pg_size = 10;
            string sCustomerName = string.Empty;
            string sCustomerCode = string.Empty;
            if (!string.IsNullOrEmpty(busscode))
            {
                dynamic dynaBusi = new DynamicObj();
                dynaBusi = PayID.Portal.Configuration.Data_S24.Get("business_profile", Query.Or(
                    Query.EQ("_id", busscode),
                    Query.EQ("general_full_name", busscode)
                    )
                );
                if (dynaBusi != null)
                {
                    sCustomerName = dynaBusi.general_full_name;
                    sCustomerCode = dynaBusi._id;
                }
                ViewBag.CustomerName = "-" + sCustomerName;
                ViewBag.CustomerCode = sCustomerCode;
            }
            else
            {
                ViewBag.CustomerName = " ";
                ViewBag.CustomerCode = "Tất Cả";
            }

            try
            {
                pg_size = int.Parse(ConfigurationManager.AppSettings["PAGE_SIZE"].ToString());
            }
            catch
            {
                pg_size = 10;
            }
            List<dynamic> list_shipment = new List<dynamic>();

            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var myshipment = new
                    {
                        current_assigned = _current_assigned,
                        from_date = String.IsNullOrEmpty(fromdt) ? "" : DateTime.Parse(fromdt).ToString("dd/MM/yyyy"),
                        to_date = String.IsNullOrEmpty(todt) ? "" : DateTime.Parse(todt).ToString("dd/MM/yyyy"),
                        status = _status,
                        busscode = _busscode,
                        district = _district,
                        pos = _pos,
                        province = _province,
                        FormType = "FormName"
                    };
                    try
                    {
                        var response = client.PostAsJsonAsync("api/Shipment", myshipment).Result;
                        list_shipment = response.Content.ReadAsAsync<IEnumerable<dynamic>>().Result.ToList();
                        //list_shipment = (from c in list_shipment orderby c.customercode descending select c).ToList<dynamic>();
                        ViewBag.total_page = (list_shipment.Count + pg_size - 1) / pg_size;
                        ViewBag.total_item = list_shipment.Count;
                        int currentPageIndex = page.HasValue ? page.Value : 1;
                        ViewBag.Page = (currentPageIndex - 1) * pg_size;

                        list_shipment = (from m in list_shipment
                                         group m by new { m.customercode, m.customername, m.system_status } into ngrp
                                         select new
                                         {
                                             customercode = ngrp.Key.customercode.ToString(),
                                             customername = ngrp.Key.customername.ToString(),
                                             system_status = ngrp.Key.system_status.ToString(),
                                             landing_count = ngrp.Count(),
                                             collectvalue = ngrp.Sum(x => x.collectvalue),
                                             MainFee = ngrp.Sum(x => x.MainFee),
                                             value = ngrp.Sum(x => x.value)
                                         }).ToList<dynamic>();

                        int skip = (p - 1) * pg_size;
                        if (list_shipment != null && list_shipment.Count > 0)
                            list_shipment = (from c in list_shipment orderby c.customername descending select c).Skip(skip).Take(pg_size).ToList<dynamic>();

                        List<dynamic> LST = new List<dynamic>();

                        dynamic ite = new DynamicObj();
                        for (int i = 0; i < list_shipment.Count; i++)
                        {
                            ite = new DynamicObj();
                            ite.customercode = list_shipment[i].customercode.ToString();
                            ite.customername = list_shipment[i].customername.ToString();
                            ite.system_status = list_shipment[i].system_status.ToString();
                            ite.landing_count = list_shipment[i].landing_count.ToString();
                            ite.collectvalue = list_shipment[i].collectvalue.ToString("N0");
                            ite.value = list_shipment[i].value.ToString("N0");

                            LST.Add(ite);

                        }
                        list_shipment = LST;


                    }
                    catch { }
                }
                catch
                {
                    list_shipment = null;
                }
            }
            return View(list_shipment);

        }
        public ActionResult ListFormCollectTH(string status, string todt, string fromdt, string province, string district, string pos, string current_assigned, string busscode, int? page)
        {
            current_assigned = ((dynamic)Session["profile"]).unit_link;
            _status = status;
            _busscode = busscode;
            _province = province;
            _district = district;
            _pos = pos;
            _current_assigned = current_assigned;
            if (!string.IsNullOrEmpty(todt))
            {
                todt = todt.Substring(3, 2) + "/" + todt.Substring(0, 2) + "/" + todt.Substring(6, 4);
                _todate = todt;
            }
            if (!string.IsNullOrEmpty(fromdt))
            {
                fromdt = fromdt.Substring(3, 2) + "/" + fromdt.Substring(0, 2) + "/" + fromdt.Substring(6, 4);
                _fromdate = fromdt;
            }

            int p = (page == null) ? 1 : (int)page;
            int pg_size = 10;
            try
            {
                pg_size = int.Parse(ConfigurationManager.AppSettings["PAGE_SIZE"].ToString());
            }
            catch
            {
                pg_size = 10;
            }
            List<dynamic> list_shipment = new List<dynamic>();

            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var myshipment = new
                    {
                        current_assigned = ((dynamic)Session["profile"]) != null ? ((dynamic)Session["profile"]).unit_link : "",
                        from_date = String.IsNullOrEmpty(fromdt) ? "" : DateTime.Parse(fromdt).ToString("dd/MM/yyyy"),
                        to_date = String.IsNullOrEmpty(todt) ? "" : DateTime.Parse(todt).ToString("dd/MM/yyyy"),
                        status = status,
                        busscode = busscode,
                        district = district,
                        pos = pos,
                        province = province,
                        FormType = "FormCollect"
                    };
                    try
                    {
                        var response = client.PostAsJsonAsync("api/Shipment", myshipment).Result;
                        list_shipment = response.Content.ReadAsAsync<IEnumerable<dynamic>>().Result.ToList();
                        list_shipment = (from c in list_shipment orderby c.AssignProvince descending select c).ToList<dynamic>();

                        ViewBag.total_page = (list_shipment.Count + pg_size - 1) / pg_size;
                        ViewBag.total_item = list_shipment.Count;
                        int currentPageIndex = page.HasValue ? page.Value : 1;
                        ViewBag.Page = (currentPageIndex - 1) * pg_size;

                        int skip = (p - 1) * pg_size;
                        if (list_shipment != null && list_shipment.Count > 0)
                            list_shipment = (from c in list_shipment orderby c.AssignProvinceName descending select c).Skip(skip).Take(pg_size).ToList<dynamic>();

                        if (!string.IsNullOrEmpty(province))
                        {
                            if (!string.IsNullOrEmpty(district))
                            {
                                list_shipment = (from c in list_shipment
                                                 where c.AssignPoName != "" && c.AssignDistrictName != "" && c.AssignDistrict != "" && c.AssignProvince == province && c.AssignDistrict == district
                                                 group c by new { district = c.AssignPoName } into grp
                                                 select new
                                                 {
                                                     province = grp.Key.district.ToString(),
                                                     detail = (from m in grp
                                                               group m by new { status = m.system_status } into ngrp
                                                               select new
                                                               {
                                                                   status = ngrp.Key.status.ToString(),
                                                                   quantity = ngrp.Count(),
                                                                   collectvalue = ngrp.Sum(x => x.collectvalue),
                                                                   MainFee = ngrp.Sum(x => x.MainFee),
                                                                   ServiceFee = ngrp.Sum(x => x.ServiceFee),
                                                                   CodFee = ngrp.Sum(x => x.CodFee),
                                                                   //TotalFee = ngrp.Sum(x => x.TotalFee),
                                                                   weight = ngrp.Sum(x => x.weight),
                                                               }).ToList<dynamic>()


                                                 }).ToList<dynamic>();
                            }
                            else
                            {
                                list_shipment = (from c in list_shipment
                                                 where c.AssignDistrictName != "" && c.AssignDistrict != "" && c.AssignProvince == province
                                                 group c by new { district = c.AssignDistrictName } into grp
                                                 select new
                                                 {
                                                     province = grp.Key.district.ToString(),
                                                     detail = (from m in grp
                                                               group m by new { status = m.system_status } into ngrp
                                                               select new
                                                               {
                                                                   status = ngrp.Key.status.ToString(),
                                                                   quantity = ngrp.Count(),
                                                                   collectvalue = ngrp.Sum(x => x.collectvalue),
                                                                   MainFee = ngrp.Sum(x => x.MainFee),
                                                                   ServiceFee = ngrp.Sum(x => x.ServiceFee),
                                                                   CodFee = ngrp.Sum(x => x.CodFee),
                                                                   //TotalFee = ngrp.Sum(x => x.TotalFee),
                                                                   weight = ngrp.Sum(x => x.weight),
                                                               }).ToList<dynamic>(),


                                                 }).ToList<dynamic>();
                            }
                        }
                        else
                        {
                            list_shipment = (from c in list_shipment
                                             where c.AssignProvince != ""
                                             group c by new { province = c.AssignProvinceName } into grp
                                             select new
                                             {
                                                 province = grp.Key.province.ToString(),
                                                 detail = (from m in grp
                                                           group m by new { status = m.system_status } into ngrp
                                                           select new
                                                           {
                                                               status = ngrp.Key.status.ToString(),
                                                               quantity = ngrp.Count(),
                                                               collectvalue = ngrp.Sum(x => x.collectvalue),
                                                               MainFee = ngrp.Sum(x => x.MainFee),
                                                               ServiceFee = ngrp.Sum(x => x.ServiceFee),
                                                               CodFee = ngrp.Sum(x => x.CodFee),
                                                               //TotalFee = ngrp.Sum(x => x.TotalFee),
                                                               weight = ngrp.Sum(x => x.weight),
                                                           }).ToList<dynamic>()
                                             }).ToList<dynamic>();
                        }

                    }
                    catch { }
                }
                catch
                {
                    list_shipment = null;
                }
            }
            List<dynamic> LST = new List<dynamic>();
            List<dynamic> LSTDetail = new List<dynamic>();
            dynamic ite = new DynamicObj();
            dynamic iteDetail = new DynamicObj();
            if (list_shipment != null && list_shipment.Count > 0)
            {
                for (int i = 0; i < list_shipment.Count; i++)
                {
                    LSTDetail = new List<dynamic>();
                    ite = new DynamicObj();
                    ite.province = list_shipment[i].province.ToString();
                    foreach (var item in list_shipment[i].detail)
                    {
                        iteDetail = new DynamicObj();
                        iteDetail.status = item.status.ToString();
                        iteDetail.quantity = long.Parse(item.quantity.ToString());
                        iteDetail.collectvalue = long.Parse(item.collectvalue.ToString());
                        iteDetail.MainFee = long.Parse(item.MainFee.ToString());
                        iteDetail.ServiceFee = long.Parse(item.ServiceFee.ToString());
                        iteDetail.CodFee = long.Parse(item.CodFee.ToString());
                        iteDetail.TotalFee = long.Parse(item.CodFee.ToString()) + long.Parse(item.ServiceFee.ToString()) + long.Parse(item.MainFee.ToString());
                        iteDetail.weight = long.Parse(item.weight.ToString());

                        LSTDetail.Add(iteDetail);
                    }

                    LST.Add(ite);
                    LST.Add(LSTDetail);
                }

            }
            else
            {
                LST = null;
            }

            return View(LST);

        }
        public ActionResult ListFormIssueTH(string status, string todt, string fromdt, string province, string district, string pos, string current_assigned, string busscode, int? page)
        {
            current_assigned = ((dynamic)Session["profile"]).unit_link;
            _status = status;
            _busscode = busscode;
            _province = province;
            _district = district;
            _pos = pos;
            _current_assigned = current_assigned;
            if (!string.IsNullOrEmpty(todt))
            {
                todt = todt.Substring(3, 2) + "/" + todt.Substring(0, 2) + "/" + todt.Substring(6, 4);
                _todate = todt;
            }
            if (!string.IsNullOrEmpty(fromdt))
            {
                fromdt = fromdt.Substring(3, 2) + "/" + fromdt.Substring(0, 2) + "/" + fromdt.Substring(6, 4);
                _fromdate = fromdt;
            }

            int p = (page == null) ? 1 : (int)page;
            int pg_size = 10;
            try
            {
                pg_size = int.Parse(ConfigurationManager.AppSettings["PAGE_SIZE"].ToString());
            }
            catch
            {
                pg_size = 10;
            }
            List<dynamic> list_shipment = new List<dynamic>();

            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var myshipment = new
                    {
                        current_assigned = ((dynamic)Session["profile"]) != null ? ((dynamic)Session["profile"]).unit_link : "",
                        from_date = String.IsNullOrEmpty(fromdt) ? "" : DateTime.Parse(fromdt).ToString("dd/MM/yyyy"),
                        to_date = String.IsNullOrEmpty(todt) ? "" : DateTime.Parse(todt).ToString("dd/MM/yyyy"),
                        status = status,
                        busscode = busscode,
                        province = province,
                        district = district,
                        pos = pos,
                        FormType = "FormIssue"
                    };
                    try
                    {
                        var response = client.PostAsJsonAsync("api/Shipment", myshipment).Result;
                        list_shipment = response.Content.ReadAsAsync<IEnumerable<dynamic>>().Result.ToList();
                        list_shipment = (from c in list_shipment orderby c.AssignProvince descending select c).ToList<dynamic>();

                        ViewBag.total_page = (list_shipment.Count + pg_size - 1) / pg_size;
                        ViewBag.total_item = list_shipment.Count;
                        int currentPageIndex = page.HasValue ? page.Value : 1;
                        ViewBag.Page = (currentPageIndex - 1) * pg_size;

                        int skip = (p - 1) * pg_size;
                        if (list_shipment != null && list_shipment.Count > 0)
                            list_shipment = (from c in list_shipment orderby c.AssignProvinceName descending select c).Skip(skip).Take(pg_size).ToList<dynamic>();

                        if (!string.IsNullOrEmpty(province))
                        {
                            if (!string.IsNullOrEmpty(district))
                            {
                                list_shipment = (from c in list_shipment
                                                 where c.AssignDistrictName != "" && c.AssignDistrict != "" && c.AssignProvince == province && c.AssignDistrict == district
                                                 group c by new { district = c.AssignDistrictName } into grp
                                                 select new
                                                 {
                                                     province = grp.Key.district.ToString(),
                                                     detail = (from m in grp
                                                               group m by new { status = m.system_status } into ngrp
                                                               select new
                                                               {
                                                                   status = ngrp.Key.status.ToString(),
                                                                   quantity = ngrp.Count(),
                                                                   collectvalue = ngrp.Sum(x => x.collectvalue),
                                                                   MainFee = ngrp.Sum(x => x.MainFee),
                                                                   ServiceFee = ngrp.Sum(x => x.ServiceFee),
                                                                   CodFee = ngrp.Sum(x => x.CodFee),
                                                                   //TotalFee = ngrp.Sum(x => x.TotalFee),
                                                                   weight = ngrp.Sum(x => x.weight),
                                                               }).ToList<dynamic>()

                                                 }).ToList<dynamic>();
                            }
                            else
                            {
                                list_shipment = (from c in list_shipment
                                                 where c.AssignDistrictName != "" && c.AssignDistrict != "" && c.AssignProvince == province
                                                 group c by new { district = c.AssignDistrictName } into grp
                                                 select new
                                                 {
                                                     province = grp.Key.district.ToString(),
                                                     detail = (from m in grp
                                                               group m by new { status = m.system_status } into ngrp
                                                               select new
                                                               {
                                                                   status = ngrp.Key.status.ToString(),
                                                                   quantity = ngrp.Count(),
                                                                   collectvalue = ngrp.Sum(x => x.collectvalue),
                                                                   MainFee = ngrp.Sum(x => x.MainFee),
                                                                   ServiceFee = ngrp.Sum(x => x.ServiceFee),
                                                                   CodFee = ngrp.Sum(x => x.CodFee),
                                                                   //TotalFee = ngrp.Sum(x => x.TotalFee),
                                                                   weight = ngrp.Sum(x => x.weight),
                                                               }).ToList<dynamic>()

                                                 }).ToList<dynamic>();
                            }
                        }
                        else
                        {
                            list_shipment = (from c in list_shipment
                                             where c.AssignProvince != ""
                                             group c by new { province = c.AssignProvinceName } into grp
                                             select new
                                             {
                                                 province = grp.Key.province.ToString(),
                                                 detail = (from m in grp
                                                           group m by new { status = m.system_status } into ngrp
                                                           select new
                                                           {
                                                               status = ngrp.Key.status.ToString(),
                                                               quantity = ngrp.Count(),
                                                               collectvalue = ngrp.Sum(x => x.collectvalue),
                                                               MainFee = ngrp.Sum(x => x.MainFee),
                                                               ServiceFee = ngrp.Sum(x => x.ServiceFee),
                                                               CodFee = ngrp.Sum(x => x.CodFee),
                                                               //TotalFee = ngrp.Sum(x => x.TotalFee),
                                                               weight = ngrp.Sum(x => x.weight),
                                                           }).ToList<dynamic>()
                                             }).ToList<dynamic>();
                        }

                    }
                    catch { }
                }
                catch
                {
                    list_shipment = null;
                }
            }
            //long sum_value = 0;
            //long sum_CodFee = 0;
            //long sum_MainFee = 0;
            //long sum_ServiceFee = 0;
            //long sum_TotalFee = 0;
            List<dynamic> LST = new List<dynamic>();
            List<dynamic> LSTDetail = new List<dynamic>();
            dynamic ite = new DynamicObj();
            dynamic iteDetail = new DynamicObj();
            if (list_shipment != null && list_shipment.Count > 0)
            {
                for (int i = 0; i < list_shipment.Count; i++)
                {
                    ite = new DynamicObj();
                    //Check exist
                    LSTDetail = new List<dynamic>();
                    ite.province = list_shipment[i].province.ToString();
                    foreach (var item in list_shipment[i].detail)
                    {
                        iteDetail = new DynamicObj();
                        iteDetail.status = item.status.ToString();
                        iteDetail.quantity = long.Parse(item.quantity.ToString());
                        iteDetail.collectvalue = long.Parse(item.collectvalue.ToString());
                        iteDetail.MainFee = long.Parse(item.MainFee.ToString());
                        iteDetail.ServiceFee = long.Parse(item.ServiceFee.ToString());
                        iteDetail.CodFee = long.Parse(item.CodFee.ToString());
                        iteDetail.TotalFee = long.Parse(item.CodFee.ToString()) + long.Parse(item.ServiceFee.ToString()) + long.Parse(item.MainFee.ToString());
                        iteDetail.weight = long.Parse(item.weight.ToString());

                        LSTDetail.Add(iteDetail);
                    }
                    LST.Add(ite);
                    LST.Add(LSTDetail);
                }

            }
            else
            {
                LST = null;
            }



            return View(LST);

        }
        public ActionResult ListRptDetail(string status, string fromdt, string todt, string busscode, string srvCode, int? page)
        {
            string _whereclause = "";
            _status = status;
            _busscode = busscode;
            _srvCode = srvCode;
            if (!string.IsNullOrEmpty(todt))
            {
                todt = todt.Substring(3, 2) + "/" + todt.Substring(0, 2) + "/" + todt.Substring(6, 4);
                _todate = todt;
            }
            if (!string.IsNullOrEmpty(fromdt))
            {
                fromdt = fromdt.Substring(3, 2) + "/" + fromdt.Substring(0, 2) + "/" + fromdt.Substring(6, 4);
                _fromdate = fromdt;
            }
            try
            {
                if (!string.IsNullOrEmpty(status) && status != "0")
                {
                    _whereclause += " AND STATUS='" + status + "' ";
                }

                if (!string.IsNullOrEmpty(busscode))
                {
                    _whereclause += " AND CUSTOMER_CODE='" + busscode + "' ";
                }

                if (!string.IsNullOrEmpty(fromdt) && string.IsNullOrEmpty(todt))
                {
                    _whereclause += " AND trunc(CREATEDDATE)=to_date('" + DateTime.Parse(fromdt).ToString("dd/MM/yyyy") + "','dd/MM/yyyy') ";
                }

                if (!string.IsNullOrEmpty(todt) && string.IsNullOrEmpty(fromdt))
                {
                    _whereclause += " AND trunc(CREATEDDATE)=to_date('" + DateTime.Parse(todt).ToString("dd/MM/yyyy") + "','dd/MM/yyyy') ";
                }

                if (!string.IsNullOrEmpty(todt) && !string.IsNullOrEmpty(fromdt))
                {
                    _whereclause += " AND trunc(CREATEDDATE) between to_date('" + DateTime.Parse(fromdt).ToString("dd/MM/yyyy") + "','dd/MM/yyyy') and to_date('" + DateTime.Parse(todt).ToString("dd/MM/yyyy") + "','dd/MM/yyyy') ";
                }

                if (!string.IsNullOrEmpty(srvCode) && srvCode != "00")
                {
                    _whereclause += " AND TYPE = " + int.Parse(srvCode);
                }

                if (Session["profile"] != null)
                {
                    dynamic profile = (dynamic)Session["profile"];

                    _whereclause += " AND (SENDER_PO_CODE=" + profile.unit_code + " or SENDER_PROVINCE_CODE=" + profile.unit_code + ")";
                }
                else
                {
                    return RedirectToAction("Login", "Home");
                }

            }
            catch
            {
                _whereclause = "";
            }
            int p = (page == null) ? 1 : (int)page;
            int pg_size = 10;
            try
            {
                pg_size = int.Parse(ConfigurationManager.AppSettings["PAGE_SIZE"].ToString());
            }
            catch
            {
                pg_size = 10;
            }
            List<dynamic> list_lading = new List<dynamic>();
            if (!string.IsNullOrEmpty(_whereclause))
            {
                using (var client = new HttpClient())
                {
                    try
                    {
                        client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var mylading = new
                        {
                            objectName = "Lading",
                            whereClause = _whereclause,
                            rptType = 1
                        };
                        try
                        {

                            var response = client.PostAsJsonAsync("api/Lading?function=lading", mylading).Result;
                            list_lading = response.Content.ReadAsAsync<IEnumerable<dynamic>>().Result.ToList();
                            //list_lading = (from c in list_lading orderby c.CREATEDDATE descending select c).ToList();
                            ViewBag.total_page = (list_lading.Count + pg_size - 1) / pg_size;
                            ViewBag.total_item = list_lading.Count;
                            int currentPageIndex = page.HasValue ? page.Value : 1;
                            ViewBag.Page = (currentPageIndex - 1) * pg_size;
                        }
                        catch { }
                    }
                    catch
                    {
                        //list_lading = null;
                    }
                }

                long sum_main_fee = 0;
                long sum_service_fee = 0;
                long sum_total_fee = 0;
                long sum_cod_fee = 0;
                long sum_total = 0;
                long sum_value = 0;
                long sum_number = 0;
                foreach (dynamic l in list_lading)
                {
                    if (l.VALUE == null) l.VALUE = "0";
                    if (l.MAIN_FEE == null) l.MAIN_FEE = "0";
                    if (l.SERVICE_FEE == null) l.SERVICE_FEE = "0";
                    if (l.TOTAL_FEE == null) l.TOTAL_FEE = 0;
                    sum_main_fee += long.Parse(string.Format("{0}", l.MAIN_FEE));
                    sum_number += 1;
                    sum_cod_fee += long.Parse(string.Format("{0}", l.COD_FEE));
                    sum_service_fee += long.Parse(string.Format("{0}", l.SERVICE_FEE));
                    sum_total_fee += long.Parse(string.Format("{0}", l.MAIN_FEE)) + long.Parse(string.Format("{0}", l.COD_FEE)) + long.Parse(string.Format("{0}", l.SERVICE_FEE));
                    //if (l.TYPE != 1)
                    //{
                    sum_value += long.Parse(string.Format("{0}", l.VALUE));
                    if (l.COD_FEE != null && l.COD_FEE > 0)
                    {
                        sum_total += long.Parse(string.Format("{0}", l.MAIN_FEE)) + long.Parse(string.Format("{0}", l.COD_FEE)) + long.Parse(string.Format("{0}", l.SERVICE_FEE)) + long.Parse(string.Format("{0}", l.VALUE));
                    }
                    //}
                }

                ViewBag.sum_main_fee = sum_main_fee;
                ViewBag.sum_service_fee = sum_service_fee;
                ViewBag.sum_total_fee = sum_total_fee;
                ViewBag.sum_total = sum_total;
                ViewBag.sum_value = sum_value;
                ViewBag.sum_number = sum_number;
                ViewBag.cod_fee = sum_cod_fee;

                int skip = (p - 1) * pg_size;
                list_lading = (from c in list_lading orderby c.CREATEDDATE descending select c).Skip(skip).Take(pg_size).ToList<dynamic>();
                ViewBag.listLading = list_lading;
            }
            else
            {
                list_lading = null;
                ViewBag.listLading = list_lading;
            }
            return View(ViewBag.listLading);

        }
        public ActionResult ListRptTotal(string status, string fromdt, string todt, string busscode, string srvCode, int? page)
        {
            string _whereclause = "";
            _status = status;
            _busscode = busscode;
            _srvCode = srvCode;
            if (!string.IsNullOrEmpty(todt))
            {
                todt = todt.Substring(3, 2) + "/" + todt.Substring(0, 2) + "/" + todt.Substring(6, 4);
                _todate = todt;
            }
            if (!string.IsNullOrEmpty(fromdt))
            {
                fromdt = fromdt.Substring(3, 2) + "/" + fromdt.Substring(0, 2) + "/" + fromdt.Substring(6, 4);
                _fromdate = fromdt;
            }

            int p = (page == null) ? 1 : (int)page;
            int pg_size = 10;
            try
            {
                pg_size = int.Parse(ConfigurationManager.AppSettings["PAGE_SIZE"].ToString());
            }
            catch
            {
                pg_size = 10;
            }
            try
            {
                if (string.IsNullOrEmpty(fromdt) && !string.IsNullOrEmpty(todt))
                {
                    _whereclause += _whereclause + DateTime.Parse(todt).ToString("dd/MM/yyyy") + ";" + DateTime.Parse(todt).ToString("dd/MM/yyyy") + ";" + busscode + ";" + status + ";" + srvCode + ";" + "N" + ";" + "N";
                }
                else if (!string.IsNullOrEmpty(fromdt) && string.IsNullOrEmpty(todt))
                {
                    _whereclause += _whereclause + DateTime.Parse(fromdt).ToString("dd/MM/yyyy") + ";" + DateTime.Parse(fromdt).ToString("dd/MM/yyyy") + ";" + busscode + ";" + status + ";" + srvCode + ";" + "N" + ";" + "N";
                }
                else if (!string.IsNullOrEmpty(fromdt) && !string.IsNullOrEmpty(todt))
                {
                    _whereclause += _whereclause + DateTime.Parse(fromdt).ToString("dd/MM/yyyy") + ";" + DateTime.Parse(todt).ToString("dd/MM/yyyy") + ";" + busscode + ";" + status + ";" + srvCode + ";" + "N" + ";" + "N";
                }
                else
                {
                    _whereclause += _whereclause + DateTime.Now.ToString("dd/MM/yyyy") + ";" + DateTime.Now.ToString("dd/MM/yyyy") + ";" + busscode + ";" + status + ";" + srvCode + ";" + "N" + ";" + "N";
                }
            }
            catch
            {
                _whereclause = "";
            }
            List<dynamic> list_lading = new List<dynamic>();
            List<dynamic> _lstLading = new List<dynamic>();
            List<Models.Lading> _listLading = new List<Models.Lading>();
            if (!string.IsNullOrEmpty(_whereclause))
            {
                using (var client = new HttpClient())
                {
                    try
                    {
                        client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var mylading = new
                        {
                            objectName = "PKG_TOTAL_LADING.GetLadingTotal",
                            whereClause = _whereclause,
                            rptType = 0,
                            expType = 2
                        };
                        try
                        {
                            var response = client.PostAsJsonAsync("api/Lading?function=lading", mylading).Result;
                            list_lading = response.Content.ReadAsAsync<IEnumerable<dynamic>>().Result.ToList();


                        }
                        catch { }
                    }
                    catch
                    {
                        //list_lading = null;
                    }
                }

                long sum_main_fee = 0;
                long sum_service_fee = 0;
                long sum_total_fee = 0;
                long sum_cod_fee = 0;
                long sum_total = 0;
                long sum_value = 0;
                long sum_number = 0;
                foreach (dynamic l in list_lading)
                {
                    if (l.VALUE == null) l.VALUE = "0";
                    if (l.MAIN_FEE == null) l.MAIN_FEE = "0";
                    if (l.SERVICE_FEE == null) l.SERVICE_FEE = "0";
                    if (l.TOTAL_FEE == null) l.TOTAL_FEE = 0;
                    sum_main_fee += long.Parse(string.Format("{0}", l.MAIN_FEE));
                    sum_number += long.Parse(string.Format("{0}", l.QUANTITY));
                    sum_cod_fee += long.Parse(string.Format("{0}", l.COD_FEE));
                    sum_service_fee += long.Parse(string.Format("{0}", l.SERVICE_FEE));
                    sum_total_fee += long.Parse(string.Format("{0}", l.MAIN_FEE)) + long.Parse(string.Format("{0}", l.COD_FEE)) + long.Parse(string.Format("{0}", l.SERVICE_FEE));
                    sum_value += long.Parse(string.Format("{0}", l.VALUE));

                    sum_total += long.Parse(string.Format("{0}", l.AMOUNT_NO_COD));
                }
                ViewBag.sum_main_fee = sum_main_fee;
                ViewBag.sum_service_fee = sum_service_fee;
                ViewBag.sum_total_fee = sum_total_fee;
                ViewBag.sum_total = sum_total;
                ViewBag.sum_value = sum_value;
                ViewBag.sum_number = sum_number;
                ViewBag.cod_fee = sum_cod_fee;

                int skip = (p - 1) * pg_size;


                list_lading = (from c in list_lading orderby c.TRANS_DATE ascending select c).ToList<dynamic>();
                //list_lading = (from c in list_lading orderby c.TRANS_DATE ascending select c).Skip(skip).Take(pg_size).ToList<dynamic>();
                foreach (dynamic _idDyna in list_lading)
                {
                    dynamic _itemDyna = new DynamicObj();
                    _itemDyna.TRANS_DATE = _idDyna.TRANS_DATE;
                    _itemDyna.TOTALAMOUNT = _idDyna.TOTALAMOUNT;
                    _itemDyna.VALUE = _idDyna.VALUE;
                    _itemDyna.QUANTITY = _idDyna.QUANTITY;
                    _itemDyna.MAIN_FEE = _idDyna.MAIN_FEE;
                    _itemDyna.COD_FEE = _idDyna.COD_FEE;
                    _itemDyna.SERVICE_FEE = _idDyna.SERVICE_FEE;
                    _itemDyna.AMOUNT_NO_COD = _idDyna.AMOUNT_NO_COD;

                    _lstLading.Add(_itemDyna);
                }
                _lstLading = (from c in _lstLading
                              group c by c.TRANS_DATE into newGroup
                              orderby newGroup.Key
                              select new
                              {
                                  TRANS_DATE = newGroup.Key.ToString("dd/MM/yyyy"),
                                  TOTALAMOUNT = (int)newGroup.Average(x => x.TOTALAMOUNT),
                                  VALUE = (int)newGroup.Sum(c => c.VALUE),
                                  QUANTITY = (int)newGroup.Sum(c => c.QUANTITY),
                                  MAIN_FEE = (int)newGroup.Sum(c => c.MAIN_FEE),
                                  COD_FEE = (int)newGroup.Sum(c => c.COD_FEE),
                                  SERVICE_FEE = (int)newGroup.Sum(c => c.SERVICE_FEE),
                                  AMOUNT_NO_COD = (int)newGroup.Sum(c => c.AMOUNT_NO_COD)
                              }).ToList<dynamic>();
                List<dynamic> _newLst = new List<dynamic>();
                for (int i = 0; i < _lstLading.Count; i++)
                {
                    dynamic _dynald = new DynamicObj();
                    _dynald.TRANS_DATE = _lstLading[i].TRANS_DATE;
                    _dynald.TOTALAMOUNT = _lstLading[i].TOTALAMOUNT;
                    _dynald.VALUE = _lstLading[i].VALUE;
                    _dynald.QUANTITY = _lstLading[i].QUANTITY;
                    _dynald.MAIN_FEE = _lstLading[i].MAIN_FEE;
                    _dynald.COD_FEE = _lstLading[i].COD_FEE;
                    _dynald.SERVICE_FEE = _lstLading[i].SERVICE_FEE;
                    _dynald.AMOUNT_NO_COD = _lstLading[i].AMOUNT_NO_COD;
                    _newLst.Add(_dynald);
                }

                ViewBag.total_page = (_newLst.Count + pg_size - 1) / pg_size;
                ViewBag.total_item = _newLst.Count;
                int currentPageIndex = page.HasValue ? page.Value : 1;
                ViewBag.Page = (currentPageIndex - 1) * pg_size;
                _newLst = (from c in _newLst orderby c.TRANS_DATE ascending select c).Skip(skip).Take(pg_size).ToList<dynamic>();
                ViewBag.listLading = _newLst;
            }
            else
            {
                _lstLading = null;
                ViewBag.listLading = _lstLading;
            }
            return View(ViewBag.listLading);

        }
        public ActionResult ListRptPayDetail(string status, string fromdt, string todt, string busscode, string srvCode, int? page)
        {
            string _whereclause = " AND PAYCODE is not null";
            _status = status;
            _busscode = busscode;
            _srvCode = srvCode;
            if (!string.IsNullOrEmpty(todt))
            {
                todt = todt.Substring(3, 2) + "/" + todt.Substring(0, 2) + "/" + todt.Substring(6, 4);
                _todate = todt;
            }
            if (!string.IsNullOrEmpty(fromdt))
            {
                fromdt = fromdt.Substring(3, 2) + "/" + fromdt.Substring(0, 2) + "/" + fromdt.Substring(6, 4);
                _fromdate = fromdt;
            }

            try
            {
                if (!string.IsNullOrEmpty(status) && status != "0")
                {
                    _whereclause += " AND STATUS='" + status + "' ";
                }

                if (!string.IsNullOrEmpty(busscode))
                {
                    _whereclause += " AND CUSTOMER_CODE='" + busscode + "' ";
                }

                if (!string.IsNullOrEmpty(fromdt) && string.IsNullOrEmpty(todt))
                {
                    _whereclause += " AND trunc(CREATEDDATE)=to_date('" + DateTime.Parse(fromdt).ToString("dd/MM/yyyy") + "','dd/MM/yyyy') ";
                }

                if (!string.IsNullOrEmpty(todt) && string.IsNullOrEmpty(fromdt))
                {
                    _whereclause += " AND trunc(CREATEDDATE)=to_date'" + DateTime.Parse(todt).ToString("dd/MM/yyyy") + "','dd/MM/yyyy') ";
                }

                if (!string.IsNullOrEmpty(todt) && !string.IsNullOrEmpty(fromdt))
                {
                    _whereclause += " AND trunc(CREATEDDATE) between to_date('" + DateTime.Parse(fromdt).ToString("dd/MM/yyyy") + "','dd/MM/yyyy') and to_date('" + DateTime.Parse(todt).ToString("dd/MM/yyyy") + "','dd/MM/yyyy') ";
                }

                if (!string.IsNullOrEmpty(srvCode) && srvCode != "00")
                {
                    _whereclause += " AND TYPE = " + int.Parse(srvCode);
                }

            }
            catch
            {
                _whereclause = " AND PayCode IS NOT NULL";
            }
            int p = (page == null) ? 1 : (int)page;
            int pg_size = 10;
            try
            {
                pg_size = int.Parse(ConfigurationManager.AppSettings["PAGE_SIZE"].ToString());
            }
            catch
            {
                pg_size = 10;
            }
            List<dynamic> list_lading = new List<dynamic>();
            if (!string.IsNullOrEmpty(_whereclause))
            {
                using (var client = new HttpClient())
                {
                    try
                    {
                        client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var mylading = new
                        {
                            objectName = "Lading",
                            whereClause = _whereclause,
                            rptType = 1
                        };
                        try
                        {

                            var response = client.PostAsJsonAsync("api/Lading", mylading).Result;
                            list_lading = response.Content.ReadAsAsync<IEnumerable<dynamic>>().Result.ToList();
                            //list_lading = (from c in list_lading orderby c.CREATEDDATE descending select c).ToList();
                            ViewBag.total_page = (list_lading.Count + pg_size - 1) / pg_size;
                            ViewBag.total_item = list_lading.Count;
                            int currentPageIndex = page.HasValue ? page.Value : 1;
                            ViewBag.Page = (currentPageIndex - 1) * pg_size;
                        }
                        catch { }
                    }
                    catch
                    {
                        //list_lading = null;
                    }
                }

                long sum_main_fee = 0;
                long sum_service_fee = 0;
                long sum_total_fee = 0;
                long sum_cod_fee = 0;
                long sum_total = 0;
                long sum_value = 0;
                long sum_number = 0;
                foreach (dynamic l in list_lading)
                {
                    if (l.VALUE == null) l.VALUE = "0";
                    if (l.MAIN_FEE == null) l.MAIN_FEE = "0";
                    if (l.SERVICE_FEE == null) l.SERVICE_FEE = "0";
                    if (l.TOTAL_FEE == null) l.TOTAL_FEE = 0;
                    sum_main_fee += long.Parse(string.Format("{0}", l.MAIN_FEE));
                    sum_number += 1;
                    sum_cod_fee += long.Parse(string.Format("{0}", l.COD_FEE));
                    sum_service_fee += long.Parse(string.Format("{0}", l.SERVICE_FEE));
                    sum_total_fee += long.Parse(string.Format("{0}", l.MAIN_FEE)) + long.Parse(string.Format("{0}", l.COD_FEE)) + long.Parse(string.Format("{0}", l.SERVICE_FEE));
                    //if (l.TYPE != 1)
                    //{
                    sum_value += long.Parse(string.Format("{0}", l.VALUE));
                    if (l.COD_FEE != null && l.COD_FEE > 0)
                    {
                        sum_total += long.Parse(string.Format("{0}", l.MAIN_FEE)) + long.Parse(string.Format("{0}", l.COD_FEE)) + long.Parse(string.Format("{0}", l.SERVICE_FEE)) + long.Parse(string.Format("{0}", l.VALUE));
                    }
                    //}
                }
                ViewBag.sum_main_fee = sum_main_fee;
                ViewBag.sum_service_fee = sum_service_fee;
                ViewBag.sum_total_fee = sum_total_fee;
                ViewBag.sum_total = sum_total;
                ViewBag.sum_value = sum_value;
                ViewBag.sum_number = sum_number;
                ViewBag.cod_fee = sum_cod_fee;

                int skip = (p - 1) * pg_size;
                list_lading = (from c in list_lading orderby c.CREATEDDATE descending select c).Skip(skip).Take(pg_size).ToList<dynamic>();
                ViewBag.listLading = list_lading;
            }
            else
            {
                list_lading = null;
                ViewBag.listLading = list_lading;
            }
            return View(ViewBag.listLading);

        }

        public ActionResult ListRptPayTotal(string status, string fromdt, string todt, string busscode, string srvCode, int? page)
        {
            string _whereclause = "";
            _status = status;
            _busscode = busscode;
            _srvCode = srvCode;
            if (!string.IsNullOrEmpty(todt))
            {
                todt = todt.Substring(3, 2) + "/" + todt.Substring(0, 2) + "/" + todt.Substring(6, 4);
                _todate = todt;
            }
            if (!string.IsNullOrEmpty(fromdt))
            {
                fromdt = fromdt.Substring(3, 2) + "/" + fromdt.Substring(0, 2) + "/" + fromdt.Substring(6, 4);
                _fromdate = fromdt;
            }

            int p = (page == null) ? 1 : (int)page;
            int pg_size = 10;
            try
            {
                pg_size = int.Parse(ConfigurationManager.AppSettings["PAGE_SIZE"].ToString());
            }
            catch
            {
                pg_size = 10;
            }
            try
            {
                if (string.IsNullOrEmpty(fromdt) && !string.IsNullOrEmpty(todt))
                {
                    _whereclause += _whereclause + DateTime.Parse(todt).ToString("dd/MM/yyyy") + ";" + DateTime.Parse(todt).ToString("dd/MM/yyyy") + ";" + busscode + ";" + status + ";" + srvCode + ";" + "N" + ";" + "Y";
                }
                else if (!string.IsNullOrEmpty(fromdt) && string.IsNullOrEmpty(todt))
                {
                    _whereclause += _whereclause + DateTime.Parse(fromdt).ToString("dd/MM/yyyy") + ";" + DateTime.Parse(fromdt).ToString("dd/MM/yyyy") + ";" + busscode + ";" + status + ";" + srvCode + ";" + "N" + ";" + "Y";
                }
                else if (!string.IsNullOrEmpty(fromdt) && !string.IsNullOrEmpty(todt))
                {
                    _whereclause += _whereclause + DateTime.Parse(fromdt).ToString("dd/MM/yyyy") + ";" + DateTime.Parse(todt).ToString("dd/MM/yyyy") + ";" + busscode + ";" + status + ";" + srvCode + ";" + "N" + ";" + "Y";
                }
                else
                {
                    _whereclause += _whereclause + DateTime.Now.ToString("dd/MM/yyyy") + ";" + DateTime.Now.ToString("dd/MM/yyyy") + ";" + busscode + ";" + status + ";" + srvCode + ";" + "N" + ";" + "Y";
                }
            }
            catch
            {
                _whereclause = "";
            }
            List<dynamic> list_lading = new List<dynamic>();
            List<dynamic> _lstLading = new List<dynamic>();
            List<Models.Lading> _listLading = new List<Models.Lading>();
            if (!string.IsNullOrEmpty(_whereclause))
            {
                using (var client = new HttpClient())
                {
                    try
                    {
                        client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var mylading = new
                        {
                            objectName = "PKG_TOTAL_LADING.GetLadingTotal",
                            whereClause = _whereclause,
                            rptType = 0,
                            expType = 2
                        };
                        try
                        {
                            var response = client.PostAsJsonAsync("api/Lading", mylading).Result;
                            list_lading = response.Content.ReadAsAsync<IEnumerable<dynamic>>().Result.ToList();


                        }
                        catch { }
                    }
                    catch
                    {
                        //list_lading = null;
                    }
                }

                long sum_main_fee = 0;
                long sum_service_fee = 0;
                long sum_total_fee = 0;
                long sum_cod_fee = 0;
                long sum_total = 0;
                long sum_value = 0;
                long sum_number = 0;
                foreach (dynamic l in list_lading)
                {
                    if (l.VALUE == null) l.VALUE = "0";
                    if (l.MAIN_FEE == null) l.MAIN_FEE = "0";
                    if (l.SERVICE_FEE == null) l.SERVICE_FEE = "0";
                    if (l.TOTAL_FEE == null) l.TOTAL_FEE = 0;
                    sum_main_fee += long.Parse(string.Format("{0}", l.MAIN_FEE));
                    sum_number += long.Parse(string.Format("{0}", l.QUANTITY));
                    sum_cod_fee += long.Parse(string.Format("{0}", l.COD_FEE));
                    sum_service_fee += long.Parse(string.Format("{0}", l.SERVICE_FEE));
                    sum_total_fee += long.Parse(string.Format("{0}", l.MAIN_FEE)) + long.Parse(string.Format("{0}", l.COD_FEE)) + long.Parse(string.Format("{0}", l.SERVICE_FEE));
                    sum_value += long.Parse(string.Format("{0}", l.VALUE));

                    sum_total += long.Parse(string.Format("{0}", l.AMOUNT_NO_COD));
                }
                ViewBag.sum_main_fee = sum_main_fee;
                ViewBag.sum_service_fee = sum_service_fee;
                ViewBag.sum_total_fee = sum_total_fee;
                ViewBag.sum_total = sum_total;
                ViewBag.sum_value = sum_value;
                ViewBag.sum_number = sum_number;
                ViewBag.cod_fee = sum_cod_fee;

                int skip = (p - 1) * pg_size;


                list_lading = (from c in list_lading orderby c.TRANS_DATE ascending select c).ToList<dynamic>();
                //list_lading = (from c in list_lading orderby c.TRANS_DATE ascending select c).Skip(skip).Take(pg_size).ToList<dynamic>();
                foreach (dynamic _idDyna in list_lading)
                {
                    dynamic _itemDyna = new DynamicObj();
                    _itemDyna.TRANS_DATE = _idDyna.TRANS_DATE;
                    _itemDyna.TOTALAMOUNT = _idDyna.TOTALAMOUNT;
                    _itemDyna.VALUE = _idDyna.VALUE;
                    _itemDyna.QUANTITY = _idDyna.QUANTITY;
                    _itemDyna.MAIN_FEE = _idDyna.MAIN_FEE;
                    _itemDyna.COD_FEE = _idDyna.COD_FEE;
                    _itemDyna.SERVICE_FEE = _idDyna.SERVICE_FEE;
                    _itemDyna.AMOUNT_NO_COD = _idDyna.AMOUNT_NO_COD;

                    _lstLading.Add(_itemDyna);
                }
                _lstLading = (from c in _lstLading
                              group c by c.TRANS_DATE into newGroup
                              orderby newGroup.Key
                              select new
                              {
                                  TRANS_DATE = newGroup.Key.ToString("dd/MM/yyyy"),
                                  TOTALAMOUNT = (int)newGroup.Average(x => x.TOTALAMOUNT),
                                  VALUE = (int)newGroup.Sum(c => c.VALUE),
                                  QUANTITY = (int)newGroup.Sum(c => c.QUANTITY),
                                  MAIN_FEE = (int)newGroup.Sum(c => c.MAIN_FEE),
                                  COD_FEE = (int)newGroup.Sum(c => c.COD_FEE),
                                  SERVICE_FEE = (int)newGroup.Sum(c => c.SERVICE_FEE),
                                  AMOUNT_NO_COD = (int)newGroup.Sum(c => c.AMOUNT_NO_COD)
                              }).ToList<dynamic>();
                List<dynamic> _newLst = new List<dynamic>();
                for (int i = 0; i < _lstLading.Count; i++)
                {
                    dynamic _dynald = new DynamicObj();
                    _dynald.TRANS_DATE = _lstLading[i].TRANS_DATE;
                    _dynald.TOTALAMOUNT = _lstLading[i].TOTALAMOUNT;
                    _dynald.VALUE = _lstLading[i].VALUE;
                    _dynald.QUANTITY = _lstLading[i].QUANTITY;
                    _dynald.MAIN_FEE = _lstLading[i].MAIN_FEE;
                    _dynald.COD_FEE = _lstLading[i].COD_FEE;
                    _dynald.SERVICE_FEE = _lstLading[i].SERVICE_FEE;
                    _dynald.AMOUNT_NO_COD = _lstLading[i].AMOUNT_NO_COD;
                    _newLst.Add(_dynald);
                }

                ViewBag.total_page = (_newLst.Count + pg_size - 1) / pg_size;
                ViewBag.total_item = _newLst.Count;
                int currentPageIndex = page.HasValue ? page.Value : 1;
                ViewBag.Page = (currentPageIndex - 1) * pg_size;
                _newLst = (from c in _newLst orderby c.TRANS_DATE ascending select c).Skip(skip).Take(pg_size).ToList<dynamic>();
                ViewBag.listLading = _newLst;
            }
            else
            {
                _lstLading = null;
                ViewBag.listLading = _lstLading;
            }
            return View(ViewBag.listLading);

        }
        public ActionResult ListRptBillDetail(string status, string fromdt, string todt, string busscode, string srvCode, int? page)
        {
            string _whereclause = " AND BILLCODE IS NOT NULL";
            _status = status;
            _busscode = busscode;
            _srvCode = srvCode;
            if (!string.IsNullOrEmpty(todt))
            {
                todt = todt.Substring(3, 2) + "/" + todt.Substring(0, 2) + "/" + todt.Substring(6, 4);
                _todate = todt;
            }
            if (!string.IsNullOrEmpty(fromdt))
            {
                fromdt = fromdt.Substring(3, 2) + "/" + fromdt.Substring(0, 2) + "/" + fromdt.Substring(6, 4);
                _fromdate = fromdt;
            }

            try
            {
                if (!string.IsNullOrEmpty(status) && status != "0")
                {
                    _whereclause += " AND STATUS='" + status + "' ";
                }

                if (!string.IsNullOrEmpty(busscode))
                {
                    _whereclause += " AND CUSTOMER_CODE='" + busscode + "' ";
                }

                if (!string.IsNullOrEmpty(fromdt) && string.IsNullOrEmpty(todt))
                {
                    _whereclause += " AND trunc(CREATEDDATE)=to_date('" + DateTime.Parse(fromdt).ToString("dd/MM/yyyy") + "','dd/MM/yyyy') ";
                }

                if (!string.IsNullOrEmpty(todt) && string.IsNullOrEmpty(fromdt))
                {
                    _whereclause += " AND trunc(CREATEDDATE)=to_date'" + DateTime.Parse(todt).ToString("dd/MM/yyyy") + "','dd/MM/yyyy') ";
                }

                if (!string.IsNullOrEmpty(todt) && !string.IsNullOrEmpty(fromdt))
                {
                    _whereclause += " AND trunc(CREATEDDATE) between to_date('" + DateTime.Parse(fromdt).ToString("dd/MM/yyyy") + "','dd/MM/yyyy') and to_date('" + DateTime.Parse(todt).ToString("dd/MM/yyyy") + "','dd/MM/yyyy') ";
                }

                if (!string.IsNullOrEmpty(srvCode) && srvCode != "00")
                {
                    _whereclause += " AND TYPE = " + int.Parse(srvCode);
                }

            }
            catch
            {
                _whereclause = " AND BillCode is not null ";
            }
            int p = (page == null) ? 1 : (int)page;
            int pg_size = 10;
            try
            {
                pg_size = int.Parse(ConfigurationManager.AppSettings["PAGE_SIZE"].ToString());
            }
            catch
            {
                pg_size = 10;
            }
            List<dynamic> list_lading = new List<dynamic>();
            if (!string.IsNullOrEmpty(_whereclause))
            {
                using (var client = new HttpClient())
                {
                    try
                    {
                        client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var mylading = new
                        {
                            objectName = "Lading",
                            whereClause = _whereclause,
                            rptType = 1
                        };
                        try
                        {

                            var response = client.PostAsJsonAsync("api/Lading", mylading).Result;
                            list_lading = response.Content.ReadAsAsync<IEnumerable<dynamic>>().Result.ToList();
                            //list_lading = (from c in list_lading orderby c.CREATEDDATE descending select c).ToList();
                            ViewBag.total_page = (list_lading.Count + pg_size - 1) / pg_size;
                            ViewBag.total_item = list_lading.Count;
                            int currentPageIndex = page.HasValue ? page.Value : 1;
                            ViewBag.Page = (currentPageIndex - 1) * pg_size;
                        }
                        catch { }
                    }
                    catch
                    {
                        //list_lading = null;
                    }
                }

                long sum_main_fee = 0;
                long sum_service_fee = 0;
                long sum_total_fee = 0;
                long sum_cod_fee = 0;
                long sum_total = 0;
                long sum_value = 0;
                long sum_number = 0;
                foreach (dynamic l in list_lading)
                {
                    if (l.VALUE == null) l.VALUE = "0";
                    if (l.MAIN_FEE == null) l.MAIN_FEE = "0";
                    if (l.SERVICE_FEE == null) l.SERVICE_FEE = "0";
                    if (l.TOTAL_FEE == null) l.TOTAL_FEE = 0;
                    sum_main_fee += long.Parse(string.Format("{0}", l.MAIN_FEE));
                    sum_number += 1;
                    sum_cod_fee += long.Parse(string.Format("{0}", l.COD_FEE));
                    sum_service_fee += long.Parse(string.Format("{0}", l.SERVICE_FEE));
                    sum_total_fee += long.Parse(string.Format("{0}", l.MAIN_FEE)) + long.Parse(string.Format("{0}", l.COD_FEE)) + long.Parse(string.Format("{0}", l.SERVICE_FEE));
                    //if (l.TYPE != 1)
                    //{
                    sum_value += long.Parse(string.Format("{0}", l.VALUE));
                    if (l.COD_FEE != null && l.COD_FEE > 0)
                    {
                        sum_total += long.Parse(string.Format("{0}", l.MAIN_FEE)) + long.Parse(string.Format("{0}", l.COD_FEE)) + long.Parse(string.Format("{0}", l.SERVICE_FEE)) + long.Parse(string.Format("{0}", l.VALUE));
                    }
                    //}
                }
                ViewBag.sum_main_fee = sum_main_fee;
                ViewBag.sum_service_fee = sum_service_fee;
                ViewBag.sum_total_fee = sum_total_fee;
                ViewBag.sum_total = sum_total;
                ViewBag.sum_value = sum_value;
                ViewBag.sum_number = sum_number;
                ViewBag.cod_fee = sum_cod_fee;

                int skip = (p - 1) * pg_size;
                list_lading = (from c in list_lading orderby c.CREATEDDATE descending select c).Skip(skip).Take(pg_size).ToList<dynamic>();
                ViewBag.listLading = list_lading;
            }
            else
            {
                list_lading = null;
                ViewBag.listLading = list_lading;
            }
            return View(ViewBag.listLading);

        }

        public ActionResult ListRptBillTotal(string status, string fromdt, string todt, string busscode, string srvCode, int? page)
        {
            string _whereclause = "";
            _status = status;
            _busscode = busscode;
            _srvCode = srvCode;
            if (!string.IsNullOrEmpty(todt))
            {
                todt = todt.Substring(3, 2) + "/" + todt.Substring(0, 2) + "/" + todt.Substring(6, 4);
                _todate = todt;
            }
            if (!string.IsNullOrEmpty(fromdt))
            {
                fromdt = fromdt.Substring(3, 2) + "/" + fromdt.Substring(0, 2) + "/" + fromdt.Substring(6, 4);
                _fromdate = fromdt;
            }

            int p = (page == null) ? 1 : (int)page;
            int pg_size = 10;
            try
            {
                pg_size = int.Parse(ConfigurationManager.AppSettings["PAGE_SIZE"].ToString());
            }
            catch
            {
                pg_size = 10;
            }
            try
            {
                if (string.IsNullOrEmpty(fromdt) && !string.IsNullOrEmpty(todt))
                {
                    _whereclause += _whereclause + DateTime.Parse(todt).ToString("dd/MM/yyyy") + ";" + DateTime.Parse(todt).ToString("dd/MM/yyyy") + ";" + busscode + ";" + status + ";" + srvCode + ";" + "Y" + ";" + "N";
                }
                else if (!string.IsNullOrEmpty(fromdt) && string.IsNullOrEmpty(todt))
                {
                    _whereclause += _whereclause + DateTime.Parse(fromdt).ToString("dd/MM/yyyy") + ";" + DateTime.Parse(fromdt).ToString("dd/MM/yyyy") + ";" + busscode + ";" + status + ";" + srvCode + ";" + "Y" + ";" + "N";
                }
                else if (!string.IsNullOrEmpty(fromdt) && !string.IsNullOrEmpty(todt))
                {
                    _whereclause += _whereclause + DateTime.Parse(fromdt).ToString("dd/MM/yyyy") + ";" + DateTime.Parse(todt).ToString("dd/MM/yyyy") + ";" + busscode + ";" + status + ";" + srvCode + ";" + "Y" + ";" + "N";
                }
                else
                {
                    _whereclause += _whereclause + DateTime.Now.ToString("dd/MM/yyyy") + ";" + DateTime.Now.ToString("dd/MM/yyyy") + ";" + busscode + ";" + status + ";" + srvCode + ";" + "Y" + ";" + "N";
                }
            }
            catch
            {
                _whereclause = "";
            }
            List<dynamic> list_lading = new List<dynamic>();
            List<dynamic> _lstLading = new List<dynamic>();
            List<Models.Lading> _listLading = new List<Models.Lading>();
            if (!string.IsNullOrEmpty(_whereclause))
            {
                using (var client = new HttpClient())
                {
                    try
                    {
                        client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var mylading = new
                        {
                            objectName = "PKG_TOTAL_LADING.GetLadingTotal",
                            whereClause = _whereclause,
                            rptType = 0,
                            expType = 2
                        };
                        try
                        {
                            var response = client.PostAsJsonAsync("api/Lading", mylading).Result;
                            list_lading = response.Content.ReadAsAsync<IEnumerable<dynamic>>().Result.ToList();


                        }
                        catch { }
                    }
                    catch
                    {
                        //list_lading = null;
                    }
                }

                long sum_main_fee = 0;
                long sum_service_fee = 0;
                long sum_total_fee = 0;
                long sum_cod_fee = 0;
                long sum_total = 0;
                long sum_value = 0;
                long sum_number = 0;
                foreach (dynamic l in list_lading)
                {
                    if (l.VALUE == null) l.VALUE = "0";
                    if (l.MAIN_FEE == null) l.MAIN_FEE = "0";
                    if (l.SERVICE_FEE == null) l.SERVICE_FEE = "0";
                    if (l.TOTAL_FEE == null) l.TOTAL_FEE = 0;
                    sum_main_fee += long.Parse(string.Format("{0}", l.MAIN_FEE));
                    sum_number += long.Parse(string.Format("{0}", l.QUANTITY));
                    sum_cod_fee += long.Parse(string.Format("{0}", l.COD_FEE));
                    sum_service_fee += long.Parse(string.Format("{0}", l.SERVICE_FEE));
                    sum_total_fee += long.Parse(string.Format("{0}", l.MAIN_FEE)) + long.Parse(string.Format("{0}", l.COD_FEE)) + long.Parse(string.Format("{0}", l.SERVICE_FEE));
                    sum_value += long.Parse(string.Format("{0}", l.VALUE));

                    sum_total += long.Parse(string.Format("{0}", l.AMOUNT_NO_COD));
                }
                ViewBag.sum_main_fee = sum_main_fee;
                ViewBag.sum_service_fee = sum_service_fee;
                ViewBag.sum_total_fee = sum_total_fee;
                ViewBag.sum_total = sum_total;
                ViewBag.sum_value = sum_value;
                ViewBag.sum_number = sum_number;
                ViewBag.cod_fee = sum_cod_fee;

                int skip = (p - 1) * pg_size;


                list_lading = (from c in list_lading orderby c.TRANS_DATE ascending select c).ToList<dynamic>();
                //list_lading = (from c in list_lading orderby c.TRANS_DATE ascending select c).Skip(skip).Take(pg_size).ToList<dynamic>();
                foreach (dynamic _idDyna in list_lading)
                {
                    dynamic _itemDyna = new DynamicObj();
                    _itemDyna.TRANS_DATE = _idDyna.TRANS_DATE;
                    _itemDyna.TOTALAMOUNT = _idDyna.TOTALAMOUNT;
                    _itemDyna.VALUE = _idDyna.VALUE;
                    _itemDyna.QUANTITY = _idDyna.QUANTITY;
                    _itemDyna.MAIN_FEE = _idDyna.MAIN_FEE;
                    _itemDyna.COD_FEE = _idDyna.COD_FEE;
                    _itemDyna.SERVICE_FEE = _idDyna.SERVICE_FEE;
                    _itemDyna.AMOUNT_NO_COD = _idDyna.AMOUNT_NO_COD;

                    _lstLading.Add(_itemDyna);
                }
                _lstLading = (from c in _lstLading
                              group c by c.TRANS_DATE into newGroup
                              orderby newGroup.Key
                              select new
                              {
                                  TRANS_DATE = newGroup.Key.ToString("dd/MM/yyyy"),
                                  TOTALAMOUNT = (int)newGroup.Average(x => x.TOTALAMOUNT),
                                  VALUE = (int)newGroup.Sum(c => c.VALUE),
                                  QUANTITY = (int)newGroup.Sum(c => c.QUANTITY),
                                  MAIN_FEE = (int)newGroup.Sum(c => c.MAIN_FEE),
                                  COD_FEE = (int)newGroup.Sum(c => c.COD_FEE),
                                  SERVICE_FEE = (int)newGroup.Sum(c => c.SERVICE_FEE),
                                  AMOUNT_NO_COD = (int)newGroup.Sum(c => c.AMOUNT_NO_COD)
                              }).ToList<dynamic>();
                List<dynamic> _newLst = new List<dynamic>();
                for (int i = 0; i < _lstLading.Count; i++)
                {
                    dynamic _dynald = new DynamicObj();
                    _dynald.TRANS_DATE = _lstLading[i].TRANS_DATE;
                    _dynald.TOTALAMOUNT = _lstLading[i].TOTALAMOUNT;
                    _dynald.VALUE = _lstLading[i].VALUE;
                    _dynald.QUANTITY = _lstLading[i].QUANTITY;
                    _dynald.MAIN_FEE = _lstLading[i].MAIN_FEE;
                    _dynald.COD_FEE = _lstLading[i].COD_FEE;
                    _dynald.SERVICE_FEE = _lstLading[i].SERVICE_FEE;
                    _dynald.AMOUNT_NO_COD = _lstLading[i].AMOUNT_NO_COD;
                    _newLst.Add(_dynald);
                }

                ViewBag.total_page = (_newLst.Count + pg_size - 1) / pg_size;
                ViewBag.total_item = _newLst.Count;
                int currentPageIndex = page.HasValue ? page.Value : 1;
                ViewBag.Page = (currentPageIndex - 1) * pg_size;
                _newLst = (from c in _newLst orderby c.TRANS_DATE ascending select c).Skip(skip).Take(pg_size).ToList<dynamic>();
                ViewBag.listLading = _newLst;
            }
            else
            {
                _lstLading = null;
                ViewBag.listLading = _lstLading;
            }
            return View(ViewBag.listLading);

        }

        public JsonResult ListStatus(string title)
        {
            lstStatus = GetLstStatus(title);
            return Json(
                (from e in lstStatus
                 select
                     new
                     {
                         StatusCode = ((dynamic)e).StatusCode,
                         StatusDescription = ((dynamic)e).StatusDescription.Trim()
                     }
                ).ToArray()
                , JsonRequestBehavior.AllowGet);
        }
        public static dynamic[] GetLstStatus(string title)
        {
            dynamic[] items;
            #region Status
            try
            {

                IMongoQuery img = Query.NE("_id", "1");
                IMongoQuery img1 = Query.NE("_id", "2");
                IMongoQuery img2 = Query.NE("_id", "3");

                img = Query.And(img, img1, img2);

                items = PayID.Portal.Configuration.Data_S24.List("Status", img);

                if (!string.IsNullOrEmpty(title))
                {
                    if (title == "tkvdtt" || title == "tkvdtt#")
                    {
                        items = items.Where(x => x.StatusCode == "C5" || x.StatusCode == "C6" || x.StatusCode == "C7").ToArray<dynamic>();
                    }
                    else if (title == "tkvdtg" || title == "tkvdtg#")
                    {
                        items = items.Where(x => x.StatusCode == "C6" || x.StatusCode == "C7" || x.StatusCode == "C8" || x.StatusCode == "C9" || x.StatusCode == "C10").ToArray<dynamic>();
                    }
                    else if (title == "tkvdph" || title == "tkvdph#")
                    {
                        items = items.Where(x => x.StatusCode == "C11" || x.StatusCode == "C12" || x.StatusCode == "C13" || x.StatusCode == "C14" || x.StatusCode == "C15" || x.StatusCode == "C16" || x.StatusCode == "C17" || x.StatusCode == "C18").ToArray<dynamic>();
                    }

                }
            }
            catch { items = null; }
            #endregion
            return items;
        }
        public ActionResult ReportFormName(string id)
        {
            LocalReport lr = new LocalReport();

            string path = Path.Combine(Server.MapPath("~/Areas/Report/Templates/Shipment"), "FormNameShipment.rdlc");
            if (System.IO.File.Exists(path))
            {
                lr.ReportPath = path;
            }
            else
            {
                return View("Index");
            }
            List<Models.Shipment> lstShipment = new List<Models.Shipment>();

            string statuscode = "", provincename = "", districtname = "", posname = "";

            List<dynamic> list_shipment = new List<dynamic>();

            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var myShipment = new
                    {

                        current_assigned = _current_assigned,
                        from_date = String.IsNullOrEmpty(_fromdate) ? "" : DateTime.Parse(_fromdate).ToString("dd/MM/yyyy"),
                        to_date = String.IsNullOrEmpty(_todate) ? "" : DateTime.Parse(_todate).ToString("dd/MM/yyyy"),
                        status = _status,
                        busscode = _busscode,
                        district = _district,
                        pos = _pos,
                        province = _province,
                        FormType = "FormName"
                    };
                    try
                    {
                        var response = client.PostAsJsonAsync("api/Shipment", myShipment).Result;
                        list_shipment = response.Content.ReadAsAsync<IEnumerable<dynamic>>().Result.ToList();
                        //list_shipment = (from c in list_shipment orderby c.CREATEDDATE descending select c).ToList();
                    }
                    catch { }
                }
                catch
                {
                    //list_shipment = null;
                }
            }

            string servCode = "Tất cả";
            for (int i = 0; i < list_shipment.Count; i++)
            {
                var l = list_shipment[i];
                Models.Shipment iShipment = new Models.Shipment();
                iShipment.STT = i + 1;
                iShipment.CustomerName = l.customername;
                iShipment.TrackingCode = l.tracking_code;
                if (l.to_address_name != null && l.to_address_name != "")
                {
                    l.name = l.to_address_name + "\n";
                }
                if (l.to_address_address != null && l.to_address_address != "")
                {
                    l.address = l.to_address_address + "\n";
                }
                if (l.to_address_phone != null && l.to_address_phone != "")
                {
                    l.phone = l.to_address_phone + "\n";
                }
                if (l.ReceiverMobile != null && l.ReceiverMobile != "")
                {
                    l.mobile = l.ReceiverMobile + "\n";
                }
                iShipment.ReceiverInfo = l.name + l.address + l.phone + l.mobile;
                iShipment.ProductVal = l.value;
                if (l.productname != null && l.productname != "")
                {
                    l.pn = l.productname + "\n";
                }
                if (l.productdescription != null && l.to_address_address != "")
                {
                    l.pd = l.productdescription + "\n";
                }
                if (l.value != null && l.to_address_address != 0)
                {
                    l.v = l.value.ToString("N0") + " đ" + "\n";
                }
                if (l.weight != null && l.to_address_address != 0)
                {
                    l.w = l.weight.ToString("N0") + " Gr" + "\n";
                }
                iShipment.Productdes = l.pn + l.pd + l.v + l.w;
                iShipment.MainFee = l.MainFee;
                iShipment.ServiceFee = l.ServiceFee;
                iShipment.CodFee = l.CodFee;
                iShipment.TotalFee = l.TotalFee;
                iShipment.Status = l.system_status;
                iShipment.ToProvince = l.to_province_name;
                iShipment.CustomerCode = l.customercode;
                iShipment.OrderId = l.order_id;
                iShipment.collectvalue = l.collectvalue;
                if (l.MainFee != null)
                {
                    iShipment.MainFee = decimal.Parse(string.IsNullOrEmpty(l.MainFee.ToString()) ? "0" : l.MainFee.ToString());
                }
                else
                {
                    iShipment.MainFee = 0;
                }

                if (l.CodFee != null)
                {
                    iShipment.CodFee = decimal.Parse(string.IsNullOrEmpty(l.CodFee.ToString()) ? "0" : l.CodFee.ToString());
                }
                else
                {
                    iShipment.CodFee = 0;
                }

                if (l.ServiceFee != null)
                {
                    iShipment.ServiceFee = decimal.Parse(string.IsNullOrEmpty(l.ServiceFee.ToString()) ? "0" : l.ServiceFee.ToString());
                }
                else
                {
                    iShipment.ServiceFee = 0;
                }
                if (l.TotalFee != null)
                {
                    iShipment.TotalFee = decimal.Parse(string.IsNullOrEmpty(l.TotalFee.ToString()) ? "0" : l.TotalFee.ToString());
                }
                else
                {
                    iShipment.TotalFee = 0;
                }
                lstShipment.Add(iShipment);
            }

            if (_srvCode == "1")
            {
                servCode = "Chuyển phát";
            }
            else if (_srvCode == "2")
            {
                servCode = "Thanh toán - Chuyển phát";
            }
            //else if (_srvCode == "3")
            //{
            //    servCode = "Chuyển phát - Thanh toán";
            //}
            else
            {
                servCode = "Tất cả";
            }

            if (!string.IsNullOrEmpty(_status))
            {
                dynamic[] _lst;
                _lst = PayID.Portal.Configuration.Data_S24.List("Status", Query.EQ("StatusCode", _status));
                if (_lst != null && _lst.Length > 0)
                {
                    foreach (dynamic iteStatus in _lst)
                    {
                        statuscode = iteStatus.StatusDescription;
                    }
                }
            }
            if (!string.IsNullOrEmpty(_province))
            {
                dynamic[] _lst;
                _lst = PayID.Portal.Configuration.Data_S24.List("mbcProvince", Query.EQ("ProvinceCode", _province));
                if (_lst != null && _lst.Length > 0)
                {
                    foreach (dynamic it in _lst)
                    {
                        provincename = it.ProvinceCode + "-" + it.ProvinceName;
                    }
                }
            }
            if (!string.IsNullOrEmpty(_district))
            {
                dynamic[] _lst;
                _lst = PayID.Portal.Configuration.Data_S24.List("mbcDistrict", Query.EQ("DistrictCode", _district));
                if (_lst != null && _lst.Length > 0)
                {
                    foreach (dynamic it in _lst)
                    {
                        districtname = it.DistrictCode + "-" + it.DistrictName;
                    }
                }
            }
            if (!string.IsNullOrEmpty(_pos))
            {
                dynamic[] _lst;
                _lst = PayID.Portal.Configuration.Data_S24.List("mbcPos", Query.EQ("POSCode", _pos));
                if (_lst != null && _lst.Length > 0)
                {
                    foreach (dynamic it in _lst)
                    {
                        posname = it.POSCode + "-" + it.POSName;
                    }
                }
            }
            ReportDataSource rd = new ReportDataSource("Shipment", lstShipment);
            lr.DataSources.Add(rd);
            string reportType = id;
            string mimeType;
            string encoding;
            string fileNameExtension;
            ReportParameter[] paras = new ReportParameter[9];
            //dynamic dyna = new DynamicObj();
            //if (Session["profile"] != null)
            //{
            //    dyna = (dynamic)Session["profile"];
            //}
            string fullname = "";

            //if (dyna.full_name != null)
            //{
            //    fullname = dyna.full_name;
            //}
            //get info over business code
            dynamic dynaBusi = new DynamicObj();
            string agent = "Tất cả";

            if (!string.IsNullOrEmpty(_busscode))
            {
                IMongoQuery img = Query.Or(
                    Query.EQ("_id", _busscode),
                    Query.EQ("general_full_name", _busscode)
                    );
                dynaBusi = PayID.Portal.Configuration.Data_S24.Get("business_profile", img);
                if (dynaBusi != null)
                {
                    agent = _busscode;
                    fullname = "- " + dynaBusi.general_full_name;
                }
            }

            paras[0] = new ReportParameter("fromdate", string.IsNullOrEmpty(_fromdate) ? "" : DateTime.Parse(_fromdate).ToString("dd/MM/yyyy"), false);
            paras[1] = new ReportParameter("todate", string.IsNullOrEmpty(_todate) ? "" : DateTime.Parse(_todate).ToString("dd/MM/yyyy"), false);
            paras[2] = new ReportParameter("status", string.IsNullOrEmpty(statuscode) ? "Tất cả" : statuscode, false);
            paras[3] = new ReportParameter("usercreated", string.IsNullOrEmpty(fullname) ? " " : fullname, false);
            paras[4] = new ReportParameter("agent", agent, false);
            paras[5] = new ReportParameter("servicetype", string.IsNullOrEmpty(servCode) ? "Tất cả" : servCode, false);
            paras[6] = new ReportParameter("provincename", provincename, false);
            paras[7] = new ReportParameter("districtname", districtname, false);
            paras[8] = new ReportParameter("posname", posname, false);
            lr.SetParameters(paras);

            string deviceInfo =

            "<DeviceInfo>" +
            "  <OutputFormat>" + id + "</OutputFormat>" +
            "  <PageWidth>16in</PageWidth>" +
            "  <PageHeight>12in</PageHeight>" +
            "  <MarginTop>0.5in</MarginTop>" +
            "  <MarginLeft>1in</MarginLeft>" +
            "  <MarginRight>1in</MarginRight>" +
            "  <MarginBottom>0.5in</MarginBottom>" +
            "</DeviceInfo>";

            Warning[] warnings;
            string[] streams;
            byte[] renderedBytes;

            renderedBytes = lr.Render(
                reportType,
                deviceInfo,
                out mimeType,
                out encoding,
                out fileNameExtension,
                out streams,
                out warnings
                );
            if (id == "PDF")
                return File(renderedBytes, mimeType, "Thống_kê_vận_đơn_theo_tên_khách_hàng.pdf");
            else
                return File(renderedBytes, mimeType, "Thống_kê_vận_đơn_theo_tên_khách_hàng.xls");
        }
        public ActionResult ReportFormCollect(string id)
        {
            LocalReport lr = new LocalReport();

            string path = Path.Combine(Server.MapPath("~/Areas/Report/Templates/Shipment"), "FormCollectShipment.rdlc");
            if (System.IO.File.Exists(path))
            {
                lr.ReportPath = path;
            }
            else
            {
                return View("Index");
            }
            List<Models.Shipment> lstShipment = new List<Models.Shipment>();
            string statuscode = "", provincename = "", districtname = "", posname = "";

            List<dynamic> list_shipment = new List<dynamic>();

            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var myShipment = new
                    {

                        current_assigned = _current_assigned,
                        from_date = String.IsNullOrEmpty(_fromdate) ? "" : DateTime.Parse(_fromdate).ToString("dd/MM/yyyy"),
                        to_date = String.IsNullOrEmpty(_todate) ? "" : DateTime.Parse(_todate).ToString("dd/MM/yyyy"),
                        status = _status,
                        busscode = _busscode,
                        district = _district,
                        pos = _pos,
                        province = _province,
                        FormType = "FormCollect"
                    };
                    try
                    {
                        var response = client.PostAsJsonAsync("api/Shipment", myShipment).Result;
                        list_shipment = response.Content.ReadAsAsync<IEnumerable<dynamic>>().Result.ToList();
                        //list_shipment = (from c in list_shipment orderby c.CREATEDDATE descending select c).ToList();
                    }
                    catch { }
                }
                catch
                {
                    //list_shipment = null;
                }
            }

            string servCode = "Tất cả";
            for (int i = 0; i < list_shipment.Count; i++)
            {
                var l = list_shipment[i];
                Models.Shipment iShipment = new Models.Shipment();
                iShipment.STT = i + 1;
                iShipment.CustomerName = l.customername;
                iShipment.TrackingCode = l.tracking_code;
                if (l.to_address_name != null && l.to_address_name != "")
                {
                    l.name = l.to_address_name + "\n";
                }
                if (l.to_address_address != null && l.to_address_address != "")
                {
                    l.address = l.to_address_address + "\n";
                }
                if (l.to_address_phone != null && l.to_address_phone != "")
                {
                    l.phone = l.to_address_phone + "\n";
                }
                if (l.ReceiverMobile != null && l.ReceiverMobile != "")
                {
                    l.mobile = l.ReceiverMobile + "\n";
                }
                iShipment.ReceiverInfo = l.name + l.address + l.phone + l.mobile;
                iShipment.ProductVal = l.value;
                if (l.productname != null && l.productname != "")
                {
                    l.pn = l.productname + "\n";
                }
                if (l.productdescription != null && l.to_address_address != "")
                {
                    l.pd = l.productdescription + "\n";
                }
                if (l.value != null && l.to_address_address != 0)
                {
                    l.v = l.value.ToString("N0") + " đ" + "\n";
                }
                if (l.weight != null && l.to_address_address != 0)
                {
                    l.w = l.weight.ToString("N0") + " Gr" + "\n";
                }
                iShipment.Productdes = l.pn + l.pd + l.v + l.w;
                iShipment.MainFee = l.MainFee;
                iShipment.ServiceFee = l.ServiceFee;
                iShipment.CodFee = l.CodFee;
                iShipment.TotalFee = l.CodFee + l.ServiceFee +l.MainFee;
                iShipment.Status = l.system_status;
                iShipment.ToProvince = l.to_province_name;
                iShipment.CustomerCode = l.customercode;
                iShipment.OrderId = l.order_id;
                iShipment.collectvalue = l.collectvalue;
                iShipment.Quantity = l.quantity;
                if (l.MainFee != null)
                {
                    iShipment.MainFee = decimal.Parse(string.IsNullOrEmpty(l.MainFee.ToString()) ? "0" : l.MainFee.ToString());
                }
                else
                {
                    iShipment.MainFee = 0;
                }

                if (l.CodFee != null)
                {
                    iShipment.CodFee = decimal.Parse(string.IsNullOrEmpty(l.CodFee.ToString()) ? "0" : l.CodFee.ToString());
                }
                else
                {
                    iShipment.CodFee = 0;
                }

                if (l.ServiceFee != null)
                {
                    iShipment.ServiceFee = decimal.Parse(string.IsNullOrEmpty(l.ServiceFee.ToString()) ? "0" : l.ServiceFee.ToString());
                }
                else
                {
                    iShipment.ServiceFee = 0;
                }
                if (l.TotalFee != null)
                {
                    iShipment.TotalFee = decimal.Parse(string.IsNullOrEmpty(l.TotalFee.ToString()) ? "0" : l.TotalFee.ToString());
                }
                else
                {
                    iShipment.TotalFee = 0;
                }
                lstShipment.Add(iShipment);
            }

            if (_srvCode == "1")
            {
                servCode = "Chuyển phát";
            }
            else if (_srvCode == "2")
            {
                servCode = "Thanh toán - Chuyển phát";
            }
            //else if (_srvCode == "3")
            //{
            //    servCode = "Chuyển phát - Thanh toán";
            //}
            else
            {
                servCode = "Tất cả";
            }

            if (!string.IsNullOrEmpty(_status))
            {
                dynamic[] _lst;
                _lst = PayID.Portal.Configuration.Data_S24.List("Status", Query.EQ("StatusCode", _status));
                if (_lst != null && _lst.Length > 0)
                {
                    foreach (dynamic iteStatus in _lst)
                    {
                        statuscode = iteStatus.StatusDescription;
                    }
                }
            }
            if (!string.IsNullOrEmpty(_province))
            {
                dynamic[] _lst;
                _lst = PayID.Portal.Configuration.Data_S24.List("mbcProvince", Query.EQ("ProvinceCode", _province));
                if (_lst != null && _lst.Length > 0)
                {
                    foreach (dynamic it in _lst)
                    {
                        provincename = it.ProvinceCode + "-" + it.ProvinceName;
                    }
                }
            }
            if (!string.IsNullOrEmpty(_district))
            {
                dynamic[] _lst;
                _lst = PayID.Portal.Configuration.Data_S24.List("mbcDistrict", Query.EQ("DistrictCode", _district));
                if (_lst != null && _lst.Length > 0)
                {
                    foreach (dynamic it in _lst)
                    {
                        districtname = it.DistrictCode + "-" + it.DistrictName;
                    }
                }
            }
            if (!string.IsNullOrEmpty(_pos))
            {
                dynamic[] _lst;
                _lst = PayID.Portal.Configuration.Data_S24.List("mbcPos", Query.EQ("POSCode", _pos));
                if (_lst != null && _lst.Length > 0)
                {
                    foreach (dynamic it in _lst)
                    {
                        posname = it.POSCode + "-" + it.POSName;
                    }
                }
            }
            ReportDataSource rd = new ReportDataSource("Shipment", lstShipment);
            lr.DataSources.Add(rd);
            string reportType = id;
            string mimeType;
            string encoding;
            string fileNameExtension;
            ReportParameter[] paras = new ReportParameter[9];
            dynamic dyna = new DynamicObj();
            if (Session["profile"] != null)
            {
                dyna = (dynamic)Session["profile"];
            }
            string fullname = "";

            if (dyna.full_name != null)
            {
                fullname = dyna.full_name;
            }
            //get info over business code
            dynamic dynaBusi = new DynamicObj();
            string agent = "Tất cả";

            if (!string.IsNullOrEmpty(_busscode))
            {
                IMongoQuery img = Query.Or(
                    Query.EQ("_id", _busscode),
                    Query.EQ("general_full_name", _busscode)
                    );
                dynaBusi = PayID.Portal.Configuration.Data_S24.Get("business_profile", img);
                if (dynaBusi != null)
                {
                    agent = _busscode + "/" + dynaBusi.general_full_name;
                }
            }

            paras[0] = new ReportParameter("fromdate", string.IsNullOrEmpty(_fromdate) ? "" : DateTime.Parse(_fromdate).ToString("dd/MM/yyyy"), false);
            paras[1] = new ReportParameter("todate", string.IsNullOrEmpty(_todate) ? "" : DateTime.Parse(_todate).ToString("dd/MM/yyyy"), false);
            paras[2] = new ReportParameter("status", string.IsNullOrEmpty(statuscode) ? "Tất cả" : statuscode, false);
            paras[3] = new ReportParameter("usercreated", string.IsNullOrEmpty(fullname) ? "admin" : fullname, false);
            paras[4] = new ReportParameter("agent", agent, false);
            paras[5] = new ReportParameter("servicetype", string.IsNullOrEmpty(servCode) ? "Tất cả" : servCode, false);
            paras[6] = new ReportParameter("provincename", provincename, false);
            paras[7] = new ReportParameter("districtname", districtname, false);
            paras[8] = new ReportParameter("posname", posname, false);
            lr.SetParameters(paras);
            string deviceInfo =

            "<DeviceInfo>" +
            "  <OutputFormat>" + id + "</OutputFormat>" +
            "  <PageWidth>13in</PageWidth>" +
            "  <PageHeight>11in</PageHeight>" +
            "  <MarginTop>0.5in</MarginTop>" +
            "  <MarginLeft>1in</MarginLeft>" +
            "  <MarginRight>1in</MarginRight>" +
            "  <MarginBottom>0.5in</MarginBottom>" +
            "</DeviceInfo>";

            Warning[] warnings;
            string[] streams;
            byte[] renderedBytes;

            renderedBytes = lr.Render(
                reportType,
                deviceInfo,
                out mimeType,
                out encoding,
                out fileNameExtension,
                out streams,
                out warnings
                );
            if (id == "PDF")
                return File(renderedBytes, mimeType, "Thống_kê_vận_đơn_theo_các_đơn_hàng_điều_tin_thu_gom.pdf");
            else
                return File(renderedBytes, mimeType, "Thống_kê_vận_đơn_theo_các_đơn_hàng_điều_tin_thu_gom.xls");
        }
        public ActionResult ReportFormIssue(string id)
        {
            LocalReport lr = new LocalReport();

            string path = Path.Combine(Server.MapPath("~/Areas/Report/Templates/Shipment"), "FormIssueShipment.rdlc");
            if (System.IO.File.Exists(path))
            {
                lr.ReportPath = path;
            }
            else
            {
                return View("Index");
            }
            List<Models.Shipment> lstShipment = new List<Models.Shipment>();


            string statuscode = "", provincename = "", districtname = "", posname = "";

            List<dynamic> list_shipment = new List<dynamic>();

            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var myShipment = new
                    {

                        current_assigned = _current_assigned,
                        from_date = String.IsNullOrEmpty(_fromdate) ? "" : DateTime.Parse(_fromdate).ToString("dd/MM/yyyy"),
                        to_date = String.IsNullOrEmpty(_todate) ? "" : DateTime.Parse(_todate).ToString("dd/MM/yyyy"),
                        status = _status,
                        busscode = _busscode,
                        district = _district,
                        pos = _pos,
                        province = _province,
                        FormType = "FormIssue"
                    };
                    try
                    {
                        var response = client.PostAsJsonAsync("api/Shipment", myShipment).Result;
                        list_shipment = response.Content.ReadAsAsync<IEnumerable<dynamic>>().Result.ToList();
                        //list_shipment = (from c in list_shipment orderby c.CREATEDDATE descending select c).ToList();
                    }
                    catch { }
                }
                catch
                {
                    //list_shipment = null;
                }
            }

            string servCode = "Tất cả";
            for (int i = 0; i < list_shipment.Count; i++)
            {
                var l = list_shipment[i];
                Models.Shipment iShipment = new Models.Shipment();
                iShipment.STT = i + 1;
                iShipment.CustomerName = l.customername;
                iShipment.TrackingCode = l.tracking_code;
                if (l.to_address_name != null && l.to_address_name != "")
                {
                    l.name = l.to_address_name + "\n";
                }
                if (l.to_address_address != null && l.to_address_address != "")
                {
                    l.address = l.to_address_address + "\n";
                }
                if (l.to_address_phone != null && l.to_address_phone != "")
                {
                    l.phone = l.to_address_phone + "\n";
                }
                if (l.to_address_mobile != null && l.to_address_mobile != "")
                {
                    l.mobile = l.to_address_mobile + "\n";
                }
                iShipment.ReceiverInfo = l.name + l.address + l.phone + l.mobile;
                iShipment.ProductVal = l.value;
                if (l.productname != null && l.productname != "")
                {
                    l.pn = l.productname + "\n";
                }
                if (l.productdescription != null && l.to_address_address != "")
                {
                    l.pd = l.productdescription + "\n";
                }
                if (l.value != null && l.to_address_address != 0)
                {
                    l.v = l.value.ToString("N0") + " đ" + "\n";
                }
                if (l.weight != null && l.to_address_address != 0)
                {
                    l.w = l.weight.ToString("N0") + " Gr" + "\n";
                }
                iShipment.Productdes = l.pn + l.pd + l.v + l.w;
                iShipment.MainFee = l.MainFee;
                iShipment.ServiceFee = l.ServiceFee;
                iShipment.CodFee = l.CodFee;
                iShipment.TotalFee = l.TotalFee;
                iShipment.Status = l.system_status;
                iShipment.ToProvince = l.to_province_name;
                iShipment.CustomerCode = l.customercode;
                iShipment.OrderId = l.order_id;
                iShipment.collectvalue = l.collectvalue;
                iShipment.Quantity = l.quantity;
                iShipment.Weight = l.weight;
                if (l.MainFee != null)
                {
                    iShipment.MainFee = decimal.Parse(string.IsNullOrEmpty(l.MainFee.ToString()) ? "0" : l.MainFee.ToString());
                }
                else
                {
                    iShipment.MainFee = 0;
                }

                if (l.CodFee != null)
                {
                    iShipment.CodFee = decimal.Parse(string.IsNullOrEmpty(l.CodFee.ToString()) ? "0" : l.CodFee.ToString());
                }
                else
                {
                    iShipment.CodFee = 0;
                }

                if (l.ServiceFee != null)
                {
                    iShipment.ServiceFee = decimal.Parse(string.IsNullOrEmpty(l.ServiceFee.ToString()) ? "0" : l.ServiceFee.ToString());
                }
                else
                {
                    iShipment.ServiceFee = 0;
                }
                if (l.TotalFee != null)
                {
                    iShipment.TotalFee = decimal.Parse(string.IsNullOrEmpty(l.TotalFee.ToString()) ? "0" : l.TotalFee.ToString());
                }
                else
                {
                    iShipment.TotalFee = 0;
                }
                lstShipment.Add(iShipment);
            }

            if (_srvCode == "1")
            {
                servCode = "Chuyển phát";
            }
            else if (_srvCode == "2")
            {
                servCode = "Thanh toán - Chuyển phát";
            }
            //else if (_srvCode == "3")
            //{
            //    servCode = "Chuyển phát - Thanh toán";
            //}
            else
            {
                servCode = "Tất cả";
            }

            if (!string.IsNullOrEmpty(_status))
            {
                dynamic[] _lst;
                _lst = PayID.Portal.Configuration.Data_S24.List("Status", Query.EQ("StatusCode", _status));
                if (_lst != null && _lst.Length > 0)
                {
                    foreach (dynamic iteStatus in _lst)
                    {
                        statuscode = iteStatus.StatusDescription;
                    }
                }
            }
            if (!string.IsNullOrEmpty(_province))
            {
                dynamic[] _lst;
                _lst = PayID.Portal.Configuration.Data_S24.List("mbcProvince", Query.EQ("ProvinceCode", _province));
                if (_lst != null && _lst.Length > 0)
                {
                    foreach (dynamic it in _lst)
                    {
                        provincename = it.ProvinceCode + "-" + it.ProvinceName;
                    }
                }
            }
            if (!string.IsNullOrEmpty(_district))
            {
                dynamic[] _lst;
                _lst = PayID.Portal.Configuration.Data_S24.List("mbcDistrict", Query.EQ("DistrictCode", _district));
                if (_lst != null && _lst.Length > 0)
                {
                    foreach (dynamic it in _lst)
                    {
                        posname = it.DistrictCode + "-" + it.DistrictName;
                    }
                }
            }
            if (!string.IsNullOrEmpty(_pos))
            {
                dynamic[] _lst;
                _lst = PayID.Portal.Configuration.Data_S24.List("mbcPos", Query.EQ("POSCode", _pos));
                if (_lst != null && _lst.Length > 0)
                {
                    foreach (dynamic it in _lst)
                    {
                        districtname = it.POSCode + "-" + it.POSName;
                    }
                }
            }
            ReportDataSource rd = new ReportDataSource("Shipment", lstShipment);
            lr.DataSources.Add(rd);
            string reportType = id;
            string mimeType;
            string encoding;
            string fileNameExtension;
            ReportParameter[] paras = new ReportParameter[9];
            dynamic dyna = new DynamicObj();
            if (Session["profile"] != null)
            {
                dyna = (dynamic)Session["profile"];
            }
            string fullname = "";

            if (dyna.full_name != null)
            {
                fullname = dyna.full_name;
            }
            //get info over business code
            dynamic dynaBusi = new DynamicObj();
            string agent = "Tất cả";

            if (!string.IsNullOrEmpty(_busscode))
            {
                IMongoQuery img = Query.Or(
                    Query.EQ("_id", _busscode),
                    Query.EQ("general_full_name", _busscode)
                    );
                dynaBusi = PayID.Portal.Configuration.Data_S24.Get("business_profile", img);
                if (dynaBusi != null)
                {
                    agent = _busscode + "/" + dynaBusi.general_full_name;
                }
            }

            paras[0] = new ReportParameter("fromdate", string.IsNullOrEmpty(_fromdate) ? "" : DateTime.Parse(_fromdate).ToString("dd/MM/yyyy"), false);
            paras[1] = new ReportParameter("todate", string.IsNullOrEmpty(_todate) ? "" : DateTime.Parse(_todate).ToString("dd/MM/yyyy"), false);
            paras[2] = new ReportParameter("status", string.IsNullOrEmpty(statuscode) ? "Tất cả" : statuscode, false);
            paras[3] = new ReportParameter("usercreated", string.IsNullOrEmpty(fullname) ? "admin" : fullname, false);
            paras[4] = new ReportParameter("agent", agent, false);
            paras[5] = new ReportParameter("servicetype", string.IsNullOrEmpty(servCode) ? "Tất cả" : servCode, false);
            paras[6] = new ReportParameter("provincename", provincename, false);
            paras[7] = new ReportParameter("districtname", districtname, false);
            paras[8] = new ReportParameter("posname", posname, false);
            lr.SetParameters(paras);

            string deviceInfo =

            "<DeviceInfo>" +
            "  <OutputFormat>" + id + "</OutputFormat>" +
            "  <PageWidth>15in</PageWidth>" +
            "  <PageHeight>12in</PageHeight>" +
            "  <MarginTop>0.5in</MarginTop>" +
            "  <MarginLeft>1in</MarginLeft>" +
            "  <MarginRight>1in</MarginRight>" +
            "  <MarginBottom>0.5in</MarginBottom>" +
            "</DeviceInfo>";

            Warning[] warnings;
            string[] streams;
            byte[] renderedBytes;

            renderedBytes = lr.Render(
                reportType,
                deviceInfo,
                out mimeType,
                out encoding,
                out fileNameExtension,
                out streams,
                out warnings
                );
            if (id == "PDF")
                return File(renderedBytes, mimeType, "Thống_kê_vận_đơn_theo_các_đơn_hàng_đã_được_phát_hành.pdf");
            else
                return File(renderedBytes, mimeType, "Thống_kê_vận_đơn_theo_các_đơn_hàng_đã_được_phát_hành.xls");
        }
        public ActionResult ReportFormNameTH(string id)
        {
            LocalReport lr = new LocalReport();

            string path = Path.Combine(Server.MapPath("~/Areas/Report/Templates/Shipment"), "FormNameTHShipment.rdlc");
            if (System.IO.File.Exists(path))
            {
                lr.ReportPath = path;
            }
            else
            {
                return View("Index");
            }
            List<Models.Shipment> lstShipment = new List<Models.Shipment>();

            string statuscode = "", provincename = "", districtname = "", posname = "";

            List<dynamic> list_shipment = new List<dynamic>();

            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var myShipment = new
                    {

                        current_assigned = _current_assigned,
                        from_date = String.IsNullOrEmpty(_fromdate) ? "" : DateTime.Parse(_fromdate).ToString("dd/MM/yyyy"),
                        to_date = String.IsNullOrEmpty(_todate) ? "" : DateTime.Parse(_todate).ToString("dd/MM/yyyy"),
                        status = _status,
                        busscode = _busscode,
                        district = _district,
                        pos = _pos,
                        province = _province,
                        FormType = "FormName"
                    };
                    try
                    {
                        var response = client.PostAsJsonAsync("api/Shipment", myShipment).Result;
                        list_shipment = response.Content.ReadAsAsync<IEnumerable<dynamic>>().Result.ToList();
                        //list_shipment = (from c in list_shipment orderby c.CREATEDDATE descending select c).ToList();
                    }
                    catch { }
                }
                catch
                {
                    //list_shipment = null;
                }
            }
            if (!string.IsNullOrEmpty(_province))
            {
                if (!string.IsNullOrEmpty(_district))
                {
                    list_shipment = (from c in list_shipment
                                     where c.AssignDistrictName != "" && c.AssignDistrict != "" && c.AssignProvince == _province && c.AssignDistrict == _district
                                     select c).ToList<dynamic>();
                }
                else
                {
                    list_shipment = (from c in list_shipment
                                     where c.AssignDistrictName != "" && c.AssignDistrict != "" && c.AssignProvince == _province
                                     select c).ToList<dynamic>();
                }
            }
            else
            {
                list_shipment = (from c in list_shipment
                                 where c.AssignProvince != ""
                                 select c).ToList<dynamic>();
            }
            string servCode = "Tất cả";

            for (int i = 0; i < list_shipment.Count; i++)
            {
                var l = list_shipment[i];
                Models.Shipment iShipment = new Models.Shipment();
                iShipment.STT = i + 1;
                iShipment.CustomerName = l.customername;
                iShipment.TrackingCode = l.tracking_code;
                if (l.to_address_name != null && l.to_address_name != "")
                {
                    l.name = l.to_address_name + "\n";
                }
                if (l.to_address_address != null && l.to_address_address != "")
                {
                    l.address = l.to_address_address + "\n";
                }
                if (l.to_address_phone != null && l.to_address_phone != "")
                {
                    l.phone = l.to_address_phone + "\n";
                }
                if (l.to_address_mobile != null && l.to_address_mobile != "")
                {
                    l.mobile = l.to_address_mobile + "\n";
                }
                iShipment.ReceiverInfo = l.name + l.address + l.phone + l.mobile;
                iShipment.ProductVal = l.value;
                if (l.productname != null && l.productname != "")
                {
                    l.pn = l.productname + "\n";
                }
                if (l.productdescription != null && l.to_address_address != "")
                {
                    l.pd = l.productdescription + "\n";
                }
                if (l.value != null && l.to_address_address != 0)
                {
                    l.v = l.value.ToString("N0") + " đ" + "\n";
                }
                if (l.weight != null && l.to_address_address != 0)
                {
                    l.w = l.weight.ToString("N0") + " Gr" + "\n";
                }
                iShipment.Productdes = l.pn + l.pd + l.v + l.w;
                iShipment.MainFee = l.MainFee;
                iShipment.ServiceFee = l.ServiceFee;
                iShipment.CodFee = l.CodFee;
                iShipment.TotalFee = l.TotalFee;
                iShipment.Status = l.system_status;
                iShipment.ToProvince = l.to_province_name;
                iShipment.CustomerCode = l.customercode;
                iShipment.OrderId = l.order_id;
                iShipment.collectvalue = l.collectvalue;
                iShipment.Weight = l.weight;
                iShipment.Quantity = l.quantity;
                if (!string.IsNullOrEmpty(_province))
                {
                    if (!string.IsNullOrEmpty(_district))
                    {
                        iShipment.province = l.AssignPoName;
                    }
                    else
                    {
                        iShipment.province = l.AssignDistrictName;
                    }

                }
                else
                {
                    iShipment.province = l.AssignProvinceName;
                }

                if (l.MainFee != null)
                {
                    iShipment.MainFee = decimal.Parse(string.IsNullOrEmpty(l.MainFee.ToString()) ? "0" : l.MainFee.ToString());
                }
                else
                {
                    iShipment.MainFee = 0;
                }

                if (l.CodFee != null)
                {
                    iShipment.CodFee = decimal.Parse(string.IsNullOrEmpty(l.CodFee.ToString()) ? "0" : l.CodFee.ToString());
                }
                else
                {
                    iShipment.CodFee = 0;
                }

                if (l.ServiceFee != null)
                {
                    iShipment.ServiceFee = decimal.Parse(string.IsNullOrEmpty(l.ServiceFee.ToString()) ? "0" : l.ServiceFee.ToString());
                }
                else
                {
                    iShipment.ServiceFee = 0;
                }
                if (l.TotalFee != null)
                {
                    iShipment.TotalFee = decimal.Parse(string.IsNullOrEmpty(l.TotalFee.ToString()) ? "0" : l.TotalFee.ToString());
                }
                else
                {
                    iShipment.TotalFee = 0;
                }
                lstShipment.Add(iShipment);
            }

            if (_srvCode == "1")
            {
                servCode = "Chuyển phát";
            }
            else if (_srvCode == "2")
            {
                servCode = "Thanh toán - Chuyển phát";
            }
            else
            {
                servCode = "Tất cả";
            }

            if (!string.IsNullOrEmpty(_status))
            {
                dynamic[] _lst;
                _lst = PayID.Portal.Configuration.Data_S24.List("Status", Query.EQ("StatusCode", _status));
                if (_lst != null && _lst.Length > 0)
                {
                    foreach (dynamic iteStatus in _lst)
                    {
                        statuscode = iteStatus.StatusDescription;
                    }
                }
            }
            if (!string.IsNullOrEmpty(_province))
            {
                dynamic[] _lst;
                _lst = PayID.Portal.Configuration.Data_S24.List("mbcProvince", Query.EQ("ProvinceCode", _province));
                if (_lst != null && _lst.Length > 0)
                {
                    foreach (dynamic it in _lst)
                    {
                        provincename = it.ProvinceCode + "-" + it.ProvinceName;
                    }
                }
            }
            if (!string.IsNullOrEmpty(_district))
            {
                dynamic[] _lst;
                _lst = PayID.Portal.Configuration.Data_S24.List("mbcDistrict", Query.EQ("DistrictCode", _district));
                if (_lst != null && _lst.Length > 0)
                {
                    foreach (dynamic it in _lst)
                    {
                        districtname = it.DistrictCode + "-" + it.DistrictName;
                    }
                }
            }
            if (!string.IsNullOrEmpty(_pos))
            {
                dynamic[] _lst;
                _lst = PayID.Portal.Configuration.Data_S24.List("mbcPos", Query.EQ("POSCode", _pos));
                if (_lst != null && _lst.Length > 0)
                {
                    foreach (dynamic it in _lst)
                    {
                        posname = it.POSCode + "-" + it.POSName;
                    }
                }
            }
            ReportDataSource rd = new ReportDataSource("Shipment", lstShipment);
            lr.DataSources.Add(rd);
            string reportType = id;
            string mimeType;
            string encoding;
            string fileNameExtension;
            ReportParameter[] paras = new ReportParameter[9];
            //dynamic dyna = new DynamicObj();
            //if (Session["profile"] != null)
            //{
            //    dyna = (dynamic)Session["profile"];
            //}
            string fullname = "";

            //if (dyna.full_name != null)
            //{
            //    fullname = dyna.full_name;
            //}
            //get info over business code
            dynamic dynaBusi = new DynamicObj();
            string agent = "Tất cả";

            if (!string.IsNullOrEmpty(_busscode))
            {
                IMongoQuery img = Query.Or(
                    Query.EQ("_id", _busscode),
                    Query.EQ("general_full_name", _busscode)
                    );
                dynaBusi = PayID.Portal.Configuration.Data_S24.Get("business_profile", img);
                if (dynaBusi != null)
                {
                    agent = _busscode;
                    fullname = dynaBusi.general_full_name;
                }
            }

            paras[0] = new ReportParameter("fromdate", string.IsNullOrEmpty(_fromdate) ? "" : DateTime.Parse(_fromdate).ToString("dd/MM/yyyy"), false);
            paras[1] = new ReportParameter("todate", string.IsNullOrEmpty(_todate) ? "" : DateTime.Parse(_todate).ToString("dd/MM/yyyy"), false);
            paras[2] = new ReportParameter("status", string.IsNullOrEmpty(statuscode) ? "Tất cả" : statuscode, false);
            paras[3] = new ReportParameter("usercreated", string.IsNullOrEmpty(fullname) ? "admin" : fullname, false);
            paras[4] = new ReportParameter("agent", agent, false);
            paras[5] = new ReportParameter("servicetype", string.IsNullOrEmpty(servCode) ? "Tất cả" : servCode, false);
            paras[6] = new ReportParameter("provincename", provincename, false);
            paras[7] = new ReportParameter("districtname", districtname, false);
            paras[8] = new ReportParameter("posname", posname, false);
            lr.SetParameters(paras);

            string deviceInfo =

            "<DeviceInfo>" +
            "  <OutputFormat>" + id + "</OutputFormat>" +
            "  <PageWidth>14in</PageWidth>" +
            "  <PageHeight>12in</PageHeight>" +
            "  <MarginTop>0.5in</MarginTop>" +
            "  <MarginLeft>1in</MarginLeft>" +
            "  <MarginRight>1in</MarginRight>" +
            "  <MarginBottom>0.5in</MarginBottom>" +
            "</DeviceInfo>";

            Warning[] warnings;
            string[] streams;
            byte[] renderedBytes;

            renderedBytes = lr.Render(
                reportType,
                deviceInfo,
                out mimeType,
                out encoding,
                out fileNameExtension,
                out streams,
                out warnings
                );
            if (id == "PDF")
                return File(renderedBytes, mimeType, "Thống_kê_vận_đơn_theo_tên_khách_hàngTH.pdf");
            else
                return File(renderedBytes, mimeType, "Thống_kê_vận_đơn_theo_tên_khách_hàngTH.xls");
        }
        public ActionResult ReportFormCollectTH(string id)
        {
            LocalReport lr = new LocalReport();

            string path = Path.Combine(Server.MapPath("~/Areas/Report/Templates/Shipment"), "FormCollectTHShipment.rdlc");
            if (System.IO.File.Exists(path))
            {
                lr.ReportPath = path;
            }
            else
            {
                return View("Index");
            }
            List<Models.Shipment> lstShipment = new List<Models.Shipment>();

            string statuscode = "", provincename = "", districtname = "", posname = "";

            List<dynamic> list_shipment = new List<dynamic>();

            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var myShipment = new
                    {

                        current_assigned = _current_assigned,
                        from_date = String.IsNullOrEmpty(_fromdate) ? "" : DateTime.Parse(_fromdate).ToString("dd/MM/yyyy"),
                        to_date = String.IsNullOrEmpty(_todate) ? "" : DateTime.Parse(_todate).ToString("dd/MM/yyyy"),
                        status = _status,
                        busscode = _busscode,
                        district = _district,
                        pos = _pos,
                        province = _province,
                        FormType = "FormCollect"
                    };
                    try
                    {
                        var response = client.PostAsJsonAsync("api/Shipment", myShipment).Result;
                        list_shipment = response.Content.ReadAsAsync<IEnumerable<dynamic>>().Result.ToList();
                        //list_shipment = (from c in list_shipment orderby c.CREATEDDATE descending select c).ToList();
                    }
                    catch { }
                }
                catch
                {
                    //list_shipment = null;
                }
            }
            if (!string.IsNullOrEmpty(_province))
            {
                if (!string.IsNullOrEmpty(_district))
                {
                    list_shipment = (from c in list_shipment
                                     where c.AssignDistrictName != "" && c.AssignDistrict != "" && c.AssignProvince == _province && c.AssignDistrict == _district
                                     select c).ToList<dynamic>();
                }
                else
                {
                    list_shipment = (from c in list_shipment
                                     where c.AssignDistrictName != "" && c.AssignDistrict != "" && c.AssignProvince == _province
                                     select c).ToList<dynamic>();
                }
            }
            else
            {
                list_shipment = (from c in list_shipment
                                 where c.AssignProvince != ""
                                 select c).ToList<dynamic>();
            }
            string servCode = "Tất cả";

            for (int i = 0; i < list_shipment.Count; i++)
            {
                var l = list_shipment[i];
                Models.Shipment iShipment = new Models.Shipment();
                iShipment.STT = i + 1;
                iShipment.CustomerName = l.customername;
                iShipment.TrackingCode = l.tracking_code;
                if (l.to_address_name != null && l.to_address_name != "")
                {
                    l.name = l.to_address_name + "\n";
                }
                if (l.to_address_address != null && l.to_address_address != "")
                {
                    l.address = l.to_address_address + "\n";
                }
                if (l.to_address_phone != null && l.to_address_phone != "")
                {
                    l.phone = l.to_address_phone + "\n";
                }
                if (l.to_address_mobile != null && l.to_address_mobile != "")
                {
                    l.mobile = l.to_address_mobile + "\n";
                }
                iShipment.ReceiverInfo = l.name + l.address + l.phone + l.mobile;
                iShipment.ProductVal = l.value;
                if (l.productname != null && l.productname != "")
                {
                    l.pn = l.productname + "\n";
                }
                if (l.productdescription != null && l.to_address_address != "")
                {
                    l.pd = l.productdescription + "\n";
                }
                if (l.value != null && l.to_address_address != 0)
                {
                    l.v = l.value.ToString("N0") + " đ" + "\n";
                }
                if (l.weight != null && l.to_address_address != 0)
                {
                    l.w = l.weight.ToString("N0") + " Gr" + "\n";
                }
                iShipment.Productdes = l.pn + l.pd + l.v + l.w;
                iShipment.Quantity = l.quantity;
                iShipment.MainFee = l.MainFee;
                iShipment.ServiceFee = l.ServiceFee;
                iShipment.CodFee = l.CodFee;
                iShipment.TotalFee = l.CodFee + l.ServiceFee + l.MainFee;
                iShipment.Status = l.system_status;
                iShipment.ToProvince = l.to_province_name;
                iShipment.CustomerCode = l.customercode;
                iShipment.OrderId = l.order_id;
                iShipment.collectvalue = l.collectvalue;
                iShipment.Weight = l.weight;
                if (!string.IsNullOrEmpty(_province))
                {
                    if (!string.IsNullOrEmpty(_district))
                    {
                        iShipment.province = l.AssignPoName;
                    }
                    else
                    {
                        iShipment.province = l.AssignDistrictName;
                    }

                }
                else
                {
                    iShipment.province = l.AssignProvinceName;
                }

                if (l.MainFee != null)
                {
                    iShipment.MainFee = decimal.Parse(string.IsNullOrEmpty(l.MainFee.ToString()) ? "0" : l.MainFee.ToString());
                }
                else
                {
                    iShipment.MainFee = 0;
                }

                if (l.CodFee != null)
                {
                    iShipment.CodFee = decimal.Parse(string.IsNullOrEmpty(l.CodFee.ToString()) ? "0" : l.CodFee.ToString());
                }
                else
                {
                    iShipment.CodFee = 0;
                }

                if (l.ServiceFee != null)
                {
                    iShipment.ServiceFee = decimal.Parse(string.IsNullOrEmpty(l.ServiceFee.ToString()) ? "0" : l.ServiceFee.ToString());
                }
                else
                {
                    iShipment.ServiceFee = 0;
                }
                if (l.TotalFee != null)
                {
                    iShipment.TotalFee = decimal.Parse(string.IsNullOrEmpty(l.TotalFee.ToString()) ? "0" : l.TotalFee.ToString());
                }
                else
                {
                    iShipment.TotalFee = 0;
                }
                lstShipment.Add(iShipment);
            }

            if (_srvCode == "1")
            {
                servCode = "Chuyển phát";
            }
            else if (_srvCode == "2")
            {
                servCode = "Thanh toán - Chuyển phát";
            }
            //else if (_srvCode == "3")
            //{
            //    servCode = "Chuyển phát - Thanh toán";
            //}
            else
            {
                servCode = "Tất cả";
            }

            if (!string.IsNullOrEmpty(_status))
            {
                dynamic[] _lst;
                _lst = PayID.Portal.Configuration.Data_S24.List("Status", Query.EQ("StatusCode", _status));
                if (_lst != null && _lst.Length > 0)
                {
                    foreach (dynamic iteStatus in _lst)
                    {
                        statuscode = iteStatus.StatusDescription;
                    }
                }
            }
            if (!string.IsNullOrEmpty(_province))
            {
                dynamic[] _lst;
                _lst = PayID.Portal.Configuration.Data_S24.List("mbcProvince", Query.EQ("ProvinceCode", _province));
                if (_lst != null && _lst.Length > 0)
                {
                    foreach (dynamic it in _lst)
                    {
                        provincename = it.ProvinceCode + "-" + it.ProvinceName;
                    }
                }
            }
            if (!string.IsNullOrEmpty(_district))
            {
                dynamic[] _lst;
                _lst = PayID.Portal.Configuration.Data_S24.List("mbcDistrict", Query.EQ("DistrictCode", _district));
                if (_lst != null && _lst.Length > 0)
                {
                    foreach (dynamic it in _lst)
                    {
                        districtname = it.DistrictCode + "-" + it.DistrictName;
                    }
                }
            }
            if (!string.IsNullOrEmpty(_pos))
            {
                dynamic[] _lst;
                _lst = PayID.Portal.Configuration.Data_S24.List("mbcPos", Query.EQ("POSCode", _pos));
                if (_lst != null && _lst.Length > 0)
                {
                    foreach (dynamic it in _lst)
                    {
                        posname = it.POSCode + "-" + it.POSName;
                    }
                }
            }
            ReportDataSource rd = new ReportDataSource("Shipment", lstShipment);
            lr.DataSources.Add(rd);
            string reportType = id;
            string mimeType;
            string encoding;
            string fileNameExtension;
            ReportParameter[] paras = new ReportParameter[9];
            dynamic dyna = new DynamicObj();
            if (Session["profile"] != null)
            {
                dyna = (dynamic)Session["profile"];
            }
            string fullname = "";

            if (dyna.full_name != null)
            {
                fullname = dyna.full_name;
            }
            //get info over business code
            dynamic dynaBusi = new DynamicObj();
            string agent = "Tất cả";

            if (!string.IsNullOrEmpty(_busscode))
            {
                IMongoQuery img = Query.Or(
                    Query.EQ("_id", _busscode),
                    Query.EQ("general_full_name", _busscode)
                    );
                dynaBusi = PayID.Portal.Configuration.Data_S24.Get("business_profile", img);
                if (dynaBusi != null)
                {
                    agent = _busscode + "/" + dynaBusi.general_full_name;
                }
            }

            paras[0] = new ReportParameter("fromdate", string.IsNullOrEmpty(_fromdate) ? "" : DateTime.Parse(_fromdate).ToString("dd/MM/yyyy"), false);
            paras[1] = new ReportParameter("todate", string.IsNullOrEmpty(_todate) ? "" : DateTime.Parse(_todate).ToString("dd/MM/yyyy"), false);
            paras[2] = new ReportParameter("status", string.IsNullOrEmpty(statuscode) ? "Tất cả" : statuscode, false);
            paras[3] = new ReportParameter("usercreated", string.IsNullOrEmpty(fullname) ? "admin" : fullname, false);
            paras[4] = new ReportParameter("agent", agent, false);
            paras[5] = new ReportParameter("servicetype", string.IsNullOrEmpty(servCode) ? "Tất cả" : servCode, false);
            paras[6] = new ReportParameter("provincename", provincename, false);
            paras[7] = new ReportParameter("districtname", districtname, false);
            paras[8] = new ReportParameter("posname", posname, false);
            lr.SetParameters(paras);

            string deviceInfo =

            "<DeviceInfo>" +
            "  <OutputFormat>" + id + "</OutputFormat>" +
            "  <PageWidth>14in</PageWidth>" +
            "  <PageHeight>12in</PageHeight>" +
            "  <MarginTop>0.5in</MarginTop>" +
            "  <MarginLeft>1in</MarginLeft>" +
            "  <MarginRight>1in</MarginRight>" +
            "  <MarginBottom>0.5in</MarginBottom>" +
            "</DeviceInfo>";

            Warning[] warnings;
            string[] streams;
            byte[] renderedBytes;

            renderedBytes = lr.Render(
                reportType,
                deviceInfo,
                out mimeType,
                out encoding,
                out fileNameExtension,
                out streams,
                out warnings
                );
            if (id == "PDF")
                return File(renderedBytes, mimeType, "Thống_kê_vận_đơn_theo_các_đơn_hàng_điều_tin_thu_gomTH.pdf");
            else
                return File(renderedBytes, mimeType, "Thống_kê_vận_đơn_theo_các_đơn_hàng_điều_tin_thu_gomTH.xls");
        }
        public ActionResult ReportFormIssueTH(string id)
        {
            LocalReport lr = new LocalReport();

            string path = Path.Combine(Server.MapPath("~/Areas/Report/Templates/Shipment"), "FormIssueTHShipment.rdlc");
            if (System.IO.File.Exists(path))
            {
                lr.ReportPath = path;
            }
            else
            {
                return View("Index");
            }
            List<Models.Shipment> lstShipment = new List<Models.Shipment>();

            string statuscode = "", provincename = "", districtname = "", posname = "";

            List<dynamic> list_shipment = new List<dynamic>();

            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var myShipment = new
                    {

                        current_assigned = _current_assigned,
                        from_date = String.IsNullOrEmpty(_fromdate) ? "" : DateTime.Parse(_fromdate).ToString("dd/MM/yyyy"),
                        to_date = String.IsNullOrEmpty(_todate) ? "" : DateTime.Parse(_todate).ToString("dd/MM/yyyy"),
                        status = _status,
                        busscode = _busscode,
                        district = _district,
                        pos = _pos,
                        province = _province,
                        FormType = "FormIssue"
                    };
                    try
                    {
                        var response = client.PostAsJsonAsync("api/Shipment", myShipment).Result;
                        list_shipment = response.Content.ReadAsAsync<IEnumerable<dynamic>>().Result.ToList();
                        //list_shipment = (from c in list_shipment orderby c.CREATEDDATE descending select c).ToList();
                    }
                    catch { }
                }
                catch
                {
                    //list_shipment = null;
                }
            }
            if (!string.IsNullOrEmpty(_province))
            {
                if (!string.IsNullOrEmpty(_district))
                {
                    list_shipment = (from c in list_shipment
                                     where c.AssignDistrictName != "" && c.AssignDistrict != "" && c.AssignProvince == _province && c.AssignDistrict == _district
                                     select c).ToList<dynamic>();
                }
                else
                {
                    list_shipment = (from c in list_shipment
                                     where c.AssignDistrictName != "" && c.AssignDistrict != "" && c.AssignProvince == _province
                                     select c).ToList<dynamic>();
                }
            }
            else
            {
                list_shipment = (from c in list_shipment
                                 where c.AssignProvince != ""
                                 select c).ToList<dynamic>();
            }
            string servCode = "Tất cả";

            for (int i = 0; i < list_shipment.Count; i++)
            {
                var l = list_shipment[i];
                Models.Shipment iShipment = new Models.Shipment();
                iShipment.STT = i + 1;
                iShipment.CustomerName = l.customername;
                iShipment.TrackingCode = l.tracking_code;
                if (l.to_address_name != null && l.to_address_name != "")
                {
                    l.name = l.to_address_name + "\n";
                }
                if (l.to_address_address != null && l.to_address_address != "")
                {
                    l.address = l.to_address_address + "\n";
                }
                if (l.to_address_phone != null && l.to_address_phone != "")
                {
                    l.phone = l.to_address_phone + "\n";
                }
                if (l.to_address_mobile != null && l.to_address_mobile != "")
                {
                    l.mobile = l.to_address_mobile + "\n";
                }
                iShipment.ReceiverInfo = l.name + l.address + l.phone + l.mobile;
                iShipment.ProductVal = l.value;
                if (l.productname != null && l.productname != "")
                {
                    l.pn = l.productname + "\n";
                }
                if (l.productdescription != null && l.to_address_address != "")
                {
                    l.pd = l.productdescription + "\n";
                }
                if (l.value != null && l.to_address_address != 0)
                {
                    l.v = l.value.ToString("N0") + " đ" + "\n";
                }
                if (l.weight != null && l.to_address_address != 0)
                {
                    l.w = l.weight.ToString("N0") + " Gr" + "\n";
                }
                iShipment.Productdes = l.pn + l.pd + l.v + l.w;
                iShipment.MainFee = l.MainFee;
                iShipment.ServiceFee = l.ServiceFee;
                iShipment.CodFee = l.CodFee;
                iShipment.TotalFee = l.CodFee + l.ServiceFee + l.MainFee;
                iShipment.Status = l.system_status;
                iShipment.ToProvince = l.to_province_name;
                iShipment.CustomerCode = l.customercode;
                iShipment.OrderId = l.order_id;
                iShipment.collectvalue = l.collectvalue;
                iShipment.Weight = l.weight;
                iShipment.Quantity = l.quantity;
                if (!string.IsNullOrEmpty(_province))
                {
                    if (!string.IsNullOrEmpty(_district))
                    {
                        iShipment.province = l.AssignPoName;
                    }
                    else
                    {
                        iShipment.province = l.AssignDistrictName;
                    }

                }
                else
                {
                    iShipment.province = l.AssignProvinceName;
                }

                if (l.MainFee != null)
                {
                    iShipment.MainFee = decimal.Parse(string.IsNullOrEmpty(l.MainFee.ToString()) ? "0" : l.MainFee.ToString());
                }
                else
                {
                    iShipment.MainFee = 0;
                }

                if (l.CodFee != null)
                {
                    iShipment.CodFee = decimal.Parse(string.IsNullOrEmpty(l.CodFee.ToString()) ? "0" : l.CodFee.ToString());
                }
                else
                {
                    iShipment.CodFee = 0;
                }

                if (l.ServiceFee != null)
                {
                    iShipment.ServiceFee = decimal.Parse(string.IsNullOrEmpty(l.ServiceFee.ToString()) ? "0" : l.ServiceFee.ToString());
                }
                else
                {
                    iShipment.ServiceFee = 0;
                }
                if (l.TotalFee != null)
                {
                    iShipment.TotalFee = decimal.Parse(string.IsNullOrEmpty(l.TotalFee.ToString()) ? "0" : l.TotalFee.ToString());
                }
                else
                {
                    iShipment.TotalFee = 0;
                }
                lstShipment.Add(iShipment);
            }

            if (_srvCode == "1")
            {
                servCode = "Chuyển phát";
            }
            else if (_srvCode == "2")
            {
                servCode = "Thanh toán - Chuyển phát";
            }
            //else if (_srvCode == "3")
            //{
            //    servCode = "Chuyển phát - Thanh toán";
            //}
            else
            {
                servCode = "Tất cả";
            }

            if (!string.IsNullOrEmpty(_status))
            {
                dynamic[] _lst;
                _lst = PayID.Portal.Configuration.Data_S24.List("Status", Query.EQ("StatusCode", _status));
                if (_lst != null && _lst.Length > 0)
                {
                    foreach (dynamic iteStatus in _lst)
                    {
                        statuscode = iteStatus.StatusDescription;
                    }
                }
            }
            if (!string.IsNullOrEmpty(_province))
            {
                dynamic[] _lst;
                _lst = PayID.Portal.Configuration.Data_S24.List("mbcProvince", Query.EQ("ProvinceCode", _province));
                if (_lst != null && _lst.Length > 0)
                {
                    foreach (dynamic it in _lst)
                    {
                        provincename = it.ProvinceCode + "-" + it.ProvinceName;
                    }
                }
            }
            if (!string.IsNullOrEmpty(_district))
            {
                dynamic[] _lst;
                _lst = PayID.Portal.Configuration.Data_S24.List("mbcDistrict", Query.EQ("DistrictCode", _district));
                if (_lst != null && _lst.Length > 0)
                {
                    foreach (dynamic it in _lst)
                    {
                        districtname = it.DistrictCode + "-" + it.DistrictName;
                    }
                }
            }
            if (!string.IsNullOrEmpty(_pos))
            {
                dynamic[] _lst;
                _lst = PayID.Portal.Configuration.Data_S24.List("mbcPos", Query.EQ("POSCode", _pos));
                if (_lst != null && _lst.Length > 0)
                {
                    foreach (dynamic it in _lst)
                    {
                        posname = it.POSCode + "-" + it.POSName;
                    }
                }
            }
            ReportDataSource rd = new ReportDataSource("Shipment", lstShipment);
            lr.DataSources.Add(rd);
            string reportType = id;
            string mimeType;
            string encoding;
            string fileNameExtension;
            ReportParameter[] paras = new ReportParameter[9];
            dynamic dyna = new DynamicObj();
            if (Session["profile"] != null)
            {
                dyna = (dynamic)Session["profile"];
            }
            string fullname = "";

            if (dyna.full_name != null)
            {
                fullname = dyna.full_name;
            }
            //get info over business code
            dynamic dynaBusi = new DynamicObj();
            string agent = "Tất cả";

            if (!string.IsNullOrEmpty(_busscode))
            {
                IMongoQuery img = Query.Or(
                    Query.EQ("_id", _busscode),
                    Query.EQ("general_full_name", _busscode)
                    );
                dynaBusi = PayID.Portal.Configuration.Data_S24.Get("business_profile", img);
                if (dynaBusi != null)
                {
                    agent = _busscode + "/" + dynaBusi.general_full_name;
                }
            }

            paras[0] = new ReportParameter("fromdate", string.IsNullOrEmpty(_fromdate) ? "" : DateTime.Parse(_fromdate).ToString("dd/MM/yyyy"), false);
            paras[1] = new ReportParameter("todate", string.IsNullOrEmpty(_todate) ? "" : DateTime.Parse(_todate).ToString("dd/MM/yyyy"), false);
            paras[2] = new ReportParameter("status", string.IsNullOrEmpty(statuscode) ? "Tất cả" : statuscode, false);
            paras[3] = new ReportParameter("usercreated", string.IsNullOrEmpty(fullname) ? "admin" : fullname, false);
            paras[4] = new ReportParameter("agent", agent, false);
            paras[5] = new ReportParameter("servicetype", string.IsNullOrEmpty(servCode) ? "Tất cả" : servCode, false);
            paras[6] = new ReportParameter("provincename", provincename, false);
            paras[7] = new ReportParameter("districtname", districtname, false);
            paras[8] = new ReportParameter("posname", posname, false);
            lr.SetParameters(paras);

            string deviceInfo =

            "<DeviceInfo>" +
            "  <OutputFormat>" + id + "</OutputFormat>" +
            "  <PageWidth>14in</PageWidth>" +
            "  <PageHeight>12in</PageHeight>" +
            "  <MarginTop>0.5in</MarginTop>" +
            "  <MarginLeft>1in</MarginLeft>" +
            "  <MarginRight>1in</MarginRight>" +
            "  <MarginBottom>0.5in</MarginBottom>" +
            "</DeviceInfo>";

            Warning[] warnings;
            string[] streams;
            byte[] renderedBytes;

            renderedBytes = lr.Render(
                reportType,
                deviceInfo,
                out mimeType,
                out encoding,
                out fileNameExtension,
                out streams,
                out warnings
                );
            if (id == "PDF")
                return File(renderedBytes, mimeType, "Thống_kê_vận_đơn_theo_các_đơn_hàng_đã_được_phát_hànhTH.pdf");
            else
                return File(renderedBytes, mimeType, "Thống_kê_vận_đơn_theo_các_đơn_hàng_đã_được_phát_hànhTH.xls");
        }
        public ActionResult Report(string id)
        {
            LocalReport lr = new LocalReport();

            string path = Path.Combine(Server.MapPath("~/Areas/Report/Templates/Lading"), "DetailLading.rdlc");
            if (System.IO.File.Exists(path))
            {
                lr.ReportPath = path;
            }
            else
            {
                return View("Index");
            }
            List<Models.Lading> lstLading = new List<Models.Lading>();

            string _whereclause = "";
            string statuscode = "";
            try
            {
                if (!string.IsNullOrEmpty(_status) && _status != "0")
                {
                    _whereclause += " AND STATUS='" + _status + "' ";
                }

                if (!string.IsNullOrEmpty(_busscode))
                {
                    _whereclause += " AND CUSTOMER_CODE='" + _busscode + "' ";
                }

                if (!string.IsNullOrEmpty(_fromdate) && string.IsNullOrEmpty(_todate))
                {
                    _whereclause += " AND trunc(CREATEDDATE)=to_date('" + DateTime.Parse(_fromdate).ToString("dd/MM/yyyy") + "','dd/MM/yyyy') ";
                }

                if (!string.IsNullOrEmpty(_todate) && string.IsNullOrEmpty(_fromdate))
                {
                    _whereclause += " AND trunc(CREATEDDATE)=to_date'" + DateTime.Parse(_todate).ToString("dd/MM/yyyy") + "','dd/MM/yyyy') ";
                }

                if (!string.IsNullOrEmpty(_todate) && !string.IsNullOrEmpty(_fromdate))
                {
                    _whereclause += " AND trunc(CREATEDDATE) between to_date('" + DateTime.Parse(_fromdate).ToString("dd/MM/yyyy") + "','dd/MM/yyyy') and to_date('" + DateTime.Parse(_todate).ToString("dd/MM/yyyy") + "','dd/MM/yyyy') ";
                }

                if (!string.IsNullOrEmpty(_srvCode) && _srvCode != "00")
                {
                    _whereclause += " AND TYPE = '" + _srvCode + "'";
                }
                if (Session["profile"] != null)
                {
                    dynamic profile = (dynamic)Session["profile"];

                    _whereclause += " AND (SENDER_PO_CODE=" + profile.unit_code + " or SENDER_PROVINCE_CODE=" + profile.unit_code + ")";
                }
                else
                {
                    return RedirectToAction("Login", "Home");
                }
            }
            catch
            {
                _whereclause = "";
            }
            List<dynamic> list_lading = new List<dynamic>();
            if (!string.IsNullOrEmpty(_whereclause))
            {
                using (var client = new HttpClient())
                {
                    try
                    {
                        client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var mylading = new
                        {
                            objectName = "Lading",
                            whereClause = _whereclause,
                            rptType = 1
                        };
                        try
                        {
                            var response = client.PostAsJsonAsync("api/Lading", mylading).Result;
                            list_lading = response.Content.ReadAsAsync<IEnumerable<dynamic>>().Result.ToList();
                            //list_lading = (from c in list_lading orderby c.CREATEDDATE descending select c).ToList();
                        }
                        catch { }
                    }
                    catch
                    {
                        //list_lading = null;
                    }
                }
            }
            string servCode = "Tất cả";
            foreach (dynamic l in list_lading)
            {
                Models.Lading iLading = new Models.Lading();

                iLading.LadingCode = l.CODE;
                iLading.ServiceType = l.TYPE.ToString();
                DateTime transDate = DateTime.Parse(l.CREATEDDATE.ToString());
                iLading.CreatedDate = transDate.ToString("dd/MM/yyyy");
                iLading.Time = transDate.ToShortTimeString();

                if (l.MAIN_FEE != null)
                {
                    iLading.MainFee = decimal.Parse(string.IsNullOrEmpty(l.MAIN_FEE.ToString()) ? "0" : l.MAIN_FEE.ToString());
                }
                else
                {
                    iLading.MainFee = 0;
                }

                if (l.PAYCODE != null)
                {
                    iLading.PayCode = l.PAYCODE;
                }
                else
                {
                    iLading.PayCode = "";
                }
                if (l.BillCode != null)
                {
                    iLading.BillCode = l.BillCode;
                }
                else
                {
                    iLading.BillCode = "";
                }

                if (l.COD_FEE != null)
                {
                    iLading.CodFee = decimal.Parse(string.IsNullOrEmpty(l.COD_FEE.ToString()) ? "0" : l.COD_FEE.ToString());
                }
                else
                {
                    iLading.CodFee = 0;
                }

                if (l.SERVICE_FEE != null)
                {
                    iLading.ServiceFee = decimal.Parse(string.IsNullOrEmpty(l.SERVICE_FEE.ToString()) ? "0" : l.SERVICE_FEE.ToString());
                }
                else
                {
                    iLading.ServiceFee = 0;
                }

                if (l.QUANTITY != null)
                {
                    iLading.Quantity = 1;// decimal.Parse(string.IsNullOrEmpty(l.Quantity.ToString()) ? "0" : l.Quantity.ToString());
                }
                else
                {
                    iLading.Quantity = 0;
                }

                iLading.GroupCode = l.CODE;

                iLading.GroupCode = l.CODE;
                iLading.BillCode = "";
                if (l.TYPE == 1)
                {
                    iLading.ServiceCode = "Chuyển phát";
                }
                else if (l.TYPE == 2)
                {
                    iLading.ServiceCode = "Thanh toán - Chuyển phát";
                }
                else
                {
                    iLading.ServiceCode = "";
                }


                iLading.Amount = decimal.Parse(string.IsNullOrEmpty(l.VALUE.ToString()) ? "0" : l.VALUE.ToString());


                iLading.TotalFee = iLading.MainFee + iLading.CodFee + iLading.ServiceFee;

                if (iLading.CodFee > 0)
                {
                    iLading.TotalAmount = iLading.Amount + iLading.MainFee + iLading.CodFee + iLading.ServiceFee;
                }
                else
                {
                    iLading.TotalAmount = 0;
                }

                lstLading.Add(iLading);
            }

            if (_srvCode == "1")
            {
                servCode = "Chuyển phát";
            }
            else if (_srvCode == "2")
            {
                servCode = "Thanh toán - Chuyển phát";
            }
            //else if (_srvCode == "3")
            //{
            //    servCode = "Chuyển phát - Thanh toán";
            //}
            else
            {
                servCode = "Tất cả";
            }

            if (!string.IsNullOrEmpty(_status))
            {
                dynamic[] _lst;
                _lst = PayID.Portal.Configuration.Data_S24.List("Status", Query.EQ("StatusCode", _status));
                if (_lst != null && _lst.Length > 0)
                {
                    foreach (dynamic iteStatus in _lst)
                    {
                        statuscode = iteStatus.StatusDescription;
                    }
                }
            }
            ReportDataSource rd = new ReportDataSource("Lading", lstLading);
            lr.DataSources.Add(rd);
            string reportType = id;
            string mimeType;
            string encoding;
            string fileNameExtension;
            ReportParameter[] paras = new ReportParameter[6];
            dynamic dyna = new DynamicObj();
            if (Session["profile"] != null)
            {
                dyna = (dynamic)Session["profile"];
            }
            string fullname = "";

            if (dyna.full_name != null)
            {
                fullname = dyna.full_name;
            }
            //get info over business code
            dynamic dynaBusi = new DynamicObj();
            string agent = "Tất cả";

            if (!string.IsNullOrEmpty(_busscode))
            {
                IMongoQuery img = Query.EQ("_id", _busscode);
                dynaBusi = PayID.Portal.Configuration.Data_S24.Get("business_profile", img);
                if (dynaBusi != null)
                {
                    agent = _busscode + dynaBusi.general_full_name;
                }
            }

            paras[0] = new ReportParameter("fromdate", string.IsNullOrEmpty(_fromdate) ? "" : _fromdate, false);
            paras[1] = new ReportParameter("todate", string.IsNullOrEmpty(_todate) ? "" : _todate, false);
            paras[2] = new ReportParameter("status", string.IsNullOrEmpty(statuscode) ? "Tất cả" : statuscode, false);
            paras[3] = new ReportParameter("usercreated", string.IsNullOrEmpty(fullname) ? "admin" : fullname, false);
            paras[4] = new ReportParameter("agent", agent, false);
            paras[5] = new ReportParameter("servicetype", string.IsNullOrEmpty(servCode) ? "Tất cả" : servCode, false);

            lr.SetParameters(paras);

            string deviceInfo =

            "<DeviceInfo>" +
            "  <OutputFormat>" + id + "</OutputFormat>" +
            "  <PageWidth>8.5in</PageWidth>" +
            "  <PageHeight>11in</PageHeight>" +
            "  <MarginTop>0.5in</MarginTop>" +
            "  <MarginLeft>1in</MarginLeft>" +
            "  <MarginRight>1in</MarginRight>" +
            "  <MarginBottom>0.5in</MarginBottom>" +
            "</DeviceInfo>";

            Warning[] warnings;
            string[] streams;
            byte[] renderedBytes;

            renderedBytes = lr.Render(
                reportType,
                deviceInfo,
                out mimeType,
                out encoding,
                out fileNameExtension,
                out streams,
                out warnings);
            if (id == "PDF")
                return File(renderedBytes, mimeType, "Báo_cáo_chi_tiết_vận_đơn.pdf");
            else
                return File(renderedBytes, mimeType, "Báo_cáo_chi_tiết_vận_đơn.xls");
        }
        public ActionResult ReportTotal(string id)
        {
            LocalReport lr = new LocalReport();

            string path = Path.Combine(Server.MapPath("~/Areas/Report/Templates/Lading"), "DetailLadingTotal.rdlc");
            if (System.IO.File.Exists(path))
            {
                lr.ReportPath = path;
            }
            else
            {
                return View("Index");
            }
            List<Models.Lading> lstLading = new List<Models.Lading>();

            string _whereclause = "";
            string statuscode = "";
            try
            {
                _whereclause += _whereclause + DateTime.Parse(_fromdate).ToString("dd/MM/yyyy") + ";" + DateTime.Parse(_todate).ToString("dd/MM/yyyy") + ";" + _busscode + ";" + _status + ";" + _srvCode + ";" + "N" + ";" + "N";
            }
            catch
            {
                _whereclause = "";
            }
            List<dynamic> list_lading = new List<dynamic>();
            List<Models.Lading> _listLading = new List<Models.Lading>();
            if (!string.IsNullOrEmpty(_whereclause))
            {
                using (var client = new HttpClient())
                {
                    try
                    {
                        client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var mylading = new
                        {
                            objectName = "PKG_TOTAL_LADING.GetLadingTotal",
                            whereClause = _whereclause,
                            rptType = 0,
                            expType = 1
                        };
                        try
                        {
                            var response = client.PostAsJsonAsync("api/Lading", mylading).Result;
                            list_lading = response.Content.ReadAsAsync<IEnumerable<dynamic>>().Result.ToList();
                        }
                        catch { }
                    }
                    catch
                    {
                        //list_lading = null;
                    }
                }
            }
            string servCode = "Tất cả";
            foreach (dynamic l in list_lading)
            {
                Models.Lading iLading = new Models.Lading();

                //iLading.LadingCode = l.CODE;
                if (l.TYPE.ToString() == "1")
                {
                    iLading.ServiceType = "1- Chuyển phát";
                }
                else
                {
                    iLading.ServiceType = "2- Thanh toán - Chuyển phát";
                }
                DateTime transDate = DateTime.Parse(l.TRANS_DATE.ToString());
                iLading.CreatedDate = transDate.ToString("dd/MM/yyyy");
                //iLading.Time = transDate.ToShortTimeString();
                iLading.Status = l.STATUS;
                iLading.CustomerCode = l.CUSTOMER_CODE;

                if (l.MAIN_FEE != null)
                {
                    iLading.MainFee = decimal.Parse(string.IsNullOrEmpty(l.MAIN_FEE.ToString()) ? "0" : l.MAIN_FEE.ToString());
                }
                else
                {
                    iLading.MainFee = 0;
                }

                if (l.COD_FEE != null)
                {
                    iLading.CodFee = decimal.Parse(string.IsNullOrEmpty(l.COD_FEE.ToString()) ? "0" : l.COD_FEE.ToString());
                }
                else
                {
                    iLading.CodFee = 0;
                }

                if (l.SERVICE_FEE != null)
                {
                    iLading.ServiceFee = decimal.Parse(string.IsNullOrEmpty(l.SERVICE_FEE.ToString()) ? "0" : l.SERVICE_FEE.ToString());
                }
                else
                {
                    iLading.ServiceFee = 0;
                }

                if (l.QUANTITY != null)
                {
                    iLading.Quantity = decimal.Parse(string.IsNullOrEmpty(l.QUANTITY.ToString()) ? "0" : l.QUANTITY.ToString());
                }
                else
                {
                    iLading.Quantity = 0;
                }


                //iLading.GroupCode = l.CODE;
                //iLading.BillCode = "";
                if (l.TYPE == 1)
                {
                    iLading.ServiceCode = "Chuyển phát";
                }
                else if (l.TypeService == 2)
                {
                    iLading.ServiceCode = "Thanh toán - Chuyển phát";
                }

                iLading.Amount = decimal.Parse(string.IsNullOrEmpty(l.VALUE.ToString()) ? "0" : l.VALUE.ToString());


                iLading.TotalFee = iLading.MainFee + iLading.CodFee + iLading.ServiceFee;
                if (l.AMOUNT_NO_COD != null)
                {
                    iLading.TotalAmount = l.AMOUNT_NO_COD;
                }
                else
                {
                    iLading.TotalAmount = 0;
                }


                lstLading.Add(iLading);
            }

            if (_srvCode == "1")
            {
                servCode = "Chuyển phát";
            }
            else if (_srvCode == "2")
            {
                servCode = "Thanh toán - Chuyển phát";
            }

            else
            {
                servCode = "Tất cả";
            }

            ReportDataSource rd = new ReportDataSource("Lading", lstLading);
            lr.DataSources.Add(rd);
            string reportType = id;
            string mimeType;
            string encoding;
            string fileNameExtension;
            ReportParameter[] paras = new ReportParameter[6];
            dynamic dyna = new DynamicObj();
            if (Session["profile"] != null)
            {
                dyna = (dynamic)Session["profile"];
            }
            string fullname = "";

            if (dyna.full_name != null)
            {
                fullname = dyna.full_name;
            }
            //get info over business code
            dynamic dynaBusi = new DynamicObj();
            string agent = "Tất cả";

            if (!string.IsNullOrEmpty(_busscode))
            {
                IMongoQuery img = Query.EQ("_id", _busscode);
                dynaBusi = PayID.Portal.Configuration.Data_S24.Get("business_profile", img);
                if (dynaBusi != null)
                {
                    agent = _busscode + dynaBusi.general_full_name;
                }
            }

            if (!string.IsNullOrEmpty(_status))
            {
                dynamic[] _lst;
                _lst = PayID.Portal.Configuration.Data_S24.List("Status", Query.EQ("StatusCode", _status));
                if (_lst != null && _lst.Length > 0)
                {
                    foreach (dynamic iteStatus in _lst)
                    {
                        statuscode = iteStatus.StatusDescription;
                    }
                }
            }
            paras[0] = new ReportParameter("fromdate", string.IsNullOrEmpty(_fromdate) ? "" : _fromdate, false);
            paras[1] = new ReportParameter("todate", string.IsNullOrEmpty(_todate) ? "" : _todate, false);
            paras[2] = new ReportParameter("status", string.IsNullOrEmpty(statuscode) ? "Tất cả" : statuscode, false);
            paras[3] = new ReportParameter("usercreated", string.IsNullOrEmpty(fullname) ? "admin" : fullname, false);
            paras[4] = new ReportParameter("agent", agent, false);
            paras[5] = new ReportParameter("servicetype", string.IsNullOrEmpty(servCode) ? "Tất cả" : servCode, false);

            lr.SetParameters(paras);

            string deviceInfo =

            "<DeviceInfo>" +
            "  <OutputFormat>" + id + "</OutputFormat>" +
            "  <PageWidth>8.5in</PageWidth>" +
            "  <PageHeight>11in</PageHeight>" +
            "  <MarginTop>0.5in</MarginTop>" +
            "  <MarginLeft>1in</MarginLeft>" +
            "  <MarginRight>1in</MarginRight>" +
            "  <MarginBottom>0.5in</MarginBottom>" +
            "</DeviceInfo>";

            Warning[] warnings;
            string[] streams;
            byte[] renderedBytes;

            renderedBytes = lr.Render(
                reportType,
                deviceInfo,
                out mimeType,
                out encoding,
                out fileNameExtension,
                out streams,
                out warnings);
            if (id == "PDF")
                return File(renderedBytes, mimeType, "Báo_cáo_tổng_hợp_quảnlý_vận_đơn.pdf");
            else
                return File(renderedBytes, mimeType, "Báo_cáo_tổng_hợp_quảnlý_vận_đơn.xls");

            //return File(renderedBytes, mimeType, "Báo cáo tổng hợp quản lý vận đơn");
        }
        public ActionResult ReportBill(string id)
        {
            LocalReport lr = new LocalReport();

            string path = Path.Combine(Server.MapPath("~/Areas/Report/Templates/Bill"), "DetailBill.rdlc");
            if (System.IO.File.Exists(path))
            {
                lr.ReportPath = path;
            }
            else
            {
                return View("Index");
            }
            List<Models.Lading> lstLading = new List<Models.Lading>();

            string _whereclause = " AND BillCode is not null ";
            string statuscode = "";
            try
            {
                if (!string.IsNullOrEmpty(_status) && _status != "0")
                {
                    _whereclause += " AND STATUS='" + _status + "' ";
                }

                if (!string.IsNullOrEmpty(_busscode))
                {
                    _whereclause += " AND CUSTOMER_CODE='" + _busscode + "' ";
                }

                if (!string.IsNullOrEmpty(_fromdate) && string.IsNullOrEmpty(_todate))
                {
                    _whereclause += " AND trunc(CREATEDDATE)=to_date('" + DateTime.Parse(_fromdate).ToString("dd/MM/yyyy") + "','dd/MM/yyyy') ";
                }

                if (!string.IsNullOrEmpty(_todate) && string.IsNullOrEmpty(_fromdate))
                {
                    _whereclause += " AND trunc(CREATEDDATE)=to_date'" + DateTime.Parse(_todate).ToString("dd/MM/yyyy") + "','dd/MM/yyyy') ";
                }

                if (!string.IsNullOrEmpty(_todate) && !string.IsNullOrEmpty(_fromdate))
                {
                    _whereclause += " AND trunc(CREATEDDATE) between to_date('" + DateTime.Parse(_fromdate).ToString("dd/MM/yyyy") + "','dd/MM/yyyy') and to_date('" + DateTime.Parse(_todate).ToString("dd/MM/yyyy") + "','dd/MM/yyyy') ";
                }

                if (!string.IsNullOrEmpty(_srvCode) && _srvCode != "00")
                {
                    _whereclause += " AND TYPE = '" + _srvCode + "'";
                }

            }
            catch
            {
                _whereclause = "";
            }
            List<dynamic> list_lading = new List<dynamic>();
            if (!string.IsNullOrEmpty(_whereclause))
            {
                using (var client = new HttpClient())
                {
                    try
                    {
                        client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var mylading = new
                        {
                            objectName = "Lading",
                            whereClause = _whereclause,
                            rptType = 1
                        };
                        try
                        {
                            var response = client.PostAsJsonAsync("api/Lading", mylading).Result;
                            list_lading = response.Content.ReadAsAsync<IEnumerable<dynamic>>().Result.ToList();
                            //list_lading = (from c in list_lading orderby c.CREATEDDATE descending select c).ToList();
                        }
                        catch { }
                    }
                    catch
                    {
                        //list_lading = null;
                    }
                }
            }
            string servCode = "Tất cả";
            foreach (dynamic l in list_lading)
            {
                Models.Lading iLading = new Models.Lading();

                iLading.LadingCode = l.CODE;
                iLading.ServiceType = l.TYPE.ToString();
                DateTime transDate = DateTime.Parse(l.CREATEDDATE.ToString());
                iLading.CreatedDate = transDate.ToString("dd/MM/yyyy");
                iLading.Time = transDate.ToShortTimeString();

                if (l.MAIN_FEE != null)
                {
                    iLading.MainFee = decimal.Parse(string.IsNullOrEmpty(l.MAIN_FEE.ToString()) ? "0" : l.MAIN_FEE.ToString());
                }
                else
                {
                    iLading.MainFee = 0;
                }

                if (l.PAYCODE != null)
                {
                    iLading.PayCode = l.PAYCODE;
                }
                else
                {
                    iLading.PayCode = "";
                }
                if (l.BillCode != null)
                {
                    iLading.BillCode = l.BillCode;
                }
                else
                {
                    iLading.BillCode = "";
                }

                if (l.COD_FEE != null)
                {
                    iLading.CodFee = decimal.Parse(string.IsNullOrEmpty(l.COD_FEE.ToString()) ? "0" : l.COD_FEE.ToString());
                }
                else
                {
                    iLading.CodFee = 0;
                }

                if (l.SERVICE_FEE != null)
                {
                    iLading.ServiceFee = decimal.Parse(string.IsNullOrEmpty(l.SERVICE_FEE.ToString()) ? "0" : l.SERVICE_FEE.ToString());
                }
                else
                {
                    iLading.ServiceFee = 0;
                }

                if (l.QUANTITY != null)
                {
                    iLading.Quantity = 1;// decimal.Parse(string.IsNullOrEmpty(l.Quantity.ToString()) ? "0" : l.Quantity.ToString());
                }
                else
                {
                    iLading.Quantity = 0;
                }

                iLading.GroupCode = l.CODE;
                iLading.BillCode = "";
                if (l.TYPE == 1)
                {
                    iLading.ServiceCode = "Chuyển phát";
                }
                else if (l.TYPE == 2)
                {
                    iLading.ServiceCode = "Thanh toán - Chuyển phát";
                }
                else
                {
                    iLading.ServiceCode = "";
                }


                iLading.Amount = decimal.Parse(string.IsNullOrEmpty(l.VALUE.ToString()) ? "0" : l.VALUE.ToString());


                iLading.TotalFee = iLading.MainFee + iLading.CodFee + iLading.ServiceFee;

                if (iLading.CodFee > 0)
                {
                    iLading.TotalAmount = iLading.Amount + iLading.MainFee + iLading.CodFee + iLading.ServiceFee;
                }
                else
                {
                    iLading.TotalAmount = 0;
                }

                lstLading.Add(iLading);
            }

            if (_srvCode == "1")
            {
                servCode = "Chuyển phát";
            }
            else if (_srvCode == "2")
            {
                servCode = "Thanh toán - Chuyển phát";
            }

            else
            {
                servCode = "Tất cả";
            }

            ReportDataSource rd = new ReportDataSource("Lading", lstLading);
            lr.DataSources.Add(rd);
            string reportType = id;
            string mimeType;
            string encoding;
            string fileNameExtension;
            ReportParameter[] paras = new ReportParameter[6];
            dynamic dyna = new DynamicObj();
            if (Session["profile"] != null)
            {
                dyna = (dynamic)Session["profile"];
            }
            string fullname = "";

            if (dyna.full_name != null)
            {
                fullname = dyna.full_name;
            }
            //get info over business code
            dynamic dynaBusi = new DynamicObj();
            string agent = "Tất cả";

            if (!string.IsNullOrEmpty(_busscode))
            {
                IMongoQuery img = Query.EQ("_id", _busscode);
                dynaBusi = PayID.Portal.Configuration.Data_S24.Get("business_profile", img);
                if (dynaBusi != null)
                {
                    agent = _busscode + dynaBusi.general_full_name;
                }
            }

            if (!string.IsNullOrEmpty(_status))
            {
                dynamic[] _lst;
                _lst = PayID.Portal.Configuration.Data_S24.List("Status", Query.EQ("StatusCode", _status));
                if (_lst != null && _lst.Length > 0)
                {
                    foreach (dynamic iteStatus in _lst)
                    {
                        statuscode = iteStatus.StatusDescription;
                    }
                }
            }
            paras[0] = new ReportParameter("fromdate", string.IsNullOrEmpty(_fromdate) ? "" : _fromdate, false);
            paras[1] = new ReportParameter("todate", string.IsNullOrEmpty(_todate) ? "" : _todate, false);
            paras[2] = new ReportParameter("status", string.IsNullOrEmpty(statuscode) ? "Tất cả" : statuscode, false);
            paras[3] = new ReportParameter("usercreated", string.IsNullOrEmpty(fullname) ? "admin" : fullname, false);
            paras[4] = new ReportParameter("agent", agent, false);
            paras[5] = new ReportParameter("servicetype", string.IsNullOrEmpty(servCode) ? "Tất cả" : servCode, false);

            lr.SetParameters(paras);

            string deviceInfo =

            "<DeviceInfo>" +
            "  <OutputFormat>" + id + "</OutputFormat>" +
            "  <PageWidth>8.5in</PageWidth>" +
            "  <PageHeight>11in</PageHeight>" +
            "  <MarginTop>0.5in</MarginTop>" +
            "  <MarginLeft>1in</MarginLeft>" +
            "  <MarginRight>1in</MarginRight>" +
            "  <MarginBottom>0.5in</MarginBottom>" +
            "</DeviceInfo>";

            Warning[] warnings;
            string[] streams;
            byte[] renderedBytes;

            renderedBytes = lr.Render(
                reportType,
                deviceInfo,
                out mimeType,
                out encoding,
                out fileNameExtension,
                out streams,
                out warnings);

            if (id == "PDF")
                return File(renderedBytes, mimeType, "Báo_cáo_chi_tiết_quảnlý_đơn_hàng.pdf");
            else
                return File(renderedBytes, mimeType, "Báo_cáo_chi_tiết_quảnlý_đơn_hàng.xls");

            //return File(renderedBytes, mimeType, "Báo cáo chi tiết quản lý đơn hàng");
        }
        public ActionResult ReportCollect(string id)
        {
            LocalReport lr = new LocalReport();

            string path = Path.Combine(Server.MapPath("~/Areas/Report/Templates/collection"), "DetailCollect.rdlc");

            if (System.IO.File.Exists(path))
            {
                lr.ReportPath = path;
            }
            else
            {
                return View("Index");
            }
            List<Models.Lading> lstLading = new List<Models.Lading>();

            string _whereclause = " AND PayCode is not null ";
            string statuscode = "";
            try
            {
                if (!string.IsNullOrEmpty(_status) && _status != "0")
                {
                    _whereclause += " AND STATUS='" + _status + "' ";
                }

                if (!string.IsNullOrEmpty(_busscode))
                {
                    _whereclause += " AND CUSTOMER_CODE='" + _busscode + "' ";
                }

                if (!string.IsNullOrEmpty(_fromdate) && string.IsNullOrEmpty(_todate))
                {
                    _whereclause += " AND trunc(CREATEDDATE)=to_date('" + DateTime.Parse(_fromdate).ToString("dd/MM/yyyy") + "','dd/MM/yyyy') ";
                }

                if (!string.IsNullOrEmpty(_todate) && string.IsNullOrEmpty(_fromdate))
                {
                    _whereclause += " AND trunc(CREATEDDATE)=to_date'" + DateTime.Parse(_todate).ToString("dd/MM/yyyy") + "','dd/MM/yyyy') ";
                }

                if (!string.IsNullOrEmpty(_todate) && !string.IsNullOrEmpty(_fromdate))
                {
                    _whereclause += " AND trunc(CREATEDDATE) between to_date('" + DateTime.Parse(_fromdate).ToString("dd/MM/yyyy") + "','dd/MM/yyyy') and to_date('" + DateTime.Parse(_todate).ToString("dd/MM/yyyy") + "','dd/MM/yyyy') ";
                }

                if (!string.IsNullOrEmpty(_srvCode) && _srvCode != "00")
                {
                    _whereclause += " AND TYPE = '" + _srvCode + "'";
                }

            }
            catch
            {
                _whereclause = "";
            }
            List<dynamic> list_lading = new List<dynamic>();
            if (!string.IsNullOrEmpty(_whereclause))
            {
                using (var client = new HttpClient())
                {
                    try
                    {
                        client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var mylading = new
                        {
                            objectName = "Lading",
                            whereClause = _whereclause,
                            rptType = 1
                        };
                        try
                        {
                            var response = client.PostAsJsonAsync("api/Lading", mylading).Result;
                            list_lading = response.Content.ReadAsAsync<IEnumerable<dynamic>>().Result.ToList();
                            //list_lading = (from c in list_lading orderby c.CREATEDDATE descending select c).ToList();
                        }
                        catch { }
                    }
                    catch
                    {
                        //list_lading = null;
                    }
                }
            }
            string servCode = "Tất cả";
            foreach (dynamic l in list_lading)
            {
                Models.Lading iLading = new Models.Lading();

                iLading.LadingCode = l.CODE;
                iLading.ServiceType = l.TYPE.ToString();
                DateTime transDate = DateTime.Parse(l.CREATEDDATE.ToString());
                iLading.CreatedDate = transDate.ToString("dd/MM/yyyy");
                iLading.Time = transDate.ToShortTimeString();

                if (l.MAIN_FEE != null)
                {
                    iLading.MainFee = decimal.Parse(string.IsNullOrEmpty(l.MAIN_FEE.ToString()) ? "0" : l.MAIN_FEE.ToString());
                }
                else
                {
                    iLading.MainFee = 0;
                }

                if (l.PAYCODE != null)
                {
                    iLading.PayCode = l.PAYCODE;
                }
                else
                {
                    iLading.PayCode = "";
                }
                if (l.BillCode != null)
                {
                    iLading.BillCode = l.BillCode;
                }
                else
                {
                    iLading.BillCode = "";
                }

                if (l.COD_FEE != null)
                {
                    iLading.CodFee = decimal.Parse(string.IsNullOrEmpty(l.COD_FEE.ToString()) ? "0" : l.COD_FEE.ToString());
                }
                else
                {
                    iLading.CodFee = 0;
                }

                if (l.SERVICE_FEE != null)
                {
                    iLading.ServiceFee = decimal.Parse(string.IsNullOrEmpty(l.SERVICE_FEE.ToString()) ? "0" : l.SERVICE_FEE.ToString());
                }
                else
                {
                    iLading.ServiceFee = 0;
                }

                if (l.QUANTITY != null)
                {
                    iLading.Quantity = 1;// decimal.Parse(string.IsNullOrEmpty(l.Quantity.ToString()) ? "0" : l.Quantity.ToString());
                }
                else
                {
                    iLading.Quantity = 0;
                }

                iLading.GroupCode = l.CODE;
                iLading.BillCode = "";
                if (l.TYPE == 1)
                {
                    iLading.ServiceCode = "Chuyển phát";
                }
                else if (l.TYPE == 2)
                {
                    iLading.ServiceCode = "Thanh toán - Chuyển phát";
                }
                else
                {
                    iLading.ServiceCode = "";
                }


                iLading.Amount = decimal.Parse(string.IsNullOrEmpty(l.VALUE.ToString()) ? "0" : l.VALUE.ToString());


                iLading.TotalFee = iLading.MainFee + iLading.CodFee + iLading.ServiceFee;

                if (iLading.CodFee > 0)
                {
                    iLading.TotalAmount = iLading.Amount + iLading.MainFee + iLading.CodFee + iLading.ServiceFee;
                }
                else
                {
                    iLading.TotalAmount = 0;
                }

                lstLading.Add(iLading);
            }

            if (_srvCode == "1")
            {
                servCode = "Chuyển phát";
            }
            else if (_srvCode == "2")
            {
                servCode = "Thanh toán - Chuyển phát";
            }

            else
            {
                servCode = "Tất cả";
            }

            ReportDataSource rd = new ReportDataSource("Lading", lstLading);
            lr.DataSources.Add(rd);
            string reportType = id;
            string mimeType;
            string encoding;
            string fileNameExtension;
            ReportParameter[] paras = new ReportParameter[6];
            dynamic dyna = new DynamicObj();
            if (Session["profile"] != null)
            {
                dyna = (dynamic)Session["profile"];
            }
            string fullname = "";

            if (dyna.full_name != null)
            {
                fullname = dyna.full_name;
            }
            //get info over business code
            dynamic dynaBusi = new DynamicObj();
            string agent = "Tất cả";

            if (!string.IsNullOrEmpty(_busscode))
            {
                IMongoQuery img = Query.EQ("_id", _busscode);
                dynaBusi = PayID.Portal.Configuration.Data_S24.Get("business_profile", img);
                if (dynaBusi != null)
                {
                    agent = _busscode + dynaBusi.general_full_name;
                }
            }

            if (!string.IsNullOrEmpty(_status))
            {
                dynamic[] _lst;
                _lst = PayID.Portal.Configuration.Data_S24.List("Status", Query.EQ("StatusCode", _status));
                if (_lst != null && _lst.Length > 0)
                {
                    foreach (dynamic iteStatus in _lst)
                    {
                        statuscode = iteStatus.StatusDescription;
                    }
                }
            }

            paras[0] = new ReportParameter("fromdate", string.IsNullOrEmpty(_fromdate) ? "" : _fromdate, false);
            paras[1] = new ReportParameter("todate", string.IsNullOrEmpty(_todate) ? "" : _todate, false);
            paras[2] = new ReportParameter("status", string.IsNullOrEmpty(statuscode) ? "Tất cả" : statuscode, false);
            paras[3] = new ReportParameter("usercreated", string.IsNullOrEmpty(fullname) ? "admin" : fullname, false);
            paras[4] = new ReportParameter("agent", agent, false);
            paras[5] = new ReportParameter("servicetype", string.IsNullOrEmpty(servCode) ? "Tất cả" : servCode, false);

            lr.SetParameters(paras);

            string deviceInfo =

            "<DeviceInfo>" +
            "  <OutputFormat>" + id + "</OutputFormat>" +
            "  <PageWidth>8.5in</PageWidth>" +
            "  <PageHeight>11in</PageHeight>" +
            "  <MarginTop>0.5in</MarginTop>" +
            "  <MarginLeft>1in</MarginLeft>" +
            "  <MarginRight>1in</MarginRight>" +
            "  <MarginBottom>0.5in</MarginBottom>" +
            "</DeviceInfo>";

            Warning[] warnings;
            string[] streams;
            byte[] renderedBytes;

            renderedBytes = lr.Render(
                reportType,
                deviceInfo,
                out mimeType,
                out encoding,
                out fileNameExtension,
                out streams,
                out warnings);

            if (id == "PDF")
                return File(renderedBytes, mimeType, "Báo_cáo_chi_tiết_giaodịch_thanhtoan.pdf");
            else
                return File(renderedBytes, mimeType, "Báo_cáo_chi_tiết_giaodịch_thanhtoan.xls");
            //return File(renderedBytes, mimeType, "Báo cáo chi tiết giao dịch thanh toán");
        }
        public ActionResult ReportTotalBill(string id)
        {
            LocalReport lr = new LocalReport();

            string path = Path.Combine(Server.MapPath("~/Areas/Report/Templates/Bill"), "DetailBillTotal.rdlc");
            if (System.IO.File.Exists(path))
            {
                lr.ReportPath = path;
            }
            else
            {
                return View("Index");
            }
            List<Models.Lading> lstLading = new List<Models.Lading>();

            string _whereclause = "";
            string statuscode = "";
            try
            {
                _whereclause += _whereclause + DateTime.Parse(_fromdate).ToString("dd/MM/yyyy") + ";" + DateTime.Parse(_todate).ToString("dd/MM/yyyy") + ";" + _busscode + ";" + _status + ";" + _srvCode + ";" + "Y" + ";" + "N";
            }
            catch
            {
                _whereclause = "";
            }
            List<dynamic> list_lading = new List<dynamic>();
            List<Models.Lading> _listLading = new List<Models.Lading>();
            if (!string.IsNullOrEmpty(_whereclause))
            {
                using (var client = new HttpClient())
                {
                    try
                    {
                        client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var mylading = new
                        {
                            objectName = "PKG_TOTAL_LADING.GetLadingTotal",
                            whereClause = _whereclause,
                            rptType = 0,
                            expType = 1
                        };
                        try
                        {
                            var response = client.PostAsJsonAsync("api/Lading", mylading).Result;
                            list_lading = response.Content.ReadAsAsync<IEnumerable<dynamic>>().Result.ToList();
                        }
                        catch { }
                    }
                    catch
                    {
                        //list_lading = null;
                    }
                }
            }
            string servCode = "Tất cả";
            foreach (dynamic l in list_lading)
            {
                Models.Lading iLading = new Models.Lading();

                //iLading.LadingCode = l.CODE;
                if (l.TYPE.ToString() == "1")
                {
                    iLading.ServiceType = "1- Chuyển phát";
                }
                else
                {
                    iLading.ServiceType = "2- Thanh toán - Chuyển phát";
                }
                DateTime transDate = DateTime.Parse(l.TRANS_DATE.ToString());
                iLading.CreatedDate = transDate.ToString("dd/MM/yyyy");
                //iLading.Time = transDate.ToShortTimeString();
                iLading.Status = l.STATUS;
                iLading.CustomerCode = l.CUSTOMER_CODE;

                if (l.MAIN_FEE != null)
                {
                    iLading.MainFee = decimal.Parse(string.IsNullOrEmpty(l.MAIN_FEE.ToString()) ? "0" : l.MAIN_FEE.ToString());
                }
                else
                {
                    iLading.MainFee = 0;
                }

                if (l.COD_FEE != null)
                {
                    iLading.CodFee = decimal.Parse(string.IsNullOrEmpty(l.COD_FEE.ToString()) ? "0" : l.COD_FEE.ToString());
                }
                else
                {
                    iLading.CodFee = 0;
                }

                if (l.SERVICE_FEE != null)
                {
                    iLading.ServiceFee = decimal.Parse(string.IsNullOrEmpty(l.SERVICE_FEE.ToString()) ? "0" : l.SERVICE_FEE.ToString());
                }
                else
                {
                    iLading.ServiceFee = 0;
                }

                if (l.QUANTITY != null)
                {
                    iLading.Quantity = decimal.Parse(string.IsNullOrEmpty(l.QUANTITY.ToString()) ? "0" : l.QUANTITY.ToString());
                }
                else
                {
                    iLading.Quantity = 0;
                }


                //iLading.GroupCode = l.CODE;
                //iLading.BillCode = "";
                if (l.TYPE == 1)
                {
                    iLading.ServiceCode = "Chuyển phát";
                }
                else if (l.TypeService == 2)
                {
                    iLading.ServiceCode = "Thanh toán - Chuyển phát";
                }
                else
                {
                    iLading.ServiceCode = "";
                }


                iLading.Amount = decimal.Parse(string.IsNullOrEmpty(l.VALUE.ToString()) ? "0" : l.VALUE.ToString());


                iLading.TotalFee = iLading.MainFee + iLading.CodFee + iLading.ServiceFee;
                if (l.AMOUNT_NO_COD != null)
                {
                    iLading.TotalAmount = l.AMOUNT_NO_COD;
                }
                else
                {
                    iLading.TotalAmount = 0;
                }


                lstLading.Add(iLading);
            }

            if (_srvCode == "1")
            {
                servCode = "Chuyển phát";
            }
            else if (_srvCode == "2")
            {
                servCode = "Thanh toán - Chuyển phát";
            }

            else
            {
                servCode = "Tất cả";
            }

            ReportDataSource rd = new ReportDataSource("Lading", lstLading);
            lr.DataSources.Add(rd);
            string reportType = id;
            string mimeType;
            string encoding;
            string fileNameExtension;
            ReportParameter[] paras = new ReportParameter[6];
            dynamic dyna = new DynamicObj();
            if (Session["profile"] != null)
            {
                dyna = (dynamic)Session["profile"];
            }
            string fullname = "";

            if (dyna.full_name != null)
            {
                fullname = dyna.full_name;
            }
            //get info over business code
            dynamic dynaBusi = new DynamicObj();
            string agent = "Tất cả";

            if (!string.IsNullOrEmpty(_busscode))
            {
                IMongoQuery img = Query.EQ("_id", _busscode);
                dynaBusi = PayID.Portal.Configuration.Data_S24.Get("business_profile", img);
                if (dynaBusi != null)
                {
                    agent = _busscode + dynaBusi.general_full_name;
                }
            }

            if (!string.IsNullOrEmpty(_status))
            {
                dynamic[] _lst;
                _lst = PayID.Portal.Configuration.Data_S24.List("Status", Query.EQ("StatusCode", _status));
                if (_lst != null && _lst.Length > 0)
                {
                    foreach (dynamic iteStatus in _lst)
                    {
                        statuscode = iteStatus.StatusDescription;
                    }
                }
            }

            paras[0] = new ReportParameter("fromdate", string.IsNullOrEmpty(_fromdate) ? "" : _fromdate, false);
            paras[1] = new ReportParameter("todate", string.IsNullOrEmpty(_todate) ? "" : _todate, false);
            paras[2] = new ReportParameter("status", string.IsNullOrEmpty(statuscode) ? "Tất cả" : statuscode, false);
            paras[3] = new ReportParameter("usercreated", string.IsNullOrEmpty(fullname) ? "admin" : fullname, false);
            paras[4] = new ReportParameter("agent", agent, false);
            paras[5] = new ReportParameter("servicetype", string.IsNullOrEmpty(servCode) ? "Tất cả" : servCode, false);

            lr.SetParameters(paras);

            string deviceInfo =

            "<DeviceInfo>" +
            "  <OutputFormat>" + id + "</OutputFormat>" +
            "  <PageWidth>8.5in</PageWidth>" +
            "  <PageHeight>11in</PageHeight>" +
            "  <MarginTop>0.5in</MarginTop>" +
            "  <MarginLeft>1in</MarginLeft>" +
            "  <MarginRight>1in</MarginRight>" +
            "  <MarginBottom>0.5in</MarginBottom>" +
            "</DeviceInfo>";

            Warning[] warnings;
            string[] streams;
            byte[] renderedBytes;

            renderedBytes = lr.Render(
                reportType,
                deviceInfo,
                out mimeType,
                out encoding,
                out fileNameExtension,
                out streams,
                out warnings);

            if (id == "PDF")
                return File(renderedBytes, mimeType, "Báo_cáo_tổng_hợp_quảnlý_đơnhàng.pdf");
            else
                return File(renderedBytes, mimeType, "Báo_cáo_tổng_hợp_quảnlý_đơnhàng.xls");
            //return File(renderedBytes, mimeType, "Báo cáo tổng hợp quản lý đơn hàng");
        }
        public ActionResult ReportTotalPay(string id)
        {
            LocalReport lr = new LocalReport();

            string path = Path.Combine(Server.MapPath("~/Areas/Report/Templates/collection"), "DetailCollectTotal.rdlc");
            if (System.IO.File.Exists(path))
            {
                lr.ReportPath = path;
            }
            else
            {
                return View("Index");
            }
            List<Models.Lading> lstLading = new List<Models.Lading>();

            string _whereclause = "";
            string statuscode = "";
            try
            {
                _whereclause += _whereclause + DateTime.Parse(_fromdate).ToString("dd/MM/yyyy") + ";" + DateTime.Parse(_todate).ToString("dd/MM/yyyy") + ";" + _busscode + ";" + _status + ";" + _srvCode + ";" + "N" + ";" + "Y";
            }
            catch
            {
                _whereclause = "";
            }
            List<dynamic> list_lading = new List<dynamic>();
            List<Models.Lading> _listLading = new List<Models.Lading>();
            if (!string.IsNullOrEmpty(_whereclause))
            {
                using (var client = new HttpClient())
                {
                    try
                    {
                        client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var mylading = new
                        {
                            objectName = "PKG_TOTAL_LADING.GetLadingTotal",
                            whereClause = _whereclause,
                            rptType = 0,
                            expType = 1
                        };
                        try
                        {
                            var response = client.PostAsJsonAsync("api/Lading", mylading).Result;
                            list_lading = response.Content.ReadAsAsync<IEnumerable<dynamic>>().Result.ToList();
                        }
                        catch { }
                    }
                    catch
                    {
                        //list_lading = null;
                    }
                }
            }
            string servCode = "Tất cả";
            foreach (dynamic l in list_lading)
            {
                Models.Lading iLading = new Models.Lading();

                //iLading.LadingCode = l.CODE;
                if (l.TYPE.ToString() == "1")
                {
                    iLading.ServiceType = "1- Chuyển phát";
                }
                else
                {
                    iLading.ServiceType = "2- Thanh toán - Chuyển phát";
                }
                DateTime transDate = DateTime.Parse(l.TRANS_DATE.ToString());
                iLading.CreatedDate = transDate.ToString("dd/MM/yyyy");
                //iLading.Time = transDate.ToShortTimeString();
                iLading.Status = l.STATUS;
                iLading.CustomerCode = l.CUSTOMER_CODE;

                if (l.MAIN_FEE != null)
                {
                    iLading.MainFee = decimal.Parse(string.IsNullOrEmpty(l.MAIN_FEE.ToString()) ? "0" : l.MAIN_FEE.ToString());
                }
                else
                {
                    iLading.MainFee = 0;
                }

                if (l.COD_FEE != null)
                {
                    iLading.CodFee = decimal.Parse(string.IsNullOrEmpty(l.COD_FEE.ToString()) ? "0" : l.COD_FEE.ToString());
                }
                else
                {
                    iLading.CodFee = 0;
                }

                if (l.SERVICE_FEE != null)
                {
                    iLading.ServiceFee = decimal.Parse(string.IsNullOrEmpty(l.SERVICE_FEE.ToString()) ? "0" : l.SERVICE_FEE.ToString());
                }
                else
                {
                    iLading.ServiceFee = 0;
                }

                if (l.QUANTITY != null)
                {
                    iLading.Quantity = decimal.Parse(string.IsNullOrEmpty(l.QUANTITY.ToString()) ? "0" : l.QUANTITY.ToString());
                }
                else
                {
                    iLading.Quantity = 0;
                }


                //iLading.GroupCode = l.CODE;
                //iLading.BillCode = "";
                if (l.TYPE == 1)
                {
                    iLading.ServiceCode = "Chuyển phát";
                }
                else if (l.TypeService == 2)
                {
                    iLading.ServiceCode = "Thanh toán - Chuyển phát";
                }
                else
                {
                    iLading.ServiceCode = "Tất cả";
                }


                iLading.Amount = decimal.Parse(string.IsNullOrEmpty(l.VALUE.ToString()) ? "0" : l.VALUE.ToString());


                iLading.TotalFee = iLading.MainFee + iLading.CodFee + iLading.ServiceFee;
                if (l.AMOUNT_NO_COD != null)
                {
                    iLading.TotalAmount = l.AMOUNT_NO_COD;
                }
                else
                {
                    iLading.TotalAmount = 0;
                }


                lstLading.Add(iLading);
            }

            if (_srvCode == "1")
            {
                servCode = "Chuyển phát";
            }
            else if (_srvCode == "2")
            {
                servCode = "Thanh toán - Chuyển phát";
            }

            else
            {
                servCode = "Tất cả";
            }

            ReportDataSource rd = new ReportDataSource("Lading", lstLading);
            lr.DataSources.Add(rd);
            string reportType = id;
            string mimeType;
            string encoding;
            string fileNameExtension;
            ReportParameter[] paras = new ReportParameter[6];
            dynamic dyna = new DynamicObj();
            if (Session["profile"] != null)
            {
                dyna = (dynamic)Session["profile"];
            }
            string fullname = "";

            if (dyna.full_name != null)
            {
                fullname = dyna.full_name;
            }
            //get info over business code
            dynamic dynaBusi = new DynamicObj();
            string agent = "Tất cả";

            if (!string.IsNullOrEmpty(_busscode))
            {
                IMongoQuery img = Query.EQ("_id", _busscode);
                dynaBusi = PayID.Portal.Configuration.Data_S24.Get("business_profile", img);
                if (dynaBusi != null)
                {
                    agent = _busscode + dynaBusi.general_full_name;
                }
            }

            if (!string.IsNullOrEmpty(_status))
            {
                dynamic[] _lst;
                _lst = PayID.Portal.Configuration.Data_S24.List("Status", Query.EQ("StatusCode", _status));
                if (_lst != null && _lst.Length > 0)
                {
                    foreach (dynamic iteStatus in _lst)
                    {
                        statuscode = iteStatus.StatusDescription;
                    }
                }
            }

            paras[0] = new ReportParameter("fromdate", string.IsNullOrEmpty(_fromdate) ? "" : _fromdate, false);
            paras[1] = new ReportParameter("todate", string.IsNullOrEmpty(_todate) ? "" : _todate, false);
            paras[2] = new ReportParameter("status", string.IsNullOrEmpty(statuscode) ? "Tất cả" : statuscode, false);
            paras[3] = new ReportParameter("usercreated", string.IsNullOrEmpty(fullname) ? "admin" : fullname, false);
            paras[4] = new ReportParameter("agent", agent, false);
            paras[5] = new ReportParameter("servicetype", string.IsNullOrEmpty(servCode) ? "Tất cả" : servCode, false);

            lr.SetParameters(paras);

            string deviceInfo =

            "<DeviceInfo>" +
            "  <OutputFormat>" + id + "</OutputFormat>" +
            "  <PageWidth>8.5in</PageWidth>" +
            "  <PageHeight>11in</PageHeight>" +
            "  <MarginTop>0.5in</MarginTop>" +
            "  <MarginLeft>1in</MarginLeft>" +
            "  <MarginRight>1in</MarginRight>" +
            "  <MarginBottom>0.5in</MarginBottom>" +
            "</DeviceInfo>";

            Warning[] warnings;
            string[] streams;
            byte[] renderedBytes;

            renderedBytes = lr.Render(
                reportType,
                deviceInfo,
                out mimeType,
                out encoding,
                out fileNameExtension,
                out streams,
                out warnings);

            if (id == "PDF")
                return File(renderedBytes, mimeType, "Báo_cáo_tổng_hợp_quảnlý_giaodịch_thanhtoán.pdf");
            else
                return File(renderedBytes, mimeType, "Báo_cáo_tổng_hợp_quảnlý_giaodịch_thanhtoán.xls");
            //return File(renderedBytes, mimeType, "Báo cáo tổng hợp quản lý giao dịch thanh toán");
        }
    }
}