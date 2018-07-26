using Dnc.Api.Restriction.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dnc.Api.Restriction
{
    public class ApiRestrictionActionFilter : IActionFilter, IAsyncActionFilter, IPageFilter
    {
        private IEnumerable<ApiRestrictionAttribute> _attrs;

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var attrs = GetAttrs(context);

        }

        public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            return Task.CompletedTask;
        }

        public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
        }

        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
        }

        public void OnPageHandlerSelected(PageHandlerSelectedContext context)
        {
        }

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
    }
}
