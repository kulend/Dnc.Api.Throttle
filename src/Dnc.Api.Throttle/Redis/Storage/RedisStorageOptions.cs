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
        public string StorageKeyPrefix {
            get 
            {
                return SameWithCache ? KeyPrefix : KeyPrefix + ":storage";
            }
        }
    }
}
