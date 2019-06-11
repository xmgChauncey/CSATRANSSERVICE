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

namespace CSATRANSSERVICE
{
    public partial class CsaService : ServiceBase
    {
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
            Thread thread = new Thread(new ThreadStart(OperateCsa02Message));
            thread.Start();

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
                //接收企业CSA01报文的msmq通道
                string receiveCSA01MqAddress = ConfigurationManager.AppSettings["ReceiveCSA01MqAddress"].ToString();

                //发送总署版CSA01报文的msmq通道
                string sendCSA01MaAddress = ConfigurationManager.AppSettings["SendCSA01MqAddress"].ToString();

                //测试使用
                string filePath = ConfigurationManager.AppSettings["CSA01FilePath"].ToString();

                //企业CSA01报文落地保存目录
                string fileSavePath = ConfigurationManager.AppSettings["CSA01FileSavePath"].ToString();

                //连接msmq
                MsmqOperate msmqOperateReceiver = new MsmqOperate();
                MsmqOperate msmqOperateSender = new MsmqOperate();
                msmqOperateReceiver.ConnectMsmq(receiveCSA01MqAddress);
                msmqOperateSender.ConnectMsmq(sendCSA01MaAddress);

                //发送数据到msmq通道
                if (msmqOperateReceiver.SendMsmq(filePath,"CSA01"))
                {
                    //接收msmq通道中的数据
                    if (msmqOperateReceiver.ReceiveMsmq())
                    {
                        Message message = msmqOperateReceiver.Message;

                        //报文落地保存名称
                        string fullFilePath = fileSavePath + @"\" + DateTime.Now.ToString("yyyyMMddHHmmss") + message.Label+".xml";

                        //报文落地，数据入库
                        CsaXmlOperate.ReceiveCSA(message.Body,fullFilePath);

                        //将企业发送的CSA01报文转换成总署版的CSA01报文
                        string  finalCsa01=CsaXmlOperate.ConvertToCSA01(message.Body,fullFilePath);

                        //发送总数版的CSA01报文
                        msmqOperateSender.SendXmlToMsmq(finalCsa01,"CSA01");
                    }
                }
                //阻止设定时间
                Thread.CurrentThread.Join(1000);
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
                //接收总署下发CSA02报文的mq通道
                string receiveCSA01MqAddress = ConfigurationManager.AppSettings["ReceiveCSA02MqAddress"].ToString();

                //发送企业版的CSA02报文的mq通道，
                string sendCSA01MaAddress = ConfigurationManager.AppSettings["SendCSA02MqAddress"].ToString();

                //测试使用
                string filePath = ConfigurationManager.AppSettings["CSA02FilePath"].ToString();

                ///企业CSA02报文落地保存目录
                string fileSavePath = ConfigurationManager.AppSettings["CSA02FileSavePath"].ToString();

                //连接msmq
                MsmqOperate msmqOperate = new MsmqOperate();
                msmqOperate.ConnectMsmq(receiveCSA01MqAddress);

                //发送数据到msmq通道
                if (msmqOperate.SendMsmq(filePath, "CSA02"))
                {
                    //接收msmq通道中的数据
                    if (msmqOperate.ReceiveMsmq())
                    {
                        Message message = msmqOperate.Message;

                        //报文落地保存名称
                        string fullFilePath = fileSavePath + @"\" + DateTime.Now.ToString("yyyyMMddHHmmss") + message.Label + ".xml";

                        //将总署下发的CSA02报文转换成企业版的CSA02报文，报文落地
                        CsaXmlOperate.ConvertToCSA02(message.Body, fullFilePath);

                        //数据入库
                        CsaXmlOperate.ReceiveCSA(fullFilePath);
                    }
                }
                //阻止设定时间
                Thread.CurrentThread.Join(1000);
            }
        }
    }
}
