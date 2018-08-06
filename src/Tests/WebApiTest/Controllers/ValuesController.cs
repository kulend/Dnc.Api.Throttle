using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dnc.Api.Throttle;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApiTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
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


        // GET api/values
        [HttpGet]
        [RateValve(Policy = Policy.Header, PolicyKey = "hkey", Limit = 1, Duration = 30, WhenNull = WhenNull.Pass)]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        [HttpPost("addblackList")]
        [WhiteListValve(Policy = Policy.Ip)]
        public async Task Post1()
        {
            await _service.AddRosterAsync(RosterType.BlackList, "WebApiTest.Controllers.ValuesController.Post1", Policy.Header, "test", TimeSpan.FromSeconds(60), "1");
        }

        // POST api/values
        [HttpPost]
        [WhiteListValve(Policy = Policy.Ip, Priority = 3)]
        [BlackListValve(Policy = Policy.UserIdentity, Priority = 2)]
        [RateValve(Policy = Policy.Header, PolicyKey = "hkey", Limit = 1, Duration = 30, WhenNull = WhenNull.Pass)]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
