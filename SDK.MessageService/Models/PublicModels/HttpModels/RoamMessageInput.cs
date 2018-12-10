using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 消息漫游输入参数[POST]
    /// </summary>
    public class RoamMessageInput
    {
        /// <summary>
        /// 当前登陆用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 会话ID
        /// </summary>
        public string sessionId { get; set; } = string.Empty;

        /// <summary>
        /// 查询的起始的chatIndex
        /// </summary>
        public int startChatIndex { get; set; }

        /// <summary>
        /// 查询的结束的chatIndex
        /// </summary>
        public int endChatIndex { get; set; }
    }
}
