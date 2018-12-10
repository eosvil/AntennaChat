using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 删除聊天室成员(MQTT接收到的消息) 
    /// </summary>
    public class MsDeleteChatRoomMembers : MsSdkMessageRoomBase
    {
        /// <summary>
        /// 
        /// </summary>
        public DeleteChatRoomMembers_content content { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class DeleteChatRoomMembers_content
    {
        /// <summary>
        /// 操作者ID
        /// </summary>
        public string operateId { get; set; } = string.Empty;

        /// <summary>
        /// 被删除成员
        /// </summary>
        public List<SdkMember> members { get; set; }
    }
}
