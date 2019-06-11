using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Xml;

namespace CSATRANSSERVICE
{
    public class MsmqOperate
    {
        MessageQueue queue;
        Message message = new Message();
        MessageQueueTransaction mqTransaction = new MessageQueueTransaction();

        public Message Message { get => message; set => message = value; }
        public MessageQueue Queue { get => queue; set => queue = value; }
        public MessageQueueTransaction MqTransaction { get => mqTransaction; set => mqTransaction = value; }


        /// <summary>
        /// Method: ConnectMsmq
        /// Description: 通过msmq地址连接msmq
        /// Author: Xiecg
        /// Date: 2019/06/11
        /// Parameter: msmqAddress msmq地址
        /// Returns: void
        ///</summary>
        public void ConnectMsmq(string msmqAddress)
        {   
                Queue = new MessageQueue(msmqAddress);
        }

        /// <summary>
        /// Method: SendMsmq
        /// Description: 获取文件路径为xmlFilePath的xml文件内容，发送到msmq通道
        /// Author: Xiecg
        /// Date: 2019/06/11
        /// Parameter: xmlFilePath xml文件路径 
        /// Parameter: msgType 发送报文的类型CSA01或者CSA02
        /// Returns: bool 发送成功为true，发送失败为false
        ///</summary>
        public bool SendMsmq(string xmlFilePath,string msgType)
        {       
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFilePath);
                string xmlContent = xmlDoc.InnerXml;
                Message.Body = xmlContent;
                Message.Label = msgType;
                Message.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
                MqTransaction.Begin();
                Queue.Send(Message, MqTransaction);
                MqTransaction.Commit();
            }
            catch (System.Exception ex)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Method: SendMsmq
        /// Description: 将包含xml文件内容的字符串，发送到msmq通道
        /// Author: Xiecg
        /// Date: 2019/06/11
        /// Parameter: xmlContent 包含xml文件内容的字符串
        /// Parameter: msgType 发送报文的类型CSA01或者CSA02
        /// Returns: bool 发送成功为true，发送失败为false
        ///</summary>
        public bool SendXmlToMsmq(string xmlContent, string msgType)
        {
            try
            {
                Message.Body = xmlContent;
                Message.Label = msgType;
                Message.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
                MqTransaction.Begin();
                Queue.Send(Message, MqTransaction);
                MqTransaction.Commit();
            }
            catch (System.Exception ex)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Method: ReceiveMsmq
        /// Description: 从msmq通道中获取数据
        /// Author: Xiecg
        /// Date: 2019/06/11
        /// Returns: bool 接收成功为true，接收失败为false
        ///</summary>
        public  bool ReceiveMsmq()
        {
            try
            {
                MqTransaction.Begin();
                Message = Queue.Receive();
                MqTransaction.Commit();
                Message.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
            }
            catch (System.Exception ex)
            {
                return false;
            }
            return true;
        }

    }
}
