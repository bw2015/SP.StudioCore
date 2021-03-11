using System.Collections.Generic;
using SP.StudioCore.Configuration;
using SP.StudioCore.MQ.RabbitMQ.Configuration;

namespace SP.StudioCore.MQ.RabbitMQ
{
    /// <summary>
    /// 模拟IOC的别名单例
    /// </summary>
    public static class RabbitBoot
    {
        private static readonly Dictionary<string, IRabbitManager> Items = new();
        private static readonly object objLock = new();

        /// <summary>
        /// 获取生产者
        /// </summary>
        /// <param name="productConfigName">生产者配置名称</param>
        /// <param name="connectionConfigName">rabbit服务器配置名称</param>
        /// <returns></returns>
        public static IRabbitManager GetProductInstance(string productConfigName,
            string connectionConfigName = "Connection")
        {
            var keyName = "Product." + productConfigName;
            if (!Items.ContainsKey(keyName))
            {
                lock (objLock)
                {
                    var configurationDefault = new ConfigurationDefault();
                    RabbitConnect rabbitConnect = configurationDefault["Rabbit:" + connectionConfigName];
                    string config = configurationDefault["Rabbit:Product:" + productConfigName] ??
                        $"ExchangeName={productConfigName}&RoutingKey=&UseConfirmModel=true";

                    ProductConfig productConfig = config;

                    Items.TryAdd(keyName, new RabbitManager(rabbitConnect, productConfig));
                }
            }
            return Items[keyName];
        }

        /// <summary>
        /// 获取消费者
        /// </summary>
        /// <param name="connectionConfigName">rabbit服务器配置名称</param>
        /// <param name="queueName">队列名称</param>
        /// <param name="lastAckTimeoutRestart">最后ACK多少秒超时则重连（默认5分钟）</param>
        /// <param name="consumeThreadNums">线程数（默认8）</param>
        public static IRabbitManager GetConsumerInstance(string connectionConfigName, string queueName, int consumeThreadNums, int lastAckTimeoutRestart)
        {
            var configurationDefault = new ConfigurationDefault();
            RabbitConnect rabbitConnect = configurationDefault["Rabbit:" + connectionConfigName];

            return new RabbitManager(rabbitConnect, queueName, consumeThreadNums, lastAckTimeoutRestart);
        }
    }
}