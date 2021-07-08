using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SP.StudioCore.Ioc;
using SP.StudioCore.MQ.RabbitMQ.Configuration;

namespace SP.StudioCore.MQ.RabbitMQ
{
    public class RabbitProduct : IRabbitProduct
    {
        private ConcurrentQueue<IModel> Stacks = new();
        /// <summary>
        /// 配置信息
        /// </summary>
        private readonly ProductConfig _productConfig;

        /// <summary>
        ///     创建消息队列属性
        /// </summary>
        private readonly RabbitConnect _connect;

        private static readonly object objLock = new();
        
        public RabbitProduct(RabbitConnect connect, ProductConfig productConfig)
        {
            if (productConfig.MinChannelPool == 0) productConfig.MinChannelPool = 10;
            _connect       = connect;
            _productConfig = productConfig;

            _connect.Open();
            // 启动后，后台立即创建10个频道
            Task.Factory.StartNew(() => 
            {
                for (int i = 0; i < productConfig.MinChannelPool; i++)
                {
                    var channel = _connect.Connection.CreateModel();
                    if (_productConfig.UseConfirmModel) channel.ConfirmSelect();
                    Stacks.Enqueue(channel);
                }
            });
        }

        
        /// <summary>
        ///     开启生产消息
        /// </summary>
        private IModel CreateChannel()
        {
            // 如果连接断开，则要重连
            if (_connect.Connection == null || !_connect.Connection.IsOpen) _connect.Open();

            lock (objLock)
            {
                // 从池中取出频道
                var tryPop = Stacks.TryDequeue(out var channel);
            
                // 取出失败，说明没有可用频道，需要创建新的
                if (tryPop && channel is {IsClosed: false}) return channel;
                
                channel = _connect.Connection.CreateModel();
                if (_productConfig.UseConfirmModel) channel.ConfirmSelect();
                return channel;
            }
        }

        /// <summary>
        ///     关闭生产者
        /// </summary>
        public void Close(IModel channel)
        {
            Stacks.Enqueue(channel);
        }

        /// <summary>
        ///     发送消息（Routingkey默认配置中的RoutingKey；ExchangeName默认配置中的ExchangeName）
        /// </summary>
        /// <param name="message">消息主体</param>
        /// <param name="funcBasicProperties">属性</param>
        public bool Send(string message, Action<IBasicProperties> funcBasicProperties = null)
        {
            return Send(message, _productConfig.RoutingKey, _productConfig.ExchangeName, funcBasicProperties);
        }

        /// <summary>
        ///     发送消息（Routingkey默认配置中的RoutingKey；ExchangeName默认配置中的ExchangeName）
        /// </summary>
        /// <param name="message">消息主体</param>
        /// <param name="funcBasicProperties">属性</param>
        public bool Send(IEnumerable<string> message, Action<IBasicProperties> funcBasicProperties = null)
        {
            return Send(message, _productConfig.RoutingKey, _productConfig.ExchangeName, funcBasicProperties);
        }

        /// <summary>
        ///     发送消息
        /// </summary>
        /// <param name="message">消息主体</param>
        /// <param name="routingKey">路由KEY名称</param>
        /// <param name="exchange">交换器名称</param>
        /// <param name="funcBasicProperties">属性</param>
        public bool Send(string message, string routingKey, string exchange = "", Action<IBasicProperties> funcBasicProperties = null)
        {
            IModel channel = null;
            try
            {
                var sw2 = Stopwatch.StartNew();
                channel = CreateChannel();
                if (sw2.ElapsedMilliseconds > 10) Console.WriteLine($"CreateChannel耗时：{sw2.ElapsedMilliseconds}");
                sw2.Restart();
                
                var basicProperties = channel.CreateBasicProperties();
                // 默认设置为消息持久化
                if (funcBasicProperties != null) funcBasicProperties(basicProperties);
                else basicProperties.DeliveryMode = 2;
                if (sw2.ElapsedMilliseconds > 10) Console.WriteLine($"CreateBasicProperties耗时：{sw2.ElapsedMilliseconds}");
                sw2.Restart();
                
                //消息内容
                var body = Encoding.UTF8.GetBytes(message);
                //发送消息
                channel.BasicPublish(exchange: exchange, routingKey: routingKey, basicProperties: basicProperties, body: body);
                var result =  !_productConfig.UseConfirmModel || channel.WaitForConfirms();
                
                if (sw2.ElapsedMilliseconds > 10) Console.WriteLine($"BasicPublish耗时：{sw2.ElapsedMilliseconds}");
                return result;
            }
            catch (Exception e)
            {
                IocCollection.GetService<ILoggerFactory>().CreateLogger<RabbitProduct>().LogError(e.ToString(), e);
                return false;
            }
            finally
            {
                Close(channel);
            }
        }

        /// <summary>
        ///     发送消息（批量）
        /// </summary>
        /// <param name="message">消息主体</param>
        /// <param name="routingKey">路由KEY名称</param>
        /// <param name="exchange">交换器名称</param>
        /// <param name="funcBasicProperties">属性</param>
        public bool Send(IEnumerable<string> message, string routingKey, string exchange = "", Action<IBasicProperties> funcBasicProperties = null)
        {
            IModel channel = null;
            try
            {
                channel = CreateChannel();

                var basicProperties = channel.CreateBasicProperties();
                // 默认设置为消息持久化
                if (funcBasicProperties != null) funcBasicProperties(basicProperties);
                else basicProperties.DeliveryMode = 2;

                foreach (var msg in message)
                {
                    //消息内容
                    var body = Encoding.UTF8.GetBytes(msg);
                    //发送消息
                    channel.BasicPublish(exchange: exchange, routingKey: routingKey, basicProperties: basicProperties, body: body);
                }

                return !_productConfig.UseConfirmModel || channel.WaitForConfirms();
            }
            catch (Exception e)
            {
                IocCollection.GetService<ILoggerFactory>().CreateLogger<RabbitProduct>().LogError(e.ToString(), e);
                return false;
            }
            finally
            {
                Close(channel);
            }
        }
    }
}