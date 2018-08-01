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
        private readonly ApiThrottleOption _options;

        public ApiThrottleService(ICacheProvider cache, IOptions<ApiThrottleOption> options, IStorageProvider storage)
        {
            _cache = cache;
            _options = options.Value;
            _storage = storage;
        }

        /// <summary>
        /// 添加Ip黑名单
        /// </summary>
        /// <param name="expiry">过期时间</param>
        /// <param name="ip">IP地址</param>
        public async Task AddIpBlackListAsync(TimeSpan? expiry, params string[] ip)
        {
            if (ip == null || ip.Length == 0)
            {
                return;
            }

            //保存黑名单
            await _storage.SaveIpBlackListAsync(expiry, ip);

            //从白名单中移除
            await _storage.RemoveIpWhiteListAsync(ip);
        }

        /// <summary>
        /// 添加Ip白名单
        /// </summary>
        /// <param name="expiry">过期时间</param>
        /// <param name="ip">IP地址</param>
        public async Task AddIpWhiteListAsync(TimeSpan? expiry, params string[] ip)
        {
            if (ip == null || ip.Length == 0)
            {
                return;
            }

            //保存黑名单
            await _storage.SaveIpWhiteListAsync(expiry, ip);

            //从黑名单中移除
            await _storage.RemoveIpBlackListAsync(ip);
        }
    }
}
