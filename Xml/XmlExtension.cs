using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using SP.StudioCore.Types;

namespace SP.StudioCore.Xml
{
    /// <summary>
    /// XML的扩展
    /// </summary>
    public static class XmlExtension
    {
        /// <summary>
        /// 获取路径的值。如果不存在则返回defaultValue 路径用/隔开
        /// 支持获取同名同级节点中的某个，格式： elementName[index] 从0开始
        /// </summary>
        public static string GetValue(this XElement element, string xPath, string defaultValue = null)
        {
            element = element.GetElement(xPath);
            return element == null ? defaultValue : element.Value;
        }

        public static T GetValue<T>(this XElement? element, string xPath, T defaultValue)
        {
            if (element == null) return defaultValue;
            if (!string.IsNullOrEmpty(xPath))
            {
                element = element.GetElement(xPath);
            }
            Type type = typeof(T);
            string? value = element.Value;
            object? obj = value.GetValue(type);
            if (obj == null) return defaultValue;
            return (T)obj;
        }

        /// <summary>
        /// 获取属性值 如果不存在则返回defaultValue
        /// </summary>
        public static string? GetAttributeValue(this XElement element, string name)
        {
            return element.GetAttributeValue<string>(name, null);
        }

        public static T GetAttributeValue<T>(this XElement element, string name, T t)
        {
            if (element.Attribute(name) == null) return t;
            string? value = element.Attribute(name)?.Value;
            if (value == null) return t;
            if (!value.IsType<T>()) return t;
            return value.GetValue<T>();
        }

        /// <summary>
        /// 获取路径的节点。如果不存在则返回defaultValue 路径用/隔开
        /// 支持获取同名同级节点中的某个，格式： elementName[index] 从0开始
        /// </summary>
        public static XElement? GetElement(this XElement? element, string xPath)
        {
            Regex regex = new Regex(@"(?<Name>.*)\[(?<Index>\d+)\]", RegexOptions.IgnoreCase);
            try
            {
                foreach (string tag in xPath.Split('/'))
                {
                    if (regex.IsMatch(tag))
                    {
                        string tagName = regex.Match(tag).Groups["Name"].Value;
                        int index = int.Parse(regex.Match(tag).Groups["Index"].Value);
                        element = element?.Elements(tagName).ToArray()[index];
                    }
                    else
                    {
                        element = element?.Element(tag);
                    }
                    if (element == null) return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\n" + xPath);
            }
            return element;

        }

        /// <summary>
        /// 两个XML合并 把obj拥有而root没有的节点和属性复制给root
        /// </summary>
        /// <param name="root">原始的对象</param>
        /// <param name="obj"></param>
        public static XElement Merger(this XElement root, XElement obj)
        {
            XmlMerger(root, obj);
            return root;
        }

        /// <summary>
        /// 递归对比两个节点，把obj拥有而root没有的节点复制到root中
        /// </summary>
        /// <param name="root"></param>
        /// <param name="obj"></param>
        private static void XmlMerger(XElement root, XElement obj)
        {
            foreach (XElement element in obj.Elements())
            {
                var childElements = root.Elements(element.Name);

                if (childElements.Count() == 0)
                {
                    root.Add(element);
                }
                else if (childElements.Count() == 1)    // 有且只有一个同名节点才启动复制递归规则
                {
                    XElement childElement = childElements.First();
                    foreach (XAttribute attribute in element.Attributes())
                    {
                        if (childElement.Attributes(attribute.Name).Count() == 0)
                            childElement.SetAttributeValue(attribute.Name, attribute.Value);
                    }
                    XmlMerger(childElement, element);
                }
            }
        }

        /// <summary>
        /// 获取xmlelemnet
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        public static XElement GetXElement(string fileName, string path = null)
        {
            XmlDocument document = new XmlDocument();
            if (string.IsNullOrWhiteSpace(path))
                path = AppContext.BaseDirectory;
            string file = path + fileName;
            return XElement.Load(file);
        }

        /// <summary>
        /// 遍历所有的子元素中包含名称的节点
        /// </summary>
        /// <param name="tagName"></param>
        /// <returns></returns>
        public static List<XElement> GetElements(this XElement root, params string[] tagName)
        {
            List<XElement> list = new List<XElement>();
            GetElements(root, list, tagName);
            return list;
        }

        private static void GetElements(XElement root, List<XElement> list, params string[] tagName)
        {
            foreach (XElement el in root.Elements())
            {
                if (tagName.Contains(el.Name.ToString()))
                {
                    list.Add(el);
                }
                GetElements(el, list, tagName);
            }
        }
    }
}
