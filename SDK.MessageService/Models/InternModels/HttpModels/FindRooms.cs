using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 查询用户所在聊天室，返回聊天室列表(输入参数)
    /// </summary>
    internal class FindRoomsInput
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;
    }
}
