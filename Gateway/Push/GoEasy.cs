using Aliyun.OSS.Util;
using Newtonsoft.Json.Linq;
using SP.StudioCore.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

namespace SP.StudioCore.Gateway.Push
{
    public class GoEasy : IPush
    {
        /// <summary>
        /// Common key 既可以发送消息或也可以订阅channel来接收消息
        /// </summary>
        [Description("commonkey")]
        public string commonkey { get; set; } ;

        /// <summary>
        /// 推送地址
        /// </summary>
        [Description("rest")]
        public string rest { get; set; } = "http://rest-hangzhou.goeasy.io";

        /// <summary>
        /// 应用所在的区域地址: 【hangzhou.goeasy.io |singapore.goeasy.io】
        /// </summary>
        [Description("host")]
        public string host { get; set; } = "hangzhou.goeasy.io";

        /// <summary>
        /// 订阅密钥 只能用来订阅channel来接收消息
        /// </summary>
        [Description("appkey")]
        public string appkey { get; set; } 

        /// <summary>
        /// commonkey=xxx&rest=http://rest-hangzhou.goeasy.io&host=hangzhou.goeasy.io&appkey=xxx
        /// </summary>
        /// <param name="queryString"></param>
        public GoEasy(string queryString) : base(queryString)
        {
        }

        public override bool Send(object message, params string[] channel)
        {
            foreach (string channelName in channel)
            {
                string postDataStr = $"appkey={this.commonkey}&channel={channelName}&content={ HttpUtility.UrlEncode(message.ToString(), Encoding.UTF8) }";
                _ = NetAgent.UploadData($"{this.rest}/publish", postDataStr);
            }
            return true;
        }

        /// <summary>
        /// 获取客户端的配置信息
        /// </summary>
        /// <returns></returns>
        public override object Client()
        {
            return new
            {
                Type = PushType.GoEasy,
                this.appkey,
                this.host
            };
        }

        public override Dictionary<string, int> GetOnlineMember(params string[] channels)
        {
            string url = $"{this.rest}/herenow?appkey={commonkey}&{string.Join("&", channels.Select(t => $"channel={t}")) }";
            string result = NetAgent.DownloadData(url, Encoding.UTF8);
            JObject json = JObject.Parse(result);
            Dictionary<string, int> data = new Dictionary<string, int>();
            var content = json["content"]["channels"];
            foreach (var item in content)
            {
                string channel = item["channel"].Value<string>();
                int count = item["clientAmount"].Value<int>();
                data.Add(channel, count);
            }
            return data;
        }
    }
}
