using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antenna.Model
{
    /// <summary>
    /// 会话信息对应的实体
    /// </summary>
    public  class SessionInfoModel
    {
        public  string SessionId { get; set; }//会话ID
        public  string photo { get; set; }//头像
        public string name { get; set; }//名称
        public string lastMessage { get; set; }//最后一条消息
        public int unreadCount { get; set; }//未读消息数
        public string lastTime { get; set; }//最后一条消息时间
        public string lastChatIndex { get; set; }
        public int? topIndex { get; set; }
        //public int MyselfMsgCount { get; set; }
    }
}
