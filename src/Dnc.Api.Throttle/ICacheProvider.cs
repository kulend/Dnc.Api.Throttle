using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dnc.Api.Throttle
{
    public interface ICacheProvider
    {
        /// <summary>
        /// 取得计时时间段api调用次数
        /// </summary>
        Task<long> GetApiRecordCountAsync(string api, Policy policy, string policyKey, string policyValue, DateTime now, int duration);

        /// <summary>
        /// 新增调用记录
        /// </summary>
        Task AddApiRecordAsync(string api, Policy policy, string policyKey, string policyValue, DateTime now, int duration);

        /// <summary>
        /// 取得名单列表
        /// </summary>
        Task<IEnumerable<ListItem>> GetRosterListAsync(RosterType rosterType, string api, Policy policy, string policyKey);

        /// <summary>
        /// 清除名单列表缓存
        /// </summary>
        /// <returns></returns>
        Task ClearRosterListCacheAsync(RosterType rosterType, string api, Policy policy, string policyKey);
    }
}
