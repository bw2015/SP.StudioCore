using Newtonsoft.Json;
using SP.StudioCore.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SP.StudioCore.Model
{
    /// <summary>
    /// JSON格式的参数配置类
    /// </summary>
    public abstract class IJsonSetting : ISetting
    {
        public IJsonSetting() { }

        public IJsonSetting(string jsonString)
        {
            if (!string.IsNullOrEmpty(jsonString) && jsonString.StartsWith("{"))
            {
                try
                {
                    JsonConvert.PopulateObject(jsonString, this);
                }
                catch
                {
                    Console.WriteLine($"錯誤的數據格式:{ jsonString }");
                }
            }
        }

        /// <summary>
        /// 转化成为JSON格式字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static implicit operator string(IJsonSetting setting)
        {
            return setting.ToString();
        }

        /// <summary>
        /// 返回可以配置的内容
        /// </summary>
        /// <returns></returns>
        public override object ToSettingObject()
        {
            return this.GetType().GetProperties().Where(t => !t.HasAttribute<IgnoreAttribute>() && t.HasAttribute<DescriptionAttribute>()).Select(t =>
            {
                string type = this.GetTypeName(t);
                return new
                {
                    t.Name,
                    Value = t.GetValue(this, null),
                    t.GetAttribute<DescriptionAttribute>().Description,
                    Type = type,
                    List = this.GetList(t, type),
                    t.PropertyType.IsEnum
                };
            });
        }

        private string GetTypeName(PropertyInfo property)
        {
            if (property.HasAttribute<SettingCustomTypeAttribute>()) return property.GetAttribute<SettingCustomTypeAttribute>();
            if (property.PropertyType.IsGenericType) return "List";
            return property.PropertyType.FullName;
        }

        private object GetList(PropertyInfo property, string type)
        {
            object result = null;
            if (type == "List")
            {
                result = new
                {
                    Value = property.GetValue(this),
                    Field = property.PropertyType.GetGenericArguments()[0].GetProperties().Where(t => t.HasAttribute<DescriptionAttribute>()).Select(t => new
                    {
                        t.Name,
                        Type = this.GetTypeName(t),
                        t.GetAttribute<DescriptionAttribute>().Description
                    })
                };
            }
            return result;

        }
    }
}
