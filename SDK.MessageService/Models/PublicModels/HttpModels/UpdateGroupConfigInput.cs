using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 更新用户在讨论组的设置
    /// </summary>
    public class UpdateGroupConfigInput
    {
        /// <summary>
        /// 当前登陆用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 讨论组ID
        /// </summary>
        public string groupId { get; set; } = string.Empty;

        /// <summary>
        ///  用户在讨论组的消息状态 1:接受并提醒 2:接受不提醒(免打扰) 3:拒绝接受(屏蔽)
        /// </summary>
        public string state { get; set; } = string.Empty;
    }
}
