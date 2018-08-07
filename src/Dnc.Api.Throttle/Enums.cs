using System;
using System.Collections.Generic;
using System.Text;

namespace Dnc.Api.Throttle
{
    /// <summary>
    /// 节流策略
    /// </summary>
    /// <remarks>修改时请务必同步修改HttpContextExtension=>GetPolicyValue方法</remarks>
    public enum Policy : short
    {
        /// <summary>
        /// IP地址
        /// </summary>
        Ip = 1,

        /// <summary>
        /// 用户身份
        /// </summary>
        UserIdentity = 2,

        /// <summary>
        /// Request Header
        /// </summary>
        Header = 3,

        /// <summary>
        /// Request Query
        /// </summary>
        Query = 4,

        /// <summary>
        /// 网址 Request path
        /// </summary>
        RequestPath = 5,

        /// <summary>
        /// Cookie
        /// </summary>
        Cookie = 6
    }

    /// <summary>
    /// 当识别值为空时处理方式
    /// </summary>
    public enum WhenNull : short
    {
        /// <summary>
        /// 通过
        /// </summary>
        Pass = 0,

        /// <summary>
        /// 拦截
        /// </summary>
        Intercept = 1
    }

    /// <summary>
    /// 拦截位置
    /// </summary>
    public enum IntercepteWhere
    {
        ActionFilter,

        PageFilter,

        Middleware
    }

    public enum RosterType : short
    {
        /// <summary>
        /// 黑名单
        /// </summary>
        BlackList = 1,

        /// <summary>
        /// 白名单
        /// </summary>
        WhiteList = 2
    }
}
