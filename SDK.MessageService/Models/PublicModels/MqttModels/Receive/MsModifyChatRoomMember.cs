using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 聊天室成员信息变更
    /// </summary>
    public class MsModifyChatRoomMember: MsSdkMessageRoomBase
    {
        public MsModifyChatRoomMember_content content { get; set; }
    }

    /// <summary>
    /// 聊天室成员信息变更
    /// </summary>
    public class MsModifyChatRoomMember_content
    {
        /// <summary>
        /// 操作者ID
        /// </summary>
        public string operateId { get; set; } = string.Empty;

        /// <summary>
        /// 成员ID
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 成员名称
        /// </summary>
        public string userName { get; set; } = string.Empty;

        /// <summary>
        /// 描述
        /// </summary>
        public string desc { get; set; } = string.Empty;

        /// <summary>
        /// 备注
        /// </summary>
        public string remark { get; set; } = string.Empty;

        /// <summary>
        /// 扩展信息
        /// </summary>
        public string attr1 { get; set; } = string.Empty;

        /// <summary>
        /// 扩展信息
        /// </summary>
        public string attr2 { get; set; } = string.Empty;

        /// <summary>
        /// 扩展信息
        /// </summary>
        public string attr3 { get; set; } = string.Empty;
    }
}
