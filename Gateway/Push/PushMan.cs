using PusherServer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Gateway.Push
{
    /// <summary>
    /// pusher.com
    /// </summary>
    public class PushMan : IPush
    {
        [Description("应用ID")]
        public string app_id { get; set; } = "";

        [Description("订阅密钥")]
        public string key { get; set; } = "";

        [Description("发布密钥")]
        public string secret { get; set; } = "";

        [Description("节点")]
        public string cluster { get; set; } = "ap3";

        public PushMan(string queryString) : base(queryString)
        {
        }

        /// <summary>
        /// 发送至单个频道
        /// </summary>
        public override bool Send(object message, params string[] channels)
        {
            if (message == null || channels.Length == 0) return false;
            PusherOptions options = new PusherOptions
            {
                Cluster = this.cluster,
                Encrypted = true
            };
            Pusher pusher = new Pusher(this.app_id, this.key, this.secret, options);
            string eventname = "*";
            bool success = true;
            Task<ITriggerResult> result = pusher.TriggerAsync(channels, eventname, message);
            if (result.Result.StatusCode != System.Net.HttpStatusCode.OK) success = false;
            return success;
        }

        public override object Client()
        {
            return new
            {
                Type = PushType.PushMan,
                this.key,
                this.app_id,
                this.cluster
            };
        }

    }
}

