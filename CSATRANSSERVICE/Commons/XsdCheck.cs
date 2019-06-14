using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace CSATRANSSERVICE
{
    public abstract class XsdCheck
    {
        private static readonly string isXsdCheck = ConfigurationManager.AppSettings["XsdCheck"].ToString();

        private static readonly string cyContaDeclareXsd = ConfigurationManager.AppSettings["CyContaDeclareXsd"].ToString();
        private static readonly string cyContaDeclareResponsXsd = ConfigurationManager.AppSettings["CyContaDeclareResponsXsd"].ToString();
        private static readonly string contaDeclareXsd = ConfigurationManager.AppSettings["ContaDeclareXsd"].ToString();
        private static readonly string contaDeclareResponsXsd = ConfigurationManager.AppSettings["ContaDeclareResponseXsd"].ToString();

        public static string IsXsdCheck => isXsdCheck;

        public static string CyContaDeclareXsd => cyContaDeclareXsd;

        public static string CyContaDeclareResponsXsd => cyContaDeclareResponsXsd;

        public static string ContaDeclareXsd => contaDeclareXsd;

        public static string ContaDeclareResponsXsd => contaDeclareResponsXsd;



        /// <summary>
        /// Method: ValidateXML
        /// Description: 使用XSD对报文进行校验
        /// Author: Xiecg
        /// Date: 2019/06/14
        /// Parameter: xmlFile xml文件
        /// Parameter: schemaFile xml文件的XSD文件
        /// Parameter: checkResult 校验结果
        /// Returns: bool 校验成功返回true，校验失败返回false
        ///</summary>
        public static bool ValidateXML(string xmlFile, string schemaFile, out string checkResult)
        {
            bool isValid = true;
            checkResult = "";
            try
            {
                XmlSchemaSet schemaSet = new XmlSchemaSet();
                schemaSet.Add(null, schemaFile);
                XmlReaderSettings readerSetting = new XmlReaderSettings();
                readerSetting.ValidationType = ValidationType.Schema;
                readerSetting.Schemas = schemaSet;

                using (XmlReader xmlReader = XmlReader.Create(xmlFile, readerSetting))
                {
                    while (xmlReader.Read())
                    {
                    }
                }
                checkResult = "XSD校验成功";
            }
            catch (Exception ex)
            {
                isValid = false;
                checkResult = ex.Message;
            }
            return isValid;
        }

        /// <summary>
        /// Method: ValidateXML
        /// Description: 使用XSD对报文进行校验
        /// Author: Xiecg
        /// Date: 2019/06/14
        /// Parameter: xmlFile xml文件
        /// Parameter: schemaFile xml文件的XSD文件
        /// Parameter: checkResult 校验结果
        /// Returns: bool 校验成功返回true，校验失败返回false
        ///</summary>
        public static bool ValidateXML(Stream stream, string schemaFile, out string checkResult)
        {
            bool isValid = true;
            checkResult = "";
            try
            {
                XmlSchemaSet schemaSet = new XmlSchemaSet();
                schemaSet.Add(null, schemaFile);
                XmlReaderSettings readerSetting = new XmlReaderSettings();
                readerSetting.ValidationType = ValidationType.Schema;
                readerSetting.Schemas = schemaSet;

                using (XmlReader xmlReader = XmlReader.Create(stream, readerSetting))
                {
                    while (xmlReader.Read())
                    {
                    }
                }
                checkResult = "XSD校验成功";
            }
            catch (Exception ex)
            {
                isValid = false;
                checkResult = ex.Message;
            }
            return isValid;
        }
    }
}
