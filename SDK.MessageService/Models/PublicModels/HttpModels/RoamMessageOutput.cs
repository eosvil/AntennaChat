using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 消息漫游信息
    /// </summary>
    public class RoamMessageOutput
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        public string messageId { get; set; } = string.Empty;

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
        public string content { get; set; } = string.Empty;

        /// <summary>
        /// 消息发送时间
        /// </summary>
        public string sendTime { get; set; } = string.Empty;

        /// <summary>
        /// 消息游标
        /// </summary>
        public string chatIndex { get; set; } = string.Empty;

        /// <summary>
        /// 会话ID
        /// </summary>
        public string sessionId { get; set; } = string.Empty;
    }
}
