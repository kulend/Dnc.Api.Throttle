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
        /// 保存黑名单
        /// </summary>
        /// <param name="policy">策略</param>
        /// <param name="expiry">过期时间</param>
        /// <param name="item">项目</param>
        /// <remarks>因为要保存过期时间，所以名单通过Redis 有序集合(sorted set)来存储，score来存储过期时间Ticks</remarks>
        public async Task SaveBlackListAsync(Policy policy, TimeSpan? expiry, params string[] item)
        {
            await SaveWhiteOrBlackListAsync("bl", policy, expiry, item);
        }

        /// <summary>
        /// 保存白名单
        /// </summary>
        /// <param name="policy">策略</param>
        /// <param name="expiry">过期时间</param>
        /// <param name="item">项目</param>
        /// <remarks>因为要保存过期时间，所以ip名单通过Redis 有序集合(sorted set)来存储，score来存储过期时间Ticks</remarks>
        public async Task SaveWhiteListAsync(Policy policy, TimeSpan? expiry, params string[] item)
        {
            await SaveWhiteOrBlackListAsync("wl", policy, expiry, item);
        }

        private async Task SaveWhiteOrBlackListAsync(string listType, Policy policy, TimeSpan? expiry, params string[] item)
        {
            if (item == null || item.Length == 0)
            {
                return;
            }

            //过期时间计算
            double score = expiry == null ? DateTime.MaxValue.Ticks : DateTime.Now.Add(expiry.Value).Ticks;

            SortedSetEntry[] values = new SortedSetEntry[item.Length];
            for (int i = 0; i < item.Length; i++)
            {
                values[i] = new SortedSetEntry(item[i], score);
            }
            string key = $"{_options.StorageKeyPrefix}:{listType}:{policy.ToString().ToLower()}";
            //保存
            await _db.SortedSetAddAsync(key, values);

            //删除过期名单数据
            await _db.SortedSetRemoveRangeByScoreAsync(key, 0, DateTime.Now.Ticks, Exclude.Both);
        }

        /// <summary>
        /// 删除黑名单中数据
        /// </summary>
        public async Task RemoveBlackListAsync(Policy policy, params string[] item)
        {
            await RemoveWhiteOrBlackListAsync("bl", policy, item);
        }

        /// <summary>
        /// 删除白名单中数据
        /// </summary>
        public async Task RemoveWhiteListAsync(Policy policy, params string[] item)
        {
            await RemoveWhiteOrBlackListAsync("wl", policy, item);
        }

        private async Task RemoveWhiteOrBlackListAsync(string listType, Policy policy, params string[] item)
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

            string key = $"{_options.StorageKeyPrefix}:{listType}:{policy.ToString().ToLower()}";

            //删除
            await _db.SortedSetRemoveAsync(key, delValues);
        }

        /// <summary>
        /// 取得黑名单列表（分页）
        /// </summary>
        public async Task<(long count, IEnumerable<ListItem> items)> GetBlackListAsync(Policy policy, long skip, long take)
        {
            string key = $"{_options.StorageKeyPrefix}:bl:{policy.ToString().ToLower()}";
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
        /// 取得黑名单列表
        /// </summary>
        public async Task<IEnumerable<ListItem>> GetBlackListAsync(Policy policy)
        {
            string key = $"{_options.StorageKeyPrefix}:bl:{policy.ToString().ToLower()}";

            //取得数据
            var data = await _db.SortedSetRangeByScoreWithScoresAsync(key, start: DateTime.Now.Ticks, stop: double.PositiveInfinity, exclude: Exclude.Both, order: Order.Ascending);

            return data.Select(x => new ListItem { Value = (string)x.Element, ExpireTicks = x.Score });
        }

        /// <summary>
        /// 取得白名单列表（分页）
        /// </summary>
        public async Task<(long count, IEnumerable<ListItem> items)> GetWhiteListAsync(Policy policy, long skip, long take)
        {
            string key = $"{_options.StorageKeyPrefix}:wl:{policy.ToString().ToLower()}";
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
        /// 取得白名单列表
        /// </summary>
        public async Task<IEnumerable<ListItem>> GetWhiteListAsync(Policy policy)
        {
            string key = $"{_options.StorageKeyPrefix}:wl:{policy.ToString().ToLower()}";

            //取得数据
            var data = await _db.SortedSetRangeByScoreWithScoresAsync(key, start: DateTime.Now.Ticks, stop: double.PositiveInfinity, exclude: Exclude.Both, order: Order.Ascending);

            return data.Select(x => new ListItem { Value = (string)x.Element, ExpireTicks = x.Score });
        }
    }

}
