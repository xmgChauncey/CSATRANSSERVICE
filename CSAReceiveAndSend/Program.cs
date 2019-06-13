using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace CSAReceiveAndSend
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            //ServiceBase[] ServicesToRun;
            //ServicesToRun = new ServiceBase[]
            //{
            //    new CSAReceiveAndSendService()
            //};
            //ServiceBase.Run(ServicesToRun);

            CSAReceiveAndSendService csaReceiveAndSendService = new CSAReceiveAndSendService();
            csaReceiveAndSendService.OnStart();
        }
    }
}
