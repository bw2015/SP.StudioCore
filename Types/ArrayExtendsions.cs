using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SP.StudioCore.Types
{
    /// <summary>
    /// 数据扩展
    /// </summary>
    public static class ArrayExtendsions
    {
        /// <summary>
        /// 字符串转化成为数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <param name="split"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetArray<T>(this string str, char split = ',')
        {
            if (str == null) yield break;
            str = str.Replace(" ", string.Empty);
            switch (typeof(T).Name)
            {
                case "Guid":
                case "Decimal":
                case "Double":
                case "DateTime":
                case "Int32":
                case "Byte":
                case "Int64":
                    foreach (string item in str.Split(split))
                    {
                        if (item.IsType<T>()) yield return item.GetValue<T>();
                    }
                    break;
                case "String":
                    foreach (string item in str.Split(split).Where(t => !string.IsNullOrEmpty(t)))
                    {
                        yield return (T)(object)item.Trim();
                    }
                    break;
                default:
                    if (typeof(T).IsEnum)
                    {
                        foreach (string item in str.Split(split).Where(t => !string.IsNullOrEmpty(t)))
                        {
                            if (Enum.IsDefined(typeof(T), item))
                            {
                                yield return (T)Enum.Parse(typeof(T), item);
                            }
                        }
                    }
                    break;
            }
        }

    }
}
