using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 终端消息类型
    /// </summary>
    public class SdkMsTerminalBase : SdkMsBase
    {
        /// <summary>
        /// app的唯一标识 String
        /// </summary>
        internal string appKey { get; set; } = SdkService.SdkSysParam?.Appkey;

        /// <summary>
        /// userId 消息发送者ID String
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 消息发送者的操作系统 Integer 1：PC，2：WEB，3：ANDROID，4：iOS
        /// </summary>
        internal int os { get; set; } = (int) SdkEnumCollection.OSType.PC;
    }

    /// <summary>
    /// 终端消息-心跳消息
    /// </summary>
    public class SdkMsHeartBeat : SdkMsTerminalBase
    {
        /// <summary>
        /// 自定义消息字段 String 可以是jsonString
        /// </summary>
        public string attr { get; set; } = string.Empty;
    }

    /// <summary>
    /// 终端消息-请求离线消息
    /// </summary>
    public class SdkMsQuestOffLine : SdkMsTerminalBase
    {
        public string[] attr { get; set; }
    }
}
