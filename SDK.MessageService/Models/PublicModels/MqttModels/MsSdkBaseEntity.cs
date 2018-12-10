using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// .NET SDK消息基类, SDK 所有消息都需要继承此类（用于检测是否是.NET SDK准备的消息类型）
    /// </summary>
    public class SdkMsBase
    {
        /// <summary>
        /// 消息（通知）类型[MQTT 服务使用到的消息类型]
        /// </summary>
        internal string messageType { get; set; }

        /// <summary>
        /// 对外暴露的消息类型[SDK 消息枚举类型]
        /// </summary>
        private SdkMsgType _msgType = SdkMsgType.UnDefineMsg;
        internal SdkMsgType MsgType
        {
            get { return _msgType; }
            set
            {
                _msgType = value;
                if (string.IsNullOrEmpty(messageType))
                {
                    messageType = EnumExpress.GetDescription(value);
                }
            }
        }
    }
}
