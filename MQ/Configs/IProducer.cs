using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.MQ.Configs
{
    /// <summary>
    /// 生产者基类
    /// </summary>
    public interface IProducer
    {
        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool BasicPublish(string queueName, MessageBody message);
    }
}
