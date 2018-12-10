using SDK.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.AntSdk.AntModels
{
    /// <summary>
    /// 查询离线消息输入对象
    /// </summary>
    public class AntSdkQueryOfflineMsgInput
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
        /// sessionId的集合
        /// </summary>
        public List<QueryOfflineMsgInput_Session> list { get; set; }
    }
    /// <summary>
    /// sessionId对象
    /// </summary>
    public class QueryOfflineMsgInput_Session
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
    public class AntSdkSynchronusMsgInput
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
        public int chatIndex { get; set; } = 0;
        /// <summary>
        /// 查询数量，范围在[1,20],超出的话，会报错
        /// </summary>
        public int count { get; set; } = 10;
        /// <summary>
        /// 第一次查询，传true；其他传false
        /// </summary>
        public bool isFirst { get; set; }
        /// <summary>
        /// 0表示正常模式的消息，1表示无痕模式的消息
        /// </summary>
        public int flag { get; set; }
        /// <summary>
        /// 聊天类型 1：点对点聊天，2：聊天室 4：群（讨论组）
        /// </summary>
        public int chatType { get; set; }
    }
}
