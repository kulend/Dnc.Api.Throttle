using System;
using System.Collections.Generic;
using System.Text;

namespace Dnc.Api.Throttle
{
    /// <summary>
    /// 节流策略
    /// </summary>
    /// <remarks>修改时请务必同步修改FilterContextExtension=>GetPolicyValue方法</remarks>
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
