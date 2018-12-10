using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDK.Service;

namespace SDK.AntSdk.AntModels
{
    public class AntSdkChangeUserStateInput : AntSdkBaseInput
    {
        /// <summary>
        /// 操作系统 1–pc 2–web 3–android 4–ios
        /// </summary>
        public int os { get; set; } = (int)SdkEnumCollection.OSType.PC;

        /// <summary>
        /// 用户状态
        /// </summary>
        public int onlineState { get; set; }
    }
}
