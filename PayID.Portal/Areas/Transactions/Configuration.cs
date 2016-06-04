using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayID.Portal.Areas.Transactions
{
    public class Configuration
    {
        public static PayID.DataHelper.MongoHelper Data;

        public static void Init()
        {
            Data = new DataHelper.MongoHelper(
                System.Configuration.ConfigurationManager.AppSettings["TRANSACTION_DB_SERVER"],
                System.Configuration.ConfigurationManager.AppSettings["TRANSACTION_DB_DATABASE"]
                );
        }
    }
}