using SDK.AntSdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antenna.Model
{
    public class SendMessageDto:BaseDto
    {
        public SendMessage_ctt ctt { set; get; }
        public class SendMessage_ctt
        {
            public AntSdkMsgType MsgType { get; set; }
            public string messageId { set; get; }
            public string sendUserId { set; get; }
            public string targetId { set; get; }
            public string companyCode { set; get; }
            public string content { set; get; }
            public string sessionId { set; get; }
            public int os { get; set; } = 1;//pc端操作系统传1
            /// <summary>
            /// 阅后即焚消息标识 1为阅后即焚消息
            /// </summary>
            public int flag { set; get; }
            public string sourceContent { get; set; }
        }

        public class SendMessageAtOut_ctt: BaseDto
        {
            public SendMessageAt_ctt ctt { set; get; }
        }
        public class SendMessageAt_ctt
        {
            public string messageId { set; get; }
            public string sendUserId { set; get; }
            public string targetId { set; get; }
            public string companyCode { set; get; }
            public ContentAtOut content { set; get; }
            public string sessionId { set; get; }
            public int os { get; set; } = 1;//pc端操作系统传1
        }

        public class ContentAtOut
        {
             public string mtp { set; get; }
             public ContentAtIn ctt { set; get; }
            
        }
        public class ContentAtIn
        {
            public string content { set; get; }
            public object ids { set; get; }
        }

    }
}
