using Dapper;
using Microsoft.Data.SqlClient;
using SP.StudioCore.Data.Schema;
using System;
using System.Collections.Generic;
using System.Data;
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
        /// <summary>
        ///  IDataReader赋值实体
        /// </summary>
        /// <returns></returns>
        public static TResult GetReaderData<TResult>(this IDataReader reader)
        {
            return (TResult)reader.GetReaderData(Activator.CreateInstance(typeof(TResult)));
        }
        public static object GetReaderData(this IDataReader reader, object source)
        {
            //映射数据库中的字段到实体属性
            IEnumerable<ColumnProperty> propertys = SchemaCache.GetColumns(source.GetType());
            foreach (ColumnProperty property in propertys)
            {
                //对实体属性进行设值
                object value = reader[property.Name];
                if (value == null) continue;
                property.Property.SetValue(source, value);
            }
            return source;
        }
    }
}
