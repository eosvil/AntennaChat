using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.AntSdk.AntModels
{
    public class AntSdkChangePasswordInput : AntSdkBaseInput
    {
        /// <summary>
        /// 新密码
        /// </summary>
        public string newPassword { get; set; } = string.Empty;

        /// <summary>
        /// 原密码
        /// </summary>
        public string password { get; set; } = string.Empty;
    }
}
