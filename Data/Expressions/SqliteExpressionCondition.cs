using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Data.Expressions
{
    internal class SqliteExpressionCondition : ExpressionCondition
    {
        public SqliteExpressionCondition(Expression expression) : base(expression)
        {

        }
    }
}
