using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// MQTT 收到群组群主变更通知
    /// </summary>
    public class MsGroupOwnerChanged : MsSdkMessageGroupBase
    {
        /// <summary>
        /// 群主变更通知内容
        /// </summary>
        public MsGroupOwnerChanged_content content { get; set; }
    }

    /// <summary>
    /// 群主变更通知内容
    /// </summary>
    public class MsGroupOwnerChanged_content
    {
        /// <summary>
        /// 操作者ID
        /// </summary>
        public string oldOwnerId { get; set; } = string.Empty;

        /// <summary>
        /// 群ID
        /// </summary>
        public string newOwnerId { get; set; } = string.Empty;

    }
}
