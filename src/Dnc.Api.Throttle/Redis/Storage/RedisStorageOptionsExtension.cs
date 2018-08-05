using System;
using System.Collections.Generic;
using System.Text;
using Dnc.Api.Throttle.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dnc.Api.Throttle.Redis.Storage
{
    public class RedisStorageOptionsExtension : IApiThrottleOptionsExtension
    {
        private readonly Action<RedisStorageOptions> _options;

        public RedisStorageOptionsExtension(Action<RedisStorageOptions> options)
        {
            _options = options;
        }

        public void AddServices(IServiceCollection services)
        {
            services.TryAddSingleton<IRedisStorageDatabaseProvider, RedisStorageDatabaseProvider>();
            services.TryAddSingleton<IStorageProvider, RedisStorageProvider>();
            services.Configure(_options);
        }
    }
}
