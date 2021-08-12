using System.Net;
using System.Text.Encodings.Web;
using System.Web;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SP.StudioCore.Ioc;
using SP.StudioCore.MQ.RabbitMQ.Configuration;
using SP.StudioCore.Web;

namespace SP.StudioCore.MQ.RabbitMQ
{
    /// <summary>
    /// Rabbit连接
    /// </summary>
    public class RabbitConnect
    {
        private RabbitServerConfig _config;

        /// <summary>
        ///     创建消息队列属性
        /// </summary>
        private readonly IConnectionFactory _factoryInfo;

        /// <summary>
        /// Rabbit连接
        /// </summary>
        public IConnection Connection { get; private set; }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="config">配置</param>
        public RabbitConnect(RabbitServerConfig config)
        {
            //ILogger<RabbitConnect>? logger = IocCollection.GetService<ILoggerFactory>()?.CreateLogger<RabbitConnect>();
            if (string.IsNullOrWhiteSpace(config.VirtualHost))
            {
                config.VirtualHost = "/";
            }

            _config = config;
            _factoryInfo = new ConnectionFactory //创建连接工厂对象
            {
                HostName                 = HttpUtility.UrlDecode(config.Server),      //IP地址
                Port                     = config.Port,                               //端口号
                UserName                 = HttpUtility.UrlDecode(config.UserName),    //用户账号
                Password                 = HttpUtility.UrlDecode(config.Password),    //用户密码
                VirtualHost              = HttpUtility.UrlDecode(config.VirtualHost), // 虚拟主机
                AutomaticRecoveryEnabled = true,
            };
            //logger?.LogInformation($"UserName：{_factoryInfo.UserName},Password：{_factoryInfo.Password}，VirtualHost：{_factoryInfo.VirtualHost}");
        }

        public static implicit operator RabbitConnect(string config)
        {
            return config == null ? null : new RabbitConnect(new RabbitServerConfig(config));
        }

        /// <summary>
        ///     开启生产消息
        /// </summary>
        public void Open()
        {
            var hostName = Dns.GetHostName();
            Connection = _factoryInfo.CreateConnection($"{hostName}/{_config.UserName}");
        }
    }
}