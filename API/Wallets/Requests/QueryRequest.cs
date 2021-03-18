using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.API.Wallets.Requests
{
    /// <summary>
    /// 查询资金是否操作成功
    /// </summary>
    public class QueryRequest : WalletRequestBase
    {
        public QueryRequest(string secretKey, string url) : base(secretKey, url)
        {
        }

        public override string Action => "Query";

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 资金类型
        /// </summary>
        public Enum Type { get; set; }

        /// <summary>
        /// 唯一ID
        /// </summary>
        public string ID { get; set; }
    }
}