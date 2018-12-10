/*
Author: tanqiyan
Crate date: 2017-06-14
Description：会话帮助接口
--------------------------------------------------------------------------------------------------------
Versions：
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntennaChat.Helper.IHelper
{
    public interface IWindowHelper : IDisposable
    {
        string WindowID { get; }
        bool IsCurrentTalkWin { get; set; }
        IMessageHelper LocalMessageHelper { get; }
    }
}
