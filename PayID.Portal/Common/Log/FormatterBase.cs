using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayID.Portal.Common.Log
{
    public class FormatterBase
    {
        protected FormatterBase() { }

        public virtual string Message
        {
            get
            {
                return "";
            }
        }
    }
}