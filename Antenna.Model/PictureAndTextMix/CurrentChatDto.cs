using SDK.AntSdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antenna.Model.PictureAndTextMix
{
    public class CurrentChatDto
    {
        /// <summary>
        /// 聊天类型
        /// </summary>
        public AntSdkchatType type { set; get; }
        /// <summary>
        /// 消息ID
        /// </summary>
        public string messageId { set; get; }
        /// <summary>
        /// 发送者ID
        /// </summary>
        public string sendUserId { set;get; }
        /// <summary>
        /// 回话ID
        /// </summary>
        public string sessionId { set; get; }
        /// <summary>
        /// 目标ID
        /// </summary>
        public string targetId { set; get; }
        public bool isOnceSend { set; get; }
    }
}
