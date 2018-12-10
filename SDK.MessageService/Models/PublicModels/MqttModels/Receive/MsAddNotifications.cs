using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{

    public class MsSdkNotificationBase : SdkMsBase
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
    }

    /// <summary>
    /// 用户登录时获取讨论组公告列表（用户未读的）topic：{appKey}/{userId}/{os} 
    /// </summary>
    public class MsUnReadNotifications : MsSdkNotificationBase
    {
        public List<MsNotification_content> content { get; set; }
    }

    /// <summary>
    /// 群组公告消息信息 topic：{appKey}/{groupId}
    /// </summary>
    public class MsAddNotification : MsSdkNotificationBase
    {
        public MsNotification_content content { get; set; }
    }

    /// <summary>
    /// MQTT收到删除讨论组公告
    /// </summary>
    public class MsDeleteNotification : MsSdkNotificationBase
    {
        public NotificationId content { get; set; }
    }

    /// <summary>
    /// MQTT收到修改公告状态为已读（多终端同步）
    /// </summary>
    public class MsModifyNotificationState : MsSdkNotificationBase
    {
        public NotificationId content { get; set; }
    }

    /// <summary>
    /// 群组公告ID信息
    /// </summary>
    public class NotificationId
    {
        /// <summary>
        /// 公告ID
        /// </summary>
        public string notificationId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 群公告内容信息
    /// </summary>
    public class MsNotification_content
    {
        /// <summary>
        /// 公告ID
        /// </summary>
        public string notificationId { get; set; } = string.Empty;

        /// <summary>
        /// 公告标题
        /// </summary>
        public string title { get; set; } = string.Empty;

        /// <summary>
        /// 公告创建时间
        /// </summary>
        public string createTime { get; set; } = string.Empty;

        /// <summary>
        /// 创建人ID
        /// </summary>
        public string createBy { get; set; } = string.Empty;

        /// <summary>
        /// 是否有附件，“0”表示没有，“1”表示有
        /// </summary>
        public string hasAttach { get; set; } = string.Empty;
    }
}
