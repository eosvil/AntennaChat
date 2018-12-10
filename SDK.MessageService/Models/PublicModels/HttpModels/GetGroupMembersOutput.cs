using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 获取讨论组成员信息
    /// </summary>
    public class GetGroupMembersOutput
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 用户名称
        /// </summary>
        public string userName { get; set; } = string.Empty;

        /// <summary>
        /// 工号
        /// </summary>
        public string userNum { get; set; } = string.Empty;

        /// <summary>
        /// 用户头像
        /// </summary>
        public string picture { get; set; } = string.Empty;

        /// <summary>
        /// 职位
        /// </summary>
        public string position { get; set; } = string.Empty;

        /// <summary>
        /// 0--普通成员 1--群主  2--管理员
        /// </summary>
        public int roleLevel { get; set; }
    }
}
