using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antenna.Model
{

    /// <summary>
    /// 消息回执(聊天消息回执)
    /// </summary>
    /// 创建者：赵雪峰 20161019
    public class ChatMsgReceipt
    {
        public ChatMsgReceipt_Ctt ctt { get; set; }
    }

    public class ChatMsgReceipt_Ctt
    {
        public string sendUserId { get; set; }//用户自身的ID
        public string companyCode { get; set; }//公司代码
        public string chatIndex { get; set; }//消息游标	
        public string sessionId { get; set; }//会话ID
        public int os { get; set; }//操作系统，PC端传1
    }

    /// <summary>
    /// 系统用户通知回执(mtp=6)
    /// </summary>
    /// 创建者：赵雪峰 20161019
    public class SysUserMsgReceipt
    {
        public int mtp { get; set; }
        public SysUserMsgReceipt_Ctt ctt { get; set; }
    }

    public class SysUserMsgReceipt_Ctt
    {
        public string sendUserId { get; set; }//用户自身的ID
        public string companyCode { get; set; }//公司代码
        public string chatIndex { get; set; }//消息游标	
        public string sessionId { get; set; }//会话ID
        //public string targetId { get; set; }//点对点通知（主题为userid）,这里传1，其他传空“”
        public int os { get; set; } = 1;//PC端为1
    }
}
