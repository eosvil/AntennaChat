using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antenna.Model
{
    /// <summary>
    /// 用户登录成功后，发送mqtt消息到服务端，告诉服务端用户上线了
    /// </summary>
    public class SendUserOnline
    {
        public SendUserOnline_Ctt ctt { get; set; }
    }
    public class SendUserOnline_Ctt
    {
        public int os { get; set; }
        public string targetId { get; set; }
        public string companycode { get; set; }
    }
}
