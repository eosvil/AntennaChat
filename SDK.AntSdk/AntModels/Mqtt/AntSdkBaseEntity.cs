using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDK.Service;
using SDK.Service.Models;

namespace SDK.AntSdk.AntModels
{
    /// <summary>
    /// 触角.NET SDK消息基类, SDK 所有消息都需要继承此类（用于检测是否是.NET SDK准备的消息类型）
    /// </summary>
    public class AntSdkMsBase
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

        /// <summary>
        /// 获取平台SDK消息信息
        /// </summary>
        /// <returns></returns>
        internal SdkMsBase GetSdk()
        {
            var sdkreceivemsgtypeValue = (long)MsgType;
            var antsdkreceivemsgType = (SdkMsgType)sdkreceivemsgtypeValue;
            var sdk = new SdkMsBase
            {
                MsgType = antsdkreceivemsgType
            };
            return sdk;
        }
    }
}
