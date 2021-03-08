using Microsoft.Extensions.DependencyInjection;
using SP.StudioCore.Web;
using System;
using System.Collections.Generic;

namespace SP.StudioCore.Ioc
{
    /// <summary>
    /// IOC容器，统一管理
    /// </summary>
    public static class IocCollection
    {
        private static IServiceCollection _services;
        private static IServiceProvider _provider;

        /// <summary>
        /// 加入到自定义的IOC容器中
        /// </summary>
        /// <param name="services"></param>
        public static void AddService(this IServiceCollection services)
        {
            _services = services;
        }

        public static IServiceCollection AddService<TService>(ServiceLifetime contextLifetime) where TService : class
        {
            return contextLifetime switch
            {
                ServiceLifetime.Singleton => _services.AddSingleton<TService>(),
                ServiceLifetime.Scoped => _services.AddScoped<TService>(),
                ServiceLifetime.Transient => _services.AddTransient<TService>(),
                _ => throw new NotSupportedException()
            };
        }

        public static IServiceCollection AddService<TService>(TService service, ServiceLifetime contextLifetime) where TService : class
        {
            return contextLifetime switch
            {
                ServiceLifetime.Singleton => _services.AddSingleton(service),
                ServiceLifetime.Scoped => _services.AddScoped(t => service),
                ServiceLifetime.Transient => _services.AddTransient(t => service),
                _ => throw new NotSupportedException()
            };
        }


        private static IServiceProvider Provider
        {
            get
            {
                if (Context.Current != null)
                {
                    return _provider = Context.Current.RequestServices;
                }
                else if (_provider == null)
                {
                    _provider = _services?.BuildServiceProvider();
                }
                return _provider;
            }
        }
        /// <summary>
        /// 从容器中拿出对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetService<T>()
        {
            return Provider == null ? default : Provider.GetService<T>();
        }

        public static object GetService(Type serviceType)
        {
            return Provider == null ? default : Provider.GetService(serviceType);
        }

        public static IEnumerable<T> GetServices<T>()
        {
            return Provider == null ? default : Provider.GetServices<T>();
        }


    }
}
