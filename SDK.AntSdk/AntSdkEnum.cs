using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDK.AntSdk.AntModels;
using SDK.Service;
using SDK.Service.Models;

namespace SDK.AntSdk
{
    /// <summary>
    /// 接收到的MQTT消息事件类型
    /// 注册回调接收消息时可标记方式使用：
    /// AntSdkReceiveMsgType sdkReceiveMsgType = AntSdkReceiveMsgType.MultiTerminalSynch | AntSdkReceiveMsgType.PointFileAccepted |
    ///                                          AntSdkReceiveMsgType.PointBurnReaded    | AntSdkReceiveMsgType.OffLine           | 
    ///                                          AntSdkReceiveMsgType.Leave;/*..........*/
    /// </summary>
    //[Flags]
    public enum AntSdkMsgType : long
    {
        /// <summary>
        /// 未定义消息类型[初始化类型使用]
        /// </summary>
        UnDefineMsg = 0,

        /// <summary>
        /// 所有类型消息[注册回调时不区分消息类型进行回调接收]
        /// </summary>
        AllMessage = 1,

        /// <summary>
        /// 回车换行消息类型
        /// </summary>
        EnterLine = 2,

        /// <summary>
        /// 表示心跳消息，终端没有收发消息时，每隔60秒发送一次心跳
        /// </summary>
        HeartBeat = 4,

        /// <summary>
        /// 请求离线消息，终端上线发送
        /// </summary>
        OffLineMsgRequest = 8,

        /// <summary>
        /// 多终端同步已读回执 topic：{appKey}/userId 
        /// 例如：用户A（android端）发送已读回执时，sdkmessage收到这条消息后，会同时向用户A的ID发送已读回执的消息，用来做多终端同步，用户A的其他端收到同步消息后 
        /// 1）如果OS和自己一样，那么不需要处理 
        /// 2）如果OS和自己不一样，那么表示其他端已读该条消息，自己也要将该条消息标识为已读
        /// </summary>
        MultiTerminalSynch = 16,

        /// <summary>
        /// 收到消息服务回执:目前只有自己发的聊天消息会收到该回执 topic:{appKey}/{userId}/{os}
        /// </summary>
        MsgReceipt = 32,

        /// <summary>
        /// 收到的聊天消息(文本)（需要发送回执）topic:{userId}
        /// </summary>
        ChatMsgText = 64,

        /// <summary>
        /// 收到的聊天消息(图片)（需要发送回执）topic:{userId}
        /// </summary>
        ChatMsgPicture = 128,

        /// <summary>
        /// 收到的聊天消息(音频)（需要发送回执）topic:{userId}
        /// </summary>
        ChatMsgAudio = 256,

        /// <summary>
        /// 收到的聊天消息(视频)（需要发送回执）topic:{userId}
        /// </summary>
        ChatMsgVideo = 512,

        /// <summary>
        /// 收到的聊天消息(文件)（需要发送回执）topic:{userId}
        /// </summary>
        ChatMsgFile = 1024,

        /// <summary>
        /// 收到的聊天消息(地理位置)（需要发送回执）topic:{userId}
        /// </summary>
        ChatMsgMapLocation = 2048,

        /// <summary>
        /// 收到的聊天消息(混合消息)（需要发送回执）topic:{userId}
        /// </summary>
        ChatMsgMixMessage = 4096,

        /// <summary>
        /// 收到的聊天消息(@消息)（需要发送回执）topic:{userId}
        /// </summary>
        ChatMsgAt = 8192,

        /// <summary>
        /// 收到的聊天消息(多人视频)（需要发送回执）topic:{userId}
        /// </summary>
        ChatMsgMultiAudioVideo = 16384,

        /// <summary>
        /// 点对点文件消息的已接受的回执 topic：{appKey}/userId 
        /// </summary>
        PointFileAccepted = 32768,

        /// <summary>
        /// 点对点阅后即焚消息已读的回执 topic：{appKey}/userId 
        /// 适用场景：用户A发送阅后即焚消息给用户B，用户B发送点对点阅后即焚已读回执，用户A收到该条消息之后，
        /// 解析content中的readIndex，删除对应的消息，并且需要向服务端发送已读回执 
        /// </summary>
        PointBurnReaded = 65536,

