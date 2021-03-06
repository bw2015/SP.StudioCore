using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.API.Wallets.Requests
{
    /// <summary>
    /// 请求余额查询
    /// </summary>
    public class BalanceRequest : WalletRequestBase
    {
        public BalanceRequest(string secretKey, string url) : base(secretKey,url)
        {
        }

        /// <summary>
        /// 动作名称
        /// </summary>
        public override string Action => "Balance";

        /// <summary>
        /// 要查询的用户名
        /// </summary>
        public string UserName { get; set; }
    }
}
