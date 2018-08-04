using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dnc.Api.Throttle.Internal
{
    internal class ApiThrottleService : IApiThrottleService
    {
        private readonly ICacheProvider _cache;
        private readonly IStorageProvider _storage;
        private readonly ApiThrottleOptions _options;

        public ApiThrottleService(ICacheProvider cache, IOptions<ApiThrottleOptions> options, IStorageProvider storage)
        {
            _cache = cache;
            _options = options.Value;
            _storage = storage;
        }

        #region 黑名单 & 白名单
        
        /// <summary>
        /// 添加黑名单
        /// </summary>
        /// <param name="policy">策略</param>
        /// <param name="expiry">过期时间</param>
        /// <param name="item">项目</param>
        public async Task AddBlackListAsync(Policy policy, TimeSpan? expiry, params string[] item)
        {
            //保存黑名单
            await _storage.SaveBlackListAsync(policy, expiry, item);

            //从白名单中移除
            await _storage.RemoveWhiteListAsync(policy, item);

            //清除缓存
            await _cache.ClearBlackListCacheAsync(policy);
            await _cache.ClearWhiteListCacheAsync(policy);
        }

        /// <summary>
        /// 添加白名单
        /// </summary>
        /// <param name="expiry">过期时间</param>
        /// <param name="item">项目</param>
        public async Task AddWhiteListAsync(Policy policy, TimeSpan? expiry, params string[] item)
        {
            //保存白名单
            await _storage.SaveWhiteListAsync(policy, expiry, item);

            //从黑名单中移除
            await _storage.RemoveBlackListAsync(policy, item);

            //清除缓存
            await _cache.ClearBlackListCacheAsync(policy);
            await _cache.ClearWhiteListCacheAsync(policy);
        }

        /// <summary>
        /// 移除黑名单
        /// </summary>
        /// <param name="policy">策略</param>
        /// <param name="item">项目</param>
        public async Task RemoveBlackListAsync(Policy policy, params string[] item)
        {
            //从黑名单中移除
            await _storage.RemoveBlackListAsync(policy, item);
            //清除缓存
            await _cache.ClearBlackListCacheAsync(policy);
        }

        /// <summary>
        /// 移除白名单
        /// </summary>
        /// <param name="policy">策略</param>
        /// <param name="item">项目</param>
        public async Task RemoveWhiteListAsync(Policy policy, params string[] item)
        {
            //从白名单中移除
            await _storage.RemoveWhiteListAsync(policy, item);
            //清除缓存
            await _cache.ClearWhiteListCacheAsync(policy);
        }

        /// <summary>
        /// 取得黑名单列表（分页）
        /// </summary>
        public async Task<(long count, IEnumerable<ListItem> items)> GetBlackListAsync(Policy policy, long skip, long take)
        {
            return await _storage.GetBlackListAsync(policy, skip, take);
        }

        /// <summary>
        /// 取得黑名单列表
        /// </summary>
        public async Task<IEnumerable<ListItem>> GetBlackListAsync(Policy policy)
        {
            return await _storage.GetBlackListAsync(policy);
        }

        /// <summary>
        /// 取得白名单列表（分页）
        /// </summary>
        public async Task<(long count, IEnumerable<ListItem> items)> GetWhiteListAsync(Policy policy, long skip, long take)
        {
            return await _storage.GetWhiteListAsync(policy, skip, take);
        }

        /// <summary>
        /// 取得白名单列表
        /// </summary>
        public async Task<IEnumerable<ListItem>> GetWhiteListAsync(Policy policy)
        {
            return await _storage.GetWhiteListAsync(policy);
        }

        #endregion

    }
}
