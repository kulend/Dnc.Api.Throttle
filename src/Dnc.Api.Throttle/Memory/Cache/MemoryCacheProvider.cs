using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dnc.Api.Throttle.Memory.Cache
{
    internal class MemoryCacheProvider : ICacheProvider
    {
        public Task AddApiRecordAsync(string api, Policy policy, string policyKey, string policyValue, DateTime now, int duration)
        {
            throw new NotImplementedException();
        }

        public Task<long> GetApiRecordCountAsync(string api, Policy policy, string policyKey, string policyValue, DateTime now, int duration)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ListItem>> GetRosterListAsync(RosterType rosterType, string api, Policy policy, string policyKey)
        {

            throw new NotImplementedException();
        }

        public Task ClearRosterListCacheAsync(RosterType rosterType, string api, Policy policy, string policyKey)
        {
            throw new NotImplementedException();
        }
    }
}
