using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using Antenna.Model;
using AntennaChat.Resource;
using SDK.AntSdk.AntModels;

namespace AntennaChat.ViewModel.Talk
{
    public class ChromiumWebBrowsers:ChromiumWebBrowser
    {
        /// <summary>
        /// 滚动开始chatindex
        /// </summary>
        public string scrollChatIndex;
        /// <summary>
        /// 会话ID
        /// </summary>
        public string sessionid;
        /// <summary>
        /// 当前消息ChatIndex
        /// </summary>
        public string currentChatIndex;
        /// <summary>
        /// 是否阅后即焚 1为阅后即焚
        /// </summary>
        public string flag;

        /// <summary>
        /// 用户id
        /// </summary>
        public string userId;

        public SendMessageDto.SendMessage_ctt s_ctt;
        public RichTextBoxEx richTextBox;
        public List<AntSdkGroupMember> GroupMembers;

    }
}
