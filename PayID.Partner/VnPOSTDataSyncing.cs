using MongoDB.Driver.Builders;
using PayID.DataHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
namespace PayID.Partner
{
    public class VnPOSTDataSyncing
    {
        // - : C1,
        // --> : Mới tạo
        // - : C2,
        // --> : Hủy
        // - : C3,
        // --> : Đã thanh toán
        // - : C4,
        // --> : Chờ thanh toán
        // - : C5,
        // --> : Chấp nhận
        // - : C6,
        // --> : Đang lấy hàng
        // - : C7,
        // --> : Lấy hàng thành công
        // - : C8,
        // --> : Hủy lấy hàng
        // - : C9,
        // --> : Lấy hàng thất bại
        // - : C10,
        // --> : Khách hàng hẹn lại
        // - : C11,
        // --> : Đang vận chuyển
        // - : C12,
        // --> : Chờ phát
        // - : C13,
        // --> : Giao thành công
        // - : C14,
        // --> : Giao thất bại
        // - : C15,
        // --> : Chuyển hoàn
        // - : C16,
        // --> : Đã thu tiền
        // - : C17,
        // --> : Đã trả tiền
        // - : C18,
        // --> : Phát hoàn
        // - : C19,
        // --> : Chuyển tiếp
        // - : C20,
        // --> : Rút bưu gửi
        // - : C21,
        // --> : Vô thừa nhận
        // - : C22,
        // --> : Đang phát
        // - : C23,
        // --> : Phát tại địa chỉ
        // - : C24,
        // --> : Phát tại quầy
        // - : C27,
        // --> : Phát hoàn thành công
        // - : C25,
        // --> : Đang thu hồi
        // - : C26,
        // --> : Thu hồi thành công
        private static PayID.DataHelper.MongoHelper _data;
        static Thread bccp, cod, push;
        private static string pathLog = ConfigurationManager.AppSettings["LOG"].ToString();
        public static void OnStart()
        {
            _data = new DataHelper.MongoHelper(
                ConfigurationManager.AppSettings["CORE_DB_SERVER"],
                ConfigurationManager.AppSettings["CORE_DB_DATABASE"]
                );
            writelog("Ngày " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "- Start Service");
            bccp = new Thread(new ThreadStart(BCCPDataSync));
            cod = new Thread(new ThreadStart(CODDataSync));
            push = new Thread(new ThreadStart(PushPartnerSync));

            bccp.Start();
            //cod.Start();
            //push.Start();
            //Console.ReadLine();
        }
        public static void OnStop()
        {

            try
            {
                writelog("Ngày " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "- Stop Service");
                bccp.Abort();
            }
            catch (Exception ex) { writelog("Ngày " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "- OnStop():" + ex.ToString()); }
            //try { cod.Abort(); }
            //catch { }
        }
        private static void BCCPDataSync()
        {
            int time_sleep = int.Parse(ConfigurationSettings.AppSettings["DATASYNC_TIME_SLEEP"].ToString());
            time_sleep = time_sleep * 60000;

            while (true)
            {
                // Lấy danh sách trong bảng shipment, trừ các trạng thái thành công.
                dynamic[] _unclosed = _data.List("shipment",
                    Query.And(
                    //Query.EQ("system_time_key.date", 20160224),
                    Query.NE("system_status", "C2")
                    , Query.NE("system_status", "C13")
                    , Query.NE("system_status", "C8")
                      , Query.NE("system_status", "C26")
                        , Query.NE("system_status", "C27")
                    //, Query.NE("system_status", "C18")
                    ));
                writelog("Ngày " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "- Số lượng vận đơn " + _unclosed.Length.ToString());
                List<string> list = new List<string>();
                foreach (dynamic _uc in _unclosed)
                {
                    if (!String.IsNullOrEmpty(_uc.tracking_code))
                    {
                        if (_uc.RefCode != null)
                        {
                            list.Add(_uc.RefCode);
                        }
                        else
                        {
                            list.Add(_uc.tracking_code);
                        }
                    }
                }
                //list.Add("EF120548071VN");
                bccp_data_sync(list);
                System.Threading.Thread.Sleep(time_sleep);
            }
        }

