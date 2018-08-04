using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Dnc.Api.Throttle.Redis.Storage
{
    internal class RedisStorageDatabaseProvider : RedisDatabaseProvider, IRedisStorageDatabaseProvider
    {
        public RedisStorageDatabaseProvider(IOptions<RedisStorageOptions> options):base(options?.Value.ConnectionString)
        {
        }
    }

    internal interface IRedisStorageDatabaseProvider : IRedisDatabaseProvider
    {

    }
}
