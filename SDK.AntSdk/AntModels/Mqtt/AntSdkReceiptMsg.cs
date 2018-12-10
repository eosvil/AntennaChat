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
    /// 触角SDK发送聊天消息回执（消息的topic已读消息：SdkRead，已收消息sdk_receive  ）
    /// </summary>
    public class AntSdkReceiptMsg : AntSdkMsBase
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 自定义消息字段 可以是jsonString
        /// </summary>
        public string attr { get; set; } = string.Empty;

        /// <summary>
        /// 方法说明：获取SDK回执发送实体
        /// </summary>
        /// <returns>SDK回执发送实体</returns>
        internal SdkMsSendMsgReceipt GetSdkSend()
        {
            var antsdkreceivemsgtypeValue = (long)MsgType;
            var sdkreceivemsgType = (SdkMsgType)antsdkreceivemsgtypeValue;
            var sdkSend = new SdkMsSendMsgReceipt
            {
                MsgType = sdkreceivemsgType,
                sessionId = sessionId,
                chatIndex = chatIndex,
                userId = userId,
                attr = attr,
            };
            return sdkSend;
        }
    }
}
