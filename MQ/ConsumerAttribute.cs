using System;
using SP.StudioCore.MQ.RabbitMQ;

namespace SP.StudioCore.MQ
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ConsumerAttribute : Attribute
    {
        /// <summary>
        /// 是否启用（默认为true）
        /// </summary>
        public bool Enable { get; set; } = true;

        /// <summary>
        /// Connect配置名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 交换器类型
        /// </summary>
        public ExchangeType ExchangeType { get; set; }

        /// <summary>
        /// 是否自动创建交换器、队列并绑定（默认true）
        /// </summary>
        public bool AutoCreateAndBind { get; set; } = true;

        /// <summary>
        /// AutoCreateAndBind=true时，会创建ExchangeName
        /// </summary>
        public string ExchangeName { get; set; }

        /// <summary>
        /// AutoCreateAndBind=true时，会创建QueueName，并绑定到ExchangeName
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        /// 指定接收的路由KEY（默认为空）
        /// </summary>
        public string RoutingKey { get; set; } = "";

        /// <summary> 一次读取的消息数量 </summary>
        public int ConsumeThreadNums { get; set; } = 8;

        /// <summary> 最后ACK多少秒超时则重连（默认5分钟） </summary>
        public int LastAckTimeoutRestart { get; set; } = 5 * 60;
        
        /// <summary>
        /// 死信交换器
        /// </summary>
        public string DlxExchangeName { get; set; }
        
        /// <summary>
        /// 死信路由key
        /// </summary>
        public string DlxRoutingKey { get; set; }
        
        /// <summary>
        /// 死信时间
        /// </summary>
        public int DlxTime { get; set; }

        /// <summary>
        /// 最后一个Consumer取消订阅后，Queue是否自动删除。
        /// </summary>
        public bool AutoDelete { get; set; }
    }
}