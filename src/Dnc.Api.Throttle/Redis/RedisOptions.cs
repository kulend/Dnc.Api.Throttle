using System;
using System.Collections.Generic;
using System.Text;

namespace Dnc.Api.Throttle.Redis
{
    public class RedisOptions
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString { set; get; }

        /// <summary>
        /// Key前缀
        /// </summary>
        public string KeyPrefix { set; get; } = "ApiThrottle";
    }
}
