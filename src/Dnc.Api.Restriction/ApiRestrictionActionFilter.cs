using Dnc.Api.Restriction.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dnc.Api.Restriction.Redis;

namespace Dnc.Api.Restriction
{
    public class ApiRestrictionActionFilter : IAsyncActionFilter, IAsyncPageFilter
    {
        private readonly RedisCacheProvider _redis;

        public ApiRestrictionActionFilter(RedisCacheProvider redis)
        {
            this._redis = redis;
        }

        private IEnumerable<ApiRestrictionAttribute> _attrs;

        private IEnumerable<ApiRestrictionAttribute> GetAttrs(FilterContext context)
        {
            if (_attrs != null)
            {
                return _attrs;
            }

            var method = context.GetHandlerMethod();
            if (method == null)
            {
                return new ApiRestrictionAttribute[] { };
            }

            _attrs = method.GetCustomAttributes<ApiRestrictionAttribute>(true);
            return _attrs;
        }

        private async Task aasas(IEnumerable<ApiRestrictionAttribute> attrs, FilterContext context)
        {
            var ip = IpToLong(context.HttpContext.GetUserIp());
            double now = DateTime.Now.Ticks;
            double min = DateTime.Now.AddMinutes(-5).Ticks;
            var keyPrefix = "ar:" + "aaaaa" + ":";
            foreach (var attr in attrs)
            {
                var key = keyPrefix;
                if (attr.RecognitionMethod == RecognitionMethod.Ip)
                {
                    key += "ip:" + ip;
                }

                //从Redis sorted set里面取得当前接口的历史数据
                long count = await _redis.SortedSetLengthAsync(key, min, now);


                //保存记录
                await _redis.SortedSetAddAsync(key, string.Empty, now);
            }








        }

        private static long IpToLong(string ip)
        {
            char[] separator = new char[] { '.' };
            string[] items = ip.Split(separator);
            return long.Parse(items[0]) << 24
                    | long.Parse(items[1]) << 16
                    | long.Parse(items[2]) << 8
                    | long.Parse(items[3]);
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await next();
        }

        public async Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
        {
            await Task.CompletedTask;
        }

        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            await next();
        }
    }
}
