﻿using Newtonsoft.Json.Linq;
using SP.StudioCore.Json;
using System;
using Nest;

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
        public bool Success;

        /// <summary>
        /// 异常信息
        /// </summary>
        public Exception Ex { get; }

        /// <summary>
        /// 附带信息
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// http响应报文
        /// </summary>
        public string ResponseBody { get; }

        /// <summary>
        /// 执行耗时
        /// </summary>
        public int Duration { get; }

        /// <summary>
        /// 扩展数据
        /// 目的1：记录日志的时候可以标记用户ID、商户ID等本实体类没有的信息
        /// </summary>
        [Ignore] public object ExtendData { get; set; }

        /// <summary>
        /// 发生异常导致失败
        /// </summary>
        protected WalletResponseBase(int duration, Exception ex)
        {
            this.Success  = false;
            this.Ex       = ex;
            this.Duration = duration;
            this.Message       = ex.Message;
        }

        /// <summary>
        /// 返回内容的Json赋值
        /// </summary>
        protected WalletResponseBase(string json, int duration)
        {
            this.Duration     = duration;
            this.ResponseBody = json;
            this.Ex           = null;
            JObject info = JObject.Parse(json);
            this.Success = info.Get<int>("success") == 1;
            this.Message = info.Get<string>("msg");
            if (this.Success && info["info"] != null && info["info"].HasValues && info["info"].Type == JTokenType.Object)
            {
                this.Construction((JObject) info["info"]);
            }
        }

        /// <summary>
        /// info返回内容的赋值操作构造
        /// </summary>
        protected abstract void Construction(JObject info);

        public static implicit operator bool?(WalletResponseBase response)
        {
            return response.Success;
        }
    }
}