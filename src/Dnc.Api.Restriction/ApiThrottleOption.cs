using Dnc.Api.Throttle.Redis;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dnc.Api.Throttle
{
    public class ApiThrottleOption
    {
        public Func<HttpContext, string> OnUserIdentity = context => "";

        public string RedisConnectionString { set; get; }

        public string RedisKeyPrefix { set; get; } = "ApiThrottle";
    }
}
