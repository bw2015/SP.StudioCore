using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.Types
{
    /// <summary>
    /// 字典扩展
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        ///  扩展 Dictionary 方法，如果没有Key值则返回默认值
        /// </summary>
        /// <returns></returns>
        public static TValue Get<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, TValue defaultValue) where TKey : notnull
        {
            if (dic.ContainsKey(key)) return dic[key];
            return defaultValue;
        }
    }
}
