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
             .AddJsonFile("appsettings.json")
             .Build();

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