using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntennaChat.ViewModel.Talk
{
    public class SendMsgListPointMonitor
    {
        /// <summary>
        /// 消息监控集合
        /// </summary>
        public static List<SendMsgStateMonitor> MessageStateMonitorList = new List<SendMsgStateMonitor>();
        public static Dictionary<string, string> MsgIdAndImgSendingId = new Dictionary<string, string>();
    }
}
