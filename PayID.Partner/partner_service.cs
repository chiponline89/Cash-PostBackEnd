using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Drawing;
using MongoDB.Driver.Builders;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using MongoDB.Driver;
using PayID.DataHelper;
namespace PayID.Partner
{
    public partial class partner_service : ServiceBase
    {
        Thread myThread;      
        public partner_service()
        {
            Process();           
        }

        protected override void OnStart(string[] args)
        {
            myThread = new Thread(new ThreadStart(Process));
            myThread.Start();
        }

        protected override void OnStop()
        {
            myThread.Abort();
        }

        protected void Process()
        {
            GET_POD_FROM_BCCP();
        }
        #region Get POD From BCCP
        protected void GET_POD_FROM_BCCP()
        {
            DataSet ds = new DataSet();
            try
            {
              //  dynamic myObj = PayID.Portal.Areas.Lading.Configuration.Data.Get("AutoGenPreCode", Query.EQ("Type", type));

                IMongoQuery _query = Query.NE("StatusCode","SUCCESSFUL");
                bccp_ws.PaycodeSoapClient sv = new bccp_ws.PaycodeSoapClient();
                PayID.DataHelper.DynamicObj[] lstJourney = PayID.Portal.Areas.Lading.Configuration.Data.List("LogJourney",_query);                
                foreach(dynamic journey in lstJourney)
                {
                    ds = sv.ProccessTrackingInfo(journey.Code);

                    dynamic dObject = new DynamicObj();
                    
                }

                //my_Querry.DateDetail = Convert.ToString(myDT.Rows[0]["DATE_"]);
                //my_Querry.TimeDetail = Convert.ToString(myDT.Rows[0]["TIMEDETAIL"]);
                //my_Querry.StatusText = Convert.ToString(myDT.Rows[0]["Statustext"]);
                //my_Querry.Location = Convert.ToString(myDT.Rows[0]["VI_TRI"]);
                
                    //objLogJourney._id = MongoHelper.GetNextSquence("LogJourney");
                    //objLogJourney.Code = _request.Code;
                    //objLogJourney.Status = "ACCEPT_REQUEST";
                    //objLogJourney.UserId = _request.CustomerCode.ToString();
                    //objLogJourney.Location = "Cast@Post";
                    //objLogJourney.Note = "Tạo vận đơn thành công.";
                    //objLogJourney.DateCreate = DateTime.Now;
                    //MongoHelper.Insert("LogJourney", objLogJourney);
                // 15p
                // Thread.Sleep(900000);
            }
            catch (Exception)
            {
            
            }
        }
        #endregion
        #region StopService
        void StopService()
        {
            ServiceController service = new ServiceController("PartnerService");
            service.Stop();
            service.WaitForStatus(ServiceControllerStatus.Stopped);
        }
        #endregion

        public string Session { get; set; }
    }
}
