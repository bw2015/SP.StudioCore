using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.MQ.Configs
{
    /// <summary>
    /// 消息体
    /// </summary>
    public struct MessageBody
    {
        /// <summary>
        /// 消息内容
        /// </summary>
        public string Content;

        public static implicit operator MessageBody(string content)
        {
            return new MessageBody
            {
                Content = content
            };
        }

        public static implicit operator string(MessageBody message)
        {
            return message.Content;
        }

        public static implicit operator MessageBody(byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }
    }
}
