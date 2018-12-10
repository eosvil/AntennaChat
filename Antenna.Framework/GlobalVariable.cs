using Antenna.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using SDK.AntSdk;

namespace Antenna.Framework
{
    /// <summary>
    /// 文件名:  GlobalVariable.cs
    /// Copyright (c) 2016  华海乐盈
    /// 描  述: 全局变量类（保存常量、枚举）
    /// </summary>
    public class GlobalVariable
    {
        public const string AES_Key = "hhly#*&^%$#@!)*(";//消息加密秘钥
        public const string AES_IV = "hhly#*&^%$#@!)*(";//偏移量(目前设置和秘钥相同)
        public static string CompanyCode = "C10086";//公司代码
        public static string Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();//程序版本号
        public static volatile bool isLoginOut = false;//是否是通过注销登录重新打开的登录界面
        public static IntPtr winHandle;//窗体句柄
        public static bool isMinimized = false;//窗体是否最小化
        public const string MassAssistantId = "P123456789000000";
        public const string AttendAssistantId = "P123456785000000";
        public static string wherefrom = "";
        public static SystemSetting systemSetting = new SystemSetting();//系统设置信息
        public static bool isCutShow = false;//截图窗口是否已经打开 防止重复打开截图
        public static bool isAudioShow = false;//音视频通话是否已经打开
        public static bool isRequestShow = false;//是否收到语音电话请求
        public static string currentAudioUserId = string.Empty;//当前语音通话的对方id
        public static int UserCurrentOnlineState = 0;
        public static bool CurrentUserIsIdleState;
        public static bool CurrentUserIsFirstLogin;
        public static DateTime LastLoginDatetime = DateTime.Now;
        public static bool MainWinIsMinimized = false;
        public static string fileSavePath = string.Empty;

        /// <summary>
        /// 主题集合
        /// </summary>
        public class TopicClass
        {
            public const string UpdatePcVersion = "update/pc/version";//版本更新
            public const string MessageRead = "message_read";//消息已读
            public const string MessageReceive = "message_receive";//消息已收
            public const string MessageSend = "message_send";//消息发送
            public const string MessageBurn = "message_burn";//用户B收到A的阅后即焚消息，发送已读回执给消息服务
        }

        /// <summary>
        /// 用于反射对应ViewModel所在的命名空间
        /// </summary>
        public static string viewPath = "";

        /// <summary>
        /// Dialog返回值
        /// </summary>
        public enum ShowDialogResult
        {
            Ok = 0,//点击“确定”
            Yes = 1,//点击“是”
            No = 2,//点击“否” 
            Cancel = 3, //点击“取消”
            Close = 4//点击右上角“X”
        }
        public class RevocationPrompt
        {
            public const string Msessage = "撤回了一条消息";
            public const string AutoReplyMessage = "您好，我现在有事不在，一会再和您联系。";
            public const string RobotFirstMsg = "你好，我是小柒机器人，你的智能帮手。";
        }
        public class NotifyIcon
        {
            public const string NotifyIconOnLine = "/AntennaChat;component/Images/ant_onLine.ico";
            public const string NotifyIconBusy = "/AntennaChat;component/Images/ant_busy.ico";
            public const string NotifyIconLeave = "/AntennaChat;component/Images/ant_leave.ico";
            public const string NotifyIconOffline = "/AntennaChat;component/ant_offLine.ico";
        }
        public class VotePrompt
        {
            public const string VoteSubjectIsEmpty = "请输入投票标题";
            public const string VoteOptionIsEmpty = "请输入投票选项";
            public const string VoteOptionNum = "投票选项不能少于2个";
            public const string VoteOptionRepeatContent = "投票选项内容不能有重复";
            public const string VoteEndDateTimePrompt = "截止时间与当前时间间隔小于30分钟";
            public const string VoteEndDateExceedCurrent = "投票截止时间超过了当前时间30天，请重新选择合适的时间。";
        }
        /// <summary>
        /// 处理结果（成功或失败）
        /// </summary>
        public enum Result
        {
            Failure = 0,//0-失败
            Success = 1//1-成功
        }

        /// <summary>
        /// 系统类型
        /// </summary>
        public enum OSType
        {
            PC = 1,//电脑终端
            Web = 2,//网页
            Android = 3,
            IOS = 4
        }

