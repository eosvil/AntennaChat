using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antenna.Model
{
    public class SendBurnDto
    {
        /// <summary>
        /// 消息发送者ID
        /// </summary>
        public string sendUserId { get; set; }
        /// <summary>
        /// 消息接收者ID
        /// </summary>
        public string targetId { get; set; }
        /// <summary>
        /// 公司代码
        /// </summary>
        public string companyCode { get; set; }
        /// <summary>
        /// 会话ID是为了维持Redis结构
        /// </summary>
        public string sessionId { get; set; }
        /// <summary>
        /// 消息内容
        /// </summary>
        public BurnContent ctt { set; get; }
    }
}
