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
    internal class MySqlExpressionCondition : ExpressionCondition
    {
        public MySqlExpressionCondition(Expression expression) : base(expression)
        {
        }
        /// <summary>
        /// 递归逐层查找
        /// </summary>
        protected override void AppendParameter(MemberExpression node, Stack<MemberInfo> members)
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
                        Sql.Push($"{fieldName}");
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


    }
}
