using System;
using System.Collections.Generic;
using System.Linq;
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
                options.UseRedisCacheAndStorage(redisOption => {
                    redisOption.ConnectionString = "localhost,connectTimeout=5000,allowAdmin=false,defaultDatabase=1";
                });
                //options.OnUserIdentity = httpContext => httpContext.User.GetUserIdOrZero().ToString();
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
                    Policy = Policy.Ip
                }, new BlackListValve
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
