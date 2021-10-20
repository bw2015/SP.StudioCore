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
        /// 获取表达式对应sql运算类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetExpressionType(this ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.OrElse:
                case ExpressionType.Or: return "OR";
                case ExpressionType.AndAlso:
                case ExpressionType.And: return "AND";
                case ExpressionType.GreaterThan: return ">";
                case ExpressionType.GreaterThanOrEqual: return ">=";
                case ExpressionType.LessThan: return "<";
                case ExpressionType.LessThanOrEqual: return "<=";
                case ExpressionType.NotEqual: return "<>";
                case ExpressionType.Add: return "+";
                case ExpressionType.Subtract: return "-";
                case ExpressionType.Multiply: return "*";
                case ExpressionType.Divide: return "/";
                case ExpressionType.Modulo: return "%";
                case ExpressionType.Equal: return "=";
            }
            return string.Empty;
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
