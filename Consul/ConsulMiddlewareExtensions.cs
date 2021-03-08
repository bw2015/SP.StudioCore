using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.Consul
{
    /// <summary>
    /// Consul相关注册方法
    /// </summary>
    public static class ConsulMiddlewareExtensions
    {
        public static IApplicationBuilder UseConsul(this IApplicationBuilder app, IHostApplicationLifetime lifetime, ConsulOption consulOption = null)
        {
            if (consulOption == null)
            {
                IConfiguration config = app.ApplicationServices.GetRequiredService<IConfiguration>();
                consulOption = config.GetSection("Consul").Get<ConsulOption>();
            }
            if (consulOption == null || string.IsNullOrEmpty(consulOption.Address)) return app;
            string clientId = Guid.NewGuid().ToString();
            ConsulClient consulClient = new ConsulClient(x =>
            {
                x.Address = new Uri(consulOption.Address);
            });

            AgentServiceRegistration registration = new AgentServiceRegistration()
            {
                ID = clientId,
                Name = consulOption.ServiceName,// 服务名
                Address = consulOption.ServiceIP, // 服务绑定IP
                Port = consulOption.ServicePort, // 服务绑定端口
                Check = new AgentServiceCheck()
                {
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),//服务启动多久后注册
                    Interval = TimeSpan.FromSeconds(10),//健康检查时间间隔
                    HTTP = consulOption.ServiceHealthCheck,//健康检查地址
                    Timeout = TimeSpan.FromSeconds(5)
                },
                Tags = new[] { $"urlperfix-/{consulOption.ServiceName}" }
            };

            // 服务注册
            consulClient.Agent.ServiceRegister(registration).Wait();

            // 应用程序终止时，服务取消注册
            lifetime.ApplicationStopping.Register(() =>
            {
                consulClient.Agent.ServiceDeregister(clientId).Wait();
            });
            return app;
        }
    }
}
