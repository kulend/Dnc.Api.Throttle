using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnc.Api.Throttle.Redis.Storage
{
    internal class RedisStorageProvider : IStorageProvider
    {
        /// <summary>
        /// The cache.
        /// </summary>
        private readonly IDatabase _db;

        /// <summary>
        /// The db provider.
        /// </summary>
        private readonly IRedisStorageDatabaseProvider _dbProvider;

        private readonly RedisStorageOptions _options;

        public RedisStorageProvider(IRedisStorageDatabaseProvider dbProvider, IOptions<RedisStorageOptions> options)
        {
            _dbProvider = dbProvider;
            _db = _dbProvider.GetDatabase();
            _options = options.Value;
        }

        /// <summary>
        /// 添加名单
        /// </summary>
        /// <param name="rosterType">名单类型</param>
        /// <param name="policy">策略</param>
        /// <param name="policyKey">策略Key</param>
        /// <param name="expiry">过期时间</param>
        /// <param name="item">项目</param>
        /// <remarks>因为要保存过期时间，所以名单通过Redis 有序集合(sorted set)来存储，score来存储过期时间Ticks</remarks>
        public async Task AddRosterAsync(RosterType rosterType, string api, Policy policy, string policyKey, TimeSpan? expiry, params string[] item)
        {
            if (item == null || item.Length == 0)
            {
                return;
            }

            //过期时间计算
            double score = expiry == null ? double.PositiveInfinity : DateTime.Now.Add(expiry.Value).Ticks;

            SortedSetEntry[] values = new SortedSetEntry[item.Length];
            for (int i = 0; i < item.Length; i++)
            {
                values[i] = new SortedSetEntry(item[i], score);
            }
            var key = FromatRosterKey(rosterType, api, policy, policyKey);
            //保存
            await _db.SortedSetAddAsync(key, values);

            //删除过期名单数据
            await _db.SortedSetRemoveRangeByScoreAsync(key, 0, DateTime.Now.Ticks, Exclude.Both);
        }

        /// <summary>
        /// 删除名单中数据
        /// </summary>
        /// <param name="rosterType">名单类型</param>
        /// <param name="api">API</param>
        /// <param name="policy">策略</param>
        /// <param name="policyKey">策略Key</param>
        /// <param name="expiry">过期时间</param>
        /// <param name="item">项目</param>
        public async Task RemoveRosterAsync(RosterType rosterType, string api, Policy policy, string policyKey, params string[] item)
        {
            if (item == null || item.Length == 0)
            {
                return;
            }
            //删除数据
            RedisValue[] delValues = new RedisValue[item.Length];
            for (int i = 0; i < item.Length; i++)
            {
                delValues[i] = item[i];
            }
            var key = FromatRosterKey(rosterType, api, policy, policyKey);

            //删除
            await _db.SortedSetRemoveAsync(key, delValues);
        }

        /// <summary>
        /// 取得名单列表（分页）
        /// </summary>
        /// <param name="rosterType">名单类型</param>
        /// <param name="api">API</param>
        /// <param name="policy">策略</param>
        /// <param name="policyKey">策略Key</param>
        public async Task<(long count, IEnumerable<ListItem> items)> GetRosterListAsync(RosterType rosterType, string api, Policy policy, string policyKey, long skip, long take)
        {
            var key = FromatRosterKey(rosterType, api, policy, policyKey);
            //取得件数
            var count = await _db.SortedSetLengthAsync(key, DateTime.Now.Ticks, double.PositiveInfinity, Exclude.Both);

            if (count == 0)
            {
                return (0, new List<ListItem>());
            }

            //取得数据
            var data = await _db.SortedSetRangeByScoreWithScoresAsync(key, start: DateTime.Now.Ticks, stop: double.PositiveInfinity, exclude: Exclude.Both, order: Order.Ascending, skip: skip, take: take);

            return (count, data.Select(x => new ListItem { Value = (string)x.Element, ExpireTicks = x.Score }));
        }

        /// <summary>
        /// 取得名单列表
        /// </summary>
        /// <param name="rosterType">名单类型</param>
        /// <param name="api">API</param>
        /// <param name="policy">策略</param>
        /// <param name="policyKey">策略Key</param>
        public async Task<IEnumerable<ListItem>> GetRosterListAsync(RosterType rosterType, string api, Policy policy, string policyKey)
        {
            var key = FromatRosterKey(rosterType, api, policy, policyKey);

            //取得数据
            var data = await _db.SortedSetRangeByScoreWithScoresAsync(key, start: DateTime.Now.Ticks, stop: double.PositiveInfinity, exclude: Exclude.Both, order: Order.Ascending);

            return data.Select(x => new ListItem { Value = (string)x.Element, ExpireTicks = x.Score });
        }

        private string FromatRosterKey(RosterType rosterType, string api, Policy policy, string policyKey)
        {
            var key = $"{_options.StorageKeyPrefix}:{rosterType.ToString().ToLower()}:{policy.ToString().ToLower()}";
            if (!string.IsNullOrEmpty(policyKey))
            {
                key += ":" + Common.EncryptMD5Short(policyKey);
            }
            key += ":" + api.ToLower();
            return key;
        }
    }

}
