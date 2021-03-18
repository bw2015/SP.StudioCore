using SP.StudioCore.API.Wallets.Requests;
using SP.StudioCore.API.Wallets.Responses;
using SP.StudioCore.Net;
using System;
using System.Collections.Generic;
using System.Text;
using SP.StudioCore.Ioc;

namespace SP.StudioCore.API.Wallets
{
    /// <summary>
    /// 单一钱包通信接口
    /// 按照通信协议的不同进行实现
    /// </summary>
    public interface IWallet
    {
        /// <summary>
        /// 获取余额
        /// </summary>
        /// <param name="request">请求内容</param>
        /// <returns>返回null表示网络请求失败或者程序异常</returns>
        BalanceResponse GetBalance(BalanceRequest request);

        /// <summary>
        /// 操作资金
        /// </summary>
        /// <param name="request">请求内容</param>
        /// <returns>是否操作成功，返回null表示异常</returns>
        MoneyResponse ExecuteMoney(MoneyRequest request);

        /// <summary>
        /// 查询资金记录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        QueryResponse Query(QueryRequest request);
    }
}