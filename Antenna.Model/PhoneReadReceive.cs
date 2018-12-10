using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antenna.Model
{
    /// <summary>
    /// PC端接收手机端已读回执
    /// </summary>
    public class PhoneReadReceive
    {
        public string sendUserId { get; set; }//用户自身的ID
        public string companyCode { get; set; }//公司代码
        public string chatIndex { get; set; }//消息游标
        public int os { get; set; }//操作系统,android--3 ios--4		
        public int status { get; set; }//消息状态--已读	
        public string sessionId { get; set; }//会话ID
        public string targetId { get; set; }//消息接收者ID
        public string flag { get; set; }//1表示其他端读了某一条单人聊天的阅后即焚消息
    }
}
