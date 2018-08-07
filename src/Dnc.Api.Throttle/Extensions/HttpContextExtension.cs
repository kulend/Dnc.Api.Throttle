using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Security.Claims;

namespace Dnc.Api.Throttle.Extensions
{
    public static class HttpContextExtension
    {
        /// <summary>
        /// 取得客户端IP地址
        /// </summary>
        internal static string GetIpAddress(this HttpContext context)
        {
            var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ip))
            {
                ip = context.Connection.RemoteIpAddress.ToString();
            }
            return ip;
        }

        /// <summary>
        /// 取得默认用户Identity值
        /// </summary>
        internal static string GetDefaultUserIdentity(this HttpContext context)
        {
            return context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        /// <summary>
        /// 取得Header值
        /// </summary>
        internal static string GetHeaderValue(this HttpContext context, string key)
        {
            return context.Request.Headers[key].FirstOrDefault();
        }

        /// <summary>
        /// 取得Query值
        /// </summary>
        internal static string GetQueryValue(this HttpContext context, string key)
        {
            return context.Request.Query[key].FirstOrDefault();
        }

        /// <summary>
        /// 取得RequestPath
        /// </summary>
        internal static string GetRequestPath(this HttpContext context)
        {
            return context.Request.Path.Value;
        }

        /// <summary>
        /// 取得Cookie值
        /// </summary>
        internal static string GetCookieValue(this HttpContext context, string key)
        {
            context.Request.Cookies.TryGetValue(key, out string value);
            return value;
        }

        internal static string GetPolicyValue(this HttpContext context, ApiThrottleOptions options, Policy policy, string policyKey)
        {
            switch (policy)
            {
                case Policy.Ip:
                    return Common.IpToNum(options.OnIpAddress(context));
                case Policy.UserIdentity:
                    return options.OnUserIdentity(context);
                case Policy.Header:
                    return context.GetHeaderValue(policyKey);
                case Policy.Query:
                    return context.GetQueryValue(policyKey);
                case Policy.RequestPath:
                    return context.GetRequestPath();
                case Policy.Cookie:
                    return context.GetCookieValue(policyKey);
                default:
                    throw new ArgumentException("参数出错", "policy");
            }
        }
    }
}
