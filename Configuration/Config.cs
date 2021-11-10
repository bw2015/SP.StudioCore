using System;
using Microsoft.Extensions.Configuration;
using SP.StudioCore.Ioc;
using SP.StudioCore.Security;

namespace SP.StudioCore.Configuration
{
    /// <summary>
    /// 读取配置
    /// </summary>
    public static class Config
    {
        private static readonly IConfigSetting _configsetting = IocCollection.GetService<IConfigSetting>() ?? new DefaultConfigSetting();


        private static readonly IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true) //增加环境配置文件
            .AddEnvironmentVariables()
            .Build();

        public static IConfigurationRoot GetConfig() => config;

        public static string GetConfig(string application, string name)
        {
            return _configsetting.GetConfigContent(config[$"{application}:{name}"]);
        }

        public static string GetConfig(params string[] args)
        {
            return _configsetting.GetConfigContent(config[string.Join(":", args)]);
            //  return Encryption.AesDecrypt(config[string.Join(":", args)], Encryption.toMD5(_configsetting.Key));

        }

        public static string GetConnectionString(string name)
        {
            return _configsetting.GetConfigContent(config.GetConnectionString(name));
        }
    }

    class DefaultConfigSetting : IConfigSetting
    {

        public string GetConfigContent(string content) => content;
    }
}