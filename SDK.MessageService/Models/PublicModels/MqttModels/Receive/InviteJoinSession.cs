using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 邀请加入会话接口
    /// </summary>
    public class InviteJoinSession
    {
        /// <summary>
        /// 消息（通知）类型
        /// </summary>
        public string messageType { get; set; } = string.Empty;

        /// <summary>
        /// 会话ID
        /// </summary>
        public string sessionId { get; set; } = string.Empty;

        /// <summary>
        /// 消息发送者ID
        /// </summary>
        public string sendUserId { get; set; } = string.Empty;

        /// <summary>
        /// 消息接收者ID
        /// </summary>
        public string targetId { get; set; } = string.Empty;

        /// <summary>
        /// app的唯一标识 
        /// </summary>
        public string appKey { get; set; } = string.Empty;

        /// <summary>
        /// 消息内容
        /// </summary>
        public InviteJoinSession_content content { get; set; }
    }

    /// <summary>
    /// 邀请加入会话
    /// </summary>
    public class InviteJoinSession_content
    {
        /// <summary>
        /// 邀请者名字
        /// </summary>
        public string username { get; set; } = string.Empty;

        /// <summary>
        /// 会话名称
        /// </summary>
        public string sessionName { get; set; } = string.Empty;
    }
}
