using System;
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
            if (configureServicesMethod != null) configureServicesMethod.Invoke(startupIns, new object[] {services});

            // 打印日志
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<ConsumerStartup>();
            logger.LogInformation("完成初始化");

            var lst = Assembly.GetCallingAssembly().GetTypes()
                .Where(o => o.IsClass && o.GetInterfaces().Contains(typeof(IListenerMessage)));

            try
            {
                foreach (var consumer in lst)
                {
                    logger.LogInformation($"正在启动：{consumer.Name}");
                    RunConsumer(consumer);
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
        private static void RunConsumer(Type consumer)
        {
            // 没有使用consumerAttribute特性的，不启用
            var consumerAttribute = consumer.GetCustomAttribute<ConsumerAttribute>();
            if (consumerAttribute == null || !consumerAttribute.Enable) return;

            var consumerInstance = RabbitBoot.GetConsumerInstance(consumerAttribute.Name, consumerAttribute.QueueName,
                consumerAttribute.ConsumeThreadNums, consumerAttribute.LastAckTimeoutRestart);
            
            // 启用启动绑定时，要创建交换器、队列，并绑定
            if (consumerAttribute.AutoCreateAndBind)
            {
                consumerInstance.CreateExchange(consumerAttribute.ExchangeName, consumerAttribute.ExchangeType);
                consumerInstance.CreateQueueAndBind(consumerAttribute.QueueName, consumerAttribute.ExchangeName,
                    consumerAttribute.RoutingKey);
            }

            consumerInstance.Consumer.Start((IListenerMessage) Activator.CreateInstance(consumer));
        }
    }
}