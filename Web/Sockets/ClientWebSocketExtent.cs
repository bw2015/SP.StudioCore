using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SP.StudioCore.Web.Sockets
{
    /// <summary>
    /// ClientWebSocket 的扩展配置信息
    /// </summary>
    public static class ClientWebSocketExtent
    {
        /// <summary>
        /// 发送文本信息
        /// </summary>
        /// <param name="ws"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Task SendAsync(this ClientWebSocket ws, string message)
        {
            ArraySegment<byte> data = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
            return ws.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
