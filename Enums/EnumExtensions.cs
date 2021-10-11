using SP.StudioCore.Types;
using SP.StudioCore.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace SP.StudioCore.Enums
{
    public static class EnumExtensions
    {
        /// <summary>
        /// 把字符串转化成为枚举（支持位枚举，使用逗号隔开）
        /// </summary>
        public static T ToEnum<T>(this string value) where T : struct, IComparable, IFormattable, IConvertible
        {
            if (string.IsNullOrEmpty(value) || !typeof(T).IsEnum) return default;
            if (value.IsType<int>())
            {
                return (T)Enum.ToObject(typeof(T), int.Parse(value));
            }
            Type type = typeof(T);

            if (type.HasAttribute<FlagsAttribute>())
            {
                return ToFlagEnum<T>(value.Split(',').Where(t => !string.IsNullOrWhiteSpace(t) && Enum.IsDefined(type, t.Trim())).Select(t => Enum.Parse(type, value)).ToArray());
            }
            return Enum.IsDefined(type, value) ? (T)Enum.Parse(type, value) : default;
        }

        /// <summary>
        /// 获取位枚举的和值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enums"></param>
        /// <returns></returns>
        private static T ToFlagEnum<T>(object[] enums) where T : struct, IComparable, IFormattable, IConvertible
        {
            T result;
            switch (Enum.GetUnderlyingType(typeof(T)).Name)
            {
                case "Int64":
                    long Int64 = 0;
                    foreach (object value in enums) Int64 |= (long)value;
                    result = (T)Enum.ToObject(typeof(T), Int64);
                    break;
                case "Int32":
                    int Int32 = 0;
                    foreach (object value in enums) Int32 |= (int)value;
                    result = (T)Enum.ToObject(typeof(T), Int32);
                    break;
                case "Int16":
                    short Int16 = 0;
                    foreach (object value in enums) Int16 |= (short)value;
                    result = (T)Enum.ToObject(typeof(T), Int16);
                    break;
                case "Byte":
                    byte Byte = 0;
                    foreach (object value in enums) Byte |= (byte)value;
                    result = (T)Enum.ToObject(typeof(T), Byte);
                    break;
                default:
                    result = default;
                    break;
            }
            return result;
        }

        public static object? ToEnum(this string value, Type type)
        {
            MethodInfo? mi = typeof(EnumExtensions).GetMethods().FirstOrDefault(t => t.Name == "ToEnum" && t.IsGenericMethod);
            MethodInfo gmi = mi.MakeGenericMethod(type);
            return gmi.Invoke(null, new object[] { value });
        }

        /// <summary>
        /// 枚举的备注信息缓存
        /// KEY = {typeof(Enum).FullName}.{Enum}.{Language}
        /// VALUE = 对应语言包的备注内容
        /// </summary>
        private static Dictionary<string, string> _enumDescription = new Dictionary<string, string>();
        /// <summary>
        /// 获取枚举的备注信息（可读取语言包）
        /// </summary>
        public static string GetDescription(this Enum em)
        {
            string fullName = $"{em.GetType().FullName}";
            string key = $"{fullName}.{em}";
            string enumKey = $"{key}";

            if (_enumDescription.ContainsKey(enumKey)) return _enumDescription[enumKey];

            lock (LockHelper.GetLoker(fullName))
            {
                foreach (FieldInfo field in em.GetType().GetFields())
                {
                    if (field.IsSpecialName) continue;
                    DescriptionAttribute? description = field.GetAttribute<DescriptionAttribute>();
                    string value = string.Empty;
                    if (description == null)
                    {
                        value = field.Name;
                    }
                    else
                    {
                        value = description.Description;
                    }
                    if (!_enumDescription.ContainsKey($"{fullName}.{field.Name}"))
                    {
                        _enumDescription.Add($"{fullName}.{field.Name}", value);
                    }
                }
                if (_enumDescription.ContainsKey(enumKey)) return _enumDescription[enumKey];
            }
            return em.ToString();
        }

        /// <summary>
        ///  获取枚举的属性值
        /// </summary>
        /// <typeparam name="TAttributes"></typeparam>
        /// <param name="em"></param>
        /// <returns></returns>
        public static TAttributes GetAttribute<TAttributes>(this Enum em) where TAttributes : Attribute
        {
            foreach (FieldInfo field in em.GetType().GetFields().Where(t => t.Name == em.ToString()))
            {
                if (field.IsSpecialName) continue;
                return field.GetAttribute<TAttributes>();
            }
            return default;
        }

        /// <summary>
        /// 获取资源内的全部枚举
        /// </summary>
        /// <param name="ass"></param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, string>> GetEnums(this Assembly ass)
        {
            var dic = new Dictionary<string, Dictionary<string, string>>();
            foreach (Type type in ass.GetTypes().Where(t => t.IsEnum))
            {
                string enumName = type.FullName;
                dic.Add(enumName, new Dictionary<string, string>());
                foreach (Enum item in Enum.GetValues(type))
                {
                    dic[enumName].Add(item.ToString(), item.GetDescription());
                }
            }
            return dic;
        }

        /// <summary>
        /// 获取指定类型的枚举内容
        /// </summary>
        /// <param name="enums"></param>
        /// <param name="language"></param>
        /// <param name="isFullName">是否使用全路径</param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, string>> GetEnums(this Type[] enums, bool isFullName = false)
        {
            var dic = new Dictionary<string, Dictionary<string, string>>();
            foreach (Type type in enums)
            {
                string? name = isFullName ? type.FullName : type.Name;
                if (name == null || dic.ContainsKey(name)) continue;
                dic.Add(name, new Dictionary<string, string>());
                foreach (object item in Enum.GetValues(type))
                {
                    if (item == null) continue;
                    string? key = item.ToString();
                    string? description = ((Enum)item).GetDescription();
                    if(key == null || description == null) continue;
                    dic[name].Add(key, description);
                }
            }
            return dic;
        }

        /// <summary>
        /// 把数值转化成为枚举的数值
        /// </summary>
        /// <param name="enumType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object? GetValue(this Type enumType, long value)
        {
            if (!enumType.IsEnum) return null;
            object? res = null;
            switch (Enum.GetUnderlyingType(enumType).Name)
            {
                case "Int64":
                    res = value;
                    break;
                case "Int32":
                    res = (int)value;
                    break;
                case "Int16":
                    res = (short)value;
                    break;
                case "Byte":
                    res = (byte)value;
                    break;
            }
            return res;
        }

        /// <summary>
        /// 位枚举转化成为字典类型
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Dictionary<T, bool> ToDictionary<T>(this T value) where T : IComparable, IFormattable, IConvertible
        {
            if (!typeof(T).HasAttribute<FlagsAttribute>()) return null;
            Dictionary<T, bool> dic = new Dictionary<T, bool>();
            foreach (T t in Enum.GetValues(typeof(T)))
            {
                if ((value as Enum).HasFlag(t as Enum))
                {
                    dic.Add(t, true);
                }
            }
            return dic;
        }

        /// <summary>
        /// 枚举类型转换成为队列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<T> ToEnumerable<T>(this Type type) where T : IComparable, IFormattable, IConvertible
        {
            foreach (T t in Enum.GetValues(type))
            {
                yield return t;
            }
        }
    }
}
