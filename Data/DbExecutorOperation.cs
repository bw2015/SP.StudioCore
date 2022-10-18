using SP.StudioCore.Data.Provider;
using SP.StudioCore.Data.Repository;
using SP.StudioCore.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SP.StudioCore.Data
{
    /// <summary>
    /// 数据库写/读操作
    /// </summary>
    partial class DbExecutor : IWriteRepository
    {
        private IWriteRepository provider => this.GetWriteRepository();

        public int Delete<T>(Expression<Func<T, bool>> condition) where T : class, new() =>
            provider.Delete(condition);

        public int ExecuteNonQuery<T>(T obj) where T : IProcedureModel =>
            provider.ExecuteNonQuery(obj);


        public TValue ExecuteScalar<T, TValue>(T obj) where T : IProcedureModel =>
            provider.ExecuteScalar<T, TValue>(obj);


        public bool Exists<T>(Expression<Func<T, bool>> condition) where T : class, new() =>
            provider.Exists(condition);


        public bool Exists<T>() where T : class, new() =>
            provider.Exists<T>();


        public DataSet GetDataSet<T>(Expression<Func<T, bool>> condition, params Expression<Func<T, object>>[] fields) where T : class, new() =>
            provider.GetDataSet(condition, fields);


        public DataSet GetDataSet<T>(T obj) where T : IProcedureModel =>
            provider.GetDataSet(obj);

        public bool Insert<T>(T entity) where T : class, new() =>
            provider.Insert(entity);

        public bool InsertIdentity<T>(T entity) where T : class, new() =>
            provider.InsertIdentity(entity);

        public bool InsertNoIdentity<T>(T entity) where T : class, new() =>
            provider.InsertNoIdentity(entity);

        public IDataReader ReadData<T>(Expression<Func<T, bool>> condition, params Expression<Func<T, object>>[] fields) where T : class, new() =>
            provider.ReadData(condition, fields);



        public IDataReader ReadData<T>(params Expression<Func<T, object>>[] fields) where T : class, new() =>
            provider.ReadData(fields);


        public IEnumerable<TValue> ReadList<T, TValue>(Expression<Func<T, TValue>> field, Expression<Func<T, bool>> condition) where T : class, new() =>
            provider.ReadList(field, condition);


        public IDataReader ReadData<T>(T obj) where T : IProcedureModel =>
            provider.ReadData(obj);

        public TValue ReadInfo<T, TValue>(Expression<Func<T, TValue>> field, Expression<Func<T, bool>> condition) where T : class, new() =>
            provider.ReadInfo(field, condition);

        public T ReadInfo<T>(Expression<Func<T, bool>> condition, params Expression<Func<T, object>>[] fields) where T : class, new() =>
            provider.ReadInfo(condition, fields);

        public IEnumerable<T> ReadList<T>(string condition, object? parameters = null, params Expression<Func<T, object>>[] fields) where T : class, new() =>
            provider.ReadList(condition, parameters, fields);

        public IEnumerable<T> ReadList<T>(int top, string condition, string sort, object? parameters = null, params Expression<Func<T, object>>[] fields) where T : class, new() =>
            provider.ReadList(top, condition, sort, parameters, fields);

        public IEnumerable<T> ReadList<T>(Expression<Func<T, bool>> condition, params Expression<Func<T, object>>[] fields) where T : class, new() =>
            provider.ReadList(condition, fields);

        public IEnumerable<T> ReadList<T>(params Expression<Func<T, object>>[] fields) where T : class, new() =>
            provider.ReadList(fields);

        public IEnumerable<TResult> ReadList<TResult, T>(T obj)
            where TResult : class, new()
            where T : IProcedureModel =>
            provider.ReadList<TResult, T>(obj);

        public IEnumerable<TResult> ReadScalar<TResult, T>(T obj) where T : IProcedureModel =>
            provider.ReadScalar<TResult, T>(obj);

        public int Update<T, TValue>(Expression<Func<T, TValue>> field, TValue value, Expression<Func<T, bool>> condition) where T : class, new() =>
            provider.Update(field, value, condition);

        public int Update<T, TField1, TField2>(Expression<Func<T, TField1>> field1, TField1 value1, Expression<Func<T, TField2>> field2, TField2 value2, Expression<Func<T, bool>> condition) where T : class, new() =>
            provider.Update(field1, value1, field2, value2, condition);

        public int Update<T, TField1, TField2, TField3>(Expression<Func<T, TField1>> field1, TField1 value1, Expression<Func<T, TField2>> field2, TField2 value2, Expression<Func<T, TField3>> field3, TField3 value3, Expression<Func<T, bool>> condition) where T : class, new() =>
            provider.Update(field1, value1, field2, value2, field3, value3, condition);

        public int Update<T>(T entity, Expression<Func<T, bool>> condition, params Expression<Func<T, object>>[] fields) where T : class, new() =>
            provider.Update(entity, condition, fields);

        public int UpdatePlus<T>(T entity, Expression<Func<T, bool>> condition, params Expression<Func<T, object>>[] fields) where T : class, new() =>
            provider.UpdatePlus(entity, condition, fields);

        public int Update<T>(T entity, params Expression<Func<T, object>>[] fields) where T : class, new() =>
            provider.Update(entity, fields);

        public bool Delete<T>(T entity) where T : class, new() =>
            provider.Delete(entity);

        public TValue? UpdatePlus<T, TValue>(Expression<Func<T, TValue>> field, TValue value, Expression<Func<T, bool>> updateCondition, Expression<Func<T, bool>>? condition = null) where T : class, new() where TValue : struct =>
            provider.UpdatePlus(field, value, updateCondition, condition);

        public int UpdateIn<T, TValue, TKey>(Expression<Func<T, TValue>> field, TValue value, Expression<Func<T, TKey>> condition, TKey[] keys)
          where T : class, new()
          where TValue : struct => provider.UpdateIn(field, value, condition, keys);

        public bool Exists<T>(T entity) where T : class, new() =>
            provider.Exists(entity);

        public int Count<T>(Expression<Func<T, bool>> condition) where T : class, new() => provider.Count(condition);

        public int Count<T>() where T : class, new() => provider.Count<T>();

        public IQueryable<TEntity> Query<TEntity>() where TEntity : class, new() => provider.Query<TEntity>();


    }
}
