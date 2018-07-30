using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dnc.Api.Restriction
{
    public interface ICacheProvider
    {
        Task<bool> SortedSetAddAsync(string key, string value, double score);

        Task<long> SortedSetLengthAsync(string key, double min, double max);

        Task<bool> KeyExpireAsync(string key, TimeSpan? expiry);
    }
}
