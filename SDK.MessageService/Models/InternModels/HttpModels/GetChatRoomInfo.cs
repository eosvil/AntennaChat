using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    ///  查询单个聊天室详细信息(输入参数)
    /// </summary>
    internal class GetChatRoomInfoInput
    {
        /// <summary>
        /// 聊天室ID
        /// </summary>
        public string roomId { get; set; } = string.Empty;

        /// <summary>
        /// 1表示带上群成员列表，0表示不带群成员列表，只返回群信息(默认不带群成员列表)
        /// </summary>
        public string ope { get; set; } = string.Empty;
    }
}
