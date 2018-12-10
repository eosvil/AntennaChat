using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// MQTT收到群组删除成员信息
    /// </summary>
    public class MsDeleteGroupMembers : MsSdkMessageGroupBase
    {
        /// <summary>
        /// 删除群组成员消息
        /// </summary>
        public MsDeleteGroupMembers_content content { get; set; }
    }

    /// <summary>
    /// 删除群组成员信息
    /// </summary>
    public class MsDeleteGroupMembers_content
    {
        /// <summary>
        /// 删除成员信息
        /// </summary>
        public List<SdkMember> members { get; set; }

        /// <summary>
        /// 操作人ID
        /// </summary>
        public string operateId { get; set; } = string.Empty;
    }
}
