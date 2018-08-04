using Dnc.Api.Throttle;
using Dnc.Api.Throttle.Redis.Cache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OptionsExtensionsForRedisCache
    {
        public static ApiThrottleOptions UseRedisCache(this ApiThrottleOptions options, Action<RedisCacheOptions> configure)
        {
            options.AddExtension(new RedisCacheOptionsExtension(configure));
            return options;
        }
    }
}