        /// <summary>
        /// 系统用户通知的消息类型（对应type）
        /// </summary>
        /// type=101：在线登陆 102：离开登陆 103：忙碌登陆 104: 用户信息更新 105: 用户被踢出登陆 100：退出
        /// type=201：讨论组新增 202：讨论组删除 203：讨论组成员更新 204: 讨论组基本信息更新 
        /// type=301：更新组织架构
        public enum SysUserMsgType
        {
            Logout = 100,//退出
            OnlineLogin = 101,//在线登录
            LeaveLogin = 102,//离开登录
            BusyLogin = 103,//忙碌登录
            UpdateUserInfo = 104,//用户信息更新
            TickOut = 105,//用户被踢出登录
            UpdatePwd = 106,//修改密码
            CreateGroup = 201,//讨论组新增
            DeleteGroup = 202,//讨论组解散
            UpdateGroupMemeber = 203,//讨论组成员更新（成员删除、踢出、新增）
            UpdateGroupInfo = 204,//讨论组基本信息更新
            GroupState = 205,//讨论组免打扰
            ContactsUpdate = 301,//组织架构更新
            Notice_Login = 401,//通知——登录推送
            Notice_Add = 402,//通知——添加推送
            Notice_Delete = 403,//通知——删除推送
            Notice_Read = 404,//通知——修改已读状态推送
            GroupBurnAfterReadMode = 501,//管理员将群由普通模式切换到阅后即焚模式
            GroupNoBurnAfterReadMode = 502,//管理员将群由阅后即焚模式切换到普通模式
            DeleteMsgBurnMode = 503//群主在阅后即焚模式下删除消息
        }
        /// <summary>
        /// 默认图片
        /// </summary>
        public class DefaultImage
        {
            public const string UserHeadDefaultImage = "pack://application:,,,/AntennaChat;Component/Images/27-头像.png";
            public const string SessionUserHeadDefaultImage = "pack://application:,,,/AntennaChat;Component/Images/44-头像.png";
            public const string MassAssistantDefaultImage = "pack://application:,,,/AntennaChat;Component/Images/群发头像-2.png";
            public const string AttendanceAssistantDefaultImage = "pack://application:,,,/AntennaChat;Component/Images/AttendanceAssistant.png";
            public const string GroupHeadDefaultImage = "pack://application:,,,/AntennaChat;Component/Images/27-头像.png";
            public const string SessionGroupHeadDefaultImage = "pack://application:,,,/AntennaChat;Component/Images/44-头像.png";
            //public const string
        }
        /// <summary>
        ///考勤图片资源
        /// </summary>
        public class AttendanceImage
        {
            public const string AttendValidationFailsIcon = "pack://application:,,,/AntennaChat;Component/Images/AttendValidationFails.png";
            public const string AttendVerificationFailedIcon = "pack://application:,,,/AntennaChat;Component/Images/AttendVerificationFailed.png";
        }
        /// <summary>
        /// 在线状态
        /// </summary>
        public enum OnLineStatus
        {
            OffLine = 0,//离线
            OnLine = 1,//在线
            Busy = 2,//忙碌
            Leave = 3,//离开
            OtherOnLine = 4//其它端在线
        }
        /// <summary>
        /// 在线状态信息
        /// </summary>
        public class UserOnlineSataeInfo
        {
            /// <summary>
            /// 在线状态大图标（下拉菜单、消息列表使用）
            /// </summary>
            public const string OnLineSatateMaxIcon = "/AntennaChat;component/Images/OnLineStatus/online_max.png";
            /// <summary>
            /// 忙碌状态大图标（下拉菜单、消息列表使用）
            /// </summary>
            public const string BusySatateMaxIcon = "/AntennaChat;component/Images/OnLineStatus/busy_max.png";
            /// <summary>
            /// 离开状态大图标（下拉菜单、消息列表使用）
            /// </summary>
            public const string LeaveSatateMaxIcon = "/AntennaChat;component/Images/OnLineStatus/leave_max.png";
            /// <summary>
            /// 在线状态小图标（群成员、组织结构中联系人使用）
            /// </summary>
            public const string OnLineSatateMinIcon = "/AntennaChat;component/Images/OnLineStatus/online_min.png";
            /// <summary>
            /// 忙碌状态小图标（群成员、组织结构中联系人使用）
            /// </summary>
            public const string BusySatateMinIcon = "/AntennaChat;component/Images/OnLineStatus/busy_min.png";
            /// <summary>
            /// 离开状态小图标（群成员、组织结构中联系人使用）
            /// </summary>
            public const string LeaveSatateMinIcon = "/AntennaChat;component/Images/OnLineStatus/leave_min.png";
            /// <summary>
            /// 手机在线状态大图标（消息列表使用）
            /// </summary>
            public const string MobilePhoneOnlineMaxIcon = "/AntennaChat;component/Images/OnLineStatus/MobilePhoneOnline_max.png";
            /// <summary>
            /// 手机在线状态大图标（群成员、组织结构中联系人使用）
            /// </summary>
            public const string MobilePhoneOnlineMinIcon = "/AntennaChat;component/Images/OnLineStatus/MobilePhoneOnline_min.png";

