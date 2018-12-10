using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 用户在线状态消息
    /// </summary>
    public class MsUserStateChange : MsSdkUserBase
    {
        /// <summary>
        /// 用户自定义消息
        /// </summary>
        public string attr { get; set; } = string.Empty;
    }
}
