/*
Author: tanqiyan
Crate date: 2017-06-14
Description：消息帮助接口
--------------------------------------------------------------------------------------------------------
Versions：
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antenna.Model;

namespace AntennaChat.Helper.IHelper
{
    public interface IMessageHelper : IDisposable
    {
        /// <summary>
        /// 即时信息接收事件
        /// </summary>
        event Action<int,SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> InstantMessageHasBeenReceived;
        /// <summary>
        /// 即时信息接收事件
        /// </summary>
        event Action<int,SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> InstantSystemMessageHasBeenReceived;

        /// <summary>
        /// 获取对应的窗体控制器
        /// </summary>
        IWindowHelper LocalWindowHelper { get; }

        /// <summary>
        /// 触发信息接收事件
        /// </summary>
        /// <param name="mtp">消息类型</param>
        /// <param name="activeInstantMessage">活动的即时消息</param>
        void TriggerInstantMessageHasBeenReceivedEvent(int mtp,SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase activeInstantMessage);
        /// <summary>
        /// 触发系统信息接收事件
        /// </summary>
        /// <param name="mtp">消息类型</param>
        /// <param name="systemInstantMessage">活动的即时消息</param>
        void TriggerInstantSystemMessageHasBeenReceivedEvent(int mtp,SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase systemInstantMessage);
    }
}
