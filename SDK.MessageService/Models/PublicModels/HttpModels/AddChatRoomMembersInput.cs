using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// HTTP请求添加聊天室输入参数Model
    /// </summary>
    public class AddChatRoomMembersInput
    {
        /// <summary>
        /// 聊天室ID 
        /// </summary>
        public string roomId { get; set; } = string.Empty;

        /// <summary>
        /// 聊天室名称
        /// </summary>
        public string roomName { get; set; } = string.Empty;

        /// <summary>
        /// 操作者账号，必须是创建者才可以操作
        /// </summary>
        public string operateId { get; set; } = string.Empty;

        /// <summary>
        /// 一次最多拉200个成员
        /// </summary>
        public List<SdkMember> members { get; set; }
    }
}
