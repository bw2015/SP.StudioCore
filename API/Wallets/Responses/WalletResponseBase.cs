using Newtonsoft.Json.Linq;
using SP.StudioCore.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.API.Wallets.Responses
{
    /// <summary>
    /// 请求之后的接口返回基类
    /// </summary>
    public abstract class WalletResponseBase
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        private readonly bool? Success;

        /// <summary>
        /// 附带信息
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// 发生异常导致失败
        /// </summary>
        public WalletResponseBase()
        {
            this.Success = null;
        }

        /// <summary>
        /// 返回内容的Json赋值
        /// </summary>
        /// <param name="json"></param>
        public WalletResponseBase(string json)
        {
            JObject info = JObject.Parse(json);
            this.Success = info.Get<int>("success") == 1;
            this.Message = info.Get<string>("msg");
            if (this.Success.Value && info["info"] != null 
                && info["info"].HasValues && info["info"].Type == JTokenType.Object)
            {
                this.Construction((JObject)info["info"]);
            }
        }

        /// <summary>
        /// info返回内容的赋值操作构造
        /// </summary>
        /// <param name="info"></param>
        protected abstract void Construction(JObject info);

        public static implicit operator bool?(WalletResponseBase response)
        {
            return response.Success;
        }
    }
}
