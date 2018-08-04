using Dnc.Api.Throttle.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Dnc.Api.Throttle
{
    /// <summary>
    /// 配置项
    /// </summary>
    public class ApiThrottleOptions
    {
        /// <summary>
        /// 取得认证用户身份
        /// </summary>
        public Func<HttpContext, string> OnUserIdentity = context => context.GetDefaultUserIdentity();

        /// <summary>
        /// 取得客户端IP地址
        /// </summary>
        public Func<HttpContext, string> OnIpAddress = context => context.GetIpAddress();

        public Func<HttpContext, IActionResult> onIntercepted = context => { return new ApiThrottleResult { Content = "访问过于频繁，请稍后重试！" }; };

        internal IList<IApiThrottleOptionsExtension> Extensions { get; } = new List<IApiThrottleOptionsExtension>();

        public void AddExtension(IApiThrottleOptionsExtension extension)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension));
            }

            Extensions.Add(extension);
        }

    }
}
