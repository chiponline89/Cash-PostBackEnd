using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayID.Portal.Models
{
    public class Unit
    {
        public string UnitCode { get; set; }

        public string UnitName { get; set; }

        public string ParentUnitCode { get; set; }

        public string CommuneCode { get; set; }

        public string DistrictCode { get; set; }

        public string ProvinceCode { get; set; }

        public string UnitTypeCode { get; set; }
    }
}