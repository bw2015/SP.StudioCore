using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SP.StudioCore.Array;
using SP.StudioCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SP.StudioCore.Tools
{
    public abstract class ToolsProgram
    {
        private static void SetThreads(int minThreads, int maxThreads)
        {
            if (minThreads != 0 && ThreadPool.SetMinThreads(minThreads, minThreads))
            {
                ConsoleHelper.WriteLine($"设定最小线程数量为:{minThreads}", ConsoleColor.Green);
            }
            if (maxThreads != 0 && ThreadPool.SetMaxThreads(maxThreads, maxThreads))
            {
                ConsoleHelper.WriteLine($"设定最大线程数量为:{maxThreads}", ConsoleColor.Green);
            }
        }

        protected static bool WebStartup(string[] args)
        {
            if (!args.Contains("--urls")) return false;
            SetThreads(args.Get("-minThreads", 0), args.Get("-maxThreads", 0));
            CreateWebHostBuilder<ToolsStartup>(args).Build().Run();
            return true;
        }

        protected static bool WebStartup<TStartup>(string[] args) where TStartup : ToolsStartup
        {
            if (!args.Contains("--urls")) return false;
            SetThreads(args.Get("-minThreads",0), args.Get("-maxThreads", 0));
            CreateWebHostBuilder<TStartup>(args).Build().Run();
            return true;
        }

        protected static void WorkStartup(string[] args,
            Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            CreateWorkHostBuilder(args, configureDelegate).Build().Run();
        }

        protected static void WorkStartup<THostedService>(string[] args) where THostedService : class, IHostedService
        {
            CreateWorkHostBuilder(args, (hostContext, services) =>
            {
                services.AddHostedService<THostedService>();
            }).Build().Run();
        }

        protected static void WorkStartup(Assembly assembly, string[] args, Action<HostBuilderContext, IServiceCollection> inject = null)
        {
            // 找出所有的Work类型
            Type[] works = assembly.GetTypes().Where(t => t.IsClass && t.GetInterfaces().Contains(typeof(IHostedService))).ToArray();

            CreateWorkHostBuilder(args, (hostContext, services) =>
            {
                if (inject != null) inject.Invoke(hostContext, services);

                foreach (Type work in works)
                {
                    ConsoleHelper.WriteLine(work.FullName ?? string.Empty, ConsoleColor.Green);

                    MethodInfo? method = typeof(ServiceCollectionHostedServiceExtensions).GetMethods().FirstOrDefault(t => t.Name == "AddHostedService" && t.GetParameters().Length == 1);

                    if (method == null) continue;
                    method.MakeGenericMethod(work).Invoke(null, new object[] { services });
                }
                //ServiceCollectionHostedServiceExtensions.AddHostedService(services);
                // services.AddHostedService<THostedService>();
            }).Build().Run();
        }

        private static IHostBuilder CreateWebHostBuilder<TStartup>(string[] args) where TStartup : ToolsStartup
        {
            return Host.CreateDefaultBuilder(args)
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder.UseStartup<TStartup>();
               });
        }

        private static IHostBuilder CreateWorkHostBuilder(string[] args, Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            return Host.CreateDefaultBuilder(args)
                   .ConfigureServices(configureDelegate);
        }
    }
}
