using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// MQTT收到讨论组成员信息变更
    /// </summary>
    public class MsModifyGroupMember : MsSdkMessageGroupBase
    {
        /// <summary>
        /// 讨论组成员信息变更
        /// </summary>
        public MsModifyGroupMember_content content { get; set; }
    }

    /// <summary>
    /// 讨论组成员信息变更
    /// </summary>
    public class MsModifyGroupMember_content
    {
        /// <summary>
        /// 操作者ID
        /// </summary>
        public string operateId { get; set; } = string.Empty;

        /// <summary>
        /// 被改变的用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 被改变的用户名称
        /// </summary>
        public string userName { get; set; } = string.Empty;
    }
}
