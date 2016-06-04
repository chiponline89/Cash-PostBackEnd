using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayID.Portal.Models
{
    public class Account
    {
        public int Id { get; set; }

        public string Amnd_User { get; set; }

        public string UnitCreate { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string FullName { get; set; }

        public string Role { get; set; }

        public string Address { get; set; }

        public string PhoneNumber { get; set; }

        public string UnitCode { get; set; }

        public string UnitName { get; set; }

        public string UserOfficer { get; set; }

        public string UnitLink { get; set; }

        public string Status { get; set; }

        public string Unit_Type { get; set; }
    }
}