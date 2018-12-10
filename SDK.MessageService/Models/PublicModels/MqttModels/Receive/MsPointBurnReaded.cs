using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// MQTT收到点对点阅后即焚消息已读的回执 topic：{appKey}/userId 
    /// </summary>
    public class MsPointBurnReaded : SdkMsBase
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
        /// 点对点阅后即焚消息已读的回执
        /// </summary>
        public MsPointBurnReaded_content content { get; set; }
    }

    /// <summary>
    /// MQTT收到点对点阅后即焚消息已读的回执内容
    /// </summary>
    public class MsPointBurnReaded_content
    {
        /// <summary>
        /// 消息已读索引[服务器删除消息使用]
        /// </summary>
        public int readIndex { get; set; }

        /// <summary>
        /// 已读消息的messageID[终端删除消息使用]
        /// </summary>
        public string messageId { get; set; } = string.Empty;
    }
}
