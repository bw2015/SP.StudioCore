using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Nest;
using SP.StudioCore.Ioc;
using SP.StudioCore.Json;

namespace SP.StudioCore.ElasticSearch
{
    public abstract class EsBase<TAgent, TModel> where TModel : class where TAgent : class
    {
        private readonly        string                   _prefixIndexName;
        private readonly        int                      _replicasCount;
        private readonly        int                      _shardsCount;
        private readonly        string                   _splitIndexDateFormat;
        protected readonly      IElasticClient           Client     = IocCollection.GetService<IElasticClient>();
        protected readonly      ILogger<TAgent>          Logger     = IocCollection.GetService<ILoggerFactory>().CreateLogger<TAgent>();
        private static readonly Dictionary<string, bool> IndexCache = new();
        protected               string                   IndexName => $"{_prefixIndexName}_{DateTime.Now.ToString(_splitIndexDateFormat)}";
        protected readonly      string                   AliasName;

        /// <summary>
        /// ES基类
        /// </summary>
        /// <param name="prefixIndexName">索引前缀</param>
        /// <param name="aliasName">别名</param>
        /// <param name="shardsCount">分片数量</param>
        /// <param name="splitIndexDateFormat">按时间分索引</param>
        /// <param name="replicasCount">副本数量</param>
        public EsBase(string prefixIndexName, string aliasName, int replicasCount = 0, int shardsCount = 3, string splitIndexDateFormat = "yyyy_MM")
        {
            _prefixIndexName      = prefixIndexName;
            AliasName             = aliasName;
            _replicasCount        = replicasCount;
            _shardsCount          = shardsCount;
            _splitIndexDateFormat = splitIndexDateFormat;
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        public virtual bool Insert(TModel model)
        {
            WhenNotExistsAddIndex();

            var result = Client.Index(new IndexRequest<TModel>(model, IndexName));
            if (!result.IsValid)
            {
                Logger.LogError($"索引失败：{model.ToJson()} \r\n" + result.OriginalException.Message);
            }

            return result.IsValid;
        }

        /// <summary>
        /// 索引不存在时，创建索引
        /// </summary>
        protected void WhenNotExistsAddIndex()
        {
            if (!IndexCache.ContainsKey(IndexName) || !IndexCache[IndexName] )
            {
                if (!Client.Indices.Exists(IndexName).Exists)
                {
                    IndexCache[IndexName] =  CreateIndex();
                }
            }
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        protected bool CreateIndex()
        {
            var rsp = Client.Indices.Create(IndexName, c => c
                .Map<TModel>(m => m.AutoMap())
                .Aliases(des => des.Alias(AliasName)).Settings(s => s.NumberOfReplicas(_replicasCount).NumberOfShards(_shardsCount))
            );
            return rsp.IsValid;
        }
    }
}