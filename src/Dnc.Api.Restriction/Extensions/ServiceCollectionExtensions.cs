using System;
using Dnc.Api.Throttle;
using Dnc.Api.Throttle.Internal;
using Dnc.Api.Throttle.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiThrottle(this IServiceCollection services, Action<ApiThrottleOption> options)
        {
            services.Configure(options);
            services.TryAddSingleton<IRedisDatabaseProvider, RedisDatabaseProvider>();
            services.TryAddSingleton<ICacheProvider, RedisCacheProvider>();
            services.TryAddSingleton<IStorageProvider, RedisStorageProvider>();
            services.TryAddSingleton<IApiThrottleService, ApiThrottleService>();

            return services;
        }
    }
}
