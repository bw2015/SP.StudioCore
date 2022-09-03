using Dapper;
using SP.StudioCore.Data.Expressions;
using SP.StudioCore.Data.Extension;
using SP.StudioCore.Data.Repository;
using SP.StudioCore.Data.Schema;
using SP.StudioCore.Model;
using SP.StudioCore.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SP.StudioCore.Data.Provider
{
    /// <summary>
    /// mysql实现
    /// </summary>
    public sealed class MySqlProvider : IWriteRepository, ISqlProvider, IProcedureRepository
    {
        private readonly DbExecutor db;

        public MySqlProvider(DbExecutor db)
        {
            this.db = db;
        }
        public SQLResult Info<T>(T obj, params Expression<Func<T, object>>[] precate) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public int Count<T>(Expression<Func<T, bool>> condition) where T : class, new()
        {
            using (IExpressionCondition exp = db.GetExpressionCondition(condition))
            {
                string where = exp.ToCondition(out DynamicParameters parameters);
                string sql = $"SELECT COUNT(0) FROM {typeof(T).GetTableName()} {where};";
                object value = db.ExecuteScalar(sql, parameters);
                return value == null ? 0 : (int)value;
            }
        }

        public int Count<T>() where T : class, new()
        {
            string sql = $"SELECT COUNT(0) FROM {typeof(T).GetTableName()};";
            object value = db.ExecuteScalar(sql);
            return value == null ? 0 : (int)value;
        }

        public int Delete<T>(Expression<Func<T, bool>> condition) where T : class, new()
        {
            using (IExpressionCondition exp = db.GetExpressionCondition(condition))
            {
                string where = exp.ToCondition(out DynamicParameters parameters);
                string sql = $"DELETE FROM {typeof(T).GetTableName()} {where};";
                return db.ExecuteNonQuery(CommandType.Text, sql, parameters);
            }
        }

        public bool Delete<T>(T entity) where T : class, new()
        {
            DynamicParameters parameters = new DynamicParameters();
            Stack<string> where = new Stack<string>();
            foreach (ColumnProperty column in SchemaCache.GetColumns<T>().Where(t => t.IsKey))
            {
                where.Push($"[{column.Name}]=@{column.Name}");
                parameters.Add(column.Name, column.Property.GetValue(entity));
            }
            string sql = $"DELETE FROM {typeof(T).GetTableName()} WHERE {string.Join(" AND ", where)};";
            return db.ExecuteNonQuery(CommandType.Text, sql, parameters) > 0;
        }

        public int ExecuteNonQuery<T>(T obj) where T : IProcedureModel
        {
            throw new NotImplementedException();
        }

        public TValue ExecuteScalar<T, TValue>(T obj) where T : IProcedureModel
        {
            throw new NotImplementedException();
        }

        public bool Exists<T>(Expression<Func<T, bool>> condition) where T : class, new()
        {
            using (IExpressionCondition expression = db.GetExpressionCondition(condition))
            {
                string sql = $"SELECT 0 WHERE EXISTS(SELECT 0 FROM {typeof(T).GetTableName()} { expression.ToCondition(out DynamicParameters parameters)} );";
                return db.ExecuteScalar(CommandType.Text, sql, parameters) != null;
            }
        }

        public bool Exists<T>() where T : class, new()
        {
            string sql = $"SELECT 0 FROM `{typeof(T).GetTableName()}` LIMIT 1;";
            return db.ExecuteScalar(CommandType.Text, sql) != null;
        }

        public bool Exists<T>(T entity) where T : class, new()
        {
            IEnumerable<ColumnProperty> fields = SchemaCache.GetColumns<T>(t => t.IsKey);
            if (!fields.Any()) throw new Exception($"{ typeof(T).GetTableName() } No primary key");
            string sql = $"SELECT 0 FROM `{typeof(T).GetTableName()}` WHERE { string.Join(" AND ", fields.Select(t => $"{t.Name}=@{t.Name}")) } LIMIT 1;";
            DynamicParameters parameters = new DynamicParameters();
            foreach (ColumnProperty column in fields)
            {
                parameters.Add($"@{column.Name}", column.Property.GetValue(entity));
            }
            return db.ExecuteScalar(CommandType.Text, sql, parameters) != null;
        }

        public DataSet GetDataSet<T>(Expression<Func<T, bool>> condition, params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            string field = string.Join(",", SchemaCache.GetColumns(fields).Select(t => $"{t.Name}"));
            using (IExpressionCondition expression = db.GetExpressionCondition(condition))
            {
                string conditionSql = expression.ToCondition(out DynamicParameters parameters);
                string sql = $"SELECT {field} FROM {typeof(T).GetTableName()} {conditionSql};";
                return db.GetDataSet(CommandType.Text, sql, parameters.ToDbParameter());
            }
        }

        public DataSet GetDataSet<T>(T obj) where T : IProcedureModel
        {
            throw new NotImplementedException();
        }


        public bool Insert<T>(T entity) where T : class, new()
        {
            IEnumerable<ColumnProperty> fields = SchemaCache.GetColumns<T>().Where(t => !t.Identity);
            string sql = $"INSERT INTO {typeof(T).GetTableName()} ({ string.Join(",", fields.Select(t => $"{t.Name}")) }) VALUES({ string.Join(",", fields.Select(t => $"@{t.Name}")) });";
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
            string sql = $"INSERT INTO {typeof(T).GetTableName()} ({ string.Join(",", fields.Select(t => $"{t.Name}")) }) VALUES({ string.Join(",", fields.Select(t => $"@{t.Name}")) });SELECT LAST_INSERT_ID();";
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
            throw new NotImplementedException();
        }

        public IDataReader ReadData<T>(Expression<Func<T, bool>> condition, params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            string field = string.Join(",", SchemaCache.GetColumns(fields).Select(t => $"{t.Name}"));
            using (IExpressionCondition expression = db.GetExpressionCondition(condition))
            {
                string conditionSql = expression.ToCondition(out DynamicParameters parameters);
                string sql = $"SELECT {field} FROM {typeof(T).GetTableName()} {conditionSql};";
                return db.ReadData(CommandType.Text, sql, parameters);
            }
        }

        public IDataReader ReadData<T>(params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            string field = string.Join(",", SchemaCache.GetColumns(fields).Select(t => $"{t.Name}"));
            string sql = $"SELECT {field} FROM {typeof(T).GetTableName()};";
            return db.ReadData(CommandType.Text, sql);
        }

        public IDataReader ReadData<T>(T obj) where T : IProcedureModel
        {
            throw new NotImplementedException();
        }

        public TValue ReadInfo<T, TValue>(Expression<Func<T, TValue>> field, Expression<Func<T, bool>> condition) where T : class, new()
        {
            using (IExpressionCondition expression = db.GetExpressionCondition(condition))
            {
                string conditionSql = expression.ToCondition(out DynamicParameters parameters);
                string sql = $"SELECT  { SchemaCache.GetColumnProperty(field).Name } FROM {typeof(T).GetTableName()} {conditionSql}  LIMIT 0,1;";
                object value = db.ExecuteScalar(CommandType.Text, sql, parameters);
                if (value == null) return default;
                return (TValue)value;
            }
        }

        public T ReadInfo<T>(Expression<Func<T, bool>> condition, params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            using (IDataReader reader = this.ReadData(condition, fields))
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

        public IEnumerable<T> ReadList<T>(string condition, object? parameters = null, params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            List<T> list = new List<T>();
            string columns =
                fields.Length == 0 ?
                string.Join(",", SchemaCache.GetColumns<T>().Select(t => $"{t.Name}")) :
                string.Join(",", SchemaCache.GetColumns(fields).Select(t => $"{t.Name}"));
            using (IDataReader reader = db.ReadData(CommandType.Text, $"SELECT {columns} FROM {typeof(T).GetTableName()} WHERE {condition};", parameters))
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

        public IEnumerable<T> ReadList<T>(Expression<Func<T, bool>> condition, params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            List<T> list = new List<T>();
            using (IDataReader reader = this.ReadData(condition, fields))
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

        public IEnumerable<TValue> ReadList<T, TValue>(Expression<Func<T, TValue>> field, Expression<Func<T, bool>> condition) where T : class, new()
        {
            string fieldName = SchemaCache.GetColumnProperty(field).Name;
            List<TValue> list = new List<TValue>();
            using (IExpressionCondition expression = db.GetExpressionCondition(condition))
            {
                string conditionSql = expression.ToCondition(out DynamicParameters parameters);
                string sql = $"SELECT {fieldName} FROM {typeof(T).GetTableName()} {conditionSql};";
                IDataReader reader = db.ReadData(CommandType.Text, sql, parameters);
                while (reader.Read())
                {
                    list.Add((TValue)reader[0]);
                }
                if (!reader.IsClosed) reader.Close();
            }
            return list;
        }

        public IEnumerable<TResult> ReadList<TResult, T>(T obj)
            where TResult : class, new()
            where T : IProcedureModel
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TResult> ReadScalar<TResult, T>(T obj) where T : IProcedureModel
        {
            throw new NotImplementedException();
        }

        public int Update<T, TValue>(Expression<Func<T, TValue>> field, TValue value, Expression<Func<T, bool>> condition) where T : class, new()
        {
            using (IExpressionCondition expression = db.GetExpressionCondition(condition))
            {
                string conditionSql = expression.ToCondition(out DynamicParameters parameters);
                ColumnProperty column = SchemaCache.GetColumnProperty(field);
                parameters.Add("@Value", value.GetSafeValue(typeof(TValue)));
                string sql = $"UPDATE {typeof(T).GetTableName()} SET {column.Name}= @Value {conditionSql};";
                return db.ExecuteNonQuery(CommandType.Text, sql, parameters);
            }
        }

        public int Update<T, TField1, TField2>(Expression<Func<T, TField1>> field1, TField1 value1, Expression<Func<T, TField1>> field2, TField2 value2, Expression<Func<T, bool>> condition) where T : class, new()
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
                string sql = $"UPDATE {typeof(T).GetTableName()} SET {string.Join(",", updateFields.Select(t => $"{t} = @{t}"))} {conditionSql};";
                return db.ExecuteNonQuery(CommandType.Text, sql, parameters);
            }
        }

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
            string sql = $"UPDATE [{typeof(T).GetTableName()}] SET {string.Join(",", updateFields.Select(t => $"{t}= @{t}"))} WHERE { string.Join(" AND ", conditionFields.Select(t => $"[{t}] = @{t}")) };";
            return db.ExecuteNonQuery(CommandType.Text, sql, parameters);
        }

        public int UpdatePlus<T>(T entity, Expression<Func<T, bool>> condition, params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public TValue? UpdatePlus<T, TValue>(Expression<Func<T, TValue>> field, TValue value, Expression<Func<T, bool>> condition)
            where T : class, new()
            where TValue : struct
        {
            throw new NotImplementedException();
        }

        public int Update<T, TField1, TField2>(Expression<Func<T, TField1>> field1, TField1 value1, Expression<Func<T, TField2>> field2, TField2 value2, Expression<Func<T, bool>> condition) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> ReadList<T>(int top, string condition, string sort, object? parameters = null, params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public IQueryable<TEntity> Query<TEntity>() where TEntity : class, new()
        {
            throw new NotImplementedException();
        }

        public int UpdateIn<T, TValue, TKey>(Expression<Func<T, TValue>> field, TValue value, Expression<Func<T, TKey>> condition, TKey[] keys)
            where T : class, new()
            where TValue : struct
        {
            throw new NotImplementedException();
        }

        public int Update<T, TField1, TField2, TField3>(Expression<Func<T, TField1>> field1, TField1 value1, Expression<Func<T, TField2>> field2, TField2 value2, Expression<Func<T, TField3>> field3, TField3 value3, Expression<Func<T, bool>> condition) where T : class, new()
        {
            throw new NotImplementedException();
        }
    }
}
