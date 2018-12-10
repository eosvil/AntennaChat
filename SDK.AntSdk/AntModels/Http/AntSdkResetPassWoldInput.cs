using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.AntSdk.AntModels.Http
{
    public class AntSdkResetPassWoldInput
    {
        /// <summary>
        /// 手机密码重置密钥
        /// </summary>
        public string mobileKey { set; get; }
        /// <summary>
        /// 密码
        /// </summary>
        public string password { set; get; }
    }
}
