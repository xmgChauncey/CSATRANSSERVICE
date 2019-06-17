using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Xml;
using System.Collections;

namespace CSAReceiveAndSend
{
    public class MsmqOperate
    {
        private MessageQueue queue = new MessageQueue();
        private Message messageTrans = new Message();
        private MessageQueueTransaction mqTransaction = new MessageQueueTransaction();

        public Message MessageTrans { get => messageTrans; set => messageTrans = value; }
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
            Queue = new MessageQueue(msmqAddress);
            return true;
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
                MessageTrans.Body = sendMessage;
                MessageTrans.Label = "Xml";
                MessageTrans.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
                Queue.Send(MessageTrans);
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
                MessageTrans.Body = sendMessage;
                MessageTrans.Label = "Xml";
                MessageTrans.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
                MqTransaction.Begin();
                Queue.Send(MessageTrans, MqTransaction);
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
                MessageTrans = new Message(binaryMessage, new BinaryMessageFormatter());
                MessageTrans.Label = "Binary" + "|" + fileType;
                Queue.Send(MessageTrans);
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
                MessageTrans = new Message(binaryMessage, new BinaryMessageFormatter());
                MessageTrans.Label = "Binary" + "|" + fileType;
                MqTransaction.Begin();
                Queue.Send(MessageTrans);
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
                MessageTrans.Body = sendMessage;
                MessageTrans.Label = "ActiveX";
                MessageTrans.Formatter = new ActiveXMessageFormatter();
                Queue.Send(MessageTrans);
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
                MessageTrans.Body = sendMessage;
                MessageTrans.Label = "ActiveX";
                MessageTrans.Formatter = new ActiveXMessageFormatter();
                MqTransaction.Begin();
                Queue.Send(MessageTrans, MqTransaction);
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
                MessageTrans = Queue.Receive();
                switch (messageFormatter)
                {
                    case "Xml":
                        MessageTrans.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
                        break;
                    case "Binary":
                        MessageTrans.Formatter = new BinaryMessageFormatter();
                        break;
                    case "ActiveX":
                        MessageTrans.Formatter = new ActiveXMessageFormatter();
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
                MessageTrans = Queue.Receive();
                MqTransaction.Commit();
                switch (messageFormatter)
                {
                    case "Xml":
                        MessageTrans.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
                        break;
                    case "Binary":
                        MessageTrans.Formatter = new BinaryMessageFormatter();
                        break;
                    case "ActiveX":
                        MessageTrans.Formatter = new ActiveXMessageFormatter();
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

        public static void ReceiveMsg(string mqPath)
        {
            MessageQueue receiveQueue = new MessageQueue(mqPath);
            receiveQueue.Formatter = new BinaryMessageFormatter();

            MessagePropertyFilter settings = new MessagePropertyFilter();
            settings.SetAll();
            receiveQueue.MessageReadPropertyFilter = settings;


            Queue msgQueue = new Queue();

            //定位事务中的第一个消息
            Message msgPeek;
            while (true)
            {
                msgPeek = receiveQueue.Peek(TimeSpan.FromMilliseconds(5000));
                if (!msgPeek.IsFirstInTransaction)
                {
                    receiveQueue.Receive(TimeSpan.FromMilliseconds(5000));
                }
                else
                {
                    break;
                }
            }

            if (msgPeek.IsFirstInTransaction)
            {
                #region 读取队列中的一个事务的消息放入队列中，并获取附加消息
                MessageQueueTransaction receiveTransaction = new MessageQueueTransaction();
                receiveTransaction.Begin();
                try
                {
                    while (true)
                    {
                        Message msg = receiveQueue.Receive(TimeSpan.FromMilliseconds(5000), receiveTransaction);
                        msgQueue.Enqueue(msg);

                        if (msg.IsLastInTransaction)
                        {
                            receiveTransaction.Commit();
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    receiveTransaction.Commit();
                    string error = string.Empty;
                }
                finally
                {
                    receiveTransaction.Dispose();
                }
                #endregion
            }
        }
    }
}
