using Microsoft.AspNetCore.Http;
using SP.StudioCore.Utils;
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
    /// WebSocket 客户端
    /// </summary>
    public class WebSocketClient
    {
        public WebSocketClient() { }

        public WebSocketClient(HttpContext context, WebSocket webSocket)
        {
            this.WebSocket = webSocket;
            this.Query = new Dictionary<string, string>();
            foreach (string key in context.Request.Query.Keys)
            {
                this.Query.Add(key, context.Request.Query[key].ToString());
            }
            if (this.Query.ContainsKey("sid"))
            {
                this.ID = Guid.Parse(this.Query["sid"]);
            }
            else
            {
                this.ID = Guid.NewGuid();
            }
            string ip = IPAgent.GetIP(context);
            if (!Query.ContainsKey("IP")) Query.Add("IP", ip);
            if (!Query.ContainsKey("IPAddress")) Query.Add("IPAddress", IPAgent.GetAddress(ip));
        }

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

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendAsync(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            if (this.WebSocket.State == WebSocketState.Open)
            {
                await this.WebSocket.SendAsync(new ArraySegment<byte>(data, 0, data.Length), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <returns></returns>
        public async Task CloseAsync()
        {
            try
            {
                if (this.WebSocket.State == WebSocketState.Open)
                {
                    await this.WebSocket.CloseAsync(WebSocketCloseStatus.Empty, string.Empty, CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteLine(ex.Message, ConsoleColor.Red);
            }
        }
    }
}
