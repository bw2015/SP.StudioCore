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
        public QueryResponse(QueryRequest request, string json, int duration, bool isException = false) : base(json, duration, isException)
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
        public bool Exists { get; private set; }

        /// <summary>
        /// 商户是有实现了该API
        /// </summary>
        public bool SiteHaveApi { get; private set; }

        protected override void Construction(JObject info)
        {
            if (info == null) return;

            var exists = info.Get<string>("Exists") ?? "";
            switch (exists)
            {
                case "0":
                    SiteHaveApi = true;
                    Exists      = false;
                    break;
                case "1":
                    SiteHaveApi = true;
                    Exists      = true;
                    break;
            }

            //this.Exists = info.Get<int>("Exists") == 1;
        }
    }
}