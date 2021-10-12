using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace SP.StudioCore.Mapper
{
    public static class MapperExtension
    {
        private static Dictionary<Type, PropertyInfo[]> _property = new Dictionary<Type, PropertyInfo[]>();
        /// <summary>
        /// 映射单个实体
        /// </summary>
        /// <typeparam name="Target">来源</typeparam>
        /// <typeparam name="TSource">目标</typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Target? Map<Target>(this object source) where Target : class, new()
        {
            if (source == null) return default;
            Type type = typeof(Target);
            Target result = new Target();
            PropertyInfo[] properties = source.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                object? value = property.GetValue(source);
                if (value == null) continue;
                PropertyInfo? resultProperty = type.GetProperty(property.Name);
                if (resultProperty == null
                    ||
                    resultProperty.PropertyType != property.PropertyType
                    ||
                    !resultProperty.CanWrite) continue;
                resultProperty.SetValue(result, Convert.ChangeType(value, property.PropertyType), null);
            }

            return result;
        }
        /// <summary>
        /// 映射List
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="Target"></typeparam>
        /// <param name="sources"></param>
        /// <returns></returns>
        public static List<Target>? Map<Target>(this IEnumerable<object> sources) where Target : class, new()
        {
            if (sources == null) return null;
            List<Target> targets = new List<Target>();
            foreach (object source in sources)
            {
                Target? target = Map<Target>(source);
                if (target == null) continue;
                targets.Add(target);
            }
            return targets;
        }

        /// <summary>
        /// 获取属性
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static PropertyInfo[] GetProperties(this object source)
        {
            Type type = source.GetType();
            if (!_property.ContainsKey(type))
            {
                lock (_property)
                {
                    if (!_property.ContainsKey(type))
                    {
                        _property.Add(type, type.GetProperties());
                    }
                }
            }
            return _property[type];
        }
    }
}