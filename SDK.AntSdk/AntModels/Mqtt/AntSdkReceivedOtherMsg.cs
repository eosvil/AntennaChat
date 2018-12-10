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
    /// 
    /// </summary>
    public class AntSdkReceivedOtherMsg
    {
        /// <summary>
        /// 收到消息服务回执（订阅的topic:{appKey}/{userId}/{os}）
        /// </summary>
        public class MsgReceipt : AntSdkMsBase
        {
            /// <summary>
            /// 消息ID
            /// </summary>
            public string messageId { get; set; } = string.Empty;

            /// <summary>
            /// 发送时间
            /// </summary>
            public string sendTime { get; set; } = string.Empty;

            /// <summary>
            /// 自定义消息字段 String 可以是jsonString 
            /// </summary>
            public string attr { get; set; } = string.Empty;

            /// <summary>
            /// 触角SDK接收回执消息
            /// </summary>
            /// <param name="entity"></param>
            /// <returns></returns>
            internal static MsgReceipt GetReceiveAntSdkMsgReceiptInfo(MsReceiveMsgReceipt entity)
            {
                if (entity == null) { return null; }
                var sdkreceivemsgtypeValue = (long)entity.MsgType;
                var antsdkreceivemsgType = (AntSdkMsgType)sdkreceivemsgtypeValue;
                var antsdkEntity = new MsgReceipt
                {
                    MsgType = antsdkreceivemsgType,
                    sessionId = entity.sessionId,
                    chatIndex = entity.chatIndex,
                    messageId = entity.messageId,
                    sendTime = entity.sendTime,
                    attr = entity.attr
                };
                return antsdkEntity;
            }
        }

        /// <summary>
        /// 触角SDK接收到的公告信息
        /// </summary>
        public class Notifications
        {
            /// <summary>
            /// 
            /// </summary>
            public class NotificationsBase : AntSdkMsBase
            {
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
            }

            /// <summary>
            /// 用户登录时获取讨论组公告列表（用户未读的）topic：{appKey}/{userId}/{os} 
            /// </summary>
            public class UnRead : NotificationsBase
            {
                public List<Notification_content> content { get; set; }
            }

            /// <summary>
            /// 群组公告消息信息 topic：{appKey}/{groupId}
            /// </summary>
            public class Add : NotificationsBase
            {
                public Notification_content content { get; set; }
            }

            /// <summary>
            /// MQTT收到修改公告状态为已读（多终端同步）
            /// </summary>
            public class State : NotificationsBase
            {
                public Id content { get; set; }
            }

            /// <summary>
            /// MQTT收到删除讨论组公告
            /// </summary>
            public class Delete : NotificationsBase
            {
                public Id content { get; set; }
            }

            /// <summary>
            /// 群组公告ID信息
            /// </summary>
            public class Id
            {
                /// <summary>
                /// 公告ID
                /// </summary>
                public string notificationId { get; set; } = string.Empty;
            }

            /// <summary>
            /// 群公告内容信息
            /// </summary>
            public class Notification_content
            {
                /// <summary>
                /// 公告ID
                /// </summary>
                public string notificationId { get; set; } = string.Empty;

                /// <summary>
                /// 公告标题
                /// </summary>
                public string title { get; set; } = string.Empty;

                /// <summary>
                /// 公告创建时间
                /// </summary>
                public string createTime { get; set; } = string.Empty;

                /// <summary>
                /// 创建人ID
                /// </summary>
                public string createBy { get; set; } = string.Empty;

                /// <summary>
                /// 是否有附件，“0”表示没有，“1”表示有
                /// </summary>
                public string hasAttach { get; set; } = string.Empty;
            }

            /// <summary>
            /// 方法说明：触角SDK聊天消息和SDK聊天消息转换
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="entity"></param>
            /// <returns></returns>
            private static T GetReceiveAntSdkNotificationBaseInfo<T>(MsSdkNotificationBase entity) where T : NotificationsBase, new()
            {
                var sdkreceivemsgtypeValue = (long)entity.MsgType;
                var antsdkreceivemsgType = (AntSdkMsgType)sdkreceivemsgtypeValue;
                var result = new T
                {
                    MsgType = antsdkreceivemsgType,
                    sessionId = entity.sessionId,
                    chatIndex = entity.chatIndex,
                    chatType = entity.chatType,
                    os = entity.os,
                    flag = entity.flag,
                    status = entity.status,
                    messageId = entity.messageId,
                    sendUserId = entity.sendUserId,
                    targetId = entity.targetId,
                    sendTime = entity.sendTime,
                    attr = entity.attr
                };
                return result;
            }

            /// <summary>
            /// 方法说明：获取接收到的平台SDK聊天消息，转化为触角SDK聊天消息
            /// </summary>
            /// <param name="entity">SDK聊天信息</param>
            /// <returns>触角SDK聊天信息</returns>
            internal static NotificationsBase GetReceiveAntSdkNotificationInfo(MsSdkNotificationBase entity)
            {
                try
                {
                    var sdknotifacationreadObj = entity as MsUnReadNotifications;
                    if (sdknotifacationreadObj != null)
                    {
                        var antsdknoteficationMsg = GetReceiveAntSdkNotificationBaseInfo<UnRead>(sdknotifacationreadObj);
                        var notefacationList = new List<Notification_content>();
                        if (sdknotifacationreadObj.content?.Count > 0)
                        {
                            notefacationList.AddRange(
                                sdknotifacationreadObj.content.Where(c => c != null).Select(n => new Notification_content
                                {
                                    notificationId = n.notificationId,
                                    title = n.title,
                                    createTime = n.createTime,
                                    createBy = n.createBy,
                                    hasAttach = n.hasAttach
                                }));
                        }
                        antsdknoteficationMsg.content = notefacationList;
                        return antsdknoteficationMsg;
                    }
                    var sdknotifacationaddObj = entity as MsAddNotification;
                    if (sdknotifacationaddObj != null)
                    {
                        var antsdknotifacationMsg = GetReceiveAntSdkNotificationBaseInfo<Add>(sdknotifacationaddObj);
                        antsdknotifacationMsg.content = new Notification_content
                        {
                            notificationId = sdknotifacationaddObj.content?.notificationId,
                            title = sdknotifacationaddObj.content?.title,
                            createTime = sdknotifacationaddObj.content?.createTime,
                            createBy = sdknotifacationaddObj.content?.createBy,
                            hasAttach = sdknotifacationaddObj.content?.hasAttach
                        };
                        return antsdknotifacationMsg;
                    }
                    var sdknotifacationstateObj = entity as MsModifyNotificationState;
                    if (sdknotifacationstateObj != null)
                    {
                        var antsdknotificationstaMsg = GetReceiveAntSdkNotificationBaseInfo<State>(sdknotifacationstateObj);
                        antsdknotificationstaMsg.content = new Id
                        {
                            notificationId = sdknotifacationstateObj.content?.notificationId
                        };
                        return antsdknotificationstaMsg;
                    }
                    var sdknotifacationdeleteObj = entity as MsDeleteNotification;
                    if (sdknotifacationdeleteObj != null)
                    {
                        var antsdknotifacationdeleteMsg =
                            GetReceiveAntSdkNotificationBaseInfo<Delete>(sdknotifacationdeleteObj);
                        antsdknotifacationdeleteMsg.content = new Id
                        {
                            notificationId = sdknotifacationdeleteObj.content?.notificationId
                        };
                        return antsdknotifacationdeleteMsg;
                    }
                    //返回空
                    return null;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError(
                        $"[AntSdkChatMsg.GetReceiveAntSdkChatInfo]:{Environment.NewLine}{ex.Message}{ex.StackTrace}");
                    return null;
                }
            }
        }

        /// <summary>
        /// 触角SDK自定义消息实体：[支持4000-9999的自定义消息，仅仅是传输][自己完成触角Sdk消息类型（AntSdkMsSdkCustomEntity）的构造并且定义自己的content内容]
        /// </summary>
        public class Custom : AntSdkMsBase
        {
            /// <summary>
            /// 自定义消息类型
            /// </summary>
            public string messageType { get; set; } = string.Empty;

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
            /// 自定义消息内容[自己定义]
            /// </summary>
            public object content { get; set; }

            /// <summary>
            /// 方法说明：获取SDK回执发送实体
            /// </summary>
            /// <returns>SDK回执发送实体</returns>
            internal MsSdkCustomEntity GetSdkCtom()
            {
                var antsdkreceivemsgtypeValue = (long)MsgType;
                var sdkreceivemsgType = (SdkMsgType)antsdkreceivemsgtypeValue;
                var sdkSend = new MsSdkCustomEntity
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
                    attr = attr,
                    content = content
                };
                return sdkSend;
            }

            /// <summary>
            /// 方法说明：触角SDK聊天消息和SDK聊天消息转换
            /// </summary>
            /// <param name="entity"></param>
            /// <returns></returns>
            internal static Custom GetReceiveAntSdkCustomMsgInfo(MsSdkCustomEntity entity)
            {
                var sdkreceivemsgtypeValue = (long)entity.MsgType;
                var antsdkreceivemsgType = (AntSdkMsgType)sdkreceivemsgtypeValue;
                var result = new Custom
                {
                    MsgType = antsdkreceivemsgType,
                    sessionId = entity.sessionId,
                    chatIndex = entity.chatIndex,
                    chatType = entity.chatType,
                    os = entity.os,
                    flag = entity.flag,
                    status = entity.status,
                    messageId = entity.messageId,
                    sendUserId = entity.sendUserId,
                    targetId = entity.targetId,
                    sendTime = entity.sendTime,
                    attr = entity.attr
                };
                return result;
            }
        }

        /// <summary>
        /// MQTT收到多终端同步的已读回执 topic：{appKey}/userId 
        /// 例如：用户A（android端）发送已读回执时，sdkmessage收到这条消息后，会同时向用户A的ID发送已读回执的消息，用来做多终端同步，用户A的其他端收到同步消息后 
        /// 1）如果OS和自己一样，那么不需要处理 
        /// 2）如果OS和自己不一样，那么表示其他端已读该条消息，自己也要将该条消息标识为已读
        /// </summary>
        public class MultiTerminalSynch : AntSdkMsBase
        {

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
            /// 多终端同步已读回执信息
            /// </summary>
            public string content { get; set; } = string.Empty;

            /// <summary>
            /// 方法说明：触角SDK聊天消息和SDK聊天消息转换
            /// </summary>
            /// <param name="entity"></param>
            /// <returns></returns>
            internal static MultiTerminalSynch GetReceiveAntSdkMterminalSynchMsgInfo(MsMultiTerminalSynch entity)
            {
                var sdkreceivemsgtypeValue = (long)entity.MsgType;
                var antsdkreceivemsgType = (AntSdkMsgType)sdkreceivemsgtypeValue;
                var result = new MultiTerminalSynch
                {
                    MsgType = antsdkreceivemsgType,
                    sessionId = entity.sessionId,
                    chatIndex = entity.chatIndex,
                    chatType = entity.chatType,
                    os = entity.os,
                    flag = entity.flag,
                    status = entity.status,
                    messageId = entity.messageId,
                    sendUserId = entity.sendUserId,
                    targetId = entity.targetId,
                    sendTime = entity.sendTime,
                    attr = entity.attr
                };
                return result;
            }
        }

        /// <summary>
        /// MQTT收到点对点阅后即焚消息已读的回执 topic：{appKey}/userId 
        /// </summary>
        public class PointBurnReaded : AntSdkMsBase
        {

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
            /// 方法说明：触角SDK聊天消息和SDK聊天消息转换
            /// </summary>
            /// <param name="entity"></param>
            /// <returns></returns>
            internal static PointBurnReaded GetReceiveAntSdkMterminalSynchMsgInfo(MsPointBurnReaded entity)
            {
                var sdkreceivemsgtypeValue = (long)entity.MsgType;
                var antsdkreceivemsgType = (AntSdkMsgType)sdkreceivemsgtypeValue;
                var result = new PointBurnReaded
                {
                    MsgType = antsdkreceivemsgType,
                    sessionId = entity.sessionId,
                    chatIndex = entity.chatIndex,
                    chatType = entity.chatType,
                    os = entity.os,
                    flag = entity.flag,
                    status = entity.status,
                    messageId = entity.messageId,
                    sendUserId = entity.sendUserId,
                    targetId = entity.targetId,
                    sendTime = entity.sendTime,
                    attr = entity.attr,
                    content = new PointBurnReaded_content
                    {
                        readIndex = entity.content?.readIndex ?? 0,
                        messageId = entity.content?.messageId
                    }
                };
                return result;
            }

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
            /// 
            /// </summary>
            public int readIndex { get; set; }

            /// <summary>
            /// 已读消息的messageID
            /// </summary>
            public string messageId { get; set; } = string.Empty;
        }

        /// <summary>
        /// MQTT收到点对点文件消息的已接受的回执 topic：{appKey}/userId 
        /// </summary>
        public class PointFileAccepted : AntSdkMsBase
        {

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
            /// 点对点文件消息的已接受的回执 topic：{appKey}/userId 
            /// </summary>
            public string content { get; set; } = string.Empty;

            /// <summary>
            /// 方法说明：触角SDK聊天消息和SDK聊天消息转换
            /// </summary>
            /// <param name="entity"></param>
            /// <returns></returns>
            internal static PointFileAccepted GetReceiveAntSdkMterminalSynchMsgInfo(MsPointFileAccepted entity)
            {
                var sdkreceivemsgtypeValue = (long)entity.MsgType;
                var antsdkreceivemsgType = (AntSdkMsgType)sdkreceivemsgtypeValue;
                var result = new PointFileAccepted
                {
                    MsgType = antsdkreceivemsgType,
                    sessionId = entity.sessionId,
                    chatIndex = entity.chatIndex,
                    chatType = entity.chatType,
                    os = entity.os,
                    flag = entity.flag,
                    status = entity.status,
                    messageId = entity.messageId,
                    sendUserId = entity.sendUserId,
                    targetId = entity.targetId,
                    sendTime = entity.sendTime,
                    attr = entity.attr,
                    content = entity.content
                };
                return result;
            }
        }

        /// <summary>
        /// MQTT收到版本更新接口(硬更新）信息
        /// </summary>
        public class VersionHardUpdate : AntSdkMsBase
        {
            /// <summary>
            /// 版本号
            /// </summary>
            public string version { get; set; } = string.Empty;

            /// <summary>
            /// 自定义消息字段
            /// </summary>
            public VersionHardUpdate_content attr { get; set; }

            /// <summary>
            /// 获取应更新实体信息
            /// </summary>
            /// <param name="sdkEntity">平台SDK硬更新信息</param>
            /// <returns></returns>
            internal static VersionHardUpdate GetAntSdkVersionUpdate(MsVersionHardUpdate sdkEntity)
            {
                if (sdkEntity == null) { return null; }
                var sdkreceivemsgtypeValue = (long)sdkEntity.MsgType;
                var antsdkreceivemsgType = (AntSdkMsgType)sdkreceivemsgtypeValue;
                var antsdkEntity = new VersionHardUpdate
                {
                    MsgType = antsdkreceivemsgType,
                    version = sdkEntity.version,
                    attr = new VersionHardUpdate_content
                    {
                        describe = sdkEntity.attr?.describe,
                        title = sdkEntity.attr?.title,
                        updateType = sdkEntity.attr?.updateType,
                        url = sdkEntity.attr?.url,
                        isDeleteDB = sdkEntity.attr?.isDeleteDB
                    }
                };
                return antsdkEntity;
            }
        }

        /// <summary>
        /// 版本更新接口(硬更新）信息
        /// </summary>
        public class VersionHardUpdate_content
        {
            /// <summary>
            /// 描述更新的内容
            /// </summary>
            public string describe { get; set; } = string.Empty;

            /// <summary>
            /// 更新的名称
            /// </summary>
            public string title { get; set; } = string.Empty;

            /// <summary>
            /// 更新类型,1:软更新，2：硬更新 固定值为2
            /// </summary>
            public string updateType { get; set; } = string.Empty;

            /// <summary>
            /// 更新url
            /// </summary>
            public string url { get; set; } = string.Empty;

            /// <summary>
            /// 是否删除数据库，1–不删除 2–删除，默认值为1（不删除）
            /// </summary>
            public string isDeleteDB { get; set; } = string.Empty;
        }

        //[0][0] messageType 消息类型 String 2301 
        //[1][0] dataVersion 版本号 String
        //[1][1] attr 自定义消息字段 String jsonString
        /// <summary>
        /// MQTT收到组织架构信息
        /// </summary>
        public class OrganizationModify : AntSdkMsBase
        {
            /// <summary>
            /// dataVersion 版本号 String
            /// </summary>
            public string dataVersion { get; set; } = string.Empty;

            /// <summary>
            /// attr 自定义消息字段 String jsonString
            /// </summary>
            public string attr { get; set; } = string.Empty;

            /// <summary>
            /// 组织架构变更通知
            /// </summary>
            /// <param name="sdkEntity"></param>
            /// <returns></returns>
            internal static OrganizationModify GetOrganizationModify(MsOrganizationModify sdkEntity)
            {
                if (sdkEntity == null) { return null; }
                var sdkreceivemsgtypeValue = (long)sdkEntity.MsgType;
                var antsdkreceivemsgType = (AntSdkMsgType)sdkreceivemsgtypeValue;
                var antsdkEntity = new OrganizationModify
                {
                    MsgType = antsdkreceivemsgType,
                    dataVersion = sdkEntity.dataVersion,
                    attr = sdkEntity.attr
                };
                return antsdkEntity;
            }
        }

        /// <summary>
        /// 用户个性化设置（需要发送已读回执）
        /// </summary>
        public class Individuation : AntSdkMsBase
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
            public Individuation_content content { get; set; }

            /// <summary>
            /// 获取应更新实体信息
            /// </summary>
            /// <param name="sdkEntity">平台SDK硬更新信息</param>
            /// <returns></returns>
            internal static Individuation GetAntSdkIndividuation(MsIndividuationSet sdkEntity)
            {
                if (sdkEntity == null) { return null; }
                var sdkreceivemsgtypeValue = (long)sdkEntity.MsgType;
                var antsdkreceivemsgType = (AntSdkMsgType)sdkreceivemsgtypeValue;
                var antsdkEntity = new Individuation
                {
                    MsgType = antsdkreceivemsgType,
                    sessionId = sdkEntity.sessionId,
                    chatIndex = sdkEntity.chatIndex,
                    sendTime = sdkEntity.sendTime,
                    content = new Individuation_content
                    {
                        chatType = sdkEntity.content?.chatType ?? 0,
                        targetId = sdkEntity.content?.targetId,
                        state = sdkEntity.content?.state
                    }
                };
                return antsdkEntity;
            }
        }

        /// <summary>
        /// 用户个性化设置内容
        /// </summary>
        public class Individuation_content
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

        /// <summary>
        /// 打卡类
        /// </summary>
        public class AttendanceRecordVerify : AntSdkMsBase
        {
            ///// <summary>
            ///// 会话ID
            ///// </summary>
            //public string sessionId { get; set; } = string.Empty;

            ///// <summary>
            ///// 通知游标（会话内消息的index）
            ///// </summary>
            //public string chatIndex { get; set; } = string.Empty;

            public int flag { get; set; }

            /// <summary>
            /// 消息发送时间
            /// </summary>
            public string sendTime { get; set; } = string.Empty;
            /// <summary>
            /// 原始的消息信息（用来进行存储或者转存后的解析）
            /// </summary>
            public AttendanceRecord_content content { get; set; }
            /// <summary>
            /// 获取应更新实体信息
            /// </summary>
            /// <param name="sdkEntity">平台SDK硬更新信息</param>
            /// <returns></returns>
            internal static AttendanceRecordVerify GetAntSdkAttendanceRecordVerify(MsAttendanceRecordVerify sdkEntity)
            {
                if (sdkEntity == null) { return null; }
                var sdkreceivemsgtypeValue = (long)sdkEntity.MsgType;
                var antsdkreceivemsgType = (AntSdkMsgType)sdkreceivemsgtypeValue;
                var antsdkEntity = new AttendanceRecordVerify
                {
                    MsgType = antsdkreceivemsgType,
                    sessionId = sdkEntity.sessionId,
                    chatIndex = sdkEntity.chatIndex,
                    sendTime = sdkEntity.sendTime,
                    content = new AttendanceRecord_content
                    {
                        address=sdkEntity.content?.address,
                        attendId = sdkEntity.content?.attendId,
                        attendTime = sdkEntity.content?.attendTime,
                        userId = sdkEntity.content?.userId,
                        flag = sdkEntity.content != null ? sdkEntity.content.flag : false
                    }
                };
                return antsdkEntity;
            }

        }
        /// <summary>
        /// 打卡验证的需要的内容
        /// </summary>
        public class AttendanceRecord_content
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
}
