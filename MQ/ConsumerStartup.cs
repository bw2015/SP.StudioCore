using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SP.StudioCore.MQ.RabbitMQ;

namespace SP.StudioCore.MQ
{
    public class ConsumerStartup
    {
        public static void Run<TStartup>() where TStartup : class
        {
            IServiceCollection services = new ServiceCollection();
            // 为了实现调用自定义的启动类，并执行ConfigureServices方法，这里采用Invoke的方式实现
            var startupIns = Activator.CreateInstance<TStartup>();
            var configureServicesMethod = typeof(TStartup).GetMethod("ConfigureServices");
            if (configureServicesMethod != null) configureServicesMethod.Invoke(startupIns, new object[] { services });

            // 打印日志
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            ILogger<ConsumerStartup> logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<ConsumerStartup>();

            var lst = Assembly.GetCallingAssembly().GetTypes()
                .Where(o => o.IsClass && o.GetInterfaces().Contains(typeof(IListenerMessage)));

            try
            {
                foreach (var consumer in lst)
                {
                    RunConsumer(consumer, logger);
                }

                logger.LogInformation("全部消费启动完成!");
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
            }
        }

        /// <summary>
        /// 启动消费程序
        /// </summary>
        /// <param name="consumer"></param>
        public static void RunConsumer(Type consumer, ILogger<ConsumerStartup> logger)
        {
            // 没有使用consumerAttribute特性的，不启用
            var consumerAttribute = consumer.GetCustomAttribute<ConsumerAttribute>();
            if (consumerAttribute == null || !consumerAttribute.Enable) return;

            var consumerInstance = RabbitBoot.GetConsumerInstance(consumerAttribute.Name, consumerAttribute.QueueName,
                consumerAttribute.ConsumeThreadNums, consumerAttribute.LastAckTimeoutRestart);

            logger?.LogInformation($"正在初始化：{consumer.Name}");
            // 启用启动绑定时，要创建交换器、队列，并绑定
            if (consumerAttribute.AutoCreateAndBind)
            {
                // 配置死信参数
                var arguments = new Dictionary<string, object>();
                if (!string.IsNullOrWhiteSpace(consumerAttribute.DlxExchangeName)) arguments["x-dead-letter-exchange"] = consumerAttribute.DlxExchangeName;
                if (!string.IsNullOrWhiteSpace(consumerAttribute.DlxRoutingKey)) arguments["x-dead-letter-routing-key"] = consumerAttribute.DlxRoutingKey;
                if (consumerAttribute.DlxTime > 0) arguments["x-message-ttl"] = consumerAttribute.DlxTime;
                consumerInstance.CreateExchange(consumerAttribute.ExchangeName, consumerAttribute.ExchangeType);
                consumerInstance.CreateQueueAndBind(consumerAttribute.QueueName, consumerAttribute.ExchangeName, consumerAttribute.RoutingKey, arguments: arguments);
            }

            logger?.LogInformation($"正在启动：{consumer.Name}");
            consumerInstance.Consumer.Start((IListenerMessage)Activator.CreateInstance(consumer));
        }
    }
}