using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Dnc.Api.Throttle.Redis.Cache
{
    internal class RedisCacheDatabaseProvider : RedisDatabaseProvider, IRedisCacheDatabaseProvider
    {
        public RedisCacheDatabaseProvider(IOptions<RedisCacheOptions> options):base(options?.Value.ConnectionString)
        {
        }
    }

    internal interface IRedisCacheDatabaseProvider : IRedisDatabaseProvider
    {

    }
}
