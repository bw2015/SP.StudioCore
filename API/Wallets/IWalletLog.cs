using SP.StudioCore.Ioc;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using SP.StudioCore.API.Wallets.Responses;
using SP.StudioCore.API.Wallets.Logs;

namespace SP.StudioCore.API.Wallets
{
    /// <summary>
    /// 免转钱包的通信日志
    /// </summary>
    public interface IWalletLog
    {
        /// <summary>
        /// 保存单一钱包的通信日志
        /// </summary>
        /// <returns></returns>
        public void SaveLog(WalletLog log);
    }
}
