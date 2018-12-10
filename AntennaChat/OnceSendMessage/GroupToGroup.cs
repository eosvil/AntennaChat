using AntennaChat.ViewModel.Talk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntennaChat.OnceSendMessage
{
    /// <summary>
    /// 群聊天重发消息集合
    /// </summary>
    public class GroupToGroup
    {
        /// <summary>
        /// 发送消息集合
        /// </summary>
        public static Dictionary<string, object> OnceMsgList = new Dictionary<string, object>();
        /// <summary>
        /// 键 sessionId 值cef
        /// </summary>
        public static Dictionary<string, GroupCef> CefList = new Dictionary<string, GroupCef>();
        /// <summary>
        /// 群组cef
        /// </summary>
        public class GroupCef
        {
            public ChromiumWebBrowsers Cef { set; get; }
            public ChromiumWebBrowsers CefBurn { set; get; }
        }
    }
}
