using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.AntSdk.AntModels
{
    /// <summary>
    /// 触角SDK登录返回的用户信息
    /// </summary>
    public class AntSdkLoginUserInfo
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 用户密码
        /// </summary>
        public string PWD { get; set; } = string.Empty;

        /// <summary>
        /// Token
        /// </summary>
        public string token { get; set; } = string.Empty;

        /// <summary>
        /// 公司名称
        /// </summary>
        public string companyName { get; set; } = string.Empty;

        /// <summary>
        /// 公司代码
        /// </summary>
        public string companyCode { get; set; } = string.Empty;
    }
}
