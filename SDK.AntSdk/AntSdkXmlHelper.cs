using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SDK.Service;

namespace SDK.AntSdk
{
    public class AntSdkXmlHelper
    {
        /// <summary>
        /// 方法说明：创建xml文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="elementschildnoteDic">元素及子节点集合</param>
        /// <returns>是否成功创建</returns>
        public static bool CreateXmlFile(string path, Dictionary<string,List<string>> elementschildnoteDic)
        {
            var createIsSucess = false;
            var xmlDoc = new XmlDocument();
            var node = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", string.Empty);
            xmlDoc.AppendChild(node);
            if (elementschildnoteDic?.Count > 0)
            {
                foreach (var elenotes in elementschildnoteDic)
                {
                    var root = xmlDoc.CreateElement(elenotes.Key);
                    xmlDoc.AppendChild(root);
                    if (!(elenotes.Value?.Count > 0)) { continue;}
                    foreach (var note in elenotes.Value)
                    {
                        CreateNode(xmlDoc, root, note, string.Empty);
                    }
                }
            }
            try
            {
                xmlDoc.Save(path);
                createIsSucess = true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"[AntSdkXmlHelper_CreateXml]:{ex.Message}{ex.Source}");
                createIsSucess = false;
            }
            //返回
            return createIsSucess;
        }

        /// <summary>
        /// 方法说明：创建xmlNode
        /// </summary>
        /// <param name="xmlDoc">XML对象</param>
        /// <param name="parentNode">父节点</param>
        /// <param name="name">子节点名称</param>
        /// <param name="value">子节点值</param>
        public static void CreateNode(XmlDocument xmlDoc, XmlNode parentNode, string name, string value)
        {
            try
            {
                var node = xmlDoc.CreateNode(XmlNodeType.Element, name, null);
                node.InnerText = value;
                parentNode.AppendChild(node);
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"[AntSdkXmlHelper_CreateNode]:{ex.Message}{ex.Source}");
            }
        }

        /// <summary>
        /// 方法说明：查找Xml节点的值
        /// </summary>
        /// <param name="elementName">元素名称</param>
        /// <param name="nodeName">子节点名称</param>
        /// <param name="path">xml路径</param>
        /// <returns>节点值</returns>
        public static string XmlFind(string path, string elementName, string nodeName)
        {
            var value = string.Empty;
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(path);
                var xmlElt = xmlDoc.DocumentElement;
                var node = xmlElt?.SelectSingleNode($"//{elementName}");
                if (node != null)
                {
                    value = node.SelectSingleNode(nodeName)?.InnerText;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"[AntSdkXmlHelper_XmlFind]:{ex.Message}{ex.Source}");
            }
            //返回值
            return value;
        }

        /// <summary>
        /// 方法说明：添加xml节点
        /// </summary>
        /// <param name="path">XML文件路径</param>
        /// <param name="parentNode">父节点</param>
        /// <param name="noteName">子节点名称</param>
        /// <param name="value">子节点值</param>
        /// <returns>是否成功添加</returns>
        public static bool XmlAdd(string path, string parentNode, string noteName, string value)
        {
            try
            {
                var xml = new XmlDocument();
                xml.Load(path);
                var root = xml.SelectSingleNode($"//{parentNode}");
                var xmlElement = xml.CreateElement(noteName);
                xmlElement.InnerText = value;
                root?.AppendChild(xmlElement);
                xml.Save(path);
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"[AntSdkXmlHelper_XmlAdd]:{ex.Message}{ex.Source}");
                return false;
            }
        }

        /// <summary>
        /// 方法说明：修改Xml节点数据
        /// </summary>
        /// <param name="path">XML文件路径</param>
        /// <param name="parentNode">父节点</param>
        /// <param name="nodeName">节点名称</param>
        /// <param name="newValue">新值</param>
        /// <returns></returns>
        public static bool XmlModify(string path, string parentNode, string nodeName, string newValue)
        {
            var modify = false;
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(path);
                var xmlElt = xmlDoc.DocumentElement;
                var node = xmlElt?.SelectSingleNode($"//{parentNode}");
                if (node != null)
                {
                    var selectSingleNode = node.SelectSingleNode(nodeName);
                    if (selectSingleNode != null) { selectSingleNode.InnerText = newValue;}
                    xmlDoc.Save(path);
                    modify = true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"[AntSdkXmlHelper_XmlModify]:{ex.Message}{ex.Source}");
                modify = false;
            }
            //返回
            return modify;
        }
    }
}
