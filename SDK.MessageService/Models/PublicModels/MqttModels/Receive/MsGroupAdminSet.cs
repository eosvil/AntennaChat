using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 管理员授权
    /// </summary>
    public class MsGroupAdminSet : MsSdkMessageGroupBase
    {
        /// <summary>
        /// 管理员授权内容
        /// </summary>
        public MsGroupAdminSet_content content { get; set; }
    }

    /// <summary>
    /// 管理员授权内容
    /// </summary>
    public class MsGroupAdminSet_content
    {
        /// <summary>
        /// 管理员ID
        /// </summary>
        public string manageId { get; set; } = string.Empty;

        /// <summary>
        /// 0表示普通成员，2表示管理员
        /// </summary>
        public int roleLevel { get; set; }
    }
}
