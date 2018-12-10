using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.AntSdk.AntModels
{
    /// <summary>
    /// 更新用户系统设置
    /// </summary>
    public class AntSdkUpdateUserSystemSettingInput : AntSdkBaseInput
    {
        /// <summary>
        /// 用户回复语配置
        /// </summary>
        public List<UserContent> config { get; set; }
    }

    /// <summary>
    /// 用户回复语配置
    /// </summary>
    public class UserContent
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
