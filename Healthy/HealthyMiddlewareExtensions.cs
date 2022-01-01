using Microsoft.AspNetCore.Builder;
using System;

namespace SP.StudioCore.Healthy
{
    public static class HealthyMiddlewareExtensions
    {
        public static IApplicationBuilder UseHealthy(this IApplicationBuilder app, HealthyOptions options)
        {
            HealthyProvider.Run(options);
            return app;
        }
        public static IApplicationBuilder UseHealthy(this IApplicationBuilder app, Func<HealthyOptions> action)
        {
            HealthyOptions options = action();
            return app.UseHealthy(options);
        }
    }
}
