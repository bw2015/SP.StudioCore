using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Web.Sockets
{
    /// <summary>
    /// WebSocket 客户端
    /// </summary>
    public class WebSocketClient
    {
        /// <summary>
        /// 客户端ID
        /// </summary>
        public Guid ID { get; set; }


        /// <summary>
        /// 链接对象
        /// </summary>
        public WebSocket WebSocket { get; set; }

        /// <summary>
        /// 附加数据
        /// </summary>
        public Dictionary<string, string> Query { get; set; }
    }
}
