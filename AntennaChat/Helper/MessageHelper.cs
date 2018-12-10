/*
Author: tanqiyan
Crate date: 2017-06-14
Description：消息帮助接口
--------------------------------------------------------------------------------------------------------
Versions：
V1.00 2017-06-14 tanqiyan 描述：当前会话消息接收事件
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antenna.Model;
using AntennaChat.Helper.IHelper;

namespace AntennaChat.Helper
{
    public class MessageHelper : IMessageHelper
    {
        #region 私有变量
        /// <summary>
        /// 对应的窗体控制器
        /// </summary>
        private IWindowHelper _localWindowHelper;
        #endregion
        public IWindowHelper LocalWindowHelper
        {
            get { return _localWindowHelper; }
        }
        /// <summary>
        /// 即时信息接收事件
        /// </summary>
        public event Action<int,SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> InstantMessageHasBeenReceived;
        /// <summary>
        /// 即时系统信息接收事件
        /// </summary>
        public event Action<int,SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> InstantSystemMessageHasBeenReceived;

        public MessageHelper(IWindowHelper windowHelper)
        {
            _localWindowHelper = windowHelper;
            MessageMonitor.AddMessageHelper(this);
        }

        public void Dispose()
        {
            MessageMonitor.DisposeMessageHelper(this._localWindowHelper.WindowID);
        }
        /// <summary>
        /// 触发信息接收事件
        /// </summary>
        /// <param name="activeInstantMessage">活动的即时消息</param>
        public void TriggerInstantMessageHasBeenReceivedEvent(int mtp,SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase activeInstantMessage)
        {
            InstantMessageHasBeenReceived?.Invoke(mtp, activeInstantMessage);
        }
        /// <summary>
        /// 触发系统信息接收事件
        /// </summary>
        /// <param name="activeInstantMessage">活动的即时消息</param>
        public void TriggerInstantSystemMessageHasBeenReceivedEvent(int mtp,SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase systemInstantMessage)
        {
            InstantSystemMessageHasBeenReceived?.Invoke(mtp, systemInstantMessage);
        }
    }
}
