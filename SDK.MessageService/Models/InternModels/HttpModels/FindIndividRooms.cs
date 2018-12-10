using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 查询用户所在聊天室
    /// </summary>
    internal class FindIndividRoomsInput : IInputQuery
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 获取Query型参数对应的路径信息
        /// </summary>
        string IInputQuery.GetQuery() => $"?userId={userId}";
    }
}
