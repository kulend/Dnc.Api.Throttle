using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dnc.Api.Throttle.Internal
{
    internal class ApiThrottleService : IApiThrottleService
    {
        private readonly ICacheProvider _cache;
        private readonly ApiThrottleOption _options;

        public ApiThrottleService(ICacheProvider cache, ApiThrottleOption options)
        {
            _cache = cache;
            _options = options;
        }

        public Task AddIpBlackListAsync(TimeSpan? expiry, params string[] ip)
        {
            _cache.SetAddAsync($"{_options.RedisKeyPrefix}:blackList:ip", ip);
            return Task.CompletedTask;
        }
    }
}
