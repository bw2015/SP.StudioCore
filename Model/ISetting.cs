using Newtonsoft.Json;
using SP.StudioCore.Enums;
using SP.StudioCore.Json;
using SP.StudioCore.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace SP.StudioCore.Model
{
    /// <summary>
    /// QueryString参数配置的基类
    /// </summary>
    public abstract class ISetting
    {
        public ISetting() { }

        /// <summary>
        /// 来自数据库的参数配置
        /// </summary>
        /// <param name="read"></param>
        public ISetting(IDataReader read)
        {
            Dictionary<string, PropertyInfo> properties = this.GetType().GetProperties().ToDictionary(t => t.Name, t => t);

            while (read.Read())
            {
                string name = (string)read[0];
                if (!properties.ContainsKey(name)) continue;
                PropertyInfo property = properties[name];
                object? value = ((string)read[1]).GetValue(property.PropertyType);
                if (value != null) property.SetValue(this, value);
            }
            if (!read.IsClosed) read.Close();
        }

        /// <summary>
        /// 赋值构造
        /// </summary>
        /// <param name="queryString"></param>
        public ISetting(string queryString)
        {
            NameValueCollection request = HttpUtility.ParseQueryString(queryString ?? string.Empty);

            foreach (PropertyInfo property in this.GetType().GetProperties().Where(t => t.CanWrite && !t.HasAttribute<IgnoreAttribute>()))
            {
                if (request.AllKeys.Contains(property.Name))
                {
                    string value = request[property.Name] ?? string.Empty;
                    object? result = null;

                    switch (property.PropertyType.Name)
                    {
                        case "Boolean":
                            result = value.Equals("1") || value.Equals("true", StringComparison.CurrentCultureIgnoreCase);
                            break;
                        case "Int32[]":
                            result = (HttpUtility.UrlDecode(value)).GetArray<int>().ToArray();
                            break;
                        case "Byte[]":
                            result = (HttpUtility.UrlDecode(value)).GetArray<byte>().ToArray();
                            break;
                        case "Decimal[]":
                            result = (HttpUtility.UrlDecode(value)).GetArray<decimal>().ToArray();
                            break;
                        case "String[]":
                            result = (HttpUtility.UrlDecode(value)).GetArray<string>().ToArray();
                            break;
                        case "Guid":
                            result = value.GetValue<Guid>();
                            break;
                        case nameof(TimeSpan):
                            result = value.GetValue<TimeSpan>();
                            break;
                        case nameof(Regex):
                            result = value.GetValue<Regex>();
                            break;
                        default:
                            if (property.PropertyType.IsArray)
                            {
                                Type? arrayType = property.PropertyType.GetElementType();
                                if (arrayType != null)
                                {
                                    string[] values = ((string)value).Split(',');
                                    System.Array array = System.Array.CreateInstance(arrayType, values.Length);
                                    for (int i = 0; i < array.Length; i++)
                                    {
                                        array.SetValue(values[i].ToEnum(arrayType), i);
                                    }
                                    result = array;
                                }
                            }
                            else if (property.PropertyType.IsEnum)
                            {
                                result = value.ToEnum(property.PropertyType);
                            }
                            // 字典类型
                            else if (property.PropertyType.IsBaseType<IDictionary>())
                            {
                                try
                                {
                                    result = JsonConvert.DeserializeObject(value, property.PropertyType);
                                }
                                catch
                                {
                                    result = Activator.CreateInstance(property.PropertyType);
                                }
                            }
                            else
                            {
                                result = value.GetValue(property.PropertyType);
                            }
                            break;
                    }
                    if (result != null)
                    {
                        property.SetValue(this, result, null);
                    }
                }
            }
        }

        /// <summary>
        /// 转化成为QueryString字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            List<string> list = new List<string>();
            foreach (PropertyInfo property in this.GetType().GetProperties())
            {
                if (!property.CanWrite || property.HasAttribute<IgnoreAttribute>()) continue;
                string value = this.GetValue(property);
                list.Add(string.Format("{0}={1}", property.Name, HttpUtility.UrlEncode(value)));
            }
            return string.Join("&", list);
        }

        private string GetValue(PropertyInfo property)
        {
            object? value = property.GetValue(this, null);
            if (value == null) return string.Empty;

            switch (property.PropertyType.Name)
            {
                case "Int32[]":
                    value = string.Join(",", (int[])value);
                    break;
                case "String[]":
                    value = string.Join(",", (string[])value);
                    break;
                default:
                    if (property.PropertyType.IsArray)
                    {
                        System.Array array = (System.Array)value;
                        string[] arrayValue = new string[array.Length];
                        for (int i = 0; i < array.Length; i++)
                        {
                            arrayValue[i] = array.GetValue(i)?.ToString() ?? string.Empty;
                        }
                        value = string.Join(",", arrayValue);
                    }
                    else if (property.PropertyType.IsBaseType<IDictionary>())
                    {
                        value = value == null ? "" : JsonConvert.SerializeObject(value);
                    }
                    break;
            }
            return value.ToString() ?? string.Empty;
        }

        /// <summary>
        /// 转化成为json 一个list对象
        /// </summary>
        /// <returns></returns>
        public virtual string ToSetting()
        {
            return this.ToSettingObject().ToJson();
        }

        /// <summary>
        /// 带属性搜索条件的配置字符串（JSON格式）返回
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public virtual string ToSetting(Func<PropertyInfo, bool> where)
        {
            return this.ToSettingObject(t => t.HasAttribute<DescriptionAttribute>() && where(t)).ToJson();
        }

        /// <summary>
        /// 属性配置对象
        /// </summary>
        /// <returns></returns>
        public virtual object ToSettingObject()
        {
            return this.ToSettingObject(t => t.HasAttribute<DescriptionAttribute>());
        }

        /// <summary>
        /// 键值对返回对应的值
        /// </summary>
        /// <returns></returns>
        public virtual Dictionary<string, object?> ToDictionary()
        {
            return this.GetType().GetProperties().Where(t => t.HasAttribute<DescriptionAttribute>()).ToDictionary(c => c.Name, c => c.GetValue(this, null));
        }

        /// <summary>
        /// 带自定义条件的属性配置对象
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public virtual object ToSettingObject(Func<PropertyInfo, bool> where)
        {
            return this.GetType().GetProperties().Where(where).Select(t =>
            {
                string type = t.PropertyType.FullName ?? string.Empty;
                string value = this.GetValue(t);

                return new
                {
                    t.Name,
                    Value = value,
                    Type = type,
                    t.GetAttribute<DescriptionAttribute>()?.Description
                };
            });
        }

        /// <summary>
        /// 默认转化成为字符串
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static implicit operator string(ISetting setting)
        {
            return setting.ToString();
        }
    }
}
