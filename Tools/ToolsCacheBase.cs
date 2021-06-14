using SP.StudioCore.Cache.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Tools
{
    public abstract class ToolsCacheBase<T> : RedisCacheBase where T : class, new()
    {
        public ToolsCacheBase(string redisConnection) : base(redisConnection) { }

        private static T _intance;
        /// <summary>
        /// 单例模式
        /// </summary>
        /// <returns></returns>
        public static T Instance()
        {
            if (_intance == null) _intance = new T();
            return _intance;
        }
    }
}
