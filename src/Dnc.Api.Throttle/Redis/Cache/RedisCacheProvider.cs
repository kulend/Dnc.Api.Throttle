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
        public async Task<long> GetApiRecordCountAsync(string api, Policy policy, string policyKey, string policyValue, DateTime now, int duration)
        {
            var key = FromatApiRecordKey(api, policy, policyKey, policyValue);
            return await _db.SortedSetLengthAsync(key, now.Ticks - TimeSpan.FromSeconds(duration).Ticks, now.Ticks);
        }

        /// <summary>
        /// 保存调用记录
        /// </summary>
        public async Task AddApiRecordAsync(string api, Policy policy, string policyKey, string policyValue, DateTime now, int duration)
        {
            var key = FromatApiRecordKey(api, policy, policyKey, policyValue);

            await _db.SortedSetAddAsync(key, now.Ticks.ToString(), now.Ticks);

            //设置过期时间
            await _db.KeyExpireAsync(key, TimeSpan.FromSeconds(duration));
        }

        /// <summary>
        /// 取得名单列表
        /// </summary>
        public async Task<IEnumerable<ListItem>> GetRosterListAsync(RosterType rosterType, string api, Policy policy, string policyKey)
        {
            var key = FromatRosterKey(rosterType, api, policy, policyKey);
            //判断是否存在key
            if (await _db.KeyExistsAsync(key))
            {
                //取得数据
                var data = await _db.SortedSetRangeByScoreWithScoresAsync(key, start: DateTime.Now.Ticks, stop: double.PositiveInfinity, exclude: Exclude.Both, order: Order.Ascending);
                return data.Select(x => new ListItem { Value = (string)x.Element, ExpireTicks = x.Score });
            }
            else
            {
                var data = (await _storage.GetRosterListAsync(rosterType, api, policy, policyKey)).ToList();
                //Ip地址转换
                if (policy == Policy.Ip)
                {
                    foreach (var item in data)
                    {
                        item.Value = Common.IpToNum(item.Value);
                    }
                }
                SortedSetEntry[] entrys = data.Select(x => new SortedSetEntry(x.Value, x.ExpireTicks)).ToArray();
                //保存
                await _db.SortedSetAddAsync(key, entrys);
                //设置缓存过期时间
                await _db.KeyExpireAsync(key, expiry);

                return data;
            }
        }

        /// <summary>
        /// 清除名单缓存
        /// </summary>
        /// <returns></returns>
        public async Task ClearRosterListCacheAsync(RosterType rosterType, string api, Policy policy, string policyKey)
        {
            var key = FromatRosterKey(rosterType, api, policy, policyKey);
            await _db.KeyDeleteAsync(key);
        }

        private string FromatRosterKey(RosterType rosterType, string api, Policy policy, string policyKey)
        {
            var key = $"{_options.CacheKeyPrefix}:{rosterType.ToString().ToLower()}:{policy.ToString().ToLower()}";
            if (!string.IsNullOrEmpty(policyKey))
            {
                key += ":" + Common.EncryptMD5Short(policyKey);
            }
            key += ":" + api.ToLower();
            return key;
        }

        private string FromatApiRecordKey(string api, Policy policy, string policyKey, string policyValue)
        {
            var key = $"{_options.CacheKeyPrefix}:record:{policy.ToString().ToLower()}";
            if (!string.IsNullOrEmpty(policyKey))
            {
                key += ":" + Common.EncryptMD5Short(policyKey);
            }
            if (!string.IsNullOrEmpty(policyValue))
            {
                key += ":" + Common.EncryptMD5Short(policyValue);
            }
            key += ":" + api.ToLower();
            return key;
        }
    }
}
