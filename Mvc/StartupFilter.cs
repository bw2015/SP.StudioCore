using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.Mvc
{
    /// <summary>
    /// 在应用程序配置中间件管道的开头或结尾处使用IStartupFilter配置中间件
    /// </summary>
    public class StartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.Use(async (context, _next) =>
                {
                    context.Response.Headers.Add("Server", this.GetType().Assembly.GetName().Name);
                    await _next().ConfigureAwait(false);
                });
                next(app);
            };
        }
    }

}
