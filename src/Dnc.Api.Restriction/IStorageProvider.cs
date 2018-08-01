using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dnc.Api.Throttle
{
    public interface IStorageProvider
    {
        /// <summary>
        /// 保存Ip黑名单
        /// </summary>
        /// <param name="expiry">过期时间</param>
        /// <param name="ip">IP地址</param>
        Task SaveIpBlackListAsync(TimeSpan? expiry, params string[] ip);

        /// <summary>
        /// 删除IP黑名单中数据
        /// </summary>
        /// <param name="ip">IP地址</param>
        Task RemoveIpBlackListAsync(params string[] ip);

        /// <summary>
        /// 保存Ip白名单
        /// </summary>
        /// <param name="expiry">过期时间</param>
        /// <param name="ip">IP地址</param>
        /// <remarks>因为要保存过期时间，所以ip名单通过Redis 有序集合(sorted set)来存储，score来存储过期时间Ticks</remarks>
        Task SaveIpWhiteListAsync(TimeSpan? expiry, params string[] ip);

        /// <summary>
        /// 删除IP白名单中数据
        /// </summary>
        /// <param name="ip">IP地址</param>
        Task RemoveIpWhiteListAsync(params string[] ip);
    }
}
