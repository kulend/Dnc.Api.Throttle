using System;
using System.Collections.Generic;
using System.Text;
using Dnc.Api.Throttle.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dnc.Api.Throttle.Redis.Cache
{
    public class RedisCacheOptionsExtension : IApiThrottleOptionsExtension
    {
        private readonly Action<RedisCacheOptions> _options;

        public RedisCacheOptionsExtension(Action<RedisCacheOptions> options)
        {
            _options = options;
        }

        public void AddServices(IServiceCollection services)
        {
            services.TryAddSingleton<IRedisCacheDatabaseProvider, RedisCacheDatabaseProvider>();
            services.TryAddSingleton<ICacheProvider, RedisCacheProvider>();
            services.Configure<RedisCacheOptions>(_options);
        }
    }
}
