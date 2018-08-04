using System;
using System.Collections.Generic;
using System.Text;

namespace Dnc.Api.Throttle
{
    /// <summary>
    /// 接口节流Attribute
    /// </summary>
    public class ApiThrottleAttribute : Attribute
    {
        /// <summary>
        /// 限制次数
        /// </summary>
        public int Limit { set; get; } = 1;

        /// <summary>
        /// 计时间隔(单位：秒)
        /// </summary>
        public int Duration { set; get; } = 60;

        /// <summary>
        /// 策略
        /// </summary>
        public Policy Policy { set; get; } = Policy.Ip;

        /// <summary>
        /// 当识别值为空时处理方式
        /// </summary>
        public WhenNull WhenNull { set; get; } = WhenNull.Pass;

    }
}
