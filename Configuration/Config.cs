using System;
using Microsoft.Extensions.Configuration;

namespace SP.StudioCore.Configuration
{
    /// <summary>
    /// 读取配置
    /// </summary>
    public static class Config
    {
        private static readonly IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json",                                                                 optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true) //增加环境配置文件
            .AddEnvironmentVariables()
            .Build();

        public static IConfigurationRoot GetConfig() => config;
        
        public static string GetConfig(string application, string name)
        {
            return config[$"{application}:{name}"];
        }

        public static string GetConfig(params string[] args)
        {
            return config[string.Join(":", args)];
        }

        public static string GetConnectionString(string name)
        {
            return config.GetConnectionString(name);
        }
    }
}