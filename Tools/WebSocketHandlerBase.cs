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
    /// 标记这是一个WebSocket处理对象
    /// </summary>
    public interface IWebSocketHandler
    {

    }

    /// <summary>
    /// WebSocket 处理的基类
    /// </summary>
    public abstract class WebSocketHandlerBase : IWebSocketHandler
    {
        private List<WebSocketClient>? _clients;

        /// <summary>
        /// 所有的客户端
        /// </summary>
        public List<WebSocketClient> clients
        {
            get
            {
                if (this._clients == null) this._clients = new();
                return this._clients;
            }
        }

        public virtual WebSocketClient Register(WebSocketClient wsClient, TaskCompletionSource<object> socketFinishedTcs)
        {
            this.clients.Add(wsClient);
            socketFinishedTcs.SetResult(wsClient.ID);
            return wsClient;
        }

        /// <summary>
        /// 注销客户端
        /// </summary>
        /// <param name="wsClient"></param>
        public virtual async Task Remove(WebSocketClient wsClient)
        {
            await wsClient.CloseAsync();
            this.clients.Remove(wsClient);
        }

        /// <summary>
        /// 批量发送信息
        /// </summary>
        /// <param name="clients"></param>
        /// <returns></returns>
        public async Task Send(string message, IEnumerable<Guid> users)
        {
            var list = this.clients.Where(t => users.Any(p => p == t.ID));
            if (!list.Any()) return;
            byte[] data = Encoding.UTF8.GetBytes(message);
            await Task.WhenAll(list.Select(t => t.WebSocket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, CancellationToken.None))).ConfigureAwait(true);
        }

        /// <summary>
        /// 接收到消息的逻辑处理
        /// </summary>
        /// <param name="wsClient"></param>
        /// <param name="message"></param>
        public abstract Task Receive(WebSocketClient wsClient, string message);
    }

    public abstract class WebSocketHandlerBase<T> : WebSocketHandlerBase where T : WebSocketHandlerBase, new()
    {
        public static T Instance()
        {
            string? name = typeof(T).Assembly.GetName().Name?.ToLower();
            if (name == null) throw new NullReferenceException($"未能找到处理类型 {name}");
            if (ToolsFactory.WebSocketHandlerCache.ContainsKey(name)) return (T)ToolsFactory.WebSocketHandlerCache[name];
            lock (typeof(T))
            {
                if (ToolsFactory.WebSocketHandlerCache.ContainsKey(name)) return (T)ToolsFactory.WebSocketHandlerCache[name];
                T instance = new();
                ToolsFactory.WebSocketHandlerCache.Add(name, instance);
                return instance;
            }
        }
    }
}
