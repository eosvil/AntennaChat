using AntennaChat.ViewModel.Talk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntennaChat.OnceSendMessage
{
    /// <summary>
    /// 单人聊天重发消息集合
    /// </summary>
    public class OneToOne
    {
        /// <summary>
        /// 发送消息集合
        /// </summary>
        public static Dictionary<string, object> OnceMsgList = new Dictionary<string, object>();
        /// <summary>
        /// 键 sessionId 值cef
        /// </summary>
        public static Dictionary<string, ChromiumWebBrowsers> CefList = new Dictionary<string, ChromiumWebBrowsers>();
    }
}
