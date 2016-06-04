using MongoDB.Bson;
using MongoDB.Driver.Builders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PayID.Partner
{
    partial class VnPOSTDataSync : ServiceBase
    {
        private PayID.DataHelper.MongoHelper _data;
        Thread bccp, cod, push;
        public VnPOSTDataSync()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {

            // TODO: Add code here to start your service.
        }

        public void manualStart()
        {
            _data = new DataHelper.MongoHelper(
                ConfigurationSettings.AppSettings["CORE_DB_SERVER"],
                ConfigurationSettings.AppSettings["CORE_DB_DATABASE"]
                );
            bccp = new Thread(new ThreadStart(BCCPDataSync));
            cod = new Thread(new ThreadStart(CODDataSync));
            push = new Thread(new ThreadStart(PushPartnerSync));
            bccp.Start();
            //cod.Start();
            //push.Start();
            Console.ReadLine();
        }

        public void manualStop()
        {

            try
            {
                bccp.Abort();
            }
            catch { }
            try { cod.Abort(); }
            catch { }
        }
        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
        }

        private void BCCPDataSync()
        {
            int time_sleep = int.Parse(ConfigurationSettings.AppSettings["BCCP_DATASYNC_TIME_SLEEP"].ToString());
            time_sleep = time_sleep * 60000;

            //while (true)
            //{
                int iPage = 1;
                long lTotal=0;
                long iTotalPage = 1;
                //while (iPage < iTotalPage)
                //{
                //    iPage += 1;
                    dynamic[] _unclosed = _data.List("shipment",
                        Query.And(
                        Query.NE("system_status", "C2"), Query.NE("system_status", "C8")));
                        //MongoDB.Driver.Builders.SortBy.Ascending("_id"), 200, iPage, out lTotal);
                  //  iTotalPage = (lTotal + 199) / 200;
                    List<string> list = new List<string>();
                    foreach (dynamic _uc in _unclosed)
                        if (!String.IsNullOrEmpty(_uc.tracking_code))
                        {
                            //bccp_data_sync(_uc.tracking_code);
                            list.Add(_uc.tracking_code);
                        }
                    bccp_data_sync_v2(list);
                    System.Threading.Thread.Sleep(time_sleep);
                //}
                
            //}
        }

        private void PushPartnerSync()
        {
            while (true)
            {
                dynamic[] apis = _data.List("api_key", null);
                DateTime dt = DateTime.Now;
                foreach (dynamic api in apis) 
                if(api.realtime_pushing)
                {
                    if (api.last_synced_time == null) api.last_synced_time = dt.AddDays(-30);
                    pushpartner(api.code, api.business_profile.ToString(), api.last_synced_time);
                    api.last_synced_time = dt;
                    _data.UpdateObject("api_key",
                        Query.EQ("_id",api._id)
                        ,Update.Set("last_synced_time",dt));
                }
                System.Threading.Thread.Sleep(300000);
            }
        }

        private void pushpartner(string partnerCode, string partnerId, DateTime dt)
        {
            Hashtable status_mapping = new Hashtable();
            dynamic[] status_list = _data.List("partner_mapping_status", MongoDB.Driver.Builders.Query.EQ("partner", long.Parse(partnerId)));
            foreach (dynamic _status in status_list)
                status_mapping.Add(_status.system_status, _status.partner_status);
            PayID.Intergration.IPushingPartner partner = PayID.Intergration.Provider.GetPartner(partnerCode);
            dynamic[] _unclosed = _data.List("shipment", 
                MongoDB.Driver.Builders.Query.And(
                MongoDB.Driver.Builders.Query.EQ("customer.code", partnerId),
                MongoDB.Driver.Builders.Query.EQ("system_status", "ACCEPTED"))
                );
            foreach (dynamic shipment in _unclosed)
                if (DateTime.Parse(shipment.system_last_updated_time).ToLocalTime() > dt.ToLocalTime())
            {
                if (status_mapping.ContainsKey(shipment.system_status)) shipment.system_status = status_mapping[shipment.system_status];
                partner.Push(shipment);
            }
        }
        private void CODDataSync()
        {
            int time_sleep = int.Parse(ConfigurationSettings.AppSettings["BCCP_DATASYNC_TIME_SLEEP"].ToString());
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

        private void cod_data_sync(string tracking_code)
        {
            try
            {
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
        private void bccp_data_sync_v2(List<string> tracking_codes)
        {
            int iPage = (tracking_codes.Count + 199) / 200;
            for (int i = 0; i < iPage; i++)
            {
                long _f = long.Parse(DateTime.Now.ToString("ddHHmmss"));
                using (vnpost.bccp.v2.ServiceClient client = new vnpost.bccp.v2.ServiceClient())
                {
                    try
                    {
                        string[] list = tracking_codes.Skip(i*200).Take(200).ToArray();
                        DataSet ds = client.GetListItem(list, "CASHPOST", "CASHPOST");

                        long _t = long.Parse(DateTime.Now.ToString("ddHHmmss"));
                        Console.WriteLine("Query: " + _f + " - " + _t + " = " + (_t - _f));
                        DataTable _dtInfo = ds.Tables["DetailItem"];
                        DataTable _dtTrace = ds.Tables["TraceItem"];
                        DataTable _dtInfo2 = ds.Tables["Item"];
                        //if (_dtInfo == null || _dtInfo.Rows.Count == 0) return;
                        foreach (string tracking_code in list)
                        {
                            //if (tracking_code == "EK717004796VN")
                            //{

                            //}
                            dynamic _current = _data.Get("shipment", Query.EQ("tracking_code", tracking_code));
                            dynamic _lading = _data.Get("Lading", Query.EQ("Code", tracking_code));
                            dynamic _info = new PayID.DataHelper.DynamicObj();
                            string _status = _current.system_status;
                            foreach (DataRow row in _dtInfo2.Rows)
                                if (row["ItemCode"].ToString() == tracking_code)
                                {
                                    try
                                    {
                                        _info.sender_country = "VN";
                                        _info.receiver_country = "VN";
                                        _info.sender_pos = row["AcceptancePOSCode"].ToString();
                                        _info.receiver_pos = "";// _dtInfo.Rows[0]["BC_PHAT"].ToString();
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
                                        catch { }
                                        _info.executer_order = "";// row["ExecuteOrder"].ToString();
                                        _info.freight = long.Parse("0"+((double)row["MainFreight"]).ToString("0"));
                                        _info.cod_freight = long.Parse("0"+((double)row["ValueAddedServiceFreightTotalFreight"]).ToString("0"));
                                        _info.cod_amount = _current.product.value;
                                        _data.Save("lading_info", _info);
                                        _current.parcel.weight = row["Weight"];
                                        //_current.parcel.height = row["Height"];
                                        //_current.parcel.length = row["Length"];
                                        //_current.parcel.width = row["Width"];


                                        _lading.Weight = _info.weight;
                                        _lading.MainFee = _info.freight;
                                        _lading.CodFee = _info.cod_freight;

                                        break;
                                    }
                                    catch { }
                                }
                            foreach (DataRow _row in _dtTrace.Rows)
                                if (_row["ItemCode"].ToString() == tracking_code)
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
                                                _status = "C11";
                                                break;
                                            case "2":
                                                _status = "C12";
                                                break;
                                            case "3":
                                                _status = "C13";
                                                break;
                                            case "4":
                                                _status = "C15";
                                                break;
                                            case "12":
                                                _status = "C12";
                                                break;
                                            case "9":
                                                _status = "C8";
                                                break;
                                            case "5":
                                                _status = "C19";
                                                break;
                                            default:
                                                //_status = "C11";
                                                break;
                                        }
                                        _trace.code = _status;
                                        _trace.pos = _row["POSCode"].ToString();
                                        _data.Save("shipment_trace", _trace);
                                    }
                                    catch { }
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
                            catch { }
                        }
                    }
                    catch
                    {
                        //return;
                    }
                }
            }
        }
        private void bccp_data_sync(string tracking_code)
        {
            long _f = long.Parse(DateTime.Now.ToString("ddHHmmss"));
            using (vnpost.bccp.v2.ServiceClient client = new vnpost.bccp.v2.ServiceClient())
            {
                try
                {
                    DataSet ds = client.GetListItem(new string[] { tracking_code }, "CASHPOST", "CASHPOST");
                
                long _t = long.Parse(DateTime.Now.ToString("ddHHmmss"));
                Console.WriteLine("Query " + tracking_code + ": " + _f + " - " + _t + " = " + (_t - _f));
                DataTable _dtInfo = ds.Tables["DetailItem"];
                if (_dtInfo == null || _dtInfo.Rows.Count == 0) return;
                dynamic _current = _data.Get("shipment", Query.EQ("tracking_code", tracking_code));
                dynamic _info = new PayID.DataHelper.DynamicObj();

                _current.parcel.weight = _dtInfo.Rows[0]["Weight"];
                _current.parcel.height = _dtInfo.Rows[0]["Height"];
                _current.parcel.length = _dtInfo.Rows[0]["Length"];
                _current.parcel.width = _dtInfo.Rows[0]["Width"];
                _data.Save("shipment", _current);

                _dtInfo = ds.Tables["Item"];
                
                _info.sender_country = "VN";
                _info.receiver_country = "VN";
                _info.sender_pos = _dtInfo.Rows[0]["AcceptancePOSCode"].ToString();
                _info.receiver_pos = "";// _dtInfo.Rows[0]["BC_PHAT"].ToString();
                _info.sender_name = _dtInfo.Rows[0]["SenderFullname"].ToString();
                _info.receiver_name = _dtInfo.Rows[0]["SenderAddress"].ToString();
                _info.weight = _dtInfo.Rows[0]["Weight"];
                _info.receiver_name = _dtInfo.Rows[0]["ReceiverFullname"].ToString();
                _info.receiver_address = _dtInfo.Rows[0]["ReceiverAddress"].ToString();
                _info.receiver_id = _dtInfo.Rows[0]["ReceiverIdentification"].ToString();
                _info.receiver_id_issued_date = _dtInfo.Rows[0]["ReceiverIssueDate"].ToString();
                _info._id = tracking_code;
                _info.is_cod = (bool)_dtInfo.Rows[0]["CODPayment"];
                _info.executer_order = "";// _dtInfo.Rows[0]["ExecuteOrder"].ToString();
                _info.freight = _dtInfo.Rows[0]["MainFreight"].ToString();
                _info.cod_freight = _dtInfo.Rows[0]["VATFreight"].ToString();
                _info.cod_amount = _current.product.value;
                _data.Save("lading_info", _info);

                DataTable _dtTrace = ds.Tables["TraceItem"];
                foreach (DataRow _row in _dtTrace.Rows)
                {
                    dynamic _trace = new PayID.DataHelper.DynamicObj();
                    _trace.trace_date = (DateTime)_row["TraceDate"];
                    _trace.tracking_code = tracking_code;
                    _trace._id = tracking_code + "_" + _trace.trace_date.ToString("yyMMddHHmmss");
                    _trace.date = _trace.trace_date.ToString("yyyyMMdd");
                    _trace.time = _trace.trace_date.ToString("hhMMss");
                    _trace.description = _row["StatusDesc"];
                    _trace.code = _row["Status"];
                    _trace.pos = _row["POSCode"];
                    _data.Save("shipment_trace", _trace);
                }
                }
                catch
                {
                    return;
                }
            }
        }
    }
}
