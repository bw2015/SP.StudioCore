using SP.StudioCore.Model;

namespace SP.StudioCore.MQ.RabbitMQ.Configuration
{
    public class RabbitServerConfig : ISetting
    {
        public RabbitServerConfig(string config) : base(config)
        {
        }
        
        public static implicit operator RabbitServerConfig(string setting)
        {
            return setting == null ? null : new RabbitServerConfig(setting);
        }

        /// <summary> 用户名 </summary>
        public string UserName { get; set; }

        /// <summary> 密码 </summary>
        public string Password { get; set; }

        /// <summary> 集群地址 </summary>
        public string Server { get; set; }

        /// <summary> 端口 </summary>
        public int Port { get; set; }

        /// <summary> 虚拟主机 </summary>
        public string VirtualHost { get; set; }
    }
}