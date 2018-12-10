using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service
{
    internal class SdkEnumCollection
    {
        /// <summary>
        /// 请求类型
        /// </summary>
        public enum RequestMethod
        {
            PATCH,
            POST,
            GET,
            DELETE,
            PUT
        }

        /// <summary>
        /// 操作系统类型
        /// </summary>
        public enum OSType
        {
            PC = 1,
            Web = 2,
            Android = 3,
            IOS = 4,
            JavaServer = 5
        }

        /// <summary>
        /// 聊天类型
        /// </summary>
        public enum ChatType
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
        /// 阅后即焚标识
        /// </summary>
        public enum BurnFlag
        {
            /// <summary>
            /// 普通消息
            /// </summary>
            Normal = 0,

            /// <summary>
            /// 阅后即焚
            /// </summary>
            Burn = 1
        }

        /// <summary>
        /// 消息状态（是否已读）
        /// </summary>
        public enum ReadStatus
        {
            /// <summary>
            /// 未读
            /// </summary>
            NoRead = 0,

            /// <summary>
            /// 已读
            /// </summary>
            Readed = 1
        }

        /// <summary>
        /// 是否允许多端登录
        /// </summary>
        public enum Ways
        {
            /// <summary>
            /// 允许多端登录
            /// </summary>
            Multi = 0,

            /// <summary>
            /// 允许一端登录
            /// </summary>
            One = 1
        }

        /// <summary>
        /// App运行状态
        /// </summary>
        public enum AppRunState
        {
            /// <summary>
            /// App前台正常运行
            /// </summary>
            Proscenium = 0,

            /// <summary>
            /// 后台运行
            /// </summary>
            Background = 1
        }


        /// <summary>
        /// 终端消息类型
        /// </summary>
        public enum TerminalInfoType
        {
            /// <summary>
            /// 表示心跳消息，终端没有收发消息时，每隔60秒发送一次心跳 
            /// </summary>
            HeartInfo = 0,

            /// <summary>
            /// 请求离线消息，终端上线发送
            /// </summary>
            QuestOffLineInfo = 1
        }

        public static string[] TerminalInfoTypeArray = { "0001", "0002" };

        #region MQTT接收消息类型相关枚举



        /// <summary>
        ///聊天消息类型（需要发送回执）
        /// </summary>
        public enum ChatMsgType
        {
            /// <summary>
            /// 文本
            /// </summary>
            Text = 1001,

            /// <summary>
            /// 图片
            /// </summary>
            Picture = 1002,

            /// <summary>
            /// 音频
            /// </summary>
            Audio = 1003,

            /// <summary>
            /// 视频
            /// </summary>
            Video = 1004,

            /// <summary>
            /// 文件
            /// </summary>
            File = 1005,

            /// <summary>
            /// 地理位置
            /// </summary>
            MapLocation = 1006,

            /// <summary>
            /// 图文混合
            /// </summary>
            MixImageText = 1007,

            /// <summary>
            /// @消息
            /// </summary>
            At = 1008,

            /// <summary>
            /// 多人音频视频
            /// </summary>
            MultiAudioVideo = 1009,

            /// <summary>
            /// 群发助手
            /// </summary>
            MultiSendMsg = 1012,

            /// <summary>
            /// 单人（点对点）音频视频消息
            /// </summary>
            PointAudioVideo = 1013,

            /// <summary>
            /// 撤销消息
            /// </summary>
            Revocation = 1014,

            /// <summary>
            /// 创建投票
            /// </summary>
            CreateVote = 1015,

            /// <summary>
            /// 删除投票
            /// </summary>
            DeleteVote = 1017,

            /// <summary>
            /// 创建活动
            /// </summary>
            CreateActivity = 1016,

            /// <summary>
            /// 删除活动
            /// </summary>
            DeleteActivity = 1018

        }

        /// <summary>
        /// 用户状态通知
        /// </summary>
        public enum UserStateNotify
        {
            /// <summary>
            /// 用户离线
            /// </summary>
            OffLine = 2100,

            /// <summary>
            /// 用户在线
            /// </summary>
            OnLine = 2101,

            /// <summary>
            /// 用户离开
            /// </summary>
            Leave = 2102,

            /// <summary>
            /// 用户忙碌
            /// </summary>
            Busy = 2103,

            /// <summary>
            /// 手机端在线
            /// </summary>
            PhoneLine = 2104,

            /// <summary>
            /// 用户账号被停用
            /// </summary>
            Disable = 2109,

            /// <summary>
            /// 用户信息变更（包括头像、名称、个性签名等信息的变更）
            /// </summary>
            ModifyInfo = 2110,

            /// <summary>
            /// 用户修改密码，踢出登录
            /// </summary>
            PaswordChangeKickOut = 2111,

            /// <summary>
            /// 用户被踢出登录
            /// </summary>
            KickOut = 2112
        }

        /// <summary>
        /// 聊天室通知（需要发送回执）
        /// </summary>
        public enum ChatRoomNotify
        {
            /// <summary>
            /// 创建聊天室
            /// </summary>
            Create = 2201,

            /// <summary>
            /// 解散聊天室
            /// </summary>
            Dismiss = 2202,

            /// <summary>
            /// 聊天室成员变更（增加成员）
            /// </summary>
            AddMember = 2203,

            /// <summary>
            /// 聊天室成员变更（删除成员）
            /// </summary>
            DeleteMember = 2204,

            /// <summary>
            /// 聊天室成员变更（成员退出）
            /// </summary>
            QuitMember = 2205,

            /// <summary>
            /// 聊天室成员变更（备注信息的变更等等） 
            /// </summary>
            ModifyMemberInfo = 2206,

            /// <summary>
            /// 聊天室更新(包括头像、名称等变更) 
            /// </summary>
            Modify = 2207,

            /// <summary>
            /// 聊天室是阅后即焚模式
            /// </summary>
            BurnMode = 2291,

            /// <summary>
            /// 聊天室是普通消息模式
            /// </summary>
            NormalMode = 2292
        }

        /// <summary>
        /// 讨论组通知（需要发送回执）
        /// </summary>
        public enum DiscussGroupInfoNotify
        {
            /// <summary>
            /// 创建讨论组
            /// </summary>
            Create = 2501,

            /// <summary>
            /// 解散讨论组
            /// </summary>
            Dismiss = 2502,

            /// <summary>
            /// 讨论组成员变更（增加成员）
            /// </summary>
            AddMember = 2503,

            /// <summary>
            /// 讨论组成员变更（删除成员） 
            /// </summary>
            DeleteMember = 2504,

            /// <summary>
            /// 讨论组成员变更（成员退出）
            /// </summary>
            QuitMember = 2505,

            /// <summary>
            /// 讨论组成员变更（成员备注信息的变更等等） 
            /// </summary>
            ModifyMemberInfo = 2506,

            /// <summary>
            /// 讨论组变更（包括头像、名称等变更）
            /// </summary>
            Modify = 2507,

            /// <summary>
            /// 讨论组是阅后即焚模式
            /// </summary>
            BurnMode = 2591,

            /// <summary>
            /// 群主在阅后即焚模式下删除消息
            /// </summary>
            BurnModeDelete = 2592,

            /// <summary>
            /// 讨论组是普通消息模式
            /// </summary>
            NormalMode = 2593,

            /// <summary>
            /// 群主变更
            /// </summary>
            OwnerChanged = 2508,

            /// <summary>
            /// 管理员授权
            /// </summary>
            AdminSet = 2509
        }

        /// <summary>
        /// 讨论组公告消息
        /// </summary>
        public enum NotificationMsgType
        {
            /// <summary>
            /// 用户登录时获取讨论组公告列表（用户未读的）topic：{appKey}/{userId}/{os} 
            /// </summary>
            UnReadNotifications = 2580,

            /// <summary>
            /// 新增讨论组公告 topic：{appKey}/{groupId}
            /// </summary>
            AddNotification = 2581,

            /// <summary>
            /// 删除讨论组公告 topic：{appKey}/{groupId}
            /// </summary>
            DeleteNotification = 2582,

            /// <summary>
            /// 修改公告状态为已读（多终端同步） topic：{appKey}/{userId}
            /// </summary>
            ModifyNotificationState = 2583
        }

        /// <summary>
        /// 其他消息类型
        /// </summary>
        public enum OtherMsgType
        {
            /// <summary>
            /// 多终端同步已读回执 topic：{appKey}/userId 
            /// 例如：用户A（android端）发送已读回执时，sdkmessage收到这条消息后，会同时向用户A的ID发送已读回执的消息，用来做多终端同步，用户A的其他端收到同步消息后 
            /// 1）如果OS和自己一样，那么不需要处理 
            /// 2）如果OS和自己不一样，那么表示其他端已读该条消息，自己也要将该条消息标识为已读
            /// </summary>
            MultiTerminalSynch = 0999,

            /// <summary>
            /// 消息服务的回执（带chatIndex和sendTime） topic:{appKey}/{userId}/{os} 
            /// </summary>
            MsgReceipt = 1000,

            /// <summary>
            /// 点对点文件消息的已接受的回执 topic：{appKey}/userId 
            /// </summary>
            PointFileAccepted = 1010,

            /// <summary>
            /// 点对点阅后即焚消息已读的回执 topic：{appKey}/userId 
            /// 适用场景：用户A发送阅后即焚消息给用户B，用户B发送点对点阅后即焚已读回执，用户A收到该条消息之后，
            /// 解析content中的readIndex，删除对应的消息，并且需要向服务端发送已读回执 
            /// </summary>
            PointBurnReaded = 1011,

            /// <summary>
            /// 透传自定义消息（目前乐盈通使用，SDK后续定义4000-9999的消息类型为自定义类型）
            /// </summary>
            UnvarnishedMsg = 1998,

            /// <summary>
            /// 硬更新  topic：{appKey}/pc/version*** 
            /// </summary>
            VersionHardUpdate = 2000,

            /// <summary>
            /// 组织架构通知 topic：{appKey}
            /// </summary>
            OrganizationModify = 2301,

            /// <summary>
            /// 用户个性化设置（需要发送已读回执）topic：{appKey}/{userId}
            /// </summary>
            UserIndividuation = 2601,

            /// <summary>
            /// 打卡验证（需要发送已读回执）
            /// </summary>
            CheckInVerify = 2602,

            /// <summary>
            /// 打卡结果（需要发送已读回执）
            /// </summary>
            CheckInResult = 2603,
        }

        /// <summary>
        /// 邀请会话通知（需要发送回执）
        /// </summary>
        public enum InviteSessionNotify
        {
            /// <summary>
            /// 邀请加入会话的通知 
            /// </summary>
            InviteJoin = 2401,

            /// <summary>
            /// 被邀请者同意加入会话的通知
            /// </summary>
            AgreeJoin = 2402,

            /// <summary>
            /// 被邀请者拒绝加入会话的通知
            /// </summary>
            RejectJoin = 2403
        }
        #endregion

        #region   //发送消息主题与类型
        /// <summary>
        /// 发送消息类型
        /// </summary>
        public enum ChatMsgSendType
        {
            /// <summary>
            /// 正常聊天消息
            /// </summary>
            Nomal,

            /// <summary>
            /// 重发聊天消息
            /// </summary>
            Repeat,

            /// <summary>
            /// 机器人聊天消息
            /// </summary>
            Robot,

            /// <summary>
            /// @机器人聊天重发
            /// </summary>
            Rerobot
        }


        /// <summary>
        /// 发送消息的主题
        /// </summary>
        public enum TopicSend
        {
            /// <summary>
            /// 终端消息 主题
            /// </summary>
            sdk_user,

            /// <summary>
            /// 聊天消息、点对点阅后即焚消息已读回执 群阅后即焚消息 主题 
            /// </summary>
            sdk_send,

            /// <summary>
            /// 聊天消息、点对点阅后即焚消息已读回执 群阅后即焚消息 主题（重发） 
            /// </summary>
            sdk_resend,

            /// <summary>
            /// 聊天消息、点对点阅后即焚消息已读回执 群阅后即焚消息 主题（机器人）
            /// </summary>
            robot_send,

            /// <summary>
            /// 只有群发消息@机器人的，重发的主题
            /// </summary>
            robot_resend,

            /// <summary>
            /// 消息已读回执 主题
            /// </summary>
            sdk_read,

            /// <summary>
            /// 消息已收回执 主题
            /// </summary>
            sdk_receive
        }


        /// <summary>
        /// 发送的消息类型
        /// </summary>
        public enum SdkSendMsgType
        {
            /// <summary>
            /// 发送终端消息:发送心跳消息、请求离线消息 发送主题：sdk_user
            /// </summary>
            MsgTerminal,

            /// <summary>
            /// 发送聊天消息：文本、图片、音频、视频、文件、地理位置、图文混合、@消息、多人音频视频、点对点聊天文件消息接受的回执、群发助手 发送主题：sdk_send
            /// </summary>
            MsgChat,

            /// <summary>
            /// 消息已读回执：普通消息已读回执 发送主题：sdk_read
            /// </summary>
            ReadReceipt,

            /// <summary>
            /// 消息已收回执：普通消息已收回执 发送主题：sdk_receive
            /// </summary>
            ReceiveReceipt,

            /// <summary>
            /// 点对点阅后即焚消息已读的回执 发送主题：sdk_send
            /// 适用场景：用户A发送阅后即焚消息给用户B，用户B发送点对点阅后即焚已读回执，用户A收到该条消息之后，
            /// 解析content中的readIndex，删除对应的消息，并且需要向服务端发送已读回执 
            /// </summary>
            PointBurnReaded,

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

        #endregion

        #region   //判断Json串的类型：对象，一维数组， 二维数组，三维数组......
        /// <summary>
        /// JSON数据的格式类型
        /// </summary>
        public enum SdkJsonType
        {
            /// <summary>
            /// JSONObject
            /// </summary>
            JstObject,

            /// <summary>
            /// JSON三维数组Array
            /// </summary>
            JstThreeArray,

            /// <summary>
            /// JSON二维数组Array
            /// </summary>
            JstTwoArray,

            /// <summary>
            /// JSON一维数组Array
            /// </summary>
            JstOneArray,

            /// <summary>
            /// 不是JSON格式的字符串
            /// </summary>
            JstError
        }

        #endregion
    }
}
