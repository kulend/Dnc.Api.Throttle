using System;
using System.Collections.Generic;
using System.Text;

namespace Dnc.Api.Throttle
{
    public enum Policy
    {
        /// <summary>
        /// IP地址
        /// </summary>
        Ip,

        /// <summary>
        /// 用户身份
        /// </summary>
        UserIdentity
    }
}
