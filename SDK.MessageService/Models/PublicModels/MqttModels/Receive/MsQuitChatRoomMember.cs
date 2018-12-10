using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 聊天室成员退出
    /// </summary>
    public class MsQuitChatRoomMember : MsSdkMessageRoomBase
    {
        public SdkMember content { get; set; }
    }
}
