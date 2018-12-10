using CefSharp.Wpf;
using SDK.AntSdk.AntModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antenna.Model
{
    /// <summary>
    /// 消息监控
    /// </summary>
    public class MessageStateArg
    {
        /// <summary>
        /// 是否为阅后即焚消息
        /// </summary>
        public AntSdkburnMsg.isBurnMsg isBurn { set; get; }
        /// <summary>
        /// 讨论组为True 单人聊天为false
        /// </summary>
        public bool isGroup { set; get; }
        /// <summary>
        /// 会话Id
        /// </summary>
        public string SessionId { set; get; }
        /// <summary>
        /// 消息Id
        /// </summary>
        public string MessageId { set; get; }
        /// <summary>
        /// ChromiumWebBrowser
        /// </summary>
        public ChromiumWebBrowser WebBrowser { set; get; }
        /// <summary>
        /// 发送中状态ID
        /// </summary>
        public string SendIngId { set; get; }
        /// <summary>
        /// 重复按钮Id
        /// </summary>
        public string RepeatId { set; get; }

    }
}
