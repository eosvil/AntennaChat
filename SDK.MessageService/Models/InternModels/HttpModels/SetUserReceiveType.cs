using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    internal class SetUserReceiveTypeInput
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 接收消息类型（1.接收并提醒【默认】；2.接收不提醒（免打扰）；3.屏蔽消息） 
        /// </summary>
        public int receiveType { get; set; }
    }
}
