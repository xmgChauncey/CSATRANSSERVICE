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
    public abstract partial class CsaXmlOperate
    {

        /// <summary>
        /// Method: ReceiveCSA
        /// Description: 将从MQ队列接收到的申报数据在数据库的MESSAGEINFO表中记录需要的数据，并且以xml格式存储在给定的目录中
        /// Author: Xiecg
        /// Date: 2019/06/13
        /// Parameter: message 包含xml数据
        /// Parameter: parentDirect 父级目录
        /// Parameter: fileNumber 文件编号(000-999)
        /// Returns: void
        ///</summary>
        public static string ReceiveCSA(object message,string parentDirect,string fileNumber, out string csa01FilePath)
        {
            string xmlString = message.ToString();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);

            #region 获取Head子节点数据
            //获取SenderID
            string senderId = XmlHelper.Read(xmlDoc, "Head", 0, "SenderID");
            //获取ReceiverID
            string receiverId = XmlHelper.Read(xmlDoc, "Head", 0, "ReceiverID"); 
            //获取MessageID
            string messageId= XmlHelper.Read(xmlDoc, "Head",0,"MessageID");
            //获取MessageType
            string messageType = XmlHelper.Read(xmlDoc, "Head",0,"MessageType");
            //获取CustomsCode
            string customsCode = XmlHelper.Read(xmlDoc, "Head", 0, "CustomsCode");
            //获取SupvLoctCode
            string supvLoctCode = XmlHelper.Read(xmlDoc, "Head", 0, "SupvLoctCode");
            //获取DeclareDataType
            string declareDataType = XmlHelper.Read(xmlDoc, "Head", 0, "DeclareDataType");
            //获取TotalMsgNo
            string totalMsgNo = XmlHelper.Read(xmlDoc, "Head", 0, "TotalMsgNo");
            //获取CurMsgNO
            string curMsgNo = XmlHelper.Read(xmlDoc, "Head", 0, "CurMsgNO");
            #endregion

            #region 将获取的数据插入MESSAGEINFO表
            string guid = "";
            guid = Guid.NewGuid().ToString();
            string cmdText = "INSERT INTO MESSAGEINFO ( GUID, SENDERID, RECEIVERID, MESSAGEID, MESSAGETYPE,CUSTOMSCODE,SUPVLOCTCODE,DECLAREDATATYPE,TOTALMSGNO,CURMSGNO,CREATETIME) VALUES(@GUID, @SENDERID, @RECEIVERID, @MESSAGEID, @MESSAGETYPE,@CUSTOMSCODE,@SUPVLOCTCODE,@DECLAREDATATYPE,@TOTALMSGNO,@CURMSGNO,@CREATETIME);";
            SqlParameter[] parameters = new SqlParameter[11];
            parameters[0] = new SqlParameter("@GUID", guid);
            parameters[1] = new SqlParameter("@SENDERID", senderId);
            parameters[2] = new SqlParameter("@RECEIVERID", receiverId);       
            parameters[3] = new SqlParameter("@MESSAGEID", messageId);
            parameters[4] = new SqlParameter("@MESSAGETYPE", messageType);
            parameters[5] = new SqlParameter("@CUSTOMSCODE", customsCode);
            parameters[6] = new SqlParameter("@SUPVLOCTCODE", supvLoctCode);
            parameters[7] = new SqlParameter("@DECLAREDATATYPE", declareDataType);
            parameters[8] = new SqlParameter("@TOTALMSGNO", totalMsgNo );
            parameters[9] = new SqlParameter("@CURMSGNO", curMsgNo);
            parameters[10] = new SqlParameter("@CREATETIME", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            int execResult=SqlHelper.ExecuteNonQuery(SqlHelper.ConnectionString,System.Data.CommandType.Text,cmdText,parameters);
            #endregion

            //保存文件
            string fullFilePath = FiletoSave(xmlString,parentDirect,senderId,"CSA01", fileNumber);
            csa01FilePath = fullFilePath;
            SaveOperateInfo(guid,OperateType.MessageReceive,OperateName.MessageReceive,OperateResult.OperateSuccess,"",fullFilePath);
            return guid;
        }

        /// <summary>
        /// Method: ReceiveCSA
        /// Description 将从MQ队列接收到的回执数据，并且以xml格式存储在给定的目录中
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
        /// Description 将企业发送的CSA01报文转换成总署版的CSA01报文,并且以xml格式存储在给定的目录中
        /// Author: Xiecg
        /// Date: 2019/06/09
        /// Parameter: message 包含xml数据
        /// Parameter: parentDirect 父级目录
        /// Parameter 文件编号(000-999)
        /// Returns: string  总署版的CSA01报文
        ///</summary>
        public static string ConvertToCSA01(object message, string parentDirect,string fileNumber,string relGuid)
        {
            string xmlString = message.ToString();

            //替换报文头的多个节点名称
            xmlString = xmlString.Replace("Manifest", "ContaDeclareInfo").Replace("MessageID", "MsgId").Replace("MessageType", "MsgType") ;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);

            //获取SenderID
            string senderId = XmlHelper.Read(xmlDoc, "Head", 0, "SenderID");

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

            //保存文件
            string fullFilePath = FiletoSave(xmlDoc.OuterXml,parentDirect,senderId,"ZSCSA01", fileNumber);
            SaveOperateInfo(relGuid,OperateType.MessageConvert,OperateName.MessageConvert,OperateResult.OperateSuccess,"", fullFilePath);

            return fullFilePath;
        }


        /// <summary>
        /// Method: ConvertToCSA02
        /// Description 将总署下发的CSA02报文转换成对接企业的报文格式,并保存转换后的文件
        /// Author: Xiecg
        /// Date: 2019/06/09
        /// Parameter: message 包含xml数据
        /// Parameter: fullFilePath xml文件的完整路径
        /// Returns: void
        ///</summary>
        public static string  ConvertToCSA02(object message, string parentDirect, string fileNumber,out string fullFilePath,out string guid)
        {
            string xmlString = message.ToString();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);

            //获取MessageID
            string messageId = XmlHelper.Read(xmlDoc, "/ContaDeclareResponseInfo/Head/MsgId", "");
            //获取MessageType
            string messageType = XmlHelper.Read(xmlDoc, "/ContaDeclareResponseInfo/Head/MsgType", "");
            //获取CustomsCode
            string customsCode=XmlHelper.Read(xmlDoc, "/ContaDeclareResponseInfo/Head/CustomsCode", "");
            //获取SupvLoctCode
            string supvLoctCode = XmlHelper.Read(xmlDoc, "/ContaDeclareResponseInfo/Head/SupvLoctCode", "");
            //获取DeclDate
            string declDate = XmlHelper.Read(xmlDoc, "/ContaDeclareResponseInfo/Head/DeclDate", "");

            #region 获取回执对应的发送方ID和接收方ID，跟企业发送的报文中的相反
            string cmdText = "SELECT TOP 1 GUID,SENDERID,RECEIVERID FROM MESSAGEINFO WHERE MESSAGEID=@MESSAGEID AND CUSTOMSCODE=@CUSTOMSCODE AND SUPVLOCTCODE=@SUPVLOCTCODE";
            SqlParameter[] parameters = new SqlParameter[3];
            parameters[0] = new SqlParameter("@MESSAGEID", messageId);
            parameters[1] = new SqlParameter("@CUSTOMSCODE", customsCode);
            parameters[2] = new SqlParameter("@SUPVLOCTCODE",supvLoctCode);
            SqlDataReader sqlDataReader = SqlHelper.ExecuteReader(SqlHelper.ConnectionString,System.Data.CommandType.Text,cmdText,parameters);
            DataTable dataTable = SqlHelper.ConvertSqlDataReadeToDataTable(sqlDataReader);

            guid = "";
            string receiverId = "";
            string senderId = "";
            if(dataTable.Rows.Count>0)
            {
                guid= dataTable.Rows[0][0].ToString();
                receiverId = dataTable.Rows[0][1].ToString();
                senderId = dataTable.Rows[0][2].ToString();
            }
            #endregion

            //保存总署版CSA02报文
            string zsCSA02FullFilePath = FiletoSave(xmlDoc.OuterXml, parentDirect, receiverId, "ZSCSA02", fileNumber);
            SaveOperateInfo(guid, OperateType.MessageReceive, OperateName.MessageReceive, OperateResult.OperateSuccess, "", zsCSA02FullFilePath);

            //替换报文头的多个节点名称
            xmlString = xmlString.Replace("ContaDeclareResponseInfo", "Manifest").Replace("MsgId", "MessageID").Replace("MsgType", "MessageType");
            xmlDoc.LoadXml(xmlString);

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

            fullFilePath = FiletoSave(xmlDocSave.OuterXml,parentDirect,receiverId,"CSA02",fileNumber);
            SaveOperateInfo(guid,OperateType.MessageConvert,OperateName.MessageConvert,OperateResult.OperateSuccess,"",fullFilePath);

            return receiverId;
        }

        public static void SendMessageToMSmqFromFile(string fullFilePath,string relGuid,string msmqAddress,string receiver,bool remote)
        {
            MsmqOperate msmqOperateSender = new MsmqOperate();
            if(msmqOperateSender.ConnectMsmq(msmqAddress,remote))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(fullFilePath);
                string xmlConvert= xmlDoc.OuterXml;
                if(msmqOperateSender.SendXmlToMsmqTransaction(xmlConvert))
                {
                    SaveOperateInfo(relGuid, OperateType.MessageSend, OperateName.MessageSend, OperateResult.OperateSuccess, receiver, fullFilePath);
                }
            }          
        }

        public static void SendMessageToMSmqByString(string messageContent, string relGuid, string msmqAddress, string receiver,string fullFilePath,bool remote)
        {
            MsmqOperate msmqOperateSender = new MsmqOperate();
            if (msmqOperateSender.ConnectMsmq(msmqAddress,remote))
            {
                if (msmqOperateSender.SendXmlToMsmqTransaction(messageContent))
                {
                    SaveOperateInfo(relGuid, OperateType.MessageSend, OperateName.MessageSend, OperateResult.OperateSuccess, receiver, fullFilePath);
                }
            }
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

        /// <summary>
        /// Method: FiletoSave
        /// Description: 按照parentDirect\userID\fileType\yyyy\MM\dd\HH\mm+ userID + fileType + "yyyyMMddHHmmssfff"+ "_" + fileNumber + ".xml"格式保存文件
        /// Author: Xiecg
        /// Date: 2019/06/13
        /// Parameter: fileContent 文件内容
        /// Parameter: parentDirect 父级目录
        /// Parameter: userID 用户ID
        /// Parameter: fileType 文件类型
        /// Parameter: fileNumber 文件编号(000-999)
        /// Returns: void
        ///</summary>
        public static string FiletoSave(string fileContent,string parentDirect,string userID,string fileType,string fileNumber)
        {
            string curYear = DateTime.Now.Year.ToString();
            string curMonth = DateTime.Now.Month.ToString();
            string curDay = DateTime.Now.Day.ToString();
            string curHour = DateTime.Now.Hour.ToString();
            string curMinute = DateTime.Now.Minute.ToString();
            string filesaveDirect = parentDirect+"\\"+userID+"\\"+fileType+"\\"+curYear+"\\"+curMonth +"\\"+curDay+"\\"+curHour+ "\\"+curMinute;
            string fileSaveName =filesaveDirect + "\\" + userID + fileType + DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + fileNumber + ".xml";
            if(!Directory.Exists(filesaveDirect))
            {
                Directory.CreateDirectory(filesaveDirect);
            }
            File.WriteAllText(fileSaveName, fileContent);
            return fileSaveName;
        }

        /// <summary>
        /// Method: SaveOperateInfo
        /// Description: 报文操作信息记录到MESSAGEINFOLOG表中
        /// Author: Xiecg
        /// Date: 2019/06/13
        /// Parameter: relGuid 关联的CSA01的GUID
        /// Parameter: operateType 操作类型
        /// Parameter: operateName 操作名称
        /// Parameter: operateResult 操作结果
        /// Parameter: operateRemark 操作备注
        /// Parameter: fileSavePath 文件路径
        /// Returns: void
        ///</summary>
        public static void SaveOperateInfo(string relGuid,string operateType,string operateName,string operateResult,string operateRemark,string fileSavePath)
        {
            #region 将对报文的操作数据记录在MESSAGEINFOLOG表中
            string cmdText = "INSERT INTO MESSAGEINFOLOG(GUID,RELGUID,OPERATEDATE,OPERATETYPE,OPERATENAME,OPERATERESULT,OPERATEREMARK,FILESAVEPATH) VALUES(@GUID,@RELGUID,@OPERATEDATE,@OPERATETYPE,@OPERATENAME,@OPERATERESULT,@OPERATEREMARK,@FILESAVEPATH)";
            SqlParameter[] parameters = new SqlParameter[8];
            parameters[0] = new SqlParameter("@GUID", Guid.NewGuid().ToString());
            parameters[1] = new SqlParameter("@RELGUID", relGuid);
            parameters[2] = new SqlParameter("@OPERATEDATE", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            parameters[3] = new SqlParameter("@OPERATETYPE", operateType);
            parameters[4] = new SqlParameter("@OPERATENAME", operateName);
            parameters[5] = new SqlParameter("@OPERATERESULT", operateResult);
            parameters[6] = new SqlParameter("@OPERATEREMARK", operateRemark);
            parameters[7] = new SqlParameter("@FILESAVEPATH", fileSavePath);
           SqlHelper.ExecuteNonQuery(SqlHelper.ConnectionString, System.Data.CommandType.Text, cmdText, parameters);
            #endregion
        }

        public static object  GetCompanyMqAddress(string companyCode)
        {
            object mqAddress = "";
            string cmdText = "SELECT MQADDRESS FROM COMPANYINFO WHERE COMPANYCODE=@COMPANYCODE";
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@COMPANYCODE", companyCode);
            mqAddress = SqlHelper.ExecuteScalar(SqlHelper.ConnectionString,System.Data.CommandType.Text,cmdText,parameters);
            return mqAddress;
        }
    }
}