        private static void PushPartnerSync()
        {
            while (true)
            {
                dynamic[] apis = _data.List("api_key", null);
                DateTime dt = DateTime.Now;
                foreach (dynamic api in apis)
                    if (api.realtime_pushing)
                    {
                        if (api.last_synced_time == null) api.last_synced_time = dt.AddDays(-30);
                        pushpartner(api.code, api.business_profile.ToString(), api.last_synced_time);
                        api.last_synced_time = dt;
                        _data.UpdateObject("api_key",
                            Query.EQ("_id", api._id)
                            , Update.Set("last_synced_time", dt));
                    }
                System.Threading.Thread.Sleep(300000);
            }
        }
        private static void pushpartner(string partnerCode, string partnerId, DateTime dt)
        {
            Hashtable status_mapping = new Hashtable();
            dynamic[] status_list = _data.List("partner_mapping_status", MongoDB.Driver.Builders.Query.EQ("partner", long.Parse(partnerId)));
            foreach (dynamic _status in status_list)
                status_mapping.Add(_status.system_status, _status.partner_status);
            PayID.Intergration.IPushingPartner partner = PayID.Intergration.Provider.GetPartner(partnerCode);
            dynamic[] _unclosed = _data.List("shipment",
                MongoDB.Driver.Builders.Query.And(
                MongoDB.Driver.Builders.Query.EQ("customer.code", partnerId),
                MongoDB.Driver.Builders.Query.EQ("system_status", "C11"))
                );
            foreach (dynamic shipment in _unclosed)
                if (DateTime.Parse(shipment.system_last_updated_time).ToLocalTime() > dt.ToLocalTime())
                {
                    if (status_mapping.ContainsKey(shipment.system_status)) shipment.system_status = status_mapping[shipment.system_status];
                    partner.Push(shipment);
                }
        }
        private static void CODDataSync()
        {
            int time_sleep = int.Parse(ConfigurationSettings.AppSettings["DATASYNC_TIME_SLEEP"].ToString());
            time_sleep = time_sleep * 1000;

            while (true)
            {
                dynamic[] _unclosed = _data.List("shipment", MongoDB.Driver.Builders.Query.NE("system_status", "CLOSED"));
                foreach (dynamic _uc in _unclosed)
                {
                    cod_data_sync(_uc.tracking_code);
                }
                System.Threading.Thread.Sleep(time_sleep);
            }
        }
        // Lấy trạng thái từ Vnpost
        // Lưu vào bảng lading_cod_trace
        private static void cod_data_sync(string tracking_code)
        {
            try
            {
                // Kiểm tra lại hàm(ko chạy).
                using (vnpost.cod.ServiceSoapClient client = new vnpost.cod.ServiceSoapClient())
                {

                    DataSet _ds = client.GET_MONEY_STATE_BY_ITEM(tracking_code);
                    DataTable _dtResponse = _ds.Tables["MESSAGES"];
                    if (_dtResponse.Rows[0]["ERROR_CODE"].ToString() != "00") return;

                    DataTable _dtTrace = _ds.Tables["MONEY_STATES"];
                    foreach (DataRow _row in _dtTrace.Rows)
                    {
                        dynamic _trace = new PayID.DataHelper.DynamicObj();
                        _trace.tracking_code = tracking_code;
                        _trace.date = _row["DATE"];
                        _trace.time = _row["TIME_DETAIL"];
                        _trace.trace_date = DateTime.ParseExact(_trace.date + " " + _trace.time, "dd/MM/yyyy HH:mm:ss", null);
                        _trace._id = tracking_code + "_" + _trace.trace_date.ToString("yyMMddHHmmss");
                        _trace.description = _row["STATUS_TEXT"];
                        _trace.pos = _row["VI_TRI"];
                        _data.Save("lading_cod_trace", _trace);
                    }
                }
            }
            catch { }
        }
        #region     ------ SYNC BCCP -----
        //private static void bccp_data_sync(List<string> tracking_codes)
        //{
        //    int iPage = (tracking_codes.Count + 199) / 200;

        //    for (int i = 0; i < iPage; i++)
        //    {
        //        try
        //        {
        //            using (vnpost.bccp.v2.ServiceClient client = new vnpost.bccp.v2.ServiceClient())
        //            {
        //                try
        //                {
        //                    string[] list = tracking_codes.Skip(i * 200).Take(200).ToArray();

        //                    DataSet ds = client.GetListItem(list, "CASHPOST", "CASHPOST");

        //                    DataTable _dtInfo = ds.Tables["DetailItem"];
        //                    DataTable _dtTrace = ds.Tables["TraceItem"];
        //                    DataTable _dtInfo2 = ds.Tables["Item"];
        //                    foreach (string tracking_code in list)
        //                    {
        //                        DataSet _dsDelivery = client.GetDelivery(tracking_code, "CASHPOST", "CASHPOST");
        //                        DataTable _dtDelivery = new DataTable();// = _dsDelivery.Tables["Delivery"];
        //                        DataRow[] _drDelivery = null;// _dtDelivery.Select("ItemCode='" + tracking_code + "'", "DeliveryDate");
        //                        if (_dsDelivery != null && _dsDelivery.Tables["Delivery"].Rows.Count > 0)
        //                        {
        //                            _drDelivery = new DataRow[_dtDelivery.Rows.Count];
        //                            _dtDelivery = _dsDelivery.Tables["Delivery"];
        //                            _drDelivery = _dtDelivery.Select("ItemCode='" + tracking_code + "'", "DeliveryDate");
        //                        }
        //                        else
        //                        {
        //                            _drDelivery = new DataRow[0];
        //                        }

