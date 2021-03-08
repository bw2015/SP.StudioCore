using Newtonsoft.Json.Linq;
using SP.StudioCore.Array;
using SP.StudioCore.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.API.Wallets.Responses
{
    public sealed class BalanceResponse : WalletResponseBase
    {
        /// <summary>
        /// 余额
        /// </summary>
        public decimal? Balance { get; private set; }

        public BalanceResponse(string json) : base(json)
        {
        }

        public BalanceResponse() : base()
        {
        }

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
