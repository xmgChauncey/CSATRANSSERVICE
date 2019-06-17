using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Messaging;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;

namespace CSAReceiveAndSend
{
    public partial class CSAReceiveAndSendService : ServiceBase
    {
        /// <summary>
        /// 日志记录
        /// </summary>
        private LogHelper log = new LogHelper();

        /// <summary>
        /// 线程
        /// </summary>
        private Thread threadOperate;

        private static int DelayTime = int.Parse(System.Configuration.ConfigurationManager.AppSettings["DelayTime"]);
        public CSAReceiveAndSendService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //延时，防止因其他相关服务如sqlserver等未启动完导致程序报错
            System.Timers.Timer t = new System.Timers.Timer(DelayTime);
            t.Elapsed += new System.Timers.ElapsedEventHandler(Timer_Click);
            t.AutoReset = false;
            t.Enabled = true;
        }

        public void OnStart()
        {
            //todo something
            BeginService();

        }

        private void Timer_Click(Object sender, ElapsedEventArgs e)
        {
            BeginService();
        }

        protected override void OnStop()
        {
            
        }

        public void BeginService()
        {
            try
            {
                //todo something
                threadOperate = new Thread(new ThreadStart(ReceiveAndSend));
                threadOperate.Start();
                log.WriteEventLog(EventLogEntryType.Information, "CSAReceiveAndSendService start successfully");
            }
            catch (Exception e)
            {
                log.WriteEventLog(EventLogEntryType.Information, "CSAReceiveAndSendService start failed");
            }
        }

        private void ReceiveAndSend()
        {
            while (true)
            {
                try
                {
                    //接收企业CSA01报文的msmq通道 
                    string receivMq = ConfigurationManager.AppSettings["ReceiveMq"].ToString();

                    //发送总署版CSA01报文的msmq通道
                    string sendMq = ConfigurationManager.AppSettings["SendMq"].ToString();

                    //MSMQ数据格式
                    string messageType = ConfigurationManager.AppSettings["MessageType"].ToString();
                    ReceiveAndSendTransaction(messageType, receivMq, sendMq);                  
                    Thread.CurrentThread.Join(100);
                }
                catch(Exception ex)
                {
                    string desc = Thread.CurrentThread.Name+ ex.StackTrace + ex.Message;
                    log.WriteEventLog(EventLogEntryType.Error, desc);
                    continue;
                }
            }
        }

        /// <summary>
        /// Method: ReceiveAndSendTransaction
        /// Description:从MSMQ接收数据并发送到另一个MSMQ，这个过程是事务性的
        /// Author: Xiecg
        /// Date: 2019/06/13
        /// Parameter: messageFormatter MSMQ中数据的数据格式Xml(XmlMessageFormatter)、Binary(BinaryMessageFormatter)、ActiveX(ActivXMessageFormatter)
        /// Parameter: receiveMqAddress 接收数据的MSMQ地址
        /// Parameter: sendMqAddress 发送数据的MSMQ地址
        /// Returns: bool 成功时返回true，失败时返回false
        ///</summary>
        public bool ReceiveAndSendTransaction(string messageFormatter, string receiveMqAddress, string sendMqAddress)
        {
            Message transMessage = new Message();
            MsmqOperate receiveMsmqOperate = new MsmqOperate();
            MsmqOperate sendMsmqOperate = new MsmqOperate();

            try
            {
                if (receiveMsmqOperate.ConnectMsmq(receiveMqAddress) && sendMsmqOperate.ConnectMsmq(sendMqAddress))
                {
                    try
                    {
                        receiveMsmqOperate.MqTransaction.Begin();
                        transMessage = receiveMsmqOperate.Queue.Receive();
                        switch (messageFormatter)
                        {
                            case "Xml":
                                transMessage.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
                                break;
                            case "Binary":
                                transMessage.Formatter = new BinaryMessageFormatter();
                                break;
                            case "ActiveX":
                                transMessage.Formatter = new ActiveXMessageFormatter();
                                break;
                        }
                        try
                        {
                            sendMsmqOperate.MqTransaction.Begin();
                            sendMsmqOperate.Queue.Send(transMessage, sendMsmqOperate.MqTransaction);
                            sendMsmqOperate.MqTransaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            string desc = Thread.CurrentThread.Name + ex.StackTrace + ex.Message;
                            log.WriteEventLog(EventLogEntryType.Error, desc);
                            sendMsmqOperate.MqTransaction.Abort();
                            throw ex;
                        }
                        receiveMsmqOperate.MqTransaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        string desc = Thread.CurrentThread.Name + ex.StackTrace + ex.Message;
                        log.WriteEventLog(EventLogEntryType.Error, desc);
                        receiveMsmqOperate.MqTransaction.Abort();
                        return false;
                        throw ex;
                    }
                }
            }
            catch (Exception ex)
            {
                string desc = Thread.CurrentThread.Name + ex.StackTrace + ex.Message;
                log.WriteEventLog(EventLogEntryType.Error, desc);
                return false;
                throw ex;
            }
            finally
            {
                sendMsmqOperate.Queue.Dispose();
                receiveMsmqOperate.Queue.Dispose();
            }
            return false;
        }
    }
}
