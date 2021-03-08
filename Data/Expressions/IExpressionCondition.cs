using Dapper;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SP.StudioCore.Data.Expressions
{
    /// <summary>
    /// 表达式解析转SQL语句
    /// </summary>
    public interface IExpressionCondition : IDisposable
    {
        string ToCondition(out DynamicParameters parameters);
    }
}
