using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayID.Portal.Models
{
    public class UpFileLading
    {
        public List<Lading> ListLading { get; set; }

        public FileUp FileUp { get; set; }
    }
}