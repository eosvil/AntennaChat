using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 创建讨论组输出
    /// </summary>
    public class CreateGroupOutput
    {
        /// <summary>
        /// 群ID
        /// </summary>
        public string groupId { get; set; } = string.Empty;

        /// <summary>
        /// 群名称
        /// </summary>
        public string groupName { get; set; } = string.Empty;

        /// <summary>
        /// 群头像地址
        /// </summary>
        public string groupPicture { get; set; } = string.Empty;

        /// <summary>
        /// 群成员数量
        /// </summary>
        public string memberCount { get; set; } = string.Empty;

        /// <summary>
        /// 群主ID
        /// </summary>
        public string groupOwnerId { get; set; } = string.Empty;
    }
}
