using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 更新用户信息输入参数
    /// </summary>
    public class UpdateUserInfoInput
    {
        /// <summary>
        /// 当前用户在app下的唯一ID
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 当前用户名称
        /// </summary>
        public string userName { get; set; } = string.Empty;

        /// <summary>
        /// 用户描述
        /// </summary>
        public string desc { get; set; } = string.Empty;

        /// <summary>
        /// 用户备注
        /// </summary>
        public string remark { get; set; } = string.Empty;

        /// <summary>
        /// 用户其他信息
        /// </summary>
        public string attr1 { get; set; } = string.Empty;

        /// <summary>
        /// 用户其他信息
        /// </summary>
        public string attr2 { get; set; } = string.Empty;

        /// <summary>
        /// 用户其他信息
        /// </summary>
        public string attr3 { get; set; } = string.Empty;
    }
}
