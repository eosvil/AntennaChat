using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.AntSdk.AntModels
{
    /// <summary>
    /// 获取用户回复用语设置输出
    /// </summary>
    public class AntSdkGetUserReturnSettingOutput
    {
        /// <summary>
        /// 常用语内容
        /// </summary>
        public string content { get; set; } = string.Empty;

        /// <summary>
        /// 是否已选
        /// </summary>
        public string selected { get; set; } = string.Empty;
    }
}
