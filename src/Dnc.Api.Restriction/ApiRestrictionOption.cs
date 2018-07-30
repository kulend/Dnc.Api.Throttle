using Dnc.Api.Restriction.Redis;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dnc.Api.Restriction
{
    public class ApiRestrictionOption
    {
        public Func<HttpContext, string> OnUserIdentity = context => "";

        public string RedisConnectionString { set; get; }

        public string RedisKeyPrefix { set; get; } = "ApiRestriction";
    }
}
