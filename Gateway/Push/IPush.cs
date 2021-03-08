using SP.StudioCore.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace SP.StudioCore.Gateway.Push
{
    /// <summary>
    /// 消息实时推送
    /// </summary>
    public abstract class IPush : ISetting
    {
        public IPush(string queryString) : base(queryString)
        {
        }

        /// <summary>
        /// 推送信息
        /// </summary>
        /// <param name="channel">频道</param>
        /// <param name="message">要推送的信息（发送到多个频道）</param>
        /// <returns></returns>
        public abstract bool Send(object message, params string[] channel);


        /// <summary>
        /// 客户端需要的订阅信息
        /// </summary>
        /// <returns></returns>
        public abstract object Client();

        /// <summary>
        /// 获取频道的在线人数
        /// </summary>
        /// <param name="channels"></param>
        /// <returns></returns>
        public virtual Dictionary<string, int> GetOnlineMember(params string[] channels)
        {
            return channels.ToDictionary(t => t, t => 0);
        }
    }
}
