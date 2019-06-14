using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Messaging;
using System.ServiceProcess;
using System.Threading;
using System.Xml;

namespace CSATRANSSERVICE
{
    public partial class CsaService : ServiceBase
    {
        int csa01FileNumber = 0;
        int csa02FileNumber = 0;
        public CsaService()
        {
            InitializeComponent();
        }

        //protected override void OnStart(string[] args)
        //{

        //}

        public void OnStart()
        {
            //todo something
            Thread threadCsa01 = new Thread(new ThreadStart(OperateCsa01Message));
            threadCsa01.Start();

            Thread threadCsa02 = new Thread(new ThreadStart(OperateCsa02Message));
            threadCsa02.Start();
        }

        protected override void OnStop()
        {

        }

        /// <summary>
        /// Method: OperateCsa01Message
        /// Description: 接收企业发送的CSA01报文，报文落地，数据入库
        /// 转换成总署版的CSA01报文，并通过msmq发送给省电子口岸
        /// Author: Xiecg
        /// Date: 2019/06/11
        /// Returns: void
        ///</summary>
        private void OperateCsa01Message()
        {
            while (true)
            {
                try
                {
                    //接收企业CSA01报文的msmq通道
                    string receiveCSA01MqAddress = ConfigurationManager.AppSettings["ReceiveCSA01MqAddress"].ToString();
                    //发送总署版CSA01报文的msmq通道
                    string sendCSA01MqAddress = ConfigurationManager.AppSettings["SendCSA01MqAddress"].ToString();
                    //网络科msmq通道
                    string sendToNetWorkDepartMqAddress = ConfigurationManager.AppSettings["SendToNetWorkDepartMqAddress"].ToString();

                    //MSMQ数据格式
                    string messageType = ConfigurationManager.AppSettings["MessageType"].ToString();
                    //报文落地保存父级目录
                    string parentDirect = ConfigurationManager.AppSettings["CSAFileSaveDirect"].ToString();
                    //数据是否需要加验签
                    string messageSign = ConfigurationManager.AppSettings["MessageSigh"].ToString();

                    //测试使用
                    string filePath = ConfigurationManager.AppSettings["CSA01FilePath"].ToString();
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load(filePath);
                    string xmlContent = xmlDocument.OuterXml;
                    MsmqOperate msmqSend = new MsmqOperate();
                    msmqSend.ConnectMsmq(receiveCSA01MqAddress, false);
                    if (msmqSend.Queue.Transactional)
                    {
                        msmqSend.SendXmlToMsmqTransaction(xmlContent);
                    }
                    else
                    {
                        msmqSend.SendXmlToMsmq(xmlContent);
                    }

                    //连接msmq
                    MsmqOperate msmqOperateReceiver = new MsmqOperate();

                    if (msmqOperateReceiver.ConnectMsmq(receiveCSA01MqAddress, false))
                    {
                        //接收msmq通道中的数据
                        if (msmqOperateReceiver.ReceiveMsmqTransaction(messageType))
                        {
                            Message message = msmqOperateReceiver.Message;
                            switch (messageType)
                            {
                                case "Xml":
                                    message.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
                                    break;
                                case "Binary":
                                    message.Formatter = new BinaryMessageFormatter();
                                    break;
                                case "ActiveX":
                                    message.Formatter = new ActiveXMessageFormatter();
                                    break;
                            }

                            //数据加验签处理
                            if (messageSign.Equals("true"))
                            {
                                //数据解签
                            }

                            string csa01FilePath = "";

                            //数据入库，报文落地
                            string relGuid = CsaXmlOperate.ReceiveCSA(message.Body, parentDirect, csa01FileNumber.ToString("000"), out csa01FilePath);

                            if (relGuid != "" && csa01FilePath != "")
                            {
                                //发送企业版的CSA01报文给网络科
                                CsaXmlOperate.SendMessageToMSmqFromFile(csa01FilePath, relGuid, sendCSA01MqAddress, Sender.SendToNetWorkDepart, true);

                                //将企业发送的CSA01报文转换成总署版的CSA01报文
                                string fullFilePath = CsaXmlOperate.ConvertToCSA01(message.Body, parentDirect, csa01FileNumber.ToString("000"), relGuid);

                                //发送总署版的CSA01报文给省电子口岸
                                CsaXmlOperate.SendMessageToMSmqFromFile(fullFilePath, relGuid, sendToNetWorkDepartMqAddress, Sender.SendToCport, true);

                                if (csa01FileNumber < 10)
                                {
                                    csa01FileNumber++;
                                }
                                else
                                {
                                    csa01FileNumber = 0;
                                }
                            }
                        }

                    }
                    //阻止设定时间
                    Thread.CurrentThread.Join(1000);
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
        }

        /// <summary>
        /// Method: OperateCsa02Message
        /// Description: 接收总署下发的CSA02报文，报文落地，数据入库
        /// 转换成企业版的CSA02报文，并发送给企业
        /// Author: Xiecg
        /// Date: 2019/06/11
        /// Returns: void
        ///</summary>
        private void OperateCsa02Message()
        {
            while (true)
            {
                try
                {
                    //接收总署下发CSA02报文的mq通道
                    string receiveCSA02MqAddress = ConfigurationManager.AppSettings["ReceiveCSA02MqAddress"].ToString();

                    //MSMQ数据格式
                    string messageType = ConfigurationManager.AppSettings["MessageType"].ToString();
                    //报文落地保存父级目录
                    string parentDirect = ConfigurationManager.AppSettings["CSAFileSaveDirect"].ToString();
                    //数据是否需要加验签
                    string messageSign = ConfigurationManager.AppSettings["MessageSigh"].ToString();

                    #region 测试使用
                    string filePath = ConfigurationManager.AppSettings["CSA02FilePath"].ToString();
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load(filePath);
                    string xmlContent = xmlDocument.OuterXml;
                    MsmqOperate msmqSend = new MsmqOperate();
                    msmqSend.ConnectMsmq(receiveCSA02MqAddress, false);
                    if (msmqSend.Queue.Transactional)
                    {
                        msmqSend.SendXmlToMsmqTransaction(xmlContent);
                    }
                    else
                    {
                        msmqSend.SendXmlToMsmq(xmlContent);
                    }
                    #endregion

                    //连接msmq
                    MsmqOperate msmqOperate = new MsmqOperate();
                    if (msmqOperate.ConnectMsmq(receiveCSA02MqAddress, false))
                    {
                        //接收msmq通道中的数据
                        if (msmqOperate.ReceiveMsmqTransaction("Xml"))
                        {
                            Message message = msmqOperate.Message;
                            switch (messageType)
                            {
                                case "Xml":
                                    message.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
                                    break;
                                case "Binary":
                                    message.Formatter = new BinaryMessageFormatter();
                                    break;
                                case "ActiveX":
                                    message.Formatter = new ActiveXMessageFormatter();
                                    break;
                            }

                            //将总署下发的CSA02报文转换成企业版的CSA02报文，报文落地
                            string csa02FileSavePath = "";
                            string relGuid = "";
                            string receiverId = CsaXmlOperate.ConvertToCSA02(message.Body, parentDirect, csa02FileNumber.ToString("000"), out csa02FileSavePath, out relGuid);

                            if(receiverId!=""&&csa02FileSavePath!="")
                            {
                                #region 发送回执给企业
                                //获取企业的msmq地址
                                string mqAddress = "";
                                object mqAddressObj = CsaXmlOperate.GetCompanyMqAddress(receiverId);
                                if (mqAddressObj != null)
                                {
                                    mqAddress = CsaXmlOperate.GetCompanyMqAddress(receiverId).ToString();

                                    //获取转换后xml文件内容
                                    string messageContent = "";
                                    XmlDocument xmlDoc = new XmlDocument();
                                    xmlDoc.Load(csa02FileSavePath);
                                    messageContent = xmlDoc.OuterXml;

                                    //数据加验签处理
                                    if (messageSign.Equals("true"))
                                    {
                                        //数据加签
                                    }

                                    //发送回执到企业MSMQ
                                    CsaXmlOperate.SendMessageToMSmqByString(messageContent, relGuid, mqAddress, Sender.SendToCompany, csa02FileSavePath, true);
                                }
                                #endregion
                            }

                        }
                    }
                    //阻止设定时间
                    Thread.CurrentThread.Join(1000);
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
        }
    }
}
