using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDK.Service;
using SDK.Service.Models;

namespace SDK.AntSdk.AntModels
{
    public class AntSdkOtherMsg
    {
        /// <summary>
        /// 触角SDK聊天消息基类
        /// </summary>
        public class OtherBase : AntSdkMsBase
        {
            /// <summary>
            /// 聊天类型 聊天类型 1：点对点聊天，2：聊天室 4：群（讨论组）
            /// </summary>
            public int chatType { get; set; }

            /// <summary>
            /// 消息发送者的操作系统 1：PC，2：WEB，3：ANDROID，4：iOS
            /// </summary>
            public int os { get; set; } = (int)SdkEnumCollection.OSType.PC;

            /// <summary>
            /// 阅后即焚的标识 1：阅后即焚，0：普通消息
            /// </summary>
            public int flag { get; set; }

            /// <summary>
            /// 消息状态 1：已读，0：未读
            /// </summary>
            public int status { get; set; }

            /// <summary>
            /// 消息ID 
            /// </summary>
            public string messageId { get; set; } = string.Empty;

            /// <summary>
            /// 消息发送者ID
            /// </summary>
            public string sendUserId { get; set; } = string.Empty;

            /// <summary>
            /// 消息接收者ID
            /// </summary>
            public string targetId { get; set; } = string.Empty;

            /// <summary>
            /// 消息发送时间
            /// </summary>
            public string sendTime { get; set; } = string.Empty;

            /// <summary>
            /// 定义消息字段 可以是jsonString
            /// </summary>
            public string attr { get; set; } = string.Empty;

            /// <summary>
            /// 消息的原始Js内容，用来存储或者转储后的解析处理
            /// </summary>
            public string sourceContent { get; set; } = string.Empty;
            /// <summary>
            /// 发送是否成功表示 1成功，0失败，接收到的消息默认是1
            /// </summary>
            public string SENDORRECEIVE { get; set; } = string.Empty;

            public int sendsucessorfail { get; set; }

            public string uploadOrDownPath { get; set; } = string.Empty;

            public int typeBurnMsg { get; set; }

            public string readtime { get; set; } = string.Empty;

            /// <summary>
            /// 发送消息时获取SDK消息用于传递
            /// </summary>
            /// <returns></returns>
            internal T GetSdkSendBase<T>() where T : MsSdkOther.SdkOtherBase, new()
            {
                var antsdkreceivemsgtypeValue = (long)MsgType;
                var sdkreceivemsgType = (SdkMsgType)antsdkreceivemsgtypeValue;
                var result = new T
                {
                    MsgType = sdkreceivemsgType,
                    sessionId = sessionId,
                    chatIndex = chatIndex,
                    chatType = chatType,
                    flag = flag,
                    status = status,
                    messageId = messageId,
                    sendUserId = sendUserId,
                    targetId = targetId,
                    sendTime = sendTime,
                    sourceContent = sourceContent,
                    attr = attr
                };
                return result;
            }
        }

        /// <summary>
        /// 方法说明：触角SDK聊天消息和SDK聊天消息转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        private static T GetAntSdkReceivedOtherBase<T>(MsSdkOther.SdkOtherBase entity) where T : OtherBase, new()
        {
            var sdkreceivemsgtypeValue = (long)entity.MsgType;
            var antsdkreceivemsgType = (AntSdkMsgType)sdkreceivemsgtypeValue;
            var result = new T
            {
                MsgType = antsdkreceivemsgType,
                sessionId = entity.sessionId,
                chatIndex = entity.chatIndex,
                chatType = entity.chatType,
                flag = entity.flag,
                os = entity.os,
                status = entity.status,
                messageId = entity.messageId,
                sendUserId = entity.sendUserId,
                targetId = entity.targetId,
                sendTime = entity.sendTime,
                attr = entity.attr,
                sourceContent = entity.sourceContent
            };
            return result;
        }

        /// <summary>
        /// 方法说明：获取接收到的平台SDK聊天消息，转化为触角SDK聊天消息
        /// </summary>
        /// <param name="entity">SDK聊天信息</param>
        /// <returns>触角SDK聊天信息</returns>
        internal static OtherBase GetAntSdkReceivedOther(MsSdkOther.SdkOtherBase entity)
        {
            try
            {
                //符合标准聊天消息结构处理的部分，走此方法处理，预留
                //返回空
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError(
                    $"[AntSdkChatMsg.GetAntSdkReceivedOther]:{Environment.NewLine}{ex.Message}{ex.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// 方法说明：根据触角SDK聊天信息转换为平台SDK聊天信息进行发送
        /// </summary>
        /// <param name="antsdksend"></param>
        /// <returns></returns>
        internal static MsSdkOther.SdkOtherBase GetSdkSend(OtherBase antsdksend)
        {
            if (antsdksend == null) { return null; }
            try
            {
                //预留符合标准聊天消息结构的其他消息发送，走此路径
                //没有找到返回空
                return antsdksend.GetSdkSendBase<MsSdkOther.SdkOtherBase>();
            }
            catch (Exception ex)
            {
                LogHelper.WriteError(
                    $"[AntSdkOtherMsg.GetSdkSend]:{Environment.NewLine}{ex.Message}{ex.StackTrace}");
                return null;
            }
        }
    }
}
