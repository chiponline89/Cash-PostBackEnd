﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayID.Portal
{
    public class Configuration
    {
        public static PayID.DataHelper.MongoHelper Data, Data_S24;
        public static void Init()
        {
            Data = new DataHelper.MongoHelper(
                System.Configuration.ConfigurationManager.AppSettings["CORE_DB_SERVER"],
                System.Configuration.ConfigurationManager.AppSettings["CORE_DB_DATABASE"]
                );
            Data_S24 = new DataHelper.MongoHelper(
                System.Configuration.ConfigurationManager.AppSettings["MERCHANT_DB_SERVER"],
                System.Configuration.ConfigurationManager.AppSettings["MERCHANT_DB_DATABASE"]
                );
        }
    }
}