        /// <summary>
        /// 群发助手消息[消息发送群发时必须传，需要使用特殊sessionId = id,id2点对点聊天sessionId的生成 id为用户自己的ID id2固定为900000]
        /// </summary>
        MultiUserSend = 131072,

        /// <summary>
        /// 单人（点对点聊天）音频视频(需要发送回执)
        /// </summary>
        PointAudioVideo = 262144,

        /// <summary>
        /// 消息撤回
        /// </summary>
        Revocation = 524288,

        /// <summary>
        /// 创建投票
        /// </summary>
        CreateVote = 524289,

        /// <summary>
        /// 创建活动
        /// </summary>
        CreateActivity = 524290,

        /// <summary>
        /// 删除投票
        /// </summary>
        DeleteVote = 524291,

        /// <summary>
        /// 删除活动
        /// </summary>
        DeleteActivity = 524292,

        /// <summary>
        /// 透传自定义消息（目前乐盈通使用，SDK后续定义4000-9999的消息类型为自定义类型, 暂时保留乐盈通的1998透传消息）
        /// </summary>
        UnvarnishedMsg = 1048576,

        /// <summary>
        /// 硬更新  topic：{appKey}/pc/version*** 
        /// </summary>
        VersionHardUpdate = 2097152,

        /// <summary>
        /// 用户离线
        /// </summary>
        OffLine = 4194304,

        /// <summary>
        /// 用户在线
        /// </summary>
        OnLine = 8388608,

        /// <summary>
        /// 用户离开
        /// </summary>
        Leave = 16777216,

        /// <summary>
        /// 用户忙碌
        /// </summary>
        Busy = 33554432,

        /// <summary>
        /// 用户账号被停用
        /// </summary>
        Disable = 67108864,

        /// <summary>
        /// 用户信息变更（包括头像、名称、个性签名等信息的变更）
        /// </summary>
        ModifyInfo = 134217728,

        /// <summary>
        /// 用户修改密码，踢出登录
        /// </summary>
        PaswordChangeKickOut = 268435456,

        /// <summary>
        /// 用户被踢出登录
        /// </summary>
        KickOut = 536870912,

        /// <summary>
        /// 用户新增聊天室(需要发送已读回执) topic：{appKey}/{userId} 
        /// </summary>
        CreateChatRoom = 1073741824,

        /// <summary>
        /// 解散聊天室（需要发送已读回执）topic：{appKey}/{roomId}
        /// </summary>
        DismissChatRoom = 2147483648,

        /// <summary>
        /// 聊天室删除成员（需发送已读回执）topic：{appKey}/{roomId}
        /// </summary>
        DeleteChatRoomMember = 4294967296,

        /// <summary>
        /// 成员退出聊天室（需发送已读回执）topic：{appKey}/{roomId}
        /// </summary>
        QuitChatRoomMember = 8589934592,

        /// <summary>
        /// 聊天室添加成员（需要发送已读回执）topic：{appKey}/{roomId}
        /// </summary>
        AddChatRoomMember = 17179869184,

        /// <summary>
        /// 聊天室更新成员信息（需要发送已读回执）topic：{appKey}/{roomId}
        /// </summary>
        ModifyChatRoomMember = 34359738368,

        /// <summary>
        /// 聊天室信息变更（需要发送已读回执）topic：{appKey}/{roomId}
        /// </summary>
        ModifyChatRoom = 68719476736,

        /// <summary>
        /// 组织架构变更(其中appKey是app的唯一标识) topic：{appKey}
        /// </summary>
        OrganizationModify = 137438953472,

        /// <summary>
        /// 邀请加入会话
        /// </summary>
        InviteJoin = 274877906944,

        /// <summary>
        /// 处理邀请会话(同意或拒绝邀请)
        /// </summary>
        HandleInvite = 549755813888,

        /// <summary>
        /// 用户新增讨论组(需要发送已读回执) topic：{appKey}/{groupId}
        /// </summary>
        CreateGroup = 1099511627776,

        /// <summary>
        /// 讨论组解散（需要发送已读回执）topic：{appKey}/{groupId}
        /// </summary>
        DissolveGroup = 2199023255552,

