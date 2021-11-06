using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SP.StudioCore.Configuration;
using SP.StudioCore.MQ.RabbitMQ;

namespace SP.StudioCore.MQ
{
    public class ConsumerStartup
    {
        /// <summary>
        /// 执行消费队列
        /// </summary>
        /// <typeparam name="TStartup"></typeparam>
        /// <param name="consumers">指定要执行的消费队列/为空则是全部</param>
        public static void Run<TStartup>(params string[] consumers) where TStartup : class
        {
            IServiceCollection services = new ServiceCollection();
            // 为了实现调用自定义的启动类，并执行ConfigureServices方法，这里采用Invoke的方式实现
            var startupIns              = Activator.CreateInstance<TStartup>();
            var configureServicesMethod = typeof(TStartup).GetMethod("ConfigureServices");
            if (configureServicesMethod != null) configureServicesMethod.Invoke(startupIns, new object[] { services });

            // 打印日志
            IServiceProvider         serviceProvider = services.BuildServiceProvider();
            ILogger<ConsumerStartup> logger          = serviceProvider.GetService<ILoggerFactory>().CreateLogger<ConsumerStartup>();

            var lst = Assembly.GetCallingAssembly().GetTypes()
                              .Where(o => o.IsClass && o.GetInterfaces().Contains(typeof(IListenerMessage)));

            try
            {
                foreach (var consumer in lst)
                {
                    if (consumers != null && consumers.Any())
                    {
                        if (!consumers.Contains(consumer.Name)) continue;
                    }
                    string log = string.Join('\t', RunConsumer(consumer, logger));
                    Console.WriteLine(log);
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
        public static List<string> RunConsumer(Type consumer, ILogger<ConsumerStartup> logger)
        {
            List<string> logs = new List<string>();
            Stopwatch    sw   = Stopwatch.StartNew();

            logs.Add(consumer.Name);
            // 没有使用consumerAttribute特性的，不启用
            var consumerAttribute = consumer.GetCustomAttribute<ConsumerAttribute>();
            if (consumerAttribute == null || !consumerAttribute.Enable) return logs;

            // 不指定消费者名字，则随机创建一个
            if (string.IsNullOrEmpty(consumerAttribute.QueueName))
            {
                consumerAttribute.QueueName  = $"{consumerAttribute.ExchangeName}-{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                consumerAttribute.AutoDelete = true;
            }
            var consumerInstance = RabbitBoot.GetConsumerInstance(consumerAttribute.Name, consumerAttribute.QueueName,
                                                                  consumerAttribute.ConsumeThreadNums, consumerAttribute.AutoDelete ? -1 : consumerAttribute.LastAckTimeoutRestart);

            logs.Add($"初始化{sw.ElapsedMilliseconds}ms");
            sw.Restart();

            // 启用启动绑定时，要创建交换器、队列，并绑定
            if (consumerAttribute.AutoCreateAndBind)
            {
                // 配置死信参数
                var arguments                                                                                           = new Dictionary<string, object>();
                if (!string.IsNullOrWhiteSpace(consumerAttribute.DlxExchangeName)) arguments["x-dead-letter-exchange"]  = consumerAttribute.DlxExchangeName;
                if (!string.IsNullOrWhiteSpace(consumerAttribute.DlxRoutingKey)) arguments["x-dead-letter-routing-key"] = consumerAttribute.DlxRoutingKey;
                if (consumerAttribute.DlxTime > 0) arguments["x-message-ttl"]                                           = consumerAttribute.DlxTime;
                consumerInstance.CreateExchange(consumerAttribute.ExchangeName, consumerAttribute.ExchangeType);
                consumerInstance.CreateQueueAndBind(consumerAttribute.QueueName, consumerAttribute.ExchangeName, consumerAttribute.RoutingKey,
                                                    arguments: arguments, autoDelete: consumerAttribute.AutoDelete);
            }

            consumerInstance.Consumer.Start((IListenerMessage)Activator.CreateInstance(consumer));
            logs.Add($"启动{sw.ElapsedMilliseconds}ms");

            return logs;
        }
    }
}