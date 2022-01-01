using Microsoft.AspNetCore.Http;
using SP.StudioCore.Mvc.Exceptions;
using SP.StudioCore.Web;
using SP.StudioCore.Web.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SP.StudioCore.Tools
{
    /// <summary>
    /// websocket处理的中间件
    /// </summary>
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        public WebSocketMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        private WebSocketClient wsClient;

        /// <summary>
        /// websocket的逻辑处理
        /// </summary>
        private WebSocketHandlerBase? ws;

        public async Task Invoke(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                ws = ToolsFactory.GetWebSocket(context);
                if (ws == null)
                {
                    throw new ResultException("不支持WebSocket连接");
                }
                //后台成功接收到连接请求并建立连接后，前台的webSocket.onopen = function (event){}才执行
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(true); ;
                Dictionary<string, string> data = context.Request.Query.ToDictionary(t => t.Key, t => t.Value.ToString());
                if (!data.ContainsKey("IP")) data.Add("IP", IPAgent.GetIP(context));
                if (!data.ContainsKey("IPAddress")) data.Add("IPAddress", IPAgent.GetAddress(IPAgent.GetIP(context)));
                wsClient = ws.Register(new WebSocketClient
                {
                    ID = Guid.NewGuid(),
                    WebSocket = webSocket,
                    Query = data
                });
                try
                {
                    await Handler(wsClient);
                }
                catch (Exception ex)
                {
                    await context.Response.WriteAsync(ex.Message).ConfigureAwait(true); ;
                }
                finally
                {
                    ws.Remove(wsClient);
                }
            }
            else
            {
                await _next(context).ConfigureAwait(true);
            }
        }

        private async Task Handler(WebSocketClient client)
        {
            WebSocketReceiveResult result = null;
            do
            {
                byte[] buffer = new byte[1024 * 4];
                result = await client.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).ConfigureAwait(true);
                if (result.MessageType == WebSocketMessageType.Text && !result.CloseStatus.HasValue)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    if (message == "ping")
                    {
                        byte[] pong = Encoding.UTF8.GetBytes("pong");
                        await wsClient.WebSocket.SendAsync(new ArraySegment<byte>(pong, 0, pong.Length), WebSocketMessageType.Text, true, CancellationToken.None).ConfigureAwait(true); ;
                    }
                    else
                    {
                        await ws.Receive(wsClient, message).ConfigureAwait(true); ;
                    }
                }
            } while (!result.CloseStatus.HasValue);
        }
    }
}
