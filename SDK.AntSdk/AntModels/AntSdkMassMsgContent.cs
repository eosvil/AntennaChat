using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.AntSdk.AntModels
{
    /// <summary>
    /// 触角SDK业务使用消息实体
    /// </summary>
    public class AntSdkMassMsgCtt
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
        /// 接收者ID组,用逗号隔开
        /// </summary>
        public string targetId { get; set; } = string.Empty;

        /// <summary>
        /// 公司代码
        /// </summary>
        public string companyCode { get; set; } = string.Empty;

        /// <summary>
        /// 群发的内容
        /// </summary>
        public string content { get; set; } = string.Empty;

        /// <summary>
        /// 操作系统，PC端传1
        /// </summary>
        public int os { get; set; }

        /// <summary>
        /// 会话ID
        /// </summary>
        public string sessionId { get; set; } = string.Empty;

        /// <summary>
        /// 消息发送时间（回执才有该字段）
        /// </summary>
        public string sendTime { get; set; } = string.Empty;

        /// <summary>
        /// 消息游标（回执才有该字段）
        /// </summary>
        public string chatIndex { get; set; } = string.Empty;
    }
}
