using Dapper;
using SP.StudioCore.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SP.StudioCore.Data.Expressions
{
    /// <summary>
    /// 用于解析表达式树到SQL的映射（转用于条件）
    /// </summary>
    internal abstract class ExpressionCondition : ExpressionVisitor, IExpressionCondition
    {
        public ExpressionCondition() { }
        public ExpressionCondition(Expression expression)
        {
            this.Visit(expression);
        }

        /// <summary>
        /// SQL 语句
        /// </summary>
        protected Stack<string> Sql = new Stack<string>();

        /// <summary>
        /// SQL参数
        /// </summary>
        private Dictionary<string, object> parameter = new Dictionary<string, object>();

        /// <summary>
        /// 参数名
        /// </summary>
        private int paramIndex = 0;



        public override Expression Visit(Expression node)
        {
            return base.Visit(node);
        }

        /// <summary>
        /// 获取sql条件内容
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public string ToCondition(out DynamicParameters parameters)
        {
            parameters = new DynamicParameters();
            if (this.Sql.Count == 0)
            {
                return string.Empty;
            }
            string condition = string.Concat(" WHERE ", string.Join(" ", this.Sql));
            foreach (var item in this.parameter)
            {
                parameters.Add(item.Key, item.Value);
            }
            this.Sql.Clear();
            this.parameter.Clear();
            return condition;
        }

        public override string ToString()
        {
            string sql = string.Join(" ", this.Sql);
            this.Sql.Clear();
            this.parameter.Clear();
            return sql;
        }


        /// <summary>
        /// 二级表达式
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            this.Sql.Push(")");
            var right = node.Right;
            this.Visit(right);

            switch (node.NodeType)
            {
                // 等于
                case ExpressionType.Equal:
                    Sql.Push("=");
                    break;
                // 不等于
                case ExpressionType.NotEqual:
                    Sql.Push("!=");
                    break;
                // 大于
                case ExpressionType.GreaterThan:
                    Sql.Push(">");
                    break;
                // 小于
                case ExpressionType.LessThan:
                    Sql.Push("<");
                    break;
                case ExpressionType.AndAlso:
                    Sql.Push("AND");
                    break;
                case ExpressionType.OrElse:
                    Sql.Push("OR");
                    break;
                case ExpressionType.LessThanOrEqual:
                    Sql.Push("<=");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    Sql.Push(">=");
                    break;
            }

            var left = node.Left;
            this.Visit(left);
            this.Sql.Push("(");

            return node;
        }


        /// <summary>
        /// 方法调用
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            string format;
            switch (node.Method.Name)
            {
                case "Contains":
                    format = "({0} LIKE '%{1}%')";
                    break;
                case "StartsWith":
                    format = "({0} LIKE '{1}%')";
                    break;
                case "EndsWith":
                    format = "({0} LIKE '%{1})'";
                    break;
                default:
                    throw new Exception($"VisitMethodCall {node.Method.Name} is not supported");
            }

            this.Visit(node.Object);
            this.Visit(node.Arguments[0]);

            string left = this.Sql.Pop();
            var value = this.parameter.LastOrDefault();

            this.Sql.Push(string.Format(format, left, value.Value));
            return node;
        }

        /// <summary>
        /// 字段名
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitMember(MemberExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.MemberAccess:
                    this.AppendParameter(node, new Stack<MemberInfo>());
                    break;
                case ExpressionType.Constant:
                    {
                        ConstantExpression constant = (ConstantExpression)node.Expression;
                        switch (node.Member.MemberType)
                        {
                            case MemberTypes.Property:
                                this.AppendParameter(((PropertyInfo)node.Member).GetValue(constant.Value));
                                break;
                            case MemberTypes.Field:
                                this.AppendParameter(((FieldInfo)node.Member).GetValue(constant.Value));
                                break;
                        }
                    }
                    break;
                default:
                    break;
            }
            return node;
        }



        /// <summary>
        /// 表达式目录树中的常量
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            object value = node.Value;
            TypeCode code = Type.GetTypeCode(value.GetType());
            switch (code)
            {
                case TypeCode.Boolean:
                    Sql.Push(((bool)value ? 1 : 0).ToString());
                    break;
                case TypeCode.Int32:
                    Sql.Push(((int)value).ToString());
                    break;
                case TypeCode.DBNull:
                    break;
                case TypeCode.Object:
                    //this.AppendParameter(value, node.Type);
                    break;
                default:
                    this.AppendParameter(value);
                    break;
            }
            return node;
        }

        /// <summary>
        /// 递归逐层查找
        /// </summary>
        protected virtual void AppendParameter(MemberExpression node, Stack<MemberInfo> members)
        {
            members.Push(node.Member);
            if (node.Expression == null)
            {
                switch (node.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        switch (node.Type.Equals(Guid.Empty))
                        {
                            default:
                                AppendParameter(Guid.Empty);
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (node.Expression.NodeType)
                {
                    case ExpressionType.Parameter:
                        string fieldName = node.Member.HasAttribute<ColumnAttribute>() ? node.Member.GetAttribute<ColumnAttribute>().Name : node.Member.Name;
                        Sql.Push($"[{fieldName}]");
                        break;
                    case ExpressionType.Constant:
                        this.AppendParameter((ConstantExpression)node.Expression, members);
                        break;
                    case ExpressionType.MemberAccess:
                        this.AppendParameter((MemberExpression)node.Expression, members);
                        break;
                    default:
                        throw new Exception($"not support {node.Expression.NodeType}");
                }
            }
        }

        protected void AppendParameter(ConstantExpression constant, Stack<MemberInfo> members)
        {
            object value = constant.Value;

            foreach (MemberInfo member in members)
            {
                switch (member.MemberType)
                {
                    case MemberTypes.Property:
                        value = ((PropertyInfo)member).GetValue(value) ?? string.Empty;
                        break;
                    case MemberTypes.Field:
                        value = ((FieldInfo)member).GetValue(value) ?? string.Empty;
                        break;
                }
            }
            if (value != null)
            {
                this.AppendParameter(Expression.Constant(value));
            }

        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            return base.VisitConditional(node);
        }

        protected override MemberBinding VisitMemberBinding(MemberBinding node)
        {
            return base.VisitMemberBinding(node);
        }

        protected void AppendParameter(object value)
        {
            if (value is ConstantExpression) value = ((ConstantExpression)value).Value;
            Sql.Push($"@_p{paramIndex}");
            parameter.Add($"@_p{paramIndex}", value);
            paramIndex++;
        }

        protected void AppendParameter(object value, Type type, string member)
        {
            FieldInfo? field = type.GetField(member);
            if (field == null) return;
            this.AppendParameter(field.GetValue(value));

        }

        public void Dispose()
        {
            this.Sql.Clear();
            this.parameter.Clear();
        }
    }
}
