using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 创建讨论组输入
    /// </summary>
    public class CreateGroupInput
    {
        /// <summary>
        /// 当前登陆用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 讨论组名称
        /// </summary>
        public string groupName { get; set; } = string.Empty;

        /// <summary>
        /// 讨论组头像，初次创建时，可以不传头像
        /// </summary>
        public string groupPicture { get; set; } = string.Empty;

        /// <summary>
        /// 讨论组成员ID列表
        /// </summary>
        public string[] userIds { get; set; }
    }
}
