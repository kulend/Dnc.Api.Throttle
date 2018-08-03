using Dnc.Api.Throttle.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Dnc.Api.Throttle
{
    public class ApiThrottleOption
    {
        /// <summary>
        /// 取得认证用户身份
        /// </summary>
        public Func<HttpContext, string> OnUserIdentity = context => context.GetDefaultUserIdentity();

        /// <summary>
        /// 取得客户端IP地址
        /// </summary>
        public Func<HttpContext, string> OnIpAddress = context => context.GetIpAddress();

        public string RedisConnectionString { set; get; }

        public string RedisKeyPrefix { set; get; } = "ApiThrottle";

        /// <summary>
        /// 取得客户端IP地址
        /// </summary>
        public Func<HttpContext, IActionResult> onIntercepted = context => { return new ApiThrottleResult { Content = "访问过于频繁，请稍后重试！" }; };
    }
}
