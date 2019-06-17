using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Configuration;

namespace  CSAReceiveAndSend

{
    public class LogHelper
    {
        private  static EventLogEntryType traceLevel = (EventLogEntryType)Enum.Parse(typeof(EventLogEntryType), System.Configuration.ConfigurationManager.AppSettings["TraceLevel"]);
        private string eventLogName = ConfigurationManager.AppSettings["EventLogName"].ToString();
        public void WriteEventLog(EventLogEntryType logType, string desc)
        {
            //只在错误级别大于配置文件设定的级别才允许记录日志
            if (int.Parse(logType.ToString("D")) <= int.Parse(traceLevel.ToString("D")))
            {
                if (!EventLog.Exists(eventLogName))
                {
                    EventLog.CreateEventSource(eventLogName, eventLogName);
                }
                System.Diagnostics.EventLog log = new System.Diagnostics.EventLog(eventLogName, ".", eventLogName);
                log.ModifyOverflowPolicy(OverflowAction.OverwriteAsNeeded, 7);
                log.WriteEntry(desc, logType);
            }
        }


        public void StopService(string strServiceName)
        {
            System.ServiceProcess.ServiceController sc = new System.ServiceProcess.ServiceController();
            //DataSendingService
            sc.ServiceName = strServiceName;
            if (sc.Status == System.ServiceProcess.ServiceControllerStatus.Running)
                sc.Stop();
        }
    }
}
