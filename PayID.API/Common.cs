using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayID.API
{
    public class Log
    {
        public static string Module = "api_log";
        public static List<dynamic> Logs = new List<dynamic>();

        public static void Start()
        {
            sendLogThread = new System.Threading.Thread(new System.Threading.ThreadStart(sendLog));
            sendLogThread.Start();
        }

        private static System.Threading.Thread sendLogThread;
        static void sendLog()
        {
            while (true)
            {
                try
                {
                    dynamic[] logToWrite = Logs.Take(100).ToArray();
                    foreach (dynamic log in logToWrite)
                    {
                        Logs.Remove(log);
                        PayID.Portal.Areas.ServiceRequest.Configuration.Data.SaveDynamic("api_log", log);
                    }
                    System.Threading.Thread.Sleep(100);
                }
                catch { }
            }
        }
    }
}