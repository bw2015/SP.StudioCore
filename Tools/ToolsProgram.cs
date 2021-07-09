using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
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
