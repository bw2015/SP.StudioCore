using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using System;

namespace SP.StudioCore.MQ.RabbitMQ
{
    /// <summary>
    /// Rabbit监听消费
    /// </summary>
    public interface IListenerMessage
    {
        /// <summary>
        /// 消费
        /// </summary>
        /// <returns>当开启手动确认时，返回true时，才会进行ACK确认</returns>
        void Consumer(string message, object sender, BasicDeliverEventArgs ea);

        /// <summary>
        /// 当异常时处理
        /// </summary>
        /// <returns>true：表示成功处理，移除消息。false：处理失败，如果是重试状态，则放回队列</returns>
        bool FailureHandling(string message, object sender, BasicDeliverEventArgs ea);
    }


    public abstract class IListenerMessage<TModel> : IListenerMessage where TModel : IMessageQueue
    {
        public void Consumer(string message, object sender, BasicDeliverEventArgs ea)
        {
            Console.WriteLine($"{this.GetType()} => {message}");
            if (string.IsNullOrEmpty(message)) return;
            TModel? model = JsonConvert.DeserializeObject<TModel>(message);
            if (model == null) return;
            this.Consumer(model, sender, ea);
        }

        public abstract void Consumer(TModel model, object sender, BasicDeliverEventArgs ea);

        public abstract bool FailureHandling(string message, object sender, BasicDeliverEventArgs ea);
    }
}