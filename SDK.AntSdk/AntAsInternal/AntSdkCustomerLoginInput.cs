using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.AntSdk.AntModels
{
    /// <summary>
    /// 触角SDK登录客户系统输入
    /// </summary>
    public class AntSdkCustomerLoginInput
    {
        /// <summary>
        /// 登录名
        /// </summary>
        public string loginName { get; set; } = string.Empty;

        /// <summary>
        /// 密码
        /// </summary>
        public string password { get; set; } = string.Empty;
    }
}
