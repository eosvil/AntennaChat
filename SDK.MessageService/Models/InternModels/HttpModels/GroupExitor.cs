using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 退出讨论组输入参数
    /// </summary>
    internal class GroupExitorInput
    {
        /// <summary>
        /// 当前登陆用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 讨论组ID
        /// </summary>
        public string groupId { get; set; } = string.Empty;
    }
}
