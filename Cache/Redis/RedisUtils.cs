using SP.StudioCore.Enums;
using SP.StudioCore.Types;
using SP.StudioCore.Web;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SP.StudioCore.Cache.Redis
{

    public static class RedisUtils
    {
        /// <summary>
        /// 把缓存实体类转化成为Redis参数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerable<HashEntry> ToHashEntry<T>(this T obj) where T : class, new()
        {
            IEnumerable<PropertyInfo> fields = obj.GetType().GetProperties();
            if (typeof(T).HasAttribute<ICacheAttribute>()) fields = fields.Where(t => t.HasAttribute<ICacheAttribute>());
            foreach (PropertyInfo property in fields.Where(t => !t.HasAttribute<NotMappedAttribute>()))
            {
                object value = null;
                string name = null;
                RedisValue redisValue;
                try
                {
                    name = property.Name;
                    value = property.GetValue(obj);
                    redisValue = value.GetRedisValue();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message + "\n" + string.Format("{0}.{1}={2}({3})转换成为RedisValue无效", typeof(T).Name, name, value, value.GetType().FullName));
                }
                yield return new HashEntry(name, redisValue);
            }
        }

        public static IEnumerable<HashEntry> ToHash<T>(this T obj) where T : struct
        {
            IEnumerable<FieldInfo> fields = obj.GetType().GetFields();
            foreach (FieldInfo field in fields.Where(t => t.IsPublic))
            {
                RedisValue? value = field.GetValue(obj)?.GetRedisValue();
                if (value == null) continue;
                yield return new HashEntry(field.Name, value.Value);
            }
        }

        public static IEnumerable<HashEntry> ToHashEntry<T>(this T t, params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            bool isHasCache = typeof(T).HasAttribute<ICacheAttribute>();
            foreach (Expression<Func<T, object>> field in fields)
            {
                PropertyInfo property = field.ToPropertyInfo();
                if (isHasCache && !property.HasAttribute<ICacheAttribute>()) continue;
                object value = property.GetValue(t);
                if (value != null) yield return new HashEntry(property.Name, value.GetRedisValue());
            }
        }

        /// <summary>
        /// 获取hash表的key-value对象
        /// 不存在的键值自动过滤
        /// </summary>
        /// <param name="db"></param>
        /// <param name="key"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static IEnumerable<HashEntry> GetHashValue(this IDatabase db, RedisKey key, params RedisValue[] fields)
        {
            RedisValue[] values = db.HashGet(key, fields);
            for (int i = 0; i < fields.Length; i++)
            {
                if (!values[i].IsNull) yield return new HashEntry(fields[i], values[i]);
            }
        }

        /// <summary>
        /// 把object转化成为Redis支持的格式
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static RedisValue GetRedisValue(this object obj)
        {
            if (obj == null) return RedisValue.Null;
            Type type = obj.GetType();
            RedisValue value;

            switch (type.Name)
            {
                case "String":
                    value = (string)obj;
                    break;
                case "Int32":
                    value = (int)obj;
                    break;
                case "Int16":
                    value = (short)obj;
                    break;
                case "Byte":
                    value = (int)(byte)obj;
                    break;
                case "Int64":
                    value = (long)obj;
                    break;
                case "Float":
                    value = (float)obj;
                    break;
                case "Double":
                    value = (double)obj;
                    break;
                case "Decimal":
                    value = ((decimal)obj).ToRedisValue();
                    break;
                case "Byte[]":
                    value = (byte[])obj;
                    break;
                case "Int32[]":
                    value = string.Join(",", (int[])obj);
                    break;
                case "Decimal[]":
                    value = string.Join(",", (decimal[])obj);
                    break;
                case "String[]":
                    value = string.Join(",", (string[])obj);
                    break;
                case "DateTime":
                    value = ((DateTime)obj).Ticks;
                    break;
                case "Boolean":
                    value = (bool)obj;
                    break;
                case "Guid":
                    value = ((Guid)obj).ToString("N");
                    break;
                default:
                    if (type.IsEnum)
                    {
                        switch (Enum.GetUnderlyingType(type).Name)
                        {
                            case "Int64":
                                value = (long)obj;
                                break;
                            case "Int32":
                                value = (int)obj;
                                break;
                            case "Int16":
                                value = (short)obj;
                                break;
                            case "Byte":
                            default:
                                value = (int)(byte)obj;
                                break;
                        }
                    }
                    else
                    {
                        value = obj.ToString();
                    }
                    break;
            }
            return value;
        }

        /// <summary>
        /// 把Redis转成系统值
        /// 如果redisValue为null则返回默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T GetRedisValue<T>(this RedisValue value)
        {
            if (value.IsNull) return default;
            return (T)value.GetRedisValue(typeof(T));
        }

        /// <summary>
        /// Redis值转化成为系统值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetRedisValue(this RedisValue value, Type type)
        {
            object obj;
            switch (type?.Name)
            {
                case "Decimal":
                    obj = ((long)value).ToRedisValue();
                    break;
                case "Boolean":
                    obj = (bool)value;
                    break;
                case "DateTime":
                    obj = new DateTime((long)value);
                    break;
                case "Int64":
                    obj = (long)value;
                    break;
                case "Int32":
                    obj = (int)value;
                    break;
                case "Int16":
                    obj = (short)value;
                    break;
                case "Byte":
                    obj = (byte)(int)value;
                    break;
                case "String":
                    obj = (string)value;
                    break;
                case "Float":
                    obj = (float)value;
                    break;
                case "Int32[]":
                    obj = WebAgent.GetArray<int>(value);
                    break;
                case "Decimal[]":
                    obj = WebAgent.GetArray<decimal>(value);
                    break;
                case "String[]":
                    obj = WebAgent.GetArray<string>(value);
                    break;
                case "Guid":
                    obj = ((string)value).GetValue<Guid>();
                    break;
                case null:
                    obj = null;
                    break;
                default:
                    if (type.IsEnum)
                    {
                        obj = type.GetValue((long)value);
                    }
                    else
                    {
                        obj = value;
                    }
                    break;
            }
            return obj;
        }

        /// <summary>
        /// 整形与MONEY型互转（保留四位小数）
        /// </summary>
        /// <param name="money"></param>
        /// <returns></returns>
        public static long ToRedisValue(this decimal money)
        {
            return (long)(money * 10000M);
        }

        /// <summary>
        /// 整形转为MONEY（保留四位小数）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static decimal ToRedisValue(this long value)
        {
            return (decimal)value / 10000M;
        }

        /// <summary>
        /// 整个对象赋值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static T GetRedisValue<T>(this IEnumerable<HashEntry> fields) where T : class, new()
        {
            if (!fields.Any()) return null;
            T t = new T();
            foreach (HashEntry item in fields)
            {
                string name = (string)item.Name;
                if (name.Contains('.')) continue;
                PropertyInfo property = typeof(T).GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                if (property == null || !property.CanWrite) continue;
                property.SetValue(t, item.Value.GetRedisValue(property.PropertyType));
            }
            return t;
        }

        public static T GetRedisHash<T>(this IEnumerable<HashEntry> fields) where T : struct
        {
            if (!fields.Any()) return default;
            T t = new T();
            foreach (HashEntry item in fields)
            {
                string name = (string)item.Name;
                if (name.Contains('.')) continue;
                FieldInfo field = typeof(T).GetField(name, BindingFlags.Public | BindingFlags.Instance);
                if (field == null) continue;
                field.SetValue(t, item.Value.GetRedisValue(field.FieldType));
            }
            return t;
        }

        /// <summary>
        /// 把对象转化成Redis的Hash对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerable<HashEntry> GetRedisEntry<T>(this T obj) where T : class, new()
        {
            foreach (PropertyInfo property in typeof(T).GetProperties())
            {
                string name = property.Name;
                object value = property.GetValue(obj);
                yield return new HashEntry(name, value.GetRedisValue());
            }
        }

        /// <summary>
        /// 把缓存值取出直接赋值给实体对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fields"></param>
        public static T Fill<T>(this IEnumerable<HashEntry> fields) where T : class, new()
        {
            if (fields == null || !fields.Any()) return default;
            T t = new T();
            t.Fill(fields);
            return t;
        }

        /// <summary>
        /// 从缓存读取出的对象赋值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="fields"></param>
        /// <param name="prefix">前缀名</param>
        public static void Fill<T>(this T t, IEnumerable<HashEntry> fields, string prefix = null) where T : class, new()
        {
            foreach (HashEntry item in fields)
            {
                string name = (string)item.Name;
                if (!string.IsNullOrEmpty(prefix) && name.StartsWith(prefix + ".", StringComparison.Ordinal))
                {
                    name = name.Substring(prefix.Length + 1);
                }
                if (name.Contains('.', StringComparison.Ordinal)) continue;
                PropertyInfo property = typeof(T).GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                if (property == null || !property.CanWrite) continue;
                property.SetValue(t, item.Value.GetRedisValue(property.PropertyType));
            }
        }

        /// <summary>
        /// 保存多语种
        /// </summary>
        /// <param name="batch">批处理对象</param>
        /// <param name="key">Hash 的Key</param>
        /// <param name="name">标题字段名</param>
        /// <param name="tranlsate">语言包</param>
        public static void SaveLanguage(this IBatch batch, string key, Dictionary<Language, string> tranlsate, string name = "Name")
        {
            if (tranlsate == null) return;
            foreach (KeyValuePair<Language, string> item in tranlsate)
            {
                batch.HashSetAsync(key, $"{name}_{item.Key}", item.Value.GetRedisValue());
            }
        }

        /// <summary>
        /// 获取语言包内容
        /// </summary>
        /// <param name="hashes"></param>
        /// <param name="language"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetLanguage(this HashEntry[] hashes, Language language, string name = "Name")
        {
            RedisValue key = $"{name}_{language}".GetRedisValue();
            return hashes.FirstOrDefault(t => t.Name == key).Value.GetRedisValue<string>();
        }
    }
}
