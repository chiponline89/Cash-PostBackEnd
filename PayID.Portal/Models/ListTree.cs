using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayID.Portal.Models
{
    public class ListTree
    {
        public string UnitCode { get; set; }

        public string UnitParent { get; set; }

        public string UnitLink { get; set; }

        public string UnitName { get; set; }

        public string ProvinceCode { get; set; }
    }
}