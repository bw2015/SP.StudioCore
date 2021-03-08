using SP.StudioCore.Net;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace SP.StudioCore.Gateway.Push
{
    /// <summary>
    /// 电报机器人
    /// </summary>
    public class TelegramBot : IPush
    {
        public TelegramBot(string queryString) : base(queryString)
        {
        }

        public override object Client()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 网址
        /// </summary>
        public string Url { get; set; } = "https://api.telegram.org/";

        /// <summary>
        /// 密钥
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 频道配置
        /// </summary>
        public string Channel { get; set; }

        public override bool Send(object message, params string[] channel)
        {
            string url = $"{this.Url}bot{this.Token}/sendMessage";
            if (channel.Length == 0) channel = this.Channel.Split(',');
            foreach (string id in channel)
            {
                string data = string.Format("chat_id={0}&text={1}", id, HttpUtility.UrlEncode(message.ToString()));
                NetAgent.UploadData(url, data, Encoding.UTF8);
            }
            return true;
        }
    }
}
