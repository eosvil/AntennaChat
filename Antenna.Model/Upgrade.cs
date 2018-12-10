using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antenna.Model
{
    public  class UpgradeInput
    {
        public int os { get; set; }
    }

    public class UpgradeOutput:BaseOutput
    {
        /// <summary>
        /// 最新应用的版本号
        /// </summary>
        public string vsersion { get; set; }
        /// <summary>
        /// 版本描述
        /// </summary>
        public string describe { get; set; }
        /// <summary>
        /// app下载地址
        /// </summary>
        public string url { get; set; }
        /// <summary>
        /// 1:软更新，2：硬更新
        /// </summary>
        public string updateType { get; set; }
        /// <summary>
        /// 版本更新标题
        /// </summary>
        public string title { set; get; }
        /// <summary>
        /// 更新包MD5
        /// </summary>
        public string md5Str { set; get; }
    }
}
