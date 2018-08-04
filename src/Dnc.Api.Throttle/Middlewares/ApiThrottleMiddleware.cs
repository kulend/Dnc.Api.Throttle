using Dnc.Api.Throttle.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public ApiThrottleMiddleware(RequestDelegate next, ICacheProvider cache, IOptions<ApiThrottleOptions> options)
        {
            _next = next;
            _cache = cache;
            _options = options.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            var result = await CheckAsync(context);
            if (result)
            {
                //在Request的header中存放状态标记
                context.Request.Headers[Common.HeaderStatusKey] = "1";

                await _next.Invoke(context);

                var status = context.GetHeaderValue(Common.HeaderStatusKey);
                if ("1".Equals(status))
                {
                    //保存
                    await SaveAsync(context);
                }
            }
            else
            {
                IActionResult actionResult =  _options.onIntercepted(context, IntercepteWhere.Middleware);
                ActionContext c = new ActionContext(httpContext:context, routeData: new RouteData(), actionDescriptor: new ActionDescriptor());
                await actionResult.ExecuteResultAsync(c);
            }
        }

        private async Task<bool> CheckAsync(HttpContext context)
        {
            //黑名单检查
            foreach (var valve in _options.Global.BlackListValves)
            {
                var wl = await _cache.GetBlackListAsync(valve.Policy);
                //取得识别值
                var policyValue = context.GetPolicyValue(_options, valve.Policy, valve.PolicyKey);
                if (!string.IsNullOrEmpty(policyValue) && wl.Any(x => string.Equals(x.Value, policyValue)))
                {
                    return false;
                }
            }

            //白名单检查
            foreach (var valve in _options.Global.WhiteListValves)
            {
                var wl = await _cache.GetWhiteListAsync(valve.Policy);
                //取得识别值
                var policyValue = context.GetPolicyValue(_options, valve.Policy, valve.PolicyKey);
                if (!string.IsNullOrEmpty(policyValue) && wl.Any(x => string.Equals(x.Value, policyValue)))
                {
                    return true;
                }
            }

            //全局阀门
            foreach (var valve in _options.Global.Valves)
            {
                if (valve.Duration <= 0 || valve.Limit <= 0)
                {
                    //不限流
                    continue;
                }

                //取得识别值
                var policyValue = context.GetPolicyValue(_options, valve.Policy, valve.PolicyKey);
                if (string.IsNullOrEmpty(policyValue))
                {
                    if (valve.WhenNull == WhenNull.Pass)
                    {
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }

                //判断是否过载
                long count = await _cache.GetValidApiRecordCount(Common.GlobalApiKey, valve.Policy, policyValue, DateTime.Now, valve.Duration);
                if (count >= valve.Limit)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 保存api调用记录
        /// </summary>
        /// <returns></returns>
        private async Task SaveAsync(HttpContext context)
        {
            DateTime nowTime = DateTime.Now;

            //循环保存记录
            foreach (var valve in _options.Global.Valves)
            {
                //取得识别值
                var policyValue = context.GetPolicyValue(_options, valve.Policy, valve.PolicyKey);

                //保存记录
                await _cache.SaveApiRecordAsync(Common.GlobalApiKey, valve.Policy, policyValue, nowTime, valve.Duration);
            }
        }
    }
}
