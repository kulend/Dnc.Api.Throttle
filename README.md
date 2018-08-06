# Dnc.Api.Throttle [![Build status](https://ci.appveyor.com/api/projects/status/8fj8htmkcudmel65?svg=true)](https://ci.appveyor.com/project/kulend/dnc-api-throttle) [![NuGet](https://img.shields.io/nuget/v/Nuget.Core.svg)](https://www.nuget.org/packages/Dnc.Api.Throttle) [![GitHub license](https://img.shields.io/github/license/kulend/Dnc.Api.Throttle.svg)](https://github.com/kulend/Dnc.Api.Throttle/blob/master/LICENSE)

## 适用于Dot Net Core的WebApi接口限流框架

**使用Dnc.Api.Throttle可以使您轻松实现WebApi接口的限流管理。Dnc.Api.Throttle支持IP、用户身份、Request Header、Request QueryString等多种限流策略，支持黑名单和白名单功能，支持全局拦截和单独Api拦截。**

**Dnc.Api.Throttle暂时只支持Redis作为缓存和存储库，后续会进行扩展。**

#### 开始使用

* 安装Dnc.Api.Throttle

    NuGet：` PM> Install-Package Dnc.Api.Throttle`

* 基本配置

    `Startup.cs`:
    ```c#
        public void ConfigureServices(IServiceCollection services)
        {
            //Api限流
            services.AddApiThrottle(options => {
                //配置redis
                //如果Cache和Storage使用同一个redis，则可以按如下配置
                options.UseRedisCacheAndStorage(opts => {
                    opts.ConnectionString = "localhost,connectTimeout=5000,allowAdmin=false,defaultDatabase=0";
                    //opts.KeyPrefix = "apithrottle"; //指定给所有key加上前缀，默认为apithrottle
                });
                //如果Cache和Storage使用不同redis库，可以按如下配置
                //options.UseRedisCache(opts => {
                //    opts.ConnectionString = "localhost,connectTimeout=5000,allowAdmin=false,defaultDatabase=0";
                //});
                //options.UseRedisStorage(opts => {
                //    opts.ConnectionString = "localhost,connectTimeout=5000,allowAdmin=false,defaultDatabase=1";
                //});
            });

            services.AddMvc(opts => {
                //这里添加ApiThrottleActionFilter拦截器
                opts.Filters.Add(typeof(ApiThrottleActionFilter));
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            ...

            //Api限流
            app.UseApiThrottle();

            app.UseMvc();
        }
    ```

* 给Api添加一个限流阀门（Valve）

    `ValuesController.cs`:
    ```c#
        // GET api/values
        [HttpGet]
        [RateValve(Policy = Policy.Ip, Limit = 10, Duration = 30)]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }
    ```
    以上特性代表给Get接口添加一个速率阀门，指定每个IP，30秒内最多调用10次该接口。

* 通过以上配置，最简单的一个接口限流就完成了。
    
    当Api被拦截时，接口不会执行，context.Result会返回一个 `new ApiThrottleResult { Content = "访问过于频繁，请稍后重试！" }`， ApiThrottleResult继承于ContentResult，你可以不继续处理，也可以在自己的ResultFilter中拦截ApiThrottleResult并处理。



#### 更多Valve范例

*   `[RateValve(Policy = Policy.UserIdentity, Limit = 1, Duration = 60)]`

    代表根据用户身份，每60秒可访问1次该接口。关于用户身份，默认是如下取得的：
    ```c#
        return context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    ```
    如果需要自定义，则可以在Startup.cs中如下配置：
    ```c#
           //Api限流
            services.AddApiThrottle(options => {
                ...
                options.OnUserIdentity = (httpContext) =>
                {
                    //这里根据自己需求返回用户唯一身份
                    return httpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                };
                ...
            });
    ```

*   `[RateValve(Policy = Policy.Header, PolicyKey = "hkey", Limit = 1, Duration = 30, WhenNull = WhenNull.Intercept)]`

    代表根据Request Header中hkey对应的值，每30秒可访问1次该接口。如果无法取得Header中的值或取得的值为空，则进行拦截。
    
    关于WhenNull：
    
    `WhenNull = WhenNull.Pass`：对应策略取得的识别值为空时，不进行拦截。

    `WhenNull = WhenNull.Intercept`：对应策略取得的识别值为空时，进行拦截。

