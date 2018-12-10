using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 处理邀请(输入参数）
    /// </summary>
    public class HandleInviteInput
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 用户名称
        /// </summary>
        public string userName { get; set; } = string.Empty;

        /// <summary>
        /// 聊天室ID
        /// </summary>
        public string targetId { get; set; } = string.Empty;

        /// <summary>
        /// 聊天室名称
        /// </summary>
        public string targetName { get; set; } = string.Empty;

        /// <summary>
        /// 邀请者id
        /// </summary>
        public string handleId { get; set; } = string.Empty;

        /// <summary>
        /// 处理方式：1为同意邀请，该人员被加入到聊天室；2为拒绝邀请
        /// </summary>
        public string state { get; set; } = string.Empty;

        /// <summary>
        /// 备注
        /// </summary>
        public string remark { get; set; } = string.Empty;
    }
}
