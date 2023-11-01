using Dapper;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using SP.StudioCore.Data.Expressions;
using SP.StudioCore.Data.Extension;
using SP.StudioCore.Data.Repository;
using SP.StudioCore.Data.Schema;
using SP.StudioCore.Model;
using SP.StudioCore.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SP.StudioCore.Data.Provider
{
    /// <summary>
    /// SQLServer的实现方法
    /// </summary>
    public sealed class SqlServerProvider : ISqlProvider, IWriteRepository, IProcedureRepository
    {
        private readonly DbExecutor db;

        public SqlServerProvider(DbExecutor db)
        {
            this.db = db;
        }

        #region ========  针对实体类的操作  ========

        /// <summary>
        /// 获取一行值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="precate"></param>
        /// <returns></returns>
        public SQLResult Info<T>(T obj, params Expression<Func<T, object>>[] precate) where T : class, new()
        {
            Dictionary<ColumnProperty, object> condition = obj.GetCondition(precate);
            string tableName = obj.GetTableName();
            IEnumerable<ColumnProperty> fields = SchemaCache.GetColumns<T>();
            string sql = $"SELECT TOP 1 {string.Join(",", fields.Select(t => string.Format("[{0}]", t.Name)))} FROM [{tableName}] WHERE {string.Join(" AND ", condition.Select(t => $"[{t.Key.Name}] = @{t.Key.Property.Name}"))}";
            DbParameter[] parameters = condition.Select(t => new SqlParameter($"@{t.Key.Property.Name}", t.Value)).ToArray();
            return new SQLResult()
            {
                CommandText = sql,
                Prameters = parameters
            };
        }

        #endregion

        /// <summary>
        /// 数据是否存在
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="condition"></param>
        /// <returns></returns>
        public bool Exists<T>(Expression<Func<T, bool>> condition) where T : class, new()
        {
            using (IExpressionCondition expression = db.GetExpressionCondition(condition))
            {
                string sql = $"SELECT 0 WHERE EXISTS(SELECT 0 FROM [{typeof(T).GetTableName()}] {expression.ToCondition(out DynamicParameters parameters)} )";
                object value = db.ExecuteScalar(CommandType.Text, sql, parameters);
                return value != null;
            }
        }

        /// <summary>
        /// 表内是否存在数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool Exists<T>() where T : class, new()
        {
            string sql = $"SELECT 0 WHERE EXISTS(SELECT 0 FROM [{typeof(T).GetTableName()}])";
            object value = db.ExecuteScalar(CommandType.Text, sql);
            return value != null;
        }


        /// <summary>
        /// 通过主键判断数据是否存在
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool Exists<T>(T entity) where T : class, new()
        {
            IEnumerable<ColumnProperty> fields = SchemaCache.GetColumns<T>(t => t.IsKey);
            if (!fields.Any()) throw new Exception($"{typeof(T).GetTableName()} No primary key");
            string sql = $"SELECT 0 WHERE EXISTS(SELECT 0 FROM [{typeof(T).GetTableName()}] WHERE {string.Join(" AND ", fields.Select(t => $"[{t.Name}]=@{t.Name}"))})";
            DynamicParameters parameters = new DynamicParameters();
            foreach (ColumnProperty column in fields)
            {
                parameters.Add($"@{column.Name}", column.Property.GetValue(entity));
            }
            object value = db.ExecuteScalar(CommandType.Text, sql, parameters);
            return value != null;
        }

        public bool Exists<T>(string condition) where T : class, new()
        {
            string sql = $"SELECT 0 WHERE EXISTS(SELECT 0 FROM [{typeof(T).GetTableName()}] WHERE {condition})";
            object value = db.ExecuteScalar(CommandType.Text, sql);
            return value != null;
        }

        public int Count<T>(Expression<Func<T, bool>> condition) where T : class, new()
        {
            using (IExpressionCondition expression = db.GetExpressionCondition(condition))
            {
                string sql = $"SELECT COUNT(0) FROM [{typeof(T).GetTableName()}] {expression.ToCondition(out DynamicParameters parameters)} ";
                object value = db.ExecuteScalar(CommandType.Text, sql, parameters);
                return value == null ? 0 : (int)value;
            }
        }

        public int Count<T>() where T : class, new()
        {
            string sql = $"SELECT COUNT(0) FROM [{typeof(T).GetTableName()}] ";
            object value = db.ExecuteScalar(CommandType.Text, sql);
            return value == null ? 0 : (int)value;
        }

        /// <summary>
        /// 获取DataSet
        /// </summary>
        public DataSet GetDataSet<T>(Expression<Func<T, bool>> condition, params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            string field = string.Join(",", SchemaCache.GetColumns(fields).Select(t => $"[{t.Name}]"));
            using (IExpressionCondition expression = db.GetExpressionCondition(condition))
            {
                string conditionSql = expression.ToCondition(out DynamicParameters parameters);
                string sql = $"SELECT {field} FROM [{typeof(T).GetTableName()}] {conditionSql}";
                return db.GetDataSet(CommandType.Text, sql, parameters.ToDbParameter());
            }
        }

        public IDataReader ReadData<T>(Expression<Func<T, bool>> condition, params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            string field = string.Join(",", SchemaCache.GetColumns(fields).Select(t => $"[{t.Name}]"));
            using (IExpressionCondition expression = db.GetExpressionCondition(condition))
            {
                string conditionSql = expression.ToCondition(out DynamicParameters parameters);
                string sql = $"SELECT {field} FROM [{typeof(T).GetTableName()}] {conditionSql}";
                return db.ReadData(CommandType.Text, sql, parameters);
            }
        }
        /// <summary>
        /// 获取指定条数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="condition"></param>
        /// <param name="count"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public IDataReader ReadData<T>(Expression<Func<T, bool>> condition, int count, params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            string field = string.Join(",", SchemaCache.GetColumns(fields).Select(t => $"[{t.Name}]"));
            using (IExpressionCondition expression = db.GetExpressionCondition(condition))
            {
                string conditionSql = expression.ToCondition(out DynamicParameters parameters);
                string sql = $"SELECT TOP {count} {field} FROM [{typeof(T).GetTableName()}] {conditionSql}";
                return db.ReadData(CommandType.Text, sql, parameters);
            }
        }

        public IDataReader ReadData<T>(params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            string field = string.Join(",", SchemaCache.GetColumns(fields).Select(t => $"[{t.Name}]"));
            string sql = $"SELECT {field} FROM [{typeof(T).GetTableName()}]";
            return db.ReadData(CommandType.Text, sql);
        }

        /// <summary>
        /// 读取一个字段
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="field"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public IEnumerable<TValue> ReadList<T, TValue>(Expression<Func<T, TValue>> field, Expression<Func<T, bool>> condition) where T : class, new()
        {
            string? fieldName = SchemaCache.GetColumnProperty(field).Name;
            List<TValue> list = new List<TValue>();
            if (string.IsNullOrEmpty(fieldName)) return list;
            using (IExpressionCondition expression = db.GetExpressionCondition(condition))
            {
                string conditionSql = expression.ToCondition(out DynamicParameters parameters);
                string sql = $"SELECT [{fieldName}] FROM [{typeof(T).GetTableName()}] {conditionSql}";
                IDataReader reader = db.ReadData(CommandType.Text, sql, parameters);
                while (reader.Read())
                {
                    list.Add((TValue)reader[0]);
                }
                if (!reader.IsClosed) reader.Close();
            }
            return list;
        }

        /// <summary>
        /// 查询一个值
        /// </summary>
        public TValue ReadInfo<T, TValue>(Expression<Func<T, TValue>> field, Expression<Func<T, bool>> condition) where T : class, new()
        {
            using (IExpressionCondition expression = db.GetExpressionCondition(condition))
            {
                string conditionSql = expression.ToCondition(out DynamicParameters parameters);
                string sql = $"SELECT TOP 1 {SchemaCache.GetColumnProperty(field).Name} FROM [{typeof(T).GetTableName()}] {conditionSql}";
                object value = db.ExecuteScalar(CommandType.Text, sql, parameters);
                if (value == null) return default;
                return (TValue)value;
            }
        }

        /// <summary>
        /// 读取单个对象（使用IDataReader构造）
        /// </summary>
        public T ReadInfo<T>(Expression<Func<T, bool>> condition, params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            using (IDataReader reader = this.ReadData(condition, 1, fields))
            {
                try
                {
                    while (reader.Read())
                    {
                        return (T)Activator.CreateInstance(typeof(T), reader);
                    }
                }
                finally
                {
                    if (!reader.IsClosed) reader.Close();
                }
            }
            return default;
        }

        /// <summary>
        /// 执行sql条件查询（使用 IDataReader 构造）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="condition"></param>
        /// <param name="parameters"></param>
        /// <param name="fields">置顶要查询的字段</param>
        /// <returns></returns>
        public IEnumerable<T> ReadList<T>(string condition, object? parameters = null, params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            List<T> list = new List<T>();
            string columns =
                fields.Length == 0 ?
                string.Join(",", SchemaCache.GetColumns<T>().Select(t => $"[{t.Name}]")) :
                string.Join(",", SchemaCache.GetColumns(fields).Select(t => $"[{t.Name}]"));
            using (IDataReader reader = db.ReadData(CommandType.Text, $"SELECT {columns} FROM [{typeof(T).GetTableName()}] WHERE {condition}", parameters))
            {
                try
                {
                    while (reader.Read())
                    {
                        T? item = (T?)Activator.CreateInstance(typeof(T), reader);
                        if (item != null)
                        {
                            list.Add(item);
                        }
                    }
                }
                finally
                {
                    if (!reader.IsClosed) reader.Close();
                }
            }
            return list;
        }

        public IEnumerable<T> ReadList<T>(int top, string condition, string sort, object? parameters = null, params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            List<T> list = new List<T>();
            string columns =
                fields.Length == 0 ?
                string.Join(",", SchemaCache.GetColumns<T>().Select(t => $"[{t.Name}]")) :
                string.Join(",", SchemaCache.GetColumns(fields).Select(t => $"[{t.Name}]"));
            using (IDataReader reader = db.ReadData(CommandType.Text, $"SELECT TOP {top} {columns} FROM [{typeof(T).GetTableName()}] WHERE {condition} ORDER BY {sort}",
                parameters))
            {
                try
                {
                    while (reader.Read())
                    {
                        T? item = (T?)Activator.CreateInstance(typeof(T), reader);
                        if (item != null)
                        {
                            list.Add(item);
                        }
                    }
                }
                finally
                {
                    if (!reader.IsClosed) reader.Close();
                }
            }
            return list;
        }

        /// <summary>
        /// 读取单个对象（使用IDataReader构造）
        /// </summary>
        public IEnumerable<T> ReadList<T>(Expression<Func<T, bool>> condition, params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            List<T> list = new();
            using (IDataReader reader = this.ReadData(condition, fields))
            {
                try
                {
                    while (reader.Read())
                    {
                        T? t = (T?)Activator.CreateInstance(typeof(T), reader);
                        if (t != null)
                        {
                            list.Add(t);
                        }
                    }
                }
                finally
                {
                    if (!reader.IsClosed) reader.Close();
                }
            }
            return list;
        }

        /// <summary>
        /// 返回表的所有数据（使用 IDataReader 构造）
        /// </summary>
        public IEnumerable<T> ReadList<T>(params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            List<T> list = new List<T>();
            using (IDataReader reader = this.ReadData(fields))
            {
                try
                {
                    while (reader.Read())
                    {
                        list.Add((T)Activator.CreateInstance(typeof(T), reader));
                    }
                }
                finally
                {
                    if (!reader.IsClosed) reader.Close();
                }
            }
            return list;
        }

        public int Delete<T>(Expression<Func<T, bool>> condition) where T : class, new()
        {
            using (IExpressionCondition expression = db.GetExpressionCondition(condition))
            {
                string sql = $"DELETE FROM [{typeof(T).GetTableName()}] {expression.ToCondition(out DynamicParameters parameters)}";
                return db.ExecuteNonQuery(CommandType.Text, sql, parameters);
            }
        }

        public bool Delete<T>(T entity) where T : class, new()
        {
            DynamicParameters parameters = new DynamicParameters();
            Stack<string> conditionFields = new Stack<string>();
            foreach (ColumnProperty column in SchemaCache.GetColumns<T>().Where(t => t.IsKey))
            {
                conditionFields.Push($"[{column.Name}] = @{column.Name}");
                parameters.Add($"@{column.Name}", column.Property.GetValue(entity));
            }
            string sql = $"DELETE FROM [{typeof(T).GetTableName()}] WHERE {string.Join(" AND ", conditionFields)}";
            return db.ExecuteNonQuery(CommandType.Text, sql, parameters) > 0;
        }

        /// <summary>
        /// 只更新一个字段
        /// </summary>
        public int Update<T, TValue>(Expression<Func<T, TValue>> field, TValue value, Expression<Func<T, bool>> condition) where T : class, new()
        {
            using (IExpressionCondition expression = db.GetExpressionCondition(condition))
            {
                string conditionSql = expression.ToCondition(out DynamicParameters parameters);
                ColumnProperty column = SchemaCache.GetColumnProperty(field);
                parameters.Add("@Value", value.GetSafeValue(typeof(TValue)));
                string sql = $"UPDATE [{typeof(T).GetTableName()}] SET [{column.Name}] = @Value {conditionSql}";
                return db.ExecuteNonQuery(CommandType.Text, sql, parameters);
            }
        }

        public int Update<T, TField1, TField2>(Expression<Func<T, TField1>> field1, TField1 value1, Expression<Func<T, TField2>> field2, TField2 value2, Expression<Func<T, bool>> condition) where T : class, new()
        {
            using (IExpressionCondition expression = db.GetExpressionCondition(condition))
            {
                string conditionSql = expression.ToCondition(out DynamicParameters parameters);
                ColumnProperty column1 = SchemaCache.GetColumnProperty(field1);
                ColumnProperty column2 = SchemaCache.GetColumnProperty(field2);
                parameters.Add("@Value1", value1.GetSafeValue(typeof(TField1)));
                parameters.Add("@Value2", value2.GetSafeValue(typeof(TField2)));
                string sql = $"UPDATE {typeof(T).GetTableName()} SET [{column1.Name}] = @Value1,[{column2.Name}] = @Value2 {conditionSql};";
                return db.ExecuteNonQuery(CommandType.Text, sql, parameters);
            }
        }

        public int Update<T, TField1, TField2, TField3>(Expression<Func<T, TField1>> field1, TField1 value1, Expression<Func<T, TField2>> field2, TField2 value2, Expression<Func<T, TField3>> field3, TField3 value3, Expression<Func<T, bool>> condition) where T : class, new()
        {
            using (IExpressionCondition expression = db.GetExpressionCondition(condition))
            {
                string conditionSql = expression.ToCondition(out DynamicParameters parameters);
                ColumnProperty column1 = SchemaCache.GetColumnProperty(field1);
                ColumnProperty column2 = SchemaCache.GetColumnProperty(field2);
                ColumnProperty column3 = SchemaCache.GetColumnProperty(field3);
                parameters.Add("@Value1", value1.GetSafeValue(typeof(TField1)));
                parameters.Add("@Value2", value2.GetSafeValue(typeof(TField2)));
                parameters.Add("@Value3", value3.GetSafeValue(typeof(TField3)));
                string sql = $"UPDATE {typeof(T).GetTableName()} SET [{column1.Name}] = @Value1,[{column2.Name}] = @Value2,[{column3.Name}] = @Value3 {conditionSql};";
                return db.ExecuteNonQuery(CommandType.Text, sql, parameters);
            }
        }

        /// <summary>
        /// 多字段更新
        /// </summary>
        /// <param name="fields">如果没有指定则更新除主键之外的字段</param>
        public int Update<T>(T entity, Expression<Func<T, bool>> condition, params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            using (IExpressionCondition expression = db.GetExpressionCondition(condition))
            {
                string conditionSql = expression.ToCondition(out DynamicParameters parameters);
                Stack<string> updateFields = new Stack<string>();
                if (fields.Length == 0)
                {
                    foreach (ColumnProperty column in SchemaCache.GetColumns<T>(t => !t.IsKey && !t.Identity))
                    {
                        parameters.Add($"@{column.Name}", column.Property.GetValue(entity).GetSafeValue(column.Property.PropertyType));
                        updateFields.Push(column.Name);
                    }
                }
                else
                {
                    foreach (var field in SchemaCache.GetCondition(entity, fields))
                    {
                        parameters.Add($"@{field.Key.Name}", field.Value.GetSafeValue(field.Key.Property.PropertyType));
                        updateFields.Push(field.Key.Name);
                    }
                }
                string sql = $"UPDATE [{typeof(T).GetTableName()}] SET {string.Join(",", updateFields.Select(t => $"[{t}] = @{t}"))} {conditionSql}";
                return db.ExecuteNonQuery(CommandType.Text, sql, parameters);
            }
        }

        /// <summary>
        /// 用主键作为条件更新数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public int Update<T>(T entity, params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            IEnumerable<ColumnProperty> columns = SchemaCache.GetColumns<T>();
            DynamicParameters parameters = new DynamicParameters();
            Stack<string> updateFields = new Stack<string>();
            Stack<string> conditionFields = new Stack<string>();

            if (fields.Length == 0)
            {
                foreach (ColumnProperty column in SchemaCache.GetColumns<T>(t => !t.IsKey && !t.Identity))
                {
                    parameters.Add($"@{column.Name}", column.Property.GetValue(entity).GetSafeValue(column.Property.PropertyType));
                    updateFields.Push(column.Name);
                }
            }
            else
            {
                foreach (KeyValuePair<ColumnProperty, object> field in SchemaCache.GetCondition(entity, fields))
                {
                    parameters.Add($"@{field.Key.Name}", field.Value.GetSafeValue(field.Key.Property.PropertyType));
                    updateFields.Push(field.Key.Name);
                }
            }

            foreach (ColumnProperty column in SchemaCache.GetColumns<T>(t => t.IsKey))
            {
                conditionFields.Push(column.Name);
                parameters.Add($"@{column.Name}", column.Property.GetValue(entity).GetSafeValue(column.Property.PropertyType));
            }
            string sql = $"UPDATE [{typeof(T).GetTableName()}] SET {string.Join(",", updateFields.Select(t => $"[{t}] = @{t}"))} WHERE {string.Join(" AND ", conditionFields.Select(t => $"[{t}] = @{t}"))}";
            return db.ExecuteNonQuery(CommandType.Text, sql, parameters);
        }

        /// <summary>
        /// 更新增长型字段（自动跳过为0的字段）
        /// </summary>
        public int UpdatePlus<T>(T entity, Expression<Func<T, bool>> condition, params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            using (IExpressionCondition expression = db.GetExpressionCondition(condition))
            {
                string whereSql = expression.ToCondition(out DynamicParameters parameters);
                Stack<string> updateFields = new Stack<string>();
                foreach (PropertyInfo property in entity.GetType().GetProperties().Where(t => t.HasAttribute<UpdatePlusAttribute>()))
                {
                    object value = property.GetValue(entity);
                    switch (property.PropertyType.Name)
                    {
                        case "Int32":
                            if ((int)value == default) continue;
                            break;
                        case "Decimal":
                            if ((decimal)value == default) continue;
                            break;
                        default:
                            continue;
                    }
                    string fieldName = property.Name;
                    parameters.Add($"@{fieldName}", value);
                    updateFields.Push(fieldName);
                }
                string sql = $"UPDATE [{typeof(T).GetTableName()}] SET {string.Join(",", updateFields.Select(t => $"[{t}] = [{t}] + @{t}"))} {whereSql}";
                return db.ExecuteNonQuery(CommandType.Text, sql, parameters);
            }
        }

        /// <summary>
        /// 自增的单个字段
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="updateCondition"></param>
        /// <returns></returns>
        public TValue? UpdatePlus<T, TValue>(Expression<Func<T, TValue>> field, TValue value, Expression<Func<T, bool>> updateCondition, Expression<Func<T, bool>>? condition = null)
            where T : class, new()
            where TValue : struct
        {
            using (IExpressionCondition expression = db.GetExpressionCondition(updateCondition))
            {
                string whereSql = expression.ToCondition(out DynamicParameters parameters);
                string? fieldName = SchemaCache.GetColumnProperty(field).Name;
                string? tableName = typeof(T).GetTableName();

                parameters.Add("@Value", value);
                string sql = $"UPDATE [{tableName}] SET [{fieldName}] = [{fieldName}] + @Value {whereSql};SELECT [{fieldName}] FROM [{tableName}] {whereSql}";
                object result = db.ExecuteScalar(CommandType.Text, sql, parameters);
                return (TValue?)result;
            }
        }

        public int UpdatePlus<T, TValue1, TValue2>(Expression<Func<T, TValue1>> field1, TValue1 value1, Expression<Func<T, TValue2>> field2, TValue2 value2, Expression<Func<T, bool>> updateCondition, Expression<Func<T, bool>>? condition = null)
          where T : class, new()
          where TValue1 : struct
          where TValue2 : struct
        {
            using (IExpressionCondition expression = db.GetExpressionCondition(updateCondition))
            {
                string whereSql = expression.ToCondition(out DynamicParameters parameters);
                string? fieldName1 = SchemaCache.GetColumnProperty(field1).Name;
                string? fieldName2 = SchemaCache.GetColumnProperty(field2).Name;
                string? tableName = typeof(T).GetTableName();

                parameters.Add("@Value1", value1);
                parameters.Add("@Value2", value2);
                string sql = $"UPDATE [{tableName}] SET [{fieldName1}] = [{fieldName1}] + @Value1,[{fieldName2}] = [{fieldName2}] + @Value2 {whereSql}";
                return db.ExecuteNonQuery(CommandType.Text, sql, parameters);
            }
        }

        public int UpdateIn<T, TValue, TKey>(Expression<Func<T, TValue>> field, TValue value, Expression<Func<T, TKey>> condition, TKey[] keys)
          where T : class, new()
          where TValue : struct
        {
            if (keys == null || keys.Length == 0) return 0;
            string? tableName = typeof(T).GetTableName();
            string? fieldName = SchemaCache.GetColumnProperty(field).Name;
            string? whereName = SchemaCache.GetColumnProperty(condition).Name;
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@Value", value);
            string sql = $"UPDATE [{tableName}] SET [{fieldName}] = @Value WHERE [{whereName}] IN ({string.Join(",", keys)})";
            return db.ExecuteNonQuery(CommandType.Text, sql, parameters);
        }

        public int ExecuteNonQuery<T>(T obj) where T : IProcedureModel
        {
            int rows = db.ExecuteNonQuery(CommandType.StoredProcedure, typeof(T).GetTableName(),
                obj.ToParameters());
            obj.Fill();
            return rows;
        }

        public DataSet GetDataSet<T>(T obj) where T : IProcedureModel
        {
            DataSet ds = db.GetDataSet(CommandType.StoredProcedure, typeof(T).GetTableName(),
                 obj.ToDbParameter());
            obj.Fill();
            return ds;
        }

        public IDataReader ReadData<T>(T obj) where T : IProcedureModel
        {
            return db.ReadData(CommandType.StoredProcedure, typeof(T).GetTableName(), obj.ToParameters());
        }

        public IEnumerable<TResult> ReadList<TResult, T>(T obj) where T : IProcedureModel where TResult : class, new()
        {
            List<TResult> list = new List<TResult>();
            using (IDataReader reader = db.ReadData(CommandType.StoredProcedure, typeof(T).GetTableName(), obj.ToParameters()))
            {
                while (reader.Read())
                {
                    list.Add((TResult)Activator.CreateInstance(typeof(TResult), reader));
                }
                if (!reader.IsClosed) reader.Close();
            }
            return list;
        }

        public IEnumerable<TResult> ReadScalar<TResult, T>(T obj) where T : IProcedureModel
        {
            List<TResult> list = new List<TResult>();
            using (IDataReader reader = db.ReadData(CommandType.StoredProcedure, typeof(T).GetTableName(), obj.ToParameters()))
            {
                while (reader.Read())
                {
                    list.Add((TResult)reader[0]);
                }
                if (!reader.IsClosed) reader.Close();
            }
            return list;
        }

        public TValue ExecuteScalar<T, TValue>(T obj) where T : IProcedureModel
        {
            object result = db.ExecuteScalar(CommandType.StoredProcedure, typeof(T).GetTableName(), obj.ToParameters());
            if (result == null) return default;
            return result.GetValue<TValue>();
        }

        public bool Insert<T>(T entity) where T : class, new()
        {
            IEnumerable<ColumnProperty> fields = SchemaCache.GetColumns<T>().Where(t => !t.Identity);
            string sql = $"INSERT INTO [{typeof(T).GetTableName()}]({string.Join(",", fields.Select(t => $"[{t.Name}]"))}) VALUES({string.Join(",", fields.Select(t => $"@{t.Name}"))});";
            DynamicParameters parameters = new DynamicParameters();
            foreach (ColumnProperty field in fields)
            {
                parameters.Add($"@{field.Name}", field.Property.GetValue(entity).GetSafeValue(field.Property.PropertyType));
            }
            return this.db.ExecuteNonQuery(CommandType.Text, sql, parameters) == 1;
        }

        public bool InsertIdentity<T>(T entity) where T : class, new()
        {
            ColumnProperty identity = SchemaCache.GetColumns<T>().FirstOrDefault(t => t.Identity);
            if (!identity) throw new InvalidOperationException();
            IEnumerable<ColumnProperty> fields = SchemaCache.GetColumns<T>().Where(t => !t.Identity);
            string sql = $"INSERT INTO [{typeof(T).GetTableName()}]({string.Join(",", fields.Select(t => $"[{t.Name}]"))}) VALUES({string.Join(",", fields.Select(t => $"@{t.Name}"))});SELECT SCOPE_IDENTITY();";
            DynamicParameters parameters = new DynamicParameters();
            foreach (ColumnProperty field in fields)
            {
                parameters.Add($"@{field.Name}", field.Property.GetValue(entity).GetSafeValue(field.Property.PropertyType));
            }
            object value = this.db.ExecuteScalar(CommandType.Text, sql, parameters);
            if (value == null || value == DBNull.Value) return false;
            identity.Property.SetValue(entity, Convert.ChangeType(value, identity.Property.PropertyType));
            return true;
        }

        public bool InsertNoIdentity<T>(T entity) where T : class, new()
        {
            IEnumerable<ColumnProperty> fields = SchemaCache.GetColumns<T>();
            StringBuilder sb = new StringBuilder()
                .Append($"SET IDENTITY_INSERT [{typeof(T).GetTableName()}] ON;")
                .Append($"INSERT INTO [{typeof(T).GetTableName()}]({string.Join(",", fields.Select(t => $"[{t.Name}]"))}) VALUES({string.Join(",", fields.Select(t => $"@{t.Name}"))});")
                .Append($"SET IDENTITY_INSERT [{typeof(T).GetTableName()}] OFF;");
            DynamicParameters parameters = new DynamicParameters();
            foreach (ColumnProperty field in fields)
            {
                parameters.Add($"@{field.Name}", field.Property.GetValue(entity).GetSafeValue(field.Property.PropertyType));
            }
            return this.db.ExecuteNonQuery(CommandType.Text, sb.ToString(), parameters) == 1;
        }

        public IQueryable<TEntity> Query<TEntity>() where TEntity : class, new()
        {
            return new DapperQueryable<TEntity>(new SqlServerQueryProvider(db));
        }


    }
}
