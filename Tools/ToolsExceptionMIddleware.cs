using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SP.StudioCore.Enums;
using SP.StudioCore.Http;
using SP.StudioCore.Mvc.Exceptions;
using SP.StudioCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Tools
{
    /// <summary>
    /// 工具类的一场处理中间件
    /// </summary>
    public class ToolsExceptionMIddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ToolsExceptionMIddleware> _logger;

        public ToolsExceptionMIddleware(RequestDelegate next, ILogger<ToolsExceptionMIddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context, IWebHostEnvironment env)
        {
            try
            {
                await _next(context).ConfigureAwait(false);
            }
            catch (TargetInvocationException ex)
            {
                string? message = ex.InnerException?.Message;
                if (!string.IsNullOrEmpty(message))
                {
                    await context.Response.WriteAsync(message);
                }
            }
            catch (ResultException ex)
            {
                await ex.WriteAsync(context).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                Guid logId = Guid.NewGuid();
                string info = ErrorHelper.GetExceptionContent(ex, context);

                Console.ForegroundColor = ConsoleColor.Red;
                _logger.LogError(info);
                Console.ResetColor();

                await context.ShowError(ErrorType.Exception, ex.Message, new Dictionary<string, object>()
                {
                    {"RequestID", logId},
                    {"Track", JObject.Parse( info) }
                }).WriteAsync(context).ConfigureAwait(true);
            }
        }
    }
}
