using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 用户个性化设置（需要发送已读回执）
    /// </summary>
    public class MsIndividuationSet : SdkMsBase
    {
        /// <summary>
        /// 会话ID 
        /// </summary>
        public string sessionId { get; set; } = string.Empty;

        /// <summary>
        ///  通知游标
        /// </summary>
        public string chatIndex { get; set; } = string.Empty;

        /// <summary>
        /// 时间戳
        /// </summary>
        public string sendTime { get; set; } = string.Empty;

        /// <summary>
        /// 用户个性化设置内容
        /// </summary>
        public MsIndividuationSet_content content { get; set; }
    }

    /// <summary>
    /// 用户个性化设置内容
    /// </summary>
    public class MsIndividuationSet_content
    {
        /// <summary>
        /// 聊天类型，4--群组，2--聊天室 1--单聊
        /// </summary>
        public int chatType { get; set; }

        /// <summary>
        /// 对目标ID设置免打扰
        /// </summary>
        public string targetId { get; set; } = string.Empty;

        /// <summary>
        /// 1--接受消息，2--免打扰 3--屏蔽
        /// </summary>
        public string state { get; set; } = string.Empty;
    }
}
