using SP.StudioCore.Types;
using SP.StudioCore.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SP.StudioCore.Data.Schema
{
    /// <summary>
    /// 数据库结构缓存
    /// </summary>
    public static class SchemaCache
    {
        /// <summary>
        /// 数据字段缓存
        /// </summary>
        private static readonly Dictionary<Type, List<ColumnProperty>> columnCache = new Dictionary<Type, List<ColumnProperty>>();

        /// <summary>
        /// 获取实体类下所有的数据库字段
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal static List<ColumnProperty> GetColumns(Type type)
        {
            if (columnCache.ContainsKey(type)) return columnCache[type];
            lock (LockHelper.GetLoker(type.Name))
            {
                List<ColumnProperty> columns = new List<ColumnProperty>();
                foreach (PropertyInfo property in type.GetProperties().Where(t => t.HasAttribute<ColumnAttribute>()))
                {
                    columns.Add(new ColumnProperty(property));
                }
                if (!columnCache.ContainsKey(type)) columnCache.Add(type, columns);
            }
            return columnCache[type];
        }

        /// <summary>
        /// 获取实体类下所有的数据库字段（泛型方式读取）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal static IEnumerable<ColumnProperty> GetColumns<T>(params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            List<ColumnProperty> list = GetColumns(typeof(T));
            if (fields.Length == 0) return list;
            return list.Where(t => fields.Any(p => p.ToPropertyInfo().Name == t.Property.Name));
        }

        /// <summary>
        /// 按条件获取字段
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        internal static IEnumerable<ColumnProperty> GetColumns<T>(Func<ColumnProperty, bool> predicate) where T : class, new()
        {
            return GetColumns<T>().Where(predicate);
        }

        /// <summary>
        /// 获取单个字段的信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="field"></param>
        /// <returns></returns>
        internal static ColumnProperty GetColumnProperty<T, TValue>(Expression<Func<T, TValue>> field) where T : class, new()
        {
            IEnumerable<ColumnProperty> list = GetColumns<T>();
            PropertyInfo property = field.ToPropertyInfo();
            return list.FirstOrDefault(t => t.Property.Name == property.Name);
        }

        /// <summary>
        /// 获取主键（类型）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ColumnProperty[] GetKey(this Type type)
        {
            IEnumerable<ColumnProperty> columns = GetColumns(type).Where(t => t.IsKey);
            return columns.ToArray();
        }

        /// <summary>
        /// 获取主键（泛型）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal static ColumnProperty[] GetKey<T>() where T : class, new()
        {
            return GetKey(typeof(T));
        }

        /// <summary>
        /// 获取查询条件（实体类）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="fields">不指定则使用主键</param>
        /// <returns></returns>
        internal static Dictionary<ColumnProperty, object> GetCondition<T>(this T obj, params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            IEnumerable<ColumnProperty> columns = fields.Length == 0 ?
                GetKey<T>() :
                GetColumns<T>(t => fields.Select(p => p.ToPropertyInfo()).Any(p => p.Name == t.Property.Name));

            return columns.ToDictionary(t => t, t => t.Property.GetValue(obj));
        }
    }
}