            private static Dictionary<int, string> _userOnlineStateIconDic;
            /// <summary>
            /// 状态图片字典（下拉菜单、消息列表使用）
            /// </summary>
            public static Dictionary<int, string> UserOnlineStateIconDic
            {
                get
                {
                    _userOnlineStateIconDic = new Dictionary<int, string>
                    {
                        {(int)OnLineStatus.OnLine, OnLineSatateMaxIcon},
                        {(int)OnLineStatus.Busy, BusySatateMaxIcon},
                        {(int)OnLineStatus.Leave, LeaveSatateMaxIcon},
                        {(int)OnLineStatus.OtherOnLine, MobilePhoneOnlineMaxIcon}
                    };
                    return _userOnlineStateIconDic;
                }
            }
            private static Dictionary<int, string> _userOnlineStateMinIconDic;
            /// <summary>
            ///  状态图片字典（群成员、组织结构中联系人使用）
            /// </summary>
            public static Dictionary<int, string> UserOnlineStateMinIconDic
            {
                get
                {
                    _userOnlineStateMinIconDic = new Dictionary<int, string>
                    {
                        {(int)OnLineStatus.OnLine, OnLineSatateMinIcon},
                        {(int)OnLineStatus.Busy, BusySatateMinIcon},
                        {(int)OnLineStatus.Leave, LeaveSatateMinIcon},
                        {(int)OnLineStatus.OtherOnLine, MobilePhoneOnlineMinIcon}
                    };
                    return _userOnlineStateMinIconDic;
                }
            }
            private static List<UserOnlineState> _userOnlineStates;
            public static List<UserOnlineState> UserOnlineStates
            {
                get
                {
                    _userOnlineStates = new List<UserOnlineState>
                    {
                        new UserOnlineState
                        {
                            OnlineState = (int) OnLineStatus.OnLine,
                            StateContent = "在线",
                            StateImage =OnLineSatateMaxIcon
                        },
                        new UserOnlineState
                        {
                            OnlineState = (int) OnLineStatus.Busy,
                            StateContent = "忙碌",
                            StateImage =BusySatateMaxIcon
                        },
                        new UserOnlineState
                        {
                            OnlineState = (int) OnLineStatus.Leave,
                            StateContent = "离开",
                            StateImage = LeaveSatateMaxIcon
                        }
                        //new UserOnlineState
                        //{
                        //    OnlineState = (int) OnLineStatus.OffLine,
                        //    StateContent = "离线",
                        //    StateImage = ""
                        //}
                    };
                    return _userOnlineStates;
                }
            }
        }

        /// <summary>
        /// 联系人头像
        /// </summary>
        public class ContactHeadImage
        {
            public static List<ContactUserImage> UserHeadImages = new List<ContactUserImage>();
        }
        /// <summary>
        /// 系消息类型(对应mtp字段)
        /// </summary>
        public enum MsgType
        {
            /// <summary>
            /// 文本
            /// </summary>
            Text = 1,
            /// <summary>
            /// 图片
            /// </summary>
            Picture = 2,//
            /// <summary>
            /// 文件
            /// </summary>
            File = 3,
            /// <summary>
            /// 音频
            /// </summary>
            Voice = 4,
            /// <summary>
            /// 系统内部通知
            /// </summary>
            SysInternalMsg = 5,
            /// <summary>
            /// 系统用户通知
            /// </summary>
            SysUserMsg = 6,
            /// <summary>
            /// 图文混合消息
            /// </summary>
            MixPictureText = 7,
            /// <summary>
            /// 用户撤回消息
            /// </summary>
            WithdrawMsg = 8,
            /// <summary>
            /// @消息
            /// </summary>
            At = 9,
            /// <summary>
            /// 多人音频视频会议消息
            /// </summary>
            MultiAudioVideo = 10,
            /// <summary>
            /// 点对点阅后即焚的已读回执
            /// </summary>
            BurnAfterReadReceipt = 11,
            /// <summary>
            /// 文件消息回执
            /// </summary>
            FileMsgReceipt = 12,
            /// <summary>
            /// 群发消息（群发助手）
            /// </summary>
            MassMsg = 13
        }
        /// <summary>  
        /// 鼠标八大位置枚举  
        /// </summary>  
        public enum MouseLocationEnum
        {
            None,
            Left,
            Up,
            Right,
            Down,
            LeftUp,
            LeftDown,
            RightUp,
            RightDown
        }
        /// <summary>  
        ///   
        /// </summary>  
        public class RectangleAreaModel
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Width { get; set; }
            public double Height { get; set; }
        }
        /// <summary>  
        /// 鼠标状态  
        /// </summary>  
        public enum MouseActionEx
        {
            /// <summary>  
            /// 无  
            /// </summary>  
            None,
            /// <summary>  
            /// 拖拽  
            /// </summary>  
            Drag,
            /// <summary>  
            /// 拖动  
            /// </summary>  
            DragMove
        }

