using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace SP.StudioCore.Types
{
    /// <summary>
    /// 表达式扩展
    /// </summary>
    public static class ExpressionExtensions
    {
        public static PropertyInfo? ToPropertyInfo<T, TKey>(this Expression<Func<T, TKey>> expression) where T : class
        {
            PropertyInfo? property = null;
            switch (expression.Body.NodeType)
            {
                case ExpressionType.Convert:
                    property = (PropertyInfo)((MemberExpression)((UnaryExpression)expression.Body).Operand).Member;
                    break;
                case ExpressionType.MemberAccess:
                    property = (PropertyInfo)((MemberExpression)expression.Body).Member;
                    break;
            }
            return property;
        }

        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static FieldInfo? ToFieldInfo<T, TKey>(this Expression<Func<T, TKey>> expression) where T : struct
        {
            FieldInfo? field = null;
            switch (expression.Body.NodeType)
            {
                case ExpressionType.Convert:
                    field = (FieldInfo)((MemberExpression)((UnaryExpression)expression.Body).Operand).Member;
                    break;
                case ExpressionType.MemberAccess:
                    field = (FieldInfo)((MemberExpression)expression.Body).Member;
                    break;
            }
            return field;
        }

        /// <summary>
        /// 获取数据库的字段名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string GetFieldName<T, TKey>(this Expression<Func<T, TKey>> expression) where T : class, new()
        {
            PropertyInfo property = expression.ToPropertyInfo();
            return property.GetAttribute<ColumnAttribute>()?.Name ?? property.Name;
        }

        /// <summary>
        /// 获取条件的属性名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="fun"></param>
        /// <returns></returns>
        public static string? GetName<T, TKey>(this Expression<Func<T, TKey>> fun)
        {
            //t => t.Title
            string title = fun.ToString();
            Regex regex = new Regex(@"\.(?<Name>\w+)$", RegexOptions.IgnoreCase);
            if (regex.IsMatch(title)) return regex.Match(title).Groups["Name"].Value;
            return null;
        }
    }
}
