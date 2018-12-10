using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 收到传透消息
    /// </summary>
    public class UnvarnishedMsg : SdkMsBase
    {
        /// <summary>
        /// 消息接收者
        /// </summary>
        public string targetId { get; set; } = string.Empty;

        /// <summary>
        /// 消息内容
        /// </summary>
        public string content { get; set; } = string.Empty;
    }
}
