using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace CSATRANSSERVICE
{
    public abstract class CsaXmlOperate
    {

        /// <summary>
        /// Method: ReceiveCSA
        /// Description 将从MQ队列接收到的申报数据在数据库的CSADECLDATAINFO表中记录需要的数据，并且以xml格式存储在给定的目录中
        /// Author: Xiecg
        /// Date: 2019/06/09
        /// Parameter: message 包含xml数据
        /// Parameter: fullFilePath xml文件的完整路径
        /// Returns: void
        ///</summary>
        public static void ReceiveCSA(object message,string fullFilePath)
        {
            string xmlString = message.ToString();
            File.WriteAllText(fullFilePath, message.ToString(), Encoding.UTF8);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);

            #region 获取Head子节点数据
            //获取SenderID
            string senderId = XmlHelper.Read(xmlDoc, "Head", 0, "SenderID");
            //获取ReceiverID
            string receiverId = XmlHelper.Read(xmlDoc, "Head", 0, "ReceiverID"); 
            //获取SendTime
            DateTime sendTime = DateTime.ParseExact(XmlHelper.Read(xmlDoc, "Head",0,"SendTime"), "yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);
            //设置ReceiveTime
            DateTime receiveTime = DateTime.Now;
            //获取MessageID
            string messageId= XmlHelper.Read(xmlDoc, "Head",0,"MessageID");
            //获取MessageType
            string messageType = XmlHelper.Read(xmlDoc, "Head",0,"MessageType");
            #endregion

            #region 将获取的数据插入CSADECLDATAINFO表
            string cmdText = "INSERT INTO CSADECLDATAINFO ( GUID, SENDERID, RECEIVERID, SENDTIME, RECEIVETIME, MESSAGEID, MESSAGETYPE, FILESAVEPATH ) VALUES(@GUID, @SENDERID, @RECEIVERID, @SENDTIME, @RECEIVETIME, @MESSAGEID, @MESSAGETYPE, @FILESAVEPATH)";
            SqlParameter[] parameters = new SqlParameter[8];
            parameters[0] = new SqlParameter("@GUID", Guid.NewGuid());
            parameters[1] = new SqlParameter("@SENDERID", senderId);
            parameters[2] = new SqlParameter("@RECEIVERID", receiverId);
            parameters[3] = new SqlParameter("@SENDTIME", sendTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            parameters[4] = new SqlParameter("@RECEIVETIME", receiveTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            parameters[5] = new SqlParameter("@MESSAGEID", messageId);
            parameters[6] = new SqlParameter("@MESSAGETYPE", messageType);
            parameters[7] = new SqlParameter("@FILESAVEPATH", fullFilePath);
            int execResult=SqlHelper.ExecuteNonQuery(SqlHelper.ConnectionString,System.Data.CommandType.Text,cmdText,parameters);
            #endregion

            //保存数据到给定目录下的xml文件中
            if (execResult == 1)
            {

            }

        }

        /// <summary>
        /// Method: ReceiveCSA
        /// Description 将从MQ队列接收到的回执数据在数据库的CSARESPONSEINFO表中记录需要的数据，并且以xml格式存储在给定的目录中
        /// Author: Xiecg
        /// Date: 2019/06/09
        /// Parameter: message 包含xml数据
        /// Parameter: fullFilePath xml文件的完整路径
        /// Returns: void
        ///</summary>
        public static void ReceiveCSA( string fullFilePath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(fullFilePath);

            #region 删除xml的命名空间
            XmlElement root = xmlDoc.DocumentElement;
            root.RemoveAllAttributes();
            string xmlContent = XmlHelper.RemoveAllXmlNamespace(xmlDoc.OuterXml);
            xmlDoc.LoadXml(xmlContent);
            #endregion

            #region 获取数据
            //获取SenderID
            string senderId = XmlHelper.Read(xmlDoc, "/Manifest/Head/SenderID", "");
            //获取ReceiverID
            string receiverId = XmlHelper.Read(xmlDoc, "/Manifest/Head/ReceiverID", "");
            //获取SendTime
            DateTime sendTime = DateTime.ParseExact(XmlHelper.Read(xmlDoc, "/Manifest/Head/SendTime", ""), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            //设置ReceiveTime
            DateTime receiveTime = DateTime.Now;
            //获取MessageID
            string messageId = XmlHelper.Read(xmlDoc, "/Manifest/Head/MessageID", "");
            //获取MessageType
            string messageType = XmlHelper.Read(xmlDoc, "/Manifest/Head/MessageType", "");
            #endregion

            #region 获取关联的CSA01报文的GUID
            string cmdTextSelect = "SELECT GUID FROM CSADECLDATAINFO WHERE MESSAGEID=@MESSAGEID AND MESSAGETYPE=@MESSAGETYPE";
            SqlParameter[] parametersSelect = new SqlParameter[2];
            parametersSelect[0] = new SqlParameter("@MESSAGEID", messageId);
            parametersSelect[1] = new SqlParameter("@MESSAGETYPE", "CSA01");
            object relGuidObject = SqlHelper.ExecuteScalar(SqlHelper.ConnectionString, System.Data.CommandType.Text, cmdTextSelect, parametersSelect);
            Guid relGuid = new Guid(relGuidObject.ToString());
            #endregion

            #region 将获取的数据插入CSARESPONSEINFO表
            string cmdTextInsert = "INSERT INTO CSARESPONSEINFO ( GUID, RELGUID,SENDERID, RECEIVERID, SENDTIME, RECEIVETIME, MESSAGEID, MESSAGETYPE, FILESAVEPATH ) VALUES(@GUID, @RELGUID, @SENDERID, @RECEIVERID, @SENDTIME, @RECEIVETIME, @MESSAGEID, @MESSAGETYPE, @FILESAVEPATH)";
            SqlParameter[] parametersInsert = new SqlParameter[9];
            parametersInsert[0] = new SqlParameter("@GUID", Guid.NewGuid());
            parametersInsert[1] = new SqlParameter("@RELGUID", relGuid);
            parametersInsert[2] = new SqlParameter("@SENDERID", senderId);
            parametersInsert[3] = new SqlParameter("@RECEIVERID", receiverId);
            parametersInsert[4] = new SqlParameter("@SENDTIME", sendTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            parametersInsert[5] = new SqlParameter("@RECEIVETIME", receiveTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            parametersInsert[6] = new SqlParameter("@MESSAGEID", messageId);
            parametersInsert[7] = new SqlParameter("@MESSAGETYPE", messageType);
            parametersInsert[8] = new SqlParameter("@FILESAVEPATH", fullFilePath);
            int execResult = SqlHelper.ExecuteNonQuery(SqlHelper.ConnectionString, System.Data.CommandType.Text, cmdTextInsert, parametersInsert);
            #endregion
        }


        /// <summary>
        /// Method: ConvertToCSA01
        /// Description 将企业发送的CSA01报文转换成总署版的CSA01报文
        /// Author: Xiecg
        /// Date: 2019/06/09
        /// Parameter: message 包含xml数据
        /// Parameter: fullFilePath xml文件的完整路径
        /// Returns: string  总署版的CSA01报文
        ///</summary>
        public static string ConvertToCSA01(object message, string fullFilePath)
        {
            string xmlString = message.ToString();

            //替换报文头的多个节点名称
            xmlString = xmlString.Replace("Manifest", "ContaDeclareInfo").Replace("MessageID", "MsgId").Replace("MessageType", "MsgType") ;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);

            #region 删除xml的命名空间和声明xml文档头
            XmlElement root = xmlDoc.DocumentElement;
            root.RemoveAllAttributes();
            XmlDeclaration xmlDecl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlDoc.InsertBefore(xmlDecl, xmlDoc.DocumentElement);
            string xmlContent = XmlHelper.RemoveAllXmlNamespace(xmlDoc.OuterXml);
            xmlDoc.LoadXml(xmlContent);
            #endregion

            #region 根据CSA01报文的标准删除多余节点
            XmlHelper.Delete(xmlDoc, "/ContaDeclareInfo/Head/SenderID", "");
            XmlHelper.Delete(xmlDoc, "/ContaDeclareInfo/Head/ReceiverID", "");
            XmlHelper.Delete(xmlDoc, "/ContaDeclareInfo/Head/SendTime", "");
            XmlHelper.Delete(xmlDoc, "/ContaDeclareInfo/Head/FunctionCode", "");
            XmlHelper.Delete(xmlDoc, "/ContaDeclareInfo/Head/Version", "");
            XmlHelper.Delete(xmlDoc, "/ContaDeclareInfo/Declaration/Data/Transport_Name", "");
            XmlHelper.Delete(xmlDoc, "/ContaDeclareInfo/Declaration/Data/Voyage_No", "");
            XmlHelper.Delete(xmlDoc, "/ContaDeclareInfo/Declaration/Data/Line_Flag", "");
            XmlHelper.Delete(xmlDoc, "/ContaDeclareInfo/Declaration/Data/IMO", "");
            XmlHelper.Delete(xmlDoc, "/ContaDeclareInfo/Declaration/Data/MMSI", "");
            #endregion

            return xmlDoc.OuterXml;
        }


        /// <summary>
        /// Method: ConvertToCSA02
        /// Description 将总署下发的CSA02报文转换成对接企业的报文格式
        /// Author: Xiecg
        /// Date: 2019/06/09
        /// Parameter: message 包含xml数据
        /// Parameter: fullFilePath xml文件的完整路径
        /// Returns: void
        ///</summary>
        public static void  ConvertToCSA02(object message, string fullFilePath)
        {
            string xmlString = message.ToString();

            //替换报文头的多个节点名称
            xmlString = xmlString.Replace("ContaDeclareResponseInfo", "Manifest").Replace("MsgId", "MessageID").Replace("MsgType", "MessageType");

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);

            //获取MessageID
            string messageId = XmlHelper.Read(xmlDoc, "/Manifest/Head/MessageID", "");
            //获取MessageType
            string messageType = XmlHelper.Read(xmlDoc, "/Manifest/Head/MessageType", "");
            //获取CustomsCode
            string customsCode=XmlHelper.Read(xmlDoc, "/Manifest/Head/CustomsCode", "");
            //获取DeclDate
            string declDate = XmlHelper.Read(xmlDoc, "/Manifest/Head/DeclDate", "");

            #region 获取回执对应的发送方ID和接收方ID，跟企业发送的报文中的相反
            string cmdText = "SELECT SENDERID,RECEIVERID FROM CSADECLDATAINFO WHERE MESSAGEID=@MESSAGEID AND MESSAGETYPE=@MESSAGETYPE";
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@MESSAGEID", messageId);
            parameters[1] = new SqlParameter("@MESSAGETYPE", "CSA01");
            SqlDataReader sqlDataReader = SqlHelper.ExecuteReader(SqlHelper.ConnectionString,System.Data.CommandType.Text,cmdText,parameters);
            DataTable dataTable = SqlHelper.ConvertSqlDataReadeToDataTable(sqlDataReader);

            string receiverId = "";
            string senderId = "";
            if(dataTable.Rows.Count>0)
            {
                receiverId = dataTable.Rows[0][0].ToString();
                senderId = dataTable.Rows[0][1].ToString();
            }
            #endregion

            # region 发送给企业的回执报文需要在Head节点下添加SenderID、ReceiverID和SendTime子节点
            string sendTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            XElement xElement = XElement.Load(new MemoryStream(Encoding.UTF8.GetBytes(xmlDoc.DocumentElement.OuterXml)));
            XmlHelper.AddToElementBefore(xElement, "Head", customsCode, "SenderID", senderId);
            XmlHelper.AddToElementBefore(xElement, "Head", customsCode, "ReceiverID", receiverId);
            XmlHelper.AddToElementBefore(xElement, "Head", customsCode, "SendTime", sendTime);
            XmlHelper.AddToElementBefore(xElement, "Head", messageType, "FunctionCode", "2");
            XmlHelper.AddToElementAfter(xElement, "Head", declDate,"Version","1.0");
            #endregion

            //删除xml的 <? xml version = "1.0" encoding = "UTF-8" ?>
            XmlDocument xmlDocSave = new XmlDocument();
            xmlDocSave.LoadXml(xElement.ToString());
            
            //添加命名空间
            XmlHelper.Update(xmlDocSave, "Manifest", "xmlns", "urn:Declaration:datamodel:standard:CN:CSA02");
            XmlHelper.Update(xmlDocSave, "Manifest", "xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");

            xmlDocSave.Save(fullFilePath);
        }


       /// <summary>
       /// Method: StreamToFile
       /// Description: 将stream流中的数据写入指定的文件中
       /// Author: Xiecg
       /// Date: 2019/06/11
       /// Parameter: stream 数据流
       /// Parameter: fileName 存储文件完整路径
       /// Returns: void
       ///</summary>
       public static void StreamToFile(Stream stream,string fileName)
        {
            // 把 Stream 转换成 
             byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);     // 设置当前流的位置为流的开始     
            stream.Seek(0, SeekOrigin.Begin);      // 把 byte[] 写入文件     
            FileStream fs = new FileStream(fileName, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(bytes);
            bw.Close();
            fs.Close(); } 
    }
}