using SP.StudioCore.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.Model
{
    /// <summary>
    /// 在设置中自定义类型名字
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class SettingCustomTypeAttribute : Attribute
    {
        public SettingCustomTypeAttribute(string type)
        {
            this.Type = type;
        }

        public SettingCustomTypeAttribute(Type type)
        {
            this.Type = type.FullName;
        }

        public SettingCustomTypeAttribute(SettingCustomType type)
        {
            this.Type = type.ToString();
        }

        /// <summary>
        /// 自己定义的名字
        /// </summary>
        public string? Type { get; set; }

        public static implicit operator string?(SettingCustomTypeAttribute? attribute)
        {
            if (attribute == null) return null;
            return attribute.Type;
        }
    }

    /// <summary>
    /// 自定义类型
    /// </summary>
    [EnumConfig(ignore: true)]
    public enum SettingCustomType
    {
        /// <summary>
        /// 上传控件
        /// </summary>
        Upload,
        /// <summary>
        /// 链接对象
        /// </summary>
        Link
    }
}
