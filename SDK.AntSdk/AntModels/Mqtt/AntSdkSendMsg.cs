using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDK.Service;
using SDK.Service.Models;

namespace SDK.AntSdk.AntModels
{
    public class AntSdkSendMsg
    {
        public class Terminal
        {
            /// <summary>
            /// 终端消息类型基类
            /// </summary>
            public class TerminalBase : AntSdkMsBase
            {
                /// <summary>
                /// userId 消息发送者ID String
                /// </summary>
                public string userId { get; set; } = string.Empty;
            }

            /// <summary>
            /// 触角SDK发送：终端消息-心跳消息
            /// </summary>
            public class HeartBeat : TerminalBase
            {
                public HeartBeat()
                {
                    MsgType = AntSdkMsgType.HeartBeat;
                }

                /// <summary>
                /// 自定义消息字段 String 可以是jsonString，没有的话，传空字符串
                /// </summary>
                public string attr { get; set; } = string.Empty;

                internal SdkMsHeartBeat GetSdkSend()
                {
                    var antsdkreceivemsgtypeValue = (long)MsgType;
                    var sdkreceivemsgType = (SdkMsgType)antsdkreceivemsgtypeValue;
                    var sdksend = new SdkMsHeartBeat
                    {
                        MsgType = sdkreceivemsgType,
                        userId = userId,
                        attr = attr
                    };
                    return sdksend;
                }
            }

            /// <summary>
            /// 触角SDK发送：终端消息-请求离线消息
            /// </summary>
            public class QuestOffLine : TerminalBase
            {
                public QuestOffLine()
                {
                    MsgType = AntSdkMsgType.OffLineMsgRequest;
                }

                /// <summary>
                /// 请求离线内容，例如
                /// "G123456",群（讨论组）ID或者聊天室ID
                /// "G234567",群（讨论组）ID或者聊天室ID
                /// "G456789"
                /// </summary>
                public string[] attr { get; set; }

                internal SdkMsQuestOffLine GetSdkSend()
                {
                    var antsdkreceivemsgtypeValue = (long)MsgType;
                    var sdkreceivemsgType = (SdkMsgType)antsdkreceivemsgtypeValue;
                    var sdksend = new SdkMsQuestOffLine
                    {
                        MsgType = sdkreceivemsgType,
                        userId = userId,
                        attr = attr
                    };
                    return sdksend;
                }
            }

            /// <summary>
            /// 方法说明：根据触角SDK发送数据获取SDK发送数据
            /// </summary>
            /// <param name="antsdksend"></param>
            /// <returns></returns>
            internal static SdkMsTerminalBase GetSdkSend(TerminalBase antsdksend)
            {
                try
                {
                    var hearbeatantsdksend = antsdksend as HeartBeat;
                    if (hearbeatantsdksend != null)
                    {
                        var sdksend = hearbeatantsdksend.GetSdkSend();
                        return sdksend;
                    }
                    var offlineantsdksend = antsdksend as QuestOffLine;
                    if (offlineantsdksend != null)
                    {
                        var sdksend = offlineantsdksend.GetSdkSend();
                        return sdksend;
                    }
                    //非终端类型的返回空
                    return null;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError(
                    $"[AntSdkSendMsg.Terminal.GetSdkSend]:{Environment.NewLine}{ex.Message}{ex.StackTrace}");
                    return null;
                } 
            }
        }


        /// <summary>
        /// [支持4000-9999的自定义消息，仅仅是传输][自己完成Sdk消息基类（MsSdkMessageEntity）的继承并且定义自己的content内容]
        /// </summary>
        public class Custom
        {
            /// <summary>
            /// 自定义消息内容[自己定义]
            /// </summary>
            public object content { get; set; }
        }

        /// <summary>
        /// MQTT收到点对点阅后即焚消息已读的回执 topic：{appKey}/userId 
        /// </summary>
        public class PointBurnReaded : AntSdkMsBase
        {
            public PointBurnReaded()
            {
                MsgType = AntSdkMsgType.PointBurnReaded;
            }

            /// <summary>
            /// 聊天类型 1：点对点聊天，2：聊天室 4：群（讨论组）
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
            /// app的唯一标识
            /// </summary>
            internal string appKey { get; set; } = SdkService.SdkSysParam?.Appkey;

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
            /// 原始的消息信息（用来进行存储或者转存后的解析）
            /// </summary>
            public string sourceContent { get; set; } = string.Empty;

            /// <summary>
            /// 点对点阅后即焚消息已读的回执
            /// </summary>
            public PointBurnReaded_content content { get; set; }

            /// <summary>
            /// 方法说明：获取SDK发送点对点阅后即焚消息已读回执
            /// </summary>
            /// <returns>SDK回执发送实体</returns>
            internal MsPointBurnReaded GetSdkSend()
            {
                var antsdkreceivemsgtypeValue = (long)MsgType;
                var sdkreceivemsgType = (SdkMsgType)antsdkreceivemsgtypeValue;
                var sdkSend = new MsPointBurnReaded
                {
                    MsgType = sdkreceivemsgType,
                    sessionId = sessionId,
                    chatIndex = chatIndex,
                    chatType = chatType,
                    os = os,
                    flag = flag,
                    status = status,
                    messageId = messageId,
                    appKey = appKey,
                    sendUserId = sendUserId,
                    targetId = targetId,
                    sendTime = sendTime,
                    attr = attr,
                    content = new MsPointBurnReaded_content
                    {
                        readIndex = content?.readIndex ?? 0,
                        messageId = content?.messageId
                    }
                };
                return sdkSend;
            }
        }

        /// <summary>
        /// MQTT收到点对点阅后即焚消息已读的回执内容
        /// </summary>
        public class PointBurnReaded_content
        {
            /// <summary>
            /// 用户B已读A发送的阅后即焚消息的chatIndex
            /// </summary>
            public int readIndex { get; set; }

            /// <summary>
            /// 已读消息的messageID
            /// </summary>
            public string messageId { get; set; } = string.Empty;
        }
    }
}
