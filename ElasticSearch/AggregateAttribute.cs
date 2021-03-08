using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.ElasticSearch
{
    /// <summary>
    /// 聚合标记
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class AggregateAttribute : Attribute
    {
        public AggregateAttribute(AggregateType type)
        {
            this.Type = type;
        }
        /// <summary>
        /// 别名，可为空
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 聚合类型
        /// </summary>
        public AggregateType Type { get; set; }
    }
    /// <summary>
    /// 聚合类型
    /// </summary>
    public enum AggregateType
    {
        /// <summary>
        /// 求和
        /// </summary>
        Sum,
        /// <summary>
        /// 总数
        /// </summary>
        Count,
        /// <summary>
        /// 平均值
        /// </summary>
        Average,
        Max,
        Min

    }
}