        /// <summary>
        /// 讨论组新增成员（需要发送已读回执）topic：{appKey}/{groupId}
        /// </summary>
        AddGroupMember = 4398046511104,

        /// <summary>
        /// 讨论组删除成员（需要发送已读回执）topic：{appKey}/{groupId}
        /// </summary>
        DeleteGroupMember = 8796093022208,

        /// <summary>
        /// 成员退出讨论组（需要发送已读回执）topic：{appKey}/{groupId}
        /// </summary>
        QuitGroupMember = 17592186044416,

        /// <summary>
        /// 讨论组成员信息变更(需要发送已读回执) topic：{appKey}/{groupId}
        /// </summary>
        ModifyGroupMember = 35184372088832,

        /// <summary>
        /// 讨论组信息变更(需要发送已读回执) topic：{appKey}/{groupId}
        /// </summary>
        ModifyGroup = 70368744177664,

        /// <summary>
        /// 讨论组群主变更(需要发送已读回执) topic：{appKey}/{groupId}
        /// </summary>
        GroupOwnerChanged = 140737488355328,

        /// <summary>
        /// 用户登录时获取讨论组公告列表（用户未读的）topic：{appKey}/{userId}/{os} 
        /// </summary>
        UnReadNotifications = 281474976710656,

        /// <summary>
        /// 新增讨论组公告 topic：{appKey}/{userId}/{os} [用户登录时获取讨论组公告列表（用户未读的）]/topic：{appKey}/{groupId}[新增讨论组公告]
        /// </summary>
        AddNotification = 562949953421312,

        /// <summary>
        /// 删除讨论组公告 topic：{appKey}/{groupId}
        /// </summary>
        DeleteNotification = 1125899906842624,

        /// <summary>
        /// 修改公告状态为已读（多终端同步）topic：{appKey}/{userId}
        /// </summary>
        ModifyNotificationState = 2251799813685248,

        /// <summary>
        /// 群主切换为阅后即焚 topic：{appKey}/{groupId}
        /// </summary>
        GroupOwnerBurnMode = 4503599627370496,

        /// <summary>
        /// 群主在阅后即焚模式下清空消息 topic：{appKey}/{groupId}
        /// </summary>
        GroupOwnerBurnDelete = 9007199254740992,

        /// <summary>
        /// 群主切换为普通消息模式 topic：{appKey}/{groupId}
        /// </summary>
        GroupOwnerNormal = 18014398509481984,

        /// <summary>
        /// 用户个性化设置（需要发送已读回执）包括群组免打扰、聊天室免打扰、个人免打扰 topic：{appKey}/{userId} 
        /// </summary>
        UserIndividuation = 36028797018963968,

        /// <summary>
        /// 打卡验证（需要发送已读回执）
        /// </summary>
        CheckInVerify = 36028797018963969,

        /// <summary>
        /// 打卡结果（需要发送已读回执）
        /// </summary>
        CheckInResult = 36028797018963970,


        /// <summary>
        /// 自定义消息[4000,9999] 或透传消息1998
        /// </summary>
        CustomMessage = 72057594037927936,

        /// <summary>
        /// 离线消息:正常聊天消息[接收]
        /// </summary>
        OffLineMessageChatMsg = 144115188075855872,

        /// <summary>
        /// 群管理员授权
        /// </summary>
        AdminSet = 288230376151711744,

        /// <summary>
        /// 手机登录
        /// </summary>
        PhoneLine = 576460752303423488,

        /// <summary>
        /// 离线消息：除正常聊天消息之外的消息[接收]
        /// </summary>
        OffLineMessageOtherMsg = 1152921504606846976

    }

    /// <summary>
    /// 普通消息/自定义消息 已读/收 回执
    /// </summary>
    public enum AntSdkReceiptType
    {
        /// <summary>
        /// 普通消息已读回执
        /// </summary>
        ReadReceipt,

        /// <summary>
        /// 普通消息已收回执
        /// </summary>
        ReceiveReceipt
    }

    /// <summary>
    /// 群主切换的阅后即焚、普通消息、阅后即焚模式下删除消息的类型
    /// </summary>
    public enum AntSdkGroupChangeMode
    {
        /// <summary>
        /// 群主发的讨论组是阅后即焚模式 发送主题：sdk_send
        /// </summary>
        BurnMode,

