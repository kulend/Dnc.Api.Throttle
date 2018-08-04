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
        private IEnumerable<ApiThrottleAttribute> _attrs;

        /// <summary>
        /// 处理接口
        /// </summary>
        /// <returns></returns>
        private async Task<bool> HandleAsync(FilterContext context)
        {
            //预处理数据
            var method = context.GetHandlerMethod();

            _api = method.DeclaringType.FullName + "." + method.Name;

            _attrs = method.GetCustomAttributes<ApiThrottleAttribute>(true);

            //检查是否过载
            var isValid =  await CheckAsync(context);
            if (isValid)
            {
                context.HttpContext.Request.Headers[Common.HeaderStatusKey] = "1";
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
            ////黑名单检查
            //foreach (Policy policy in Enum.GetValues(typeof(Policy)))
            //{
            //    if (policy == Policy.Header || policy == Policy.Query)
            //    {
            //        continue;
            //    }
            //    var bl = await _cache.GetBlackListAsync(policy);
            //    //取得识别值
            //    var policyValue = context.GetPolicyValue(_options, policy, null);
            //    if (!string.IsNullOrEmpty(policyValue) && bl.Any(x => string.Equals(x.Value, policyValue)))
            //    {
            //        return false;
            //    }
            //}

            ////白名单检查
            //foreach (Policy policy in Enum.GetValues(typeof(Policy)))
            //{
            //    if (policy == Policy.Header || policy == Policy.Query)
            //    {
            //        continue;
            //    }
            //    var wl = await _cache.GetWhiteListAsync(policy);
            //    //取得识别值
            //    var policyValue = context.HttpContext.GetPolicyValue(_options, policy, null);
            //    if (!string.IsNullOrEmpty(policyValue) && wl.Any(x => string.Equals(x.Value, policyValue)))
            //    {
            //        return true;
            //    }
            //}

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
            var policyValue = context.HttpContext.GetPolicyValue(_options, attr.Policy, attr.PolicyKey);
            if (string.IsNullOrEmpty(policyValue))
            {
                return attr.WhenNull == WhenNull.Pass;
            }
            //判断是否过载
            long count = await _cache.GetValidApiRecordCount(_api, attr.Policy, policyValue, DateTime.Now, attr.Duration);
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
                var policyValue = context.HttpContext.GetPolicyValue(_options, attr.Policy, attr.PolicyKey);

                //保存记录
                await _cache.SaveApiRecordAsync(_api, attr.Policy, policyValue, nowTime, attr.Duration);
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
                context.Result = _options.onIntercepted(context.HttpContext, IntercepteWhere.ActionFilter);
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

                context.Result = _options.onIntercepted(context.HttpContext, IntercepteWhere.PageFilter);
            }
        }
    }
}
