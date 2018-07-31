using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dnc.Api.Throttle.Redis
{
    internal class RedisStorageProvider : IStorageProvider
    {
        /// <summary>
        /// The cache.
        /// </summary>
        private readonly IDatabase _cache;

        /// <summary>
        /// The db provider.
        /// </summary>
        private readonly IRedisDatabaseProvider _dbProvider;

        /// <summary>
        /// The servers.
        /// </summary>
        private readonly IEnumerable<IServer> _servers;

        private readonly ApiThrottleOption _options;

        public RedisStorageProvider(IRedisDatabaseProvider dbProvider, IOptions<ApiThrottleOption> options)
        {
            _dbProvider = dbProvider;
            _cache = _dbProvider.GetDatabase();
            _servers = _dbProvider.GetServerList();
            _options = options.Value;
        }

        public async Task SaveIpBlackListAsync(TimeSpan? expiry, params string[] ip)
        {
            await _cache.SetAddAsync($"{_options.RedisKeyPrefix}:blackList:ip", ip);
        }
    }
}
