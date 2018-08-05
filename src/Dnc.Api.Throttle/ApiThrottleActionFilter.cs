using Dnc.Api.Throttle.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;

namespace Dnc.Api.Throttle
{
    public class ApiThrottleActionFilter : IAsyncActionFilter, IAsyncPageFilter
    {
        private readonly ICacheProvider _cache;
        private readonly ApiThrottleOptions _options;

        public ApiThrottleActionFilter(ICacheProvider cache, ApiThrottleOptions options)
        {
            _cache = cache;
            _options = options;
        }

        //Api名称
        private string _api = null;
        private IEnumerable<RateValve> _valves;

        /// <summary>
        /// 处理接口
        /// </summary>
        /// <returns></returns>
        private async Task<(bool result, Valve valve)> HandleAsync(FilterContext context)
        {
            //预处理数据
            var method = context.GetHandlerMethod();

            _api = method.DeclaringType.FullName + "." + method.Name;

            _valves = method.GetCustomAttributes<RateValve>(true);

            //检查是否过载
            var result =  await CheckAsync(context);
            if (result.result)
            {
                context.HttpContext.Request.Headers[Common.HeaderStatusKey] = "1";
                //保存记录
                await SaveAsync(context);
            }
            else
            {
                context.HttpContext.Request.Headers[Common.HeaderStatusKey] = "0";
            }

            return result;
        }

        /// <summary>
        /// 检查过载
        /// </summary>
        /// <returns></returns>
        private async Task<(bool result, Valve valve)> CheckAsync(FilterContext context)
        {
            //循环验证是否过载
            foreach (var valve in _valves)
            {
                if (valve.Duration <= 0 || valve.Limit <= 0)
                {
                    //不限流
                    continue;
                }
                //取得识别值
                var policyValue = context.HttpContext.GetPolicyValue(_options, valve.Policy, valve.PolicyKey);
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
                //判断是否过载
                long count = await _cache.GetApiRecordCountAsync(_api, valve.Policy, valve.PolicyKey, policyValue, DateTime.Now, valve.Duration);
                if (count >= valve.Limit)
                {
                    return (false, valve);
                }
            }

            return (true, null);
        }

        /// <summary>
        /// 保存api调用记录
        /// </summary>
        /// <returns></returns>
        private async Task SaveAsync(FilterContext context)
        {
            DateTime nowTime = DateTime.Now;

            //循环保存记录
            foreach (var valve in _valves)
            {
                //取得识别值
                var policyValue = context.HttpContext.GetPolicyValue(_options, valve.Policy, valve.PolicyKey);

                //保存记录
                await _cache.AddApiRecordAsync(_api, valve.Policy, valve.PolicyKey, policyValue, nowTime, valve.Duration);
            }
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var result = await HandleAsync(context);
            if (result.result)
            {
                await next();
            }
            else
            {
                context.Result = _options.onIntercepted(context.HttpContext, result.valve, IntercepteWhere.ActionFilter);
            }
        }

        public async Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
        {
            await Task.CompletedTask;
        }

        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            var result = await HandleAsync(context);
            if (result.result)
            {
                await next();
            }
            else
            {
                context.Result = _options.onIntercepted(context.HttpContext, result.valve, IntercepteWhere.PageFilter);
            }
        }
    }
}
