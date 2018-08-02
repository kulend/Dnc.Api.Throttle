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

        private readonly IStorageProvider _storage;

        public RedisCacheProvider(IRedisDatabaseProvider dbProvider, IStorageProvider storage)
        {
            _dbProvider = dbProvider;
            _cache = _dbProvider.GetDatabase();
            _servers = _dbProvider.GetServerList();
            _storage = storage;
        }

        /// <summary>
        /// 取得计时时间段api调用次数
        /// </summary>
        public async Task<long> GetValidApiRecordCount(string apikey, Policy policy, string policyValue, DateTime now, int duration)
        {
            var key = apikey + ":" + policy.ToString() + ":" + policyValue;
            return await _cache.SortedSetLengthAsync(key, now.Ticks - TimeSpan.FromSeconds(duration).Ticks, now.Ticks);
        }

        /// <summary>
        /// 保存调用记录
        /// </summary>
        public async Task SaveApiRecordAsync(string apikey, Policy policy, string policyValue, DateTime now, int duration)
        {
            var key = apikey + ":" + policy.ToString() + ":" + policyValue;

            await _cache.SortedSetAddAsync(key, now.Ticks.ToString(), now.Ticks);

            //设置过期时间
            await _cache.KeyExpireAsync(key, TimeSpan.FromSeconds(duration));
        }

        /// <summary>
        /// 取得黑名单列表
        /// </summary>
        public async Task<IEnumerable<string>> GetBlackListAsync(Policy policy)
        {
            return await _storage.GetBlackListAsync(policy);
        }

        /// <summary>
        /// 取得白名单列表
        /// </summary>
        public async Task<IEnumerable<string>> GetWhiteListAsync(Policy policy)
        {
            return await _storage.GetWhiteListAsync(policy);
        }
    }
}
