using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace PayID.Portal.Common.Log
{
    public class LoggerGeneral : ILoggerGeneral
    {
        public void Write(FormatterBase formatter, string logFileName)
        {
            TextWriterTraceListener listener = new TextWriterTraceListener(logFileName);
            listener.WriteLine(formatter.Message);
            listener.Flush();
            listener.Close();
        }
    }
}