*   `[RateValve(Policy = Policy.Query, PolicyKey = "mobile", Limit = 1, Duration = 30, WhenNull = WhenNull.Pass)]`

    代表根据Request Query中mobile对应的值，每30秒可访问1次该接口。如果无法取得识别值或取得的值为空，则不进行拦截。

*   `[BlackListValve(Policy = Policy.Query, PolicyKey =  "mobile")]`

    黑名单拦截，代表根据Request Query中mobile对应的值，如果在黑名单中，则进行拦截。关于如何添加黑名单，请参照后面关于IApiThrottleService部分。

*   `[WhiteListValve(Policy = Policy.Ip)]`

    白名单拦截，代表根据客户端IP地址，如果在白名单中，则不进行拦截（如果同一个Api上有多个Valve，按序当检查到白名单符合时，则代表检查通过，不进行后续Valve的拦截检查）。关于如何添加白名单，请参照后面关于IApiThrottleService部分。

*   一个Api多个Valve
    ```c#
        // POST api/values
        [HttpPost]
        [WhiteListValve(Policy = Policy.Ip, Priority = 3)]
        [BlackListValve(Policy = Policy.UserIdentity, Priority = 2)]
        [RateValve(Policy = Policy.Header, PolicyKey = "hkey", Limit = 1, Duration = 30, WhenNull = WhenNull.Pass)]
        public void Post([FromBody] string value)
        {
        }
    ```
    多个Valve根据Priority值从大到小进行拦截，如果被拦截，则不进行后续Valve拦截检查。


#### 全局限流配置

    以上都是对单个Api进行限流管理的，如果需要对全局进行限流管理，可在`Startup.cs`中进行如下配置：
    ```c#
           //Api限流
            services.AddApiThrottle(options => {
                ...
                options.Global.AddValves(new BlackListValve
                {
                    Policy = Policy.Ip,
                    Priority = 99
                }, new WhiteListValve
                {
                    Policy = Policy.UserIdentity,
                    Priority = 88
                },
                new BlackListValve
                {
                    Policy = Policy.Header,
                    PolicyKey = "throttle"
                }, new RateValve
                {
                    Policy = Policy.Ip,
                    Limit = 5,
                    Duration = 10,
                    WhenNull = WhenNull.Pass
                });
                ...
            });
    ```
    以上代表给全局添加了4个Valve进行拦截，如果被拦截，则不进行后续操作，白名单检查通过时，代表全局拦截通过，不进行后续全局Valve检查（后续单独Api的检查还会进行）。

    相同识别策略（Policy+PolicyKey）的Valve只能添加一个，重复不会添加。

    全局限流拦截在Middlewarez中进行，单独Api限流拦截在IAsyncActionFilter中进行，当然也支持Razor Page，在IAsyncPageFilterz中进行限流。


#### 其他自定义配置项

* 自定义IP地址取得方法：
    ```c#
           //Api限流
            services.AddApiThrottle(options => {
                ...
                //以下是 Dnc.Api.Throttle 默认取得Ip地址的方法，可进行自定义
                options.OnIpAddress = (context) => {
                    var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                    if (string.IsNullOrEmpty(ip))
                    {
                        ip = context.Connection.RemoteIpAddress.ToString();
                    }
                    return ip;
                };
                ...
            });
    ```

* 自定义拦截后处理：
    ```c#
           //Api限流
            services.AddApiThrottle(options => {
                ...
                options.onIntercepted = (context, valve, where) =>
                {
                    //valve：引发拦截的valve
                    //where：拦截发生的地方，有ActionFilter,PageFilter,Middleware(全局)
                    if (where == IntercepteWhere.Middleware)
                    {
                        //注意：Middleware中返回的ActionResult无法在ResultFilter中拦截处理。
                        return new JsonResult(new { code = 99, message = "访问过于频繁，请稍后重试！" });
                    }
                    else
                    {
                        return new ApiThrottleResult { Content = "访问过于频繁，请稍后重试！" };
                    }
                };
                ...
            });
    ```

### IApiThrottleService

**使用IApiThrottleService接口可实现黑名单、白名单的管理维护等其他功能。**

