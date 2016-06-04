using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayID.Portal.Areas.Lading
{
    public class Configuration
    {
        public static PayID.DataHelper.MongoHelper Data;
        public static PayID.DataHelper.MongoHelper Data_core;
        public static string urlAPI = "";
        public static void Init()
        {
            Data = new DataHelper.MongoHelper(
                System.Configuration.ConfigurationManager.AppSettings["LADING_DB_SERVER"],
                System.Configuration.ConfigurationManager.AppSettings["LADING_DB_DATABASE"]
                );
            Data_core = new DataHelper.MongoHelper(
                System.Configuration.ConfigurationManager.AppSettings["CORE_DB_SERVER"],
                System.Configuration.ConfigurationManager.AppSettings["SERVICE_REQUEST_DB_DATABASE"]
                );
        }    
    }
}