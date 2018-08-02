using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Dnc.Api.Throttle.Redis
{
    internal class RedisCacheProvider : ICacheProvider
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

        public RedisCacheProvider(IRedisDatabaseProvider dbProvider)
        {
            _dbProvider = dbProvider;
            _cache = _dbProvider.GetDatabase();
            _servers = _dbProvider.GetServerList();
        }

        public async Task<bool> SortedSetAddAsync(string key, string value, double score)
        {
            return await _cache.SortedSetAddAsync(key, value, score);
        }

        public async Task<long> SortedSetLengthAsync(string key, double min, double max)
        {
            return await _cache.SortedSetLengthAsync(key, min, max);
        }

        public async Task<bool> KeyExpireAsync(string key, TimeSpan? expiry)
        {
            return await _cache.KeyExpireAsync(key, expiry);
        }

        public async Task<long> SetAddAsync(string key, params string[] values)
        {
            if (values == null || values.Length == 0)
            {
                return 0;
            }

            RedisValue[] redisValues = new RedisValue[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                redisValues[i] = values[i];
            }
            return await _cache.SetAddAsync(key, redisValues);
        }

        /// <summary>
        /// 取得计时时间段api调用次数
        /// </summary>
        public async Task<long> GetValidApiRecordCount(string apikey, Policy policy, string policyValue, DateTime now, int duration)
        {
            var key = apikey + ":" + policy.ToString() + ":" + policyValue;
            return await _cache.SortedSetLengthAsync(key, now.Ticks - TimeSpan.FromSeconds(duration).Ticks, now.Ticks);
        }

    }
}
