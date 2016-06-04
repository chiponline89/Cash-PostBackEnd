using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayID.Portal.Common.Log
{
    public interface ILoggerGeneral
    {
        void Write(FormatterBase formatter, string logFileName);
    }
}