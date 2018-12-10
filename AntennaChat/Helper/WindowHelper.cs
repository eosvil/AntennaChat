/*
Author: tanqiyan
Crate date: 2017-06-14
Description：会话窗体帮助类
--------------------------------------------------------------------------------------------------------
Versions：
V1.00 2017-06-14 tanqiyan 描述：记录和删除已打开过得会话
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntennaChat.Helper.IHelper;

namespace AntennaChat.Helper
{
    public class WindowHelper : IWindowHelper
    {
        #region 私有变量
        /// <summary>
        /// 窗体标识
        /// </summary>
        private string _strWindowID;
        /// <summary>
        /// 窗体控制器对应的消息控制器
        /// </summary>
        private IMessageHelper _localMessageHelper;

        /// <summary>
        /// 是否是当前聊天窗体
        /// </summary>
        private bool _isCurrentTalkWin;
        #endregion

        public WindowHelper(string strWindowID)
        {
            _strWindowID = strWindowID;
            _localMessageHelper=new MessageHelper(this);
            WindowMonitor.AddWindowHelper(this);
        }
        /// <summary>
        /// 窗体控制器对应的消息控制器
        /// </summary>
        public IMessageHelper LocalMessageHelper
        {
            get { return _localMessageHelper; }
        }
        /// <summary>
        /// 窗体标识
        /// </summary>
        public string WindowID
        {
            get { return _strWindowID; }
        }

        /// <summary>
        /// 是否是当前聊天窗体
        /// </summary>
        public bool IsCurrentTalkWin { get; set; }

        public void Dispose()
        {
            WindowMonitor.RemoveWindowHelper(_strWindowID);
            _localMessageHelper?.Dispose();
        }
    }
}
