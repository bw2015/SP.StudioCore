using Microsoft.EntityFrameworkCore.Internal;
using SP.StudioCore.Model;
using SP.StudioCore.Types;
using SP.StudioCore.Web;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using Array = System.Array;

namespace SP.StudioCore.Array
{
    /// <summary>
    /// 数据扩展
    /// </summary>
    public static class ArrayExtendssion
    {
        /// <summary>
        /// 获取参数名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <param name="argName"></param>
        /// <returns></returns>
        public static T Get<T>(this string[] args, string argName, T defaultValue)
        {
            int index = System.Array.IndexOf(args, argName);
            if (index == -1 || args.Length <= index + 1) return defaultValue;
            string value = args[index + 1];
            return value.GetValue<T>() ?? defaultValue;
        }

        public static string Get(this string[] args, string argName)
        {
            return args.Get(argName, string.Empty);
        }

        /// <summary>
        /// 字典转字符串
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="data"></param>
        /// <param name="urlEncode">是否进行URL转换</param>
        /// <returns></returns>
        public static string ToQueryString<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> data, bool urlEncode = false)
        {
            return string.Join("&", data.Select(t => $"{t.Key}={ (t.Value == null ? string.Empty : (urlEncode ? t.Value.ToString() : HttpUtility.UrlEncode(t.Value.ToString()))) }"));
        }

        /// <summary>
        /// 把匿名类转化成为 QueryString 格式字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToQueryString(this object obj, bool urlEncode = false)
        {
            if (obj == null) return string.Empty;
            List<string> list = new List<string>();
            foreach (PropertyInfo property in obj.GetType().GetProperties())
            {
                string name = property.Name;
                object? value = property.GetValue(obj);
                if (value == null)
                {
                    continue;
                }
                if (property.PropertyType == typeof(string) && urlEncode)
                {
                    list.Add($"{name}={HttpUtility.UrlEncode((string)value)}");
                }
                else
                {
                    list.Add($"{name}={value}");
                }
            }
            return string.Join("&", list);
        }

        /// <summary>
        /// 字典转化成为二维数组（方便前端转化成为Map对象）
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static object[][] AsArray<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> data)
        {
            List<object[]> list = new();
            foreach (KeyValuePair<TKey, TValue> item in data)
            {
                if (item.Key == null || item.Value == null) continue;
                list.Add(new object[] { item.Key, item.Value });
            }
            return list.ToArray();
        }

        /// <summary>
        /// 字符串转字典
        /// </summary>
        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this string queryString) where TKey : notnull
        {
            NameValueCollection? request = HttpUtility.ParseQueryString(queryString ?? string.Empty);
            Dictionary<TKey, TValue> data = new Dictionary<TKey, TValue>();
            if (request == null) return data;
            foreach (string? key in request.AllKeys)
            {
                if (key == null) continue;
                TKey tKey = key.GetValue<TKey>();
                string value = request?[key] ?? String.Empty;
                if (!data.ContainsKey(tKey)) data.Add(tKey, value.GetValue<TValue>());
            }
            return data;
        }

        public static Dictionary<string, string> ToDictionary(this string queryString)
        {
            return queryString.ToDictionary<string, string>();
        }

        /// <summary>
        /// 把队列转化成为字典（自动覆盖同名Key）
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="list"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> ToDistinctDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> list, Func<TSource, TKey> funKey, Func<TSource, TValue> funValue) where TKey : struct
        {
            Dictionary<TKey, TValue> dic = new Dictionary<TKey, TValue>();
            foreach (TSource item in list)
            {
                TKey key = funKey.Invoke(item);
                TValue value = funValue.Invoke(item);
                if (dic.ContainsKey(key))
                {
                    dic[key] = value;
                }
                else
                {
                    dic.Add(key, value);
                }
            }
            return dic;
        }

        public static IEnumerable<KeyValue<TKey, TValue>> ToList<TKey, TValue>(this string queryString)
        {
            return queryString.ToDictionary<TKey, TValue>().Select(t => new KeyValue<TKey, TValue>(t.Key, t.Value));
        }

        public static IEnumerable<KeyValue<string, string>> ToList(this string queryString)
        {
            return queryString.ToList<string, string>();
        }

        /// <summary>
        /// 获取字段对象的值
        /// </summary>
        public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> data, TKey key, TValue defaultValue)
        {
            if (data.ContainsKey(key)) return data[key];
            return defaultValue;
        }

        /// <summary>
        /// 从字典中获取对象值，如果Key不存在则返回默认值
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> data, TKey key)
        {
            if (data.ContainsKey(key)) return data[key];
            return default;
        }

        /// <summary>
        /// 乘法积
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static decimal Multiplication(this IEnumerable<decimal> list)
        {
            decimal result = decimal.One;
            foreach (decimal item in list)
            {
                result *= item;
            }
            return result;
        }

        /// <summary>
        /// 合并后的输出
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dic"></param>
        /// <param name="datas"></param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> Extend<TKey, TValue>(this Dictionary<TKey, TValue> dic, params Dictionary<TKey, TValue>[] datas)
        {
            if (datas == null) return dic;
            foreach (Dictionary<TKey, TValue> data in datas)
            {
                if (data == null) continue;
                foreach (KeyValuePair<TKey, TValue> item in data)
                {
                    if (dic.ContainsKey(item.Key))
                    {
                        dic[item.Key] = item.Value;
                    }
                    else
                    {
                        dic.Add(item.Key, item.Value);
                    }
                }
            }
            return dic;
        }

        /// <summary>
        /// 二维数组转一维数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static IEnumerable<T> AsArray<T>(this IEnumerable<T[]> array)
        {
            foreach (T[] item in array)
            {
                foreach (T t in item)
                {
                    yield return t;
                }
            }
        }

        /// <summary>
        /// 一维数组转二维数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="array"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public static IEnumerable<T[]> AsArray<T, TValue>(this IEnumerable<T> array, Func<T, TValue> keySelector)
        {
            return array.GroupBy(keySelector).Select(t => t.ToArray());
        }

        /// <summary>
        /// 二维数组的循环处理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="action"></param>
        public static void ForEach<T>(this T[][] array, Action<T> action)
        {
            for (int i = 0; i < array.Length; i++)
            {
                for (int j = 0; j < array[i].Length; j++)
                {
                    action(array[i][j]);
                }
            }
        }

        /// <summary>
        /// 二维数组判断是否存在
        /// </summary>
        public static bool Any<T>(this T[][] array, Func<T, bool> predicate)
        {
            foreach (T[] items in array)
            {
                if (items.Any(predicate)) return true;
            }
            return false;
        }

        /// <summary>
        /// 从数组中随机取出一个
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T GetRandom<T>(this IEnumerable<T> list)
        {
            if (!list.Any()) return default(T);

            T[] array = list.ToArray();
            if (array.Length == 1) return array[0];

            return array[WebAgent.GetRandom(0, array.Length)];
        }
    }
}