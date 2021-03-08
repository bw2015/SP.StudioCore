using SP.StudioCore.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SP.StudioCore.Data.Repository
{
    /// <summary>
    /// 对存储过程的操作
    /// </summary>
    public interface IProcedureRepository
    {
        /// <summary>
        /// 执行存储过程，返回受影响的行数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        int ExecuteNonQuery<T>(T obj) where T : IProcedureModel;

        /// <summary>
        /// 从存储过程获取DataSet
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        DataSet GetDataSet<T>(T obj) where T : IProcedureModel;

        /// <summary>
        /// 读取存储过程（单表返回）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        IDataReader ReadData<T>(T obj) where T : IProcedureModel;

        /// <summary>
        /// 读取存储过程（只有一张表的返回，使用IDataReader构造进行数据返回）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        IEnumerable<TResult> ReadList<TResult, T>(T obj) where T : IProcedureModel where TResult : class, new();

        /// <summary>
        /// 获取只有一列的LIST数据返回
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        IEnumerable<TResult> ReadScalar<TResult, T>(T obj) where T : IProcedureModel;

        /// <summary>
        /// 返回第一行第一列的内容
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        TValue ExecuteScalar<T, TValue>(T obj) where T : IProcedureModel;
    }
}
