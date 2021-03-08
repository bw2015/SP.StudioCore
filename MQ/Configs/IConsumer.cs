using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.MQ.Configs
{
    /// <summary>
    /// 消费者基类
    /// </summary>
    public interface IConsumer
    {
        /// <summary>
        /// 普通消息消费
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="action"></param>
        public void BasicConsumer(string queueName, Func<MessageBody, bool> action);
    }
}
