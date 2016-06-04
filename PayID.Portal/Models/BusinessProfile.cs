using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayID.Portal.Models
{
    public class BusinessProfile
    {
        public long Id { get; set; }

        public string GeneralEmail { get; set; }

        public string GeneralSystem { get; set; }

        public string GeneralAccountType { get; set; }

        public string GeneralShortName { get; set; }

        public string GeneralFullName { get; set; }

        public string BusinessTax { get; set; }

        public string BusinessWebsite { get; set; }

        public string ContactName { get; set; }

        public string Address { get; set; }

        public int ProvinceId { get; set; }

        public int DistrictId { get; set; }

        public int WardId { get; set; }

        public string ContactPhoneMobile { get; set; }

        public string ContactPhoneWork { get; set; }
    }
}