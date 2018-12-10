using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antenna.Model
{
    /// <summary>
    /// 离线消息接收
    /// </summary>
    public class OfflineMsgReceive
    {
        public int mtp { get; set; }
        public ChatMsgReceive ctt { get; set; }
    }
}
