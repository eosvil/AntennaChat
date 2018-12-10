using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    ///  获取用户的讨论组列表，讨论组信息   
    /// </summary>
    public class GetGroupListOutput
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

        /// <summary>
        /// 群管理员ID的集合
        /// </summary>
        public List<string> managerIds { get; set; }

        /// <summary>
        /// 状态1:接受消息并提;2：接受消息不提醒
        /// </summary>
        public int state { get; set; }
    }
}
