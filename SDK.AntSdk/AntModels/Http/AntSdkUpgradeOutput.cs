using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDK.AntSdk.AntModels;

namespace SDK.AntSdk.AntModels
{
    public class AntSdkUpgradeOutput
    {
        /// <summary>
        /// 最新应用的版本号
        /// </summary>
        public string version { get; set; } = string.Empty;

        /// <summary>
        /// 版本描述
        /// </summary>
        public string describe { get; set; } = string.Empty;

        /// <summary>
        /// app下载地址
        /// </summary>
        public string url { get; set; } = string.Empty;

        /// <summary>
        /// 更新包MD5
        /// </summary>
        public string md5Str { set; get; } = string.Empty;

        /// <summary>
        /// 1:软更新，2：硬更新
        /// </summary>
        public string updateType { get; set; } = string.Empty;
    }
}
