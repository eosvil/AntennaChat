using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    internal class SetRoomReceiveTypeInput
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 聊天室ID
        /// </summary>
        public string roomId { get; set; } = string.Empty;

        /// <summary>
        /// 接收消息类型（1.接收并提醒【默认】；2.接收不提醒；3.屏蔽群消息） 
        /// </summary>
        public int receiveType { get; set; }
    }
}
