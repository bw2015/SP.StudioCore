using SP.StudioCore.Model;
using SP.StudioCore.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SP.StudioCore.Data.Extension
{
    /// <summary>
    /// 操作存储过程
    /// </summary>
    [Obsolete("被IProcedureRepository取代")]
    public static class DbExecutorProcedure
    {
        /// <summary>
        /// 执行存储过程，返回受影响的行数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static int ExecuteNonQuery<T>(this DbExecutor db, T obj) where T : IProcedureModel
        {
            int rows = db.ExecuteNonQuery(CommandType.StoredProcedure, typeof(T).Name,
                 obj.ToParameters());
            obj.Fill();
            return rows;
        }

        /// <summary>
        /// 从存储过程获取DataSet
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static DataSet GetDataSet<T>(this DbExecutor db, T obj) where T : IProcedureModel
        {
            DataSet ds = db.GetDataSet(CommandType.StoredProcedure, typeof(T).Name,
                obj.ToDbParameter());
            obj.Fill();
            return ds;
        }

        /// <summary>
        /// 读取存储过程（单表返回）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IDataReader ReadData<T>(this DbExecutor db, T obj) where T : IProcedureModel
        {
            return db.ReadData(CommandType.StoredProcedure, typeof(T).Name, obj.ToParameters());
        }

        /// <summary>
        /// 读取存储过程（只有一张表的返回，使用IDataReader构造进行数据返回）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static List<TResult> ReadList<TResult, T>(this DbExecutor db, T obj) where T : IProcedureModel where TResult : class, new()
        {
            List<TResult> list = new List<TResult>();
            using (IDataReader reader = db.ReadData(CommandType.StoredProcedure, typeof(T).Name, obj.ToParameters()))
            {
                while (reader.Read())
                {
                    list.Add((TResult)Activator.CreateInstance(typeof(TResult), reader));
                }
                if (!reader.IsClosed) reader.Close();
            }
            return list;
        }

        /// <summary>
        /// 获取只有一列的LIST数据返回
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static List<TResult> ReadScalar<TResult, T>(this DbExecutor db, T obj) where T : IProcedureModel
        {
            List<TResult> list = new List<TResult>();
            using (IDataReader reader = db.ReadData(CommandType.StoredProcedure, typeof(T).Name, obj.ToParameters()))
            {
                while (reader.Read())
                {
                    list.Add((TResult)reader[0]);
                }
                if (!reader.IsClosed) reader.Close();
            }
            return list;
        }

        public static TValue ExecuteScalar<T, TValue>(this DbExecutor db, T obj) where T : IProcedureModel
        {
            object result = db.ExecuteScalar(CommandType.StoredProcedure, typeof(T).Name, obj.ToParameters());
            if (result == null) return default;
            return result.GetValue<TValue>();
        }
    }
}
