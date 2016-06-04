using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayID.Portal.Areas.Report
{
    public class Configuration
    {
        public static PayID.DataHelper.MongoHelper Data;
        public static dynamic[] LIST_STATUS;
        public static void Init()
        {
            Data = new DataHelper.MongoHelper(
                System.Configuration.ConfigurationManager.AppSettings["REPORT_DB_SERVER"],
                System.Configuration.ConfigurationManager.AppSettings["REPORT_DB_DATABASE"]
                );


        }


    }
}