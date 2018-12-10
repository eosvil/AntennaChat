using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDK.Service;

namespace SDK.AntSdk.AntModels
{
    internal class AntSdkUpgradeInput : IAntSdkInputQuery
    {
        /// <summary>
        /// 消息发送者的操作系统 1：PC，2：WEB，3：ANDROID，4：iOS
        /// </summary>
        internal int os { get; set; } = (int) SdkEnumCollection.OSType.PC;

        /// <summary>
        /// 获取Query型参数对应的路径信息
        /// </summary>
        string IAntSdkInputQuery.GetQuery() => $"?os={os}";
    }
}
