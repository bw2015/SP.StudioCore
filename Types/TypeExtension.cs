using SP.StudioCore.Enums;
using SP.StudioCore.Model;
using SP.StudioCore.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;


namespace SP.StudioCore.Types
{
    /// <summary>
    /// 类型扩展
    /// </summary>
    public static class TypeExtension
    {
        /// <summary>
        /// 把字符串类型转化成为安全类型（如果错误则返回默认值）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T? GetValue<T>(this object value)
        {
            return (T?)value.GetValue(typeof(T));
        }

        /// <summary>
        /// 转化成为安全类型
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object? GetValue(this object? value, Type? type = null)
        {
            if (value == null && type == null) throw new NullReferenceException();
            if (type == null)
            {
                type = value?.GetType();
                if (type == null) return null;
            }
            if (value == null || value == DBNull.Value)
            {
                return type?.GetDefaultValue();
            }
            object? obj = null;

            bool isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            if (isNullable)
            {
                type = type.GetGenericArguments().FirstOrDefault();
            }
            switch (value.GetType().Name)
            {
                case "DateTime":
                    if (isNullable && (DateTime)value == DateTime.MinValue)
                    {
                        obj = null;
                    }
                    else
                    {
                        obj = (DateTime)value == DateTime.MinValue ? DateTime.Parse("1900-1-1") : value;
                    }
                    break;
                case "String":
                    string str = (string)value;
                    switch (type?.Name)
                    {
                        case "Guid":
                            if (Guid.TryParse((string)value, out Guid guid))
                            {
                                obj = guid;
                            }
                            else
                            {
                                obj = Guid.Empty;
                            }
                            break;
                        case "Boolean":
                            obj = str.Equals("1") || str.Equals("true", StringComparison.CurrentCultureIgnoreCase) || str.Equals("on", StringComparison.CurrentCultureIgnoreCase);
                            break;
                        case "Decimal":
                            obj = decimal.TryParse((string)value, out decimal decimalValue) ? decimalValue : 0.00M;
                            break;
                        case "Double":
                            obj = double.TryParse((string)value, out double doubleValue) ? doubleValue : 0.00D;
                            break;
                        case "Int64":
                            obj = long.TryParse((string)value, out long longValue) ? longValue : 0;
                            break;
                        case "Int32":
                            if (int.TryParse((string)value, out int intValue))
                            {
                                if (isNullable)
                                {
                                    obj = (int?)intValue;
                                }
                                else
                                {
                                    obj = intValue;
                                }
                            }
                            else if (isNullable)
                            {
                                obj = default(int?);
                            }
                            else
                            {
                                obj = default(int);
                            }
                            break;
                        case "Int16":
                        case "Short":
                            obj = short.TryParse((string)value, out short shortValue) ? shortValue : (short)0;
                            break;
                        case "Single":
                            obj = float.TryParse((string)value, out float floatValue) ? floatValue : 0;
                            break;
                        case "Byte":
                            byte byteValue;
                            obj = byte.TryParse((string)value, out byteValue) ? byteValue : (byte)0;
                            break;
                        case "DateTime":
                            DateTime dateTime;
                            if (DateTime.TryParse((string)value, out dateTime))
                            {
                                if (isNullable)
                                {
                                    obj = (DateTime?)dateTime;
                                }
                                else
                                {
                                    obj = dateTime;
                                }
                            }
                            else if (isNullable)
                            {
                                obj = null;
                            }
                            else
                            {
                                obj = new DateTime(1900, 1, 1);
                            }
                            break;
                        case nameof(TimeSpan):
                            if (TimeSpan.TryParse((string)value, out TimeSpan timeSpan))
                            {
                                obj = timeSpan;
                            }
                            else
                            {
                                obj = new TimeSpan();
                            }
                            break;
                        case "Int32[]":
                        case "System.Int32[]":
                            obj = ((string)value).GetArray<int>().ToArray();
                            break;
                        case "String[]":
                            obj = ((string)value).GetArray<string>().ToArray();
                            break;
                        case nameof(String):
                            obj = str;
                            break;
                        case nameof(Regex):
                            try
                            {
                                obj = new Regex(str);
                            }
                            catch
                            {
                                obj = null;
                            }
                            break;
                        default:
                            if (type.IsBaseType<ISetting>())
                            {
                                obj = Activator.CreateInstance(type, (string)value);
                            }
                            else if (type.IsEnum)
                            {
                                if (isNullable && !Enum.IsDefined(type, (string)value))
                                {
                                    return obj = null;
                                }
                                else
                                {
                                    obj = ((string)value).ToEnum(type);
                                }
                            }
                            else if (type.IsArray)
                            {
                                Type? elemType = type.GetElementType();
                                if (elemType != null)
                                {
                                    string[] values = ((string)value).Split(',');
                                    System.Array array = System.Array.CreateInstance(elemType, values.Length);
                                    for (int i = 0; i < array.Length; i++)
                                    {
                                        array.SetValue(values[i].GetValue(elemType), i);
                                    }
                                    obj = array;
                                }
                            }
                            break;
                    }
                    break;
                default:
                    if (type.IsBaseType<ISetting>()) obj = Activator.CreateInstance(type, value.ToString());
                    else obj = value;
                    break;
            }
            return obj;
        }

