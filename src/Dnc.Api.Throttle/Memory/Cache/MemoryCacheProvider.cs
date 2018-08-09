using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dnc.Api.Throttle.Memory.Cache
{
    internal class MemoryCacheProvider : ICacheProvider
    {
        private readonly IMemoryCache _cache;
        private readonly MemoryCacheOptions _options;

        public MemoryCacheProvider(IMemoryCache cache, IOptions<MemoryCacheOptions> options)
        {
            _cache = cache;
            _options = options.Value;
        }

        public Task AddApiRecordAsync(string api, Policy policy, string policyKey, string policyValue, DateTime now, int duration)
        {
            throw new NotImplementedException();
        }

        public Task<long> GetApiRecordCountAsync(string api, Policy policy, string policyKey, string policyValue, DateTime now, int duration)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ListItem>> GetRosterListAsync(RosterType rosterType, string api, Policy policy, string policyKey)
        {
            var key = FromatRosterKey(rosterType, api, policy, policyKey);

            var result = await Task.FromResult((IEnumerable<ListItem>)_cache.Get(key));

            throw new NotImplementedException();
        }

        public Task ClearRosterListCacheAsync(RosterType rosterType, string api, Policy policy, string policyKey)
        {
            throw new NotImplementedException();
        }

        private string FromatRosterKey(RosterType rosterType, string api, Policy policy, string policyKey)
        {
            var key = $"{_options.KeyPrefix}:{rosterType.ToString().ToLower()}:{policy.ToString().ToLower()}";
            if (!string.IsNullOrEmpty(policyKey))
            {
                key += ":" + Common.EncryptMD5Short(policyKey);
            }
            key += ":" + api.ToLower();
            return key;
        }
    }
}
