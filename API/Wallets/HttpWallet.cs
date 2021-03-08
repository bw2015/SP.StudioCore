using SP.StudioCore.API.Wallets.Requests;
using SP.StudioCore.API.Wallets.Responses;
using SP.StudioCore.Ioc;
using SP.StudioCore.Net;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using SP.StudioCore.API.Wallets.Logs;
using System.Diagnostics;
using SP.StudioCore.Web;
using SP.StudioCore.Json;

namespace SP.StudioCore.API.Wallets
{
    /// <summary>
    /// 基于http通信
    /// </summary>
    public class HttpWallet : IWallet
    {
        /// <summary>
        /// 日志操作对象
        /// </summary>
        private IWalletLog WalletLog => IocCollection.GetService<IWalletLog>();

        /// <summary>
        /// 异常处理
        /// </summary>
        private IWalletQuery WalletQuery => IocCollection.GetService<IWalletQuery>();

        protected virtual string GetPostData(MoneyRequest request)
        {
            return request.ToString();
        }

        protected virtual string GetPostData(BalanceRequest request)
        {
            return request.ToString();
        }

        /// <summary>
        /// 执行资金操作
        /// </summary>
        /// <param name="url"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual MoneyResponse ExecuteMoney(string url, MoneyRequest request)
        {
            WalletLog log = default;
            string postData = this.GetPostData(request);
            string result = string.Empty;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            // 是否发生了异常
            bool isException = false;
            try
            {
                result = NetAgent.UploadData(url, postData, Encoding.UTF8, null, new Dictionary<string, string>()
                {
                    {"Content-Type","application/json" },
                    {"Content-Language",request.Language.ToString() },
                    {"X-Forwarded-IP",IPAgent.IP }
                });
                MoneyResponse response = new MoneyResponse(result);
                log = new WalletLog(request.Action, response, url, postData, result, sw.ElapsedMilliseconds, request.ExtendData);
                isException = (bool?)response == null;
                return response;
            }
            catch (Exception ex)
            {
                log = new WalletLog(request.Action, null, url, postData, new { ex.Message, Result = result }.ToJson(), sw.ElapsedMilliseconds, request.ExtendData);
                isException = true;
                return new MoneyResponse();
            }
            finally
            {
                if (this.WalletLog != null)
                {
                    this.WalletLog.SaveLog(log);
                }
                if (isException && this.WalletQuery != null)
                {
                    this.WalletQuery.SaveException(url, request);
                }
            }
        }

        public virtual BalanceResponse GetBalance(string url, BalanceRequest request)
        {
            WalletLog log = default;
            string postData = this.GetPostData(request);
            string result = string.Empty;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                result = NetAgent.UploadData(url, postData, Encoding.UTF8, null, new Dictionary<string, string>()
                {
                    {"Content-Type","application/json" },
                    {"Content-Language",request.Language.ToString() },
                    {"X-Forwarded-IP",IPAgent.IP }
                });
                BalanceResponse response = new BalanceResponse(result);
                log = new WalletLog(request.Action, response, url, postData, result, sw.ElapsedMilliseconds, request.ExtendData);
                return response;
            }
            catch (Exception ex)
            {
                log = new WalletLog(request.Action, false, url, postData, ex.Message + "\n" + result, sw.ElapsedMilliseconds, request.ExtendData);
                return new BalanceResponse();
            }
            finally
            {
                if (WalletLog != null) WalletLog.SaveLog(log);
            }
        }

        public virtual QueryResponse Query(string url, QueryRequest request)
        {
            WalletLog log = default;
            string postData = request.ToString();
            string result = string.Empty;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                result = NetAgent.UploadData(url, request.ToString(), Encoding.UTF8, null, new Dictionary<string, string>()
                {
                    {"Content-Type","application/json" },
                    {"Content-Language",request.Language.ToString() }
                });
                QueryResponse response = new QueryResponse(result);
                log = new WalletLog(request.Action, response, url, postData, result, sw.ElapsedMilliseconds, request.ExtendData);
                return response;
            }
            catch (Exception ex)
            {
                log = new WalletLog(request.Action, false, url, postData, ex.Message + "\n" + result, sw.ElapsedMilliseconds, request.ExtendData);
                return new QueryResponse();
            }
            finally
            {
                if (WalletLog != null) WalletLog.SaveLog(log);
            }
        }
    }
}
