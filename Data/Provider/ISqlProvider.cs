using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;

namespace SP.StudioCore.Data.Provider
{
    /// <summary>
    /// 数据库操作
    /// </summary>
    public interface ISqlProvider
    {
        /// <summary>
        /// 查询单个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="precate"></param>
        /// <returns></returns>
        SQLResult Info<T>(T obj, params Expression<Func<T, object>>[] precate) where T : class, new();
    }

    /// <summary>
    /// SQL返回对象
    /// </summary>
    public struct SQLResult
    {
        /// <summary>
        /// SQL语句
        /// </summary>
        public string CommandText;

        /// <summary>
        /// 参数对象
        /// </summary>
        public DbParameter[] Prameters;
    }
}
