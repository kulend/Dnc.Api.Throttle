using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Dnc.Api.Restriction.Extensions
{
    internal static class FilterContextExtension
    {
        internal static MethodInfo GetHandlerMethod(this FilterContext context)
        {
            if (context == null)
            {
                return null;
            }

            MethodInfo method = null;

            //RazorPages
            if (context is PageHandlerExecutingContext c1)
            {
                method = c1.HandlerMethod?.MethodInfo;
            }
            else if (context is PageHandlerExecutedContext c2)
            {
                method = c2.HandlerMethod?.MethodInfo;
            }
            else if (context is PageHandlerSelectedContext c3)
            {
                method = c3.HandlerMethod?.MethodInfo;
            }
            else if (context is ResourceExecutingContext c4)
            {
                method = null;
            }
            else if (context is ResourceExecutedContext c5)
            {
                method = null;
            }
            else if (context.ActionDescriptor is ControllerActionDescriptor controllerDescriptor)
            {
                //兼容MVC Controller
                method = controllerDescriptor.MethodInfo;
            }
            else
            {
                return null;
            }

            return method;
        }
    }
}
