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
        /// 错误次数
        /// </summary>
        public int ErrorCount { get; set; }
    }
}
