using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDK.Service;

namespace SDK.AntSdk.AntModels
{
    internal class AntSdkLoginInput
    {
        /// <summary>
        /// 操作系统 1：PC，2：WEB，3：ANDROID，4：iOS
        /// </summary>
        public int os { get; set; } = (int)SdkEnumCollection.OSType.PC;

        /// <summary>
        /// 操作系统描述
        /// </summary>
        public string osStr { get; set; } = string.Empty;

        /// <summary>
        /// 版本信息
        /// </summary>
        public string version { get; set; } = SDK.AntSdk.AntSdkService.AntSdkConfigInfo.AppVersion;

        /// <summary>
        /// 登录名
        /// </summary>
        public string loginName { get; set; } = string.Empty;

        /// <summary>
        /// 密码
        /// </summary>
        public string password { get; set; } = string.Empty;

        /// <summary>
        /// 制造商
        /// </summary>
        public string manufacture { get; set; } = string.Empty;

        /// <summary>
        /// 验证码
        /// </summary>
        public string validateCode { get; set; } = string.Empty;
        /// <summary>
        /// mobile(android、ios)、 pc(pc端)、 web(网页端)
        /// </summary>
        public string source { get; set; } = SdkEnumCollection.OSType.PC.ToString();
    }
}
