using Antenna.Framework;
using Antenna.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace AntennaChat.ViewModel.Talk
{
    public class DispatcherTimers:DispatcherTimer
    {
        public GlobalVariable.GroupOrPerson GOrP;
        public object obj;
        public List<OfflineMsgReceive> offlineMsgList;
        public DispatcherTimers(GlobalVariable.GroupOrPerson GOrP,object obj, List<OfflineMsgReceive> offlineMsgList)
        { 
            this.GOrP = GOrP;
            this.obj = obj;
            this.offlineMsgList = offlineMsgList;
        }
    }
    public class DispatcherTimerMsgMonitor:DispatcherTimer
    {
        public MessageStateArg arg;
        public DispatcherTimerMsgMonitor(MessageStateArg arg)
        {
            this.arg = arg;
        }
    }
}