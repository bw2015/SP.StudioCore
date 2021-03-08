using Newtonsoft.Json.Linq;
using SP.StudioCore.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.API.Wallets.Responses
{
    /// <summary>
    /// 查询资金记录
    /// </summary>
    public class QueryResponse : WalletResponseBase
    {
        public QueryResponse()
        {

        }

        public QueryResponse(string json) : base(json)
        {

        }

        /// <summary>
        /// 是否存在该笔资金记录
        /// </summary>
        public bool? Exists { get; private set; }

        protected override void Construction(JObject info)
        {
            if (info == null) return;
            this.Exists = info.Get<int>("Exists") == 1;
        }
    }
}
