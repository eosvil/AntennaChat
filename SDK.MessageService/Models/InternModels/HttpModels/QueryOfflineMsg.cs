using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 查询离线消息
    /// </summary>
    internal class QueryOfflineMsgInput
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 1–pc 2–web 3–android 4–ios
        /// </summary>
        public int os { get; set; } = (int)SdkEnumCollection.OSType.PC;

        /// <summary>
        /// 要查询离线消息的会话列表和chatIndex，chatIndex没有的话（比如用户初次安装app），就传0
        /// </summary>
        public List<QueryOfflineMsgInput_Session> list { get; set; }
    }

    internal class QueryOfflineMsgInput_Session
    {
        /// <summary>
        /// 会话ID
        /// </summary>
        public string sessionId { get; set; } = string.Empty;

        /// <summary>
        /// 用户ID
        /// </summary>
        //public string chatIndex { get; set; } = string.Empty;
    }

    /// <summary>
    /// 消息同步输入对象
    /// </summary>
    public class SynchronusMsgInput
    {
        /// <summary>
        /// 当前登陆用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;
        /// <summary>
        /// 会话ID
        /// </summary>
        public string sessionId { get; set; } = string.Empty;
        /// <summary>
        /// 起始chatIndex，如果第一次查询的话，可以传0；
        /// 如果不是第一次查询，
        /// 那么查询出来的消息的index都是小于这个chatIndex的
        /// </summary>
        public int chatIndex { get; set; }
        /// <summary>
        /// 查询数量，范围在[1,20],超出的话，会报错
        /// </summary>
        public int count { get; set; }
        /// <summary>
        /// 第一次查询，传true；其他传false
        /// </summary>
        public bool first { get; set; }
        /// <summary>
        /// 0表示正常模式的消息，1表示无痕模式的消息
        /// </summary>
        public int flag { get; set; }

        /// <summary>
        /// 聊天类型 1：点对点聊天，2：聊天室 4：群（讨论组）
        /// </summary>
        public int chatType { get; set; }
    }
    /// <summary>
    /// 消息内容对象
    /// </summary>
    public class SynchronusMsgOutput
    {
        /// <summary>
        /// 消息类型
        /// </summary>
        public string messageType { get; set; }

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
        public string messageId { get; set; }
        /// <summary>
        /// app的唯一标识
        /// </summary>
        public string appKey { get; set; }
        /// <summary>
        /// 消息发送者ID
        /// </summary>
        public string sendUserId { get; set; }
        /// <summary>
        /// 消息接收者ID
        /// </summary>
        public string targetId { get; set; }
        /// <summary>
        /// 消息内容
        /// </summary>
        public string content { get; set; }
        /// <summary>
        /// 会话ID
        /// </summary>
        public string sessionId { get; set; }
        /// <summary>
        /// 消息发送时间
        /// </summary>
        public string sendTime { get; set; }
        /// <summary>
        /// 通知游标（会话内消息的index）
        /// </summary>
        public string chatIndex { get; set; }

        /// <summary>
        /// 定义消息字段 可以是jsonString
        /// </summary>
        public string attr { get; set; }
    }
    /// <summary>
    /// 群组无痕模式切换输入对象
    /// </summary>
    public class SwitchBurnAfterReadInput
    {
        /// <summary>
        /// 当前登陆用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;
        /// <summary>
        ///会话ID
        /// </summary>
        public string sessionId { get; set; } = string.Empty;
        /// <summary>
        /// 只能为2591、2592、2593
        /// </summary>
        public string messageType { get; set; } = string.Empty;
        /// <summary>
        /// 终端类型 1-pc 2-web 3-android 4-ios
        /// </summary>
        public int os { get; set; } = (int)SdkEnumCollection.OSType.PC;
    }
}
