using System;

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
        /// <summary>
        /// 别名
        /// </summary>
        public string[] AliasNames { get; }
        /// <summary>
        /// 副本数量
        /// </summary>
        public int ReplicasCount { get; }
        /// <summary>
        /// 分片数量
        /// </summary>
        public int ShardsCount { get; }
        /// <summary>
        /// 格式
        /// </summary>
        public string Format { get; }

        /// <summary>
        /// 初始构造
        /// </summary>
        /// <param name="indexname"></param>
        public ElasticSearchIndexAttribute(string indexname) : this(indexname, new[] { indexname })
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexname">索引名称</param>
        /// <param name="aliasnams">别名</param>
        /// <param name="replicascount">副本数量</param>
        /// <param name="shardscount">分片数量</param>
        /// <param name="fomat">格式</param>
        public ElasticSearchIndexAttribute(string indexname, string[] aliasnams, int replicascount = 0, int shardscount = 3, string fomat = "yyyyMM")
        {
            this.AliasNames = aliasnams;
            this.ReplicasCount = replicascount;
            this.ShardsCount = shardscount;
            this.Format = fomat;
            this.IndexName = $"{indexname}_{DateTime.Now.ToString(fomat)}";
        }
    }
}
