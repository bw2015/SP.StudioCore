using Microsoft.AspNetCore.Hosting;
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

            CreateHostBuilder(args).Build().Run();

            return true;
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args)
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder.UseStartup<ToolsStartup>();
               });
    }
}
