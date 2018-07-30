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
        public Func<HttpContext, string> OnUserIdentity;

        public string RedisConnectionString { set; get; }

    }
}
