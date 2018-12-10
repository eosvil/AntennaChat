using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 创建讨论组
    /// </summary>
    internal class CreateGroup : MsSdkMessageGroupBase
    {
        /// <summary>
        /// 创建讨论组信息
        /// </summary>
       public  CreateGroup_content content { get; set; }
    }

    /// <summary>
    /// 创建讨论组内容
    /// </summary>
    internal class CreateGroup_content
    {
        /// <summary>
        /// 讨论组ID
        /// </summary>
        public string groupId { get; set; } = string.Empty;

        /// <summary>
        /// 讨论组名称
        /// </summary>
        public string groupName { get; set; } = string.Empty;

        /// <summary>
        /// 讨论组头像，初次创建时，可以不传头像
        /// </summary>
        public string groupPicture { get; set; } = string.Empty;

        /// <summary>
        /// 群成员数量
        /// </summary>
        public int memberCount { get; set; }

        /// <summary>
        /// 群主ID
        /// </summary>
        public string groupOwnerId { get; set; } = string.Empty;
    }
}
