using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.AntSdk.AntModels
{
    /// <summary>
    /// 数据库--会话列表
    /// </summary>
    public class AntSdkTsession
    {
        /// <summary>
        /// 会话ID
        /// </summary>
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// 用户ID（讨论组会话，该字段为空）
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 讨论组ID（单人聊天，该字段为空）
        /// </summary>
        public string GroupId { get; set; } = string.Empty;

        /// <summary>
        /// 未读消息数
        /// </summary>
        public int UnreadCount { get; set; }

        /// <summary>
        /// 最后一条消息
        /// </summary>
        public string LastMsg { get; set; } = string.Empty;

        /// <summary>
        /// 最后一条消息时间
        /// </summary>
        public string LastMsgTimeStamp { get; set; } = string.Empty;

        /// <summary>
        /// 最后一条消息索引（可用于判断是否发送成功）
        /// </summary>
        public string LastChatIndex { get; set; } = string.Empty;

        /// <summary>
        /// 未读消息数(讨论组阅后即焚消息)
        /// </summary>
        public int BurnUnreadCount { get; set; }

        /// <summary>
        /// 最后一条消息(讨论组阅后即焚消息)
        /// </summary>
        public string BurnLastMsg { get; set; } = string.Empty;

        /// <summary>
        /// 最后一条消息时间(讨论组阅后即焚消息)
        /// </summary>
        public string BurnLastMsgTimeStamp { get; set; } = string.Empty;

        /// <summary>
        /// 最后一条消息索引（可用于判断是否发送成功）(讨论组阅后即焚消息)
        /// </summary>
        public string BurnLastChatIndex { get; set; } = string.Empty;

        /// <summary>
        /// 1表示是阅后即焚，0表示普通模式
        /// </summary>
        public int IsBurnMode { get; set; }

        /// <summary>
        /// 表示会话置顶的索引,如果为null,则未设置索引，否则，按数字从小到大进行排序
        /// </summary>
        public int? TopIndex { get; set; }
    }
}
