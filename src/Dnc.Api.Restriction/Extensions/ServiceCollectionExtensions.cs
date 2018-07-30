using System;
using Dnc.Api.Restriction;
using Dnc.Api.Restriction.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiRestriction(this IServiceCollection services, Action<ApiRestrictionOption> options)
        {
            services.Configure(options);
            services.TryAddSingleton<IRedisDatabaseProvider, RedisDatabaseProvider>();
            services.TryAddSingleton<ICacheProvider, RedisCacheProvider>();

            return services;
        }
    }
}
