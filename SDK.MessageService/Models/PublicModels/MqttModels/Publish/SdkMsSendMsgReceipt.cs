using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 聊天消息回执（消息的topic已读消息：SdkRead，已收消息sdk_receive  ）
    /// </summary>
    public class SdkMsSendMsgReceipt : SdkMsBase
    {
        /// <summary>
        /// 会话ID
        /// </summary>
        public string sessionId { get; set; } = string.Empty;

        /// <summary>
        /// 通知游标（会话内消息的index）
        /// </summary>
        public string chatIndex { get; set; } = string.Empty;

        /// <summary>
        /// 消息发送者的操作系统 1：PC，2：WEB，3：ANDROID，4：iOS
        /// </summary>
        internal int os { get; set; } = (int) SdkEnumCollection.OSType.PC;

        /// <summary>
        /// app的唯一标识
        /// </summary>
        internal string appKey { get; set; } = SdkService.SdkSysParam?.Appkey;

        /// <summary>
        /// 用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 自定义消息字段 可以是jsonString
        /// </summary>
        public string attr { get; set; } = string.Empty;
    }
}
