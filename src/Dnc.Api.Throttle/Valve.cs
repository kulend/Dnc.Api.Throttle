using System;
using System.Collections.Generic;
using System.Text;

namespace Dnc.Api.Throttle
{
    /// <summary>
    /// 阀门
    /// </summary>
    public class Valve
    {
        /// <summary>
        /// 策略
        /// </summary>
        public Policy Policy { set; get; } = Policy.Ip;

        /// <summary>
        /// 策略Key
        /// </summary>
        /// <remarks>
        /// Policy == Policy.Header是，PolicyKey指定为对应Header的key
        /// Policy == Policy.Query是，PolicyKey指定为对应Query的key
        /// </remarks>
        public string PolicyKey { set; get; }

        /// <summary>
        /// 当识别值为空时处理方式
        /// </summary>
        public WhenNull WhenNull { set; get; } = WhenNull.Pass;
    }

    /// <summary>
    /// 频率阀门
    /// </summary>
    public class RateValve : Valve
    {
        /// <summary>
        /// 限制次数
        /// </summary>
        public int Limit { set; get; } = 1;

        /// <summary>
        /// 计时间隔(单位：秒)
        /// </summary>
        public int Duration { set; get; } = 60;

    }

    /// <summary>
    /// 名册阀门
    /// </summary>
    public class RosterValve : Valve
    {
       
    }

    /// <summary>
    /// 黑名单阀门
    /// </summary>
    public class BlackListValve : RosterValve
    {

    }

    /// <summary>
    /// 白名单阀门
    /// </summary>
    public class WhiteListValve : RosterValve
    {

    }
}
