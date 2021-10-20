using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SP.StudioCore.Data.Expressions
{
    /// <summary>
    /// 设定要更新的字段内容
    /// </summary>
    public class ExpressionUpdate : ExpressionVisitor
    {
        public override Expression? Visit(Expression? node)
        {
            return base.Visit(node);
        }


    }
}
