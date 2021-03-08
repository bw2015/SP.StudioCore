using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.API.Wallets.Responses
{
    /// <summary>
    /// 资金操作之后的返回对象
    /// </summary>
    public class MoneyResponse : WalletResponseBase
    {
        public MoneyResponse() : base()
        {

        }

        public MoneyResponse(string json) : base(json)
        {
        }

        protected override void Construction(JObject info)
        {
            
        }
    }
}
