﻿using SP.StudioCore.API.Wallets.Requests;
using SP.StudioCore.API.Wallets.Responses;
using SP.StudioCore.Net;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using SP.StudioCore.Web;

namespace SP.StudioCore.API.Wallets
{
    /// <summary>
    /// 基于http通信
    /// </summary>
    public class HttpWallet : IWallet
    {
        /// <summary>
        /// 执行资金操作
        /// </summary>
        public virtual MoneyResponse ExecuteMoney(MoneyRequest request)
        {
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                request.RequestAt = DateTime.Now;
                var result = NetAgent.UploadData(request.Url, request.PostData, Encoding.UTF8, null, new Dictionary<string, string>()
                {
                    {"Content-Type", "application/json"},
                    {"Content-Language", request.Language.ToString()},
                    {"X-Forwarded-IP", IPAgent.IP}
                });

                return new MoneyResponse(request, result, (int)sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                return new MoneyResponse(request, (int)sw.ElapsedMilliseconds, ex);
            }
        }

        public virtual BalanceResponse GetBalance(BalanceRequest request)
        {
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                request.RequestAt = DateTime.Now;
                var result = NetAgent.UploadData(request.Url, request.PostData, Encoding.UTF8, null, new Dictionary<string, string>()
                {
                    {"Content-Type", "application/json"},
                    {"Content-Language", request.Language.ToString()},
                    {"X-Forwarded-IP", IPAgent.IP}
                });

                return new BalanceResponse(request, result, (int)sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                return new BalanceResponse(request, (int)sw.ElapsedMilliseconds, ex);
            }
        }

        public virtual QueryResponse Query(QueryRequest request)
        {
            var sw = new Stopwatch();
            sw.Start();

            try
            {
                request.RequestAt = DateTime.Now;
                var result = NetAgent.UploadData(request.Url, request.PostData, Encoding.UTF8, null, new Dictionary<string, string>()
                {
                    {"Content-Type", "application/json"},
                    {"Content-Language", request.Language.ToString()}
                });

                return new QueryResponse(request, result, (int)sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                return new QueryResponse(request, (int)sw.ElapsedMilliseconds, ex);
            }
        }
    }
}