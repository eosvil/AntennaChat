using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 添加用户黑名单
    /// </summary>
    internal class AddBlacklistInput
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 屏蔽的用户ID
        /// </summary>
        public string targetId { get; set; } = string.Empty;
    }
}
