using Antenna.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace AntennaChat.ViewModel.Talk
{
    /// <summary>
    /// 发送消息监控
    /// </summary>
    public class SendMsgStateMonitor:IDisposable
    {
        public DispatcherTimerMsgMonitor _dispatcherTimer;
        public DispatcherTimerMsgMonitor dispatcherTimer
        {
            get { return _dispatcherTimer; }
        }
        private MessageStateArg Arg;
        /// <summary>
        /// 消息
        /// </summary>
        /// <param name="sessionId">会话id</param>
        /// <param name="messageId">消息id</param>
        /// <param name="isGroup">是否群聊</param>
        public SendMsgStateMonitor(MessageStateArg Arg)
        {
            this.Arg = Arg;
            App.Current.Dispatcher.Invoke((Action)(() =>
            {
                _dispatcherTimer = new DispatcherTimerMsgMonitor(Arg);
                _dispatcherTimer.Tick += _dispatcherTimer_Tick;
                _dispatcherTimer.Interval = TimeSpan.FromMilliseconds(30000);
                _dispatcherTimer.Start();
            }));
        }
        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            _dispatcherTimer.Stop();
            //隐藏发送状态
            PublicTalkMothed.HiddenMsgDiv(Arg.WebBrowser, Arg.SendIngId);
            //显示重发按钮
            PublicTalkMothed.VisibleMsgDiv(Arg.WebBrowser, Arg.RepeatId);
        }

        public void Dispose()
        {
            GC.Collect();
        }
    }
}
