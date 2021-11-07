using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using SP.StudioCore.Ioc;
using SP.StudioCore.Log;
using SP.StudioCore.MQ.RabbitMQ.Configuration;


namespace SP.StudioCore.MQ.RabbitMQ
{
    public class RabbitConsumer : IRabbitConsumer
    {
        /// <summary>
        /// 创建消息队列属性
        /// </summary>
        private readonly RabbitConnect _connect;

        /// <summary>
        /// 创建连接会话对象
        /// </summary>
        private IModel _channel;

        /// <summary>
        /// 针对后台定时检查状态的取消令牌
        /// </summary>
        private CancellationTokenSource _cts;

        /// <summary>
        /// 最后一次ACK确认时间
        /// </summary>
        private DateTime _lastAckAt;

        /// <summary>
        /// 消费监听
        /// </summary>
        private IListenerMessage _listener;

        /// <summary>
        /// 是否自动ack
        /// </summary>
        private bool _autoAck;

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
        private readonly string _queueName;

        /// <summary>
        /// 消费客户端
        /// </summary>
        /// <param name="connect"></param>
        /// <param name="queueName">队列名称</param>
        /// <param name="lastAckTimeoutRestart">最后ACK多少秒超时则重连（默认5分钟）</param>
        /// <param name="consumeThreadNums">线程数（默认8）</param>
        public RabbitConsumer(RabbitConnect connect, string queueName, int lastAckTimeoutRestart, int consumeThreadNums)
        {
            this._connect               = connect;
            this._lastAckTimeoutRestart = lastAckTimeoutRestart;
            this._consumeThreadNums     = consumeThreadNums;
            this._queueName             = queueName;
            this._lastAckAt             = DateTime.Now;

            if (_lastAckTimeoutRestart == 0) _lastAckTimeoutRestart = 5 * 60;
            if (_consumeThreadNums     == 0) _consumeThreadNums     = 8;
        }

        /// <summary>
        /// 监控消费
        /// </summary>
        /// <param name="listener">消费事件</param>
        /// <param name="autoAck">是否自动确认，默认false</param>
        public void Start(IListenerMessage listener, bool autoAck = false)
        {
            _listener = listener;
            _autoAck  = autoAck;
            Connect(_listener, _autoAck);
            CheckStatsAndConnect();
        }

        /// <summary>
        /// 重启
        /// </summary>
        private void ReStart()
        {
            Close();
            Connect(_listener, _autoAck);
        }

        /// <summary>
        /// 定时检查连接状态
        /// </summary>
        private void CheckStatsAndConnect()
        {
            // 大于0，才开起自动重连
            if (this._lastAckTimeoutRestart < 1) return;

            // 检查连接状态
            _cts = new CancellationTokenSource();
            Task.Factory.StartNew(token =>
            {
                var cancellationToken = (CancellationToken)token;
                try
                {
                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        // 未打开、关闭状态、上一次ACK超时，则重启
                        if (_channel == null || _channel.IsClosed)
                        {
                            IocCollection.GetService<ILoggerFactory>().CreateLogger(this.GetType()).LogWarning($"发现Rabbit未连接，或已关闭，开始重新连接");
                            ReStart();
                        }
                        else if ((DateTime.Now - _lastAckAt).TotalSeconds >= _lastAckTimeoutRestart)
                        {
                            IocCollection.GetService<ILoggerFactory>().CreateLogger(this.GetType()).LogWarning($"rabbit距上一次消费过去了{(DateTime.Now - _lastAckAt).TotalSeconds}秒后没有新的消息，尝试重新连接Rabbit。");
                            ReStart();
                        }

                        Thread.Sleep(3000);
                    }
                }
                catch (Exception e)
                {
                    IocCollection.GetService<ILoggerFactory>().CreateLogger(this.GetType()).LogWarning(e.Message);
                }
            }, _cts.Token);
        }

        /// <summary>
        /// 监控消费（只消费一次）
        /// </summary>
        /// <param name="listener">消费事件</param>
        /// <param name="autoAck">是否自动确认，默认false</param>
        public void StartSignle(IListenerMessageSingle listener, bool autoAck = false)
        {
            Connect();

            // 只获取一次
            var resp = _channel.BasicGet(_queueName, autoAck);

            var result  = false;
            var message = Encoding.UTF8.GetString(resp.Body.ToArray());
            try
            {
                result = listener.Consumer(message, resp);
                if (!result)
                {
                    result = listener.FailureHandling(message, resp);
                }
            }
            catch (Exception e)
            {
                IocCollection.GetService<ILoggerFactory>().CreateLogger(listener.GetType()).LogError(e, e.Message);
                // 消费失败后处理
                try
                {
                    result = listener.FailureHandling(message, resp);
                }
                catch (Exception exception)
                {
                    IocCollection.GetService<ILoggerFactory>()
                                 .CreateLogger(listener.GetType())
                                 .LogError(exception, "失败处理出现异常：" + listener.GetType().FullName);
                    result = false;
                }
            }
            finally
            {
                if (!autoAck)
                {
                    if (result)
                    {
                        _channel.BasicAck(resp.DeliveryTag, false);
                    }
                    else
                    {
                        _channel.BasicReject(resp.DeliveryTag, true);
                    }
                }

                Close();
            }
        }

        /// <summary>
        /// 单次消费连接MQ
        /// </summary>
        private void Connect()
        {
            if (_connect.Connection == null || !_connect.Connection.IsOpen) _connect.Open();
            if (_channel            == null || _channel.IsClosed) _channel = _connect.Connection.CreateModel();
            _lastAckAt = DateTime.Now;
        }

        /// <summary>
        /// 持续消费，并检查连接状态并自动恢复
        /// </summary>
        private void Connect(IListenerMessage listener, bool autoAck = false)
        {
            Connect();

            _channel.BasicQos(0, (ushort)_consumeThreadNums, false);
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                bool    result  = true;
                string? message = Encoding.UTF8.GetString(ea.Body.ToArray());
                try
                {
                    listener.Consumer(message, model, ea);
                    _lastAckAt = DateTime.Now;
                }
                catch (AlreadyClosedException e) // rabbit被关闭了，重新打开链接
                {
                    ReStart();
                    IocCollection.GetService<ILoggerFactory>()
                                 .CreateLogger(listener.GetType())
                                 .LogError(e, e.ToString());
                }
                catch (Exception e)
                {
                    // 全局异常处理
                    IocCollection.GetService<IGlobalException>()?.Handle(e);
                    // 消费失败后处理
                    IocCollection.GetService<ILoggerFactory>()
                                 .CreateLogger(listener.GetType())
                                 .LogError(e, e.ToString());
                    try
                    {
                        result = listener.FailureHandling(message, model, ea);
                    }
                    catch (Exception exception)
                    {
                        IocCollection.GetService<ILoggerFactory>()
                                     .CreateLogger(listener.GetType())
                                     .LogError(exception, "失败处理出现异常：" + listener.GetType().FullName);
                        result = false;
                    }
                }
                finally
                {
                    if (!autoAck)
                    {
                        if (result)
                        {
                            _channel.BasicAck(ea.DeliveryTag, false);
                        }
                        else
                        {
                            _channel.BasicReject(ea.DeliveryTag, true);
                        }
                    }
                }
            };
            // 消费者开启监听
            _channel.BasicConsume(queue: _queueName, autoAck: autoAck, consumer: consumer);
        }

        /// <summary>
        ///     关闭生产者
        /// </summary>
        public void Close()
        {
            _cts?.Cancel();

            if (_channel != null)
            {
                _channel.Close();
                _channel.Dispose();
                _channel = null;
            }
        }
    }
}