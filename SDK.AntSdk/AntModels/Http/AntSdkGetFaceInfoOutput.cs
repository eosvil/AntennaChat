using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.AntSdk.AntModels
{
    /// <summary>
    /// 获取表情输出
    /// </summary>
    public class AntSdkGetFaceInfoOutput
    {
        /// <summary>
        /// 表情ID
        /// </summary>
        public string expressionId { get; set; } = string.Empty;

        /// <summary>
        /// 表情地址
        /// </summary>
        public string expressionUrl { get; set; } = string.Empty;
    }
}
