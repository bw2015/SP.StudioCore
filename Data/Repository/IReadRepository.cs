using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Data;
using System.Linq;

namespace SP.StudioCore.Data.Repository
{
    /// <summary>
    /// 只读操作
    /// </summary>
    public interface IReadRepository : IProcedureRepository, IRepository
    {
        #region ========  ReadInfo  ========

        /// <summary>
        /// 条件筛选获取一个值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="field">要获取的字段</param>
        /// <param name="condition">查询条件</param>
        /// <returns></returns>
        TValue ReadInfo<T, TValue>(Expression<Func<T, TValue>> field, Expression<Func<T, bool>> condition) where T : class, new();

        /// <summary>
        /// 读取单个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="condition">查询条件</param>
        /// <param name="fields">指定要查询的字段</param>
        /// <returns></returns>
        T ReadInfo<T>(Expression<Func<T, bool>> condition, params Expression<Func<T, object>>[] fields) where T : class, new();

        #endregion

        #region ========  ReadData  ========

        /// <summary>
        /// 返回一个数据库读取对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="condition"></param>
        /// <param name="fields">指定要查询的字段</param>
        /// <returns></returns>
        IDataReader ReadData<T>(Expression<Func<T, bool>> condition, params Expression<Func<T, object>>[] fields) where T : class, new();

        /// <summary>
        /// 返回一个不带条件的数据库读取对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fields">指定要查询的字段</param>
        /// <returns></returns>
        IDataReader ReadData<T>(params Expression<Func<T, object>>[] fields) where T : class, new();

        #endregion

        #region ========  GetDataSet  ========

        /// <summary>
        /// 获取DataSet
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="condition"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        DataSet GetDataSet<T>(Expression<Func<T, bool>> condition, params Expression<Func<T, object>>[] fields) where T : class, new();

        #endregion

        #region ========  ReadList  ========

        /// <summary>
        /// 使用SQL查询返回一个list（使用 IDataReader 构造）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="condition">查询条件</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IEnumerable<T> ReadList<T>(string condition, object parameters = null, params Expression<Func<T, object>>[] fields) where T : class, new();

        /// <summary>
        /// 查询表的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="condition">查询条件</param>
        /// <param name="fields">指定要返回的字段 不填写为所有</param>
        /// <returns></returns>
        IEnumerable<T> ReadList<T>(Expression<Func<T, bool>> condition, params Expression<Func<T, object>>[] fields) where T : class, new();

        /// <summary>
        /// 返回表的所有数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="fields">指定要返回的字段 不填写为所有</param>
        /// <returns></returns>
        IEnumerable<T> ReadList<T>(params Expression<Func<T, object>>[] fields) where T : class, new();

        /// <summary>
        /// 读取数据，使用泛型返回（只返回一列）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="field"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        IEnumerable<TValue> ReadList<T, TValue>(Expression<Func<T, TValue>> field, Expression<Func<T, bool>> condition) where T : class, new();

        /// <summary>
        /// 查询（延迟执行）
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        IQueryable<TEntity> Query<TEntity>() where TEntity : class, new();

        #endregion

        #region ========  Exists  ========

        /// <summary>
        /// 是否存在数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="condition">查询条件</param>
        /// <returns></returns>
        bool Exists<T>(Expression<Func<T, bool>> condition) where T : class, new();

        /// <summary>
        /// 整个表内是否存在数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool Exists<T>() where T : class, new();

        /// <summary>
        /// 根据主键判断是否存在
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        bool Exists<T>(T entity) where T : class, new();

        #endregion

        #region ========== Count ===========
        int Count<T>(Expression<Func<T, bool>> condition) where T : class, new();
        int Count<T>() where T : class, new();
        #endregion

    }
}
