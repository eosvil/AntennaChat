using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// SDK消息基类Model
    /// </summary>
    public class MsSdkMessageRoomBase : SdkMsBase
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
        /// 时间戳
        /// </summary>
        public string sendTime { get; set; } = string.Empty;
    }

    /// <summary>
    /// SDK消息基类Model
    /// </summary>
    public class MsSdkMessageGroupBase : SdkMsBase
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
        /// 时间戳
        /// </summary>
        public string sendTime { get; set; } = string.Empty;
    }

    /// <summary>
    /// SDK消息基类Model
    /// </summary>
    public class MsSdkMessageOtherBase : SdkMsBase
    {
        /// <summary>
        /// 会话ID
        /// </summary>
        public string sessionId { get; set; } = string.Empty;

        /// <summary>
        /// 通知游标（会话内消息的index）
        /// </summary>
        public string chatIndex { get; set; } = string.Empty;
    }
}