        //                        try
        //                        {
        //                            dynamic _current = _data.Get("shipment", Query.Or(Query.EQ("tracking_code", tracking_code), Query.EQ("RefCode", tracking_code)));

        //                            dynamic _lading = _data.Get("Lading", Query.Or(Query.EQ("Code", tracking_code), Query.EQ("RefCode", tracking_code)));

        //                            dynamic _info = new PayID.DataHelper.DynamicObj();

        //                            string _status = _current.system_status;

        //                            DataRow[] _dtInfo2F = _dtInfo2.Select("ItemCode='" + tracking_code + "'");

        //                            if (_current != null && _lading != null)
        //                            {
        //                                foreach (DataRow row in _dtInfo2F)
        //                                {
        //                                    try
        //                                    {
        //                                        _info.sender_country = "VN";
        //                                        _info.receiver_country = "VN";
        //                                        _info.sender_pos = row["AcceptancePOSCode"].ToString();
        //                                        _info.receiver_pos = "";// _dtInfo.Rows[0]["BC_PHAT"].ToString();
        //                                        _info.sender_name = row["SenderFullname"].ToString();
        //                                        _info.receiver_name = row["SenderAddress"].ToString();
        //                                        _info.weight = row["Weight"];
        //                                        _info.receiver_name = row["ReceiverFullname"].ToString();
        //                                        _info.receiver_address = row["ReceiverAddress"].ToString();
        //                                        _info.receiver_id = row["ReceiverIdentification"].ToString();
        //                                        _info.receiver_id_issued_date = row["ReceiverIssueDate"].ToString();
        //                                        _info._id = tracking_code;
        //                                        try
        //                                        {
        //                                            _info.is_cod = (bool)row["CODPayment"];
        //                                        }
        //                                        catch { }
        //                                        _info.executer_order = "";// row["ExecuteOrder"].ToString();
        //                                        _info.freight = long.Parse("0" + ((double)row["MainFreight"]).ToString("0"));
        //                                        _info.cod_freight = long.Parse("0" + ((double)row["ValueAddedServiceFreightTotalFreight"]).ToString("0"));
        //                                        _info.cod_amount = _current.product.value;
        //                                        _data.Save("lading_info", _info);
        //                                        _current.parcel.weight = row["Weight"];

        //                                        _lading.Weight = _info.weight;
        //                                        _lading.MainFee = _info.freight;
        //                                        _lading.CodFee = _info.cod_freight;

        //                                        break;
        //                                    }
        //                                    catch { }
        //                                }

        //                                bool _return = false;
        //                                bool _isDelevery = false;
        //                                int DeliveryDate = 0;
        //                                //DataRow[] _drDelivery = _dtDelivery.Select("ItemCode='" + tracking_code + "'", "DeliveryDate");

        //                                DateTime _tracedate = DateTime.Now;
        //                                string _date = "0";
        //                                string _time = "0";
        //                                string _now = DateTime.Now.ToString("yyyyMMdd");
        //                                //_status = "C11";
        //                                DataRow[] _dtTraceF = _dtTrace.Select("ItemCode='" + tracking_code + "'", "TraceDate");

        //                                writelog("Bắt đầu kiểm tra và lưu trạng thái mã vận đơn " + tracking_code + " lúc " + DateTime.Now.ToString("HHmmss") + " Ngày " + DateTime.Now.ToString("yyyyMMdd"));

        //                                if (_dtTraceF != null && _dtTraceF.Length > 0)
        //                                {
        //                                    foreach (DataRow _row in _dtTraceF)
        //                                    {
        //                                        int _traceDte = int.Parse(((DateTime)_row["TraceDate"]).ToString("yyyyMMdd"));
        //                                        if (_traceDte == int.Parse(_now))
        //                                        {
        //                                            try
        //                                            {
        //                                                dynamic _trace = new PayID.DataHelper.DynamicObj();
        //                                                _trace.trace_date = (DateTime)_row["TraceDate"];
        //                                                _trace.tracking_code = tracking_code;
        //                                                _trace._id = tracking_code + "_" + _trace.trace_date.ToString("yyMMddHHmmss");
        //                                                _trace.date = _trace.trace_date.ToString("yyyyMMdd");
        //                                                _trace.time = _trace.trace_date.ToString("hhMMss");
        //                                                _trace.description = _row["StatusDesc"].ToString();
        //                                                //_status = _row["Status"].ToString();

        //                                                if (_drDelivery.Length > 0)
        //                                                {

