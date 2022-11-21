using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Array
{
    /// <summary>
    /// 字典扩展
    /// </summary>
    public static class DictionaryExtendssion
    {
        /// <summary>
        /// 添加字典
        /// </summary>
        public static Dictionary<TKey, TValue> Add<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Dictionary<TKey, TValue> data) where TKey : notnull
        {
            dictionary ??= new Dictionary<TKey, TValue>(data);
            foreach (TKey key in data.Keys)
            {
                if (!dictionary.ContainsKey(key)) dictionary.Add(key, data[key]);
            }
            return dictionary;
        }
    }
}
