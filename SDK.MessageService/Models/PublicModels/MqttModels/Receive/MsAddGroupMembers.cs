using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// MQTT收到讨论组添加成员信息
    /// </summary>
    public class MsAddGroupMembers : MsSdkMessageGroupBase
    {
        /// <summary>
        /// 聊天室添加成员
        /// </summary>
        public MsAddGroupMembers_content content { get; set; }
    }

    /// <summary>
    /// 讨论组添加成员
    /// </summary>
    public class MsAddGroupMembers_content
    {
        /// <summary>
        /// 添加成员信息
        /// </summary>
        public List<SdkMember> members { get; set; }

        /// <summary>
        /// 操作人ID
        /// </summary>
        public string operateId { get; set; } = string.Empty;
    }
}
