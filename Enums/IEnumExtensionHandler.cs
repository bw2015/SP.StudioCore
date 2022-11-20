using SP.StudioCore.Base;
using SP.StudioCore.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Enums
{
    /// <summary>
    /// 枚举扩展的实现基类
    /// </summary>
    public interface IEnumExtensionHandler
    {
        /// <summary>
        /// 获取枚举的备注信息
        /// </summary>
        string GetDescription(Enum @enum);
    }

    internal class DefaultEnumExtensionHandler : Instance<DefaultEnumExtensionHandler>, IEnumExtensionHandler
    {
        private static Dictionary<string, string> _cache = new Dictionary<string, string>();

        private static object _cacheLock = new();

        public string GetDescription(Enum @enum)
        {
            string enumName = @enum.ToString();
            Type type = @enum.GetType();
            string cacheKey = $"{type.FullName}.{enumName}";
            if (_cache.ContainsKey(cacheKey)) return _cache[cacheKey];

            lock (_cacheLock)
            {
                FieldInfo? field = type.GetField(enumName);
                if (field == null) return enumName;
                DescriptionAttribute? description = field.GetAttribute<DescriptionAttribute>();
                string value = description?.Description ?? enumName;
                if (!_cache.ContainsKey(cacheKey)) _cache.Add(cacheKey, value);
                return value;
            }
        }
    }
}
