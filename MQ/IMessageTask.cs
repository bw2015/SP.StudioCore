using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.MQ
{
    /// <summary>
    /// 标记一个任务ID
    /// </summary>
    public interface IMessageTask
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        Guid TaskID { get; set; }
    }
}
