using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace SP.StudioCore.Log
{
    public static class SpJsonConsoleLoggerExtensions
    {
        /// <summary>
        /// 添加Json输出的日志（方便于容器日志采集）
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection AddSpLogging(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            services.AddLogging(logging =>
            {
                logging.AddSpJsonConsole(opt =>
                {
                    opt.UseUtcTimestamp = true;
                    opt.TimestampFormat = "yyyy-MM-dd HH:mm:ss";
                });
                
                // 非生产环境下添加调试输出
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "" && Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Production") logging.AddDebug();
            });

            return services;
        }
        
        /// <summary>
        /// 添加Json输出的日志（方便于容器日志采集）
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static ILoggingBuilder AddSpJsonConsole(this ILoggingBuilder builder, Action<JsonConsoleFormatterOptions> options = null)
        {
            options ??= _ => new JsonConsoleFormatterOptions
            {
                UseUtcTimestamp = true,
                TimestampFormat = "yyyy-MM-dd HH:mm:ss"
            };

            //添加控制台输出
            builder.AddConsoleFormatter<SpJsonConsole, JsonConsoleFormatterOptions>(_ => options(_));

            builder.AddConsole(o => { o.FormatterName = "json"; });
            return builder;
        }
    }
}