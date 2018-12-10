using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 收到消息服务回执（订阅的topic:{appKey}/{userId}/{os}）
    /// </summary>
    public class MsReceiveMsgReceipt : SdkMsBase
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
        /// 消息ID
        /// </summary>
        public string messageId { get; set; } = string.Empty;

        /// <summary>
        /// 发送时间
        /// </summary>
        public string sendTime { get; set; } = string.Empty;

        /// <summary>
        /// 自定义消息字段 String 可以是jsonString 
        /// </summary>
        public string attr { get; set; } = string.Empty;
    }
}
