using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Dnc.Api.Throttle.Redis.Cache
{
    internal class RedisCacheProvider : ICacheProvider
    {
        //缓存过期时间
        private TimeSpan? expiry = TimeSpan.FromHours(24);

        /// <summary>
        /// The cache.
        /// </summary>
        private readonly IDatabase _db;

        /// <summary>
        /// The db provider.
        /// </summary>
        private readonly IRedisCacheDatabaseProvider _dbProvider;

        /// <summary>
        /// The servers.
        /// </summary>
        private readonly IEnumerable<IServer> _servers;

        private readonly IStorageProvider _storage;

        private readonly RedisCacheOptions _options;

        public RedisCacheProvider(IRedisCacheDatabaseProvider dbProvider, IStorageProvider storage, IOptions<RedisCacheOptions> options)
        {
            _dbProvider = dbProvider;
            _db = _dbProvider.GetDatabase();
            _servers = _dbProvider.GetServerList();
            _storage = storage;
            _options = options.Value;
        }

        /// <summary>
        /// 取得计时时间段api调用次数
        /// </summary>
        public async Task<long> GetValidApiRecordCount(string apikey, Policy policy, string policyValue, DateTime now, int duration)
        {
            var key = apikey + ":" + policy.ToString() + ":" + policyValue;
            return await _db.SortedSetLengthAsync(key, now.Ticks - TimeSpan.FromSeconds(duration).Ticks, now.Ticks);
        }

        /// <summary>
        /// 保存调用记录
        /// </summary>
        public async Task SaveApiRecordAsync(string apikey, Policy policy, string policyValue, DateTime now, int duration)
        {
            var key = apikey + ":" + policy.ToString() + ":" + policyValue;

            await _db.SortedSetAddAsync(key, now.Ticks.ToString(), now.Ticks);

            //设置过期时间
            await _db.KeyExpireAsync(key, TimeSpan.FromSeconds(duration));
        }

        /// <summary>
        /// 取得黑名单列表
        /// </summary>
        public async Task<IEnumerable<ListItem>> GetBlackListAsync(Policy policy)
        {
            //如果和Storage是同一个redis库，则直接从Storage取数据，不用缓存
            if (_options.SameWithStorage)
            {
                return await _storage.GetBlackListAsync(policy);
            }

            var key = $"{_options.CacheKeyPrefix}:bl:{policy.ToString().ToLower()}";
            //判断是否存在key
            if (await _db.KeyExistsAsync(key))
            {
                //取得数据
                var data = await _db.SortedSetRangeByScoreWithScoresAsync(key, start: DateTime.Now.Ticks, stop: double.PositiveInfinity, exclude: Exclude.Both, order: Order.Ascending);
                return data.Select(x => new ListItem { Value = (string)x.Element, ExpireTicks = x.Score });
            }
            else
            {
                var data = await _storage.GetBlackListAsync(policy);
                SortedSetEntry[] entrys = data.Select(x => new SortedSetEntry(x.Value, x.ExpireTicks)).ToArray();
                //保存
                await _db.SortedSetAddAsync(key, entrys);
                //设置缓存过期时间
                await _db.KeyExpireAsync(key, expiry);

                return data;
            }
        }

        /// <summary>
        /// 取得白名单列表
        /// </summary>
        public async Task<IEnumerable<ListItem>> GetWhiteListAsync(Policy policy)
        {
            //如果和Storage是同一个redis库，则直接从Storage取数据，不用缓存
            if (_options.SameWithStorage)
            {
                return await _storage.GetWhiteListAsync(policy);
            }

            var key = $"{_options.CacheKeyPrefix}:wl:{policy.ToString().ToLower()}";
            //判断是否存在key
            if (await _db.KeyExistsAsync(key))
            {
                //取得数据
                var data = await _db.SortedSetRangeByScoreWithScoresAsync(key, start: DateTime.Now.Ticks, stop: double.PositiveInfinity, exclude: Exclude.Both, order: Order.Ascending);
                return data.Select(x => new ListItem { Value = (string)x.Element, ExpireTicks = x.Score });
            }
            else
            {
                var data = await _storage.GetWhiteListAsync(policy);
                SortedSetEntry[] entrys = data.Select(x => new SortedSetEntry(x.Value, x.ExpireTicks)).ToArray();
                //保存
                await _db.SortedSetAddAsync(key, entrys);
                //设置缓存过期时间
                await _db.KeyExpireAsync(key, expiry);

                return data;
            }
        }

        /// <summary>
        /// 清除黑名单缓存
        /// </summary>
        /// <returns></returns>
        public async Task ClearBlackListCacheAsync(Policy policy)
        {
            //如果和Storage是相同的redis库，则不需要清缓存
            if (!_options.SameWithStorage)
            {
                var key = $"{_options.CacheKeyPrefix}:bl:{policy.ToString().ToLower()}";
                await _db.KeyDeleteAsync(key);
            }
        }

        /// <summary>
        /// 清除白名单缓存
        /// </summary>
        /// <returns></returns>
        public async Task ClearWhiteListCacheAsync(Policy policy)
        {
            //如果和Storage是相同的redis库，则不需要清缓存
            if (!_options.SameWithStorage)
            {
                var key = $"{_options.CacheKeyPrefix}:wl:{policy.ToString().ToLower()}";
                await _db.KeyDeleteAsync(key);
            }
        }
    }
}
