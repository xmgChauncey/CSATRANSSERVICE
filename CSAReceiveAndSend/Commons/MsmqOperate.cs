using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Xml;

namespace CSAReceiveAndSend
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
        public bool ConnectMsmq(string msmqAddress)
        {
            if (MessageQueue.Exists(msmqAddress))
            {
                Queue = new MessageQueue(msmqAddress);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Method: SendXmlToMsmq
        /// Description: 发送字符串数据到msmq通道
        /// Author: Xiecg
        /// Date: 2019/06/12
        /// Parameter: sendMessage 发送的字符串数据
        /// Returns: bool 发送成功为true，发送失败为false
        ///</summary>
        public bool SendXmlToMsmq(string sendMessage)
        {
            try
            {
                Message.Body = sendMessage;
                Message.Label = "Xml";
                Message.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
                Queue.Send(Message);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Method: SendXmlToMsmqTransaction
        /// Description: 发送字符串数据到msmq通道，发送过程是事务性的，发送成功提交事务，发送失败回滚事务
        /// Author: Xiecg
        /// Date: 2019/06/12
        /// Parameter: sendMessage 发送的字符串数据
        /// Returns: bool 发送成功为true，发送失败为false
        ///</summary>
        public bool SendXmlToMsmqTransaction(string sendMessage)
        {
            try
            {
                Message.Body = sendMessage;
                Message.Label = "Xml";
                Message.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
                MqTransaction.Begin();
                Queue.Send(Message, MqTransaction);
                MqTransaction.Commit();
            }
            catch (Exception ex)
            {
                MqTransaction.Abort();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Method: SendBinaryToMsmq
        /// Description: 将二进制数据格式发送到MSMQ
        /// Author: Xiecg
        /// Date: 2019/06/12
        /// Parameter: binaryMessage 发送的二进制数据
        /// Parameter: fileType 指定数据包含的对象类型，图像(ImageFile)、文本文件(TextFile)、音频(SoundFile)和视频(VideoFile)
        /// Returns: bool 发送成功为true，发送失败为false
        ///</summary>
        public bool SendBinaryToMsmq(object binaryMessage, string fileType)
        {
            try
            {
                Message = new Message(binaryMessage, new BinaryMessageFormatter());
                Message.Label = "Binary" + "|" + fileType;
                Queue.Send(Message);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Method: SendBinaryToMsmq
        /// Description: 将二进制数据格式发送到MSMQ，发送过程是事务性的，发送成功提交事务，发送失败回滚事务
        /// Author: Xiecg
        /// Date: 2019/06/12
        /// Parameter: binaryMessage 发送的二进制数据
        /// Parameter: fileType 指定数据包含的对象类型，图像(ImageFile)、文本文件(TextFile)、音频(SoundFile)和视频(VideoFile)
        /// Returns: bool 发送成功为true，发送失败为false
        ///</summary>
        public bool SendBinaryToMsmqTransaction(object binaryMessage, string fileType)
        {
            try
            {
                Message = new Message(binaryMessage, new BinaryMessageFormatter());
                Message.Label = "Binary" + "|" + fileType;
                MqTransaction.Begin();
                Queue.Send(Message);
                MqTransaction.Commit();
            }
            catch (Exception ex)
            {
                MqTransaction.Abort();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Method: SendXmlToMsmqTransaction
        /// Description: 发送ActiveX数据到msmq通道
        /// Author: Xiecg
        /// Date: 2019/06/12
        /// Parameter: sendMessage 发送的字符串数据
        /// Returns: bool 发送成功为true，发送失败为false
        ///</summary>
        public bool SendActiveXToMsmq(string sendMessage)
        {
            try
            {
                Message.Body = sendMessage;
                Message.Label = "ActiveX";
                Message.Formatter = new ActiveXMessageFormatter();
                Queue.Send(Message);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Method: SendXmlToMsmqTransaction
        /// Description: 发送ActiveX数据到msmq通道，发送过程是事务性的，发送成功提交事务，发送失败回滚事务
        /// Author: Xiecg
        /// Date: 2019/06/12
        /// Parameter: sendMessage 发送的字符串数据
        /// Returns: bool 发送成功为true，发送失败为false
        ///</summary>
        public bool SendActiveXToMsmqTransaction(string sendMessage)
        {
            try
            {
                Message.Body = sendMessage;
                Message.Label = "ActiveX";
                Message.Formatter = new ActiveXMessageFormatter();
                MqTransaction.Begin();
                Queue.Send(Message, MqTransaction);
                MqTransaction.Commit();
            }
            catch (Exception ex)
            {
                MqTransaction.Abort();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Method: ReceiveMsmq
        /// Description: 从msmq通道中获取数据
        /// Author: Xiecg
        /// Date: 2019/06/12
        /// Parameter: messageFormatter msmq数据格式Xml、Binary或者ActiveX
        /// Returns: bool 接收成功为true，接收失败为false
        ///</summary>
        public bool ReceiveMsmq(string messageFormatter)
        {
            try
            {               
                Message = Queue.Receive();
                switch (messageFormatter)
                {
                    case "Xml":
                        Message.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
                        break;
                    case "Binary":
                        Message.Formatter = new BinaryMessageFormatter();
                        break;
                    case "ActiveX":
                        Message.Formatter = new ActiveXMessageFormatter();
                        break;
                }
            }
            catch (System.Exception ex)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Method: ReceiveMsmq
        /// Description: 从msmq通道中获取数据，接收过程是事务性的，接收成功提交事务，接收失败回滚事务
        /// Author: Xiecg
        /// Date: 2019/06/12
        /// Parameter: messageFormatter msmq数据格式Xml、Binary或者ActiveX
        /// Returns: bool 接收成功为true，接收失败为false
        ///</summary>
        public bool ReceiveMsmqTransaction(string messageFormatter)
        {
            try
            {
                MqTransaction.Begin();
                Message = Queue.Receive();
                MqTransaction.Commit();
                switch (messageFormatter)
                {
                    case "Xml":
                        Message.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
                        break;
                    case "Binary":
                        Message.Formatter = new BinaryMessageFormatter();
                        break;
                    case "ActiveX":
                        Message.Formatter = new ActiveXMessageFormatter();
                        break;
                }
            }
            catch (System.Exception ex)
            {
                MqTransaction.Abort();
                return false;
            }
            return true;
        }
    }
}
