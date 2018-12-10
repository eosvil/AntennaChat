using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDK.Service.Models;

namespace SDK.AntSdk.AntModels
{
    public class AntSdkBaseInput
    {
        /// <summary>
        /// 当前app的版本号
        /// </summary>
        public string version { get; set; } = AntSdkService.AntSdkConfigInfo.AppVersion;
    }
}
