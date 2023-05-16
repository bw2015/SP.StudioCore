using Microsoft.AspNetCore.Http;
using SP.StudioCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using SP.StudioCore.Enums;
using SP.StudioCore.Data;
using System.Runtime.InteropServices;

namespace SP.StudioCore.Linq
{
    /// <summary>
    /// Linq的扩展
    /// </summary>
    public static class LinqExtensions
    {
        /// <summary>
        /// 要查询的字段值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="perdicate"></param>
        public static IQueryable<T> Where<T>(this IQueryable<T> list, HttpContext context, string key, Expression<Func<T, bool>> predicate)
        {
            if (string.IsNullOrEmpty(context.QF(key))) return list;
            return list.Where(predicate);
        }
        /// <summary>
        /// 查询，支持多值数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="list"></param>
        /// <param name="values"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IQueryable<T> Where<T, TValue>(this IQueryable<T> list, TValue[] values, Expression<Func<T, bool>> predicate) where TValue : struct
        {
            if (values == null) return list;
            return list.Where(predicate);
        }

        public static IQueryable<T> Where<T>(this IQueryable<T> list, object value, Expression<Func<T, bool>> predicate)
        {
            if (value == null) return list;
            bool isValue = true;
            switch (value.GetType().Name)
            {
                case "String":
                    isValue = !string.IsNullOrEmpty((string)value);
                    break;
                case "Int32":
                    isValue = (int)value != 0;
                    break;
                case "Int64":
                    isValue = (long)value != 0;
                    break;
                case "Byte":
                    isValue = (byte)value != 0;
                    break;
            }
            if (!isValue) return list;
            return list.Where(predicate);
        }


        public static IEnumerable<T> Where<T>(this IEnumerable<T> list, object value, Func<T, bool> predicate)
        {
            if (value == null) return list;
            bool isValue = true;
            switch (value.GetType().Name)
            {
                case "String":
                    isValue = !string.IsNullOrEmpty((string)value);
                    break;
                case "Int32":
                    isValue = (int)value != 0;
                    break;
                case "Int64":
                    isValue = (long)value != 0;
                    break;
                case "Byte":
                    isValue = (byte)value != 0;
                    break;
            }
            if (!isValue) return list;
            return list.Where(predicate);
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="field">字段</param>
        /// <param name="type">排序类型</param>
        /// <returns></returns>
        public static IQueryable<T> Sort<T>(this IQueryable<T> query, string field, string type)
        {
            if (string.IsNullOrWhiteSpace(field) || string.IsNullOrWhiteSpace(type)) return query;
            string sorting = string.Empty;
            if (type.ToUpper().Trim() == "ASC" || type.ToLower().Trim() == "asc" || type.ToLower().Trim() == "ascending")
            {
                sorting = "OrderBy";
            }
            else if (type.ToUpper().Trim() == "DESC" || type.ToLower().Trim() == "desc" || type.ToLower().Trim() == "descending")
            {
                sorting = "OrderByDescending";
            }
            ParameterExpression param = Expression.Parameter(typeof(T), field);
            PropertyInfo property = typeof(T).GetProperty(field);
            if (property == null) return query;
            Type[] types = new Type[2];
            types[0] = typeof(T);
            types[1] = property.PropertyType;
            Expression exp = Expression.Call(typeof(Queryable), sorting, types, query.Expression, Expression.Lambda(Expression.Property(param, field), param));
            return query.AsQueryable().Provider.CreateQuery<T>(exp);
        }

        /// <summary>
        /// 默认排序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="query"></param>
        /// <param name="expression"></param>
        /// <param name="field">字段</param>
        /// <param name="type">排序类型</param>
        /// <returns></returns>
        public static IOrderedQueryable<T> OrderByDescending<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> expression, string field, string type)
        {
            if (string.IsNullOrWhiteSpace(field) || string.IsNullOrWhiteSpace(type)) return query.OrderByDescending(expression);
            return (IOrderedQueryable<T>)query.Sort(field, type);
        }

        /// <summary>
        /// 把字段转化成为表达式方式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="field"></param>
        /// <returns></returns>
        public static Expression<Func<T, TKey>> ToKeySelector<T, TKey>(this string field)
        {
            PropertyInfo? property = typeof(T).GetProperty(field);
            if (property == null) return null;
            ParameterExpression parameter = Expression.Parameter(typeof(T), "p");
            MemberExpression propertyAccess = Expression.MakeMemberAccess(parameter, property);
            return Expression.Lambda<Func<T, TKey>>(propertyAccess, parameter);
        }

        /// <summary>
        /// 对linq查询语句进行自定义字段内容的排序处理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IOrderedQueryable<T> OrderBy<T, TKey>(this IQueryable<T> query, string field, SortType type)
        {
            Expression<Func<T, TKey>> sort = field.ToKeySelector<T, TKey>();
            if (sort == null)
            {
                throw new Exception($"Column {typeof(T).GetTableName()}.{field} Not Found.");
            }

            return type switch
            {
                SortType.ASC => query.OrderBy(sort),
                SortType.DESC => query.OrderByDescending(sort),
                _ => throw new NotImplementedException()
            };
        }
        /// <summary>
        /// 分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public static IQueryable<T> PageBy<T>(this IQueryable<T> query, int page, int limit)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }
            if (page <= 0) page = 1;
            if (limit <= 0) limit = 20;
            int skip = (page - 1) * limit;
            return query.Skip(skip).Take(limit);
        }
    }
}
