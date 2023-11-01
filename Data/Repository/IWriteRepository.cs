using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;

namespace SP.StudioCore.Data.Repository
{
    /// <summary>
    /// 可写操作
    /// </summary>
    public interface IWriteRepository : IReadRepository
    {
        #region ========  Delete  ========

        /// <summary>
        /// 根据条件删除数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="condition"></param>
        /// <returns></returns>
        int Delete<T>(Expression<Func<T, bool>> condition) where T : class, new();

        /// <summary>
        /// 使用主键删除对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        bool Delete<T>(T entity) where T : class, new();

        #endregion

        #region ========  Update  ========

        /// <summary>
        /// 更新一个字段
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="field">更新的字段</param>
        /// <param name="value">更新的值</param>
        /// <param name="condition">条件</param>
        /// <returns></returns>
        int Update<T, TValue>(Expression<Func<T, TValue>> field, TValue value, Expression<Func<T, bool>> condition) where T : class, new();

        /// <summary>
        /// 更新两个字段
        /// </summary>
        int Update<T, TField1, TField2>(Expression<Func<T, TField1>> field1, TField1 value1, Expression<Func<T, TField2>> field2, TField2 value2, Expression<Func<T, bool>> condition) where T : class, new();


        /// <summary>
        /// 更新三个字段
        /// </summary>
        int Update<T, TField1, TField2, TField3>(Expression<Func<T, TField1>> field1, TField1 value1, Expression<Func<T, TField2>> field2, TField2 value2, Expression<Func<T, TField3>> field3, TField3 value3, Expression<Func<T, bool>> condition) where T : class, new();

        /// <summary>
        /// 更新多个字段
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">实体</param>
        /// <param name="condition">条件</param>
        /// <param name="fields">多个值，如果留空的话则更新除主键和自增列之外的所有字段</param>
        /// <returns></returns>
        int Update<T>(T entity, Expression<Func<T, bool>> condition, params Expression<Func<T, object>>[] fields) where T : class, new();

        /// <summary>
        /// 使用主键作为条件更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="fields">多个值，如果留空的话则更新除主键和自增列之外的所有字段</param>
        /// <returns></returns>
        int Update<T>(T entity, params Expression<Func<T, object>>[] fields) where T : class, new();

        /// <summary>
        /// 更新增长型字段（自动跳过为0的字段）
        /// </summary>
        int UpdatePlus<T>(T entity, Expression<Func<T, bool>> condition, params Expression<Func<T, object>>[] fields) where T : class, new();

        /// <summary>
        /// 自增单个字段，TValue只能是数字型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="updateCondition"></param>
        /// <returns></returns>
        TValue? UpdatePlus<T, TValue>(Expression<Func<T, TValue>> field, TValue value, Expression<Func<T, bool>> updateCondition, Expression<Func<T, bool>>? condition = null) where T : class, new() where TValue : struct;

        /// <summary>
        /// 执行两个字段的自加
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TValue1"></typeparam>
        /// <typeparam name="TValue2"></typeparam>
        /// <param name="field1"></param>
        /// <param name="value1"></param>
        /// <param name="field2"></param>
        /// <param name="value2"></param>
        /// <param name="updateCondition"></param>
        /// <param name="condition"></param>
        /// <returns>返回受影响的行</returns>
        int UpdatePlus<T, TValue1, TValue2>(Expression<Func<T, TValue1>> field1, TValue1 value1, Expression<Func<T, TValue2>> field2, TValue2 value2, Expression<Func<T, bool>> updateCondition, Expression<Func<T, bool>>? condition = null) where T : class, new() where TValue1 : struct where TValue2 : struct;

        /// <summary>
        /// 批量修改 IN 方式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="condition"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        int UpdateIn<T, TValue, TKey>(Expression<Func<T, TValue>> field, TValue value, Expression<Func<T, TKey>> condition, TKey[] keys) where T : class, new() where TValue : struct;

        #endregion

        #region ========  Insert ========

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        bool Insert<T>(T entity) where T : class, new();

        /// <summary>
        /// 插入数据并且取得自动编号值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        bool InsertIdentity<T>(T entity) where T : class, new();

        /// <summary>
        /// 插入数据并指定自动编号字段的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        bool InsertNoIdentity<T>(T entity) where T : class, new();

        #endregion
    }
}
