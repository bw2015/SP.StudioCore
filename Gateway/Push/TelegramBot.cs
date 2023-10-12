using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SP.StudioCore.Gateway.Push.Telegrams;
using SP.StudioCore.Json;
using SP.StudioCore.Model;
using SP.StudioCore.Net;
using SP.StudioCore.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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
        /// 读取信息
        /// </summary>
        public IEnumerable<getUpdatesResponse>? getUpdates(long offset = 0)
        {
            string url = $"{this.Url}bot{this.Token}/getUpdates?offset={offset}";
            string content = NetAgent.DownloadData(url, Encoding.UTF8);
            TelegramResponse<getUpdatesResponse[]>? response = JsonConvert.DeserializeObject<TelegramResponse<getUpdatesResponse[]>>(content);
            if (response == null || !response) return null;
            return (getUpdatesResponse[])response;
        }

        /// <summary>
        /// 密钥
        /// </summary>
        [Description("密钥")]
        public string? Token { get; set; }

        /// <summary>
        /// 默认频道配置
        /// </summary>
        [Description("频道")]
        public string? Channel { get; set; }

        public bool sendPhoto(string? photo, params string[] channel)
        {
            if (photo == null) return false;
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
        public bool sendMessage(string? message, params string[] channel)
        {
            if (message == null) return false;
            string url = $"{this.Url}bot{this.Token}/sendMessage";
            bool isSend = false;
            foreach (string id in this.GetChannels(channel))
            {
                string data = $"chat_id={id}&text={HttpUtility.UrlEncode(message)}&parse_mode=HTML";
                string result = NetAgent.UploadData(url, data, Encoding.UTF8);
                try
                {
                    JObject info = JObject.Parse(result);
                    isSend = info["ok"]?.Value<bool>() ?? false;
                    if (!isSend)
                    {
                        ConsoleHelper.WriteLine(result, ConsoleColor.Red);
                    }
                }
                catch (Exception ex)
                {
                    ConsoleHelper.WriteLine($"{ex.Message} - {result}", ConsoleColor.Red);
                    isSend = false;
                }
            }
            return isSend;
        }


        /// <summary>
        /// 发送文本信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        public void SendMessageAsync(string message, params string[] channel)
        {
            string url = $"{this.Url}bot{this.Token}/sendMessage";
            var lstTask = new List<Task>();
            foreach (string id in this.GetChannels(channel))
            {
                string data = $"chat_id={id}&text={HttpUtility.UrlEncode(message)}&parse_mode=HTML";
                lstTask.Add(HttpAgent.PostAsync(url, data, Encoding.UTF8));
            }
            Task.WaitAny(lstTask.ToArray());
        }

        /// <summary>
        /// 引用回复信息
        /// </summary>
        /// <returns></returns>
        public bool replyMessage(long replyId, string message, params string[] channel)
        {
            string url = $"{this.Url}bot{this.Token}/sendMessage";
            List<Task> task = new();
            foreach (string id in this.GetChannels(channel))
            {
                string data = string.Format("chat_id={0}&text={1}&reply_to_message_id={2}&parse_mode=HTML", id, HttpUtility.UrlEncode(message.ToString()), replyId);
                task.Add(Task.Run(() => { NetAgent.UploadData(url, data, Encoding.UTF8); }));
            }
            Task.WaitAll(task.ToArray());
            return true;
        }

        /// <summary>
        /// 获取频道
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        private IEnumerable<string> GetChannels(string[] channel)
        {
            if (channel == null || !channel.Any()) return this.Channel?.Split(',') ?? Enumerable.Empty<string>();
            return channel;
        }
    }
}
