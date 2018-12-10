using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service
{
    /// <summary>
    /// 委托类型：Mqtt服务消息接收事件类型
    /// </summary>
    /// <param name="msgType">消息传递类型</param>
    /// <param name="contentObject">消息传递内容</param>
    public delegate void SdkPublicationReceivedHandler(SdkMsgType msgType, object contentObject);
}