using Dnc.Api.Throttle.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public Func<HttpContext, IntercepteWhere, IActionResult> onIntercepted = (context, where) => { return new ApiThrottleResult { Content = "访问过于频繁，请稍后重试！" }; };

        internal IList<IApiThrottleOptionsExtension> Extensions { get; } = new List<IApiThrottleOptionsExtension>();

        public void AddExtension(IApiThrottleOptionsExtension extension)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension));
            }

            Extensions.Add(extension);
        }

        /// <summary>
        /// 全局配置
        /// </summary>
        public GlobalOptions Global { set; get; }

        public ApiThrottleOptions()
        {
            Global = new GlobalOptions();
        }
    }

    public class GlobalOptions
    {
        /// <summary>
        /// 全局频率阀门
        /// </summary>
        public IList<RateValve> Valves { set; get; } = new List<RateValve>();

        /// <summary>
        /// 全局黑名单阀门
        /// </summary>
        internal IList<BlackListValve> BlackListValves { set; get; } = new List<BlackListValve>();

        /// <summary>
        /// 全局白名单阀门
        /// </summary>
        internal IList<WhiteListValve> WhiteListValves { set; get; } = new List<WhiteListValve>();

        /// <summary>
        /// 添加 黑名单阀门
        /// </summary>
        public void AddBlackListValve(params BlackListValve[] valves)
        {
            if (valves == null)
            {
                return;
            }
            foreach (var valve in valves)
            {
                if (valve == null)
                {
                    continue;
                }
                if (valve.Policy == Policy.Ip || valve.Policy == Policy.UserIdentity)
                {
                    if (!BlackListValves.Any(x => x.Policy == valve.Policy))
                    {
                        BlackListValves.Add(valve);
                    }
                }
                else
                {
                    if (!BlackListValves.Any(x => x.Policy == valve.Policy && string.Equals(x.PolicyKey, valve.PolicyKey)))
                    {
                        BlackListValves.Add(valve);
                    }
                }
            }
        }

        /// <summary>
        /// 添加 白名单阀门
        /// </summary>
        public void AddWhiteListValve(params WhiteListValve[] valves)
        {
            if (valves == null)
            {
                return;
            }
            foreach (var valve in valves)
            {
                if (valve == null)
                {
                    continue;
                }
                if (valve.Policy == Policy.Ip || valve.Policy == Policy.UserIdentity)
                {
                    if (!BlackListValves.Any(x => x.Policy == valve.Policy))
                    {
                        WhiteListValves.Add(valve);
                    }
                }
                else
                {
                    if (!BlackListValves.Any(x => x.Policy == valve.Policy && string.Equals(x.PolicyKey, valve.PolicyKey)))
                    {
                        WhiteListValves.Add(valve);
                    }
                }
            }
        }

    }
}
