using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 打卡基类
    /// </summary>
    public class MsAttendanceRecordBase: SdkMsBase
    {
        /// <summary>
        /// 会话ID
        /// </summary>
        public string sessionId { get; set; } = string.Empty;

        /// <summary>
        /// 通知游标（会话内消息的index）
        /// </summary>
        public string chatIndex { get; set; } = string.Empty;

        public int flag { get; set; }

        /// <summary>
        /// 消息发送时间
        /// </summary>
        public string sendTime { get; set; } = string.Empty;
        /// <summary>
        /// 原始的消息信息（用来进行存储或者转存后的解析）
        /// </summary>
        public string sourceContent { get; set; } = string.Empty;
    }
    /// <summary>
    /// 打卡验证消息推送对象
    /// </summary>
    public class MsAttendanceRecordVerify : MsAttendanceRecordBase
    {
        public MsAttendanceRecord_content content { get; set; }
    }
    /// <summary>
    /// 打卡验证的需要的内容
    /// </summary>
    public class MsAttendanceRecord_content
    {
        /// <summary>
        /// 打卡地址
        /// </summary>
        public string address { get; set; }
        /// <summary>
        /// 打卡ID
        /// </summary>
        public string attendId { get; set; }
        /// <summary>
        /// 打卡时间
        /// </summary>
        public string attendTime { get; set; }
        public bool flag { get; set; }
        /// <summary>
        /// 打卡用户
        /// </summary>
        public string userId { get; set; }
    }
}
