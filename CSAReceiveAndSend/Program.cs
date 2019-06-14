using System;
using System.Collections.Generic;
using System.Configuration;
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
            string isDebug = ConfigurationManager.AppSettings["IsDebug"].ToString();
            if(isDebug.Equals("true"))
            {
                CSAReceiveAndSendService csaReceiveAndSendService = new CSAReceiveAndSendService();
                csaReceiveAndSendService.OnStart();
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new CSAReceiveAndSendService()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
