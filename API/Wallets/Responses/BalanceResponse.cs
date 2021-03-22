using Newtonsoft.Json.Linq;
using SP.StudioCore.Array;
using SP.StudioCore.Json;
using System;
using System.Collections.Generic;
using System.Text;
using SP.StudioCore.API.Wallets.Requests;

namespace SP.StudioCore.API.Wallets.Responses
{
    public sealed class BalanceResponse : WalletResponseBase
    {
        /// <summary>
        /// 余额
        /// </summary>
        public decimal? Balance { get; private set; }

        public BalanceResponse(BalanceRequest request, string json, int duration, bool isException = false) : base(json, duration, isException)
        {
            Request = request;
        }


        /// <summary>
        /// 请求报文
        /// </summary>
        public BalanceRequest Request { get; }

        protected override void Construction(JObject info)
        {
            if (info != null)
            {
                this.Balance = info.Get<decimal>("Balance");
            }
        }

        public static implicit operator decimal(BalanceResponse response)
        {
            return response.Balance ?? decimal.Zero;
        }
    }
}