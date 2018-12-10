using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.AntSdk.AntModels.Http
{
    public class AntSdkSendVerifyCodeInput
    {
        /// <summary>
        /// 短信验证码
        /// </summary>
        public string validateCode { set; get; }
        /// <summary>
        /// 手机号码
        /// </summary>
        public string mobile { set; get; }
    }
}
