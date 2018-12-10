using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// MQTT收到成员退出讨论组
    /// </summary>
    public class MsQuitGroupMember : MsSdkMessageGroupBase
    {
        /// <summary>
        /// 成员退出讨论组
        /// </summary>
        public MsQuitGroupMember_content content { get; set; }
    }

    /// <summary>
    /// 成员退出讨论组
    /// </summary>
    public class MsQuitGroupMember_content
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
        /// 群主ID，如果群主退出有值
        /// </summary>
        public string groupOwnerId { get; set; } = string.Empty;

        /// <summary>
        /// 群主名称，如果群主退出有值
        /// </summary>
        public string groupOwnerName { get; set; } = string.Empty;
    }
}
