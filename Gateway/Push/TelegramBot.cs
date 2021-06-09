using SP.StudioCore.Model;
using SP.StudioCore.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;

namespace SP.StudioCore.Gateway.Push
{
    /// <summary>
    /// 电报机器人
    /// </summary>
    public class TelegramBot : ISetting
    {
        public TelegramBot(string queryString) : base(queryString)
        {
        }

        /// <summary>
        /// 网址
        /// </summary>
        [Description("网关")]
        public string Url { get; set; } = "https://api.telegram.org/";

        /// <summary>
        /// 密钥
        /// </summary>
        [Description("密钥")]
        public string Token { get; set; }

        /// <summary>
        /// 频道配置
        /// </summary>
        [Description("频道")]
        public string Channel { get; set; }

        public bool SendPhoto(string photo, params string[] channel)
        {
            string url = $"{this.Url}bot{this.Token}/sendPhoto";
            foreach (string id in this.GetChannels(channel))
            {
                string data = $"chat_id={id}&photo={HttpUtility.UrlEncode(photo)}";
                NetAgent.UploadData(url, data, Encoding.UTF8);
            }
            return true;
        }

        /// <summary>
        /// 发送文本信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        public bool Send(string message, params string[] channel)
        {
            string url = $"{this.Url}bot{this.Token}/sendMessage";
            foreach (string id in this.GetChannels(channel))
            {
                string data = $"chat_id={id}&text={HttpUtility.UrlEncode(message)}&parse_mode=HTML";
                NetAgent.UploadData(url, data, Encoding.UTF8);
            }
            return true;
        }

        /// <summary>
        /// 获取频道
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        private IEnumerable<string> GetChannels(string[] channel)
        {
            if (channel == null || !channel.Any()) return this.Channel.Split(',');
            return channel;
        }
    }
}
