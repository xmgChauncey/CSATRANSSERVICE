using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace CSATRANSSERVICE
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            string isDebug= ConfigurationManager.AppSettings["IsDebug"].ToString();

            if(isDebug.Equals("true"))
            {
                CsaService csaService = new CsaService();
                csaService.OnStart();
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new CsaService()
                };
                ServiceBase.Run(ServicesToRun);
            }

        }
    }
}
