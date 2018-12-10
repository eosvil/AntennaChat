using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDK.Service.Models;

namespace SDK.AntSdk.AntModels
{
    public class AntSdkOffLineSimpleMsg
    {
        /// <summary>
        /// 对外暴露的触角SDK消息类型[触角SDK 消息枚举类型]
        /// </summary>
        public AntSdkMsgType MsgType { get; set; }

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
