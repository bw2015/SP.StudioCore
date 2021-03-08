using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.Model
{
    /// <summary>
    /// 日期类型
    /// </summary>
    public class DateTimeAttribute : IsoDateTimeConverter
    {
        public DateTimeAttribute()
        {
            this.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        }

        public DateTimeAttribute(string format)
        {
            this.DateTimeFormat = format;
        }
    }

    /// <summary>
    /// 标记属性为精确到分的时间类型
    /// </summary>
    public sealed class SmallDateTimeAttribute : DateTimeAttribute
    {
        public SmallDateTimeAttribute() : base("yyyy-MM-dd HH:mm")
        {
        }
    }

    /// <summary>
    /// 标记为日期类型
    /// </summary>
    public sealed class DateAttribute : DateTimeAttribute
    {
        public DateAttribute() : base("yyyy-MM-dd")
        {
        }
    }

    /// <summary>
    /// 字段长度
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class LengthAttribute : Attribute
    {
        public LengthAttribute(int length, bool isUnicode = false)
        {
            this.Length = length;
            this.IsUnicode = isUnicode;
        }

        /// <summary>
        /// 长度
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// 是否是nvarchar类型
        /// </summary>
        public bool IsUnicode { get; set; }
    }
}
