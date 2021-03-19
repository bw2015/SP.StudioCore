using Newtonsoft.Json.Linq;
using SP.StudioCore.Json;
using System;
using System.Collections.Generic;
using System.Text;
using SP.StudioCore.API.Wallets.Requests;

namespace SP.StudioCore.API.Wallets.Responses
{
    /// <summary>
    /// 查询资金记录
    /// </summary>
    public class QueryResponse : WalletResponseBase
    {
        public QueryResponse(QueryRequest request, long duration, Exception ex) : base(duration, ex)
        {
            Request = request;
        }

        public QueryResponse(QueryRequest request, string json, long duration) : base(json,duration)
        {
            Request = request;
        }

        /// <summary>
        /// 请求报文
        /// </summary>
        public QueryRequest Request { get; }

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
