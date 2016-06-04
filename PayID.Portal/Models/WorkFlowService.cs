using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayID.Portal.Models
{
    public class WorkFlowService
    {
        public string Id { get; set; }

        public string UnitCode { get; set; }

        public string RequestService { get; set; }

        public string RequestType { get; set; }

        public string Steps { get; set; }

        public string StepName { get; set; }
    }
}