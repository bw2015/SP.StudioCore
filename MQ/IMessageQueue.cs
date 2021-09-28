using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.MQ
{
    /// <summary>
    /// 标记这是一个用于MQ的实体类
    /// </summary>
    public interface IMessageQueue
    {
        /// <summary>
        /// 消息的唯一编号（用于消息重试）
        /// </summary>
        //public Guid MessageID { get; set; }
    }
}
