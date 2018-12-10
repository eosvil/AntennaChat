using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// MQTT收到添加聊天室成员消息
    /// </summary>
    public  class MsAddChatRoomMembers : MsSdkMessageRoomBase
    {
       public MsAddChatRoomMembers_content content { get; set; }
    }

    /// <summary>
    /// MQTT收到添加聊天室成员结构
    /// </summary>
    public class MsAddChatRoomMembers_content
    {
        /// <summary>
        /// 一次最多拉200个成员
        /// </summary>
        public List<SdkMember> members { get; set; }

        /// <summary>
        /// 操作者账号，必须是创建者才可以操作
        /// </summary>
        public string operateId { get; set; } = string.Empty;
    }
}
