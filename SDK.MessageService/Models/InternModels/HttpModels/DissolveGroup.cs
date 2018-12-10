using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 解散讨论组[DELETE]
    /// </summary>
    internal class DissolveGroupInput : IInputSuffPath
    {
        /// <summary>
        /// 当前登陆用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 群ID
        /// </summary>
        public string groupId { get; set; } = string.Empty;

        /// <summary>
        /// 路径型参数必须实现路径合成
        /// </summary>
        string IInputSuffPath.GetSuffPath() => $"/{groupId}/user/{userId}";
    }
}
