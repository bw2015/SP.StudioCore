using Microsoft.AspNetCore.Http;
using SP.StudioCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace SP.StudioCore.Mvc.MiddleWare
{
    /// <summary>
    /// Header路由中间件
    /// </summary>
    public class HeaderRouteMiddleware
    {
        private readonly RequestDelegate _next;
        public HeaderRouteMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context)
        {
            var    sw   = Stopwatch.StartNew();
            string path = context.Request.Path.Value;
            if (path == "/")
            {
                switch (context.Request.Method)
                {
                    case "HEAD":
                    case "GET":
                        if (!context.Response.HasStarted)
                        {
                            context.Response.StatusCode = 200;
                            string result = "success";
                            if (context.Request.Headers.ContainsKey("X-Log-Header"))
                            {
                                result = context.GetLog();
                            }
                            await context.Response.WriteAsync(result).ConfigureAwait(false);
                            return;
                        }
                        return;
                    case "POST":
                        path = context.Request.Headers["Path"].ToString();
                        if (string.IsNullOrWhiteSpace(path))
                        {
                            context.Response.StatusCode = 404;
                            await context.Response.WriteAsync(string.Empty).ConfigureAwait(false);
                            return;
                        }
                        if (!path.StartsWith("/"))
                        {
                            path = "/" + path;
                        }
                        context.Request.Path = path;
                        break;
                }
            }
            else if (path.Substring(0, 2) == "//")
            {
                context.Request.Path = path[1..];
            }

            Console.WriteLine($"HeaderRouteMiddleware {sw.ElapsedMilliseconds}");
            await _next(context).ConfigureAwait(false);
        }
    }
}
