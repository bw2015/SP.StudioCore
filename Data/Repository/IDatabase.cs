using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SP.StudioCore.Data.Repository
{
    public interface IDatabase
    {
        /// <summary>
        /// 回滚
        /// </summary>
        void RollBack();
        /// <summary>
        /// 提交
        /// </summary>
        void Commit();
        /// <summary>
        /// 回调
        /// </summary>
        /// <param name="action"></param>
        void AddCallback(Action action);
        /// <summary>
        /// Execute
        /// </summary>
        /// <param name="cmdText"></param>
        /// <returns></returns>
        bool Execute(string cmdText);
        bool Execute(string cmdText, object param);
        bool Execute(CommandType type, string cmdText, object param);
        T ExecuteScalar<T>(string cmdText);
        T ExecuteScalar<T>(string cmdText, object param);
        T ExecuteScalar<T>(CommandType type, string cmdText, object param);
        IDataReader ExecuteReader(string cmdText);
        IDataReader ExecuteReader(string cmdText, object param);
        IDataReader ExecuteReader(CommandType type, string cmdText, object param);
        DataSet GetDataSet(string cmdText);
        DataSet GetDataSet(string cmdText, object param);
        DataSet GetDataSet(CommandType type, string cmdText, object param);
    }
}
