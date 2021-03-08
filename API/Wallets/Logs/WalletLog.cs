using SP.StudioCore.API.Wallets.Requests;
using SP.StudioCore.API.Wallets.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.API.Wallets.Logs
{
    /// <summary>
    /// 单一钱包的通信日志
    /// </summary>
    public struct WalletLog
    {
        public WalletLog(string action, bool? success, string url, string postData, string resultData, long time, object extendData = null)
        {
            this.Action = action;
            this.Success = success != null && success.Value;
            this.Url = url;
            this.PostData = postData;
            this.ResultData = resultData;
            this.CreateAt = DateTime.Now;
            this.Time = time;
            this.ExtendData = extendData;
        }

        /// <summary>
        /// 请求类型
        /// </summary>
        public string Action;

        /// <summary>
        /// 是否成功（为null表示异常）
        /// </summary>
        public bool? Success;

        /// <summary>
        /// 发送到的地址
        /// </summary>
        public string Url;

        /// <summary>
        /// 返回内容
        /// </summary>
        public string PostData;

        /// <summary>
        /// 返回内容
        /// </summary>
        public string ResultData;

        /// <summary>
        /// 提交时间
        /// </summary>
        public DateTime CreateAt;

        /// <summary>
        /// 执行时长
        /// </summary>
        public long Time;

        /// <summary>
        /// 扩展信息
        /// </summary>
        public object ExtendData;
    }
}
