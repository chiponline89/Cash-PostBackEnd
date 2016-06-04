using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayID.Portal.Areas.Systems
{
    public class Configuration
    {
        public static PayID.DataHelper.MongoHelper Data;
        public static void Init()
        {
            Data = new DataHelper.MongoHelper(
                System.Configuration.ConfigurationManager.AppSettings["SYSTEM_CFG_DB_SERVER"],
                System.Configuration.ConfigurationManager.AppSettings["SYSTEM_CFG_DB_DATABASE"]
                );
        }
    }
}