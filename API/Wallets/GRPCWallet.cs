using SP.StudioCore.API.Wallets.Requests;
using SP.StudioCore.API.Wallets.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.API.Wallets
{
    /// <summary>
    /// gPRC通信
    /// </summary>
    public sealed class GRPCWallet : IWallet
    {
        public GRPCWallet()
        {
        }

        public MoneyResponse ExecuteMoney(string url, MoneyRequest request)
        {
            throw new NotImplementedException();
        }

        public BalanceResponse GetBalance(string url, BalanceRequest request)
        {
            throw new NotImplementedException();
        }

        public QueryResponse Query(string url, QueryRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
