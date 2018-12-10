using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antenna.Model
{
    /// <summary>
    /// 数据库--会话列表
    /// </summary>
    public class T_Session
    {
        public string SessionId { get; set; }//会话ID
        public string UserId { get; set; }//用户ID（讨论组会话，该字段为空）
        public string GroupId { get; set; }//讨论组ID（单人聊天，该字段为空）
        public int UnreadCount { get; set; }//未读消息数
        public string LastMsg { get; set; }//最后一条消息
        public string LastMsgTimeStamp { get; set; }//最后一条消息时间
        public string LastChatIndex { get; set; }//最后一条消息索引（可用于判断是否发送成功）
        public int BurnUnreadCount { get; set; }//未读消息数(讨论组阅后即焚消息)
        public string BurnLastMsg { get; set; }//最后一条消息(讨论组阅后即焚消息)
        public string BurnLastMsgTimeStamp { get; set; }//最后一条消息时间(讨论组阅后即焚消息)
        public string BurnLastChatIndex { get; set; }//最后一条消息索引（可用于判断是否发送成功）(讨论组阅后即焚消息)
        public int IsBurnMode { get; set; }//1表示是阅后即焚，0表示普通模式
        public int? TopIndex { get; set; } //表示会话置顶的索引,如果为null,则未设置索引，否则，按数字从小到大进行排序
    }
}
