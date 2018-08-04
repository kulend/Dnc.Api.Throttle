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
        public static IServiceCollection AddApiThrottle(this IServiceCollection services, Action<ApiThrottleOptions> options)
        {
            services.Configure(options);
            services.TryAddSingleton<IApiThrottleService, ApiThrottleService>();

            //Options and extension service
            var opts = new ApiThrottleOptions();
            options(opts);

            foreach (var serviceExtension in opts.Extensions)
            {
                serviceExtension.AddServices(services);
            }

            services.AddSingleton(opts);

            return services;
        }
    }
}
