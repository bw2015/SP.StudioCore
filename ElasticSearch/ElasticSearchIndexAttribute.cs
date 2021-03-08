using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.ElasticSearch
{
    /// <summary>
    /// ES索引名称标记
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ElasticSearchIndexAttribute : Attribute
    {
        /// <summary>
        /// 索引名称
        /// </summary>
        public string IndexName { get; }
        public ElasticSearchIndexAttribute(string indexname)
        {
            this.IndexName = indexname;
        }
    }
}
