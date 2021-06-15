using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text;
using System.Web;
using static Dapper.SqlMapper;

namespace SP.StudioCore.Data
{
    /// <summary>
    /// 数据库操作对象
    /// </summary>
    public partial class DbExecutor : IDisposable
    {
        private readonly IDbConnection conn;

        private readonly IDbTransaction tran;

        private List<Action> callback;

        /// <summary>
        /// 数据库类型
        /// </summary>
        public DatabaseType DBType { get; private set; }

        /// <summary>
        /// commit之後要執行的回調方法
        /// </summary>
        /// <param name="action"></param>
        public void AddCallback(Action action)
        {
            if (this.tran == null) throw new NotSupportedException("未开启事务不能注册回调");
            if (this.callback == null) this.callback = new List<Action>();
            this.callback.Add(action);
        }

        /// <summary>
        /// 创建一个数据库链接对象
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        private IDbConnection CreateDbConnection(string connectionString)
        {
            return this.DBType switch
            {
                DatabaseType.SqlServer => new SqlConnection(connectionString),
                DatabaseType.MySql => new MySqlConnection(connectionString),
                DatabaseType.SQLite => new SqliteConnection(connectionString),
                _ => throw new NotSupportedException($"暂不支持数据库类型 { this.DBType }")
            };
        }

        /// <summary>
        /// 构造函数 赋值
        /// </summary>
        /// <param name="connectionString">链接字符串</param>
        /// <param name="dbType">数据库类型</param>
        /// <param name="isTran">是否启用事务</param>
        public DbExecutor(string connectionString, DatabaseType dbType = DatabaseType.SqlServer, IsolationLevel tranLevel = IsolationLevel.Unspecified)
        {
            this.DBType = dbType;
            conn = this.CreateDbConnection(connectionString);

            if (tranLevel != IsolationLevel.Unspecified)
            {
                conn.Open();
                tran = conn.BeginTransaction(tranLevel);
            }
        }

        public int ExecuteNonQuery(string commandText)
        {
            return this.ExecuteNonQuery(CommandType.Text, commandText);
        }

        public int ExecuteNonQuery(string procedureName, object parameters = null)
        {
            return this.ExecuteNonQuery(CommandType.StoredProcedure, procedureName, parameters);
        }

        public int ExecuteNonQuery(CommandType cmdType, string cmdText, object parameters = null)
        {
            try
            {
                return this.conn.Execute(cmdText, parameters, this.tran, null, cmdType);
            }
            catch (Exception ex)
            {
                throw new DataException(this.ThrowException(ex, cmdText, parameters));
            }
            finally
            {
                this.CloseConnection();
            }
        }

        public object ExecuteScalar(string commandText)
        {
            return this.ExecuteScalar(CommandType.Text, commandText);
        }

        public object ExecuteScalar(string procedureName, object parameters = null)
        {
            return this.ExecuteScalar(CommandType.StoredProcedure, procedureName, parameters);
        }

        public object ExecuteScalar(CommandType cmdType, string cmdText, object parameters = null)
        {
            try
            {
                return this.conn.ExecuteScalar(cmdText, parameters, this.tran, null, cmdType);
            }
            catch (Exception ex)
            {
                throw new DataException(this.ThrowException(ex, cmdText, parameters));
            }
            finally
            {
                this.CloseConnection();
            }
        }

        public DataSet GetDataSet(string commandText)
        {
            return this.GetDataSet(CommandType.Text, commandText);
        }

        public DataSet GetDataSet(string procedureName, params DbParameter[] parameter)
        {
            return this.GetDataSet(CommandType.StoredProcedure, procedureName, parameter);
        }

