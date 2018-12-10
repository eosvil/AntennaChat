using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 群主转让输入参数
    /// </summary>
    public class GroupOwnerChangeInput
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
        /// 新群主ID
        /// </summary>
        public string newOwnerId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 群管理员设置输入参数
    /// </summary>
    public class GroupManagerChangeInput
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
        /// 新管理员ID
        /// </summary>
        public string newOwnerId { get; set; } = string.Empty;

        /// <summary>
        /// 角色
        /// </summary>
        public int roleLevel { get; set; } = 0;

    }
}
