using System.Collections.Generic;
using RabbitMQ.Client;
using SP.StudioCore.MQ.RabbitMQ.Configuration;
using System;

namespace SP.StudioCore.MQ.RabbitMQ
{
    /// <summary>
    ///     Rabbit管理器
    /// </summary>
    public class RabbitManager : IRabbitManager
    {
        private static readonly object ObjLock = new();

        /// <summary>
        ///     生产消息
        /// </summary>
        private IRabbitProduct? _product;
        /// <summary>
        /// 配置信息
        /// </summary>
        private readonly ProductConfig? _productConfig;
        /// <summary>
        /// Rabbit连接
        /// </summary>
        private readonly RabbitConnect? _connect;
        /// <summary>
        /// 最后ACK多少秒超时则重连（默认5分钟）
        /// </summary>
        private readonly int _lastAckTimeoutRestart;
        /// <summary>
        /// 线程数（默认8）
        /// </summary>
        private readonly int _consumeThreadNums;
        /// <summary>
        /// 队列名称
        /// </summary>
        private readonly string? _queueName;

        /// <summary> Rabbit管理器 </summary>
        public RabbitManager(RabbitConnect? connect, ProductConfig? productConfig)
        {
            _connect = connect;
            _productConfig = productConfig;
        }

        /// <summary> Rabbit管理器 </summary>
        /// <param name="connect"></param>
        /// <param name="queueName">队列名称</param>
        /// <param name="lastAckTimeoutRestart">最后ACK多少秒超时则重连（默认5分钟）</param>
        /// <param name="consumeThreadNums">线程数（默认8）</param>
        public RabbitManager(RabbitConnect connect, string queueName, int consumeThreadNums = 8, int lastAckTimeoutRestart = 5 * 60)
        {
            this._connect = connect;
            this._lastAckTimeoutRestart = lastAckTimeoutRestart;
            this._consumeThreadNums = consumeThreadNums;
            this._queueName = queueName;

            if (_lastAckTimeoutRestart == 0) _lastAckTimeoutRestart = 5 * 60;
            if (_consumeThreadNums == 0) _consumeThreadNums = 8;
        }

        /// <summary>
        /// 生产普通消息
        /// </summary>
        public IRabbitProduct Product
        {
            get
            {
                //if (_productConfig == null) throw new Exception("当前配置的是消费端的配置，你只可以使用Consumer属性");
                if (_product != null) return _product;
                lock (ObjLock)
                {
                    if (_connect == null || _productConfig == null) throw new NullReferenceException();
                    return _product ??= new RabbitProduct(_connect, _productConfig);
                }
            }
        }

        /// <summary>
        ///     消费普通消息
        /// </summary>
        public IRabbitConsumer Consumer
        {
            get
            {
                if (_connect == null) throw new NullReferenceException();
                return new RabbitConsumer(_connect, _queueName ?? string.Empty, _lastAckTimeoutRestart, _consumeThreadNums);
            }
        }

        /// <summary>
        /// 创建队列
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="durable">是否持久化（默认true）</param>
        /// <param name="exclusive">是否排他队列（默认false）</param>
        /// <param name="autoDelete">是否自动删除（默认false）</param>
        /// <param name="arguments">队列参数</param>
        public void CreateQueue(string queueName, bool durable = true, bool exclusive = false, bool autoDelete = false,
            IDictionary<string, object>? arguments = null)
        {
            if (_connect?.Connection == null || !_connect.Connection.IsOpen) _connect?.Open();
            using (IModel? channel = _connect?.Connection.CreateModel())
            {
                //声明一个队列
                channel?.QueueDeclare(
                    queue: queueName, //消息队列名称
                    durable: durable, //是否缓存
                    exclusive: exclusive, // 创建后删除
                    autoDelete: autoDelete,
                    arguments: arguments
                );
            }
        }

        /// <summary>
        /// 创建交换器
        /// </summary>
        /// <param name="exchangeName">交换器名称</param>
        /// <param name="exchangeType">交换器类型</param>
        /// <param name="durable">是否持久化（默认true）</param>
        /// <param name="autoDelete">是否自动删除（默认false）</param>
        /// <param name="arguments">参数</param>
        public void CreateExchange(string exchangeName, ExchangeType exchangeType, bool durable = true,
            bool autoDelete = false, IDictionary<string, object>? arguments = null)
        {
            if (_connect?.Connection == null || !_connect.Connection.IsOpen) _connect?.Open();
            using (IModel? channel = _connect?.Connection.CreateModel())
            {
                channel?.ExchangeDeclare(exchangeName, exchangeType.ToString(), durable, autoDelete,
                    arguments); // 声明fanout交换器
            }
        }

        /// <summary>
        /// 创建交换器
        /// </summary>
        /// <param name="durable">是否持久化（默认true）</param>
        /// <param name="autoDelete">是否自动删除（默认false）</param>
        /// <param name="arguments">参数</param>
        public void CreateExchange(bool durable = true, bool autoDelete = false, IDictionary<string, object> arguments = null)
        {
            if (_connect?.Connection == null || !_connect.Connection.IsOpen) _connect?.Open();
            using (IModel? channel = _connect?.Connection.CreateModel())
            {
                channel?.ExchangeDeclare(_productConfig?.ExchangeName, _productConfig?.ExchangeType.ToString(), durable, autoDelete,
                    arguments); // 声明fanout交换器
            }
        }

        /// <summary>
        /// 创建队列并绑定到交换器
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="exchangeName">交换器名称</param>
        /// <param name="routingKey">路由标签</param>
        /// <param name="durable">是否持久化（默认true）</param>
        /// <param name="exclusive">是否排他队列（默认false）</param>
        /// <param name="autoDelete">是否自动删除（默认false）</param>
        /// <param name="arguments">队列参数</param>
        public void CreateQueueAndBind(string queueName, string exchangeName, string routingKey, bool durable = true,
            bool exclusive = false, bool autoDelete = false, IDictionary<string, object> arguments = null)
        {
            CreateQueue(queueName, durable, exclusive, autoDelete, arguments);
            BindQueueExchange(exchangeName, queueName, routingKey);
        }

        /// <summary>
        /// 绑定队列到交换器
        /// </summary>
        /// <param name="exchangeName">交换器名称</param>
        /// <param name="queueName">队列名称</param>
        /// <param name="routingKey">路由标签</param>
        public void BindQueueExchange(string exchangeName, string queueName, string routingKey)
        {
            if (_connect?.Connection == null || !_connect.Connection.IsOpen) _connect?.Open();
            using (IModel? channel = _connect?.Connection.CreateModel())
            {
                channel?.QueueBind(queueName, exchangeName, routingKey);
            }
        }
    }
}