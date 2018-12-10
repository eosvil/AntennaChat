using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.AntSdk.AntModels
{
    /// <summary>
    /// 触角SDK 错误代码及错误信息Model
    /// </summary>
    internal class AntSdkErrorOutput
    {
        /// <summary>
        /// 错误码
        /// </summary>
        public string errorCode { get; set; } = string.Empty;

        /// <summary>
        /// 错误信息
        /// </summary>
        public string errorMsg { get; set; } = string.Empty;
    }

    /// <summary>
    /// 输出基类
    /// </summary>
    internal class AntSdkBaseOutput : AntSdkErrorOutput
    {
        /// <summary>
        /// Http请求数据
        /// </summary>
        public object data { get; set; }
    }
}
