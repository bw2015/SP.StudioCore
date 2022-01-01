using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Gateway.Push.Telegrams.Models
{
    /// <summary>
    /// 消息内容
    /// </summary>
    public class messageResponse
    {
        /// <summary>
        /// 消息编号
        /// </summary>
        public long message_id { get; set; }

        /// <summary>
        /// 信息发送人
        /// </summary>
        public chatTargetResponse from { get; set; }

        /// <summary>
        /// 信息接收人
        /// </summary>
        public chatTargetResponse chat { get; set; }

        /// <summary>
        /// 时间（秒 时间戳）
        /// </summary>
        public long date { get; set; }

        /// <summary>
        /// 文本内容
        /// </summary>
        public string text { get; set; }

        public entitiesResponse[] entities { get; set; }

    }
}
