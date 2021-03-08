using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace SP.StudioCore.Data.Extension
{
    /// <summary>
    /// Dapper的扩展
    /// </summary>
    public static class DapperExtension
    {
        /// <summary>
        /// 转换成为ADO.Net 支持的数据库参数对象
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static DbParameter[] ToDbParameter(this DynamicParameters parameters)
        {
            Stack<DbParameter> list = new Stack<DbParameter>();

            foreach (string name in parameters.ParameterNames)
            {
                list.Push(new SqlParameter(name, parameters.Get<object>(name)));
            }
            return list.ToArray();
        }
    }
}
