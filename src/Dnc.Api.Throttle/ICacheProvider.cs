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
        Task<long> GetValidApiRecordCount(string apikey, Policy policy, string policyValue, DateTime now, int duration);

        /// <summary>
        /// 保存调用记录
        /// </summary>
        Task SaveApiRecordAsync(string apikey, Policy policy, string policyValue, DateTime now, int duration);

        /// <summary>
        /// 取得黑名单列表
        /// </summary>
        Task<IEnumerable<string>> GetBlackListAsync(Policy policy);

        /// <summary>
        /// 取得白名单列表
        /// </summary>
        Task<IEnumerable<string>> GetWhiteListAsync(Policy policy);
    }
}
