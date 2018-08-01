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

        /// <summary>
        /// 保存Ip黑名单
        /// </summary>
        /// <param name="expiry">过期时间</param>
        /// <param name="ip">IP地址</param>
        /// <remarks>因为要保存过期时间，所以ip名单通过Redis 有序集合(sorted set)来存储，score来存储过期时间Ticks</remarks>
        public async Task SaveIpBlackListAsync(TimeSpan? expiry, params string[] ip)
        {
            if (ip == null || ip.Length == 0)
            {
                return;
            }

            //过期时间计算
            double score = expiry == null ? DateTime.MaxValue.Ticks : DateTime.Now.Add(expiry.Value).Ticks;

            SortedSetEntry[] values = new SortedSetEntry[ip.Length];
            for (int i = 0; i < ip.Length; i++)
            {
                values[i] = new SortedSetEntry(ip[i], score);
            }
            await _cache.SortedSetAddAsync($"{_options.RedisKeyPrefix}:bl:ip", values);

            //删除过期黑名单数据
            await _cache.SortedSetRemoveRangeByScoreAsync($"{_options.RedisKeyPrefix}:bl:ip", 0, DateTime.Now.Ticks, Exclude.Both);
        }

        /// <summary>
        /// 删除IP黑名单中数据
        /// </summary>
        /// <param name="ip">IP地址</param>
        public async Task RemoveIpBlackListAsync(params string[] ip)
        {
            if (ip == null || ip.Length == 0)
            {
                return;
            }

            //删除白名单中数据
            RedisValue[] delValues = new RedisValue[ip.Length];
            for (int i = 0; i < ip.Length; i++)
            {
                delValues[i] = ip[i];
            }
            await _cache.SortedSetRemoveAsync($"{_options.RedisKeyPrefix}:bl:ip", delValues);
        }

        /// <summary>
        /// 保存Ip白名单
        /// </summary>
        /// <param name="expiry">过期时间</param>
        /// <param name="ip">IP地址</param>
        /// <remarks>因为要保存过期时间，所以ip名单通过Redis 有序集合(sorted set)来存储，score来存储过期时间Ticks</remarks>
        public async Task SaveIpWhiteListAsync(TimeSpan? expiry, params string[] ip)
        {
            if (ip == null || ip.Length == 0)
            {
                return;
            }

            //过期时间计算
            double score = expiry == null ? DateTime.MaxValue.Ticks : DateTime.Now.Add(expiry.Value).Ticks;

            SortedSetEntry[] values = new SortedSetEntry[ip.Length];
            for (int i = 0; i < ip.Length; i++)
            {
                values[i] = new SortedSetEntry(ip[i], score);
            }
            await _cache.SortedSetAddAsync($"{_options.RedisKeyPrefix}:wl:ip", values);

            //删除过期白名单数据
            await _cache.SortedSetRemoveRangeByScoreAsync($"{_options.RedisKeyPrefix}:wl:ip", 0, DateTime.Now.Ticks, Exclude.Both);
        }

        /// <summary>
        /// 删除IP白名单中数据
        /// </summary>
        /// <param name="ip">IP地址</param>
        public async Task RemoveIpWhiteListAsync(params string[] ip)
        {
            if (ip == null || ip.Length == 0)
            {
                return;
            }

            //删除白名单中数据
            RedisValue[] delValues = new RedisValue[ip.Length];
            for (int i = 0; i < ip.Length; i++)
            {
                delValues[i] = ip[i];
            }
            await _cache.SortedSetRemoveAsync($"{_options.RedisKeyPrefix}:wl:ip", delValues);
        }
    }

}
