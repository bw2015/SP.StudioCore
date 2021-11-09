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
        private static readonly IConfigSetting _configsetting;
        static Config()
        {
            _configsetting = IocCollection.GetService<IConfigSetting>();
        }
        private static readonly IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true) //增加环境配置文件
            .AddEnvironmentVariables()
            .Build();

        public static IConfigurationRoot GetConfig() => config;

        public static string GetConfig(string application, string name)
        {
            if (_configsetting == null)
            {
                return config[$"{application}:{name}"];
            }
            else
            {
                return Encryption.AesDecrypt(config[$"{application}:{name}"], Encryption.toMD5(_configsetting.Key));
            }
        }

        public static string GetConfig(params string[] args)
        {
            if (_configsetting == null)
            {
                return config[string.Join(":", args)];
            }
            else
            {
                return Encryption.AesDecrypt(config[string.Join(":", args)], Encryption.toMD5(_configsetting.Key));
            }
        }

        public static string GetConnectionString(string name)
        {
            if (_configsetting == null)
            {
                return config.GetConnectionString(name);
            }
            else
            {
                return Encryption.AesDecrypt(config.GetConnectionString(name), Encryption.toMD5(_configsetting.Key));
            }
        }
    }
}