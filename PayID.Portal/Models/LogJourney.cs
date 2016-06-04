using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayID.Portal.Models
{
    public class LogJourney
    {
        public string Code { get; set; }

        public string Status { get; set; }

        public string UserId { get; set; }

        public string Location { get; set; }

        public string Note { get; set; }

        public DateTime DateCreate { get; set; }
    }
}