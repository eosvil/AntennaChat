using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antenna.Model
{
    /// <summary>
    /// 用户B收到A的阅后即焚消息，发送已读回执给消息服务
    /// </summary>
    public class BurnAfterReadReceipt
    {
        public BurnAfterReadReceiptCtt ctt { get; set; }
    }
    public class BurnAfterReadReceiptCtt
    {
        public string sendUserId { get; set; }
        public string messageId { get; set; }
        public string targetId { get; set; }
        public string companyCode { get; set; }
        public string chatIndex { get; set; }
        public string os { get; set; }
        public string sessionId { get; set; }
        public string content { get; set; }
        public int chatType { get; set; }
        public string sendTime { get; set; }
    }
}
