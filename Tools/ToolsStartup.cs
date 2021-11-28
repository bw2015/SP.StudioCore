using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using SP.StudioCore.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Tools
{
    public class ToolsStartup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
            services.AddCors()
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app
                .UseCors(builder =>
                {
                    builder.WithOrigins("*");
                    builder.AllowAnyHeader();
                    builder.AllowAnyMethod();
                })
                .UseWebSockets(new WebSocketOptions
                {
                    KeepAliveInterval = TimeSpan.FromMinutes(1)
                })
            .UseStaticFiles()
            .UseHttpContext()          
            .UseMiddleware<WebSocketMiddleware>()
            .UseMiddleware<ToolsExceptionMIddleware>()
                .Run(async (context) =>
                {
                    await ToolsFactory.Invote(context).WriteAsync(context).ConfigureAwait(true);
                });
        }
    }
}
