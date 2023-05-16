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
    public class WebSocketClient : IDisposable
    {
        public WebSocketClient() { }

        public HttpContext HttpContext { get; private set; }

        public WebSocketClient(HttpContext context, WebSocket webSocket)
        {
            this.HttpContext = context;
            this.ID = Guid.NewGuid();
            this.WebSocket = webSocket;
            this.Host = context.Request.Host.Value;
            this.Query = context.Request.Query.ToDictionary(t => t.Key, t => t.Value.ToString());
            this.Headers = context.Request.Headers.ToDictionary(t => t.Key, t => t.Value.ToString());
            this.IpAddress = IPAgent.GetIP(context);
            this.StartTime = this.OnlineTime = WebAgent.GetTimestamps();
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
        /// 连接主机
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 附加数据
        /// </summary>
        public Dictionary<string, string> Query { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// 客户端的IP地址
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// 连接时间
        /// </summary>
        public long StartTime { get; set; }

        /// <summary>
        /// 在线时间
        /// </summary>
        public long OnlineTime { get; set; }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendAsync(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            try
            {
                if (this.WebSocket.State == WebSocketState.Open)
                {
                    using (var source = new CancellationTokenSource())
                    {
                        // 3秒超时
                        source.CancelAfter(3000);
                        await this.WebSocket.SendAsync(new ArraySegment<byte>(data, 0, data.Length), WebSocketMessageType.Text, true, source.Token);
                    }
                }
            }
            catch (WebSocketException ex)
            {

            }
            catch (TaskCanceledException ex)
            {
                ConsoleHelper.WriteLine($"[SendAsync - {ex.GetType().Name}] {ex.Message}", ConsoleColor.DarkGray);
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteLine($"[SendAsync - {ex.GetType().Name}] {ex.Message}", ConsoleColor.Red);
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
                if (!this.WebSocket.CloseStatus.HasValue)
                {
                    using (CancellationTokenSource source = new CancellationTokenSource(1000))
                    {
                        await this.WebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, source.Token);
                    }
                }
            }
            catch (TaskCanceledException ex)
            {

            }
            catch (WebSocketException ex)
            {

            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteLine($"[CloseAsync - {ex.GetType().Name}] {ex.Message}   -   WebSocketClient.CloseAsync", ConsoleColor.Red);
            }
        }

        public void Dispose()
        {
            this.WebSocket?.Dispose();
        }

        public static implicit operator WebSocket(WebSocketClient client)
        {
            return client.WebSocket;
        }
    }
}
