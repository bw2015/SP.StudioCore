using Microsoft.AspNetCore.Http;
using SP.StudioCore.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Cache.Memory
{
    /// <summary>
    /// 检查本地内存的中间件
    /// </summary>
    public class MemoryMiddleware
    {
        private readonly RequestDelegate _next;
        public MemoryMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context)
        {
            string path = context.Request.Path.Value;
            if (path.EndsWith("/Tools/MemoryCheck"))
            {
                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "text/json";
                    string keys = context.Request.Query["keys"];
                    string result;
                    switch (context.Request.Query["action"])
                    {
                        case "remove":
                            {
                                Dictionary<string, bool> data = new Dictionary<string, bool>();
                                foreach (string key in keys.Split(','))
                                {
                                    if (!data.ContainsKey(key))
                                    {
                                        MemoryUtils.Remove(key);
                                        data.Add(key, true);
                                    }
                                }
                                result = data.ToJson();
                            }
                            break;
                        default:
                            {
                                Dictionary<string, object> data = new Dictionary<string, object>();
                                foreach (string key in (keys ?? string.Empty).Split(','))
                                {
                                    if (!data.ContainsKey(key))
                                    {
                                        data.Add(key, MemoryUtils.Get(key) ?? string.Empty);
                                    }
                                }
                                result = data.ToJson();
                            }
                            break;
                    }
                    await context.Response.WriteAsync(result).ConfigureAwait(false);
                }
                return;
            }

            await _next(context).ConfigureAwait(false);
        }

    }
}
