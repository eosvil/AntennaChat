using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.AntSdk.AntModels
{
    public class AntSdkUpdateUserInput : AntSdkBaseInput
    {
        /// <summary>
        /// 用户头像地址
        /// </summary>
        public string picture { get; set; } = string.Empty;

        /// <summary>
        /// 性别
        /// </summary>
        public int sex { get; set; }

        /// <summary>
        /// “0”：禁用“1”：启用
        /// </summary>
        public string voiceMode { get; set; } = string.Empty;

        /// <summary>
        /// “0”：禁用“1”：启用
        /// </summary>
        public string vibrateMode { get; set; } = string.Empty;

        /// <summary>
        /// 个性签名信息
        /// </summary>
        public string signature { get; set; } = string.Empty;
    }
}
