using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SP.StudioCore.Model;
using SP.StudioCore.Web;
using System;

namespace SP.StudioCore.Mvc.Startups
{
    /// <summary>
    /// Startup 的基类
    /// 默认开启 Cors，针对所有方法
    /// </summary>
    public abstract class MvcStartupBase
    {
        protected abstract void Configure(IServiceCollection services);

        /// <summary>
        /// 使用中间件
        /// </summary>
        /// <param name="app"></param>
        protected abstract void UseMiddleware(IApplicationBuilder app);

        public virtual void ConfigureServices(IServiceCollection services)
        {
            this.Configure(
                services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                // 消息传递注意，此处注入不支持非web环境的消息传递
                .AddScoped<MessageResult>());
        }

        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseHttpContext();

            this.UseMiddleware(app);

            app.UseRouting();

            //app.UseCors();

            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

    }
}
