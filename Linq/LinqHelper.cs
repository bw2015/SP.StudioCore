using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;

namespace SP.StudioCore.Linq
{
    /// <summary>
    /// EF的扩展帮助方法
    /// </summary>
    public static class LinqHelper
    {
        /// <summary>
        /// 执行 WITH(NOLOCK) 的查询操作
        /// </summary>
        /// <typeparam name="DB"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public static TResult NoLockExecute<DB, TResult>(Func<DB, TResult> action, TransactionOptions transactionOptions) where DB : DbContext, new()
        {
            using TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionOptions);
            try
            {
                using DB obj = new DB();
                return action(obj);
            }
            finally
            {
                transactionScope.Complete();
            }
        }

        public static TResult NoLockExecute<DB, TResult>(Func<DB, TResult> action) where DB : DbContext, new()
        {
            TransactionOptions transactionOptions = new TransactionOptions()
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            };
            using TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionOptions);
            try
            {
                using DB obj = new DB();
                return action(obj);
            }
            finally
            {
                transactionScope.Complete();
            }
        }
    }
}
