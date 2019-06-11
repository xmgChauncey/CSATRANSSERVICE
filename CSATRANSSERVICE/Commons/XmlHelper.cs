using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace CSATRANSSERVICE
{
    public class XmlHelper
    {
        public XmlHelper()
        {
        }

        /// <summary>
        /// Method: Read
        /// Description: 读取数据，xml文件没有命名空间
        /// Author: Xiecg
        /// Date: 2019/06/09
        /// Parameter: doc XmlDocument类型数据
        /// Parameter: node 节点
        /// Parameter: attribute 属性名，非空时返回该属性值，否则返回串联值
        /// Returns: string
        ///</summary>
        public static string Read(XmlDocument doc, string node, string attribute)
        {
            string value = "";
            try
            {
                XmlNode xn = doc.SelectSingleNode(node);
                value = (attribute.Equals("") ? xn.InnerText : xn.Attributes[attribute].Value);
            }
            catch
            {

            }
            return value;
        }

        public static string Read(XmlDocument doc,string tableName,int rowNumber,string columnName)
        {
            string value = "";
            try
            {
	            DataSet dataSet = new DataSet();
	            dataSet.ReadXml(new MemoryStream(Encoding.UTF8.GetBytes(doc.OuterXml)));
                value = dataSet.Tables[tableName].Rows[rowNumber][columnName].ToString();
            }
            catch (System.Exception ex)
            {
 	
            }
            return value;
        }

        /// <summary>
        /// Method: Insert
        /// Description: 插入数据
        /// Author: Xiecg
        /// Date: 2019/06/09
        /// Parameter: doc XmlDocument类型数据
        /// Parameter: node 节点
        /// Parameter: element 元素名，非空时插入新元素，否则在该元素中插入属性
        /// Parameter: attribute 属性名，非空时插入该元素属性值，否则插入元素值
        /// Parameter: value 值
        /// Returns: void
        ///</summary>
        public static void Insert(XmlDocument doc, string node, string element, string attribute, string value)
        {
            try
            {
                XmlNode xn = doc.SelectSingleNode(node);
                if (element.Equals(""))
                {
                    if (!attribute.Equals(""))
                    {
                        XmlElement xe = (XmlElement)xn;
                        xe.SetAttribute(attribute, value);
                    }
                }
                else
                {
                    XmlElement xe = doc.CreateElement(element);
                    if (attribute.Equals(""))
                        xe.InnerText = value;
                    else
                        xe.SetAttribute(attribute, value);
                    xn.AppendChild(xe);
                }
            }
            catch { }
        }

        /// <summary>
        /// Method: Update
        /// Description: 修改数据
        /// Author: Xiecg
        /// Date: 2019/06/09
        /// Parameter: doc  XmlDocument类型数据
        /// Parameter: node 节点
        /// Parameter: attribute 属性名，非空时修改该节点属性值，否则修改节点值
        /// Parameter: value 值
        /// Returns: void
        ///</summary>
        public static void Update(XmlDocument doc, string node, string attribute, string value)
        {
            try
            {
                XmlNode xn = doc.SelectSingleNode(node);
                XmlElement xe = (XmlElement)xn;
                if (attribute.Equals(""))
                    xe.InnerText = value;
                else
                    xe.SetAttribute(attribute, value);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Method: Delete
        /// Description: 删除数据
        /// Author: Xiecg
        /// Date: 2019/06/09
        /// Parameter: doc XmlDocument类型数据
        /// Parameter: node 节点
        /// Parameter: attribute 属性名，非空时删除该节点属性值，否则删除节点值
        /// Returns: void 
        ///</summary>
        public static void Delete(XmlDocument doc, string node, string attribute)
        {
            try
            {
                XmlNodeList xnl = doc.SelectNodes(node);
                foreach (XmlNode xn in xnl)
                {
                    XmlElement xe = (XmlElement)xn;
                    if (attribute.Equals(""))
                        xn.ParentNode.RemoveChild(xn);
                    else
                        xe.RemoveAttribute(attribute);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Method: AddToElementBefore
        /// Description: 在指定的节点前插入同级节点
        /// Author: Xiecg
        /// Date: 2019/06/09
        /// Parameter: xElement XElement类型数据
        /// Parameter: refXNameParent 父节点
        /// Parameter: refXValues 指定节点值
        /// Parameter: newXName 新增的节点名字
        /// Parameter: newXValues 新增的节点值
        /// Returns: void
        ///</summary>
        public static void AddToElementBefore(XElement xElement,string refXNameParent,string refXValues,string newXName,string newXValues)
        {
            var item = (from ele in xElement.Element(refXNameParent).Elements()
                        where ele.Value.Equals(refXValues)
                        select ele).SingleOrDefault();
            if (item != null)
            {
                XElement newXmlNode = new XElement(newXName, newXValues);
                item.AddBeforeSelf(newXmlNode);
            }
        }

        /// <summary>
        /// Method: AddToElementBefore
        /// Description: 在指定的节点后插入同级节点
        /// Author: Xiecg
        /// Date: 2019/06/09
        /// Parameter: xElement XElement类型数据
        /// Parameter: refXNameParent 父节点
        /// Parameter: refXValues 指定节点值
        /// Parameter: newXName 新增的节点名字
        /// Parameter: newXValues 新增的节点值
        /// Returns: void
        ///</summary>
        public static void AddToElementAfter(XElement xElement, string refXNameParent, string refXValues, string newXName, string newXValues)
        {
            var item = (from ele in xElement.Element(refXNameParent).Elements()
                        where ele.Value.Equals(refXValues)
                        select ele).SingleOrDefault();
            if (item != null)
            {
                XElement newXmlNode = new XElement(newXName, newXValues);
                item.AddAfterSelf (newXmlNode);
            }
        }


        /// <summary>
        /// Method: RemoveAllXmlNamespace
        /// Description: 删除xml文件中的默认命名空间xmlns
        /// Author: Xiecg
        /// Date: 2019/06/11
        /// Parameter: xmlData 包含xml格式内容的字符串
        /// Returns: string 返回删除了xmlns的xml格式内容的字符串
        ///</summary>
        public static string RemoveAllXmlNamespace(string xmlData)
        {
            string xmlnsPattern = "\\s+xmlns\\s*(:\\w)?\\s*=\\s*\\\"(?<url>[^\\\"]*)\\\"";
            MatchCollection matchCol = Regex.Matches(xmlData, xmlnsPattern);

            foreach (Match m in matchCol)
            {
                xmlData = xmlData.Replace(m.ToString(), "");
            }
            return xmlData;
        }

        public static void AddXmlNamespace(XElement xElement)
        {

        }
    }
}