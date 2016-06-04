using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayID.Portal.Models
{
    public class Store
    {
        public long Id { get; set; }

        public string CustomerCode { get; set; }

        public string StoreCode { get; set; }

        public string StoreName { get; set; }

        public string Address { get; set; }

        public string ManagerName { get; set; }

        public string ManagerMobile { get; set; }

        public string ManagerEmail { get; set; }

        public string ProvinceCode { get; set; }

        public string DistrictCode { get; set; }

        public int Default { get; set; }

        public string PostCode { get; set; }
    }
}