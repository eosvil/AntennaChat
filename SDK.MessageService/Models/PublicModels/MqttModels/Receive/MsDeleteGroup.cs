using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// MQTT收到解散聊天室消息
    /// </summary>
    public class MsDeleteGroup : MsSdkMessageGroupBase
    {
        /// <summary>
        /// 解散聊天室信息
        /// </summary>
        public MsDeleteGroup_content content { get; set; }
    }

    /// <summary>
    /// 解散聊天室信息
    /// </summary>
    public class MsDeleteGroup_content
    {
        /// <summary>
        /// 群ID
        /// </summary>
        public string groupId { get; set; } = string.Empty;
    }
}
