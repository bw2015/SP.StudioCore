using Microsoft.AspNetCore.Http;
using NativeWebSocket;
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
using WebSocket = System.Net.WebSockets.WebSocket;

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

        private WebSocketClient? wsClient;

        /// <summary>
        /// websocket的逻辑处理
        /// </summary>
        private WebSocketHandlerBase? wsHandler;

        public async Task Invoke(HttpContext context)
        {
            // 如果是websocket的请求
            if (context.WebSockets.IsWebSocketRequest)
            {
                wsHandler = ToolsFactory.GetWebSocket(context);
                if (wsHandler == null)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    return;
                }

                using (WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync(new WebSocketAcceptContext
                {
                    DangerousEnableCompression = true
                }))
                {
                    TaskCompletionSource<object> socketFinishedTcs = new TaskCompletionSource<object>();
                    wsClient = wsHandler.Register(new WebSocketClient(context, webSocket), socketFinishedTcs);
                    await socketFinishedTcs.Task;

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
                        await wsHandler.Remove(wsClient);
                    }
                }
            }
            else
            {
                await _next(context).ConfigureAwait(true);
            }
        }

        private async Task Handler(WebSocketClient client)
        {
            WebSocket webSocket = client.WebSocket;
            byte[] buffer = new byte[1024 * 4];
            WebSocketReceiveResult receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!receiveResult.CloseStatus.HasValue)
            {
                if (receiveResult.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                    if (message == "ping")
                    {
                        await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("pong"), 0, receiveResult.Count),
                            receiveResult.MessageType, receiveResult.EndOfMessage, CancellationToken.None);
                    }
                    else
                    {
                        if (wsHandler != null && wsClient != null)
                        {
                            await wsHandler.Receive(wsClient, message);
                        }
                    }
                }
                receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(receiveResult.CloseStatus.Value, receiveResult.CloseStatusDescription, CancellationToken.None);
        }
    }
}
