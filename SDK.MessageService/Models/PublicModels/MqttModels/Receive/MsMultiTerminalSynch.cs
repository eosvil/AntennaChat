using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// MQTT收到多终端同步的已读回执 topic：{appKey}/userId 
    /// 例如：用户A（android端）发送已读回执时，sdkmessage收到这条消息后，会同时向用户A的ID发送已读回执的消息，用来做多终端同步，用户A的其他端收到同步消息后 
    /// 1）如果OS和自己一样，那么不需要处理 
    /// 2）如果OS和自己不一样，那么表示其他端已读该条消息，自己也要将该条消息标识为已读
    /// </summary>
    public class MsMultiTerminalSynch : SdkMsBase
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
        /// 聊天类型 1：点对点聊天，2：聊天室 4：群（讨论组）
        /// </summary>
        public int chatType { get; set; }

        /// <summary>
        /// 消息发送者的操作系统 1：PC，2：WEB，3：ANDROID，4：iOS
        /// </summary>
        public int os { get; set; } = (int)SdkEnumCollection.OSType.PC;

        /// <summary>
        /// 阅后即焚的标识 1：阅后即焚，0：普通消息
        /// </summary>
        public int flag { get; set; }

        /// <summary>
        /// 消息状态 1：已读，0：未读
        /// </summary>
        public int status { get; set; }

        /// <summary>
        /// 消息ID 
        /// </summary>
        public string messageId { get; set; } = string.Empty;

        /// <summary>
        /// app的唯一标识
        /// </summary>
        internal string appKey { get; set; } = SdkService.SdkSysParam?.Appkey;

        /// <summary>
        /// 消息发送者ID
        /// </summary>
        public string sendUserId { get; set; } = string.Empty;

        /// <summary>
        /// 消息接收者ID
        /// </summary>
        public string targetId { get; set; } = string.Empty;

        /// <summary>
        /// 消息发送时间
        /// </summary>
        public string sendTime { get; set; } = string.Empty;

        /// <summary>
        /// 定义消息字段 可以是jsonString
        /// </summary>
        public string attr { get; set; } = string.Empty;

        /// <summary>
        /// 原始的消息信息（用来进行存储或者转存后的解析）
        /// </summary>
        public string sourceContent { get; set; } = string.Empty;

        /// <summary>
        /// 多终端同步已读回执信息
        /// </summary>
        public string content { get; set; } = string.Empty;
    }
}
