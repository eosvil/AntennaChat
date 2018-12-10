using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.AntSdk
{
    /// <summary>
    /// 处理结果（成功或失败）
    /// </summary>
    internal enum AntSdkResult
    {
        /// <summary>
        /// 0-失败
        /// </summary>
        Failure = 0,
        /// <summary>
        /// 1-成功
        /// </summary>
        Success = 1
    }

    /// <summary>
    /// 系统用户通知的消息类型（对应type）
    /// </summary>
    /// type=101：在线登陆 102：离开登陆 103：忙碌登陆 104: 用户信息更新 105: 用户被踢出登陆 100：退出
    /// type=201：讨论组新增 202：讨论组删除 203：讨论组成员更新 204: 讨论组基本信息更新 
    /// type=301：更新组织架构
    internal enum SysUserMsgType
    {
        Logout = 100, //退出
        OnlineLogin = 101, //在线登录
        LeaveLogin = 102, //离开登录
        BusyLogin = 103, //忙碌登录
        UpdateUserInfo = 104, //用户信息更新
        TickOut = 105, //用户被踢出登录
        UpdatePwd = 106, //修改密码
        CreateGroup = 201, //讨论组新增
        DeleteGroup = 202, //讨论组解散
        UpdateGroupMemeber = 203, //讨论组成员更新（成员删除、踢出、新增）
        UpdateGroupInfo = 204, //讨论组基本信息更新
        ContactsUpdate = 301, //组织架构更新
        Notice_Login = 401, //通知——登录推送
        Notice_Add = 402, //通知——添加推送
        Notice_Delete = 403, //通知——删除推送
        Notice_Read = 404, //通知——修改已读状态推送
        GroupBurnAfterReadMode = 501, //管理员将群由普通模式切换到阅后即焚模式
        GroupNoBurnAfterReadMode = 502, //管理员将群由阅后即焚模式切换到普通模式
        DeleteMsgBurnMode = 503 //群主在阅后即焚模式下删除消息
    }

    /// <summary>
    /// 在线状态
    /// </summary>
    internal enum OnLineStatus
    {
        OffLine = 0, //离线
        OnLine = 1, //在线
        Busy = 2, //忙碌
        Leave = 3 //离开
    }

    /// <summary>
    /// 系消息类型(对应mtp字段)
    /// </summary>
    internal enum MsgType
    {
        Text = 1, //文本
        Picture = 2, //图片
        File = 3, //文件
        Voice = 4, //音频
        SysInternalMsg = 5, //系统内部通知
        SysUserMsg = 6, //系统用户通知
        MixPictureText = 7, //图文混合消息
        WithdrawMsg = 8, //用户撤回消息
        At = 9, //@消息
        MultiAudioVideo = 10, //多人音频视频会议消息
        BurnAfterReadReceipt = 11, //点对点阅后即焚的已读回执
        FileMsgReceipt = 12, //文件消息回执
        MassMsg = 13 //群发消息（群发助手）
    }

    /// <summary>  
    /// 鼠标八大位置枚举  
    /// </summary>  
    internal enum MouseLocationEnum
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
    internal class RectangleAreaModel
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }

    /// <summary>  
    /// 鼠标状态  
    /// </summary>  
    internal enum MouseActionEx
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

    /// <summary>
    /// 联系人控件所属容器（即调用位置）
    /// </summary>
    internal enum ContactInfoViewContainer
    {
        ContactListView, //组织结构列表
        GroupEditWindowViewLeft, //编辑讨论组左侧
        GroupEditWindowViewRight, //编辑讨论组右侧
        MultiContactsSelectLeft, //群发消息时多联系人选择左侧
        MultiContactsSelectRight //群发消息时多联系人选择右侧
    }

    /// <summary>
    /// 讨论组成员角色等级
    /// </summary>
    internal enum GroupRoleLevel
    {
        Ordinary = 0, //普通成员
        Admin = 1 //管理员
    }

    /// <summary>
    /// 接收消息提醒类型
    /// </summary>
    internal enum MsgRemind
    {
        Remind = 1, //接受消息并提醒
        NoRemind = 2 //接受消息不提醒
    }

    /// <summary>
    /// SQL类型
    /// </summary>
    internal enum SqlType
    {
        Insert,
        Delete,
        Update,
        Query
    }

    /// <summary>
    /// 请求类型
    /// </summary>
    internal enum RequestMethod
    {
        PATCH,
        POST,
        PUT,
        GET,
        DELETE
    }

    /// <summary>
    /// 是否已读
    /// </summary>
    internal enum IsRead
    {
        Unread = 0, //未读
        Read = 1 //已读
    }

    /// <summary>
    /// 消息类型
    /// </summary>
    internal enum selectType
    {
        text,
        image,
        voice,
        file
    }

    /// <summary>
    /// 会话类型
    /// </summary>
    internal enum SessionType
    {
        /// <summary>
        /// 单聊
        /// </summary>
        SingleChat = 0,
        /// <summary>
        /// 群聊
        /// </summary>
        GroupChat = 1,
        /// <summary>
        /// 群发助手
        /// </summary>
        MassAssistant = 2
    }

    /// <summary>
    /// （群发）消息发送状态
    /// </summary>
    internal enum SendMsgState
    {
        /// <summary>
        /// 发送中
        /// </summary>
        Sending,
        /// <summary>
        /// 发送失败
        /// </summary>
        Failure,
        ///发送成功 
        Success
    }

    /// <summary>
    /// 聊天消息的阅后即焚标识
    /// </summary>
    internal enum BurnFlag
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
    internal enum IsRun
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
    internal enum BurnRun
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
}
