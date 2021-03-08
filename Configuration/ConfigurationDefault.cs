using System;
using Microsoft.Extensions.Configuration;

namespace SP.StudioCore.Configuration
{
    /// <summary>
    /// 根据默认环境读取配置
    /// </summary>
    public class ConfigurationDefault
    {
        private readonly IConfigurationRoot configurationRoot;

        public ConfigurationDefault()
        {
            this.configurationRoot = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
                    optional: true, reloadOnChange: true) //增加环境配置文件
                .AddEnvironmentVariables()
                .Build();
        }

        public string this[string configName] => configurationRoot[configName];

        /// <summary>
        /// 读取配置文件
        /// </summary>
        /// <param name="configName"></param>
        /// <returns></returns>
        public string Get(string configName) => configurationRoot.GetConnectionString(configName);
    }
}