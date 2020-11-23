using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace FrpWindowsService
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            /*  ServiceBase[] ServicesToRun;
              ServicesToRun = new ServiceBase[]
              {
                  new Service1()
              };
              ServiceBase.Run(ServicesToRun);*/
            Service1 service1 =   new Service1();
            service1.OnStart();
        }
    }
}
