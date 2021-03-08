using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text;

namespace SP.StudioCore.Data
{
    internal static class DbFactory
    {
        /// <summary>
        /// 创建数据库操作对象
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="dbType"></param>
        /// <param name="tranLevel"></param>
        /// <returns></returns>
        internal static DbExecutor CreateExecutor(string connectionString, DatabaseType dbType = DatabaseType.SqlServer, IsolationLevel tranLevel = IsolationLevel.Unspecified)
        {
            return new DbExecutor(connectionString, dbType, tranLevel);
        }

        /// <summary>
        /// 数据库实体类反射缓存类库
        /// </summary>
        internal static Dictionary<Type, Dictionary<string, PropertyInfo>> _dataPropertyModel = new Dictionary<Type, Dictionary<string, PropertyInfo>>();


        internal static DbParameter NewParam(this DatabaseType type, string parameterName, object value)
        {
            return type switch
            {
                DatabaseType.SqlServer => new SqlParameter(parameterName, value),
                _ => throw new NotSupportedException($"DbParameter don't support {type}")
            };
        }
    }


}
