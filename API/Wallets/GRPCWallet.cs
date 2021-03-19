using SP.StudioCore.API.Wallets.Requests;
using SP.StudioCore.API.Wallets.Responses;
using System;

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

        public BalanceResponse GetBalance(BalanceRequest request) => throw new NotImplementedException();

        public MoneyResponse ExecuteMoney(MoneyRequest request) => throw new NotImplementedException();

        public QueryResponse Query(QueryRequest request) => throw new NotImplementedException();
    }
}