        internal static object? GetDefaultValue(this Type? type)
        {
            if (type == null) return null;
            if (type == typeof(string)) return string.Empty;
            if (type == typeof(Guid)) return Guid.Empty;
            if (type == typeof(object[]) || type == typeof(int[])) return null;
            try
            {
                return Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("{0}\nType:{1}", ex.Message, type.FullName));
            }
        }

        /// <summary>
        /// 得到安全类型
        /// 1、不能为空
        /// 2、日期大于1900
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetSafeValue(this object value, Type type)
        {
            return type.Name switch
            {
                "String" => value ?? string.Empty,
                "DateTime" => ((DateTime)value).Max(),
                _ => value
            };
        }

        public static bool IsType(this string value, Type type)
        {
            bool isType;
            switch (type.Name)
            {
                case "Int32":
                    isType = int.TryParse(value, out _);
                    break;
                case "Int16":
                    isType = short.TryParse(value, out _);
                    break;
                case "Int64":
                    isType = long.TryParse(value, out _);
                    break;
                case "Guid":
                    isType = Guid.TryParse(value, out _);
                    break;
                case "DateTime":
                    isType = DateTime.TryParse(value, out _);
                    break;
                case "Decimal":
                    isType = decimal.TryParse(value, out _);
                    break;
                case "Double":
                    isType = double.TryParse(value, out _);
                    break;
                case "String":
                    isType = true;
                    break;
                case "Boolean":
                    isType = Regex.IsMatch(value, "1|0|true|false", RegexOptions.IgnoreCase);
                    break;
                case "Byte":
                    isType = byte.TryParse(value, out _);
                    break;
                default:
                    if (type.IsEnum)
                    {
                        isType = Enum.IsDefined(type, value);
                    }
                    else
                    {
                        throw new Exception("WebAgent.IsType 方法暂时未能检测该种类型" + type.FullName);
                    }
                    break;
            }
            return isType;
        }

        public static bool IsType<T>(this string value)
        {
            return IsType(value, typeof(T));
        }

        /// <summary>
        /// 判断当前类型是否属于T的子类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsBaseType<T>(this Type? type)
        {
            if (type == null) return false;
            if (typeof(T).IsInterface)
            {
                return type.GetInterface(typeof(T).Name) != null;
            }
            while (type != null)
            {
                if (type == typeof(T)) return true;
                type = type.BaseType;
            }
            return false;
        }

        public static bool HasAttribute<T>(this Object obj) where T : Attribute
        {
            ICustomAttributeProvider custom = obj is ICustomAttributeProvider ? (ICustomAttributeProvider)obj : (ICustomAttributeProvider)obj.GetType();
            foreach (var t in custom.GetCustomAttributes(false))
            {
                if (t.GetType().Equals(typeof(T))) return true;
            }
            return false;
        }

        public static T? GetAttribute<T>(this object obj) where T : Attribute
        {
            if (obj == null) return default;
            ICustomAttributeProvider custom = obj is ICustomAttributeProvider provider ? provider : obj.GetType();
            foreach (object t in custom.GetCustomAttributes(true))
            {
                if (t.GetType().Equals(typeof(T))) return (T)t;
            }
            return default;
        }

        /// <summary>
        /// 获取备注信息
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string? GetDescription(this Type type)
        {
            foreach (object t in type.GetCustomAttributes(true))
            {
                if (t.GetType().Equals(typeof(DescriptionAttribute)))
                {
                    return ((DescriptionAttribute)t).Description;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取一个资源文件内继承自某个类且具有Description标记的所有类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="types"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetDescription<T>(this Type[] types)
        {
            return types.Where(t => t.IsBaseType<T>() && t.HasAttribute<DescriptionAttribute>())
                .ToDictionary(t => t.Name, t => t.GetAttribute<DescriptionAttribute>()?.Description ?? t.Name);
        }

        /// <summary>
        /// 添加资源文件内的继承自某个类且具有Description标记的所有类型
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="assembly">指定资源，默认为T所在的资源文件</param>
        public static void Add<T>(this Dictionary<string, Dictionary<string, string>> dic, Assembly? assembly = null)
        {
            string? key = typeof(T).FullName;
            if (key == null) return;
            if (assembly == null) assembly = typeof(T).Assembly;
            if (dic.ContainsKey(key))
            {
                Dictionary<string, string> data = dic[key];
                foreach (var item in assembly.GetTypes().GetDescription<T>())
                {
                    if (!data.ContainsKey(item.Key)) data.Add(item.Key, item.Value);
                }
            }
            else
            {
                dic.Add(key, assembly.GetTypes().GetDescription<T>());
            }
        }

    }
}
