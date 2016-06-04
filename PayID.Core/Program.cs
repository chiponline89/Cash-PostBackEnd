using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace PayID.Core
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            //CoreService core = new CoreService();
            //ServiceBase[] ServicesToRun;
            //ServicesToRun = new ServiceBase[] 
            //{ 
            //    new CoreService() 
            //};
            //ServiceBase.Run(ServicesToRun);
            CoreService core = new CoreService();
            core.ManualStart();
            Console.ReadLine();
        }
    }
}
