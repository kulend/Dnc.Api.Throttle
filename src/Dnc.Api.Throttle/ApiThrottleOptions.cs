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

        public Func<HttpContext, Valve, IntercepteWhere, IActionResult> onIntercepted = (context, valve, where) => { return new ApiThrottleResult { Content = "访问过于频繁，请稍后重试！" }; };

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
        /// 全局阀门
        /// </summary>
        internal IList<Valve> Valves { set; get; } = new List<Valve>();

        /// <summary>
        /// 添加阀门
        /// </summary>
        public void AddValves(params Valve[] valves)
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

                if (valve is RosterValve roster)
                {
                    //名单阀门
                    //判断是否重复添加阀门
                    if (roster.Policy == Policy.Ip || roster.Policy == Policy.UserIdentity)
                    {
                        if (!Valves.Any(x => x.GetType() == roster.GetType() &&  x.Policy == roster.Policy))
                        {
                            Valves.Add(valve);
                        }
                    }
                    else
                    {
                        if (!Valves.Any(x => x.GetType() == roster.GetType() && x.Policy == valve.Policy && string.Equals(x.PolicyKey, valve.PolicyKey)))
                        {
                            Valves.Add(valve);
                        }
                    }
                }
                else
                {
                    //速率阀门
                    Valves.Add(valve);
                }
            }
        }
    }
}