        //                                                    foreach (DataRow _dr in _drDelivery)
        //                                                    {
        //                                                        DeliveryDate = int.Parse(((DateTime)_dr["DeliveryDate"]).ToString("yyyyMMdd"));
        //                                                        if (DeliveryDate == _traceDte && _row["Status"].ToString() == "5")
        //                                                        {

        //                                                            if (!string.IsNullOrEmpty(_dr["isDeliverable"].ToString()) && bool.Parse(_dr["isDeliverable"].ToString()))
        //                                                            {
        //                                                                _isDelevery = true;
        //                                                            }
        //                                                            else
        //                                                            {
        //                                                                _isDelevery = false;
        //                                                            }

        //                                                            if (!string.IsNullOrEmpty(_dr["DeliveryReturn"].ToString()) && bool.Parse(_dr["DeliveryReturn"].ToString()))
        //                                                            {
        //                                                                _return = true;
        //                                                            }
        //                                                            else
        //                                                            {
        //                                                                _return = false;
        //                                                            }


        //                                                            if (_isDelevery && _return)
        //                                                            {
        //                                                                _status = "13";
        //                                                            }
        //                                                            else if (_isDelevery && !_return)
        //                                                            {
        //                                                                _status = "5";
        //                                                            }
        //                                                            else
        //                                                            {
        //                                                                _status = _row["Status"].ToString();
        //                                                            }

        //                                                        }
        //                                                        else
        //                                                        {
        //                                                            _status = _row["Status"].ToString();
        //                                                        }
        //                                                    }
        //                                                }
        //                                                else
        //                                                {
        //                                                    _status = _row["Status"].ToString();
        //                                                }


        //                                                switch (_status)
        //                                                {
        //                                                    case "1":
        //                                                        _status = "C5";
        //                                                        break;
        //                                                    case "2":
        //                                                        _status = "C11";
        //                                                        break;
        //                                                    case "3":
        //                                                        _status = "C12";
        //                                                        break;
        //                                                    case "4":
        //                                                        _status = "C12";
        //                                                        break;
        //                                                    case "5":
        //                                                        _status = "C13";
        //                                                        break;
        //                                                    case "6":
        //                                                        _status = "C15";
        //                                                        break;
        //                                                    case "7":
        //                                                        _status = "C19";
        //                                                        break;
        //                                                    case "8":
        //                                                        _status = "C21";
        //                                                        break;
        //                                                    case "9":
        //                                                        _status = "C14";
        //                                                        break;
        //                                                    case "12":
        //                                                        _status = "C12";
        //                                                        break;
        //                                                    case "13":
        //                                                        _status = "C18";
        //                                                        break;
        //                                                    default:
        //                                                        //_status = "C11";
        //                                                        break;
        //                                                }
        //                                                _trace.code = _status;
        //                                                _trace.pos = _row["POSCode"].ToString();
        //                                                _data.Save("shipment_trace", _trace);


        //                                                dynamic[] _journey = _data.List("LogJourney", Query.And(Query.EQ("Code", _current.tracking_code)
        //                                                    , Query.EQ("Status", _status)));
        //                                                if (_journey != null && _journey.Length > 0)
        //                                                {

        //                                                    if (!string.IsNullOrEmpty(_row["TraceDate"].ToString()))
        //                                                    {
        //                                                        _tracedate = DateTime.Parse(_row["TraceDate"].ToString());
        //                                                        _date = _tracedate.ToString("yyyyMMdd");
        //                                                        _time = _tracedate.ToString("HHmmss");

        //                                                        if (int.Parse(_now) == int.Parse(_date))
        //                                                        {
        //                                                            int _isEq = 0;
        //                                                            for (int j = 0; j < _journey.Length; j++)
        //                                                            {
        //                                                                if (_journey[j].traceDate != null)
        //                                                                    _isEq++;
        //                                                            }
        //                                                            if (_isEq == 0)
        //                                                            {
        //                                                                for (int j = 0; j < _journey.Length; j++)
        //                                                                {

        //                                                                    dynamic objLogJourney = _journey[_journey.Length - 1];

        //                                                                    dynamic _traceDate = new PayID.DataHelper.DynamicObj();
        //                                                                    _traceDate.date = int.Parse(string.IsNullOrEmpty(_date) ? "0" : _date);
        //                                                                    _traceDate.time = int.Parse(string.IsNullOrEmpty(_time) ? "0" : _time);
        //                                                                    objLogJourney.traceDate = _traceDate;
        //                                                                    _data.Save("LogJourney", objLogJourney);

        //                                                                }
        //                                                            }
        //                                                            else
        //                                                            {
        //                                                                for (int j = 0; j < _journey.Length; j++)
        //                                                                {
        //                                                                    if (_journey[j].traceDate != null)
        //                                                                    {
        //                                                                        if (_journey[j].traceDate.time != null)
        //                                                                        {
        //                                                                            if (_journey[j].traceDate.time != int.Parse(_time) && _journey[j].traceDate.date == int.Parse(_date) && _journey[j].Status != "C13" && _journey[j].Status != "C5")
        //                                                                            {

