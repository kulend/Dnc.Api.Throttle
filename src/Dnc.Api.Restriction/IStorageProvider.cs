﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dnc.Api.Throttle
{
    public interface IStorageProvider
    {
        #region 黑名单 & 白名单

        /// <summary>
        /// 保存黑名单
        /// </summary>
        /// <param name="policy">策略</param>
        /// <param name="expiry">过期时间</param>
        /// <param name="item">项目</param>
        Task SaveBlackListAsync(Policy policy, TimeSpan? expiry, params string[] item);

        /// <summary>
        /// 保存白名单
        /// </summary>
        /// <param name="policy">策略</param>
        /// <param name="expiry">过期时间</param>
        /// <param name="item">项目</param>
        Task SaveWhiteListAsync(Policy policy, TimeSpan? expiry, params string[] item);

        /// <summary>
        /// 删除黑名单中数据
        /// </summary>
        Task RemoveBlackListAsync(Policy policy, params string[] item);

        /// <summary>
        /// 删除白名单中数据
        /// </summary>
        Task RemoveWhiteListAsync(Policy policy, params string[] item);

        #endregion
    }
}
