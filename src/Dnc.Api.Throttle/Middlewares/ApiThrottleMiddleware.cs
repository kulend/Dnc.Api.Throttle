using Dnc.Api.Throttle.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Dnc.Api.Throttle.Middlewares
{
    /// <summary>
    /// Api限流中间件
    /// </summary>
    public class ApiThrottleMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// 缓存工具
        /// </summary>
        private readonly ICacheProvider _cache;

        /// <summary>
        /// 配置
        /// </summary>
        private readonly ApiThrottleOptions _options;

        private DateTime nowTime;

        public ApiThrottleMiddleware(RequestDelegate next, ICacheProvider cache, IOptions<ApiThrottleOptions> options)
        {
            _next = next;
            _cache = cache;
            _options = options.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            nowTime = DateTime.Now;

            //check
            var result = await CheckAsync(context);
            if (result.result)
            {
                //在Request的header中存放状态标记
                context.Request.Headers[Common.HeaderStatusKey] = "1";

                await _next.Invoke(context);

                //因为考虑到性能问题，全局valve暂时去掉RateValve的添加
                //var status = context.GetHeaderValue(Common.HeaderStatusKey);
                //if ("1".Equals(status))
                //{
                //    //保存
                //    await SaveAsync(context);
                //}
            }
            else
            {
                context.Request.Headers[Common.HeaderStatusKey] = "0";
                IActionResult actionResult =  _options.onIntercepted(context, result.valve, IntercepteWhere.Middleware);
                ActionContext c = new ActionContext(httpContext:context, routeData: new RouteData(), actionDescriptor: new ActionDescriptor());
                await actionResult.ExecuteResultAsync(c);
            }
        }

        private async Task<(bool result, Valve valve)> CheckAsync(HttpContext context)
        {
            //全局阀门
            foreach (var valve in _options.Global.Valves.OrderByDescending(x => x.Priority))
            {
                //取得识别值
                var policyValue = context.GetPolicyValue(_options, valve.Policy, valve.PolicyKey);
                //识别值为空时处理
                if (string.IsNullOrEmpty(policyValue))
                {
                    if (valve.WhenNull == WhenNull.Pass)
                    {
                        continue;
                    }
                    else
                    {
                        return (false, valve);
                    }
                }

                if (valve is BlackListValve)
                {
                    //黑名单
                    var wl = await _cache.GetRosterListAsync(RosterType.BlackList, Common.GlobalApiKey, valve.Policy, valve.PolicyKey);
                    if (wl.Any(x => string.Equals(x.Value, policyValue)))
                    {
                        return (false, valve);
                    }
                }
                else if (valve is WhiteListValve)
                {
                    //白名单
                    var wl = await _cache.GetRosterListAsync(RosterType.WhiteList, Common.GlobalApiKey, valve.Policy, valve.PolicyKey);
                    if (wl.Any(x => string.Equals(x.Value, policyValue)))
                    {
                        return (true, null);
                    }
                }
                else if(valve is RateValve rateValve)
                {
                    //速率阀门
                    if (rateValve.Duration <= 0 || rateValve.Limit <= 0)
                    {
                        //不限流
                        continue;
                    }
                    //判断是否过载
                    long count = await _cache.GetApiRecordCountAsync(Common.GlobalApiKey, rateValve.Policy, rateValve.PolicyKey, policyValue, DateTime.Now, rateValve.Duration);
                    if (count >= rateValve.Limit)
                    {
                        return (false, valve);
                    }
                }
            }

            return (true, null);
        }

        /// <summary>
        /// 保存api调用记录
        /// </summary>
        /// <returns></returns>
        private async Task SaveAsync(HttpContext context)
        {
            //循环保存记录
            foreach (RateValve valve in _options.Global.Valves.Where(x => x is RateValve))
            {
                //取得识别值
                var policyValue = context.GetPolicyValue(_options, valve.Policy, valve.PolicyKey);

                //保存记录
                await _cache.AddApiRecordAsync(Common.GlobalApiKey, valve.Policy, valve.PolicyKey, policyValue, nowTime, valve.Duration);
            }
        }
    }
}