        //                                                                                dynamic objLogJourney = new DynamicObj();
        //                                                                                objLogJourney._id = _data.GetNextSquence("LogJourney");
        //                                                                                objLogJourney.Code = _current.tracking_code;
        //                                                                                objLogJourney.Status = _status;
        //                                                                                objLogJourney.UserId = _current.customer.code.ToString();// _request.CustomerCode.ToString();
        //                                                                                objLogJourney.Location = _row["POSCode"].ToString();
        //                                                                                objLogJourney.Note = "BCCP Update";
        //                                                                                objLogJourney.DateCreate = DateTime.Now;

        //                                                                                dynamic _traceDate = new PayID.DataHelper.DynamicObj();
        //                                                                                _traceDate.date = int.Parse(string.IsNullOrEmpty(_date) ? "0" : _date);
        //                                                                                _traceDate.time = int.Parse(string.IsNullOrEmpty(_time) ? "0" : _time);
        //                                                                                objLogJourney.traceDate = _traceDate;
        //                                                                                _data.Save("LogJourney", objLogJourney);

        //                                                                            }
        //                                                                        }
        //                                                                    }

        //                                                                }
        //                                                            }
        //                                                        }

        //                                                    }
        //                                                }
        //                                                else
        //                                                {
        //                                                    dynamic objLogJourney = new DynamicObj();
        //                                                    objLogJourney._id = _data.GetNextSquence("LogJourney");
        //                                                    objLogJourney.Code = _current.tracking_code;
        //                                                    objLogJourney.Status = _status;
        //                                                    objLogJourney.UserId = _current.customer.code.ToString();// _request.CustomerCode.ToString();
        //                                                    objLogJourney.Location = _row["POSCode"].ToString();
        //                                                    objLogJourney.Note = "BCCP Update";
        //                                                    objLogJourney.DateCreate = DateTime.Now;

        //                                                    dynamic _traceDate = new PayID.DataHelper.DynamicObj();
        //                                                    _traceDate.date = int.Parse(string.IsNullOrEmpty(_date) ? "0" : _date);
        //                                                    _traceDate.time = int.Parse(string.IsNullOrEmpty(_time) ? "0" : _time);
        //                                                    objLogJourney.traceDate = _traceDate;

        //                                                    _data.Save("LogJourney", objLogJourney);
        //                                                }

        //                                            }
        //                                            catch { }
        //                                        }
        //                                    }
        //                                }
        //                                writelog("Kết thúc kiểm tra và lưu trạng thái bảng logJourney lúc " + DateTime.Now.ToString("HHmmss") + " Ngày" + DateTime.Now.ToString("yyyyMMdd"));
        //                                try
        //                                {
        //                                    writelog("Bat dau kiểm tra và lưu trạng thái bảng Shipment va Lading lúc " + DateTime.Now.ToString("HHmmss") + " Ngày" + DateTime.Now.ToString("yyyyMMdd"));
        //                                    if (int.Parse(_now) == int.Parse(_date))
        //                                    {
        //                                        _current.system_status = _status;
        //                                        _lading.Status = _status;
        //                                        dynamic _traceDate = new PayID.DataHelper.DynamicObj();
        //                                        _traceDate.date = int.Parse(string.IsNullOrEmpty(_date) ? "0" : _date);
        //                                        _traceDate.time = int.Parse(string.IsNullOrEmpty(_time) ? "0" : _time);

        //                                        if (_current.traceDate != null)
        //                                        {
        //                                            if (_current.traceDate.time != null && _current.traceDate.date != null)
        //                                            {
        //                                                if (_current.traceDate.time != int.Parse(string.IsNullOrEmpty(_time) ? "0" : _time) && _current.traceDate.date >= int.Parse(string.IsNullOrEmpty(_date) ? "0" : _date) && _status != "C5" && _status != "C13")
        //                                                {
        //                                                    try
        //                                                    {
        //                                                        _current.traceDate.time = int.Parse(string.IsNullOrEmpty(_time) ? "0" : _time);
        //                                                        _lading.traceDate.time = int.Parse(string.IsNullOrEmpty(_time) ? "0" : _time);

        //                                                        _data.Save("shipment", _current);
        //                                                        _data.Save("Lading", _lading);
        //                                                    }
        //                                                    catch (Exception ex)
        //                                                    {
        //                                                        writelog("Có lỗi khi lưu trạng thái khác C13 và C5 vào bảng Shipment và lading, lối: " + ex.ToString());
        //                                                    }
        //                                                }
        //                                                else
        //                                                {
        //                                                    try
        //                                                    {
        //                                                        _current.traceDate = _traceDate;
        //                                                        _lading.traceDate = _traceDate;

