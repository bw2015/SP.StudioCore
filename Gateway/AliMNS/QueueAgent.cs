using System;
using System.Collections.Generic;
using System.Text;
using SP.StudioCore.Gateway.AliMNS.Interface;

namespace SP.StudioCore.Gateway.AliMNS
{
    /// <summary>
    /// 队列信息实现类
    /// </summary>
    public class QueueAgent : IMessageMns
    {

        public QueueAgent(string topicName)
        {

        }
        /// <summary>
        /// 发送信息
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public string Send(string body)
        {
            throw new NotImplementedException();
        }
    }
}
