using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SP.StudioCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Tools
{
    public abstract class ToolsProgram
    {
        protected static bool WebStartup(string[] args)
        {
            if (!args.Contains("--urls")) return false;
            CreateWebHostBuilder(args).Build().Run();
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

        protected static void WorkStartup(Assembly assembly, string[] args)
        {
            // 找出所有的Work类型
            Type[] works = assembly.GetTypes().Where(t => t.IsClass && t.GetInterfaces().Contains(typeof(IHostedService))).ToArray();

            CreateWorkHostBuilder(args, (hostContext, services) =>
            {
                foreach (Type work in works)
                {
                    ConsoleHelper.WriteLine(work.FullName, ConsoleColor.Green);

                    MethodInfo method = typeof(ServiceCollectionHostedServiceExtensions).GetMethods().FirstOrDefault(t => t.Name == "AddHostedService" && t.GetParameters().Length == 1);

                    method.MakeGenericMethod(work).Invoke(null,new object[] { services });
                }
                //ServiceCollectionHostedServiceExtensions.AddHostedService(services);
                // services.AddHostedService<THostedService>();
            }).Build().Run();
        }

        private static IHostBuilder CreateWebHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args)
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder.UseStartup<ToolsStartup>();
               });

        private static IHostBuilder CreateWorkHostBuilder(string[] args, Action<HostBuilderContext, IServiceCollection> configureDelegate) =>
           Host.CreateDefaultBuilder(args)
               .ConfigureServices(configureDelegate);
    }
}
