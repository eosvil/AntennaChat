using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// MQTT收到讨论组信息变更
    /// </summary>
    public class MsModifyGroup : MsSdkMessageGroupBase
    {
        /// <summary>
        /// 讨论组信息变更
        /// </summary>
        public MsModifyGroup_content content { get; set; }
    }

    /// <summary>
    /// 讨论组信息变更
    /// </summary>
    public class MsModifyGroup_content
    {
        /// <summary>
        /// 操作者ID
        /// </summary>
        public string operateId { get; set; } = string.Empty;

        /// <summary>
        /// 群ID
        /// </summary>
        public string groupId { get; set; } = string.Empty;

        /// <summary>
        /// 群名称
        /// </summary>
        public string groupName { get; set; } = string.Empty;

        /// <summary>
        /// 群头像地址
        /// </summary>
        public string groupPicture { get; set; } = string.Empty;
    }
}
