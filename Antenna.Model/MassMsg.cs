using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antenna.Model
{
    /// <summary>
    /// 群发消息(发送/回执)
    /// </summary>
    public class MassMsg
    {
        public int mtp { get; set; }        //"mtp":12,消息类型--群发消息
        public MassMsg_Ctt ctt { get; set; }
    }
    public class MassMsg_Ctt
    {
        public string messageId { get; set; }//消息ID
        public string sendUserId { get; set; }//消息发送者ID
        public string targetId { get; set; }//接收者ID组,用逗号隔开
        public string companyCode { get; set; }//公司代码
        public string content { get; set; }//群发的内容
        public int os { get; set; }//操作系统，PC端传1
        public string sessionId { get; set; }//会话ID
        public string sendTime { get; set; }//消息发送时间（回执才有该字段）
        public string chatIndex { get; set; }//消息游标（回执才有该字段）
    }

    /// <summary>
    /// 群发消息服务器端给的回执
    /// </summary>
    //public class MassMsgReceipt
    //{
    //    public int mtp { get; set; }        //"mtp":12,消息类型--群发消息
    //    public MassMsgReceipt_ctt ctt { get; set; }
    //}
    //public class MassMsgReceipt_ctt: AntSdkMassMsgCtt
    //{
    //}
}
