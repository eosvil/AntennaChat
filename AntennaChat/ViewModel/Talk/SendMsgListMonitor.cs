using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AntennaChat.ViewModel.Talk
{
    public class SendMsgListMonitor
    {
        /// <summary>
        /// 消息监控集合
        /// </summary>
        public static List<SendMsgStateMonitor> MessageStateMonitorList = new List<SendMsgStateMonitor>();
        public static Dictionary<string, string> MsgIdAndImgSendingId = new Dictionary<string, string>();
    }
}