        public enum SculptureAction
        {
            Nomal,
            Video,
            Image,
        }

        /// <summary>
        /// 联系人控件所属容器（即调用位置）
        /// </summary>
        public enum ContactInfoViewContainer
        {
            ContactListView,//组织结构列表
            GroupEditWindowViewLeft,//编辑讨论组左侧
            GroupEditWindowViewRight,//编辑讨论组右侧
            MultiContactsSelectLeft,//群发消息时多联系人选择左侧
            MultiContactsSelectRight//群发消息时多联系人选择右侧
        }

        /// <summary>
        /// 讨论组成员角色等级 
        /// 0--普通成员 1--群主  2--管理员
        /// </summary>
        /// </summary>
        public enum GroupRoleLevel
        {
            Ordinary = 0,//普通成员
            GroupOwner = 1,//群主
            Admin = 2//管理员

        }

        /// <summary>
        /// 接收消息提醒类型
        /// </summary>
        public enum MsgRemind
        {
            Remind = 1,//接受消息并提醒
            NoRemind = 2//接受消息不提醒

        }

        /// <summary>
        /// SQL类型
        /// </summary>
        public enum SqlType
        {
            Insert,
            Delete,
            Update,
            Query
        }
        /// <summary>
        /// 请求类型
        /// </summary>
        public enum RequestMethod
        {
            PATCH,
            POST,
            GET,
            DELETE
        }

        /// <summary>
        /// 是否已读
        /// </summary>
        public enum IsRead
        {
            Unread = 0,//未读
            Read = 1//已读
        }
        /// <summary>
        /// 消息类型
        /// </summary>
        public enum selectType
        {
            text,
            image,
            voice,
            file
        }

        /// <summary>
        /// 会话类型
        /// </summary>
        public enum SessionType
        {
            SingleChat = 0,//单聊
            GroupChat = 1,//群聊
            MassAssistant = 2,//群发助手
            AttendanceAssistant=3//考勤助手
        }

        /// <summary>
        /// （群发）消息发送状态
        /// </summary>
        public enum SendMsgState
        {
            Sending,//发送中
            Failure,//发送失败
            Success//发送成功
        }
        /// <summary>
        /// 是否阅后即焚
        /// </summary>
        //public enum IsBurnMode
        //{
        //    /// <summary>
        //    /// 阅后即焚模式
        //    /// </summary>
        //    IsBurn = 501,
        //    /// <summary>
        //    /// 正常模式
        //    /// </summary>
        //    NotIsBurn = 502
        //}

        /// <summary>
        /// 聊天消息的阅后即焚标识
        /// </summary>
        public enum BurnFlag
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
        /// 正常消息初始化
        /// </summary>
        public enum IsRun
        {
            /// <summary>
            /// 未初始化
            /// </summary>
            NotIsRun = 0,
            /// <summary>
            /// 未初始化
            /// </summary>
            IsRun = 1
        }
        /// <summary>
        /// 阅后即焚消息初始化
        /// </summary>
        public enum BurnRun
        {
            /// <summary>
            /// 未初始化
            /// </summary>
            NotIsRun = 0,
            /// <summary>
            /// 已经初始化
            /// </summary>
            IsRun = 1
        }
        public enum GroupOrPerson
        {
            /// <summary>
            /// 群组
            /// </summary>
            Group = 0,
            /// <summary>
            /// 个人
            /// </summary>
            Person
        }
        public enum WarnOrSuccess
        {
            /// <summary>
            /// 错误提示
            /// </summary>
            Warn = 0,
            /// <summary>
            /// 成功提示
            /// </summary>
            Success
        }
        /// <summary>
        /// 语音电话
        /// </summary>
        public enum AudioSendOrReceive
        {
            /// <summary>
            /// 发送方
            /// </summary>
            Send=0,
            /// <summary>
            /// 接收方
            /// </summary>
            Receive=1
        }
    }
}