        //                                                        _data.Save("shipment", _current);
        //                                                        _data.Save("Lading", _lading);
        //                                                    }
        //                                                    catch (Exception ex)
        //                                                    {
        //                                                        writelog("Có lỗi khi lưu tracedate vào bảng Shipment va lading, lối: " + ex.ToString());
        //                                                    }
        //                                                }
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            try
        //                                            {
        //                                                _current.traceDate = _traceDate;
        //                                                _lading.traceDate = _traceDate;

        //                                                _data.Save("shipment", _current);
        //                                                _data.Save("Lading", _lading);
        //                                            }
        //                                            catch (Exception ex)
        //                                            {
        //                                                writelog("Có lỗi khi lưu tracedate khi chua co thong tin ve tracedate vào bảng Shipment va lading, lối: " + ex.ToString());
        //                                            }
        //                                        }

        //                                    }
        //                                    else
        //                                    {
        //                                        if (_current.system_status != _status)
        //                                        {
        //                                            _current.system_status = _status;
        //                                            _lading.Status = _status;

        //                                            dynamic _traceDate = new PayID.DataHelper.DynamicObj();
        //                                            _traceDate.date = int.Parse(string.IsNullOrEmpty(_date) ? "0" : _date);
        //                                            _traceDate.time = int.Parse(string.IsNullOrEmpty(_time) ? "0" : _time);

        //                                            _current.traceDate = _traceDate;
        //                                            _lading.traceDate = _traceDate;

