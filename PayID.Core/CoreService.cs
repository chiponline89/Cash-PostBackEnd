using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace PayID.Core
{
    public partial class CoreService : ServiceBase
    {
        public static PayID.DataHelper.MongoHelper Data = null;
        public CoreService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            ManualStart();
        }

        public void ManualStart()
        {
            if(Data==null)
            {
                Data = new DataHelper.MongoHelper(
                    System.Configuration.ConfigurationManager.AppSettings["NOTIFICATION_SERVER"],
                    System.Configuration.ConfigurationManager.AppSettings["NOTIFICATION_DATABASE"]);
                Helper.Mail_Server = System.Configuration.ConfigurationManager.AppSettings["Mail_Server"];
                Helper.Mail_Port = int.Parse(System.Configuration.ConfigurationManager.AppSettings["Mail_Port"]);
                Helper.Mail_Is_SSL = System.Configuration.ConfigurationManager.AppSettings["Mail_Is_SSL"] == "true";
                Helper.Mail_From = System.Configuration.ConfigurationManager.AppSettings["Mail_From"]; ;
                Helper.Mail_User_Id = System.Configuration.ConfigurationManager.AppSettings["Mail_User_Id"]; ;
                Helper.Mail_User_Password = System.Configuration.ConfigurationManager.AppSettings["Mail_User_Password"];
            }
            //NotificationProcessing.OnStart();
            Partner.VnPOSTDataSyncing.OnStart();
        }

        protected override void OnStop()
        {
            NotificationProcessing.OnStop();
            Partner.VnPOSTDataSyncing.OnStop();
        }
    }
}
