using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 更新讨论组信息
    /// </summary>
    public class UpdateGroupInput
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
        /// 讨论组名称
        /// </summary>
        public string groupName { get; set; } = string.Empty;

        /// <summary>
        /// 讨论组头像，初次创建时，可以不传头像
        /// </summary>
        public string groupPicture { get; set; } = string.Empty;

        /// <summary>
        /// 如果是添加成员，这里存添加的讨论组成员Id集合
        /// </summary>
        public List<string> userIds { get; set; }

        /// <summary>
        /// 如果是添加成员，这里存添加的讨论组成员名称集合
        /// </summary>
        public List<string> userNames { get; set; }

        /// <summary>
        /// 如果是删除成员，这里存删除的讨论组成员Id集合
        /// </summary>
        public List<string> deleteUserIds { get; set; }

        /// <summary>
        /// 如果是删除成员，这里存删除的讨论组成员名称集合
        /// </summary>
        public List<string> delUserNames { get; set; }
    }
}
