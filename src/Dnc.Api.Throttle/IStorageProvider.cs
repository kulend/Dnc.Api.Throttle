using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dnc.Api.Throttle
{
    public interface IStorageProvider
    {
        #region 黑名单 & 白名单

        /// <summary>
        /// 添加名单
        /// </summary>
        /// <param name="rosterType">名单类型</param>
        /// <param name="policy">策略</param>
        /// <param name="policyKey">策略Key</param>
        /// <param name="expiry">过期时间</param>
        /// <param name="item">项目</param>
        /// <remarks>因为要保存过期时间，所以名单通过Redis 有序集合(sorted set)来存储，score来存储过期时间Ticks</remarks>
        Task AddRosterAsync(RosterType rosterType, string api, Policy policy, string policyKey, TimeSpan? expiry, params string[] item);

        /// <summary>
        /// 删除名单中数据
        /// </summary>
        /// <param name="rosterType">名单类型</param>
        /// <param name="api">API</param>
        /// <param name="policy">策略</param>
        /// <param name="policyKey">策略Key</param>
        /// <param name="expiry">过期时间</param>
        /// <param name="item">项目</param>
        Task RemoveRosterAsync(RosterType rosterType, string api, Policy policy, string policyKey, params string[] item);

        /// <summary>
        /// 取得名单列表（分页）
        /// </summary>
        /// <param name="rosterType">名单类型</param>
        /// <param name="api">API</param>
        /// <param name="policy">策略</param>
        /// <param name="policyKey">策略Key</param>
        Task<(long count, IEnumerable<ListItem> items)> GetRosterListAsync(RosterType rosterType, string api, Policy policy, string policyKey, long skip, long take);

        /// <summary>
        /// 取得名单列表
        /// </summary>
        /// <param name="rosterType">名单类型</param>
        /// <param name="api">API</param>
        /// <param name="policy">策略</param>
        /// <param name="policyKey">策略Key</param>
        Task<IEnumerable<ListItem>> GetRosterListAsync(RosterType rosterType, string api, Policy policy, string policyKey);

        #endregion
    }
}
