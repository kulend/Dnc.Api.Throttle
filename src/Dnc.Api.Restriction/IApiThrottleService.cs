using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dnc.Api.Throttle
{
    public interface IApiThrottleService
    {
        /// <summary>
        /// 添加Ip黑名单
        /// </summary>
        /// <param name="expiry">过期时间</param>
        /// <param name="ip">IP地址</param>
        /// <remarks>会删除Ip白名单中相关数据</remarks>
        Task AddIpBlackListAsync(TimeSpan? expiry, params string[] ip);

        /// <summary>
        /// 添加Ip白名单
        /// </summary>
        /// <param name="expiry">过期时间</param>
        /// <param name="ip">IP地址</param>
        /// <remarks>会删除Ip黑名单中相关数据</remarks>
        Task AddIpWhiteListAsync(TimeSpan? expiry, params string[] ip);
    }
}
