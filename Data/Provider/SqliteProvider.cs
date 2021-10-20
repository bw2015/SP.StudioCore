using Dapper;
using Microsoft.Data.Sqlite;
using SP.StudioCore.Data.Expressions;
using SP.StudioCore.Data.Extension;
using SP.StudioCore.Data.Repository;
using SP.StudioCore.Data.Schema;
using SP.StudioCore.Model;
using SP.StudioCore.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Data.Provider
{
    /// <summary>
    /// Sqlite 数据库的实现
    /// </summary>
    public sealed class SqliteProvider : ISqlProvider, IWriteRepository, IProcedureRepository
    {
        private readonly DbExecutor db;

        public SqliteProvider(DbExecutor db)
        {
            this.db = db;
        }

        public int Count<T>(Expression<Func<T, bool>> condition) where T : class, new()
        {
            using (IExpressionCondition expression = db.GetExpressionCondition(condition))
            {
                string sql = $"SELECT COUNT(0) FROM [{typeof(T).GetTableName()}] { expression.ToCondition(out DynamicParameters parameters)} ";
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

        public int Delete<T>(Expression<Func<T, bool>> condition) where T : class, new()
        {
            using (IExpressionCondition expression = db.GetExpressionCondition(condition))
            {
                string sql = $"DELETE FROM [{typeof(T).GetTableName()}] { expression.ToCondition(out DynamicParameters parameters) }";
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
            string sql = $"DELETE FROM [{typeof(T).GetTableName()}] WHERE { string.Join(" AND ", conditionFields) }";
            return db.ExecuteNonQuery(CommandType.Text, sql, parameters) > 0;
        }

        public int ExecuteNonQuery<T>(T obj) where T : IProcedureModel
        {
            int rows = db.ExecuteNonQuery(CommandType.StoredProcedure, typeof(T).GetTableName(),
               obj.ToParameters());
            obj.Fill();
            return rows;
        }

        public TValue ExecuteScalar<T, TValue>(T obj) where T : IProcedureModel
        {
            object result = db.ExecuteScalar(CommandType.StoredProcedure, typeof(T).GetTableName(), obj.ToParameters());
            if (result == null) return default;
            return result.GetValue<TValue>();
        }

        public bool Exists<T>(Expression<Func<T, bool>> condition) where T : class, new()
        {
            using (IExpressionCondition expression = db.GetExpressionCondition(condition))
            {
                string sql = $"SELECT 0 WHERE EXISTS(SELECT 0 FROM [{typeof(T).GetTableName()}] { expression.ToCondition(out DynamicParameters parameters)} )";
                object value = db.ExecuteScalar(CommandType.Text, sql, parameters);
                return value != null;
            }
        }

        public bool Exists<T>() where T : class, new()
        {
            string sql = $"SELECT 0 WHERE EXISTS(SELECT 0 FROM [{typeof(T).GetTableName()}])";
            object value = db.ExecuteScalar(CommandType.Text, sql);
            return value != null;
        }

        public bool Exists<T>(T entity) where T : class, new()
        {
            IEnumerable<ColumnProperty> fields = SchemaCache.GetColumns<T>(t => t.IsKey);
            if (!fields.Any()) throw new Exception($"{ typeof(T).GetTableName() } No primary key");
            string sql = $"SELECT 0 WHERE EXISTS(SELECT 0 FROM [{typeof(T).GetTableName()}] WHERE { string.Join(" AND ", fields.Select(t => $"[{t.Name}]=@{t.Name}")) })";
            DynamicParameters parameters = new DynamicParameters();
            foreach (ColumnProperty column in fields)
            {
                parameters.Add($"@{column.Name}", column.Property.GetValue(entity));
            }
            object value = db.ExecuteScalar(CommandType.Text, sql, parameters);
            return value != null;
        }

        public DataSet GetDataSet<T>(T obj) where T : IProcedureModel
        {
            DataSet ds = db.GetDataSet(CommandType.StoredProcedure, typeof(T).GetTableName(),
               obj.ToDbParameter());
            obj.Fill();
            return ds;
        }

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

        public SQLResult Info<T>(T obj, params Expression<Func<T, object>>[] precate) where T : class, new()
        {
            Dictionary<ColumnProperty, object> condition = obj.GetCondition(precate);
            string tableName = obj.GetTableName();
            IEnumerable<ColumnProperty> fields = SchemaCache.GetColumns<T>();
            string sql = $"SELECT TOP 1 { string.Join(",", fields.Select(t => string.Format("[{0}]", t.Name))) } FROM [{tableName}] WHERE { string.Join(" AND ", condition.Select(t => $"[{t.Key.Name}] = @{t.Key.Property.Name}")) }";
            DbParameter[] parameters = condition.Select(t => new SqliteParameter($"@{t.Key.Property.Name}", t.Value)).ToArray();
            return new SQLResult()
            {
                CommandText = sql,
                Prameters = parameters
            };
        }

        public bool Insert<T>(T entity) where T : class, new()
        {
            IEnumerable<ColumnProperty> fields = SchemaCache.GetColumns<T>().Where(t => !t.Identity);
            string sql = $"INSERT INTO [{typeof(T).GetTableName()}]({ string.Join(",", fields.Select(t => $"[{t.Name}]")) }) VALUES({ string.Join(",", fields.Select(t => $"@{t.Name}")) });";
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
            string sql = $"INSERT INTO [{typeof(T).GetTableName()}]({ string.Join(",", fields.Select(t => $"[{t.Name}]")) }) VALUES({ string.Join(",", fields.Select(t => $"@{t.Name}")) });SELECT @@IDENTITY;";
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

        public IDataReader ReadData<T>(T obj) where T : IProcedureModel
        {
            return db.ReadData(CommandType.StoredProcedure, typeof(T).GetTableName(), obj.ToParameters());
        }

        public IDataReader ReadData<T>(Expression<Func<T, bool>> condition, params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public IDataReader ReadData<T>(params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public TValue ReadInfo<T, TValue>(Expression<Func<T, TValue>> field, Expression<Func<T, bool>> condition) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public T ReadInfo<T>(Expression<Func<T, bool>> condition, params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TResult> ReadList<TResult, T>(T obj)
            where TResult : class, new()
            where T : IProcedureModel
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

        public IEnumerable<T> ReadList<T>(string condition, object parameters = null, params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> ReadList<T>(Expression<Func<T, bool>> condition, params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> ReadList<T>(params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TValue> ReadList<T, TValue>(Expression<Func<T, TValue>> field, Expression<Func<T, bool>> condition) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> ReadList<T>(int top, string condition, string sort, object? parameters = null, params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            throw new NotImplementedException();
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

        public int Update<T, TValue>(Expression<Func<T, TValue>> field, TValue value, Expression<Func<T, bool>> condition) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public int Update<T>(T entity, Expression<Func<T, bool>> condition, params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public int Update<T>(T entity, params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public int Update<T, TField1, TField2>(Expression<Func<T, TField1>> field1, TField1 value1, Expression<Func<T, TField2>> field2, TField2 value2, Expression<Func<T, bool>> condition) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public int UpdatePlus<T>(T entity, Expression<Func<T, bool>> condition, params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public int UpdatePlus<T, TValue>(Expression<Func<T, TValue>> field, TValue value, Expression<Func<T, bool>> condition)
            where T : class, new()
            where TValue : struct
        {
            throw new NotImplementedException();
        }
    }
}
