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
        private readonly ApiThrottleOption _options;

        public ApiThrottleActionFilter(ICacheProvider cache, IOptions<ApiThrottleOption> options)
        {
            _cache = cache;
            _options = options.Value;
        }

        private IEnumerable<ApiThrottleAttribute> _attrs;
        private string _actionName = null;
        private string _apiKey = null;

        /// <summary>
        /// 处理接口
        /// </summary>
        /// <returns></returns>
        private async Task<bool> HandleAsync(FilterContext context)
        {
            //预处理数据
            var method = context.GetHandlerMethod();

            _actionName = method.DeclaringType.FullName + "." + method.Name;

            _attrs = method.GetCustomAttributes<ApiThrottleAttribute>(true);

            _apiKey = $"{_options.RedisKeyPrefix}:{_actionName}";

            //检查是否过载
            var isValid =  await CheckAsync(context);
            if (isValid)
            {
                //保存记录
                await SaveAsync(context);
            }

            return isValid;
        }

        /// <summary>
        /// 检查过载
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CheckAsync(FilterContext context)
        {
            //黑名单检查
            foreach (Policy policy in Enum.GetValues(typeof(Policy)))
            {
                var bl = await _cache.GetBlackListAsync(policy);
                //取得识别值
                var policyValue = context.GetPolicyValue(policy, _options);
                if (bl.Contains(policyValue))
                {
                    return false;
                }
            }

            //白名单检查
            foreach (Policy policy in Enum.GetValues(typeof(Policy)))
            {
                var wl = await _cache.GetWhiteListAsync(policy);
                //取得识别值
                var policyValue = context.GetPolicyValue(policy, _options);
                if (wl.Contains(policyValue))
                {
                    return true;
                }
            }

            //循环验证是否过载
            foreach (var attr in _attrs)
            {
                var isValid = await CheckItemAsync(context, attr);
                if (!isValid)
                {
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> CheckItemAsync(FilterContext context, ApiThrottleAttribute attr)
        {
            if (attr.Duration <= 0 || attr.Limit <= 0)
            {
                //不限流
                return true;
            }
            //取得识别值
            var policyValue = context.GetPolicyValue(attr.Policy, _options);

            //判断是否过载
            long count = await _cache.GetValidApiRecordCount(_apiKey, attr.Policy, policyValue, DateTime.Now, attr.Duration);
            return count < attr.Limit;
        }

        /// <summary>
        /// 保存api调用记录
        /// </summary>
        /// <returns></returns>
        private async Task SaveAsync(FilterContext context)
        {
            DateTime nowTime = DateTime.Now;

            //循环保存记录
            foreach (var attr in _attrs)
            {
                //取得识别值
                var policyValue = context.GetPolicyValue(attr.Policy, _options);

                //保存记录
                await _cache.SaveApiRecordAsync(_apiKey, attr.Policy, policyValue, nowTime, attr.Duration);
            }
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var result = await HandleAsync(context);
            if (result)
            {
                await next();
            }
            else
            {
                context.Result = new ApiThrottleResult { Content = "访问过于频繁，请稍后重试"};
            }
        }

        public async Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
        {
            await Task.CompletedTask;
        }

        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            var result = await HandleAsync(context);
            if (result)
            {
                await next();
            }
            else
            {
                context.Result = new ApiThrottleResult { Content = "访问过于频繁，请稍后重试" };
            }
        }
    }
}
