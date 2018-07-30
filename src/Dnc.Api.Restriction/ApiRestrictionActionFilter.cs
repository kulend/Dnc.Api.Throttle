using Dnc.Api.Restriction.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;

namespace Dnc.Api.Restriction
{
    public class ApiRestrictionActionFilter : IAsyncActionFilter, IAsyncPageFilter
    {
        private readonly ICacheProvider _cache;
        private readonly ApiRestrictionOption _options;

        public ApiRestrictionActionFilter(ICacheProvider cache, IOptions<ApiRestrictionOption> options)
        {
            _cache = cache;
            _options = options.Value;
        }

        private IEnumerable<ApiRestrictionAttribute> _attrs;
        private string _actionName;
        private string _ip;
        private string _keyPrefix;
        private string _userIdentity;

        /// <summary>
        /// 处理接口过载
        /// </summary>
        /// <returns></returns>
        private async Task<bool> HandleAsync(FilterContext context)
        {
            //处理数据
            GetData(context);

            //检查是否过载
            var isOverload =  await CheckAsync(context);
            if (isOverload)
            {
                return false;
            }
            else
            {
                //未过载
                //保存记录到Redis
                await SaveAsync();
                return true;
            }
        }

        private void GetData(FilterContext context)
        {
            var method = context.GetHandlerMethod();

            _actionName = method.DeclaringType.FullName + "." + method.Name;

            _attrs = method.GetCustomAttributes<ApiRestrictionAttribute>(true);

            _ip = IpToNum(context.HttpContext.GetUserIp());

            _keyPrefix = $"{_options.RedisKeyPrefix}:{_actionName}:";
        }

        /// <summary>
        /// 检查是否过载
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CheckAsync(FilterContext context)
        {
            DateTime nowTime = DateTime.Now;

            //循环验证是否过载
            foreach (var attr in _attrs)
            {
                string key = "";
                switch (attr.BasisCondition)
                {
                    case BasisCondition.Ip:
                        key = _keyPrefix + "ip:" + _ip;
                        break;
                    case BasisCondition.UserIdentity:
                        _userIdentity = _options.OnUserIdentity(context.HttpContext);
                        key = _keyPrefix + "user:" + _userIdentity;
                        break;
                    default:
                        break;
                }

                //从Redis sorted set里面取得当前接口的历史数据
                long count = await _cache.SortedSetLengthAsync(key, nowTime.Ticks - attr.Duration.Ticks, nowTime.Ticks);
                if (count >= attr.Limit)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 保持记录到redis
        /// </summary>
        /// <returns></returns>
        private async Task SaveAsync()
        {
            DateTime nowTime = DateTime.Now;

            //循环保存记录
            foreach (var attr in _attrs)
            {
                string key = "";
                switch (attr.BasisCondition)
                {
                    case BasisCondition.Ip:
                        key = _keyPrefix + "ip:" + _ip;
                        break;
                    case BasisCondition.UserIdentity:
                        key = _keyPrefix + "user:" + _userIdentity;
                        break;
                    default:
                        break;
                }

                //保存记录
                await _cache.SortedSetAddAsync(key, nowTime.Ticks.ToString(), nowTime.Ticks);

                //设置过期时间
                await _cache.KeyExpireAsync(key, attr.Duration.Add(TimeSpan.FromMinutes(1)));
            }
        }

        private static string IpToNum(string ip)
        {
            if (ip.Contains("."))
            {
                //IPv4
                char[] separator = new char[] { '.' };
                string[] items = ip.Split(separator);
                return (long.Parse(items[0]) << 24
                        | long.Parse(items[1]) << 16
                        | long.Parse(items[2]) << 8
                        | long.Parse(items[3])).ToString();
            }
            else
            {
                //IPv6
                IPAddress ipAddr = IPAddress.Parse(ip);
                List<Byte> ipFormat = ipAddr.GetAddressBytes().ToList();
                ipFormat.Reverse();
                ipFormat.Add(0);
                BigInteger ipAsInt = new BigInteger(ipFormat.ToArray());
                return ipAsInt.ToString();
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
                context.Result = new ApiRestrictionResult { Content = "访问过于频繁，请稍后重试"};
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
                context.Result = new ApiRestrictionResult { Content = "访问过于频繁，请稍后重试" };
            }
        }
    }
}
