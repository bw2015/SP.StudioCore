using SP.StudioCore.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.MQ.Configs
{
    /// <summary>
    /// MQ的连接信息
    /// </summary>
    public sealed class MQConfig : ISetting
    {
        public MQConfig(string queryString) : base(queryString)
        {
        }

        /// <summary>
        /// 访问消息队列的用户名
        /// </summary>
        public string uid { get; set; }

        /// <summary>
        /// 访问消息队列的密码
        /// </summary>
        public string pwd { get; set; }

        /// <summary>
        /// 消息队列的主机地址
        /// </summary>
        public string server { get; set; }

        /// <summary>
        /// 消息队列的主机开放端口
        /// </summary>
        public int port { get; set; }

        public static implicit operator MQConfig(string config)
        {
            return new MQConfig(config);
        }
    }
}
