using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Gateway.Push.Telegrams.Models
{
    /// <summary>
    /// 实体信息
    /// </summary>
    public class entitiesResponse
    {
        public int offset { get; set; }

        public int length { get; set; }

        /// <summary>
        /// 消息类型
        /// </summary>
        public string type { get; set; }

    }
}
