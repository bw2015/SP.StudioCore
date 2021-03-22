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
        public MoneyResponse(MoneyRequest request, string json, int duration, bool isException = false) : base(json, duration, isException)
        {
            Request = request;
        }

        /// <summary>
        /// 请求报文
        /// </summary>
        public MoneyRequest Request { get; }
        
        /// <summary>
        /// 是否允许重试
        /// </summary>
        public bool CanRetry { get; set; }

        protected override void Construction(JObject info)
        {
        }
    }
}