using System;
using System.Collections.Generic;
using System.Text;

namespace Dnc.Api.Throttle.Redis.Storage
{
    public class RedisStorageOptions : RedisOptions
    {
        internal bool SameWithCache { set; get; } = false;

        /// <summary>
        /// Redis Key Prefix
        /// </summary>
        internal string StorageKeyPrefix {
            get 
            {
                return  KeyPrefix + ":storage";
            }
        }
    }
}
