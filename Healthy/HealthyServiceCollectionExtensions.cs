using Microsoft.Extensions.DependencyInjection;

namespace SP.StudioCore.Healthy
{
    public static class HealthyServiceCollectionExtensions
    {
        /// <summary>
        /// 注册健康检查组件
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection AddHealthy(this IServiceCollection services, HealthyOptions options)
        {
            if (string.IsNullOrEmpty(options.Address)) return services;
            HealthyProvider.Run(options);
            return services;
        }
    }
}