        public DataSet GetDataSet(CommandType cmdType, string cmdText, params DbParameter[] parameter)
        {
            IDbCommand comm = this.conn.CreateCommand();
            comm.CommandType = cmdType;
            comm.CommandText = cmdText;
            foreach (DbParameter param in parameter)
            {
                comm.Parameters.Add(param);
            }
            DbDataAdapter adapter = new SqlDataAdapter();

            if (tran != null) comm.Transaction = tran;
            try
            {
                adapter.SelectCommand = (SqlCommand)comm;
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                return ds;
            }
            catch (Exception ex)
            {
                throw new Exception(this.ThrowException(ex, cmdText, parameter));
            }
            finally
            {
                this.CloseConnection();
            }
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IDataReader ReadData(CommandType cmdType, string cmdText, object parameters = null)
        {
            try
            {
                return this.conn.ExecuteReader(cmdText, parameters, this.tran, null, cmdType);
            }
            catch (Exception ex)
            {
                throw new DataException(this.ThrowException(ex, cmdText, parameters));
            }
            finally
            {
                this.CloseConnection();
            }
        }


        public void Commit()
        {
            if (tran == null)
                throw new DataException("未开启事务");

            tran.Commit();

            if (callback != null)
            {
                foreach (Action action in this.callback)
                {
                    action.Invoke();
                }
            }
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        public void Rollback()
        {
            if (tran == null)
                throw new DataException("未开启事务");
            tran.Rollback();
        }

        /// <summary>
        /// 关闭数据库连接
        /// </summary>
        private void CloseConnection()
        {

        }

        /// <summary>
        /// Dapar抛出异常
        /// </summary>
        private string ThrowException(Exception ex, string cmdText, object param)
        {
            StringBuilder sb = new StringBuilder("{")
                .Append($"\"Message\":\"{ HttpUtility.JavaScriptStringEncode(ex.Message) }\"")
                .Append($",\"Command\":\"{ HttpUtility.JavaScriptStringEncode(cmdText) }\"");
            if (param != null)
            {
                sb.Append(",\"Params\":{");
                List<string> parameters = new List<string>();
                if (param.GetType() == typeof(DynamicParameters))
                {
                    DynamicParameters dynamicParameters = (DynamicParameters)param;

                    foreach (string paramName in dynamicParameters.ParameterNames)
                    {
                        object value = dynamicParameters.Get<object>(paramName);
                        if (value == null) value = string.Empty;
                        parameters.Add($"\"{ paramName }\":\"{ HttpUtility.JavaScriptStringEncode(value.ToString()) }\"");
                    }
                }
                else
                {
                    foreach (PropertyInfo property in param.GetType().GetProperties())
                    {
                        object value = property.GetValue(param);
                        if (value == null) value = string.Empty;
                        parameters.Add($"\"{ property.Name }\":\"{ HttpUtility.JavaScriptStringEncode(value.ToString()) }\"");
                    }
                }
                sb.Append(string.Join(",", parameters))
                    .Append("}");
            }
            sb.Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// ADO.Net操作抛出异常
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="cmdText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private string ThrowException(Exception ex, string cmdText, params DbParameter[] parameters)
        {
            StringBuilder sb = new StringBuilder("{")
               .Append($"\"Message\":\"{ HttpUtility.JavaScriptStringEncode(ex.Message) }\"")
               .Append($",\"Command\":\"{HttpUtility.JavaScriptStringEncode(cmdText)}\"");
            sb.Append(",\"Params\":{");
            List<string> list = new();
            foreach (DbParameter param in parameters)
            {
                list.Add($"\"{ param.ParameterName }\":\"{ HttpUtility.JavaScriptStringEncode(param.Value.ToString()) }\"");
            }
            sb.Append(string.Join(",", list))
                .Append('}').Append('}');
            return sb.ToString();
        }

        public DbParameter NewParam(string parameterName, object value)
        {
            DbType dbType;
            switch (value.GetType().Name)
            {
                case "DateTime":
                    dbType = DbType.DateTime;
                    break;
                case "Boolean":
                    dbType = DbType.Boolean;
                    break;
                case "Int32":
                    dbType = DbType.Int32;
                    break;
                case "Int16":
                    dbType = DbType.Int16;
                    break;
                case "Decimal":
                    dbType = DbType.Decimal;
                    break;
                case "Byte":
                    dbType = DbType.Byte;
                    break;
                case "Guid":
                    dbType = DbType.Guid;
                    break;
                default:
                    dbType = DbType.String;
                    break;
            }
            return this.NewParam(parameterName, value, dbType, 0, ParameterDirection.Input);
        }

        /// <summary>
        /// ADO.Net的参数对象
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <param name="dbType"></param>
        /// <param name="size"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public DbParameter NewParam(string parameterName, object value, DbType dbType, int size, ParameterDirection direction)
        {
            DbParameter param = this.DBType switch
            {
                DatabaseType.SqlServer => new SqlParameter()
                {
                    ParameterName = parameterName,
                    Value = value,
                    DbType = dbType,
                    Size = size,
                    Direction = direction
                },
                _ => throw new NotImplementedException()
            };
            return param;
        }

        public void Dispose()
        {
            if (this.tran != null) tran.Dispose();

            if (this.conn.State == ConnectionState.Open)
            {
                this.conn.Close();
            }

            GC.SuppressFinalize(this);
        }
    }
}
