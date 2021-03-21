using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using SP.StudioCore.API.Wallets.Requests;

namespace SP.StudioCore.API.Wallets.Responses
{
    /// <summary>
    /// 资金操作之后的返回对象
    /// </summary>
    public class MoneyResponse : WalletResponseBase
    {
        public MoneyResponse(MoneyRequest request, int duration, Exception ex) : base(duration, ex)
        {
            Request = request;
        }

        public MoneyResponse(MoneyRequest request, string json, int duration) : base(json, duration)
        {
            Request = request;
        }

        /// <summary>
        /// 请求报文
        /// </summary>
        public MoneyRequest Request { get; }

        protected override void Construction(JObject info)
        {
        }
    }
}