        //                                            _data.Save("shipment", _current);
        //                                            _data.Save("Lading", _lading);
        //                                        }
        //                                    }
        //                                    writelog("Kết thúc kiểm tra và lưu trạng thái bảng Shipment và Lading lúc " + DateTime.Now.ToString("HHmmss") + " Ngày" + DateTime.Now.ToString("yyyyMMdd"));
        //                                }
        //                                catch (Exception ex) { writelog("Có lỗi trong quá trình lưu trạng thái vào các bảng shipment và Lading. Cụ thể: " + ex.ToString()); }
        //                            }
        //                        }
        //                        catch (Exception ex) { writelog(ex.ToString()); }
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    //return;
        //                    writelog(ex.ToString());
        //                }
        //            }
        //            System.Threading.Thread.Sleep(60000);
        //        }
        //        catch { }
        //    }
        //}
        #endregion
        private static void bccp_data_sync(List<string> tracking_codes)
        {

            int iPage = (tracking_codes.Count + 199) / 200;
            writelog("Ngày " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "- Bắt đầu gọi sang BCCP Api và cập nhật trạng thái");
            for (int i = 0; i < iPage; i++)
            {
                try
                {
                    using (vnpost.bccp.v2.ServiceClient client = new vnpost.bccp.v2.ServiceClient())
                    {
                        try
                        {
                            string[] list = tracking_codes.Skip(i * 200).Take(200).ToArray();
                           
                            DataSet ds = client.GetListItem(list, "CASHPOST", "CASHPOST");
                            DataSet _ds = client.GetListDelivery(list, "CASHPOST", "CASHPOST");

                            DataTable _dtInfo = ds.Tables["DetailItem"];
                            DataTable _dtTrace = ds.Tables["TraceItem"];
                            DataTable _dtInfo2 = ds.Tables["Item"];
                            DataTable _dtDelivery = _ds.Tables["Delivery"];
                            //writelog("Ngày " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "- Kết quả trả về từ BCCP - bảng chi tiết hành trình vận đơn có số bản ghi: " + _dtTrace.Rows.Count.ToString());
                            //writelog("Ngày " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "- Kết quả trả về từ BCCP - bảng chỉ tiêu phát có số bản ghi: " + _dtDelivery.Rows.Count.ToString());
                            foreach (string tracking_code in list)
                            {
                                //writelog("Ngày " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "- Vận đơn " + tracking_code);
                                try
                                {
                                    dynamic _current = _data.Get("shipment", Query.Or(Query.EQ("tracking_code", tracking_code), Query.EQ("RefCode", tracking_code)));
                                    dynamic _lading = _data.Get("Lading", Query.Or(Query.EQ("Code", tracking_code), Query.EQ("RefCode", tracking_code)));
                                    dynamic _info = new PayID.DataHelper.DynamicObj();
                                    string _status = _current.system_status;
                                    DataRow[] _dtInfo2F = _dtInfo2.Select("ItemCode='" + tracking_code + "'");
                                    if (_current != null && _lading != null)
                                    {
                                        foreach (DataRow row in _dtInfo2F)
                                        {
                                            try
                                            {
                                                _info.sender_country = "VN";
                                                _info.receiver_country = "VN";
                                                _info.sender_pos = row["AcceptancePOSCode"].ToString();
                                                _info.receiver_pos = "";
                                                _info.sender_name = row["SenderFullname"].ToString();
                                                _info.receiver_name = row["SenderAddress"].ToString();
                                                _info.weight = row["Weight"];
                                                _info.receiver_name = row["ReceiverFullname"].ToString();
                                                _info.receiver_address = row["ReceiverAddress"].ToString();
                                                _info.receiver_id = row["ReceiverIdentification"].ToString();
                                                _info.receiver_id_issued_date = row["ReceiverIssueDate"].ToString();
                                                _info._id = tracking_code;
                                                try
                                                {
                                                    _info.is_cod = (bool)row["CODPayment"];
                                                }
                                                catch (Exception ex)
                                                {
                                                    writelog("Ngày " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "- Lỗi:" + ex.ToString());
                                                }
                                                _info.executer_order = "";
                                                _info.freight = long.Parse("0" + ((double)row["MainFreight"]).ToString("0"));
                                                _info.cod_freight = long.Parse("0" + ((double)row["ValueAddedServiceFreightTotalFreight"]).ToString("0"));
                                                _info.cod_amount = _current.product.value;
                                                _data.Save("lading_info", _info);
                                                _current.parcel.weight = row["Weight"];


                                                _lading.Weight = _info.weight;
                                                _lading.MainFee = _info.freight;
                                                _lading.CodFee = _info.cod_freight;

                                                break;
                                            }
                                            catch (Exception ex)
                                            {
                                                writelog("Ngày " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "- Lỗi:" + ex.ToString());
                                            }
                                        }
                                        int count = 0;
                                        DataRow[] _dtTraceF = _dtTrace.Select("ItemCode='" + tracking_code + "'", "TraceDate");
                                        // Câu lệnh lấy trạng thái phát hoàn tc
                                        string expression = "ItemCode='" + tracking_code + "' and IsDeliverable = true and DeliveryReturn = true ";
                                        string expressionFail = "ItemCode='" + tracking_code + "' and IsDeliverable = false";
                                        string failturecode = "";
                                        foreach (DataRow _row in _dtTraceF)
                                        {
                                            try
                                            {
                                                dynamic _trace = new PayID.DataHelper.DynamicObj();
                                                _trace.trace_date = (DateTime)_row["TraceDate"];
                                                _trace.tracking_code = tracking_code;
                                                _trace._id = tracking_code + "_" + _trace.trace_date.ToString("yyMMddHHmmss");
                                                _trace.date = _trace.trace_date.ToString("yyyyMMdd");
                                                _trace.time = _trace.trace_date.ToString("hhMMss");
                                                _trace.description = _row["StatusDesc"].ToString();
                                                _status = _row["Status"].ToString();
                                                switch (_status)
                                                {
                                                    case "1":
                                                        _status = "C5";
                                                        break;
                                                    case "2":
                                                        _status = "C11";
                                                        break;
                                                    case "3":
                                                        _status = "C12";
                                                        break;
                                                    case "4":
                                                        _status = "C12";
                                                        break;
                                                    case "5":
                                                        _status = "C13";
                                                        #region Xử lý phát hoàn tc
                                                        DataRow[] _dtDeliveryF = _dtDelivery.Select(expression, "ItemCode");
                                                        if (_dtDeliveryF.Count() > 0)
                                                            _status = "C27";
                                                        #endregion
                                                        break;
                                                    case "6":
                                                        _status = "C15";
                                                        break;
                                                    case "7":
                                                        _status = "C19";
                                                        break;
                                                    case "8":
                                                        _status = "C21";
                                                        break;
                                                    case "9":
                                                        _status = "C14";
                                                        DataRow[] _dtDeliveryFail = _dtDelivery.Select(expressionFail, "DeliveryDate");
                                                        if (_dtDeliveryFail.Count() > 0)
                                                        {
                                                            foreach (DataRow dr in _dtDeliveryFail)
                                                            {
                                                                DateTime _deliverydate = (DateTime)dr["DeliveryDate"];
                                                                if ((long.Parse(_trace.trace_date.ToString("yyMMddHHmmss")) >= long.Parse(_deliverydate.ToString("yyMMddHHmmss"))) && (long.Parse(_trace.date) == long.Parse(_deliverydate.ToString("yyyyMMdd"))))
                                                                    failturecode = dr["CauseCode"].ToString();
                                                            }
                                                        }
                                                        break;
                                                    case "12":
                                                        _status = "C12";
                                                        break;
                                                    default:
                                                        //    _status = _row["Status"].ToString() + " (bccp status)";
                                                        break;
                                                }
                                                _trace.code = _status;
                                                _trace.pos = _row["POSCode"].ToString();
                                                _trace.reason = failturecode;
                                                _data.Save("shipment_trace", _trace);

                                                #region code
                                                count = 0;
                                                IMongoQuery query = Query.And(Query.EQ("Code", _current.tracking_code)
                                       , Query.EQ("Status", _status));
                                                //     dynamic _journey = _data.List("LogJourney", query);
                                                dynamic[] _journey = _data.List("LogJourney", query);

                                                if (_journey == null || _journey.Length <= 0)
                                                {
                                                    if (_status != "C5")
                                                    {
                                                        //Query.EQ("DateCreate", (DateTime)_row["TraceDate"]
                                                        dynamic objLogJourney = new DynamicObj();
                                                        objLogJourney._id = _data.GetNextSquence("LogJourney");
                                                        objLogJourney.Code = _current.tracking_code;
                                                        objLogJourney.Status = _status;
                                                        objLogJourney.UserId = _current.customer.code.ToString();
                                                        objLogJourney.Location = _row["POSCode"].ToString();
                                                        objLogJourney.Note = "BCCP Update";
                                                        objLogJourney.DateCreate = (DateTime)_row["TraceDate"];
                                                        if(_status=="C14")
                                                        {
                                                            objLogJourney.reason = failturecode;
                                                        }
                                                        _data.Save("LogJourney", objLogJourney);
                                                    }

                                                }
                                                else
                                                {
                                                    DateTime datetime = (DateTime)_row["TraceDate"];
                                                    foreach (dynamic ite in _journey)
                                                    {
                                                        if ((DateTime)ite.DateCreate == datetime.ToUniversalTime())
                                                        {
                                                            count++;
                                                        }
                                                    }

                                                    if (count == 0)
                                                    {
                                                        if (_status != "C5")
                                                        {
                                                            //Query.EQ("DateCreate", (DateTime)_row["TraceDate"]
                                                            dynamic objLogJourney = new DynamicObj();
                                                            objLogJourney._id = _data.GetNextSquence("LogJourney");
                                                            objLogJourney.Code = _current.tracking_code;
                                                            objLogJourney.Status = _status;
                                                            objLogJourney.UserId = _current.customer.code.ToString();
                                                            objLogJourney.Location = _row["POSCode"].ToString();
                                                            objLogJourney.Note = "BCCP Update";
                                                            objLogJourney.DateCreate = datetime;
                                                            if (_status == "C14")
                                                            {
                                                                objLogJourney.reason = failturecode;
                                                            }
                                                            _data.Save("LogJourney", objLogJourney);
                                                        }
                                                    }
                                                }


                                            }
                                            catch (Exception ex)
                                            {
                                                writelog("Ngày " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "- Lỗi xảy ra khi cập nhật trạng thái bản LogJourney:" + ex.ToString());
                                            }

                                                #endregion
                                        }
                                        try
                                        {
                                            if (_status != _current.system_status)
                                            {
                                                _current.system_status = _status;
                                                _lading.Status = _status;
                                                _data.Save("shipment", _current);
                                                _data.Save("Lading", _lading);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            writelog("Ngày " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "- Lỗi cập nhật trạng thái bảng shipment và Lading:" + ex.ToString());
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    writelog("Ngày " + DateTime.Now.ToString("dd/MM/yyyy  HH:mm:ss") + "- Lỗi kết nối db Mongo: " + ex.ToString());
                                }
                            }
                            writelog("Ngày " + DateTime.Now.ToString("dd/MM/yyyy  HH:mm:ss") + "- Kết thúc cập nhật 200 bản ghi");
                        }
                        catch (Exception ex)
                        {
                            writelog("Ngày " + DateTime.Now.ToString("dd/MM/yyyy  HH:mm:ss") + "- Lỗi kết nối sang BCCP:" + ex.ToString());
                        }
                    }

                    int time_sleep = int.Parse(ConfigurationSettings.AppSettings["DATASYNC_TIME_SLEEP"].ToString());
                    time_sleep = time_sleep * 60000;
                    System.Threading.Thread.Sleep(time_sleep);
                }
                catch(Exception ex)
                {
                    writelog("Ngày " + DateTime.Now.ToString("dd/MM/yyyy  HH:mm:ss") + "- Lỗi: danh sách vận đơn bị null hoặc kết nối db Mongo bị ngắt - " + ex.ToString());
                }
            }
            writelog("Ngày " + DateTime.Now.ToString("dd/MM/yyyy  HH:mm:ss") + "- Kết thúc cập nhật");
        }
        public static void writelog(string strLogText)
        {
            //create folder
            string FolderPath = pathLog;
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
                path = FolderPathDate + @"\SrvCore" + string.Format("{0:ddMMyyyy}", DateTime.Now) + ".txt";
            else
                path = @"C:\Log" + string.Format("{0:ddMMyyyy}", DateTime.Now) + ".txt";
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
