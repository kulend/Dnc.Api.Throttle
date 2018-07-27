using Dnc.Api.Restriction.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dnc.Api.Restriction
{
    public class ApiRestrictionActionFilter : IAsyncActionFilter, IAsyncPageFilter
    {
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

        private void aasas(IEnumerable<ApiRestrictionAttribute> attrs, FilterContext context)
        {
            var ip = context.HttpContext.GetUserIp();

            
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
