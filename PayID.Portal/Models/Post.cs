using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayID.Portal.Models
{
    public class Post
    {
        public int Id { get; set; }

        public string PosCode { get; set; }

        public string PosName { get; set; }

        public string Address { get; set; }

        public string CommuneCode { get; set; }

        public string Tel { get; set; }

        public string Fax { get; set; }

        public int PosTypeCode { get; set; }

        public string ProvinceCode { get; set; }

        public string DistrictCode { get; set; }

        public string PosLevelCode { get; set; }

        public string AddressCode { get; set; }

        public string Status { get; set; }

        public string UnitCode { get; set; }

        public bool IsDispatch { get; set; }

        public bool IsWorkflowUnit { get; set; }
    }
}