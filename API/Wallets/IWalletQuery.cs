using SP.StudioCore.API.Wallets.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.API.Wallets
{
    /// <summary>
    /// 资金异常的查询实现方法
    /// </summary>
    public interface IWalletQuery
    {
        /// <summary>
        /// 发生异常的时候保存查询
        /// </summary>
        /// <param name="request">发生异常的资金请求对象</param>
        void SaveException(string url, MoneyRequest request);
    }
}
