using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dnc.Api.Throttle
{
    public interface IApiThrottleService
    {
        /// <summary>
        /// 添加黑名单
        /// </summary>
        /// <param name="policy">策略</param>
        /// <param name="expiry">过期时间</param>
        /// <param name="item">项目</param>
        Task AddBlackListAsync(Policy policy, TimeSpan? expiry, params string[] item);

        /// <summary>
        /// 添加白名单
        /// </summary>
        /// <param name="expiry">过期时间</param>
        /// <param name="ip">IP地址</param>
        Task AddWhiteListAsync(Policy policy, TimeSpan? expiry, params string[] item);

        /// <summary>
        /// 移除黑名单
        /// </summary>
        /// <param name="policy">策略</param>
        /// <param name="item">项目</param>
        Task RemoveBlackListAsync(Policy policy, params string[] item);

        /// <summary>
        /// 移除白名单
        /// </summary>
        /// <param name="policy">策略</param>
        /// <param name="item">项目</param>
        Task RemoveWhiteListAsync(Policy policy, params string[] item);

        /// <summary>
        /// 取得黑名单列表（分页）
        /// </summary>
        Task<(long count, IEnumerable<ListItem> items)> GetBlackListAsync(Policy policy, long skip, long take);

        /// <summary>
        /// 取得黑名单列表
        /// </summary>
        Task<IEnumerable<ListItem>> GetBlackListAsync(Policy policy);

        /// <summary>
        /// 取得白名单列表（分页）
        /// </summary>
        Task<(long count, IEnumerable<ListItem> items)> GetWhiteListAsync(Policy policy, long skip, long take);

        /// <summary>
        /// 取得白名单列表
        /// </summary>
        Task<IEnumerable<ListItem>> GetWhiteListAsync(Policy policy);
    }
}
