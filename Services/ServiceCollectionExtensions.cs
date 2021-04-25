using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting;
using SP.StudioCore.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SP.StudioCore.Services
{

    [Obsolete("已被Mvc.Startup.MvcStartupBase替代")]
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 容器初始化
        /// 1、注册MessageResult，用于跨程序的消息传递
        /// 2、注册HttpContext对象，全局可以使用静态变量 SP.StudioCore.Web.Context.Current
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection Initialize(this IServiceCollection services)
        {
            //作用域 仅在当前请求上下文中复用
            return services
                 // 消息体信息传递（仅适用于web程序)
                 .AddScoped<MessageResult>()
                 // httpContext注入
                 .AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); ;
        }
        /// <summary>
        /// 注入定时任务
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public static IServiceCollection AddJob(this IServiceCollection services)
        {
            foreach (var assemblie in GetAssemblies())
            {
                IEnumerable<Type> types = assemblie.GetTypes().Where(t => t.IsPublic && !t.IsAbstract && t.BaseType == typeof(BackgroundService));
                Parallel.ForEach(types, type =>
                {
                    Console.WriteLine($"==============已启动{type.Name}任务=================");
                    BackgroundService? service = (BackgroundService?)Activator.CreateInstance(type);
                    if (service != null)
                    {
                        service.Start();
                    }

                });
            }
            return services;
        }
        /// <summary>
        /// 获取项目中所有程序集
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Assembly> GetAssemblies()
        {
            var libs = DependencyContext.Default.CompileLibraries.Where(lib => lib.Type == "project");
            foreach (var lib in libs)
            {
                yield return AppDomain.CurrentDomain.Load(lib.Name);
            }
        }
    }
}