        /// <summary>
        /// 群主发的讨论组是普通消息模式 发送主题：sdk_send
        /// </summary>
        NormalMode,

        /// <summary>
        /// 群主发的在阅后即焚模式下删除消息 发送主题：sdk_send
        /// </summary>
        BurnModeDelete
    }

    /// <summary>
    /// 触角SDK中日志级别相关的布尔属性模式，可扩展
    /// <code>
    ///     //初始化SDK日志对象
    ///     AntSdkLogLevel sdklogMode = AntSdkLogLevel.DebugLogEnable | AntSdkLogLevel.ErrorLogEnable |
    ///     AntSdkLogLevel.FatalLogEnable | AntSdkLogLevel.InfoLogEnable | AntSdkLogLevel.WarnLogEnable;
    /// </code>
    /// </summary>
    [Flags]
    public enum AntSdkLogLevel
    {
        /// <summary>
        /// 默认,不设置(单独使用，不能同其他枚举值进行或操作)
        /// </summary>
        Default = 0,

        /// <summary>
        /// 调试
        /// </summary>
        DebugLogEnable = 1,

        /// <summary>
        /// 操作
        /// </summary>
        InfoLogEnable = 2,

        /// <summary>
        /// 警告
        /// </summary>
        WarnLogEnable = 4,

        /// <summary>
        /// 错误
        /// </summary>
        ErrorLogEnable = 8,

        /// <summary>
        /// 致命
        /// </summary>
        FatalLogEnable = 16
    }

    /// <summary>
    /// 聊天消息的阅后即焚标识
    /// </summary>
    public enum AntSdkBurnFlag
    {
        /// <summary>
        /// 正常消息
        /// </summary>
        NotIsBurn = 0,
        /// <summary>
        /// 阅后即焚消息
        /// </summary>
        IsBurn = 1,
    }

    /// <summary>
    /// 聊天类型 1：点对点聊天，2：群聊
    /// </summary>
    public enum AntSdkchatType
    {
        /// <summary>
        /// 默认,不设置(单独使用，不能同其他枚举值进行或操作)
        /// </summary>
        Default = 0,

        /// <summary>
        /// 单聊
        /// </summary>
        Point = 1,

        /// <summary>
        /// 聊天室
        /// </summary>
        Room = 2,

        /// <summary>
        /// 评论
        /// </summary>
        Comment = 3,

        /// <summary>
        /// 群聊
        /// </summary>
        Group = 4
    }

    /// <summary>
    /// 个性化设置状态
    /// </summary>
    public enum AntSdkIndividState
    {
        /// <summary>
        /// 默认,不设置(单独使用，不能同其他枚举值进行或操作)
        /// </summary>
        Default = 0,

        /// <summary>
        /// 接收消息
        /// </summary>
        Accept = 1,

        /// <summary>
        /// 免打扰
        /// </summary>
        Undisturb = 2,

        /// <summary>
        /// 屏蔽
        /// </summary>
        Block = 3
    }

    /// <summary>
    /// 消息状态 1：已读，0：未读
    /// </summary>
    public enum AntSdkMsgStatus
    {
        /// <summary>
        /// 未读
        /// </summary>
        UnRead = 0,

        /// <summary>
        /// 已读
        /// </summary>
        Readed = 1
    }

    /// <summary>
    /// At消息类型：0000：回车换行；1001：文本(包含表情)内容；1002：图片内容；1111：@全体成员；1112：@普通成员；
    /// </summary>
    public class AntSdkAtMsgType
    {
        /// <summary>
        /// 回车换行:0000
        /// </summary>
        public static string Enter = "0000";

        /// <summary>
        /// 文本内容:1001
        /// </summary>
        public static string Text = "1001";

        /// <summary>
        /// 图片内容:1002
        /// </summary>
        public static string Picture = "1002";

        /// <summary>
        /// @全体成员:1111
        /// </summary>
        public static string AtAll = "1111";

        /// <summary>
        /// @普通成员:1112
        /// </summary>
        public static string AtPerson = "1112";
    }
}
