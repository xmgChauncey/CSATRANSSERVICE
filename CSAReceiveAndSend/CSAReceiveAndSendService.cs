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
using System.Xml;

namespace CSAReceiveAndSend
{
    public partial class CSAReceiveAndSendService : ServiceBase
    {
        public CSAReceiveAndSendService()
        {
            InitializeComponent();
        }

        //protected override void OnStart(string[] args)
        //{
        //}

        public void OnStart()
        {
            //todo something
            Thread thread = new Thread(new ThreadStart(ReceiveAndSend));
            thread.Start();

        }

        protected override void OnStop()
        {

        }

        private void ReceiveAndSend()
        {
            while (true)
            {
                //接收企业CSA01报文的msmq通道 
                string receivMq = ConfigurationManager.AppSettings["ReceiveMq"].ToString();

                //发送总署版CSA01报文的msmq通道
                string sendMq = ConfigurationManager.AppSettings["SendMq"].ToString();

                //测试使用
                string filePath = ConfigurationManager.AppSettings["CSA01FilePath"].ToString();

                string fileSavePath = ConfigurationManager.AppSettings["CSA01FileSavePath"].ToString();

                //创建发送数据
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(filePath);
                string xmlContent = xmlDoc.InnerXml;
                MsmqOperate msmqSend = new MsmqOperate();
                msmqSend.ConnectMsmq(receivMq);
                msmqSend.SendXmlToMsmqTransaction(xmlContent);

                ReceiveAndSendTransaction("Xml",receivMq,sendMq);

                MsmqOperate msmqReceive = new MsmqOperate();
                msmqReceive.ConnectMsmq(sendMq);
                msmqReceive.ReceiveMsmqTransaction("Xml");
                msmqReceive.Message.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
                string bodyContent = msmqReceive.Message.Body.ToString();
                fileSavePath = fileSavePath + "\\" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xml";
                File.WriteAllText(fileSavePath, bodyContent);

                Thread.CurrentThread.Join(1000);
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
                            sendMsmqOperate.MqTransaction.Abort();
                            throw ex;
                        }
                        receiveMsmqOperate.MqTransaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        receiveMsmqOperate.MqTransaction.Abort();
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return false;
        }
    }
}
