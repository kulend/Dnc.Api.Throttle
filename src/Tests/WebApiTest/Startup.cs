using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Dnc.Api.Throttle;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WebApiTest
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Api限流
            services.AddApiThrottle(options => {
                //配置redis
                //如果Cache和Storage使用同一个redis，则可以按如下配置
                options.UseRedisCacheAndStorage(opts => {
                    opts.ConnectionString = "localhost,connectTimeout=5000,allowAdmin=false,defaultDatabase=0";
                    opts.KeyPrefix = "apithrottle"; //指定给所有key加上前缀，默认为apithrottle
                });
                //如果Cache和Storage使用不同redis库，可以按如下配置
                //options.UseRedisCache(opts => {
                //    opts.ConnectionString = "localhost,connectTimeout=5000,allowAdmin=false,defaultDatabase=0";
                //});
                //options.UseRedisStorage(opts => {
                //    opts.ConnectionString = "localhost,connectTimeout=5000,allowAdmin=false,defaultDatabase=1";
                //});

                options.OnIpAddress = (context) => {
                    var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                    if (string.IsNullOrEmpty(ip))
                    {
                        ip = context.Connection.RemoteIpAddress.ToString();
                    }
                    return ip;
                };

                options.OnUserIdentity = (httpContext) =>
                {
                    //这里根据自己需求返回用户唯一身份
                    return httpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                };

                options.onIntercepted = (context, valve, where) =>
                {
                    if (where == IntercepteWhere.Middleware)
                    {
                        return new JsonResult(new { code = 99, message = "访问过于频繁，请稍后重试！" });
                    }
                    else
                    {
                        return new ApiThrottleResult { Content = "访问过于频繁，请稍后重试！" };
                    }
                };
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
            });

            services.AddMvc(opts => {
                opts.Filters.Add(typeof(ApiThrottleActionFilter));
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            //Api限流
            app.UseApiThrottle();

            app.UseMvc();
        }
    }
}
