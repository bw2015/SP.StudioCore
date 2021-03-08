using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Mvc.Startups
{
    /// <summary>
    /// 对XSS注入内容进行处理
    /// </summary>
    public class XSSFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                builder.UseMiddleware<RequestSetOptionsMiddleware>();
                next(builder);
            };
        }
    }

    public class RequestSetOptionsMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestSetOptionsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.HasFormContentType && context.Request.Form.Keys.Count > 0)
            {
                foreach (string key in context.Request.Form.Keys)
                {
                    StringValues value = context.Request.Form[key];
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                       // context.Request.Form[key] = value;
                    }
                }
            }

            await _next(context).ConfigureAwait(false);
        }
    }
}