使用范例：

    ```c#
        /// <summary>
        /// Api限流管理服务
        /// </summary>
        private readonly IApiThrottleService _service;

        public ValuesController(IApiThrottleService service)
        {
            _service = service;
        }

        [HttpPost]
        [BlackListValve(Policy = Policy.Ip)]
        public async Task AddBlackList()
        {
            var ip = GetIpAddress(HttpContext);
            //添加IP黑名单
            await _service.AddRosterAsync(RosterType.BlackList, "WebApiTest.Controllers.ValuesController.AddBlackList", Policy.Ip, null, TimeSpan.FromSeconds(60), ip);
        }

        /// <summary>
        /// 取得客户端IP地址
        /// </summary>
        private static string GetIpAddress(HttpContext context)
        {
            var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ip))
            {
                ip = context.Connection.RemoteIpAddress.ToString();
            }
            return ip;
        }
    ```
    AddBlackList中针对WebApiTest.Controllers.ValuesController.AddBlackList方法添加了一个有效期60的IP黑名单，当前IP调用改接口会被IP黑名单拦截。


现有接口：
    ```c#
        #region 黑名单 & 白名单

        /// <summary>
        /// 添加名单
        /// </summary>
        /// <param name="rosterType">名单类型</param>
        /// <param name="api">Api</param>
        /// <param name="policy">策略</param>
        /// <param name="policyKey">策略Key</param>
        /// <param name="expiry">过期时间</param>
        /// <param name="item">项目</param>
        Task AddRosterAsync(RosterType rosterType, string api, Policy policy, string policyKey, TimeSpan? expiry, params string[] item);

        /// <summary>
        /// 删除名单中数据
        /// </summary>
        /// <param name="rosterType">名单类型</param>
        /// <param name="api">API</param>
        /// <param name="policy">策略</param>
        /// <param name="policyKey">策略Key</param>
        /// <param name="expiry">过期时间</param>
        /// <param name="item">项目</param>
        Task RemoveRosterAsync(RosterType rosterType, string api, Policy policy, string policyKey, params string[] item);

        /// <summary>
        /// 取得名单列表（分页）
        /// </summary>
        /// <param name="rosterType">名单类型</param>
        /// <param name="api">API</param>
        /// <param name="policy">策略</param>
        /// <param name="policyKey">策略Key</param>
        Task<(long count, IEnumerable<ListItem> items)> GetRosterListAsync(RosterType rosterType, string api, Policy policy, string policyKey, long skip, long take);

        /// <summary>
        /// 取得名单列表
        /// </summary>
        /// <param name="rosterType">名单类型</param>
        /// <param name="api">API</param>
        /// <param name="policy">策略</param>
        /// <param name="policyKey">策略Key</param>
        Task<IEnumerable<ListItem>> GetRosterListAsync(RosterType rosterType, string api, Policy policy, string policyKey);

        /// <summary>
        /// 添加全局名单
        /// </summary>
        /// <param name="rosterType">名单类型</param>
        /// <param name="policy">策略</param>
        /// <param name="policyKey">策略Key</param>
        /// <param name="expiry">过期时间</param>
        /// <param name="item">项目</param>
        Task AddGlobalRosterAsync(RosterType rosterType, Policy policy, string policyKey, TimeSpan? expiry, params string[] item);

        /// <summary>
        /// 移除全局名单
        /// </summary>
        /// <param name="policy">策略</param>
        /// <param name="item">项目</param>
        Task RemoveGlobalRosterAsync(RosterType rosterType, Policy policy, string policyKey, params string[] item);

        /// <summary>
        /// 取得全局名单列表（分页）
        /// </summary>
        /// <param name="rosterType">名单类型</param>
        /// <param name="policy">策略</param>
        /// <param name="policyKey">策略Key</param>
        Task<(long count, IEnumerable<ListItem> items)> GetGlobalRosterListAsync(RosterType rosterType, Policy policy, string policyKey, long skip, long take);

        /// <summary>
        /// 取得全局名单列表
        /// </summary>
        /// <param name="rosterType">名单类型</param>
        /// <param name="policy">策略</param>
        /// <param name="policyKey">策略Key</param>
        Task<IEnumerable<ListItem>> GetGlobalRosterListAsync(RosterType rosterType, Policy policy, string policyKey);

        #endregion
    ```
