using Dnc.Api.Throttle.Middlewares;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseApiThrottle(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiThrottleMiddleware>();
        }
    }
}
