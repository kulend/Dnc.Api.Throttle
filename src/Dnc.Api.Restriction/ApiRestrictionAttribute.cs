using System;
using System.Collections.Generic;
using System.Text;

namespace Dnc.Api.Restriction
{
    /// <summary>
    /// 接口管制Attribute
    /// </summary>
    public class ApiRestrictionAttribute : Attribute
    {
        /// <summary>
        /// 限制次数
        /// </summary>
        public int Limit { set; get; } = 1;

        /// <summary>
        /// 计时间隔
        /// </summary>
        public TimeSpan Duration { set; get; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// 依据条件
        /// </summary>
        public BasisCondition BasisCondition { set; get; } = BasisCondition.Ip;

    }

    public enum BasisCondition
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
